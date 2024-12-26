﻿#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ModTek.Features.Logging;

// RingBuffer with pre-allocated structs=32ns (pre-allocation also reduces DTO setup times from 150-300ns to 50ns)
// Other variants:
// RingBuffer with nullable reference types=38ns
// ConcurrentQueue+custom size tracking+addingComplete=40ns
// BlockingCollection=170ns
// Use-Cases:
// 1. no items -> consumer: wait and don't use cpu
// 2. almost no items -> producer + consumer: low latency
// 3. full -> producer: low latency; consumer: high throughput (solution is not optimized for this case)
internal class LightWeightBlockingQueue
{
    internal void Shutdown() => _shutdown = true;
    private volatile bool _shutdown; // some way to break the waiting

    // 65k leads to about ~20MB, pre-allocation reports less due to absent strings
    // see https://en.wikipedia.org/wiki/Modulo#Performance_issues
    private const int MaxRingBufferSize = 1 << 16; // power of 2 required by FastModuloMaskForBitwiseAnd
    private const int FastModuloMaskForBitwiseAnd = MaxRingBufferSize - 1;
    private const int MaxQueueSize = MaxRingBufferSize - 1; // Start and End need to be distinguishable
    // ring buffer is used by Disruptor(.NET), seems to work well for them
    // typed based exchanges are 56ns (fixed as of .NET 7) hence why we use object based ones
    private readonly MTLoggerMessageDto[] _ringBuffer = new MTLoggerMessageDto[MaxRingBufferSize];
    // end - start = size
    private volatile int _nextWriteIndex;
    private volatile int _nextReadIndex;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Next(int index)
    {
        return (index + 1) & FastModuloMaskForBitwiseAnd;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Size(int startIndex, int endIndex)
    {
        return (endIndex - startIndex) & FastModuloMaskForBitwiseAnd;
    }

    internal ref MTLoggerMessageDto AcquireCommittedOrWait()
    {
        var spinWait = new SpinWait();
        while (true)
        {
            var index = _nextReadIndex;
            if (Size(index, _nextWriteIndex) > 0)
            {
                ref var item = ref _ringBuffer[index];
                // makes sure no overtake on the ring happens
                if (item.CommittedToQueue)
                {
                    if (Interlocked.CompareExchange(ref _nextReadIndex, Next(index), index) == index)
                    {
                        return ref item;
                    }
                }
                else
                {
                    // this branch happens 292 times for 157187 dispatches (0.19%)
                    // for now not worth it to optimize
                }

                spinWait.Reset(); // fast retry if something was found earlier
            }
            else
            {
                if (_shutdown)
                {
                    // this can still drop logs, very unlikely but possible
                    Thread.Sleep(1);
                    if (Size(_nextReadIndex, _nextWriteIndex) == 0)
                    {
                        throw new ShutdownException();
                    }
                }
            }
            spinWait.SpinOnce(); // reader should yield and sleep if nothing comes in after some time
        }
    }

    internal ref MTLoggerMessageDto AcquireUncommitedOrWait()
    {
        while (true)
        {
            var index = _nextWriteIndex;
            if (Size(_nextReadIndex, index) < MaxQueueSize)
            {
                ref var item = ref _ringBuffer[index];
                if (!item.CommittedToQueue)
                {
                    if (Interlocked.CompareExchange(ref _nextWriteIndex, Next(index), index) == index)
                    {
                        return ref item;
                    }
                }
            }
            
            Thread.SpinWait(4); // main thread should always try to dispatch asap, never wait that much!
        }
    }

    internal class ShutdownException : Exception;
}
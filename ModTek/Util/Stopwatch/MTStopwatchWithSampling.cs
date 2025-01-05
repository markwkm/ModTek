﻿using System.Runtime.CompilerServices;

namespace ModTek.Util.Stopwatch;

internal sealed class MTStopwatchWithSampling : MTStopwatch
{
    internal MTStopwatchWithSampling(uint sampling)
    {
        Sampling = sampling;
        _sampleIfRandomSmallerOrEqualsTo = ulong.MaxValue / sampling;
    }
    internal readonly uint Sampling;
    private readonly ulong _sampleIfRandomSmallerOrEqualsTo;
    private readonly FastRandom _random = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void EndMeasurement(long start)
    {
        // Stopwatch.GetTimestamp takes about 16-30ns, probably due to "extern" overhead
        // fast random is much faster, runs unrolled and therefore in parallel on the CPU
        if (_random.NextUInt64() <= _sampleIfRandomSmallerOrEqualsTo)
        {
            AddMeasurement(GetTimestamp() - start);
        }
    }
}
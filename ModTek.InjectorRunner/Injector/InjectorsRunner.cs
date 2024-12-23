﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ModTek.Common.Globals;
using ModTek.Common.Logging;
using ModTek.Common.Utils;

namespace ModTek.InjectorRunner.Injector;

internal class InjectorsRunner : IDisposable
{
    internal static void Run()
    {
        using var injectorsRunner = new InjectorsRunner();
        if (injectorsRunner.IsUpToDate)
        {
            return;
        }

        injectorsRunner.RunInjectors();
        injectorsRunner.SaveToDisk();
    }

    private readonly AssemblyCache _assemblyCache;
    private readonly InjectionCacheManifest _injectionCacheManifest;
    private InjectorsRunner()
    {
        _assemblyCache = new AssemblyCache();
        _injectionCacheManifest = new InjectionCacheManifest();
    }

    internal bool IsUpToDate => _injectionCacheManifest.IsUpToDate;

    internal void RunInjectors()
    {
        Logger.Main.Log($"Searching injector assemblies in `{FileUtils.GetRelativePath(Paths.InjectorsDirectory)}`:");
        foreach (var injectorPath in Directory.GetFiles(Paths.InjectorsDirectory, "*.dll").OrderBy(p => p))
        {
            SearchInjectorEntrypointAndInvoke(injectorPath);
        }
    }

    internal void SaveToDisk()
    {
        _assemblyCache.SaveAssembliesToDisk();
        _injectionCacheManifest.RefreshAndSave();
    }

    private void SearchInjectorEntrypointAndInvoke(string injectorPath)
    {
        Logger.Main.Log($"\t{Path.GetFileName(injectorPath)}");
        var injector = Assembly.LoadFile(injectorPath);
        foreach (var injectMethod in injector
                     .GetTypes()
                     .Where(t => t.Name == "Injector")
                     .Select(t => t.GetMethod("Inject", BindingFlags.Public | BindingFlags.Static))
                     .Where(m => m != null))
        {
            var name = injector.GetName().Name;

            InvokeInjector(name, injectMethod);
            break;
        }
    }

    private void InvokeInjector(string name, MethodInfo injectMethod)
    {
        using var errorLogger = new ConsoleLoggerAdapter(Logger.Main) { Prefix = $"{name} Error: " };
        using var infoLogger = new ConsoleLoggerAdapter(Logger.Main) { Prefix = $"{name}: " };

        var originalConsoleOut = Console.Out;
        var originalConsoleError = Console.Error;
        Console.SetOut(infoLogger);
        Console.SetError(errorLogger);
        try
        {
            injectMethod.Invoke(null, new object[] { _assemblyCache });
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
            Console.SetError(originalConsoleError);
        }
    }

    public void Dispose()
    {
        _assemblyCache.Dispose();
    }
}
using System;
using BenchmarkDotNet.Running;

namespace P1SmartMeter.Benchmark.Tests;

public static class Program
{
    public static void Main()
    {
#pragma warning disable CA1852
#pragma warning disable CS8625
#pragma warning disable S1481
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly, null, new string[] { "--filter", "*Multiple*" });
    }
}

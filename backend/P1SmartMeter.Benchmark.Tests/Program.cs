using System;
using BenchmarkDotNet.Running;

//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(new string[] { "-d" });

var summary = BenchmarkRunner.Run(typeof(Program).Assembly, null, new string[] { "--filter", "*Multiple*"});


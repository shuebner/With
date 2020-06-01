using BenchmarkDotNet.Running;
using System;

namespace SvSoft.With
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<WithBenchmark>();
        }
    }
}

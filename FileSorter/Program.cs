using System;
using System.Diagnostics;
using System.IO;

namespace FileSorter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please provide path to test file as an argument!");
                return;
            }

            var overallTimer = new Stopwatch();
            overallTimer.Start();
            var sorter = new ExternalMergeSorter();
            var filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Cannot find provided file!");
            }
            sorter.Sort(filePath, "Sorted.txt");
            overallTimer.Stop();
            System.Console.WriteLine($"File sorted in {overallTimer.Elapsed.TotalSeconds} seconds");
        }
    }
}

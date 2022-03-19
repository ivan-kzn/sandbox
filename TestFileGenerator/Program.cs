using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TestFileGenerator
{
    class Program
    {
        private static readonly string[] PredefinedWords =
        {
            "Apple",
            "Cherry is the best",
            "Banana is yellow",
            "Something something something"
        };

        static void Main(string[] args)
        {
            Console.Write("Please enter expected file size in Mb: ");
            var input = Console.ReadLine();
            var timer = new Stopwatch();
            timer.Start();
            ulong currentSize = 0;
            if (ulong.TryParse(input, out ulong expectedFileSize))
            {
                expectedFileSize *= 1024 * 1024; //get size in bytes
                using (var sw = new StreamWriter("TestFile.txt"))
                {
                    while (currentSize < expectedFileSize)
                    {
                        string line = GenerateString();
                        sw.WriteLine(line);
                        currentSize += (ulong)line.Length + 2;
                    }
                }
            }
            else
            {
                Console.WriteLine("File size wasn't in correct format!!!");
            }
            timer.Stop();
            Console.WriteLine($"File generated in {timer.Elapsed.TotalSeconds} seconds");
        }

        private static string GenerateString()
        {
            var sb = new StringBuilder();
            var rand = new Random();
            var number = rand.Next(1, 100000);
            var numberOfWords = rand.Next(1, 100);
            sb.Append($"{number}. {PredefinedWords[rand.Next(PredefinedWords.Length)]} ");
            for (var i = 0; i < numberOfWords; i++)
            {
                sb.Append($"{Vokabulary.words[rand.Next(Vokabulary.words.Length)]} ");
                numberOfWords--;
            }

            return sb.ToString().Trim();
        }
    }
}

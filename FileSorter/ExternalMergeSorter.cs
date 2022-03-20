﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace FileSorter
{
    public class ExternalMergeSorter
    {
        private readonly ExternalMergeSorterOptions _options;

        public ExternalMergeSorter()
        {
            _options = new ExternalMergeSorterOptions();
            if (!Directory.Exists(_options.TempFileLocation))
            {
                Directory.CreateDirectory(_options.TempFileLocation);
            }
        }

        public void Sort(string filePath, string outputFileName)
        {
            try
            {
                var timer = new Stopwatch();

                Console.WriteLine("Processing splitting and sorting files...");
                timer.Start();
                var sortedFiles = Split(filePath);
                timer.Stop();
                Console.WriteLine($"Splitting completed in {timer.Elapsed.TotalSeconds} seconds");

                Console.WriteLine("Processing merging files...");
                timer.Restart();
                MergeTheChunks(sortedFiles, outputFileName);
                timer.Stop();
                Console.WriteLine($"Merging completed in {timer.Elapsed.TotalSeconds} seconds");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception was thrown: {e}");
            }
        }

        private List<string> Split(string file)
        {
            var fileNames = new List<string>();

            using (var sourceStream = File.OpenRead(file))
            {
                var streamLength = sourceStream.Length;
                var fileSize = streamLength / _options.Split.SplitFilesCount;
                var currentFile = 0L;
                var counter = 0;

                using (var sr = new StreamReader(sourceStream))
                {
                    while (!sr.EndOfStream)
                    {
                        var unsortedRecords = new List<Record>();
                        while (currentFile < fileSize)
                        {
                            var line = sr.ReadLine();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                unsortedRecords.Add(line.ToRecord());
                                currentFile += line.Length + 2;
                            }
                            if (sr.EndOfStream)
                            {
                                break;
                            }
                        }
                        var filename = $"sorted{++counter}.dat";
                        var sortedFile = Path.Combine(_options.TempFileLocation, filename);
                        SortFile(unsortedRecords.ToArray(), sortedFile);
                        currentFile = 0;
                        fileNames.Add(sortedFile);
                    }
                }

                return fileNames;
            }
        }

        private void SortFile(Record[] unsortedRecords, string sortedFile)
        {
            var sortHelper = new SortHelper<Record>();
            sortHelper.MergeSort(unsortedRecords, 0, unsortedRecords.Length - 1);

            using (var streamWriter = new StreamWriter(File.OpenWrite(sortedFile)))
            {
                foreach (var row in unsortedRecords)
                {
                    streamWriter.WriteLine(row.ToString());
                }
            }

            Array.Clear(unsortedRecords);
        }

        private void MergeTheChunks(IReadOnlyList<string> sortedFiles, string target)
        {
            var chunks = sortedFiles.Count;
            var maxUsage = _options.Merge.MaxMemoryUsage;
            var bufferLength = maxUsage / chunks;
            var readers = new StreamReader[chunks];
            try
            {
                for (var i = 0; i < chunks; i++)
                {
                    readers[i] = new StreamReader(sortedFiles[i]);
                }

                var queues = new Queue<Record>[chunks];
                for (var i = 0; i < chunks; i++)
                {
                    queues[i] = new Queue<Record>(bufferLength);
                }

                for (var i = 0; i < chunks; i++)
                {
                    LoadQueue(queues[i], readers[i], bufferLength);
                }

                using (var sw = new StreamWriter(target))
                {
                    while (true)
                    {
                        var lowestIndex = -1;
                        var lowestValue = new Record();
                        for (var j = 0; j < chunks; j++)
                        {
                            if (queues[j] != null)
                            {
                                if (lowestIndex < 0 || queues[j].Peek().CompareTo(lowestValue) < 0)
                                {
                                    lowestIndex = j;
                                    lowestValue = queues[j].Peek();
                                }
                            }
                        }

                        if (lowestIndex == -1)
                        {
                            break;
                        }

                        sw.WriteLine(lowestValue.ToString());

                        queues[lowestIndex].Dequeue();
                        if (queues[lowestIndex].Count == 0)
                        {
                            LoadQueue(queues[lowestIndex], readers[lowestIndex], bufferLength);
                            if (queues[lowestIndex].Count == 0)
                            {
                                queues[lowestIndex] = null;
                            }
                        }
                    }
                }
            }
            finally
            {
                for (var i = 0; i < chunks; i++)
                {
                    readers[i].Close();
                    File.Delete(sortedFiles[i]);
                }
            }
        }

        private static void LoadQueue(Queue<Record> queue, StreamReader file, int records)
        {
            for (var i = 0; i < records; i++)
            {
                if (file.Peek() < 0)
                    break;
                var line = file.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    queue.Enqueue(line.ToRecord());
                }
            }
        }
    }
}
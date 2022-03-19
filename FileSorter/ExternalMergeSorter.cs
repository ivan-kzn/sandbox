using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileSorter
{
    public class ExternalMergeSorter
    {
        private readonly ExternalMergeSorterOptions _options;
        private long _testFileSize;
        private long _recordsCount;

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

                timer.Start();
                var sortedFiles = Split(filePath);
                timer.Stop();
                Console.WriteLine($"Splitting completed in {timer.Elapsed.TotalSeconds} seconds");

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
            var extraBuffer = new List<byte>();
            var fileNames = new List<string>();

            using (var sourceStream = File.OpenRead(file))
            {
                var streamLength = sourceStream.Length;
                _testFileSize = streamLength;
                var fileSize = streamLength / _options.Split.SplitFilesCount;
                var totalLines = 0;
                var buffer = new byte[fileSize];
                var currentFile = 0L;
                while (sourceStream.Position < streamLength)
                {
                    var runBytesRead = 0;
                    while (runBytesRead < fileSize)
                    {
                        var value = sourceStream.ReadByte();
                        if (value == -1)
                        {
                            break;
                        }

                        var valueAsByte = (byte) value;
                        buffer[runBytesRead] = valueAsByte;
                        runBytesRead++;
                        if (valueAsByte == _options.Split.NewLineSeparator)
                        {
                            totalLines++;
                        }
                    }

                    var extraByte = buffer[fileSize - 1];

                    while (extraByte != _options.Split.NewLineSeparator)
                    {
                        var value = sourceStream.ReadByte();
                        if (value == -1)
                        {
                            break;
                        }

                        extraByte = (byte) value;
                        extraBuffer.Add(extraByte);
                    }

                    var filename = $"chunk{++currentFile}.dat";
                    var unsortedFile = Path.Combine(_options.TempFileLocation, filename);
                    using (var fs = File.Create(unsortedFile))
                    {
                        fs.Write(buffer, 0, runBytesRead);
                        if (extraBuffer.Count > 0)
                        {
                            totalLines++;
                            fs.Write(extraBuffer.ToArray(), 0, extraBuffer.Count);
                        }
                    }

                    var sortedFile = SortFile(unsortedFile, totalLines);
                    fileNames.Add(sortedFile);

                    _recordsCount += totalLines;
                    totalLines = 0;
                    extraBuffer.Clear();
                }

                return fileNames;
            }
        }

        private string SortFile(string unsortedFile, int totalLines)
        {
            var unsortedRecords = new Record[totalLines];
            var counter = 0;
            using (var streamReader = new StreamReader(File.OpenRead(unsortedFile)))
            {

                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        unsortedRecords[counter++] = line.ToRecord();
                    }
                }
            }

            var sortHelper = new Sorter<Record>();
            sortHelper.MergeSort(unsortedRecords, 0, unsortedRecords.Length - 1);
            var sortedFile = unsortedFile.Replace("chunk", "sorted");

            using (var streamWriter = new StreamWriter(File.OpenWrite(sortedFile)))
            {
                foreach (var row in unsortedRecords)
                {
                    streamWriter.WriteLine(row.ToString());
                }
            }

            Array.Clear(unsortedRecords);
            File.Delete(unsortedFile);
            return sortedFile;
        }

        private void MergeTheChunks(IReadOnlyList<string> sortedFiles, string target)
        {
            var chunks = sortedFiles.Count;
            var maxUsage = _testFileSize;
            var recordLength = _options.Merge.RecordLength;
            var bufferSize = maxUsage / chunks;
            var bufferLength = (int)(bufferSize / recordLength / 10);
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
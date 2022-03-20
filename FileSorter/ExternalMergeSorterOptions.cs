
namespace FileSorter
{
    public class ExternalMergeSorterOptions
    {
        public ExternalMergeSorterOptions()
        {
            Split = new ExternalMergeSortSplitOptions();
            Merge = new ExternalMergeSortMergeOptions();
        }

        public string TempFileLocation { get; init; } = @"temp\";
        public ExternalMergeSortSplitOptions Split { get; init; }
        public ExternalMergeSortMergeOptions Merge { get; init; }
    }

    public class ExternalMergeSortSplitOptions
    {
        /// <summary>
        /// Size of unsorted file (chunk) (in bytes)
        /// </summary>
        public int SplitFilesCount { get; init; } = 10;
    }

    public class ExternalMergeSortMergeOptions
    {
        public int FilesPerRun { get; init; } = 5;
        public int MaxMemoryUsage { get; init; } = 1 * 1024 * 1024;
    }
}

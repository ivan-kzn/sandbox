
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
        public int SplitFilesCount { get; init; } = 30;
        public char NewLineSeparator { get; init; } = '\n';
    }

    public class ExternalMergeSortMergeOptions
    {
        /// <summary>
        /// Estimate record length
        /// </summary>
        public int RecordLength { get; init; } = 200;
    }
}

using System;
using System.Threading.Tasks;

namespace FileSorter
{
    class SortHelper<T> where T : IComparable<T>
    {

        private const int SequentialThreshold = 2048;
        public void MergeSort(T[] array, int left, int right)
        {
            var copy = (T[]) array.Clone();
            ParallelMergeSort(array, copy, left, right, GetMaxDepth());
        }

        private void SequentialMergeSort(T[] array, T[] temp, int left, int right)
        {
            if (left >= right)
                return;
            var mid = (left + right) / 2;
            
            SequentialMergeSort(temp, array, left, mid);
            SequentialMergeSort(temp, array, mid + 1, right);

            SequentialMerge(array, temp, left, mid, mid + 1, right, left);
        }

        private void SequentialMerge(T[] array, T[] temp, int leftFirst, int rightFirst, int leftSecond, int rightSecond, int leftTo)
        {
            var rightTo = leftTo + rightFirst - leftFirst + rightSecond - leftSecond + 1;
            while (leftTo <= rightTo)
            {
                if (leftFirst > rightFirst)
                    array[leftTo] = temp[leftSecond++];
                else if (leftSecond > rightSecond)
                    array[leftTo] = temp[leftFirst++];
                else
                    array[leftTo] = temp[leftFirst].CompareTo(temp[leftSecond]) < 0
                        ? temp[leftFirst++]
                        : temp[leftSecond++];
                leftTo++;
            }
        }

        private void ParallelMergeSort(T[] array, T[] temp, int left, int right, int depth)
        {
            if (right - left + 1 <= SequentialThreshold || depth <= 0)
            {
                SequentialMergeSort(array, temp, left, right);
                return;
            }

            var mid = (left + right) / 2;
            depth--;
            Parallel.Invoke(
                () => ParallelMergeSort(temp, array, left, mid, depth),
                () => ParallelMergeSort(temp, array, mid + 1, right, depth)
            );
            
            ParallelMerge(array, temp, left, mid, mid + 1, right, left, depth);
        }

        private void ParallelMerge(T[] array, T[] temp, int leftFirst, int rightFirst, int leftSecond, int rightSecond, int leftTo, int depth)
        {
            var lengthFirst = rightFirst - leftFirst + 1;
            var lengthSecond = rightSecond - leftSecond + 1;

            if (lengthFirst + lengthSecond <= SequentialThreshold || depth <= 0)
            {
                SequentialMerge(array, temp, leftFirst, rightFirst, leftSecond, rightSecond, leftTo);
                return;
            }

            if (lengthFirst < lengthSecond)
            {
                ParallelMerge(array, temp, leftSecond, rightSecond, leftFirst, rightFirst, leftTo, depth);
                return;
            }

            var midFirst = (leftFirst + rightFirst) / 2;
            var midSecond = BinarySearch(temp, leftSecond, rightSecond, temp[midFirst]);
            var midTo = leftTo + midFirst - leftFirst + midSecond - leftSecond;
            array[midTo] = temp[midFirst];
            depth--;
            Parallel.Invoke(
                () => ParallelMerge(array, temp, leftFirst, midFirst - 1, leftSecond, midSecond - 1, leftTo, depth),
                () => ParallelMerge(array, temp, midFirst + 1, rightFirst, midSecond, rightSecond, midTo + 1, depth)
            );
        }

        private int BinarySearch(T[] from, int left, int right, T lessThanOrEqualTo)
        {
            right = Math.Max(left, right + 1);
            while (left < right)
            {
                var mid = (left + right) / 2;
                if (from[mid].CompareTo(lessThanOrEqualTo) < 0)
                    left = mid + 1;
                else
                    right = mid;
            }

            return left;
        }

        private static int GetMaxDepth()
        {
            return (int) Math.Log(Environment.ProcessorCount, 2) + 4;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSorter
{
    class Sorter<T> where T : IComparable<T>
    {

        private const int SequentialThreshold = 2048;
        public void MergeSort(T[] array, int low, int high)
        {
            var copy = (T[]) array.Clone();
            ParallelMergeSort(array, copy, low, high, GetMaxDepth());
        }

        private void SequentialMergeSort(T[] to, T[] temp, int low, int high)
        {
            if (low >= high)
                return;
            var mid = (low + high) / 2;
            
            SequentialMergeSort(temp, to, low, mid);
            SequentialMergeSort(temp, to, mid + 1, high);

            SequentialMerge(to, temp, low, mid, mid + 1, high, low);
        }

        private void SequentialMerge(T[] to, T[] temp, int lowX, int highX, int lowY, int highY, int lowTo)
        {
            var highTo = lowTo + highX - lowX + highY - lowY + 1;
            for (; lowTo <= highTo; lowTo++)
            {
                if (lowX > highX)
                    to[lowTo] = temp[lowY++];
                else if (lowY > highY)
                    to[lowTo] = temp[lowX++];
                else
                    to[lowTo] = temp[lowX].CompareTo(temp[lowY]) < 0
                        ? temp[lowX++]
                        : temp[lowY++];
            }
        }

        private void ParallelMergeSort(T[] to, T[] temp, int low, int high, int depth)
        {
            if (high - low + 1 <= SequentialThreshold || depth <= 0)
            {
                SequentialMergeSort(to, temp, low, high);
                return;
            }

            var mid = (low + high) / 2;
            depth--;
            Parallel.Invoke(
                () => ParallelMergeSort(temp, to, low, mid, depth),
                () => ParallelMergeSort(temp, to, mid + 1, high, depth)
            );
            
            ParallelMerge(to, temp, low, mid, mid + 1, high, low, depth);
        }

        private void ParallelMerge(T[] to, T[] temp, int lowX, int highX, int lowY, int highY, int lowTo, int depth)
        {
            var lengthX = highX - lowX + 1;
            var lengthY = highY - lowY + 1;

            if (lengthX + lengthY <= SequentialThreshold || depth <= 0)
            {
                SequentialMerge(to, temp, lowX, highX, lowY, highY, lowTo);
                return;
            }

            if (lengthX < lengthY)
            {
                ParallelMerge(to, temp, lowY, highY, lowX, highX, lowTo, depth);
                return;
            }

            var midX = (lowX + highX) / 2;
            var midY = BinarySearch(temp, lowY, highY, temp[midX]);
            var midTo = lowTo + midX - lowX + midY - lowY;
            to[midTo] = temp[midX];
            depth--;
            Parallel.Invoke(
                () => ParallelMerge(to, temp, lowX, midX - 1, lowY, midY - 1, lowTo, depth),
                () => ParallelMerge(to, temp, midX + 1, highX, midY, highY, midTo + 1, depth)
            );
        }

        private int BinarySearch(T[] from, int low, int high, T lessThanOrEqualTo)
        {
            high = Math.Max(low, high + 1);
            while (low < high)
            {
                var mid = (low + high) / 2;
                if (from[mid].CompareTo(lessThanOrEqualTo) < 0)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }

        private static int GetMaxDepth()
        {
            return (int) Math.Log(Environment.ProcessorCount, 2) + 4;
        }
    }
}

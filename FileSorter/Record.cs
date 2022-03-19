using System;
using System.Collections;
using System.Collections.Generic;

namespace FileSorter
{
    internal readonly struct Record : IComparable<Record>
    {
        public string Text { get; init; }
        public int Number { get; init; }

        public int CompareTo(Record record)
        {
            var compareResult = string.Compare(Text, record.Text, StringComparison.Ordinal);
            if (compareResult == 0)
            {
                compareResult = Number.CompareTo(record.Number);
            }
            return compareResult;
        }

        public override string ToString()
        {
            return $"{Number}. {Text}";
        }
    }

    internal static class StringExtension
    {
        public static Record ToRecord(this string str)
        {
            var separator = str.IndexOf('.');
            if (separator < 0 || str.Length <= separator + 2)
            {
                throw new ArgumentException("invalid record format!");
            }
            if (separator > 0)
            {
                if (int.TryParse(str.Substring(0, separator), out var number))
                {
                    return new Record { Text = str.Substring(separator + 2), Number = number };
                }

                throw new ArgumentException("invalid record format!");
            }
            return new Record();
        }
    }
}

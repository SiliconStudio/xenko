using System;

namespace SiliconStudio.Assets.Diff
{
    public struct DataMatch : IEquatable<DataMatch>
    {
        public static DataMatch Empty = new DataMatch(0, 0);
        public static DataMatch MatchOne = new DataMatch(1, 1);
        public static DataMatch NoMatchOne = new DataMatch(0, 1);

        public DataMatch(int count, int total)
        {
            Count = count;
            Total = total;
        }

        public readonly int Count;

        public readonly int Total;

        public bool Succeed
        {
            get
            {
                return Count == Total && Count >= 0 && Total >= 0;
            }
        }

        public bool Equals(DataMatch other)
        {
            return Count == other.Count && Total == other.Total;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DataMatch && Equals((DataMatch)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Count*397) ^ Total;
            }
        }

        public static DataMatch operator +(DataMatch left, DataMatch right)
        {
            return new DataMatch(left.Count + right.Count, left.Total + right.Total);
        }

        public static bool operator ==(DataMatch left, DataMatch right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DataMatch left, DataMatch right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Match {0}: {1} / {2}", Succeed, Count, Total);
        }
    }
}
// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Text.RegularExpressions;

namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// Represent a metadata key. This object is immutable.
    /// </summary>
    public struct MetadataKey : IEquatable<MetadataKey>
    {
        public enum DatabaseType  
        {
            Char,
            Byte,
            Short,
            UnsignedShort,
            Int,
            UnsignedInt,
            Long,
            UnsignedLong,
            Float,
            Double,
            String
        };

        public string Name { get; private set; }

        public DatabaseType Type { get; private set; }

        public MetadataKey(string key, DatabaseType type)
            : this()
        {
            if (key == null) throw new ArgumentNullException("key");
            Name = key;
            Type = type;
        }

        public bool IsValid()
        {
            return Name != null && Name.Length < 256 && Regex.IsMatch(Name, "\\w");
        }

        public static bool operator ==(MetadataKey left, MetadataKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MetadataKey left, MetadataKey right)
        {
            return (!left.Equals(right));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj.GetType() == GetType() && Equals((MetadataKey)obj);
        }

        public bool Equals(MetadataKey other)
        {
            return other.Name == Name && other.Type == Type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Type.GetHashCode();
            }
        }

        public override string ToString()
        {
            return (Name ?? "{null}") + " (" + Type + ")";
        }

        public object ConvertValue(string databaseValue)
        {
            switch (Type)
            {
                case DatabaseType.Char:
                    return databaseValue[0];
                case DatabaseType.Byte:
                    return byte.Parse(databaseValue);
                case DatabaseType.Short:
                    return short.Parse(databaseValue);
                case DatabaseType.UnsignedShort:
                    return ushort.Parse(databaseValue);
                case DatabaseType.Int:
                    return int.Parse(databaseValue);
                case DatabaseType.UnsignedInt:
                    return uint.Parse(databaseValue);
                case DatabaseType.Long:
                    return long.Parse(databaseValue);
                case DatabaseType.UnsignedLong:
                    return ulong.Parse(databaseValue);
                case DatabaseType.Float:
                    return float.Parse(databaseValue);
                case DatabaseType.Double:
                    return double.Parse(databaseValue);
                case DatabaseType.String:
                    return databaseValue;
                default:
                    throw new InvalidOperationException("Type is unknown or its conversion has not been implemented.");
            }
        }

        public Type GetKeyType()
        {
            switch (Type)
            {
                case DatabaseType.Char:
                    return typeof(char);
                case DatabaseType.Byte:
                    return typeof(byte);
                case DatabaseType.Short:
                    return typeof(short);
                case DatabaseType.UnsignedShort:
                    return typeof(ushort);
                case DatabaseType.Int:
                    return typeof(int);
                case DatabaseType.UnsignedInt:
                    return typeof(uint);
                case DatabaseType.Long:
                    return typeof(long);
                case DatabaseType.UnsignedLong:
                    return typeof(ulong);
                case DatabaseType.Float:
                    return typeof(float);
                case DatabaseType.Double:
                    return typeof(double);
                case DatabaseType.String:
                    return typeof(string);
                default:
                    throw new InvalidOperationException("Type is unknown.");
            }
        }

        public object GetDefaultValue()
        {
            return Type == DatabaseType.String ? "" : Activator.CreateInstance(GetKeyType());
        }
    }
}
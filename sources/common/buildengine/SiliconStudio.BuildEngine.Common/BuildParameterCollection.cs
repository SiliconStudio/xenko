// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.BuildEngine
{
    public class BuildConfiguration
    {
        public readonly string Name;
        public readonly BuildParameterCollection Parameters = new BuildParameterCollection();

        BuildConfiguration(string name)
        {
            Name = name;
        }
    }

    [DataContract]
    [DataSerializer(typeof(Serializer))]
    public struct BuildParameter
    {
        public readonly string Name;
        public readonly string Value;

        public BuildParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public bool Equals(BuildParameter other)
        {
            return string.Equals(Name, other.Name) && Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BuildParameter && Equals((BuildParameter)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public static bool operator ==(BuildParameter left, BuildParameter right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BuildParameter left, BuildParameter right)
        {
            return !left.Equals(right);
        }

        internal class Serializer : DataSerializer<BuildParameter>
        {
            public override void Serialize(ref BuildParameter obj, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Serialize)
                {
                    stream.Write(obj.Name);
                    stream.Write(obj.Value);
                }
                else
                {
                    var name = stream.ReadString();
                    var value = stream.ReadString();
                    obj = new BuildParameter(name, value);
                }
            }
        }
    }

    [DataContract]
    [DataSerializer(typeof(Serializer))]
    [DataSerializerGlobal(null, typeof(List<BuildParameter>))]
    public class BuildParameterCollection : IEnumerable<BuildParameter>
    {
        private readonly List<BuildParameter> parameterList = new List<BuildParameter>();

        public void Add(string name, string value)
        {
            Add(new BuildParameter(name, value));
        }

        public void Add(string name, IEnumerable<string> values)
        {
            foreach (string value in values)
            {
                Add(new BuildParameter(name, value));
            }
        }

        public void Add(BuildParameter parameter)
        {
            parameterList.Add(parameter);
        }

        public bool Contains(string name)
        {
            return parameterList.Any(x => x.Name == name);
        }

        public bool ContainsMultiple(string name)
        {
            return parameterList.Count(x => x.Name == name) > 1;
        }

        public bool ContainsSingle(string name)
        {
            return parameterList.Count(x => x.Name == name) == 1;
        }

        public IEnumerable<string> GetRange(string name)
        {
            return parameterList.Where(x => x.Name == name).Select(x => x.Value);
        }

        public string GetSingle(string name)
        {
            return parameterList.SingleOrDefault(x => x.Name == name).Value;
        }

        public IEnumerator<BuildParameter> GetEnumerator()
        {
            return parameterList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return parameterList.GetEnumerator();
        }

        internal class Serializer : DataSerializer<BuildParameterCollection>
        {
            public override void Serialize(ref BuildParameterCollection obj, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Serialize)
                {
                    stream.Write(obj.parameterList);
                }
                else
                {
                    var parameterList = stream.Read<List<BuildParameter>>();
                    obj = new BuildParameterCollection();
                    obj.parameterList.AddRange(parameterList);
                }
            }
        }
    }
}

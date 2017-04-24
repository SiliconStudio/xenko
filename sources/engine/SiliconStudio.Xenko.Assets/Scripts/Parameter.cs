// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract]
    public class Parameter : Symbol
    {
        public Parameter()
        {
        }

        public Parameter(string type, string name) : base(type, name)
        {
        }

        /// <summary>
        /// Describes whether the parameter is passed by value or by reference.
        /// </summary>
        [DataMember(-40)]
        public ParameterRefKind RefKind { get; set; }

        public override string ToString()
        {
            var result = base.ToString();

            // Add ref kind
            if (RefKind != ParameterRefKind.None)
                result = RefKind.ToString().ToLowerInvariant() + " " + result;

            return result;
        }
    }
}

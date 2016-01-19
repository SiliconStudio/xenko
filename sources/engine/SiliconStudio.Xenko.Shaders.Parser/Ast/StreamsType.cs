// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Xenko.Shaders.Parser.Ast
{
    /// <summary>
    /// A State type.
    /// </summary>
    public class StreamsType : ObjectType
    {
        #region Constants and Fields

        /// <summary>
        /// The constants streams.
        /// </summary>
        public static readonly StreamsType Constants = new StreamsType("Constants", false);

        /// <summary>
        /// The Input streams.
        /// </summary>
        public static readonly StreamsType Input = new StreamsType("Input");

        /// <summary>
        /// The Input2 type streams.
        /// </summary>
        public static readonly StreamsType Input2 = new StreamsType("Input2");

        /// <summary>
        /// The output type streams.
        /// </summary>
        public static readonly StreamsType Output = new StreamsType("Output");

        /// <summary>
        /// The streams type streams.
        /// </summary>
        public static readonly StreamsType Streams = new StreamsType("Streams");

        /// <summary>
        /// A fake variable of custom type stream
        /// </summary>
        public static readonly Variable ThisStreams = new Variable(Streams, "streams");


        /// <summary>
        /// Gets the streams.
        /// </summary>
        /// <returns>IEnumerable&lt;StreamsType&gt;.</returns>
        public static IEnumerable<StreamsType> GetStreams()
        {
            return AllTypes;
        }

        private static readonly StreamsType[] AllTypes = new[] { Constants, Input, Input2, Output, Streams};

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamsType"/> class.
        /// </summary>
        public StreamsType()
        {
            IsBuiltIn = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamsType" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isInputOutput">if set to <c>true</c> [is input output].</param>
        public StreamsType(string name, bool isInputOutput = true)
            : base(name)
        {
            IsBuiltIn = true;
            IsInputOutput = isInputOutput;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is input output.
        /// </summary>
        /// <value><c>true</c> if this instance is input output; otherwise, <c>false</c>.</value>
        public bool IsInputOutput { get; private set; }

        public static bool IsInputOutputStream(TypeBase type)
        {
            var streamType = type as StreamsType;
            if (streamType == null)
            {
                return false;
            }
            return streamType.IsInputOutput;
        }

        /// <inheritdoc/>
        public bool Equals(StreamsType other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return Equals(obj as StreamsType);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(StreamsType left, StreamsType right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(StreamsType left, StreamsType right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Parses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static StreamsType Parse(string name)
        {
            return AllTypes.FirstOrDefault(textureType => CultureInfo.InvariantCulture.CompareInfo.Compare(name, textureType.Name.Text, CompareOptions.None) == 0);
        }
    }
}

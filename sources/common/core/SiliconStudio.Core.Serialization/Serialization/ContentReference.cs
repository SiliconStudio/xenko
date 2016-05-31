// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Serialization
{
    internal abstract class ContentReference : ILoadableReference, IEquatable<ContentReference>
    {
        internal const int NullIdentifier = -1;

        /// <summary>
        /// Gets or sets the location of the referenced content.
        /// </summary>
        /// <value>
        /// The location of the referenced content.
        /// </value>
        public abstract string Location { get; set; }

        public ContentReferenceState State { get; protected set; }

        public abstract Type Type { get; }

        public abstract object ObjectValue { get; }

        public bool Equals(ContentReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Location, other.Location);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ContentReference)obj);
        }

        public override int GetHashCode()
        {
            return Location?.GetHashCode() ?? 0;
        }

        public static bool operator ==(ContentReference left, ContentReference right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ContentReference left, ContentReference right)
        {
            return !Equals(left, right);
        }


        public override string ToString()
        {
            return $"{{{Location}}}";
        }
    }

    [DataSerializer(typeof(ContentReferenceDataSerializer<>), Mode = DataSerializerGenericMode.GenericArguments)]
    internal sealed class ContentReference<T> : ContentReference where T : class
    {
        // Depending on state, either value or Location is null (they can't be both non-null)
        private T value;
        private string url;

        public override Type Type => typeof(T);

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReference"/> class with the given value.
        /// </summary>
        /// <param name="value">The value of the referenced conten.t</param>
        /// <remarks>This constructor should be used during serialization.</remarks>
        public ContentReference(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReference"/> class with the given id and location.
        /// </summary>
        /// <param name="location">The location of the referenced content.</param>
        /// <remarks>This constructor should be used during deserialization.</remarks>
        public ContentReference(string location)
        {
            Location = location;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <exception cref="System.InvalidOperationException">Value can't be read in this state.</exception>
        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                State = ContentReferenceState.Modified;
                this.value = value;
                url = null;
            }
        }

        public override string Location
        {
            get
            {
                // TODO: Should we return value.Location if value is not null, or just reference Location?
                if (ObjectValue == null)
                    return url;

                return AttachedReferenceManager.GetUrl(ObjectValue);
            }
            set
            {
                if (ObjectValue == null)
                {
                    url = value;
                }
                else
                {
                    AttachedReferenceManager.SetUrl(ObjectValue, value);
                }
            }
        }

        public override object ObjectValue => Value;
    }
}

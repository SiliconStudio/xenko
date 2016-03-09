// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Serialization
{
    [Obsolete("This class is deprecated will be removed in the future. Use AttachedReference instead.")]
    public abstract class ContentReference : ITypedContentReference, IEquatable<ContentReference>
    {
        internal const int NullIdentifier = -1;

        /// <summary>
        /// Gets or sets the asset unique identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the URL of the referenced data.
        /// </summary>
        /// <value>
        /// The URL of the referenced data.
        /// </value>
        public abstract string Location { get; set; }

        public ContentReferenceState State { get; set; }

        public abstract Type Type { get; }

        public abstract object ObjectValue { get; set; }

        public bool Equals(ContentReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Equals(Location, other.Location);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContentReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode()*397) ^ (Location?.GetHashCode() ?? 0);
            }
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
            return $"{Id}:{Location}";
        }
    }

    [DataSerializer(typeof(ContentReferenceDataSerializer<>), Mode = DataSerializerGenericMode.GenericArguments)]
    [Obsolete("This class is deprecated will be removed in the future. Use AttachedReference instead.")]
    public sealed class ContentReference<T> : ContentReference where T : class
    {
        // Depending on state, either value or Location is null (they can't be both non-null)
        private T value;
        private string url;

        public override Type Type => typeof(T);

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
                //if (State == ContentReferenceState.NeverLoad)
                //    throw new InvalidOperationException("Value can't be read in this state.");
                //lock (ContentManager)
                //{
                //    if (State == ContentReferenceState.LoadFirstAccess)
                //    {
                //        value = ContentManager.Load<T>(Location);
                //        State = ContentReferenceState.Loaded;
                //    }
                //    else if (State == ContentReferenceState.LoadEverytime)
                //    {
                //        return ContentManager.Load<T>(Location);
                //    }
                //}
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
                if (value == null)
                    return url;

                return AttachedReferenceManager.GetUrl(value);
            }
            set
            {
                if (this.value == null)
                {
                    url = value;
                }
                else
                {
                    AttachedReferenceManager.SetUrl(this.value, value);
                }
            }
        }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = (T)value; }
        }

        public ContentReference()
        {
            Value = null;
        }

        public ContentReference(Guid id, string location)
        {
            Id = id;
            Location = location;
        }

        public static implicit operator ContentReference<T>(T value)
        {
            return new ContentReference<T>() { Value = value };
        }


    }
}
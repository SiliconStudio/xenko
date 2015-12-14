// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A composite enum.
    /// </summary>
    public class CompositeEnum : Node, IEnumerable<CompositeEnum>
    {
        [VisitorIgnore]
        private OrderedSet<CompositeEnum> values;
        
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "CompositeEnum" /> class.
        /// </summary>
        public CompositeEnum()
        {
            Values = new OrderedSet<CompositeEnum>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeEnum"/> class.
        /// </summary>
        /// <param name="isFlag">
        /// if set to <c>true</c> [is flag].
        /// </param>
        public CompositeEnum(bool isFlag)
            : this()
        {
            IsFlag = isFlag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeEnum"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="isFlag">if set to <c>true</c> [is flag].</param>
        public CompositeEnum(object key, bool isFlag)
            : this(isFlag)
        {
            Key = key;
            Values.Add(this);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets a value indicating whether this instance is a composition enum (a combination of enums).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a composition enum (a combination of enums); otherwise, <c>false</c>.
        /// </value>
        public bool IsComposition
        {
            get
            {
                return Key == null;
            }
        }

        /// <summary>
        ///   Gets a value indicating whether this instance is an enum flag.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is an enum flag; otherwise, <c>false</c>.
        /// </value>
        public bool IsFlag { get; private set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public object Key { get; set; }


        /// <summary>
        ///   Gets or sets the values.
        /// </summary>
        /// <value>
        ///   The values.
        /// </value>
        public OrderedSet<CompositeEnum> Values
        {
            get
            {
                return values;
            }
            set
            {
                values = value;
            }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public virtual string DisplayName
        {
            get
            {
                return Key != null ? Key.ToString() : "null";
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether [contains] [the specified enum value].
        /// </summary>
        /// <param name="enumValue">
        /// The enum value.
        /// </param>
        /// <returns>
        /// <c>true</c> if [contains] [the specified enum value]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(CompositeEnum enumValue)
        {
            return enumValue.Values.IsSubsetOf(Values);
        }

        /// <summary>
        /// Determines whether [contains] [the specified enum values].
        /// </summary>
        /// <param name="enumValues">
        /// The enum values.
        /// </param>
        /// <returns>
        /// <c>true</c> if [contains] [the specified enum values]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(params CompositeEnum[] enumValues)
        {
            return enumValues.Any(Contains);
        }

        /// <summary>
        /// Determines whether the specified enum values contains all.
        /// </summary>
        /// <param name="enumValues">
        /// The enum values.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified enum values contains all; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsAll(params CompositeEnum[] enumValues)
        {
            return enumValues.All(Contains);
        }

        /// <summary>
        /// Determines whether the specified <see cref="CompositeEnum"/> is equal to this instance.
        /// </summary>
        /// <param name="other">
        /// The <see cref="CompositeEnum"/> to compare with this instance.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="CompositeEnum"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(CompositeEnum other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other.IsFlag != IsFlag)
            {
                return false;
            }

            if (Values.Count != other.Values.Count)
            {
                return false;
            }

            if (Key != null && other.Key != null)
            {
                return Key.Equals(other.Key);
            }

            // Optim to speed up comparison
            if (ReferenceEquals(Values, other.Values))
            {
                return true;
            }

            return Values.SetEquals(other.Values);
        }

        /// <inheritdoc />
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

            if (!(obj is CompositeEnum))
            {
                return false;
            }

            return Equals((CompositeEnum)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hashCode = IsFlag.GetHashCode() * 397;
            if (Key != null)
            {
                return hashCode ^ Key.GetHashCode() * 397;
            } 

            return Values.Aggregate(hashCode, (current, value) => current ^ value.GetHashCode() * 397);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<CompositeEnum> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToString<CompositeEnum>(null);
        }

        /// <inheritdoc />
        public string ToString<T>(Func<T,bool> filterEnum) where T : CompositeEnum
        {
            var builder = new StringBuilder();
            bool isNext = false;
            var filteredValues = Values.OfType<T>();
            if (filterEnum != null)
            {
                filteredValues = filteredValues.Where(filterEnum);
            }

            foreach (var value in filteredValues)
            {
                if (isNext)
                {
                    builder.Append(" ");
                }

                if (value.Key != null)
                {
                    if (string.Empty.Equals(value.Key))
                    {
                        isNext = false;
                    }
                    else
                    {
                        builder.Append(value.DisplayName);
                        isNext = true;
                    }
                }
                else
                {
                    builder.Append(value.ToString());
                    isNext = true;
                }
            }

            return builder.ToString();
        }


        #endregion

        #region Methods

        /// <summary>
        /// Operators And.
        /// </summary>
        /// <typeparam name="T1">
        /// The type of the 1.
        /// </typeparam>
        /// <param name="left">
        /// The left.
        /// </param>
        /// <param name="right">
        /// The right.
        /// </param>
        /// <returns>
        /// Result of And operation
        /// </returns>
        public static T1 OperatorAnd<T1>(T1 left, T1 right)
            where T1 : CompositeEnum, new()
        {
            var result = new T1 { IsFlag = left.IsFlag, Values = new OrderedSet<CompositeEnum>(left.Values) };
            result.Values.IntersectWith(right.Values);
            return result;
        }

        /// <summary>
        /// Operators Or.
        /// </summary>
        /// <typeparam name="T1">
        /// The type of the 1.
        /// </typeparam>
        /// <param name="left">
        /// The left.
        /// </param>
        /// <param name="right">
        /// The right.
        /// </param>
        /// <returns>
        /// Result of Or operation
        /// </returns>
        public static T1 OperatorOr<T1>(T1 left, T1 right)
            where T1 : CompositeEnum, new()
        {
            var result = new T1 { IsFlag = left.IsFlag, Values = new OrderedSet<CompositeEnum>(left.Values) };
            result.Values.UnionWith(right.Values);
            return result;
        }

        /// <summary>
        /// Operators Xor.
        /// </summary>
        /// <typeparam name="T1">
        /// The type of the 1.
        /// </typeparam>
        /// <param name="left">
        /// The left.
        /// </param>
        /// <param name="right">
        /// The right.
        /// </param>
        /// <returns>
        /// Result of Xor operation
        /// </returns>
        public static T1 OperatorXor<T1>(T1 left, T1 right)
            where T1 : CompositeEnum, new()
        {
            var result = new T1 { IsFlag = left.IsFlag, Values = new OrderedSet<CompositeEnum>(left.Values) };
            result.Values.SymmetricExceptWith(right.Values);
            return result;
        }

        /// <summary>
        /// Prepares the parsing for a specific enum by returning a pre-computed dictionary with all allowed mapping name =&gt; enum.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the enum to compute the dictionary
        /// </typeparam>
        /// <returns>
        /// A pre-computed dictionary with all allowed mapping name =&gt; enum
        /// </returns>
        protected static StringEnumMap PrepareParsing<T>() where T : CompositeEnum
        {
            var map = new StringEnumMap();

            var type = typeof(T);
            while (type != typeof(CompositeEnum))
            {
                foreach (var field in type.GetFields())
                {
                    if (typeof(CompositeEnum).GetTypeInfo().IsAssignableFrom(field.FieldType.GetTypeInfo()) && field.IsStatic)
                    {
                        var fieldValue = (CompositeEnum)field.GetValue(null);
                        if (fieldValue.Values.Count == 1)
                        {
                            var key = fieldValue.Values.FirstOrDefault();
                            if (key != null && key.Key != null && !map.ContainsKey(key.Key))
                            {
                                map.Add(key.Key, fieldValue);
                            }
                        }
                    }
                }

                type = type.GetTypeInfo().BaseType;
            }

            return map;
        }

        /// <inheritdoc/>
        public override IEnumerable<Node> Childrens()
        {
            if (Values.Count == 0) return ChildrenList;

            ChildrenList.Clear();
            foreach (var compositeEnum in Values)
            {
                if (!ReferenceEquals(this, compositeEnum) && !string.Empty.Equals(compositeEnum.Key))
                {
                    ChildrenList.Add(compositeEnum);
                }
            }

            return ChildrenList;
        }

        #endregion

        #region Operators

        /// <summary>
        ///   Implements the operator ==.
        /// </summary>
        /// <param name = "left">The left.</param>
        /// <param name = "right">The right.</param>
        /// <returns>
        ///   The result of the operator.
        /// </returns>
        public static bool operator ==(CompositeEnum left, CompositeEnum right)
        {
            return Equals(left, right);
        }
        
        /// <summary>
        ///   Implements the operator !=.
        /// </summary>
        /// <param name = "left">The left.</param>
        /// <param name = "right">The right.</param>
        /// <returns>
        ///   The result of the operator.
        /// </returns>
        public static bool operator !=(CompositeEnum left, CompositeEnum right)
        {
            return !Equals(left, right);
        }

        #endregion

        /// <summary>
        /// Internal dictionary that provides conversion helper methods.
        /// </summary>
        protected class StringEnumMap : Dictionary<object, CompositeEnum>
        {
            #region Public Methods

            /// <summary>
            /// Parses the name of the enum from.
            /// </summary>
            /// <typeparam name="T">Type of the enum</typeparam>
            /// <param name="key">The key.</param>
            /// <returns>
            /// The converted enum or null otherwises
            /// </returns>
            public T ParseEnumFromName<T>(object key) where T : CompositeEnum, new()
            {
                CompositeEnum value;
                if (!TryGetValue(key, out value))
                {
                    throw new ArgumentException(string.Format("Unable to convert [{0}] to qualifier", key), "key");
                    //var none = new T { Key = string.Empty };
                    //none.Values.Add(none);
                    //return none;
                }

                // If same value, than return it directly
                if (typeof(T).GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))
                {
                    return (T)value;
                }

                return new T { IsFlag = value.IsFlag, Values = value.Values };
            }

            #endregion
        }
    }
}
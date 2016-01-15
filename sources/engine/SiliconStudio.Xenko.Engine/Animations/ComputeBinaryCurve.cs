// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq.Expressions;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Animations
{
    public enum BinaryCurveOperator
    {
        /// <summary>
        /// Add the sampled value of both sides.
        /// </summary>
        Add,

        /// <summary>
        /// Subtracts the right sampled value from the left sampled value.
        /// </summary>
        Subtract,

    }

    /// <summary>
    /// A node which describes a binary operation between two compute curves
    /// </summary>
    /// <typeparam name="T">Sampled data's type</typeparam>
    [DataContract(Inherited = true)]
    [Display("Binary Operator")]
    [InlineProperty]
    public abstract class ComputeBinaryCurve<T> : IComputeCurve<T> where T : struct
    {
        /// <inheritdoc/>
        public T SampleAt(float location)
        {
            var lValue = LeftChild?.SampleAt(location) ?? new T();
            var rValue = RightChild?.SampleAt(location) ?? new T();

            switch (Operator)
            {
                case BinaryCurveOperator.Add:
                    return Add(lValue, rValue);

                case BinaryCurveOperator.Subtract:
                    return Subtract(lValue, rValue);

            }

            throw new ArgumentException("Invalid Operator argument in ComputeBinaryCurve");
        }

        /// <summary>
        /// The operation used to blend the two values
        /// </summary>
        /// <userdoc>
        /// The operation used to blend the two values
        /// </userdoc>
        [DataMember(10)]
        [InlineProperty]
        public BinaryCurveOperator Operator { get; set; } = BinaryCurveOperator.Add;

        /// <summary>
        /// The left child node
        /// </summary>
        /// <userdoc>
        /// The left child value
        /// </userdoc>
        [DataMember(20)]
        [Display("Left")]
        public IComputeCurve<T> LeftChild { get; set; }

        /// <summary>
        /// The right child node
        /// </summary>
        /// <userdoc>
        /// The right child value
        /// </userdoc>
        [DataMember(30)]
        [Display("Right")]
        public IComputeCurve<T> RightChild { get; set; }

        /// <summary>
        /// Adds the left value to the right value and retuns their sum
        /// </summary>
        /// <param name="a">Left value A</param>
        /// <param name="b">Right value B</param>
        /// <returns>The sum A + B</returns>
        static T Add(T a, T b)
        {
            // TODO Test performance
            ParameterExpression paramA = Expression.Parameter(typeof(T), "a");
            ParameterExpression paramB = Expression.Parameter(typeof(T), "b");
            BinaryExpression body = Expression.Add(paramA, paramB);

            Func<T, T, T> add = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();

            return add(a, b);
        }

        /// <summary>
        /// Subtracts the right value from the left value and retuns the result
        /// </summary>
        /// <param name="a">Left value A</param>
        /// <param name="b">Right value B</param>
        /// <returns>The result A - B</returns>
        static T Subtract(T a, T b)
        {
            // TODO Test performance
            ParameterExpression paramA = Expression.Parameter(typeof(T), "a");
            ParameterExpression paramB = Expression.Parameter(typeof(T), "b");
            BinaryExpression body = Expression.Subtract(paramA, paramB);

            Func<T, T, T> subtract = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();

            return subtract(a, b);
        }
    }
}

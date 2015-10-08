// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq.Expressions;

namespace SiliconStudio.Presentation.Core
{
    /// <summary>
    /// A class that provides runtime evaluation of member names using <see cref="Expression"/>. Note that this class should not be used when performances matter.
    /// </summary>
    /// <typeparam name="TType">The type that contains the member to retrieve.</typeparam>
    [Obsolete(message: "Use nameof operator instead (see https://msdn.microsoft.com/en-us/library/dn986596.aspx).", error: true)]
    public static class NameOf<TType>
    {
        /// <summary>
        /// Gets the name of the given member.
        /// </summary>
        /// <param name="expression">An expression accessing the member. Must be a <see cref="MemberExpression"/>/</param>
        /// <returns>The name of the given member.</returns>
        public static string Member<TMember>(Expression<Func<TType, TMember>> expression)
        {
            var body = expression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("The given expression must be a MemberExpression.");
            return body.Member.Name;
        }
    }
}

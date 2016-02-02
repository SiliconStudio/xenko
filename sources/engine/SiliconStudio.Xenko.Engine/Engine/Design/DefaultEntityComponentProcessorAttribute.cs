// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Engine.Design
{
    /// <summary>
    /// An attribute used to associate a default <see cref="EntityProcessor"/> to an entity component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DefaultEntityComponentProcessorAttribute : DynamicTypeAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEntityComponentProcessorAttribute"/> class.
        /// </summary>
        /// <param name="type">The type must derived from <see cref="EntityProcessor"/>.</param>
        public DefaultEntityComponentProcessorAttribute(Type type) : base(type)
        {
        }

        public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.All;
    }
} 
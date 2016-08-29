// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// Definition of a class.
    /// </summary>
    public partial class ClassType
    {
        /// <summary>
        /// Determines whether the specified type is a a stream type.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns><c>true</c> if [the specified target type] [is stream type] ; otherwise, <c>false</c>.</returns>
        public static bool IsStreamOutputType(TypeBase targetType)
        {
            return targetType is ClassType && ((ClassType)targetType).GenericArguments.Count > 0 && targetType.IsStreamTypeName();
        }
    }
}
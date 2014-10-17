// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Contains the value of an asset member returned by <see cref="AssetItemAccessor.TryGetMemberValue"/>
    /// </summary>
    public struct AssetMemberValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetMemberValue"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="override">The override.</param>
        /// <param name="overriderItem">The overrider item.</param>
        public AssetMemberValue(object value, OverrideType @override, AssetItem overriderItem)
            : this()
        {
            IsValid = true;
            Value = value;
            Override = @override;
            OverriderItem = overriderItem;
        }

        /// <summary>
        /// Gets the valid state of this instance, <c>true</c> if the object is valid; otherwise <c>false</c>
        /// </summary>
        public readonly bool IsValid;

        /// <summary>
        /// The value of the member.
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// The override state of this member.
        /// </summary>
        public readonly OverrideType Override;

        /// <summary>
        /// The overrider item if any, or <c>null</c> if no base overriders.
        /// </summary>
        public readonly AssetItem OverriderItem;
    }
}
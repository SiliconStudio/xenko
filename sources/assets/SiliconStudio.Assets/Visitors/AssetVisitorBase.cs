// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Visitors
{
    /// <summary>
    /// Visitor for assets.
    /// </summary>
    public abstract class AssetVisitorBase : DataVisitorBase
    {
        protected AssetVisitorBase() : this(Core.Reflection.TypeDescriptorFactory.Default)
        {
        }

        protected AssetVisitorBase(ITypeDescriptorFactory typeDescriptorFactory) : base(typeDescriptorFactory)
        {
            // Add automatically registered custom data visitors
            CustomVisitors.AddRange(AssetRegistry.GetDataVisitNodes());
        }
    }
}
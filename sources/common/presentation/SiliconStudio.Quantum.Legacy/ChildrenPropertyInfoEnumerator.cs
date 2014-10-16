// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

using System.Reflection;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.Quantum.Legacy
{
    public class ChildrenPropertyInfoEnumerator : IChildrenPropertyEnumerator
    {
        /// <inheritdoc/>
        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            foreach (var propertyInfo in viewModelNode.Content.Type.GetTypeInfo().GetProperties()
                .Where(x => x.PropertyType == typeof(string) || x.PropertyType == typeof(int) || x.PropertyType == typeof(Guid)))
            {
                var childNode = new ViewModelNode(propertyInfo.Name, new PropertyInfoViewModelContent(new ParentValueViewModelContent(), propertyInfo));
                viewModelNode.Children.Add(childNode);
                handled = true;
            }
        }
    }
}
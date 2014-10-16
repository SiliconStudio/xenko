// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

namespace SiliconStudio.Quantum.Legacy
{
    public static class ViewModelExtensions
    {
        public static IViewModelNode GenerateChildren(this IViewModelNode viewModelNode, ViewModelContext context, IChildrenPropertyEnumerator[] additionalEnumerators = null)
        {
            context.GenerateChildren(viewModelNode, additionalEnumerators);
            return viewModelNode;
        }

        public static IViewModelNode GetChild(this IViewModelNode viewModelNode, string name)
        {
            return viewModelNode.Children.FirstOrDefault(modelNode => modelNode.Name == name);
        }
    }
}
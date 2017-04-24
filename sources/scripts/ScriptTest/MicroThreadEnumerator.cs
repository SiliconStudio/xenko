// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Games.MicroThreading;

using SiliconStudio.Xenko.Games.ViewModel;

namespace ScriptTest
{
    class MicroThreadEnumerator : IChildrenPropertyEnumerator
    {
        private ViewModelContext selectedEntitiesContext;

        public MicroThreadEnumerator(ViewModelContext selectedEntitiesContext)
        {
            this.selectedEntitiesContext = selectedEntitiesContext;
        }
        
        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            if (viewModelNode.NodeValue is MicroThread)
            {
                viewModelNode.Children.Add(new ViewModelNode("Id", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(MicroThread).GetProperty("Id"))));
                viewModelNode.Children.Add(new ViewModelNode("Name", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(MicroThread).GetProperty("Name"))));
                viewModelNode.Children.Add(new ViewModelNode("ScriptName",
                    LambdaViewModelContent<string>.FromParent<MicroThread>(x => x.Get(ScriptManager.ScriptEntryProperty).TypeName)));

                viewModelNode.Children.Add(new ViewModelNode("EventOpen", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                {
                    selectedEntitiesContext.ViewModelByGuid.Clear();
                    selectedEntitiesContext.Root = selectedEntitiesContext.GetModelView(viewModel2.Parent.NodeValue);
                }))));
                handled = true;
            }
        }
    }
}

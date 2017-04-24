// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Linq;

using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Games.ViewModel;

namespace ScriptTest
{
    public class ScriptAssemblyEnumerator : IChildrenPropertyEnumerator
    {
        private EngineContext engineContext;

        public ScriptAssemblyEnumerator(EngineContext engineContext)
        {
            this.engineContext = engineContext;
        }

        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            var type = viewModelNode.Type;
            if (viewModelNode.NodeValue is ScriptAssembly)
            {
                viewModelNode.Children.Add(new ViewModelNode("Url", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(ScriptAssembly).GetProperty("Url"))));
                viewModelNode.Children.Add(new ViewModelNode("Scripts", EnumerableViewModelContent.FromUnaryLambda<ViewModelReference, ScriptAssembly>(new ParentNodeValueViewModelContent(),
                    (scriptAssembly) => scriptAssembly.Scripts.Select(x => new ViewModelReference(x, true)))));
                handled = true;
            }
            if (viewModelNode.NodeValue is ScriptEntry2)
            {
                viewModelNode.Children.Add(new ViewModelNode("TypeName", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(ScriptEntry2).GetProperty("TypeName"))));
                viewModelNode.Children.Add(new ViewModelNode("MethodName", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(ScriptEntry2).GetProperty("MethodName"))));
                viewModelNode.Children.Add(new ViewModelNode("Run", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                    {
                        var scriptEntry = (ScriptEntry2)viewModel2.Parent.NodeValue;
                        var microThread = engineContext.ScriptManager.RunScript(scriptEntry, null);
                    }))));
                handled = true;
            }
        }
    }
}

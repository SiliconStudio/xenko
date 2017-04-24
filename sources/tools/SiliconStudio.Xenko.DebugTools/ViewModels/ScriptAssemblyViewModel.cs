// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using SiliconStudio.Presentation;

namespace SiliconStudio.Xenko.DebugTools.ViewModels
{
    public class ScriptAssemblyViewModel : DeprecatedViewModelBase
    {
        public RootViewModel Parent { get; private set; }

        private readonly ScriptAssembly scriptAssembly;

        public ScriptAssemblyViewModel(ScriptAssembly scriptAssembly, RootViewModel parent)
        {
            if (scriptAssembly == null)
                throw new ArgumentNullException("scriptAssembly");

            if (parent == null)
                throw new ArgumentNullException("parent");

            Parent = parent;
            this.scriptAssembly = scriptAssembly;

            UpdateScripts();
        }

        public string Url
        {
            get
            {
                if (scriptAssembly.Url == null)
                    return "<anonymous assembly>";
                return scriptAssembly.Url.TrimStart('/');
            }
        }

        public string Assembly
        {
            get
            {
                if (scriptAssembly.Assembly == null)
                    return "-";
                return scriptAssembly.Assembly.ToString();
            }
        }

        public IEnumerable<ScriptTypeViewModel> Types { get; private set; }

        internal void UpdateScripts()
        {
            Types = from script in scriptAssembly.Scripts group script by script.TypeName into g select new ScriptTypeViewModel(g.Key, g, this);
            OnPropertyChanged<ScriptAssemblyViewModel>(n => n.Types);
        }
    }
}

// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Framework.MicroThreading;
using SiliconStudio.Xenko.Extensions;
using System.Windows.Input;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation;

namespace SiliconStudio.Xenko.DebugTools.ViewModels
{
    public class ScriptTypeViewModel : DeprecatedViewModelBase
    {
        public ScriptAssemblyViewModel Parent { get; private set; }
        public IEnumerable<ScriptMethodViewModel> Methods { get; private set; }

        private readonly string typeName;

        public ScriptTypeViewModel(string typeName, IEnumerable<ScriptEntry2> scriptEntries, ScriptAssemblyViewModel parent)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            if (scriptEntries == null)
                throw new ArgumentNullException("scriptEntries");

            if (parent == null)
                throw new ArgumentNullException("parent");

            Parent = parent;
            this.typeName = typeName;

            Methods = scriptEntries.Select(item => new ScriptMethodViewModel(item, this));
        }

        public string[] Namespaces
        {
            get
            {
                string ns = Namespace;
                return ns != null ? ns.Split('.') : null;
            }
        }

        public string Namespace
        {
            get
            {
                string[] elements = typeName.Split('.');

                if (elements.Length == 1)
                    return null; // namespace-less type

                return string.Join(".", elements.Take(elements.Length - 1));
            }
        }

        public string[] TypeNames
        {
            get
            {
                return TypeName.Split('+'); // to get nested types
            }
        }

        public string TypeName
        {
            get
            {
                return typeName.Split('.').Last();
            }
        }

        public string FullName
        {
            get
            {
                return typeName;
            }
        }
    }
}

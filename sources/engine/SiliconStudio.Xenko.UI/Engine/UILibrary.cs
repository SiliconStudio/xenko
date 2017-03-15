// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("UIlibrary")]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<UILibrary>))]
    [ReferenceSerializer, DataSerializerGlobal(typeof(ReferenceSerializer<UILibrary>), Profile = "Content")]
    public class UILibrary : ComponentBase
    {
        public UILibrary()
        {
            UIElements = new Dictionary<string, UIElement>();
        }

        /// <summary>
        /// Gets the UI elements.
        /// </summary>
        public Dictionary<string, UIElement> UIElements { get; }
    }
}

// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Input.Data
{
    [ContentSerializer(typeof(DataContentSerializer<MouseVirtualButtonDesc>))]
    [DataContract]
    public class MouseVirtualButtonDesc : VirtualButtonDescBase, IVirtualButtonDesc
    {
        [DataMember]
        public MouseButton MouseButton;

        public IVirtualButton Create()
        {
            return new VirtualButton.Keyboard(VirtualButtonType.Mouse, (int)MouseButton);
        }
    }
}
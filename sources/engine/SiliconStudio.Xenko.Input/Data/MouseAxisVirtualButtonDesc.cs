// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Input.Data
{
    [ContentSerializer(typeof(DataContentSerializer<MouseAxisVirtualButtonDesc>))]
    [DataContract]
    public class MouseAxisVirtualButtonDesc : VirtualButtonDescBase, IVirtualButtonDesc
    {
        [DataMember]
        public MouseAxis MouseAxis;

        public IVirtualButton Create()
        {
            return new VirtualButton.Mouse(VirtualButtonType.Mouse, (int)MouseAxis + 5);
        }
    }
}
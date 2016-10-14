// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Input.Data
{
    [ContentSerializer(typeof(DataContentSerializer<KeyboardVirtualButtonDesc>))]
    [DataContract]
    public class KeyboardVirtualButtonDesc : VirtualButtonDescBase, IVirtualButtonDesc
    {
        [DataMember]
        public Keys KeyboardKey;

        public IVirtualButton Create()
        {
            return new VirtualButton.Keyboard(VirtualButtonType.Keyboard, (int)KeyboardKey);
        }
    }
}
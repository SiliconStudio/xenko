// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Input.Data
{
    [ContentSerializer(typeof(DataContentSerializer<GamePadVirtualButtonDesc>))]
    [DataContract]
    public class GamePadVirtualButtonDesc : VirtualButtonDescBase, IVirtualButtonDesc
    {
        [DataMember]
        public GamePadButtonSingle GamePadButton;

        public IVirtualButton Create()
        {
            return new VirtualButton.Keyboard(VirtualButtonType.GamePad, (int)GamePadButton);
        }
    }
}
// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class FakeTexture : Texture
    {
        public FakeTexture()
        {
        }

        public override Texture ToTexture(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            throw new NotImplementedException();
        }

        public override Texture Clone()
        {
            throw new NotImplementedException();
        }

        public override Texture ToStaging()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Fake 2D texture (URL should point to Image).</summary>
    public class FakeTexture2D : Texture2D
    {
        public FakeTexture2D()
        {
        }
    }
}
// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using Microsoft.CodeAnalysis;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public struct SlotGeneratorContext
    {
        public SlotGeneratorContext(Compilation compilation)
        {
            Compilation = compilation;
        }

        public Compilation Compilation { get; }
    }
}

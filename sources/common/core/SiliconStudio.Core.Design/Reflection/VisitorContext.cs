// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Reflection
{
    public struct VisitorContext
    {
        public IDataVisitor Visitor { get; set; }

        public ITypeDescriptorFactory DescriptorFactory { get; set; }

        public object Instance { get; set; }

        public ObjectDescriptor Descriptor { get; set; }
    }
}

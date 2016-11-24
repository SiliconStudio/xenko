// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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

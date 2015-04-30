// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Engine.Design
{
    public class ContentReferenceCloneDataSerializer<T> : DataSerializer<ContentReference<T>> where T : class
    {
        public override void Serialize(ref ContentReference<T> contentReference, ArchiveMode mode, SerializationStream stream)
        {
            var cloneContext = stream.Context.Get(EntityCloner.CloneContextProperty);
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(cloneContext.ContentReferences.Count);
                cloneContext.ContentReferences.Add(contentReference);
            }
            else
            {
                int index = stream.ReadInt32();
                contentReference = (ContentReference<T>)cloneContext.ContentReferences[index];
            }
        }
    }
}
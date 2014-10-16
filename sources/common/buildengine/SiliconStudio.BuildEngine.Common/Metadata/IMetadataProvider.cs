// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.BuildEngine
{
    public interface IMetadataProvider : IDisposable
    {
        bool Open(string path, bool create);
        void Close();

        IEnumerable<MetadataKey> FetchAllKeys();
        IEnumerable<string> FetchAllObjectUrls();

        IEnumerable<IObjectMetadata> Fetch(string objectUrl);
        IEnumerable<IObjectMetadata> Fetch(MetadataKey key);
        IObjectMetadata Fetch(string objectUrl, MetadataKey key);
        IObjectMetadata Fetch(IObjectMetadata data);
        IEnumerable<IObjectMetadata> FetchAll();

        bool Write(IObjectMetadata data);
        bool Delete(IObjectMetadata data);
    }
}
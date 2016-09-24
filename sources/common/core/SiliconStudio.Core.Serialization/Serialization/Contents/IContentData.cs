// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Serialization.Contents
{
    /// <summary>
    /// A content data storing its own Location.
    /// </summary>
    public interface IContentData
    {
        string Url { get; set; }
    }
}
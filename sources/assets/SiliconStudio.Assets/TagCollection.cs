// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A collection of tags.
    /// </summary>
    [DataContract("TagCollection")]
    public class TagCollection : List<string>
    {
    }
}
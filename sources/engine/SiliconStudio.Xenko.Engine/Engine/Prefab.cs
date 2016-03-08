// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A prefab that contains entities.
    /// </summary>
    [DataContract("Prefab")]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<Prefab>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<Prefab>), Profile = "Content")]
    public sealed class Prefab : PrefabBase
    {
    }
}
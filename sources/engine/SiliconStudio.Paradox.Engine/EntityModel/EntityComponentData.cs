// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.EntityModel.Data
{
    public partial class EntityComponentData
    {
        /// <summary>
        /// Entity will get updated when added to EntityData.Components.
        /// </summary>
        [DataMemberIgnore]
        public EntityData Entity;
    }

    // Components as they are stored in the git shared scene file
    public partial class EntityComponentData
    {
        // Used to store entity data while in merge/text mode
        public static PropertyKey<EntityComponentData> Key = new PropertyKey<EntityComponentData>("Key", typeof(EntityComponentData));

        //public List<EntityComponentProperty> Properties;
    }
}
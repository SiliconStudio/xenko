// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Text;

using SiliconStudio.AssemblyProcessor.Serializers;

namespace SiliconStudio.AssemblyProcessor
{
    internal class ProfileSerializerProcessor : ICecilSerializerProcessor
    {
        public void ProcessSerializers(CecilSerializerContext context)
        {
            var defaultProfile = context.SerializableTypes;

            foreach (var profile in context.SerializableTypesProfiles)
            {
                // Skip default profile
                if (profile.Value == defaultProfile)
                    continue;

                defaultProfile.IsFrozen = true;

                // For each profile, try to instantiate all types existing in default profile
                foreach (var type in defaultProfile.SerializableTypes)
                {
                    context.GenerateSerializer(type.Key, false, profile.Key);
                }
            }
        }
    }
}
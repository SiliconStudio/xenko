// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Paradox.Animations;
using Quaternion = SiliconStudio.Core.Mathematics.Quaternion;
using Vector3 = SiliconStudio.Core.Mathematics.Vector3;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Applies animation from a <see cref="AnimationClip"/> to a <see cref="ModelViewHierarchyUpdater"/>.
    /// </summary>
    public class MeshAnimationUpdater
    {
        // Keep track of source channel (if changed, we should regenerate updateChannels)
        private List<AnimationBlender.Channel> currentSourceChannels;
        private int currentSourceChannelCount;
        private UpdateChannel[] updateChannels;

        public unsafe void Update(ModelViewHierarchyUpdater hierarchy, AnimationClipResult result)
        {
            // Check if we need to regenerate "update channels" (i.e. how to copy data from result to hierarchy)
            if (updateChannels == null // First time?...
                || currentSourceChannels != result.Channels // ... or changed?
                || currentSourceChannels.Count != currentSourceChannelCount) // .. or modified? (could only append channel)
            {
                RegenerateUpdateChannels(hierarchy, result.Channels);
                currentSourceChannels = result.Channels;
                currentSourceChannelCount = currentSourceChannels.Count;
            }

            // Copy results to node hierarchy
            fixed (byte* structures = result.Data)
            {
                foreach (var updateChannel in updateChannels)
                {
                    var structureData = (float*)(structures + updateChannel.Offset);
                    var factor = *structureData++;
                    if (factor == 0.0f)
                        continue;

                    switch (updateChannel.Type)
                    {
                        case ChannelType.Translation:
                            Utilities.Read((IntPtr)structureData, ref hierarchy.NodeTransformations[updateChannel.Index].Transform.Translation);
                            break;
                        case ChannelType.Rotation:
                            Utilities.Read((IntPtr)structureData, ref hierarchy.NodeTransformations[updateChannel.Index].Transform.Rotation);
                            break;
                        case ChannelType.Scaling:
                            Utilities.Read((IntPtr)structureData, ref hierarchy.NodeTransformations[updateChannel.Index].Transform.Scaling);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private static bool NodeNameMatch(string name1, string name2)
        {
            // Try exact match
            if (name1 == name2)
                return true;

            // Try to match only after : so that patterns like "Model::j_rarm03" and "Model::mc00_rig:protomodel:j_rarm03" matches (happens with FBX references and Maya)
            // TODO: Maybe this should be done at FBX export time? (need to find the FBX naming pattern w/ references -- and check it with every DCC tool)
            name1 = name1.Substring(name1.LastIndexOf(':') + 1);
            name2 = name2.Substring(name2.LastIndexOf(':') + 1);

            return name1 == name2;
        }

        private void RegenerateUpdateChannels(ModelViewHierarchyUpdater hierarchy, List<AnimationBlender.Channel> channels)
        {
            var newUpdateChannels = new List<UpdateChannel>();

            // TODO: Temporary implementation due to lack of time before first release.
            foreach (var channel in channels)
            {
                string nodeName = channel.NodeName;
                if (nodeName == null)
                    continue;

                var updateChannel = new UpdateChannel();
                updateChannel.Index = -1;

                var hierarchyNodes = hierarchy.Nodes;
                for (int i = 0; i < hierarchyNodes.Length; ++i)
                {
                    var node = hierarchyNodes[i];
                    if (node.Name == nodeName)
                    {
                        updateChannel.Index = i;
                        break;
                    }
                }

                if (updateChannel.Index == -1)
                {
                    // TODO: Warning?
                    //throw new InvalidOperationException(string.Format("Could not find matching node in animation for {0}", nodeName));
                    continue;
                }

                updateChannel.Offset = channel.Offset;
                updateChannel.Type = channel.Type;

                newUpdateChannels.Add(updateChannel);
            }

            updateChannels = newUpdateChannels.ToArray();
        }

        internal static ChannelType GetType(string propertyName)
        {
            if (propertyName.StartsWith("Transform.Position["))
            {
                return ChannelType.Translation;
            }
            if (propertyName.StartsWith("Transform.Rotation["))
            {
                return ChannelType.Rotation;
            }
            if (propertyName.StartsWith("Transform.Scale["))
            {
                return ChannelType.Scaling;
            }

            return ChannelType.Unknown;
        }

        internal static string GetNodeName(string propertyName)
        {
            int nodeNameFirstChar = propertyName.IndexOf('[');
            if (nodeNameFirstChar == -1)
                return null;

            int nodeNameLastChar = propertyName.IndexOf(']', nodeNameFirstChar);

            return propertyName.Substring(nodeNameFirstChar + 1, nodeNameLastChar - nodeNameFirstChar - 1);
        }

        [DataContract]
        public enum ChannelType
        {
            Unknown,
            Translation,
            Rotation,
            Scaling,
        }

        /// <summary>
        /// Describes how to update data from <see cref="AnimationClipResult"/> to <see cref="ModelViewHierarchyUpdater"/>.
        /// </summary>
        private struct UpdateChannel
        {
            public ChannelType Type;
            public int Index;
            public int Offset;
        }
    }
}
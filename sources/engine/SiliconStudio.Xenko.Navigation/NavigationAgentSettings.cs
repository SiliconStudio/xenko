// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// Navigation agent
    /// </summary>
    [DataContract]
    [ObjectFactory(typeof(NavigationAgentSettingsFactory))]
    public struct NavigationAgentSettings
    {
        /// <summary>
        /// Height of the actor
        /// </summary>
        [DataMemberRange(0, float.MaxValue)] public float Height;

        /// <summary>
        /// Radius of the actor
        /// </summary>
        [DataMemberRange(0, float.MaxValue)] public float Radius;

        /// <summary>
        /// Maximum vertical distance this agent can climb
        /// </summary>
        [Display("Maximum Climb Height")] [DataMemberRange(0, float.MaxValue)] public float MaxClimb;

        /// <summary>
        /// Maximum slope angle this agent can climb (in degrees)
        /// </summary>
        [Display("Maximum Slope")] public AngleSingle MaxSlope;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Height.GetHashCode();
                hashCode = (hashCode*397) ^ Radius.GetHashCode();
                hashCode = (hashCode*397) ^ MaxClimb.GetHashCode();
                hashCode = (hashCode*397) ^ MaxSlope.GetHashCode();
                return hashCode;
            }
        }
    }

    public class NavigationAgentSettingsFactory : IObjectFactory
    {
        public object New(Type type)
        {
            return new NavigationAgentSettings
            {
                Height = 1.0f,
                MaxClimb = 0.25f,
                MaxSlope = new AngleSingle(45.0f, AngleType.Degree),
                Radius = 0.5f
            };
        }
    }
}
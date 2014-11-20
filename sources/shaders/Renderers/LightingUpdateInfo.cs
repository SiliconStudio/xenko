// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Renderers
{
    public class LightingUpdateInfo
    {
        public int Index;
        public LightingUpdateType Type;
        public int Count;
        public LightParamSemantic Semantic;
        public ParameterKey<Vector3[]> PositionKey;
        public ParameterKey<Vector3[]> DirectionKey;
        public ParameterKey<Color3[]> ColorKey;
        public ParameterKey<float[]> IntensityKey;
        public ParameterKey<float[]> DecayKey;
        public ParameterKey<float[]> SpotBeamAngleKey;
        public ParameterKey<float[]> SpotFieldAngleKey;
        public ParameterKey<int> LightCountKey;

        public LightingUpdateInfo()
        {
            Index = -1;
            Type = LightingUpdateType.Directional;
            Count = 0;
            Semantic = 0;
            PositionKey = null;
            DirectionKey = null;
            ColorKey = null;
            IntensityKey = null;
            DecayKey = null;
            SpotBeamAngleKey = null;
            SpotFieldAngleKey = null;
            LightCountKey = null;
        }
    };
}
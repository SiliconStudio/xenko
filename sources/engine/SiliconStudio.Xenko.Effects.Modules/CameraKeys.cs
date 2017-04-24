// Copyright (c) 2011 Silicon Studio

using Xenko.Framework.Mathematics;

namespace Xenko.Effects.Modules
{
    /// <summary>
    /// Keys used by transformations.
    /// </summary>
    public static partial class CameraKeys
    {
        static CameraKeys()
        {
            ZProjection = ParameterKeys.Value(ParameterDynamicValue.New<Vector2, float, float>(NearClipPlane, FarClipPlane, ZProjectionACalculate));
        }

        /// <summary>
        /// Camera focus distance
        /// </summary>
        public static readonly ParameterValueKey<float> FocusDistance = ParameterKeys.Value(0.0f);

        private static void ZProjectionACalculate(ref float nearClipPlane, ref float farClipPlane, ref Vector2 output)
        {
            // Formuluas to retro project a non-linear zdepth in the range [0.0 - 1.0] to a linear-depth in view space.

            // (0) ZProjection: A + B / z = depth in the range [0.0-1.0]
            // (1) A + B / NearClipPlane = 0.0
            // (2) A + B / FarClipPlane = 1.0
            //
            // From (1): A = -B / NearClipPlane
            // From (2) and (1): B = (-FarClipPlane * NearClipPlane) / (FarClipPlane - NearClipPlane)
            //
            // From (0) z = B / (depth - A)
            output = new Vector2(farClipPlane / (farClipPlane - nearClipPlane), (-farClipPlane * nearClipPlane) / (farClipPlane - nearClipPlane));
        }
    }
}
// Copyright (c) 2011 Silicon Studio

using Xenko.Framework.Mathematics;

namespace Xenko.Effects.Modules
{
    /// <summary>
    /// Keys used by transformations.
    /// </summary>
    public static partial class TransformationKeys
    {
        static TransformationKeys()
        {
            View = ParameterKeys.Value(Matrix.Identity);
            Projection = ParameterKeys.Value(Matrix.Identity);
            World = ParameterKeys.Value(Matrix.Identity);
            ViewProjection = ParameterKeys.Value(ParameterDynamicValue.New<Matrix, Matrix, Matrix>(View, Projection, Matrix.Multiply));
            WorldView = ParameterKeys.Value(ParameterDynamicValue.New<Matrix, Matrix, Matrix>(World, View, Matrix.Multiply));
            WorldViewProjection = ParameterKeys.Value(ParameterDynamicValue.New<Matrix, Matrix, Matrix>(World, ViewProjection, Matrix.Multiply));
            ProjScreenRay = ParameterKeys.Value(ParameterDynamicValue.New<Vector2, Matrix>(Projection, ExtractProjScreenRay));
            Eye = ParameterKeys.Value(ParameterDynamicValue.New<Vector4, Matrix>(View, ViewToEye));
            ViewInverse = ParameterKeys.Value(ParameterDynamicValue.New<Matrix, Matrix>(View, InvertMatrix));
            WorldViewInverse = ParameterKeys.Value(ParameterDynamicValue.New<Matrix, Matrix>(WorldView, InvertMatrix));
        }

        /// <summary>
        /// Projection frustum planes.
        /// </summary>
        public static readonly ParameterArrayValueKey<Plane> ProjectionFrustumPlanes = ParameterKeys.ArrayValue(6,
            ParameterDynamicValue.New<Plane[], Matrix>(Projection, ExtractFrustumPlanes));

        /// <summary>
        /// Extracts the projected screem 2d vector from the projection matrix.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="projScreenRay">The proj screen ray.</param>
        private static void ExtractProjScreenRay(ref Matrix projection, ref Vector2 projScreenRay)
        {
            projScreenRay = new Vector2(-1.0f / projection.M11, 1.0f / projection.M22);
        }

        private static void InvertMatrix(ref Matrix inMatrix, ref Matrix outMatrix)
        {
            Matrix.Invert(ref inMatrix, out outMatrix);
        }

        /// <summary>
        /// Invert the view matrix and build an eye vector.
        /// </summary>
        /// <param name="view">The view matrix.</param>
        /// <param name="eye">The eye vector.</param>
        private static void ViewToEye(ref Matrix view, ref Vector4 eye)
        {
            Matrix inverseView;
            Matrix.Invert(ref view, out inverseView);
            eye = new Vector4(inverseView.M41, inverseView.M42, inverseView.M43, 1.0f);
        }

        /// <summary>
        /// Extracts the frustum planes from given matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="planes">The planes.</param>
        private static void ExtractFrustumPlanes(ref Matrix matrix, ref Plane[] planes)
        {
            // Left
            planes[0] = Plane.Normalize(new Plane(
                matrix.M14 + matrix.M11,
                matrix.M24 + matrix.M21,
                matrix.M34 + matrix.M31,
                matrix.M44 + matrix.M41));

            // Right
            planes[1] = Plane.Normalize(new Plane(
                matrix.M14 - matrix.M11,
                matrix.M24 - matrix.M21,
                matrix.M34 - matrix.M31,
                matrix.M44 - matrix.M41));

            // Top
            planes[2] = Plane.Normalize(new Plane(
                matrix.M14 - matrix.M12,
                matrix.M24 - matrix.M22,
                matrix.M34 - matrix.M32,
                matrix.M44 - matrix.M42));

            // Bottom
            planes[3] = Plane.Normalize(new Plane(
                matrix.M14 + matrix.M12,
                matrix.M24 + matrix.M22,
                matrix.M34 + matrix.M32,
                matrix.M44 + matrix.M42));

            // Near
            planes[4] = Plane.Normalize(new Plane(
                matrix.M13,
                matrix.M23,
                matrix.M33,
                matrix.M43));

            // Far
            planes[5] = Plane.Normalize(new Plane(
                matrix.M14 - matrix.M13,
                matrix.M24 - matrix.M23,
                matrix.M34 - matrix.M33,
                matrix.M44 - matrix.M43));
        }
    }
}
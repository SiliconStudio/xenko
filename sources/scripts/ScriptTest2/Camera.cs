// Copyright (c) 2011 ReShader - Alexandre Mutel

using System;

using SiliconStudio.Xenko.Games.Mathematics;

namespace ScriptTest2
{
    /// <summary>
    /// Camera mode.
    /// </summary>
    public enum CameraMode
    {
        Free,
        Target
    }

    /// <summary>
    /// Camera component.
    /// </summary>
    public class Camera
    {
        private float orthographicSize;
        private bool isOrthographic;
        private float nearClipPlane;
        private float farClipPlane;
        private float aspect;
        private float fieldOfView;
        private CameraMode mode;
        private Matrix projection;
        private Vector3 position;
        private Matrix worldToCamera;
        private Vector3 target;

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        public Camera()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="target">The target.</param>
        /// <param name="fov">The fov.</param>
        /// <param name="aspect">The aspect.</param>
        /// <param name="nearClipPlane">The near clip plane.</param>
        /// <param name="farClipPlane">The far clip plane.</param>
        public Camera(Vector3 position, Vector3 target, float fov, float width, float height, float aspect, float nearClipPlane, float farClipPlane)
        {
            this.orthographicSize = 1; // default value for orthographic
            this.mode = CameraMode.Target;
            this.position = position;
            this.target = target;
            this.fieldOfView = fov;
            this.Width = width;
            this.Height = height;
            this.aspect = aspect;
            this.nearClipPlane = nearClipPlane;
            this.farClipPlane = farClipPlane;
            UpdateWorldToCamera();
            UpdateProjection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="yaw">The yaw.</param>
        /// <param name="pitch">The pitch.</param>
        /// <param name="roll">The roll.</param>
        /// <param name="fov">The fov.</param>
        /// <param name="aspect">The aspect.</param>
        /// <param name="nearClipPlane">The near clip plane.</param>
        /// <param name="farClipPlane">The far clip plane.</param>
        public Camera(Vector3 position, float yaw, float pitch, float roll, float fov, float width, float height, float aspect, float nearClipPlane, float farClipPlane)
        {
            Matrix.RotationYawPitchRoll(yaw, pitch, roll, out worldToCamera);
            this.position = position;
            this.orthographicSize = 1; // default value for orthographic
            this.mode = CameraMode.Free;
            this.fieldOfView = fov;
            this.Width = width;
            this.Height = height;
            this.aspect = aspect;
            this.nearClipPlane = nearClipPlane;
            this.farClipPlane = farClipPlane;
            UpdateWorldToCamera();
            UpdateProjection();
        }

        /// <summary>
        /// Gets the width of the camera.
        /// </summary>
        public float Width { get; private set; }

        /// <summary>
        /// Gets the height of the camera.
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// Gets or sets the near clip plane.
        /// </summary>
        /// <value>
        /// The near clip plane.
        /// </value>
        public float NearClipPlane
        {
            get
            {
                return nearClipPlane;
            }
            set
            {
                nearClipPlane = value;
                UpdateProjection();
            }
        }

        /// <summary>
        /// Gets or sets the far clip plane.
        /// </summary>
        /// <value>
        /// The far clip plane.
        /// </value>
        public float FarClipPlane
        {
            get
            {
                return farClipPlane;
            }
            set
            {
                farClipPlane = value;
                UpdateProjection();
            }
        }

        /// <summary>
        /// Gets or sets the vertical field of view.
        /// </summary>
        /// <value>
        /// The field of view.
        /// </value>
        /// <remarks>
        /// The horizontal field of view is determined
        /// </remarks>
        public float FieldOfView
        {
            get
            {
                return fieldOfView;
            }
            set
            {
                fieldOfView = value;
                UpdateProjection();
            }
        }

        /// <summary>
        /// Gets or sets the aspect.
        /// </summary>
        /// <value>
        /// The aspect.
        /// </value>
        public float Aspect
        {
            get
            {
                return aspect;
            }
            set
            {
                aspect = value;
                UpdateProjection();
            }
        }

        /// <summary>
        /// Gets or sets the camera half vertical size in orthographic mode.
        /// </summary>
        /// <value>
        /// The camera half vertical size .
        /// </value>
        /// <remarks>
        /// This value is valid only when <see cref="IsOrthographic"/> is set to true.
        /// </remarks>
        public float OrthographicSize
        {
            get
            {
                return orthographicSize;
            }
            set
            {
                orthographicSize = value;
                UpdateProjection();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is orthographic.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is orthographic; otherwise, <c>false</c>.
        /// </value>
        public bool IsOrthographic
        {
            get
            {
                return isOrthographic;
            }
            set
            {
                isOrthographic = value;
                UpdateProjection();
            }
        }

        /// <summary>
        /// Gets or sets the mode of this camera.
        /// </summary>
        /// <value>
        /// The camera mode.
        /// </value>
        public CameraMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }

        /// <summary>
        /// Gets or sets the world to camera matrix.
        /// </summary>
        /// <value>
        /// The world to camera matrix.
        /// </value>
        public Matrix WorldToCamera
        {
            get
            {
                return worldToCamera;
            }
            set
            {
                // If WorldToCamera matrix is setup manually, then force to free camera mode
                mode = CameraMode.Free;

                worldToCamera = value;
            }
        }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        /// <value>
        /// The projection matrix.
        /// </value>
        public Matrix Projection
        {
            get
            {
                return projection;
            }
            set
            {
                projection = value;
            }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public Vector3 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
                UpdateWorldToCamera();
            }
        }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        /// <remarks>>Only available for camera mode <see cref="CameraMode.Target"/></remarks>
        public Vector3 Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
                UpdateWorldToCamera();
            }
        }

        public override string ToString()
        {
            return string.Format("Mode: {0}, NearClipPlane: {1}, FarClipPlane: {2}, FieldOfView: {3}, Aspect: {4}", mode, nearClipPlane, farClipPlane, fieldOfView, aspect);
        }

        public static Matrix YawPitchRoll(Vector3 position, Matrix matrix, float yaw, float pitch, float roll)
        {
            var tempMatrix = Matrix.Identity;
            var rotateZ = Matrix.RotationZ(yaw) * Matrix.RotationX(roll);
            tempMatrix.Column1 = Vector3.Transform((Vector3)matrix.Column1, rotateZ);
            tempMatrix.Column3 = Vector3.Transform((Vector3)matrix.Column3, rotateZ);
            tempMatrix.Column2 = (Vector4)Vector3.Cross((Vector3)tempMatrix.Column3, (Vector3)tempMatrix.Column1);
            tempMatrix.M41 = 0;
            tempMatrix.M42 = 0;
            tempMatrix.M43 = 0;
            return Matrix.Translation(-position) * tempMatrix * Matrix.RotationX(pitch);
        }

        private void UpdateWorldToCamera()
        {
            if (mode == CameraMode.Target)
            {
                worldToCamera = Matrix.LookAtLH(Position, Target, Vector3.UnitZ);
            } 
            else
            {
                var defaultCamera = worldToCamera;
                defaultCamera.M41 = 0;
                defaultCamera.M42 = 0;
                defaultCamera.M43 = 0;
                worldToCamera = Matrix.Translation(-Position) * defaultCamera;
            }
        }

        private void UpdateProjection()
        {
            if (IsOrthographic)
            {
                Matrix.OrthoLH(OrthographicSize * 2 * Aspect, OrthographicSize * 2, NearClipPlane, FarClipPlane, out projection);
            }
            else
            {
                Matrix.PerspectiveFovLH(FieldOfView, Aspect, NearClipPlane, FarClipPlane, out projection);
            }
        }
    }
}

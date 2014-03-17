using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Pong
{
    public class Camera
    {
        private struct GlobalMatricies
        {
            public Matrix4 ViewMatrix;
            public Matrix4 PerspectiveMatrix;

            public IntPtr Size { get { return (IntPtr)(sizeof(float) * 16 * 2); } }
        }

        public struct CameraInfo
        {
            public Vector3 Pos;
            public Vector3 Target;
            public Vector3 Up;

            public CameraInfo(Vector3 CameraPosition, Vector3 CameraTarget, Vector3 UpVector)
            {
                Pos = CameraPosition;
                Target = CameraTarget;
                Up = UpVector;
            }
        }

        private GlobalMatricies cameraMatricies;
        private int globalBindingIndex = 0;
        private int globalMatrixUBO = -1;
        private CameraInfo info;

        public Camera(float Width, float Height, float zNear, float zFar, CameraInfo CameraInformation)
        {
            info = CameraInformation;

            //Set the Perspective and View Matricies using the floats and info passed to the constructor
            cameraMatricies.PerspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (Width / Height), zNear, zFar);
            cameraMatricies.ViewMatrix = Matrix4.LookAt(CameraInformation.Pos, CameraInformation.Target, CameraInformation.Up);

            GL.GenBuffers(1, out globalMatrixUBO); //Generate a new Buffer Object
            GL.BindBuffer(BufferTarget.UniformBuffer, globalMatrixUBO);
            GL.BufferData(BufferTarget.UniformBuffer, cameraMatricies.Size, ref cameraMatricies, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            GL.BindBufferRange(BufferRangeTarget.UniformBuffer, globalBindingIndex, globalMatrixUBO, IntPtr.Zero, cameraMatricies.Size);
        }

        private void SetView()
        {
            cameraMatricies.ViewMatrix = Matrix4.LookAt(info.Pos, info.Target, info.Up);
            Console.WriteLine(cameraMatricies.ViewMatrix.ToString());
            GL.BindBuffer(BufferTarget.UniformBuffer, globalMatrixUBO);
            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, cameraMatricies.Size, ref cameraMatricies);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void Zoom(float distance = 1.0f) { info.Pos.Z += distance; SetView(); }
        public void Pan(Vector2 Vec) { info.Pos.X += Vec.X; info.Pos.Y += Vec.Y; SetView(); }


    }
}

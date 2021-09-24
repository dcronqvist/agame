using System;
using System.Numerics;

namespace AGame.Engine.Graphics.Cameras
{
    public class Camera2D
    {
        public Vector2 FocusPosition { get; set; }
        public float Zoom { get; set; }

        public Camera2D(Vector2 focusPosition, float zoom)
        {
            FocusPosition = focusPosition;
            Zoom = zoom;
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return GetProjectionMatrix(DisplayManager.GetWindowSizeInPixels());
        }

        public Matrix4x4 GetProjectionMatrix(Vector2 viewSize)
        {
            Vector2 windowSize = viewSize;

            float left = FocusPosition.X - windowSize.X / 2f;
            float right = FocusPosition.X + windowSize.X / 2f;
            float bottom = FocusPosition.Y + windowSize.Y / 2f;
            float top = FocusPosition.Y - windowSize.Y / 2f;

            Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.01f, 100);
            Matrix4x4 zoomMatrix = Matrix4x4.CreateScale(Zoom);

            return orthoMatrix * zoomMatrix;
        }
    }
}
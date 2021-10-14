using System.Numerics;

namespace AGame.Engine.Graphics
{
    public interface IRenderable
    {
        public Vector2 Position { get; set; }
        public Vector2 BasePosition { get; set; }

        public void Render();
    }
}
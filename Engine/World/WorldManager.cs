using System.Drawing;
using System.Numerics;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World.Entities;

namespace AGame.Engine.World
{
    public static class WorldManager
    {
        public static Crater CurrentCrater { get; set; }
        public static Camera2D PlayerCamera { get; set; }
        public static Player Player { get; set; }

        public static void Init()
        {
            Utilities.InitRNG();
            CurrentCrater = new Crater(Utilities.GetRandomInt(0, 100), new TestingGenerator());
            PlayerCamera = new Camera2D(Vector2.Zero, 1f);
            Player = new Player(CurrentCrater.AbsoluteMiddle());
        }

        public static void Update()
        {
            CurrentCrater.Update();
            Player.Update(CurrentCrater);
            DisplayManager.SetWindowTitle($"Player: {Player.Position.ToString()}");

            PlayerCamera.FocusPosition = Player.MiddleOfSpritePosition;
        }

        public static void Render()
        {
            Renderer.SetRenderTarget(null, PlayerCamera);
            Renderer.Clear(ColorF.Black);
            RenderTexture crt = CurrentCrater.Render(PlayerCamera);
            Renderer.RenderRenderTexture(crt);
            Renderer.SetRenderTarget(null, PlayerCamera);
            Player.Render(CurrentCrater);
        }
    }
}
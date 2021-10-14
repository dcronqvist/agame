using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AGame.Engine.Assets;
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

        static DynamicInstancedTextureRenderer ditr;

        public static void Init()
        {
            Utilities.InitRNG();
            //CurrentCrater = new Crater(Utilities.GetRandomInt(0, 100), new TestingGenerator());
            CurrentCrater = new Crater();
            PlayerCamera = new Camera2D(Vector2.Zero, 2f);
            Player = new Player(new Vector2(100, 100));
        }

        public static void Update()
        {
            //CurrentCrater.Update();

            if (Input.IsMouseButtonPressed(GLFW.MouseButton.Left))
            {
                int x = CurrentCrater.grid.GetTileXFromPosition(Input.GetMousePosition(PlayerCamera));
                int y = CurrentCrater.grid.GetTileYFromPosition(Input.GetMousePosition(PlayerCamera));

                CurrentCrater.grid.UpdateTile(x, y, 4);
            }
            if (Input.IsMouseButtonPressed(GLFW.MouseButton.Right))
            {
                int x = CurrentCrater.grid.GetTileXFromPosition(Input.GetMousePosition(PlayerCamera));
                int y = CurrentCrater.grid.GetTileYFromPosition(Input.GetMousePosition(PlayerCamera));

                CurrentCrater.grid.UpdateTile(x, y, 0);
            }

            Player.Update(CurrentCrater);
            DisplayManager.SetWindowTitle($"Player: {Player.Position.ToString()}");
            PlayerCamera.FocusPosition = Player.MiddleOfSpritePosition;
        }

        public static void Render()
        {
            Renderer.SetRenderTarget(null, PlayerCamera);
            Renderer.Clear(ColorF.Black);

            CurrentCrater.Render();

            List<IRenderable> craterRenderables = CurrentCrater.GetRenderables().ToList();
            craterRenderables.Add(Player.GetRenderable());

            craterRenderables.Sort((a, b) =>
            {
                if (a.BasePosition.Y > b.BasePosition.Y)
                {
                    return 1;
                }
                else if (a.BasePosition.Y == b.BasePosition.Y)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            });

            foreach (IRenderable ir in craterRenderables)
            {
                ir.Render();
            }

            int x = CurrentCrater.grid.GetTileXFromPosition(Input.GetMousePosition(PlayerCamera));
            int y = CurrentCrater.grid.GetTileYFromPosition(Input.GetMousePosition(PlayerCamera));

            Renderer.Primitive.RenderRectangle(new RectangleF(x * TileGrid.TILE_SIZE, y * TileGrid.TILE_SIZE, TileGrid.TILE_SIZE, TileGrid.TILE_SIZE), ColorF.BlueGray * 0.2f);
        }
    }
}
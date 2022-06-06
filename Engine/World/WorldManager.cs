using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.World
{
    public static class WorldManager
    {
        public static Crater CurrentCrater { get; set; }
        public static Camera2D PlayerCamera { get; set; }

        public static void Init()
        {
            Utilities.InitRNG();
            //CurrentCrater = new Crater(Utilities.GetRandomInt(0, 100), new TestingGenerator());
            CurrentCrater = new Crater(100, 100);
            PlayerCamera = new Camera2D(Vector2.Zero, 2f);
        }

        public static void Update()
        {
            //CurrentCrater.Update();
        }

        public static void Render()
        {
            Renderer.SetRenderTarget(null, PlayerCamera);
            Renderer.Clear(ColorF.Black);

            CurrentCrater.Render();

            List<IRenderable> craterRenderables = CurrentCrater.GetRenderables().ToList();

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
        }
    }
}
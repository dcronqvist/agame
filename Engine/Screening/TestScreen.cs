using System;
using System.Drawing;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.Screening;
using AGame.Engine.World;

namespace AGame.Engine.Screening
{
    class TestScreen : Screen
    {
        GameServer server;
        GameClient client;

        bool init = false;

        public TestScreen() : base("testscreen")
        {

        }

        public override Screen Initialize()
        {
            return this;
        }

        public override async void OnEnter(string[] args)
        {
            DisplayManager.SetTargetFPS(144);

            if (!init)
            {
                server = new GameServer(int.Parse(args[0]));
                client = new GameClient("127.0.0.1", int.Parse(args[0]));
                await server.StartAsync();

                Console.WriteLine("Sending connect request");
                ConnectResponse response = await client.ConnectAsync(new ConnectRequest());

                if (!(response is null || !response.Accepted))
                {
                    client.EnqueuePacket(new ConnectReadyForECS(), false, false);
                }

                init = true;
            }
        }

        public override async void OnLeave()
        {
            await this.client.DisconnectAsync(1000);
            await this.server.StopAsync(1000);
        }

        public override void Update()
        {
            this.server.Update();
            this.client.Update();
        }

        public override void Render()
        {
            client.Render();
        }
    }
}
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

        public TestScreen() : base("testscreen")
        {

        }

        public override Screen Initialize()
        {
            server = new GameServer(28000);
            client = new GameClient("127.0.0.1", 28000);

            return this;
        }

        public override async void OnEnter(string[] args)
        {
            await server.StartAsync();

            // ConnectResponse response = await client.ConnectAsync(new ConnectRequest());

            // if (!(response is null || !response.Accepted))
            // {
            //     client.EnqueuePacket(new ConnectReadyForECS(), false, false);
            // }
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
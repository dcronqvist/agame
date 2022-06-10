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
    class RemoteScreen : Screen
    {
        GameClient client;

        public RemoteScreen() : base("remotescreen")
        {

        }

        public override Screen Initialize()
        {
            return this;
        }

        public override async void OnEnter(string[] args)
        {
            client = new GameClient(args[0], 28000);
            ConnectResponse response = await client.ConnectAsync(new ConnectRequest());

            if (!(response is null || !response.Accepted))
            {
                client.EnqueuePacket(new ConnectReadyForMap(), false, false);
            }
            else
            {
                // Wait 5 seconds and try again
                await Task.Delay(5000);

                this.OnEnter(args);
            }
        }

        public override async void OnLeave()
        {
            await this.client.DisconnectAsync(1000);
        }

        public override void Update()
        {
            client.Update();
        }

        public override void Render()
        {
            client.Render();
        }
    }
}
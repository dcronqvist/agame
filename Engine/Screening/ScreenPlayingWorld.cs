using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Screening;

public class EnterPlayingWorldArgs : ScreenEnterArgs
{
    public GameServer Server { get; set; } = null;
    public GameClient Client { get; set; } = null;
    public int PlayerEntityID { get; set; } = -1;
}

public class ScreenPlayingWorld : Screen<EnterPlayingWorldArgs>
{
    private bool _disconnected = false;
    private GameServer _server;
    private GameClient _client;
    public Camera2D Camera { get; set; }
    private Vector2 _cameraTargetPosition;

    List<Packet> _receivedPackets = new List<Packet>();

    bool _paused = false;
    bool _showingInventory = false;

    public override void Initialize()
    {

    }

    private void OnFramebufferResize(object sender, Vector2 newSize)
    {
        // Make sure that zoom always stays the same, no matter the window size
        float targetZoom = 2f; // On full hd, the window size is 1920x1080, so the zoom is 2.0f

        Vector2 targetSize = new Vector2(1920, 1080);
        Vector2 factor = newSize / targetSize;

        Camera.Zoom = targetZoom * factor.X;
    }

    public override void OnEnter(EnterPlayingWorldArgs args)
    {
        _paused = false;
        Camera = new Camera2D(Vector2.Zero, 2f);

        DisplayManager.OnFramebufferResize += OnFramebufferResize;
        this.OnFramebufferResize(null, DisplayManager.GetWindowSizeInPixels());

        _server = args.Server;
        _client = args.Client;

        _client.ServerDisconnectedClient += (sender, e) =>
        {
            Task.Run(async () =>
            {
                ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Disconnected from server..." });
                await Task.Delay(1000);
                ScreenManager.GoToScreen<ScreenMainMenu, EnterMainMenuArgs>(new EnterMainMenuArgs());
            });
        };
    }

    public override async void OnLeave()
    {
        if (!this._disconnected)
        {
            DisplayManager.OnFramebufferResize -= OnFramebufferResize;
            await this.ExitWorld();
        }
    }

    public void SetCameraPosition(CoordinateVector position, bool snap = false)
    {
        if (snap)
        {
            this.Camera.FocusPosition = position.ToWorldVector().ToVector2();
            this._cameraTargetPosition = position.ToWorldVector().ToVector2();
        }
        else
        {
            this._cameraTargetPosition = position.ToWorldVector().ToVector2();
        }
    }

    public override void Render()
    {
        this.Camera.FocusPosition += (this._cameraTargetPosition - this.Camera.FocusPosition) * GameTime.DeltaTime * 7f;

        Renderer.SetRenderTarget(null, this.Camera);
        Renderer.Clear(ColorF.Black);
        _client.Render();

        if (_paused)
        {
            GUI.Begin();

            // Render pause screen
            Renderer.SetRenderTarget(null, null);
            Renderer.Primitive.RenderRectangle(new RectangleF(0, 0, DisplayManager.GetWindowSizeInPixels().X, DisplayManager.GetWindowSizeInPixels().Y), ColorF.Black * 0.6f);

            Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2;

            if (GUI.Button("Back to game", new Vector2(middleOfScreen.X - 150, middleOfScreen.Y - 50), new Vector2(300, 50f)))
            {
                _paused = false;
            }
            if (GUI.Button("Exit to menu", new Vector2(middleOfScreen.X - 150, middleOfScreen.Y + 10f), new Vector2(300, 50f)))
            {
                _ = this.ExitWorld();
            }

            GUI.End();
        }

        Renderer.SetRenderTarget(null, null);

        Font f = ModManager.GetAsset<Font>("default.font.rainyhearts");
        Renderer.Text.RenderText(f, $"Ping: {this._client.GetPing()}ms", new Vector2(20, 20), 1f, ColorF.White, Renderer.Camera);
        Renderer.Text.RenderText(f, $"Entities: {this._client.GetAmountOfEntities()}", new Vector2(20, 40), 1f, ColorF.White, Renderer.Camera);

        if (this._client.GetPlayerEntity() != null)
        {
            Entity localPlayer = this._client.GetPlayerEntity();
            int remotePlayerID = this._client.GetRemoteIDForEntity(localPlayer.ID);

            var position = localPlayer.GetComponent<TransformComponent>().Position;
            Renderer.Text.RenderText(f, $"X: {MathF.Round(position.X, 1)} Y: {MathF.Round(position.Y, 1)}", new Vector2(150, 80), 1f, ColorF.White, Renderer.Camera);

            var animator = localPlayer.GetComponent<AnimatorComponent>();
            var offset = animator.GetAnimator().GetCurrentAnimation().GetMiddleOfCurrentFrameScaled();

            this.SetCameraPosition(position + new CoordinateVector(offset.X / TileGrid.TILE_SIZE, offset.Y / TileGrid.TILE_SIZE), false);
            this.RenderPlayerHotbar();
        }

        if (_showingInventory)
        {
            this.RenderPlayerInventory();
        }
    }

    public override void Update()
    {
        // Set audio's listener's position to player's position
        //Audio.SetListenerPosition(this._client.GetPlayerEntity().GetComponent<TransformComponent>().Position);

        //this._server?.Update();
        Vector2 pos = Input.GetMousePosition(this.Camera);
        WorldVector wv = new WorldVector(pos.X, pos.Y);
        this._client.Update(!this._paused && !this._showingInventory, wv.ToCoordinateVector().ToTileAligned());

        if (Input.IsKeyPressed(GLFW.Keys.Escape))
        {
            this._paused = !this._paused;
        }

        if (Input.IsKeyPressed(GLFW.Keys.E))
        {
            if (!this._showingInventory)
            {
                this._client.RequestPlayerInventory();
                this._showingInventory = true;
            }
            else
            {
                this._showingInventory = false;
            }
        }
    }

    public void RenderPlayerInventory()
    {
        Entity player = this._client.GetPlayerEntity();
        Inventory inventory = player.GetComponent<InventoryComponent>().GetInventory();

        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f;
        Vector2 inventorySize = inventory.GetRenderSize();

        inventory.Render(middleOfScreen - inventorySize / 2f);
    }

    public void RenderPlayerHotbar()
    {
        Entity player = this._client.GetPlayerEntity();
        HotbarComponent hotbar = player.GetComponent<HotbarComponent>();
        Inventory inventory = player.GetComponent<InventoryComponent>().GetInventory();

        float inventoryWidth = inventory.GetRenderSize().X;
        Vector2 middleOfScreen = new Vector2(DisplayManager.GetWindowSizeInPixels().X / 2f - inventoryWidth / 2f, DisplayManager.GetWindowSizeInPixels().Y - 74);

        hotbar.Render(middleOfScreen, inventory);
    }

    public async Task ExitWorld()
    {
        await Task.Run(async () =>
        {
            ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Closing world..." });
            await this._client.DisconnectAsync(1000);
            this._disconnected = true;

            await Task.Delay(1000);

            if (this._server is not null)
            {
                await this._server.StopAsync(1000);

                // Save the world
                this._server.SaveServer();
            }

            ScreenManager.GoToScreen<ScreenMainMenu, EnterMainMenuArgs>(new EnterMainMenuArgs());
        });
    }
}
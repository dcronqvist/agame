using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.DebugTools;
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

    bool _paused = false;
    bool _showingInventory = false;
    bool _inConsole = false;
    ContainerInteractionGUI _currentContainerInteraction;

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

        _currentContainerInteraction = null;

        GameConsole.InitializeCommands(this._client);
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

        GUI.Begin();
        if (_paused)
        {

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
            //Renderer.Text.RenderText(f, $"X: {MathF.Round(position.X, 1)} Y: {MathF.Round(position.Y, 1)}", new Vector2(150, 80), 1f, ColorF.White, Renderer.Camera);

            var animator = localPlayer.GetComponent<AnimatorComponent>();
            var offset = animator.GetAnimator().GetCurrentAnimation().GetMiddleOfCurrentFrameScaled();

            if (this._currentContainerInteraction is null)
                this.RenderHotbar(this._client.GetPlayerEntity());

            this.SetCameraPosition(position + new CoordinateVector(offset.X / TileGrid.TILE_SIZE, offset.Y / TileGrid.TILE_SIZE), false);
        }

        if (_currentContainerInteraction is not null)
        {
            // Render large black overlay
            var displayRect = Renderer.Camera.VisibleArea;
            Renderer.Primitive.RenderRectangle(displayRect, ColorF.Black * 0.3f);
        }
        _currentContainerInteraction?.Render(0f);

        if (this._inConsole)
        {
            var rt = GameConsole.Render(ModManager.GetAsset<Font>("default.font.rainyhearts"));
            Renderer.RenderRenderTexture(rt);
        }
        GUI.End();

    }

    private void RenderHotbar(Entity playerEntity)
    {
        var container = playerEntity.GetComponent<ContainerComponent>();
        var hotbar = playerEntity.GetComponent<HotbarComponent>();

        var slots = container.GetContainer().GetSlots(hotbar.ContainerSlots.ToArray()).ToList();

        var width = slots.Count * (ContainerSlot.WIDTH + 5);
        var middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f;

        var bottomMiddle = new Vector2(middleOfScreen.X, DisplayManager.GetWindowSizeInPixels().Y);
        var hotbarTopLeft = new Vector2(bottomMiddle.X - width / 2f, bottomMiddle.Y - ContainerSlot.HEIGHT - 10);

        var selectedSlot = hotbar.SelectedSlot;

        var font = ModManager.GetAsset<Font>("default.font.rainyhearts");

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];

            var color = ColorF.Black;

            var position = new Vector2(hotbarTopLeft.X + i * (ContainerSlot.WIDTH + 5) + 5, hotbarTopLeft.Y + 5);

            if (i == selectedSlot)
            {
                Renderer.Primitive.RenderRectangle(new RectangleF(hotbarTopLeft.X + i * (ContainerSlot.WIDTH + 5), hotbarTopLeft.Y, ContainerSlot.WIDTH + 10, ContainerSlot.HEIGHT + 10), ColorF.LightGray * 0.8f);
            }

            Renderer.Primitive.RenderRectangle(new RectangleF(hotbarTopLeft.X + i * (ContainerSlot.WIDTH + 5) + 5, hotbarTopLeft.Y + 5, ContainerSlot.WIDTH, ContainerSlot.HEIGHT), color * 0.6f);

            slot.Item?.RenderInSlot(position);
            var size = slot.GetSize();

            // Render count
            if (slot.Item is not null)
            {
                float scale = 1f;
                var text = slot.Count.ToString();
                var textSize = font.MeasureString(text, scale);
                var textPosition = position + new Vector2(size.X - textSize.X, size.Y - textSize.Y);
                Renderer.Text.RenderText(font, text, textPosition.PixelAlign(), scale, ColorF.White, Renderer.Camera);

                //If has tool component, render durability
                if (slot.Item.TryGetComponent<DefaultMod.Tool>(out DefaultMod.Tool t))
                {
                    var durability = t.Definition.Durability;
                    var currDur = t.CurrentDurability;
                    var perc = ((float)currDur / durability).ToString("0.00");

                    var durabilitySize = font.MeasureString(perc, scale);
                    var durabilityPosition = position + new Vector2(size.X - durabilitySize.X, size.Y - durabilitySize.Y - textSize.Y);
                    Renderer.Text.RenderText(font, perc, durabilityPosition.PixelAlign(), scale, ColorF.White, Renderer.Camera);
                }
            }
        }
    }

    public override void Update()
    {
        // Set audio's listener's position to player's position
        //Audio.SetListenerPosition(this._client.GetPlayerEntity().GetComponent<TransformComponent>().Position);

        //this._server?.Update();
        Vector2 pos = Input.GetMousePosition(this.Camera);
        WorldVector wv = new WorldVector(pos.X, pos.Y);
        this._client.Update(!this._paused && this._currentContainerInteraction == null && !this._inConsole, wv.ToCoordinateVector().ToTileAligned());

        if (this._inConsole)
        {
            GameConsole.SetEnabled(true);
            GameConsole.Update(this._client.GetPlayerEntity(), this._client.GetECS(), this._client);

            if (Input.IsKeyPressed(GLFW.Keys.Escape))
            {
                this._inConsole = false;
            }
        }
        else
        {
            GameConsole.SetEnabled(false);

            if (Input.IsKeyPressed(GLFW.Keys.Escape))
            {
                this._paused = !this._paused;
            }

            if (Input.IsKeyPressed(GLFW.Keys.Enter))
            {
                this._inConsole = true;
            }

            if (Input.IsKeyPressed(GLFW.Keys.E))
            {
                if (this._currentContainerInteraction == null)
                {
                    var playerContainer = this._client.GetPlayerEntity().GetComponent<ContainerComponent>();
                    this._currentContainerInteraction = new ContainerInteractionGUI(this._client.GetPlayerEntity(), null);
                }
                else
                {
                    this._currentContainerInteraction = null;
                }
            }

            if (this._client.ReceivedEntityOpenContainer != -1)
            {
                this._currentContainerInteraction = new ContainerInteractionGUI(this._client.GetPlayerEntity(), this._client.GetECS().GetEntityFromID(this._client.ReceivedEntityOpenContainer));
                this._client.ReceivedEntityOpenContainer = -1;
            }

            this._currentContainerInteraction?.UpdateInteract(this._client, 0f);
        }
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
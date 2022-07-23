using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;

namespace AGame.Engine.Items;

public class ContainerInteractionGUI
{
    private Entity _playerEntity;
    private Entity _otherEntity;

    private ContainerSlot _mouseSlot;

    public ContainerInteractionGUI(Entity playerEntity, Entity otherEntity)
    {
        this._playerEntity = playerEntity;
        this._otherEntity = otherEntity;
        this._mouseSlot = new ContainerSlot(Vector2.Zero);
    }

    public Vector2 GetTotalSize()
    {
        var otherContainer = this._otherEntity.GetComponent<ContainerComponent>();
        var playerContainer = this._playerEntity.GetComponent<ContainerComponent>();

        var otherSize = otherContainer.GetContainer().Provider.GetRenderSize();
        var playerSize = playerContainer.GetContainer().Provider.GetRenderSize();

        // Stacking containers above each other
        var totalHeight = otherSize.Y + playerSize.Y;
        var totalWidth = Math.Max(otherSize.X, playerSize.X);

        return new Vector2(totalWidth, totalHeight);
    }

    public Vector2 MiddleOfWindow()
    {
        var windowSize = DisplayManager.GetWindowSizeInPixels();
        return new Vector2(windowSize.X / 2, windowSize.Y / 2);
    }

    public void UpdateInteract(GameClient client, float deltaTime)
    {
        var playerState = this._playerEntity.GetComponent<PlayerStateComponent>();
        _mouseSlot.Item = playerState.ItemOnMouse;
        _mouseSlot.Count = playerState.ItemOnMouseCount;

        var middleOfWindow = this.MiddleOfWindow();
        var totalSize = this.GetTotalSize();

        // Render other container
        var otherContainer = this._otherEntity.GetComponent<ContainerComponent>();
        var otherSize = otherContainer.GetContainer().Provider.GetRenderSize();

        var otherContainerStartPos = new Vector2(middleOfWindow.X - otherSize.X / 2f, middleOfWindow.Y - totalSize.Y / 2f);
        otherContainer.GetContainer().UpdateInteract(ref _mouseSlot, _otherEntity.ID, client, otherContainerStartPos, deltaTime);

        // Render player container
        var playerContainer = this._playerEntity.GetComponent<ContainerComponent>();
        var playerSize = playerContainer.GetContainer().Provider.GetRenderSize();

        var playerContainerStartPos = new Vector2(middleOfWindow.X - playerSize.X / 2f, otherContainerStartPos.Y + otherSize.Y);
        playerContainer.GetContainer().UpdateInteract(ref _mouseSlot, _playerEntity.ID, client, playerContainerStartPos, deltaTime);

        playerState.ItemOnMouse = _mouseSlot.Item;
        playerState.ItemOnMouseCount = _mouseSlot.Count;
    }

    public void Render(float deltaTime)
    {
        var middleOfWindow = this.MiddleOfWindow();
        var totalSize = this.GetTotalSize();
        var topLeft = middleOfWindow - totalSize / 2f;

        Renderer.Primitive.RenderRectangle(new RectangleF(topLeft.X, topLeft.Y, totalSize.X, totalSize.Y), ColorF.Green * 0.2f);

        // Render other container
        var otherContainer = this._otherEntity.GetComponent<ContainerComponent>();
        var otherSize = otherContainer.GetContainer().Provider.GetRenderSize();

        var otherContainerStartPos = new Vector2(middleOfWindow.X - otherSize.X / 2f, middleOfWindow.Y - totalSize.Y / 2f);
        otherContainer.GetContainer().Render(otherContainerStartPos);

        // Render player container
        var playerContainer = this._playerEntity.GetComponent<ContainerComponent>();
        var playerSize = playerContainer.GetContainer().Provider.GetRenderSize();

        var playerContainerStartPos = new Vector2(middleOfWindow.X - playerSize.X / 2f, otherContainerStartPos.Y + otherSize.Y);
        playerContainer.GetContainer().Render(playerContainerStartPos);

        var mousePos = Input.GetMousePositionInWindow();

        var playerState = this._playerEntity.GetComponent<PlayerStateComponent>();
        _mouseSlot.Item = playerState.ItemOnMouse;
        _mouseSlot.Count = playerState.ItemOnMouseCount;

        if (_mouseSlot.Item != "")
        {
            var item = ItemManager.GetItem(_mouseSlot.Item);
            Renderer.Texture.Render(item.Texture, mousePos, Vector2.One * 2f, 0f, ColorF.White);
        }
    }
}
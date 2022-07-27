using System;
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

    private float _interactionTitleHeight = 50f;

    public ContainerInteractionGUI(Entity playerEntity, Entity otherEntity)
    {
        this._playerEntity = playerEntity;
        this._otherEntity = otherEntity;
        this._mouseSlot = new ContainerSlot(Vector2.Zero);
    }

    public Vector2 GetTotalSize(bool showPlayerContainer)
    {
        if (_otherEntity is null)
        {
            var playerContainer = this._playerEntity.GetComponent<ContainerComponent>();

            var playerSize = playerContainer.GetContainer().Provider.GetRenderSize();

            // Stacking containers above each other
            return playerSize + new Vector2(0, this._interactionTitleHeight);
        }
        else
        {
            var otherContainer = this._otherEntity.GetComponent<ContainerComponent>();
            var playerContainer = this._playerEntity.GetComponent<ContainerComponent>();

            var otherSize = otherContainer.GetContainer().Provider.GetRenderSize();
            var playerSize = playerContainer.GetContainer().Provider.GetRenderSize();

            // Stacking containers above each other
            var totalHeight = otherSize.Y + (showPlayerContainer ? playerSize.Y : 0);
            var totalWidth = Math.Max(otherSize.X, (showPlayerContainer ? playerSize.X : 0));

            return new Vector2(totalWidth, totalHeight) + new Vector2(0, this._interactionTitleHeight);
        }
    }

    public Vector2 MiddleOfWindow()
    {
        var windowSize = DisplayManager.GetWindowSizeInPixels();
        return new Vector2(windowSize.X / 2, windowSize.Y / 2);
    }

    private void RenderTitle(string title, Vector2 position)
    {
        float titleScale = 2f;
        Font titleFont = ModManager.GetAsset<Font>("default.font.rainyhearts");

        Vector2 measure = titleFont.MeasureString(title, titleScale);

        var offset = new Vector2(this._interactionTitleHeight / 2f - measure.Y / 2f, this._interactionTitleHeight / 2f - measure.Y / 2f);

        Renderer.Text.RenderText(titleFont, title, position + offset + Vector2.One * 2f, titleScale, ColorF.Black, Renderer.Camera);
        Renderer.Text.RenderText(titleFont, title, position + offset, titleScale, ColorF.White, Renderer.Camera);
    }

    public void UpdateInteract(GameClient client, float deltaTime)
    {
        if (this._otherEntity is null)
        {
            var playerState = this._playerEntity.GetComponent<PlayerStateComponent>();
            _mouseSlot.Item = playerState.MouseSlot.Item.Instance;
            _mouseSlot.Count = playerState.MouseSlot.ItemCount;

            var middleOfWindow = this.MiddleOfWindow();
            var totalSize = this.GetTotalSize(true);

            var playerContainer = this._playerEntity.GetComponent<ContainerComponent>();
            var playerSize = playerContainer.GetContainer().Provider.GetRenderSize();

            var playerContainerStartPos = new Vector2(middleOfWindow.X - playerSize.X / 2f, middleOfWindow.Y - playerSize.Y / 2f + this._interactionTitleHeight / 2f);
            playerContainer.GetContainer().UpdateInteract(ref _mouseSlot, _playerEntity.ID, client, playerContainerStartPos, deltaTime);

            playerState.MouseSlot = _mouseSlot.ToSlotInfo(0);
        }
        else
        {
            // Render other container
            var otherContainer = this._otherEntity.GetComponent<ContainerComponent>();
            var otherSize = otherContainer.GetContainer().Provider.GetRenderSize();

            if (otherContainer.GetContainer().Provider.ShowPlayerContainer)
            {
                var playerState = this._playerEntity.GetComponent<PlayerStateComponent>();
                _mouseSlot.Item = playerState.MouseSlot.Item.Instance;
                _mouseSlot.Count = playerState.MouseSlot.ItemCount;

                var middleOfWindow = this.MiddleOfWindow();
                var totalSize = this.GetTotalSize(true);

                var otherContainerStartPos = new Vector2(middleOfWindow.X - otherSize.X / 2f, middleOfWindow.Y - totalSize.Y / 2f + this._interactionTitleHeight);
                otherContainer.GetContainer().UpdateInteract(ref _mouseSlot, _otherEntity.ID, client, otherContainerStartPos, deltaTime);

                // Render player container
                var playerContainer = this._playerEntity.GetComponent<ContainerComponent>();
                var playerSize = playerContainer.GetContainer().Provider.GetRenderSize();

                var playerContainerStartPos = new Vector2(middleOfWindow.X - playerSize.X / 2f, otherContainerStartPos.Y + otherSize.Y);
                playerContainer.GetContainer().UpdateInteract(ref _mouseSlot, _playerEntity.ID, client, playerContainerStartPos, deltaTime);

                playerState.MouseSlot = _mouseSlot.ToSlotInfo(0);
            }
            else
            {
                var playerState = this._playerEntity.GetComponent<PlayerStateComponent>();
                _mouseSlot.Item = playerState.MouseSlot.Item.Instance;
                _mouseSlot.Count = playerState.MouseSlot.ItemCount;

                var middleOfWindow = this.MiddleOfWindow();
                var totalSize = this.GetTotalSize(false);

                var otherContainerStartPos = new Vector2(middleOfWindow.X - otherSize.X / 2f, middleOfWindow.Y - totalSize.Y / 2f + this._interactionTitleHeight);
                otherContainer.GetContainer().UpdateInteract(ref _mouseSlot, _otherEntity.ID, client, otherContainerStartPos, deltaTime);

                playerState.MouseSlot = _mouseSlot.ToSlotInfo(0);
            }
        }
    }

    public void Render(float deltaTime)
    {
        if (this._otherEntity is null)
        {
            var middleOfWindow = this.MiddleOfWindow();
            var totalSize = this.GetTotalSize(true);
            var topLeft = middleOfWindow - totalSize / 2f;

            Container.RenderBackground(topLeft, totalSize);

            // Render player container
            var playerContainer = this._playerEntity.GetComponent<ContainerComponent>();
            var playerSize = playerContainer.GetContainer().Provider.GetRenderSize();

            this.RenderTitle(playerContainer.GetContainer().Provider.Name, topLeft);

            var playerContainerStartPos = new Vector2(middleOfWindow.X - playerSize.X / 2f, middleOfWindow.Y - playerSize.Y / 2f + this._interactionTitleHeight / 2);
            playerContainer.GetContainer().Render(playerContainerStartPos);

            var mousePos = Input.GetMousePositionInWindow();

            var playerState = this._playerEntity.GetComponent<PlayerStateComponent>();

            if (playerState.MouseSlot.Item.Instance != null)
            {
                var item = playerState.MouseSlot.Item.Instance;
                Renderer.Texture.Render(item.GetTexture(), mousePos, Vector2.One * 2f, 0f, ColorF.White);
            }
        }
        else
        {
            var middleOfWindow = this.MiddleOfWindow();
            // Render other container
            var otherContainer = this._otherEntity.GetComponent<ContainerComponent>();
            var otherSize = otherContainer.GetContainer().Provider.GetRenderSize();

            if (otherContainer.GetContainer().Provider.ShowPlayerContainer)
            {
                var totalSize = this.GetTotalSize(true);
                var topLeft = middleOfWindow - totalSize / 2f;
                Container.RenderBackground(topLeft, totalSize);

                this.RenderTitle(otherContainer.GetContainer().Provider.Name, topLeft);
                var otherContainerStartPos = new Vector2(middleOfWindow.X - otherSize.X / 2f, middleOfWindow.Y - totalSize.Y / 2f + this._interactionTitleHeight);
                otherContainer.GetContainer().Render(otherContainerStartPos);

                // Render player container
                var playerContainer = this._playerEntity.GetComponent<ContainerComponent>();
                var playerSize = playerContainer.GetContainer().Provider.GetRenderSize();

                var playerContainerStartPos = new Vector2(middleOfWindow.X - playerSize.X / 2f, otherContainerStartPos.Y + otherSize.Y);
                playerContainer.GetContainer().Render(playerContainerStartPos);

                var mousePos = Input.GetMousePositionInWindow();

                var playerState = this._playerEntity.GetComponent<PlayerStateComponent>();

                if (playerState.MouseSlot.Item.Instance != null)
                {
                    var item = playerState.MouseSlot.Item.Instance;
                    Renderer.Texture.Render(item.GetTexture(), mousePos, Vector2.One * 2f, 0f, ColorF.White);
                }
            }
            else
            {
                var totalSize = this.GetTotalSize(false);
                var topLeft = middleOfWindow - totalSize / 2f;
                Container.RenderBackground(topLeft, totalSize);
                this.RenderTitle(otherContainer.GetContainer().Provider.Name, topLeft);
                var otherContainerStartPos = new Vector2(middleOfWindow.X - otherSize.X / 2f, middleOfWindow.Y - totalSize.Y / 2f + this._interactionTitleHeight);
                otherContainer.GetContainer().Render(otherContainerStartPos);

                var mousePos = Input.GetMousePositionInWindow();

                var playerState = this._playerEntity.GetComponent<PlayerStateComponent>();

                if (playerState.MouseSlot.Item.Instance != null)
                {
                    var item = playerState.MouseSlot.Item.Instance;
                    Renderer.Texture.Render(item.GetTexture(), mousePos, Vector2.One * 2f, 0f, ColorF.White);
                }
            }
        }
    }
}
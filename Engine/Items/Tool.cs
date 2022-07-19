using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.Items;

public class Tool : Item
{
    public int Durability { get; set; }
    public IUseTool OnUse { get; set; }
    public int Reach { get; set; }

    public Tool(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, int durability, IUseTool onUse, int reach, GameClient gameClient) : base(itemID, itemName, texture, itemType, maxStack, gameClient)
    {
        Durability = durability;
        OnUse = onUse;
        Reach = reach;
    }

    public void RenderReach(CoordinateVector middle)
    {
        for (int x = -Reach; x <= Reach; x++)
        {
            for (int y = -Reach; y <= Reach; y++)
            {
                CoordinateVector position = new CoordinateVector(middle.X + x, middle.Y + y);
                Vector2i tileAligned = position.ToTileAligned();

                CoordinateVector cv = new CoordinateVector(tileAligned.X, tileAligned.Y);

                if ((cv - middle).Length() <= Reach)
                {
                    RectangleF rec = new RectangleF(cv.X * 32, cv.Y * 32, 32, 32);

                    Renderer.Primitive.RenderRectangle(rec, ColorF.Green * 0.5f);
                }
            }
        }
    }

    private float _totalTimeUsed = 0f;
    private Vector2i _startMouseTilePos;

    private bool CanReach(Vector2i mouseTilePos, CoordinateVector playerPos)
    {
        CoordinateVector cv = new CoordinateVector(mouseTilePos.X, mouseTilePos.Y);
        return (cv - playerPos).Length() <= Reach;
    }

    private CoordinateVector GetMiddleOfPlayerEntity(Entity playerEntity)
    {
        var transform = playerEntity.GetComponent<TransformComponent>();
        var animator = playerEntity.GetComponent<AnimatorComponent>();

        var playerPos = transform.Position;
        var vec2Offset = animator.GetAnimator().GetCurrentAnimation().GetMiddleOfCurrentFrameScaled();

        return playerPos + (new CoordinateVector(vec2Offset.X, vec2Offset.Y));
    }

    public override void OnHoldLeftClick(Entity playerEntity, Vector2i mouseTilePos, ECS ecs, float deltaTime)
    {
        if (!mouseTilePos.Equals(_startMouseTilePos))
        {
            _totalTimeUsed = 0f;
            _startMouseTilePos = mouseTilePos;
        }
        else
        {
            AnimatorComponent ac = playerEntity.GetComponent<AnimatorComponent>();
            Vector2 offset = ac.GetAnimator().GetCurrentAnimation().GetMiddleOfCurrentFrameScaled() / 2f;
            CoordinateVector playerPos = playerEntity.GetComponent<TransformComponent>().Position + new CoordinateVector(offset.X / TileGrid.TILE_SIZE, offset.Y / TileGrid.TILE_SIZE);
            CoordinateVector cv = new CoordinateVector(mouseTilePos.X, mouseTilePos.Y);

            if (this.CanReach(mouseTilePos, playerPos) && this.OnUse.CanUse(this, playerEntity, new CoordinateVector(mouseTilePos.X, mouseTilePos.Y), ecs))
            {
                _totalTimeUsed += deltaTime;
                bool done = this.OnUse.UseTool(this, playerEntity, cv, ecs, deltaTime, this._totalTimeUsed);
                if (done)
                {
                    this._totalTimeUsed = 0f;
                }
            }
            else
            {
                _totalTimeUsed = 0f;
            }
        }
    }

    public override void OnReleaseLeftClick(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
    {
        this._totalTimeUsed = 0f;
    }

    public override void OnHoldLeftClickRender(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
    {
        if (this.CanReach(mouseWorldPosition, playerEntity.GetComponent<TransformComponent>().Position) && this.OnUse.CanUse(this, playerEntity, new CoordinateVector(mouseWorldPosition.X, mouseWorldPosition.Y), ecs))
        {
            var animator = playerEntity.GetComponent<AnimatorComponent>();
            var animationOffset = animator.GetAnimator().GetCurrentAnimation().GetMiddleOfCurrentFrameScaled();

            CoordinateVector playerPos = playerEntity.GetComponent<TransformComponent>().Position;
            CoordinateVector mousePos = new CoordinateVector(mouseWorldPosition.X, mouseWorldPosition.Y);

            Vector2 playerPos2 = playerPos.ToWorldVector().ToVector2() + animationOffset;
            Vector2 mousePos2 = mousePos.ToWorldVector().ToVector2() + new Vector2(TileGrid.TILE_SIZE, TileGrid.TILE_SIZE) / 2f;

            Renderer.Primitive.RenderLine(playerPos2, mousePos2, 2, ColorF.Green * 0.5f);

            base.OnHoldLeftClickRender(playerEntity, mouseWorldPosition, ecs);
        }
    }

    public override void OnReleaseLeftClickRender(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
    {
        base.OnReleaseLeftClickRender(playerEntity, mouseWorldPosition, ecs);
    }
}

public interface IUseTool
{
    bool CanUse(Tool tool, Entity playerEntity, CoordinateVector mouseWorldPosition, ECS ecs);
    bool UseTool(Tool tool, Entity playerEntity, CoordinateVector mouseWorldPosition, ECS ecs, float deltaTime, float totalTimeUsed);
}
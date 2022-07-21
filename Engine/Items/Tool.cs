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
    public float UseTime { get; set; }

    public Tool(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, int durability, IUseTool onUse, int reach, float useTime, GameServer gameServer, GameClient gameClient) : base(itemID, itemName, texture, itemType, maxStack, gameServer, gameClient)
    {
        Durability = durability;
        OnUse = onUse;
        Reach = reach;
        UseTime = useTime;
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

    private Vector2i _startMouseTilePos;

    private bool CanReach(CoordinateVector mouseTilePos, CoordinateVector playerPos)
    {
        return (mouseTilePos - playerPos).Length() <= Reach;
    }

    private CoordinateVector GetMiddleOfPlayerEntity(Entity playerEntity)
    {
        var transform = playerEntity.GetComponent<TransformComponent>();
        var animator = playerEntity.GetComponent<AnimatorComponent>();

        var playerPos = transform.Position.ToWorldVector().ToVector2();
        var vec2Offset = animator.GetAnimator().GetCurrentAnimation().GetMiddleOfCurrentFrameScaled();

        return new CoordinateVector(playerPos.X + vec2Offset.X, playerPos.Y + vec2Offset.Y) / TileGrid.TILE_SIZE;
    }

    public override bool OnHoldLeftClick(UserCommand originatingCommand, Entity playerEntity, Vector2i mouseTilePos, ECS ecs, float deltaTime, float totalTimeHeld)
    {
        if (!mouseTilePos.Equals(_startMouseTilePos))
        {
            _startMouseTilePos = mouseTilePos;
            return false;
        }
        else
        {
            var playerPos = GetMiddleOfPlayerEntity(playerEntity);
            var cv = new CoordinateVector(mouseTilePos.X, mouseTilePos.Y);
            var middleOfTilePos = cv + new CoordinateVector(0.5f, 0.5f);

            if (this.CanReach(middleOfTilePos, playerPos) && this.OnUse.CanUse(this, playerEntity, mouseTilePos, ecs) && totalTimeHeld > this.UseTime)
            {
                if (!originatingCommand.HasBeenRun)
                {
                    // Only run this once on the client.
                    this.OnUseTick(playerEntity, mouseTilePos, ecs);

                    if (ecs.IsRunner(SystemRunner.Client))
                    {
                        Audio.Play("default.audio.click");
                    }
                    else if (ecs.IsRunner(SystemRunner.Server))
                    {
                        // Tell other clients to play the sound.
                    }
                }
                return false;
            }
            return true;
        }
    }

    public override void OnHoldLeftClickRender(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float totalTimeHeld)
    {
        var playerPos = GetMiddleOfPlayerEntity(playerEntity);
        var playerPos2 = playerPos.ToWorldVector().ToVector2();
        var cv = new CoordinateVector(mouseWorldPosition.X, mouseWorldPosition.Y);
        var middleOfTilePos = cv + new CoordinateVector(0.5f, 0.5f);

        if (this.CanReach(middleOfTilePos, playerPos) && this.OnUse.CanUse(this, playerEntity, mouseWorldPosition, ecs))
        {
            Renderer.Primitive.RenderLine(playerPos2, middleOfTilePos.ToWorldVector().ToVector2(), 2, ColorF.Green * (totalTimeHeld / this.UseTime));
        }
    }

    public override void OnUseTick(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
    {
        this.OnUse.UseTool(this, playerEntity, mouseWorldPosition, ecs);
    }
}

public interface IUseTool
{
    bool CanUse(Tool tool, Entity playerEntity, Vector2i mouseTilePos, ECS ecs);
    void UseTool(Tool tool, Entity playerEntity, Vector2i mouseTilePos, ECS ecs);
}
using System.Numerics;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Items;
using AGame.Engine.World;

namespace DefaultMod;

[ScriptClass(Name = "ecs_common")]
public class ECSCommonFunctionality : IECSCommonFunctionality
{
    public bool EntityHasPosition(Entity entity)
    {
        return entity.HasComponent<TransformComponent>();
    }

    public CoordinateVector GetCameraFocusPositionForEntity(Entity entity)
    {
        var position = entity.GetComponent<TransformComponent>().Position;

        if (entity.TryGetComponent<AnimatorComponent>(out var anim))
        {
            position += anim.GetAnimator().GetCurrentAnimation().GetMiddleOfCurrentFrameScaled() / TileGrid.TILE_SIZE;
        }

        return position;
    }

    public Container GetContainerForEntity(Entity entity)
    {
        return entity.GetComponent<ContainerComponent>().GetContainer();
    }

    public CoordinateVector GetCoordinatePositionForEntity(Entity entity)
    {
        return entity.GetComponent<TransformComponent>().Position;
    }

    public ContainerSlot GetEntityMouseContainerSlot(Entity entity)
    {
        var slotInfo = entity.GetComponent<PlayerStateComponent>().MouseSlot;
        var slot = new ContainerSlot(Vector2.Zero);

        slot.Item = slotInfo.Item.Instance;
        slot.Count = slotInfo.ItemCount;

        return slot;
    }

    public (int[], int) GetHotbarInfoFromEntity(Entity entity)
    {
        var hotbar = entity.GetComponent<HotbarComponent>();

        return (hotbar.ContainerSlots, hotbar.SelectedSlot);
    }

    public void SetEntityMouseContainerSlot(Entity entity, ContainerSlot mouseSlot)
    {
        var playerState = entity.GetComponent<PlayerStateComponent>();
        playerState.MouseSlot = mouseSlot.ToSlotInfo(0);
    }
}
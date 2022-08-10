using AGame.Engine.Items;
using AGame.Engine.World;

namespace AGame.Engine.ECSys;

public interface IECSCommonFunctionality
{
    bool EntityHasPosition(Entity entity);
    CoordinateVector GetCoordinatePositionForEntity(Entity entity);
    Container GetContainerForEntity(Entity entity);
    ContainerSlot GetEntityMouseContainerSlot(Entity entity);
    void SetEntityMouseContainerSlot(Entity entity, ContainerSlot mouseSlot);
    CoordinateVector GetCameraFocusPositionForEntity(Entity entity);
    (int[], int) GetHotbarInfoFromEntity(Entity entity);
}
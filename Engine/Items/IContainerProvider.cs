using System.Collections.Generic;
using System.Numerics;

namespace AGame.Engine.Items;

// Used for in game containers, like inventories, etc.
public interface IContainerProvider
{
    string Name { get; }
    bool ShowPlayerContainer { get; }

    // Good practice to use yield etc. in this method.
    IEnumerable<ContainerSlot> GetSlots();
    IEnumerable<int> GetSlotSeekOrder();

    // Return true when an update inside the container has occured
    bool Update(float deltaTime);
    Vector2 GetRenderSize();
    void RenderBackgroundUI(Vector2 topLeft);
}
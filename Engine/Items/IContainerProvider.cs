using System.Numerics;

namespace AGame.Engine.Items;

// Used for in game containers, like inventories, etc.
public interface IContainerProvider
{
    string Name { get; }
    bool ShowPlayerContainer { get; }

    // Good practice to use yield etc. in this method.
    IEnumerable<ContainerSlot> GetSlots();

    // Return true when an update inside the container has occured
    bool Update(float deltaTime);
    bool AddItems(string item, int amount, out int remaining);
    void RemoveItem(int slot, int amount);
    Vector2 GetRenderSize();
}
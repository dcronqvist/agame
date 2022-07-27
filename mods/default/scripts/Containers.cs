using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;

namespace DefaultMod
{
    [ScriptClass(Name = "container_player_inventory")]
    public class ContainerPlayerInventory : IContainerProvider
    {
        public string Name => "Inventory";

        public bool ShowPlayerContainer => true;

        private List<ContainerSlot> _slots;

        public ContainerPlayerInventory()
        {
            IEnumerable<ContainerSlot> topSlots = Utilities.CreateSlotGrid(5, 9, 3);

            IEnumerable<ContainerSlot> hotbarSlots = Utilities.CreateSlotGrid(5, 9, 1, offset: new Vector2(0, 3 * (64 + 5) + 5));

            this._slots = hotbarSlots.Concat(topSlots).ToList();
        }

        public Vector2 GetRenderSize()
        {
            return new Vector2(9 * (64 + 5) + 5, 3 * (64 + 5) + 10 + 64 + 5);
        }

        public IEnumerable<ContainerSlot> GetSlots()
        {
            return this._slots;
        }

        public IEnumerable<int> GetSlotSeekOrder()
        {
            return Enumerable.Range(0, 4 * 9);
        }

        private float _counter = 0f;

        public bool Update(float deltaTime)
        {
            return false;
        }

        public void RenderBackgroundUI(Vector2 topLeft)
        {

        }
    }

    [ScriptClass(Name = "container_small")]
    public class SmallContainerProvider : IContainerProvider
    {
        public string Name => "Small Container";

        public bool ShowPlayerContainer => true;

        private List<ContainerSlot> _slots;

        public SmallContainerProvider()
        {
            IEnumerable<ContainerSlot> topSlots = Utilities.CreateSlotGrid(5, 3, 3);

            this._slots = topSlots.ToList();
        }

        public Vector2 GetRenderSize()
        {
            return new Vector2(3 * (64 + 5) + 5, 3 * (64 + 5) + 5);
        }

        public IEnumerable<ContainerSlot> GetSlots()
        {
            return this._slots;
        }

        public IEnumerable<int> GetSlotSeekOrder()
        {
            return Enumerable.Range(0, 3 * 3);
        }

        private float _counter = 0f;

        public bool Update(float deltaTime)
        {
            return false;
        }

        public void RenderBackgroundUI(Vector2 topLeft)
        {

        }
    }

    [ScriptClass(Name = "open_container")] // default.script.open_container
    public class OpenContainerInteract : IOnInteract
    {
        public void OnInteract(Entity playerEntity, Entity interactingWith, UserCommand command, ECS ecs)
        {
            var c = ecs.IsRunner(SystemRunner.Client) ? "client: " : "server: ";
            Logging.Log(LogLevel.Debug, $"{c} {playerEntity.ID} interacting with {interactingWith.ID}");

            interactingWith.GetComponent<ContainerComponent>().GetContainer().AddItem(ItemManager.GetItemDef("default.item.pebble").CreateItem());
            ScriptingAPI.SendContainerContentsToViewers(interactingWith);

            ScriptingAPI.OpenContainerInteract(playerEntity, ecs, interactingWith);
        }
    }
}
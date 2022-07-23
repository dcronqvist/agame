using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.Items;

public abstract class Item
{
    public string ItemID { get; set; }
    public string ItemName { get; set; }
    public Texture2D Texture { get; set; }
    public ItemType ItemType { get; set; }
    public int MaxStack { get; set; }

    // References to game client to perform actions on the world etc.
    private GameServer _gameServer;
    private GameClient _gameClient;

    public bool ShouldBeConsumed { get; set; }

    public Item(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, GameServer gameServer, GameClient gameClient)
    {
        this.ItemID = itemID;
        ItemName = itemName;
        Texture = texture;
        ItemType = itemType;
        MaxStack = maxStack;
        _gameServer = gameServer;
        _gameClient = gameClient;
        this.ShouldBeConsumed = false;
    }

    public abstract bool OnHoldLeftClick(UserCommand originatingCommand, Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float deltaTime, float totalTimeHeld);
    public abstract void OnHoldLeftClickRender(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float totalTimeHeld);
    public abstract void OnUseTick(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs);

    public void PlayAudio(string audio, ECS ecs, float pitch)
    {
        if (ecs.IsRunner(SystemRunner.Client))
        {
            this.PlayAudioSelfClient(audio, pitch);
        }
        else if (ecs.IsRunner(SystemRunner.Server))
        {
            this.PlayAudioBroadcastToClients(audio, pitch);
        }
    }

    private void PlayAudioSelfClient(string audio, float pitch)
    {
        Audio.Play(audio, pitch);
    }

    private void PlayAudioBroadcastToClients(string audio, float pitch)
    {
        // TODO: Broadcast to other clients
    }

    public void CreateEntity(string entity, ECS ecs, Action<Entity> onCreate)
    {
        if (ecs.IsRunner(SystemRunner.Server))
        {
            // Only the server is allowed to create entities, so we should only create the entity on the server.
            // The clients will receive the entity creation message and create the entity on their own.
            _gameServer.PerformOnECS((ecs) =>
            {
                var e = ecs.CreateEntityFromAsset(entity);
                onCreate.Invoke(e);
            });
        }

        // However if we are client, we kinda should predict the created entity and then let the server dictate if it can stay or not
        // TODO: Implement this
    }

    public Entity GetEntityAtTileAlignedPos(Vector2i tilePosition, ECS ecs)
    {
        var pos = new CoordinateVector(tilePosition.X, tilePosition.Y);

        var entities = ecs.GetAllEntities(e => e.HasComponent<TransformComponent>());

        return entities.FirstOrDefault(e => e.GetComponent<TransformComponent>().Position.Equals(pos), null);
    }
}

public class Consumable : Item
{
    public string OnConsume { get; set; }
    public bool ConsumesOnUse { get; set; }

    public Consumable(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, string onConsume, bool consumesOnUse, GameServer gameServer, GameClient gameClient) : base(itemID, itemName, texture, itemType, maxStack, gameServer, gameClient)
    {
        OnConsume = onConsume;
        ConsumesOnUse = consumesOnUse;
    }

    public override void OnUseTick(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
    {

    }

    public override bool OnHoldLeftClick(UserCommand originatingCommand, Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float deltaTime, float totalTimeHeld)
    {
        return true;
    }

    public override void OnHoldLeftClickRender(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float totalTimeHeld)
    {

    }
}

public class Equipable : Item
{
    public string Effect { get; set; }

    public Equipable(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, string effect, GameServer gameServer, GameClient gameClient) : base(itemID, itemName, texture, itemType, maxStack, gameServer, gameClient)
    {
        Effect = effect;
    }

    public override void OnUseTick(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
    {

    }

    public override bool OnHoldLeftClick(UserCommand originatingCommand, Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float deltaTime, float totalTimeHeld)
    {
        return true;
    }

    public override void OnHoldLeftClickRender(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float totalTimeHeld)
    {

    }
}

public class Placeable : Item
{
    public string PlacesEntity { get; set; }
    public bool ConsumesOnPlace { get; set; }

    public Placeable(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, string placesEntity, bool consumesOnPlace, GameServer gameServer, GameClient gameClient) : base(itemID, itemName, texture, itemType, maxStack, gameServer, gameClient)
    {
        PlacesEntity = placesEntity;
        ConsumesOnPlace = consumesOnPlace;
    }

    public override void OnUseTick(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
    {

    }

    public override bool OnHoldLeftClick(UserCommand originatingCommand, Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float deltaTime, float totalTimeHeld)
    {
        if (totalTimeHeld > 0.05f)
        {
            var playerPos = playerEntity.GetComponent<TransformComponent>().Position;
            var tilePos = new CoordinateVector(mouseWorldPosition.X, mouseWorldPosition.Y);

            var distance = playerPos.DistanceTo(tilePos);

            if (distance > 2)
            {
                return false;
            }

            var entityAtMouse = this.GetEntityAtTileAlignedPos(mouseWorldPosition, ecs);

            if (entityAtMouse != null)
            {
                // There is an entity at the mouse position, so we can't place the item.
                return false;
            }

            // There is no entity at the mouse position, so we can place the item.

            if (!originatingCommand.HasBeenRun)
            {
                this.CreateEntity(this.PlacesEntity, ecs, (entity) =>
                {
                    var transform = entity.GetComponent<TransformComponent>();
                    transform.Position = new CoordinateVector(mouseWorldPosition.X, mouseWorldPosition.Y);
                });

                if (this.ConsumesOnPlace)
                {
                    this.ShouldBeConsumed = true;
                }
            }
            return false;
        }

        return true;
    }

    public override void OnHoldLeftClickRender(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float totalTimeHeld)
    {

    }
}

public class Resource : Item
{
    public Resource(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, GameServer gameServer, GameClient gameClient) : base(itemID, itemName, texture, itemType, maxStack, gameServer, gameClient)
    {
    }

    public override bool OnHoldLeftClick(UserCommand originatingCommand, Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float deltaTime, float totalTimeHeld)
    {
        return true;
    }

    public override void OnHoldLeftClickRender(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float totalTimeHeld)
    {

    }

    public override void OnUseTick(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
    {

    }
}
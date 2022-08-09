using System;
using System.Collections.Generic;
using System.Linq;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Assets.Scripting;

public static class ScriptingAPI
{
    internal static GameClient _gameClient;
    internal static GameServer _gameServer;

    internal static void Initialize(GameServer gameServer, GameClient gameClient)
    {
        _gameServer = gameServer;
        _gameClient = gameClient;
    }

    public static void SendContainerContentsToViewers(Entity entityWithContainer)
    {
        if (_gameServer != null)
        {
            _gameServer.SendContainerContentsToViewers(entityWithContainer);
        }
    }

    public static void SendContainerProviderDataToViewers(Entity entityWithContainer)
    {
        if (_gameServer != null)
        {
            _gameServer.SendContainerProviderDataToViewers(entityWithContainer);
        }
    }

    public static void NotifyPlayerInventoryUpdate(Entity playerEntity)
    {
        SendContainerContentsToViewers(playerEntity);
    }

    // Can be called from both client and server, will do correct thing depending on context.
    public static void CreateEntity(Entity playerEntity, ECS ecs, string entity, Action<Entity> onCreate)
    {
        if (ecs.IsRunner(SystemRunner.Server))
        {
            // IF WE ARE ON THE SERVER
            _gameServer.CreateEntityAsClient(playerEntity.ID, entity, onCreate);
        }
        else if (ecs.IsRunner(SystemRunner.Client))
        {
            // IF WE ARE ON THE CLIENT
            _gameClient.AttemptCreateEntity(entity, onCreate);
        }
    }

    // Can be called from both client and server, will do correct thing depending on context.
    public static void DestroyEntity(Entity playerEntity, ECS ecs, Entity otherEntity)
    {
        if (ecs.IsRunner(SystemRunner.Server))
        {
            // IF WE ARE ON THE SERVER
            _gameServer.DestroyEntity(otherEntity.ID);
        }
        else if (ecs.IsRunner(SystemRunner.Client))
        {
            // IF WE ARE ON THE CLIENT
            _gameClient.AttemptDestroyEntity(otherEntity.ID);
        }
    }

    /// <summary>
    /// Open container interact 
    /// Should never be run on client and server simultaneously.
    /// </summary>
    public static void OpenContainerInteract(Entity playerEntity, ECS ecs, Entity entityWithContainer)
    {
        if (ecs.IsRunner(SystemRunner.Client))
        {
            _gameClient.RequestOpenContainer(entityWithContainer.ID);
        }
        else
        {
            _gameServer.SendContainerContentTo(playerEntity.ID, entityWithContainer);
        }
    }

    public static IEnumerable<Entity> FindEntities(ECS ecs, Predicate<Entity> predicate)
    {
        return ecs.GetAllEntities((entity) => predicate(entity));
    }

    public static Entity GetEntityAtPosition(ECS ecs, Vector2i tilePosition)
    {
        return FindEntities(ecs, (entity) => entity.TryGetComponent<TransformComponent>(out var transform) && transform.Position.Equals(new CoordinateVector(tilePosition.X, tilePosition.Y))).FirstOrDefault();
    }

    public static void PlayAudioFromPlayerAction(Entity playerEntity, ECS ecs, UserCommand command, string audioName)
    {
        if (ecs.IsRunner(SystemRunner.Server))
        {
            _gameServer.PlayAudioOnAllClients(audioName);
        }
        else if (ecs.IsRunner(SystemRunner.Client))
        {
            if (!command.HasBeenRun)
                Audio.Play(audioName);
        }
    }
}
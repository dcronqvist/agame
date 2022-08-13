using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod
{
    [ScriptType(Name = "command_test_client")]
    public class TestClientCommand : ClientSideCommand
    {
        public override IEnumerable<string> GetAliases()
        {
            yield return "test";
        }

        public override Command GetCommand(Entity callingEntity, ECS ecs, GameClient gameClient)
        {
            Command c = new Command("test");

            c.SetHandler((context) =>
            {
                Logging.Log(LogLevel.Debug, $"Test client command executed from entity {callingEntity.ID}");
            });

            return c;
        }
    }

    [ScriptType(Name = "command_test_server")]
    public class TestServerCommand : ServerSideCommand
    {
        public override IEnumerable<string> GetAliases()
        {
            yield return "test_server";
        }

        public override Command GetCommand(Entity callingEntity, ECS ecs, GameServer gameServer)
        {
            Command c = new Command("test_server");

            c.SetHandler((context) =>
            {
                Logging.Log(LogLevel.Debug, $"Test server command executed from entity {callingEntity.ID}");
            });

            return c;
        }
    }

    [ScriptType(Name = "command_give")]
    public class GiveItemCommand : ServerSideCommand
    {
        public override IEnumerable<string> GetAliases()
        {
            yield return "give";
        }

        public override Command GetCommand(Entity callingEntity, ECS ecs, GameServer gameServer)
        {
            Command c = new Command("give");

            Argument<string> itemName = new Argument<string>("itemName");
            Argument<int> itemCount = new Argument<int>("amount", getDefaultValue: () => 1);

            c.AddArgument(itemName);
            c.AddArgument(itemCount);

            c.SetHandler((itemNameValue, amount) =>
            {
                Logging.Log(LogLevel.Debug, $"Give item command executed from entity {callingEntity.ID}");

                foreach (var i in Enumerable.Range(0, amount))
                {
                    callingEntity.GetComponent<ContainerComponent>().GetContainer().AddItem(ItemManager.GetItemDef(itemNameValue).CreateItem());
                }

                gameServer.SendContainerContentsToViewers(callingEntity);

            }, itemName, itemCount);

            return c;
        }
    }

    [ScriptType(Name = "command_tp")]
    public class TeleportPlayerCommand : ServerSideCommand
    {
        public override IEnumerable<string> GetAliases()
        {
            yield return "tp";
        }

        public override Command GetCommand(Entity callingEntity, ECS ecs, GameServer gameServer)
        {
            Command c = new Command("tp");

            Argument<int> x = new Argument<int>("x");
            Argument<int> y = new Argument<int>("y");

            c.AddArgument(x);
            c.AddArgument(y);

            c.SetHandler((xVal, yVal) =>
            {
                callingEntity.GetComponent<TransformComponent>().Position = new CoordinateVector(xVal, yVal);
            }, x, y);

            return c;
        }
    }

    [ScriptType(Name = "command_set_charge")]
    public class SetToolChargeCommand : ServerSideCommand
    {
        public override IEnumerable<string> GetAliases()
        {
            yield return "set_charge";
        }

        public override Command GetCommand(Entity callingEntity, ECS ecs, GameServer gameServer)
        {
            Command c = new Command("set_charge");

            Argument<int> charge = new Argument<int>("charge");

            c.AddArgument(charge);

            c.SetHandler((chargeVal) =>
            {
                var hotbar = callingEntity.GetComponent<HotbarComponent>();
                var inventory = callingEntity.GetComponent<ContainerComponent>().GetContainer();

                var toolSlot = inventory.GetSlot(hotbar.SelectedSlot);
                if (toolSlot.Item is null)
                {
                    Logging.Log(LogLevel.Warning, $"Trying to set charge of empty slot {hotbar.SelectedSlot}");
                    return;
                }
                else
                {
                    if (toolSlot.Item.TryGetComponent<Tool>(out Tool tool))
                    {
                        tool.CurrentEnergyCharge = chargeVal;
                        Logging.Log(LogLevel.Debug, $"Set charge of tool {toolSlot.Item.Definition.ItemID} to {chargeVal}");
                        ScriptingAPI.NotifyPlayerInventoryUpdate(callingEntity);
                    }
                    else
                    {
                        Logging.Log(LogLevel.Warning, $"Trying to set charge of non-tool slot {hotbar.SelectedSlot}");
                    }
                }

            }, charge);

            return c;
        }
    }

    [ScriptType(Name = "command_get_id")]
    public class GetEntityIDCommand : ServerSideCommand
    {
        public override IEnumerable<string> GetAliases()
        {
            yield return "get_id";
        }

        public override Command GetCommand(Entity callingEntity, ECS ecs, GameServer gameServer)
        {
            Command c = new Command("get_id");

            c.SetHandler((context) =>
            {
                // Assuming that the calling entity is a player
                var playerState = callingEntity.GetComponent<PlayerStateComponent>();
                var x = playerState.MouseTileX;
                var y = playerState.MouseTileY;

                ecs.GetAllEntities((e) => e.TryGetComponent<TransformComponent>(out var transform) && transform.Position.DistanceTo(new CoordinateVector(x, y)) < 1).ToList().ForEach((e) =>
                {
                    var transform = e.GetComponent<TransformComponent>();
                    Logging.Log(LogLevel.Debug, $"Entity {e.ID} at {transform.Position.X},{transform.Position.Y}");
                });
            });

            return c;
        }
    }

    [ScriptType(Name = "command_set_prop")]
    public class SetEntityComponentPropertyCommand : ServerSideCommand
    {
        public override IEnumerable<string> GetAliases()
        {
            yield return "set_prop";
        }

        public override System.CommandLine.Command GetCommand(Entity callingEntity, ECS ecs, GameServer gameServer)
        {
            // set_prop <entity_id:int> <component_name:string> <property_name:string> <property_value:string>
            Command c = new Command("set_prop");
            Argument<int> entityID = new Argument<int>("entity_id");
            Argument<string> componentName = new Argument<string>("component_name");
            Argument<string> propertyName = new Argument<string>("property_name");
            Argument<string> propertyValue = new Argument<string>("property_value");
            c.AddArgument(entityID);
            c.AddArgument(componentName);
            c.AddArgument(propertyName);
            c.AddArgument(propertyValue);
            c.SetHandler((entityIDValue, componentNameValue, propertyNameValue, propertyValueValue) =>
            {
                var entity = ecs.GetEntityFromID(entityIDValue);
                if (entity is null)
                {
                    Logging.Log(LogLevel.Warning, $"Entity {entityIDValue} not found");
                    return;
                }
                else
                {
                    var component = entity.GetComponent(ecs.GetComponentType(componentNameValue));
                    if (component is null)
                    {
                        Logging.Log(LogLevel.Warning, $"Component {componentNameValue} not found on entity {entityIDValue}");
                        return;
                    }
                    else
                    {
                        var property = component.GetType().GetProperty(propertyNameValue);
                        if (property is null)
                        {
                            Logging.Log(LogLevel.Warning, $"Property {propertyNameValue} not found on component {componentNameValue}");
                            return;
                        }
                        else
                        {
                            var value = Convert.ChangeType(propertyValueValue, property.PropertyType);
                            property.SetValue(component, value);
                            Logging.Log(LogLevel.Info, $"Set property {propertyNameValue} of component {componentNameValue} on entity {entityIDValue} to {propertyValueValue}");
                        }
                    }
                }
            }, entityID, componentName, propertyName, propertyValue);

            return c;
        }
    }
}
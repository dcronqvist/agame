using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod
{
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
}
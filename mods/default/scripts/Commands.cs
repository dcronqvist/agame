using System.Collections.Generic;
using System.CommandLine;
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

            c.AddArgument(itemName);

            c.SetHandler<string>((itemNameValue) =>
            {
                Logging.Log(LogLevel.Debug, $"Give item command executed from entity {callingEntity.ID}");
                callingEntity.GetComponent<ContainerComponent>().GetContainer().AddItem(ItemManager.GetItemDef(itemNameValue).CreateItem());
                gameServer.SendContainerContentsToViewers(callingEntity);
            }, itemName);

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
}
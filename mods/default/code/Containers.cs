using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;
using GameUDPProtocol;

namespace DefaultMod
{
    [ScriptType(Name = "container_player_inventory")]
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

        public void RenderBackgroundUI(Vector2 topLeft, float deltaTime)
        {

        }

        public bool Update(Container owner, float deltaTime)
        {
            return false;
        }

        public void ReceiveProviderData(SetContainerProviderDataPacket packet)
        {
            // Should never happen
        }

        public SetContainerProviderDataPacket GetContainerProviderData(int entityID)
        {
            return SetContainerProviderDataPacket.GetDefault(entityID);
        }

        public bool ShouldSendProviderData()
        {
            return false;
        }
    }

    [ScriptType(Name = "container_small")]
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

        public void RenderBackgroundUI(Vector2 topLeft, float deltaTime)
        {

        }

        public bool Update(Container owner, float deltaTime)
        {
            return false;
        }

        public void ReceiveProviderData(SetContainerProviderDataPacket packet)
        {
            // Should never happen
        }

        public SetContainerProviderDataPacket GetContainerProviderData(int entityID)
        {
            return SetContainerProviderDataPacket.GetDefault(entityID);
        }

        public bool ShouldSendProviderData()
        {
            return false;
        }
    }

    [ScriptType(Name = "open_container")] // default.script.open_container
    public class OpenContainerInteract : IOnInteract
    {
        public void OnInteract(Entity playerEntity, Entity interactingWith, UserCommand command, ECS ecs)
        {
            var c = ecs.IsRunner(SystemRunner.Client) ? "client: " : "server: ";
            Logging.Log(LogLevel.Debug, $"{c} {playerEntity.ID} interacting with {interactingWith.ID}");

            ScriptingAPI.SendContainerContentsToViewers(interactingWith);
            ScriptingAPI.SendContainerProviderDataToViewers(interactingWith);

            ScriptingAPI.OpenContainerInteract(playerEntity, ecs, interactingWith);
        }
    }

    public class RockCrusherData : IPacketable
    {
        public float Progress { get; set; }

        public RockCrusherData()
        {
            this.Progress = 0f;
        }

        public byte[] ToBytes() => PacketUtils.Serialize(this);
        public int Populate(byte[] data, int offset) => PacketUtils.Deserialize(this, data, offset);
    }

    [ScriptType(Name = "container_rock_crusher")] // default.script.container_rock_crusher
    public class RockCrusherProvider : ContainerProvider<RockCrusherData>
    {
        public override string Name => "Rock Crusher";

        public override bool ShowPlayerContainer => true;

        private List<ContainerSlot> _slots;
        private ContainerSlot left;
        private ContainerSlot right;

        public RockCrusherProvider()
        {
            _slots = new List<ContainerSlot>();

            left = new ContainerSlot(Vector2.Zero);
            right = new ContainerSlot(new Vector2(64 + ((64 + 5) * 5), 0));

            _slots.Add(left);
            _slots.Add(right);
        }

        public override Vector2 GetRenderSize()
        {
            return new Vector2(64 + (5 * (64 + 5)) + 64 + 5 + 5, 64 + 5 + 5);
        }

        public override IEnumerable<ContainerSlot> GetSlots()
        {
            return _slots;
        }

        public override IEnumerable<int> GetSlotSeekOrder()
        {
            return Enumerable.Range(0, 2);
        }

        private InterpolationQueue<float> _progressQueue = new InterpolationQueue<float>(0f, (from, to, delta) =>
        {
            return from + (to - from) * 15f * delta;
        });

        public override void RenderBackgroundUI(Vector2 topLeft, float deltaTime)
        {
            float crushTime = -1f;

            if (left.Item is null)
            {
                _counter = 0f;
            }

            if (left.Item is not null && left.Item.TryGetComponent<RockCrusherYield>(out var yie))
            {
                if (right.Item is null)
                {
                    _counter += deltaTime;
                }
                else
                {
                    if (right.Item is not null && right.Item.Definition.MaxStack > right.Count + yie.Definition.Amount && right.Item.Definition.ItemID == yie.Definition.Item)
                    {
                        // Can perform crushing
                        _counter += deltaTime;
                    }
                    else
                    {
                        _counter = 0f;
                    }
                }

                crushTime = yie.Definition.TimeToCrush;
            }
            else
            {
                _counter = 0f;
            }

            var startPos = topLeft + new Vector2(64 + 5, 0);
            var endPos = startPos + new Vector2((64 + 5) * 5 - 10, 64);

            var progress = MathF.Min(this._counter / crushTime, 1f);

            var progressPos = startPos + (endPos - startPos) * progress;

            var fullRect = new RectangleF(startPos.X, startPos.Y, endPos.X - startPos.X, endPos.Y - startPos.Y);
            Renderer.Primitive.RenderRectangle(fullRect, ColorF.Black * 0.8f);

            var rect = new RectangleF(startPos.X, startPos.Y, (progressPos.X - startPos.X), endPos.Y - startPos.Y);
            Renderer.Primitive.RenderRectangle(rect, ColorF.RoyalBlue);

            var middlePos = ((endPos - startPos) / 2f) + startPos;
            var textToShow = $"{(int)(progress * 100)}%";
            var font = ModManager.GetAsset<Font>("default.font.rainyhearts");
            var textSize = font.MeasureString(textToShow, 2f);
            var textPos = middlePos - textSize / 2f;

            Renderer.Text.RenderText(font, textToShow, textPos, 2f, ColorF.White, Renderer.Camera);
        }

        private float _counter = 0f;

        public override bool Update(Container owner, float deltaTime)
        {
            if (left.Item is null)
            {
                _counter = 0f;
                return false;
            }

            if (left.Item is not null && left.Item.TryGetComponent<RockCrusherYield>(out var yie))
            {
                if (right.Item is null)
                {
                    _counter += deltaTime;
                }
                else
                {
                    if (right.Item is not null && right.Item.Definition.MaxStack > right.Count + yie.Definition.Amount && right.Item.Definition.ItemID == yie.Definition.Item)
                    {
                        // Can perform crushing
                        _counter += deltaTime;
                    }
                    else
                    {
                        _counter = 0f;
                    }
                }

                if (_counter >= left.Item.GetComponent<RockCrusherYield>().Definition.TimeToCrush)
                {
                    var yield = left.Item.GetComponent<RockCrusherYield>();

                    if (right.Item == null)
                    {
                        right.Item = ItemManager.GetItemDef(yield.Definition.Item).CreateItem();
                        right.Count = yield.Definition.Amount;
                    }
                    else
                    {
                        right.Count += yield.Definition.Amount;
                    }

                    _counter = 0f;
                    this._lastSend = 0f;

                    left.Count -= 1;
                    if (left.Count <= 0)
                    {
                        left.Item = null;
                    }

                    return true;
                }
            }
            else
            {
                _counter = 0f;
                this._lastSend = 0f;
            }

            return false;
        }

        private float _lastSend = 0f;

        public override bool ShouldSendProviderData()
        {
            if ((_counter % 1f).InRange(0f, 0.1f) && left.Item is not null && _counter != 0f)
            {
                _lastSend = _counter;
                return true;
            }

            return false;
        }

        public override void Receive(RockCrusherData packet)
        {
            this._counter = packet.Progress;
        }

        public override RockCrusherData GetData(int entityID)
        {
            return new RockCrusherData()
            {
                Progress = this._counter
            };
        }
    }
}
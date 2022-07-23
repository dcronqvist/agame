using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Items;
using AGame.Engine.World;

namespace DefaultMod
{
    [ScriptClass(Name = "container_test_provider")]
    public class TestContainerProvider : IContainerProvider
    {
        public string Name => "Testing Container";

        public bool ShowPlayerContainer => true;

        ContainerSlot A;
        ContainerSlot B;
        ContainerSlot C;
        ContainerSlot D;

        public TestContainerProvider()
        {
            float spacing = 5f;

            A = new ContainerSlot(new Vector2(spacing, spacing));
            B = new ContainerSlot(new Vector2(64 + spacing * 2, spacing));
            C = new ContainerSlot(new Vector2(spacing, 64 + spacing * 2));
            D = new ContainerSlot(new Vector2(64 + spacing * 2, 64 + spacing * 2));
        }

        public IEnumerable<ContainerSlot> GetSlots()
        {
            yield return A;
            yield return B;
            yield return C;
            yield return D;
        }

        private float _counter = 0f;

        public bool Update(float deltaTime)
        {
            // Update slots, if the container contains any logic
            if (this.A.Item == "default.item.pebble")
            {
                this._counter += deltaTime;
            }

            if (this._counter > 5f)
            {
                this.A.Item = "default.item.test_item_2";
                this._counter = 0f;
                return true;
            }

            return false;
        }

        private int FindSlotWithSameItem(string item, int roomFor)
        {
            var slots = new ContainerSlot[] { A, B, C, D };

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Item == item && (slots[i].GetItem().MaxStack - slots[i].Count) >= roomFor)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindNextEmptySlot()
        {
            var slots = new ContainerSlot[] { A, B, C, D };

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Item == null || slots[i].Item == "")
                {
                    return i;
                }
            }

            return -1;
        }

        public bool AddItems(string item, int amount, out int remaining)
        {
            remaining = amount;

            var slots = new ContainerSlot[] { A, B, C, D };

            while (remaining > 0)
            {
                int sameItemSlot = this.FindSlotWithSameItem(item, 1);

                if (sameItemSlot != -1)
                {
                    var foundSlot = slots[sameItemSlot];

                    if (foundSlot.Count + remaining > foundSlot.GetItem().MaxStack)
                    {
                        int diff = foundSlot.GetItem().MaxStack - foundSlot.Count;
                        foundSlot.Count = foundSlot.GetItem().MaxStack;
                        remaining -= diff;
                    }
                    else
                    {
                        foundSlot.Count += remaining;
                        remaining -= remaining;
                    }
                }
                else
                {
                    int emptySlot = this.FindNextEmptySlot();

                    if (emptySlot != -1)
                    {
                        var foundSlot = slots[emptySlot];
                        foundSlot.Item = item;
                        foundSlot.Count = remaining;
                        remaining = 0;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public Vector2 GetRenderSize()
        {
            return new Vector2(2 * (64 + 5) + 5, 2 * (64 + 5) + 5);
        }

        public void RemoveItem(int slot, int amount)
        {
            var slots = new ContainerSlot[] { A, B, C, D };

            var foundSlot = slots[slot];
            foundSlot.Count -= amount;

            if (foundSlot.Count <= 0)
            {
                foundSlot.Item = null;
                foundSlot.Count = 0;
            }
        }
    }

    [ScriptClass(Name = "container_large")]
    public class LargeContainerProvider : IContainerProvider
    {
        public string Name => "Large Container";

        public bool ShowPlayerContainer => true;

        private List<ContainerSlot> _slots;

        public LargeContainerProvider()
        {
            this._slots = Utilities.CreateSlotGrid(5, 9, 3).ToList();
        }

        private int FindSlotWithSameItem(string item, int roomFor)
        {
            for (int i = 0; i < this._slots.Count; i++)
            {
                if (this._slots[i].Item == item && (this._slots[i].GetItem().MaxStack - this._slots[i].Count) >= roomFor)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindNextEmptySlot()
        {
            for (int i = 0; i < this._slots.Count; i++)
            {
                if (this._slots[i].Item == null || this._slots[i].Item == "")
                {
                    return i;
                }
            }

            return -1;
        }

        public bool AddItems(string item, int amount, out int remaining)
        {
            remaining = amount;

            while (remaining > 0)
            {
                int sameItemSlot = this.FindSlotWithSameItem(item, 1);

                if (sameItemSlot != -1)
                {
                    var foundSlot = this._slots[sameItemSlot];

                    if (foundSlot.Count + remaining > foundSlot.GetItem().MaxStack)
                    {
                        int diff = foundSlot.GetItem().MaxStack - foundSlot.Count;
                        foundSlot.Count = foundSlot.GetItem().MaxStack;
                        remaining -= diff;
                    }
                    else
                    {
                        foundSlot.Count += remaining;
                        remaining -= remaining;
                    }
                }
                else
                {
                    int emptySlot = this.FindNextEmptySlot();

                    if (emptySlot != -1)
                    {
                        var foundSlot = this._slots[emptySlot];
                        foundSlot.Item = item;
                        foundSlot.Count = remaining;
                        remaining = 0;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public Vector2 GetRenderSize()
        {
            return new Vector2(9 * (ContainerSlot.WIDTH + 5) + 5, 3 * (ContainerSlot.WIDTH + 5) + 5);
        }

        public IEnumerable<ContainerSlot> GetSlots()
        {
            return this._slots;
        }

        public bool Update(float deltaTime)
        {
            return false;
        }

        public void RemoveItem(int slot, int amount)
        {
            this._slots[slot].Count -= amount;

            if (this._slots[slot].Count <= 0)
            {
                this._slots[slot].Item = "";
                this._slots[slot].Count = 0;
            }
        }
    }
}
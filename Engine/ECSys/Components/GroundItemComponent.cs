// using System;
// using System.Collections.Generic;
// using System.Text;
// using AGame.Engine.Items;
// using AGame.Engine.Networking;
// using AGame.Engine.World;

// namespace AGame.Engine.ECSys.Components;

// [ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
// public class GroundItemComponent : Component
// {
//     private ItemInstance _item;
//     public ItemInstance Item
//     {
//         get => _item;
//         set
//         {
//             if (_item != value)
//             {
//                 _item = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private int _pickedUpBy;
//     public int PickedUpBy
//     {
//         get => _pickedUpBy;
//         set
//         {
//             if (_pickedUpBy != value)
//             {
//                 _pickedUpBy = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     public GroundItemComponent()
//     {
//         _pickedUpBy = -1;
//     }

//     public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
//     {
//         // Do nothing
//     }

//     public override Component Clone()
//     {
//         return new GroundItemComponent()
//         {
//             Item = this.Item,
//             PickedUpBy = this.PickedUpBy
//         };
//     }

//     public override ulong GetHash()
//     {
//         return Utilities.Hash(this.ToBytes());
//     }

//     public override void InterpolateProperties(Component from, Component to, float amt)
//     {
//         GroundItemComponent toComp = (GroundItemComponent)to;
//         this.Item = toComp.Item;
//         this.PickedUpBy = toComp.PickedUpBy;
//     }

//     public override int Populate(byte[] data, int offset)
//     {
//         int start = offset;

//         PackedItem pi = new PackedItem();
//         offset += pi.Populate(data, offset);
//         this.Item = pi.Instance;

//         this.PickedUpBy = BitConverter.ToInt32(data, offset);
//         offset += sizeof(int);
//         return offset - start;
//     }

//     public override byte[] ToBytes()
//     {
//         List<byte> bytes = new List<byte>();

//         PackedItem pi = new PackedItem(this.Item);
//         bytes.AddRange(pi.ToBytes());

//         bytes.AddRange(BitConverter.GetBytes(this.PickedUpBy));
//         return bytes.ToArray();
//     }

//     public override string ToString()
//     {
//         return $"GroundItemComponent: {this.Item}";
//     }

//     public override void UpdateComponent(Component newComponent)
//     {
//         GroundItemComponent newComp = (GroundItemComponent)newComponent;
//         this.Item = newComp.Item;
//         this.PickedUpBy = newComp.PickedUpBy;
//     }
// }
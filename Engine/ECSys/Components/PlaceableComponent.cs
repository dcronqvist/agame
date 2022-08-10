// using System;
// using System.Collections.Generic;
// using System.Numerics;
// using System.Text;
// using AGame.Engine.Networking;
// using AGame.Engine.World;

// namespace AGame.Engine.ECSys.Components;

// [ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
// public class PlaceableComponent : Component
// {
//     private Vector2 _placeOffset;
//     public Vector2 PlaceOffset
//     {
//         get => _placeOffset;
//         set
//         {
//             if (_placeOffset != value)
//             {
//                 _placeOffset = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
//     {

//     }

//     public override Component Clone()
//     {
//         return new PlaceableComponent()
//         {
//             PlaceOffset = this.PlaceOffset
//         };
//     }

//     public override ulong GetHash()
//     {
//         return Utilities.Hash(this.ToBytes());
//     }

//     public override void InterpolateProperties(Component from, Component to, float amt)
//     {
//         var fromC = (PlaceableComponent)from;
//         var toC = (PlaceableComponent)to;

//         this.PlaceOffset = Vector2.Lerp(fromC.PlaceOffset, toC.PlaceOffset, amt);
//     }

//     public override int Populate(byte[] data, int offset)
//     {
//         int sOffset = offset;
//         this.PlaceOffset = new Vector2(BitConverter.ToSingle(data, sOffset), BitConverter.ToSingle(data, sOffset + 4));
//         offset += sizeof(float) * 2;
//         return offset - sOffset;
//     }

//     public override byte[] ToBytes()
//     {
//         List<byte> bytes = new List<byte>();
//         bytes.AddRange(BitConverter.GetBytes(this.PlaceOffset.X));
//         bytes.AddRange(BitConverter.GetBytes(this.PlaceOffset.Y));
//         return bytes.ToArray();
//     }

//     public override string ToString()
//     {
//         return $"PlaceableComponent: {this.PlaceOffset}";
//     }

//     public override void UpdateComponent(Component newComponent)
//     {
//         var newC = (PlaceableComponent)newComponent;
//         this.PlaceOffset = newC.PlaceOffset;
//     }
// }
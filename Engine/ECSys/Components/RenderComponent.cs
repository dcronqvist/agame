// using System;
// using System.Collections.Generic;
// using System.Numerics;
// using AGame.Engine.Networking;
// using AGame.Engine.World;

// namespace AGame.Engine.ECSys.Components;

// [ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
// public class RenderComponent : Component
// {
//     private bool _sortByY;
//     public bool SortByY
//     {
//         get => _sortByY;
//         set
//         {
//             if (_sortByY != value)
//             {
//                 _sortByY = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private byte _renderLayer;
//     public byte RenderLayer
//     {
//         get => _renderLayer;
//         set
//         {
//             if (_renderLayer != value)
//             {
//                 _renderLayer = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private Vector2 _feetOffset;
//     public Vector2 FeetOffset
//     {
//         get => _feetOffset;
//         set
//         {
//             if (_feetOffset != value)
//             {
//                 _feetOffset = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
//     {

//     }

//     public override Component Clone()
//     {
//         return new RenderComponent()
//         {
//             SortByY = this.SortByY,
//             RenderLayer = this.RenderLayer,
//             FeetOffset = this.FeetOffset
//         };
//     }

//     public override ulong GetHash()
//     {
//         return Utilities.Hash(this.ToBytes());
//     }

//     public override void InterpolateProperties(Component from, Component to, float amt)
//     {
//         var toC = (RenderComponent)to;
//         this.SortByY = toC.SortByY;
//         this.RenderLayer = toC.RenderLayer;
//         this.FeetOffset = toC.FeetOffset;
//     }

//     public override int Populate(byte[] data, int offset)
//     {
//         int start = offset;
//         this.SortByY = BitConverter.ToBoolean(data, offset);
//         offset += sizeof(bool);
//         this.RenderLayer = data[offset];
//         offset += sizeof(byte);
//         this.FeetOffset = new Vector2(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + sizeof(float)));
//         offset += sizeof(float) * 2;
//         return offset - start;
//     }

//     public override byte[] ToBytes()
//     {
//         List<byte> bytes = new List<byte>();
//         bytes.AddRange(BitConverter.GetBytes(this.SortByY));
//         bytes.Add(this.RenderLayer);
//         bytes.AddRange(BitConverter.GetBytes(this.FeetOffset.X));
//         bytes.AddRange(BitConverter.GetBytes(this.FeetOffset.Y));
//         return bytes.ToArray();
//     }

//     public override string ToString()
//     {
//         return $"SortByY: {this.SortByY}, RenderLayer: {this.RenderLayer}";
//     }

//     public override void UpdateComponent(Component newComponent)
//     {
//         var newC = (RenderComponent)newComponent;
//         this.SortByY = newC.SortByY;
//         this.RenderLayer = newC.RenderLayer;
//         this.FeetOffset = newC.FeetOffset;
//     }
// }
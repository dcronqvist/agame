// using System;
// using System.Collections.Generic;
// using AGame.Engine.Networking;
// using AGame.Engine.World;

// namespace AGame.Engine.ECSys.Components;

// [ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true, NetworkUpdateRate = 5)]
// public class ShadowComponent : Component
// {
//     private float _radius;
//     public float Radius
//     {
//         get => _radius;
//         set
//         {
//             if (_radius != value)
//             {
//                 _radius = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private float _opacity;
//     public float Opacity
//     {
//         get => _opacity;
//         set
//         {
//             if (_opacity != value)
//             {
//                 _opacity = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
//     {

//     }

//     public override Component Clone()
//     {
//         return new ShadowComponent()
//         {
//             Radius = this.Radius,
//             Opacity = this.Opacity
//         };
//     }

//     public override ulong GetHash()
//     {
//         return Utilities.Hash(this.ToBytes());
//     }

//     public override void InterpolateProperties(Component from, Component to, float amt)
//     {
//         ShadowComponent fromShadow = from as ShadowComponent;
//         ShadowComponent toShadow = to as ShadowComponent;
//         this.Radius = Utilities.Lerp(fromShadow.Radius, toShadow.Radius, amt);
//         this.Opacity = Utilities.Lerp(fromShadow.Opacity, toShadow.Opacity, amt);
//     }

//     public override int Populate(byte[] data, int offset)
//     {
//         int sOffset = offset;
//         this.Radius = BitConverter.ToSingle(data, offset);
//         offset += sizeof(float);
//         this.Opacity = BitConverter.ToSingle(data, offset);
//         offset += sizeof(float);
//         return offset - sOffset;
//     }

//     public override byte[] ToBytes()
//     {
//         List<byte> bytes = new List<byte>();
//         bytes.AddRange(BitConverter.GetBytes(this.Radius));
//         bytes.AddRange(BitConverter.GetBytes(this.Opacity));
//         return bytes.ToArray();
//     }

//     public override string ToString()
//     {
//         return $"ShadowComponent: {this.Radius}, {this.Opacity}";
//     }

//     public override void UpdateComponent(Component newComponent)
//     {
//         var newShadow = (ShadowComponent)newComponent;
//         this.Radius = newShadow.Radius;
//         this.Opacity = newShadow.Opacity;
//     }
// }
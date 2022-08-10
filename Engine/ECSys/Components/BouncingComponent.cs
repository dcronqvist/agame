// using System;
// using System.Collections.Generic;
// using System.Numerics;
// using AGame.Engine.Networking;
// using AGame.Engine.World;

// namespace AGame.Engine.ECSys.Components;

// [ComponentNetworking(CreateTriggersNetworkUpdate = false, UpdateTriggersNetworkUpdate = false, NetworkUpdateRate = 50)]
// public class BouncingComponent : Component
// {
//     private float _gravityFactor;
//     public float GravityFactor
//     {
//         get => _gravityFactor;
//         set
//         {
//             if (_gravityFactor != value)
//             {
//                 _gravityFactor = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private float _fallOffFactor;
//     public float FallOffFactor
//     {
//         get => _fallOffFactor;
//         set
//         {
//             if (_fallOffFactor != value)
//             {
//                 _fallOffFactor = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private float _verticalVelocity;
//     public float VerticalVelocity
//     {
//         get => _verticalVelocity;
//         set
//         {
//             if (_verticalVelocity != value)
//             {
//                 _verticalVelocity = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private Vector2 _velocity;
//     public Vector2 Velocity
//     {
//         get => _velocity;
//         set
//         {
//             if (_velocity != value)
//             {
//                 _velocity = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private float _velocityFriction;
//     public float VelocityFriction
//     {
//         get => _velocityFriction;
//         set
//         {
//             if (_velocityFriction != value)
//             {
//                 _velocityFriction = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private float _velocityThreshold;
//     public float VelocityThreshold
//     {
//         get => _velocityThreshold;
//         set
//         {
//             if (_velocityThreshold != value)
//             {
//                 _velocityThreshold = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
//     {

//     }

//     public override Component Clone()
//     {
//         return new BouncingComponent()
//         {
//             GravityFactor = this.GravityFactor,
//             FallOffFactor = this.FallOffFactor,
//             VerticalVelocity = this.VerticalVelocity,
//             Velocity = this.Velocity,
//             VelocityFriction = this.VelocityFriction,
//             VelocityThreshold = this.VelocityThreshold
//         };
//     }

//     public override ulong GetHash()
//     {
//         return Utilities.CombineHash(this.GravityFactor.Hash(), this.FallOffFactor.Hash(), this.VelocityFriction.Hash(), this.VelocityThreshold.Hash());
//     }

//     public override void InterpolateProperties(Component from, Component to, float amt)
//     {
//         var fromC = (BouncingComponent)from;
//         var toC = (BouncingComponent)to;

//         this.GravityFactor = Utilities.Lerp(fromC.GravityFactor, toC.GravityFactor, amt);
//         this.FallOffFactor = Utilities.Lerp(fromC.FallOffFactor, toC.FallOffFactor, amt);
//         this.VerticalVelocity = Utilities.Lerp(fromC.VerticalVelocity, toC.VerticalVelocity, amt);
//         this.Velocity = Vector2.Lerp(fromC.Velocity, toC.Velocity, amt);
//         this.VelocityFriction = Utilities.Lerp(fromC.VelocityFriction, toC.VelocityFriction, amt);
//         this.VelocityThreshold = Utilities.Lerp(fromC.VelocityThreshold, toC.VelocityThreshold, amt);
//     }

//     public override int Populate(byte[] data, int offset)
//     {
//         int sOffset = offset;
//         this.GravityFactor = BitConverter.ToSingle(data, offset);
//         offset += sizeof(float);
//         this.FallOffFactor = BitConverter.ToSingle(data, offset);
//         offset += sizeof(float);
//         this.VerticalVelocity = BitConverter.ToSingle(data, offset);
//         offset += sizeof(float);
//         this.VelocityThreshold = BitConverter.ToSingle(data, offset);
//         offset += sizeof(float);
//         this.Velocity = new Vector2(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + sizeof(float)));
//         offset += sizeof(float) * 2;
//         this.VelocityFriction = BitConverter.ToSingle(data, offset);
//         offset += sizeof(float);
//         return offset - sOffset;
//     }

//     public override byte[] ToBytes()
//     {
//         List<byte> bytes = new List<byte>();
//         bytes.AddRange(BitConverter.GetBytes(this.GravityFactor));
//         bytes.AddRange(BitConverter.GetBytes(this.FallOffFactor));
//         bytes.AddRange(BitConverter.GetBytes(this.VerticalVelocity));
//         bytes.AddRange(BitConverter.GetBytes(this.VelocityThreshold));
//         bytes.AddRange(BitConverter.GetBytes(this.Velocity.X));
//         bytes.AddRange(BitConverter.GetBytes(this.Velocity.Y));
//         bytes.AddRange(BitConverter.GetBytes(this.VelocityFriction));
//         return bytes.ToArray();
//     }

//     public override string ToString()
//     {
//         return $"BouncingComponent=[gravityFactor={this.GravityFactor}, fallOffFactor={this.FallOffFactor}]";
//     }

//     public override void UpdateComponent(Component newComponent)
//     {
//         var newC = newComponent as BouncingComponent;
//         this.GravityFactor = newC.GravityFactor;
//         this.FallOffFactor = newC.FallOffFactor;
//         this.VerticalVelocity = newC.VerticalVelocity;
//         this.VelocityThreshold = newC.VelocityThreshold;
//         this.Velocity = newC.Velocity;
//         this.VelocityFriction = newC.VelocityFriction;
//     }
// }
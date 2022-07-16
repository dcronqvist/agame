// using AGame.Engine.Networking;
// using AGame.Engine.World;

// namespace AGame.Engine.ECSys.Components;

// [ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
// public class TileAlignedPlaceableComponent : Component
// {
//     private CoordinateVector _position;
//     public CoordinateVector Position
//     {
//         get => _position;
//         set
//         {
//             if (!_position.Equals(value))
//             {
//                 _position = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private int _width;
//     public int Width
//     {
//         get => _width;
//         set
//         {
//             if (_width != value)
//             {
//                 _width = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private int _height;
//     public int Height
//     {
//         get => _height;
//         set
//         {
//             if (_height != value)
//             {
//                 _height = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     private bool _solid;
//     public bool Solid
//     {
//         get => _solid;
//         set
//         {
//             if (_solid != value)
//             {
//                 _solid = value;
//                 this.NotifyPropertyChanged();
//             }
//         }
//     }

//     public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
//     {
//     }

//     public override Component Clone()
//     {
//         return new TileAlignedPlaceableComponent()
//         {
//             Position = this.Position,
//             Width = this.Width,
//             Height = this.Height
//         };
//     }

//     public override int GetHashCode()
//     {
//         return HashCode.Combine(this.Position, this.Width, this.Height);
//     }

//     public override void InterpolateProperties(Component from, Component to, float amt)
//     {
//         TileAlignedPlaceableComponent fromPlaceable = (TileAlignedPlaceableComponent)from;
//         TileAlignedPlaceableComponent toPlaceable = (TileAlignedPlaceableComponent)to;
//         this.Position = CoordinateVector.Lerp(fromPlaceable.Position, toPlaceable.Position, amt);
//         this.Width = (int)Math.Round(fromPlaceable.Width + (toPlaceable.Width - fromPlaceable.Width) * amt);
//         this.Height = (int)Math.Round(fromPlaceable.Height + (toPlaceable.Height - fromPlaceable.Height) * amt);
//     }

//     public override int Populate(byte[] data, int offset)
//     {
//         int startOffset = offset;
//         float x = BitConverter.ToSingle(data, offset);
//         offset += sizeof(float);
//         float y = BitConverter.ToSingle(data, offset);
//         offset += sizeof(float);
//         int width = BitConverter.ToInt32(data, offset);
//         offset += sizeof(int);
//         int height = BitConverter.ToInt32(data, offset);
//         offset += sizeof(int);
//         this.Position = new CoordinateVector(x, y);
//         this.Width = width;
//         this.Height = height;
//         return offset - startOffset;
//     }

//     public override byte[] ToBytes()
//     {
//         List<byte> bytes = new List<byte>();
//         bytes.AddRange(BitConverter.GetBytes(this.Position.X));
//         bytes.AddRange(BitConverter.GetBytes(this.Position.Y));
//         bytes.AddRange(BitConverter.GetBytes(this.Width));
//         bytes.AddRange(BitConverter.GetBytes(this.Height));
//         return bytes.ToArray();
//     }

//     public override string ToString()
//     {
//         return $"TileAlignedPlaceableComponent: {this.Position} {this.Width}x{this.Height}";
//     }

//     public override void UpdateComponent(Component newComponent)
//     {
//         TileAlignedPlaceableComponent newPlaceable = (TileAlignedPlaceableComponent)newComponent;
//         this.Position = newPlaceable.Position;
//         this.Width = newPlaceable.Width;
//         this.Height = newPlaceable.Height;
//     }
// }
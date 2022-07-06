using System.Text;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

public interface IMovement
{
    CoordinateVector GetVelocity(Entity entity);
}

public class PlayerMovement : IMovement
{
    public CoordinateVector GetVelocity(Entity entity)
    {
        CoordinateVector movement = CoordinateVector.Zero;

        KeyboardInputComponent kic = entity.GetComponent<KeyboardInputComponent>();

        if (kic.IsKeyDown(KeyboardInputComponent.KEY_W))
        {
            movement.Y -= 1;
        }
        if (kic.IsKeyDown(KeyboardInputComponent.KEY_S))
        {
            movement.Y += 1;
        }
        if (kic.IsKeyDown(KeyboardInputComponent.KEY_A))
        {
            movement.X -= 1;
        }
        if (kic.IsKeyDown(KeyboardInputComponent.KEY_D))
        {
            movement.X += 1;
        }

        if (!movement.Equals(CoordinateVector.Zero))
        {
            return movement.Normalize() * 10f;
        }

        return CoordinateVector.Zero;
    }
}

[ComponentNetworking(CNType.Update, NDirection.ServerToClient, MaxUpdatesPerSecond = 1, IsReliable = false)]
public class MovementComponent : Component
{
    private string _movementType;
    public string MovementType
    {
        get => _movementType;
        set
        {
            if (_movementType != value)
            {
                _movementType = value;
                this._movement = null;
                this.NotifyPropertyChanged();
            }
        }
    }

    private IMovement _movement;
    public IMovement Movement
    {
        get
        {
            if (_movement == null)
            {
                Type[] allTypes = Utilities.FindDerivedTypes(typeof(IMovement)).ToArray();
                Type thisType = allTypes.First(t => t.Name == MovementType);

                _movement = (IMovement)Activator.CreateInstance(thisType);
            }

            return _movement;
        }
    }

    public override Component Clone()
    {
        return new MovementComponent()
        {
            MovementType = MovementType,
        };
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        // No interpolation really needed
    }

    public override int Populate(byte[] data, int offset)
    {
        int initialOffset = offset;
        int movementTypeLength = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.MovementType = Encoding.UTF8.GetString(data, offset, movementTypeLength);
        offset += movementTypeLength;
        return offset - initialOffset;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(MovementType.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(MovementType));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"MovementType={MovementType}";
    }

    public override void UpdateComponent(Component newComponent)
    {
        MovementComponent mc = newComponent as MovementComponent;

        this.MovementType = mc.MovementType;
    }

    public override int GetHashCode()
    {
        return this.MovementType.GetHashCode();
    }

    public override void ApplyInput(UserCommand command)
    {

    }
}
using System.Numerics;
using AGame.Engine.Networking;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CNType.Update, NDirection.ClientToServer, IsReliable = false)]
public class PlayerInputComponent : Component
{
    public const int KEY_W = 1 << 0;
    public const int KEY_A = 1 << 1;
    public const int KEY_S = 1 << 2;
    public const int KEY_D = 1 << 3;
    public const int KEY_SPACE = 1 << 4;

    public int PreviousKeyBitmask { get; set; }
    private int _keyBitmask;
    public int KeyBitmask
    {
        get => _keyBitmask;
        set
        {
            if (_keyBitmask != value)
            {
                _keyBitmask = value;
                this.NotifyPropertyChanged();
            }
        }
    }
    public int NewBitmask { get; set; }

    private Vector2 _currentMousePosition;
    public Vector2 CurrentMousePosition
    {
        get => _currentMousePosition;
        set
        {
            if (_currentMousePosition != value)
            {
                _currentMousePosition = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public PlayerInputComponent()
    {
        KeyBitmask = 0;
    }

    public void SetKeyDown(int key)
    {
        KeyBitmask |= key;
    }

    public void SetKeyUp(int key)
    {
        KeyBitmask &= ~key;
    }

    public bool IsKeyDown(int key)
    {
        return (KeyBitmask & key) != 0;
    }

    public bool IsKeyPressed(int key)
    {
        return (PreviousKeyBitmask & key) == 0 && (KeyBitmask & key) != 0;
    }

    public override Component Clone()
    {
        return new PlayerInputComponent()
        {
            KeyBitmask = KeyBitmask,
            PreviousKeyBitmask = PreviousKeyBitmask,
            NewBitmask = NewBitmask,
            CurrentMousePosition = CurrentMousePosition
        };
    }

    public override int Populate(byte[] data, int offset)
    {
        KeyBitmask = BitConverter.ToInt32(data, offset);
        CurrentMousePosition = new Vector2(BitConverter.ToSingle(data, offset + 4), BitConverter.ToSingle(data, offset + 8));
        return sizeof(int) + 2 * sizeof(float);
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(KeyBitmask));
        bytes.AddRange(BitConverter.GetBytes(CurrentMousePosition.X));
        bytes.AddRange(BitConverter.GetBytes(CurrentMousePosition.Y));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return "PlayerInputComponent";
    }

    public override void UpdateComponent(Component newComponent)
    {
        this.NewBitmask = ((PlayerInputComponent)newComponent).KeyBitmask;
        this.CurrentMousePosition = ((PlayerInputComponent)newComponent).CurrentMousePosition;
    }

    public override void InterpolateProperties()
    {

    }
}
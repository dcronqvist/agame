using System.Numerics;
using AGame.Engine.Networking;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CNType.Update, NDirection.ClientToServer, IsReliable = false)]
public class KeyboardInputComponent : Component
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

    public KeyboardInputComponent()
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
        return new KeyboardInputComponent()
        {
            KeyBitmask = KeyBitmask,
            PreviousKeyBitmask = PreviousKeyBitmask,
            NewBitmask = NewBitmask
        };
    }

    public override int Populate(byte[] data, int offset)
    {
        _keyBitmask = BitConverter.ToInt32(data, offset);
        return sizeof(int);
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(KeyBitmask));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return "KeyboardInputComponent";
    }

    public override void UpdateComponent(Component newComponent)
    {
        this.NewBitmask = ((KeyboardInputComponent)newComponent).KeyBitmask;
    }

    public override void InterpolateProperties()
    {

    }
}
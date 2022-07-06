using System.Numerics;
using AGame.Engine.Networking;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CNType.Update, NDirection.ClientToServer, IsReliable = false, MaxUpdatesPerSecond = 20)]
public class MouseInputComponent : Component
{
    private Vector2 _mousePosition;
    public Vector2 MousePosition
    {
        get => _mousePosition;
        set
        {
            if (_mousePosition != value)
            {
                _mousePosition = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public MouseInputComponent()
    {

    }

    public override Component Clone()
    {
        return new MouseInputComponent()
        {
            MousePosition = this.MousePosition
        };
    }

    public override int Populate(byte[] data, int offset)
    {
        this._mousePosition = new Vector2(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + 4));
        return sizeof(float) * 2;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(MousePosition.X));
        bytes.AddRange(BitConverter.GetBytes(MousePosition.Y));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return "MouseInputComponent";
    }

    public override void UpdateComponent(Component newComponent)
    {
        MouseInputComponent newMouseInputComponent = (MouseInputComponent)newComponent;
        this._mousePosition = newMouseInputComponent.MousePosition;
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        // No interpolation really needed
    }

    public override int GetHashCode()
    {
        return this.MousePosition.GetHashCode();
    }

    public override void ApplyInput(UserCommand command)
    {

    }
}
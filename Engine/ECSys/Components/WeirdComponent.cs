using AGame.Engine.Networking;

namespace AGame.Engine.ECSys.Components;

[NetworkingBehaviour(NBType.Update)]
public class WeirdComponent : Component
{
    public float Direction { get; set; }

    public override Component Clone()
    {
        return new WeirdComponent()
        {
            Direction = this.Direction
        };
    }

    public override void InterpolateProperties()
    {

    }

    public override int Populate(byte[] data, int offset)
    {
        int bytesRead = 0;
        this.Direction = BitConverter.ToSingle(data, offset + bytesRead);
        bytesRead += sizeof(float);
        return bytesRead;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.Direction));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return "WeirdComponent";
    }

    public override void UpdateComponent(Component newComponent)
    {
        WeirdComponent newWeirdComponent = newComponent as WeirdComponent;
        this.Direction = newWeirdComponent.Direction;
    }
}
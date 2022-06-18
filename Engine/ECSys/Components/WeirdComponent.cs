using AGame.Engine.Networking;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CNType.Update, NDirection.ServerToClient, MaxUpdatesPerSecond = 1)]
public class WeirdComponent : Component
{
    public float Direction { get; set; }
    public float TimeAlive { get; set; }

    public event EventHandler<TECEventArgsEmpty> Testing;

    public override Component Clone()
    {
        WeirdComponent wc = new WeirdComponent()
        {
            Direction = this.Direction,
            TimeAlive = this.TimeAlive
        };

        wc.Testing += (sender, e) =>
        {
            Console.WriteLine("I am a WeirdComponent and I received the Testing event!");
        };

        return wc;
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
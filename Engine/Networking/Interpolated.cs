using System.Numerics;
using System.Text.Json.Serialization;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public abstract class Interpolated<T> : IPacketable
{
    public T TargetValue { get; set; }
    public T CurrentValue { get; set; }
    public float InterpolationFactor { get; set; }

    [JsonConstructor]
    public Interpolated()
    {
        this.CurrentValue = default(T);
        this.TargetValue = default(T);
        this.InterpolationFactor = 1f;
    }

    public Interpolated(T initialValue, float interpolationFactor)
    {
        this.TargetValue = initialValue;
        this.CurrentValue = initialValue;
        this.InterpolationFactor = interpolationFactor;
    }

    public abstract void Update(float deltaTime);
    public abstract byte[] ToBytes();
    public abstract int Populate(byte[] data, int offset);
}

public class InterpolatedVector2 : Interpolated<Vector2>
{
    [JsonConstructor]
    public InterpolatedVector2()
    {

    }

    public InterpolatedVector2(Vector2 initialValue, float interpolationFactor) : base(initialValue, interpolationFactor)
    {

    }

    public override int Populate(byte[] data, int offset)
    {
        int bytesRead = 0;
        this.TargetValue = new Vector2(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + 4));
        this.CurrentValue = this.TargetValue;
        bytesRead += 8;
        this.InterpolationFactor = BitConverter.ToSingle(data, offset + 8);
        bytesRead += 4;
        return bytesRead;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.TargetValue.X));
        bytes.AddRange(BitConverter.GetBytes(this.TargetValue.Y));
        bytes.AddRange(BitConverter.GetBytes(this.InterpolationFactor));
        return bytes.ToArray();
    }

    public override void Update(float deltaTime)
    {
        this.CurrentValue += (this.TargetValue - this.CurrentValue) * this.InterpolationFactor * deltaTime;
    }
}
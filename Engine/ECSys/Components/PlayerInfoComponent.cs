using System.Text;
using AGame.Engine.Networking;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CNType.Update, NDirection.ServerToClient)]
public class PlayerInfoComponent : Component
{
    public string Name { get; set; }

    public override Component Clone()
    {
        return new PlayerInfoComponent()
        {
            Name = this.Name ?? ""
        };
    }

    public override int GetHashCode()
    {
        return this.Name.GetHashCode();
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        // No interpolation really needed
    }

    public override int Populate(byte[] data, int offset)
    {
        int startOffset = offset;

        int length = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Name = Encoding.UTF8.GetString(data, offset, length);
        offset += length;

        return offset - startOffset;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(this.Name.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.Name));

        return bytes.ToArray();
    }

    public override string ToString()
    {
        return "PlayerInfo";
    }

    public override void UpdateComponent(Component newComponent)
    {
        PlayerInfoComponent newPlayerInfoComponent = newComponent as PlayerInfoComponent;

        this.Name = newPlayerInfoComponent.Name;
    }

    public override void ApplyInput(UserCommand command)
    {

    }
}
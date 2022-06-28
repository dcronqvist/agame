using System.Text;
using AGame.Engine.Networking;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CNType.Update, NDirection.ServerToClient | NDirection.ClientToServer, MaxUpdatesPerSecond = 20)]
public class AudioComponent : Component
{
    private Queue<string> _audioQueue;

    public AudioComponent()
    {
        this._audioQueue = new Queue<string>();
    }

    public void EnqueueAudio(string audio)
    {
        this._audioQueue.Enqueue(audio);
        this.NotifyPropertyChanged(nameof(_audioQueue));
    }

    public string DequeueAudio()
    {
        this.NotifyPropertyChanged(nameof(_audioQueue));
        return this._audioQueue.Dequeue();
    }

    public bool HasAudio()
    {
        return this._audioQueue.Count > 0;
    }

    public override Component Clone()
    {
        return new AudioComponent()
        {
            _audioQueue = this._audioQueue
        };
    }

    public override void InterpolateProperties()
    {
    }

    public override int Populate(byte[] data, int offset)
    {
        this._audioQueue = new Queue<string>();
        int startOffset = offset;
        int count = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        for (int i = 0; i < count; i++)
        {
            int length = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            string audio = Encoding.UTF8.GetString(data, offset, length);
            offset += length;
            this._audioQueue.Enqueue(audio);
        }
        return offset - startOffset;
    }

    public override byte[] ToBytes()
    {
        string[] audioQueue = this._audioQueue.ToArray();
        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(audioQueue.Length));

        foreach (string audio in audioQueue)
        {
            bytes.AddRange(BitConverter.GetBytes(audio.Length));
            bytes.AddRange(Encoding.UTF8.GetBytes(audio));
        }

        return bytes.ToArray();
    }

    public override string ToString()
    {
        return "AudioComponent";
    }

    public override void UpdateComponent(Component newComponent)
    {
        this._audioQueue = ((AudioComponent)newComponent)._audioQueue;
    }
}
using AGame.Engine.Configuration;
using OpenTK.Audio.OpenAL;

namespace AGame.Engine.Assets;

public class Audio : Asset
{
    public enum Priority
    {
        LOW,
        MEDIUM,
        HIGH
    }

    private byte[] _data;
    private int _channels;
    private int _bitsPerSample;
    private int _sampleRate;

    public int BufferID { get; private set; }

    public Audio(byte[] wavData, int channels, int bitsPerSample, int sampleRate)
    {
        this._data = wavData;
        this._channels = channels;
        this._bitsPerSample = bitsPerSample;
        this._sampleRate = sampleRate;
    }

    private ALFormat GetSoundFormat(int channels, int bits)
    {
        switch (channels)
        {
            case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
            case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
            default: throw new NotSupportedException("The specified sound format is not supported.");
        }
    }

    public unsafe override bool InitOpenGL()
    {
        int buffer = AL.GenBuffer();

        fixed (byte* data = &_data[0])
        {
            AL.BufferData(buffer, GetSoundFormat(_channels, _bitsPerSample), data, _data.Length, _sampleRate);
        }

        this.BufferID = buffer;

        this._data = null;

        return true;
    }

    // Stuff related to playback
    static Queue<int> _sources;

    public unsafe static void Init()
    {
        AL.GetError();

        ALDevice device = ALC.OpenDevice(null);

        ALError error;
        if ((error = AL.GetError()) != ALError.NoError)
        {
            Console.WriteLine(AL.GetErrorString(error));
        }

        ALContext context = ALC.CreateContext(device, (int*)null);

        if ((error = AL.GetError()) != ALError.NoError)
        {
            Console.WriteLine(AL.GetErrorString(error));
        }

        ALC.MakeContextCurrent(context);
        if ((error = AL.GetError()) != ALError.NoError)
        {
            Console.WriteLine(AL.GetErrorString(error));
        }

        var version = AL.Get(ALGetString.Version);
        var vendor = AL.Get(ALGetString.Vendor);
        var renderer = AL.Get(ALGetString.Renderer);

        _sources = new Queue<int>();

        int sources = 5;

        for (int i = 0; i < sources; i++)
        {
            int source = AL.GenSource();
            _sources.Enqueue(source);
        }
    }

    private int GetNextSource()
    {
        return _sources.Dequeue();
    }

    private void ReturnSource(int source)
    {
        _sources.Enqueue(source);
    }

    public static void Play(string audioName, float pitch = 1.0f)
    {
        Audio audio = AssetManager.GetAsset<Audio>(audioName);
        audio.Play(pitch);
    }

    public void Play(float pitch = 1.0f)
    {
        int source = GetNextSource();
        AL.BindBufferToSource(source, this.BufferID);
        AL.Source(source, ALSourcef.Pitch, pitch);
        AL.SourcePlay(source);
        ReturnSource(source);
    }
}
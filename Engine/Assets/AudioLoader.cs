using System;
using System.Diagnostics;
using System.IO;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Assets
{
    class AudioLoader : IAssetLoader
    {
        public string AssetPrefix()
        {
            return "audio";
        }

        public Asset LoadAsset(Stream fileStream)
        {
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riffChunkSize = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string formatSignature = new string(reader.ReadChars(4));
                if (formatSignature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int formatChunkSize = reader.ReadInt32();
                int audioFormat = reader.ReadInt16();
                int numChannels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                int byteRate = reader.ReadInt32();
                int blockAlign = reader.ReadInt16();
                int bitsPerSample = reader.ReadInt16();

                string dataSignature = new string(reader.ReadChars(4));
                if (dataSignature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int dataChunkSize = reader.ReadInt32();

                byte[] soundData = reader.ReadBytes((int)reader.BaseStream.Length);
                return new Audio(soundData, numChannels, bitsPerSample, sampleRate);
            }
        }
    }
}
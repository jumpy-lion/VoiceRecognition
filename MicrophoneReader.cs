using System.Text;
using OpenTK.Audio.OpenAL;

namespace VoiceRecognition;

public class MicrophoneReader
{
    public bool ShouldRun { get; set; } = true;
    private const int SampleRate = 16000;
    private const ALFormat Format = ALFormat.Mono16;
    private const int BufferSize = SampleRate / 2;
    private static readonly char[] WavHeaderRiff = ['R', 'I', 'F', 'F'];
    private static readonly char[] WavHeaderWave = ['W', 'A', 'V', 'E'];
    private static readonly char[] WavHeaderFmt = ['f', 'm', 't', ' '];
    private static readonly char[] WavHeaderData = ['d', 'a', 't', 'a'];

    public void Start(Stream output)
    {
        var intermediateStream = new MemoryStream();
        var thread = new Thread(() =>
        {
            var device = ALC.CaptureOpenDevice(null, SampleRate, Format, BufferSize);
            ALC.CaptureStart(device);
            var buffer = new byte[SampleRate];
            var totalSampleCount = 0;
            while (ShouldRun)
            {
                var samples = ALC.GetInteger(device, AlcGetInteger.CaptureSamples);
                ALC.CaptureSamples(device, buffer, samples);
                intermediateStream.Write(buffer, 0, samples * sizeof(short));
                totalSampleCount += samples;
            }

            ALC.CaptureStop(device);
            ALC.CaptureCloseDevice(device);
            WriteWavHeader(output, 1, totalSampleCount);
            intermediateStream.Position = 0;
            intermediateStream.CopyTo(output);
        });
        thread.Start();
    }

    private static void WriteWavHeader(Stream stream, ushort channelCount, int totalSampleCount)
    {
        const ushort bitDepth = 16;

        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(WavHeaderRiff);
        writer.Write(44 + totalSampleCount * bitDepth / 8);
        writer.Write(WavHeaderWave);
        writer.Write(WavHeaderFmt);
        writer.Write(16);
        writer.Write((ushort)1);
        writer.Write(channelCount);
        writer.Write(SampleRate);
        writer.Write(SampleRate * channelCount * bitDepth / 8);
        writer.Write((ushort)(channelCount * bitDepth / 8));
        writer.Write(bitDepth);
        writer.Write(WavHeaderData);
        writer.Write(totalSampleCount * bitDepth / 8);
    }
}
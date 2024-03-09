using FFMpegCore.Extend;
using FFMpegCore.Pipes;
using OpenTK.Audio.OpenAL;

namespace VoiceRecognition;

public class MicrophoneReader
{
    public bool ShouldRun { get; set; } = true;

    private const int SampleRate = 16000;
    private const ALFormat Format = ALFormat.Mono16;
    private const int BufferSize = SampleRate/2;
    private FileStream Output { get; }

    public MicrophoneReader(string destination)
    {
        Output = File.Open(destination, FileMode.Create);
        var thread = new Thread(() =>
        {
            var device = ALC.CaptureOpenDevice(null, SampleRate, Format, BufferSize);
            ALC.CaptureStart(device);
            var buffer = new byte[SampleRate];
            while (ShouldRun)
            {
                var samples = ALC.GetInteger(device, AlcGetInteger.CaptureSamples);
                ALC.CaptureSamples(device, buffer, samples);
                Output.Write(buffer, 0, samples*sizeof(short));
            }
            ALC.CaptureStop(device);
            ALC.CaptureCloseDevice(device);
        });
        thread.Start();
    }
}
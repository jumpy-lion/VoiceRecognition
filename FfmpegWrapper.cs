using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using OpenTK.Audio.OpenAL;

namespace VoiceRecognition;

public class FfmpegWrapper(string source, string result)
{
    private FileInfo Source { get; set; } = new(source);
    private string Result { get; set; } = result;

    public Task<bool> ConversionTask()
    {
        return FFMpegArguments
            .FromFileInput(Source, options => options
                .ForceFormat("s16le")
                .WithAudioSamplingRate(16000)
            )
            .OutputToFile(Result, addArguments: options => options
                .WithAudioSamplingRate(16000) // Whisper oczekuje częstotliwości próbkowania 16KHz
            )
            .ProcessAsynchronously();
    }
}
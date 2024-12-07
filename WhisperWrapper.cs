using Whisper.net;

namespace VoiceRecognition;

public class WhisperWrapper(string modelPath, string language="auto")
{
    private WhisperProcessor Processor { get; } = WhisperFactory
        .FromPath(modelPath)
        .CreateBuilder()
        .WithLanguage(language)
        .Build();

    public async IAsyncEnumerable<string> ProcessStream(Stream stream)
    {
        await foreach (var detected in Processor.ProcessAsync(stream))
        {
            yield return $"{detected.Start}->{detected.End}: {detected.Text}";
        }

    }
}
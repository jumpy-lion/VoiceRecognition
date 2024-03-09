using Whisper.net;

namespace VoiceRecognition;

public class WhisperWrapper(string modelPath, string language="auto")
{
    private WhisperProcessor Processor { get; } = WhisperFactory
        .FromPath(modelPath)
        .CreateBuilder()
        .WithLanguage(language)
        .Build();

    public async IAsyncEnumerable<string> ProcessFile(FileStream fileStream)
    {
        await foreach (var detected in Processor.ProcessAsync(fileStream))
        {
            yield return $"{detected.Start}->{detected.End}: {detected.Text}";
        }

    }
}
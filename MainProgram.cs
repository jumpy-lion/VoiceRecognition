using Whisper.net;
using Whisper.net.Ggml;

namespace VoiceRecognition;

internal static class MainProgram
{
    public static async Task Main(string[] args)
    {
        

        var dataDirectory = Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/VoiceRecognitionData");
        var modelPath = $"{dataDirectory.FullName}/model.bin";
        if (!File.Exists(modelPath))
        {
            using var client = new HttpClient();
            var netStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
            var fileStream = File.Open(modelPath, FileMode.Create);
            await netStream.CopyToAsync(fileStream);
        }
        Console.WriteLine("Czytam wejście z mikrofonu, wciśnij enter, żeby zakończyć...");
        var inputFilePath = $"{dataDirectory.FullName}/input.raw";
        var reader = new MicrophoneReader(inputFilePath);
        Console.ReadLine();
        reader.ShouldRun = false;
        var outputFile = $"{dataDirectory.FullName}/output.wav";
        var wrapper = new FfmpegWrapper
        (
            inputFilePath,
            outputFile
        );
        await wrapper.ConversionTask();
        Console.WriteLine("Rozpoznaję mowę...:");
        using var whisperFactory = WhisperFactory.FromPath(modelPath);
        await using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("pl")
            .Build();
        var readFile = File.OpenRead(outputFile);
        await foreach (var result in processor.ProcessAsync(readFile))
        {
            Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
        }
    }

}
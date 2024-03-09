using Whisper.net.Ggml;

namespace VoiceRecognition;

internal static class MainProgram
{
    public static async Task Main(string[] args)
    {
        

        var dataDirectory = Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/VoiceRecognitionData");
        var modelPath = $"{dataDirectory.FullName}/model.bin";
        Task? modelDownloadStream = null;
        if (!File.Exists(modelPath))
        {
            Console.WriteLine("Rozpoczynam pobieranie modelu w tle...");
            var netStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Medium);
            var fileStream = File.Open(modelPath, FileMode.Create);
            modelDownloadStream = netStream.CopyToAsync(fileStream);
        }
        Console.Write("Czytam wejście z mikrofonu, wciśnij enter, żeby zakończyć...");
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
        if (modelDownloadStream != null)
        {
            Console.WriteLine("Czekam, aż model zostanie pobrany...");
            await modelDownloadStream;
        }
        Console.WriteLine("Rozpoznaję mowę...:");
        File.OpenRead(outputFile);
        var whisperWrapper = new WhisperWrapper(modelPath, "pl");
        await foreach (var line in whisperWrapper.ProcessFile(File.OpenRead(outputFile)))
        {
            Console.WriteLine(line);
        }
    }

}
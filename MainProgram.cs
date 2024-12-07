using Whisper.net.Ggml;

namespace VoiceRecognition;

internal static class MainProgram
{
    public static async Task Main(string[] args)
    {
        

        var dataDirectory = Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/VoiceRecognitionData");
        var modelPath = $"{dataDirectory.FullName}/model.bin";
        Task? modelDownloadStream = null;
        FileStream? fileStream = null;
        if (!File.Exists(modelPath))
        {
            Console.WriteLine("Rozpoczynam pobieranie modelu w tle...");
            var netStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Medium);
            fileStream = File.Open(modelPath, FileMode.Create);
            modelDownloadStream = netStream.CopyToAsync(fileStream);
        }
        Console.Write("Czytam wejście z mikrofonu, wciśnij enter, żeby zakończyć...");
        var reader = new MicrophoneReader();
        var memoryStream = new MemoryStream();
        reader.Start(memoryStream);
        Console.ReadLine();
        reader.ShouldRun = false;
        if (modelDownloadStream != null)
        {
            Console.WriteLine("Czekam, aż model zostanie pobrany...");
            while (!modelDownloadStream.IsCompleted)
            {
            }
            fileStream!.Close();
        }
        Console.WriteLine("Rozpoznaję mowę...:");
        var whisperWrapper = new WhisperWrapper(modelPath, "pl");
        memoryStream.Position = 0;
        await foreach (var line in whisperWrapper.ProcessStream(memoryStream))
        {
            Console.WriteLine(line);
        }
    }

}
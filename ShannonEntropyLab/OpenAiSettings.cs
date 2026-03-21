namespace ShannonEntropyLab
{
    using System.Text.Json;

    /// <summary>
    /// Azure OpenAI 接続設定。JSON ファイルに永続化する。
    /// </summary>
    internal sealed class OpenAiSettings
    {
        public string Endpoint { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string ApiVersion { get; set; } = "2024-02-15-preview";
        public string DeploymentName { get; set; } = "gpt-4";

        private static readonly string SettingsDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShannonEntropyLab");

        private static readonly string SettingsPath =
            Path.Combine(SettingsDir, "openai_settings.json");

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true
        };

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Endpoint) &&
            !string.IsNullOrWhiteSpace(ApiKey) &&
            !string.IsNullOrWhiteSpace(DeploymentName);

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                var json = JsonSerializer.Serialize(this, JsonOpts);
                File.WriteAllText(SettingsPath, json);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new IOException(
                    $"設定ファイルへの書き込み権限がありません。\nパス: {SettingsPath}", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new IOException(
                    $"設定ディレクトリを作成できませんでした。\nパス: {SettingsDir}", ex);
            }
        }

        public static OpenAiSettings Load()
        {
            if (!File.Exists(SettingsPath))
                return new OpenAiSettings();

            try
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<OpenAiSettings>(json) ?? new OpenAiSettings();
            }
            catch
            {
                return new OpenAiSettings();
            }
        }
    }
}

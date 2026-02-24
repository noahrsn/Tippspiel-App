using System.Text.Json;
using TippspielApp.Models;

namespace TippspielApp.Services
{
    public class DataHandler
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        /// <summary>Lädt alle User-Tipps aus einer JSON-Datei.</summary>
        public List<User> LoadUsers(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"users.json nicht gefunden: {filePath}");

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<User>>(json, _options)
                   ?? throw new InvalidDataException("users.json konnte nicht deserialisiert werden.");
        }

        /// <summary>Lädt die aktuellen Turnierdaten aus einer JSON-Datei.</summary>
        public TournamentData LoadTournamentData(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"tournament_data.json nicht gefunden: {filePath}");

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<TournamentData>(json, _options)
                   ?? throw new InvalidDataException("tournament_data.json konnte nicht deserialisiert werden.");
        }

        /// <summary>Exportiert das berechnete Ranking in eine JSON-Datei.</summary>
        public void ExportRanking(RankingReport report, string filePath)
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(report, _options);
            File.WriteAllText(filePath, json);
        }
    }
}
using System.Text.Json;
using TippspielApp.Domain;

namespace TippspielApp.Infrastructure
{
    /// <summary>
    /// Verantwortlich für alle JSON-Datei-Operationen: Lesen und Schreiben.
    /// Kapselt System.Text.Json-Konfiguration an einem Ort.
    /// </summary>
    public class JsonDataStore
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented               = true
        };

        /// <summary>Lädt alle User-Tipps aus einer JSON-Datei.</summary>
        public List<User> LoadUsers(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"users.json nicht gefunden: {filePath}");

            return JsonSerializer.Deserialize<List<User>>(File.ReadAllText(filePath), Options)
                   ?? throw new InvalidDataException("users.json konnte nicht deserialisiert werden.");
        }

        /// <summary>Lädt die aktuellen Turnierdaten aus einer JSON-Datei.</summary>
        public TournamentData LoadTournamentData(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"tournament_data.json nicht gefunden: {filePath}");

            return JsonSerializer.Deserialize<TournamentData>(File.ReadAllText(filePath), Options)
                   ?? throw new InvalidDataException("tournament_data.json konnte nicht deserialisiert werden.");
        }

        /// <summary>Exportiert das berechnete Ranking als JSON-Datei.</summary>
        public void ExportRanking(RankingReport report, string filePath)
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(filePath, JsonSerializer.Serialize(report, Options));
        }
    }
}

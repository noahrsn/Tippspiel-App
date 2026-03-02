using System.Text.Json;
using TippspielApp.Domain;

namespace TippspielApp.Infrastructure
{
    /// <summary>
    /// Kümmert sich um alles rund ums Lesen und Schreiben der JSON-Dateien.
    /// Die Serializer-Optionen sind einmal hier definiert und werden überall genutzt.
    /// </summary>
    public class JsonDataStore
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented               = true
        };

        /// <summary>Liest die komplette User-Liste aus der JSON-Datei – wirft Exception wenn die Datei fehlt.</summary>
        public List<User> LoadUsers(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"users.json nicht gefunden: {filePath}");

            // ?? throw: Deserialize gibt null zurück wenn das JSON leer oder ungültig ist
            return JsonSerializer.Deserialize<List<User>>(File.ReadAllText(filePath), Options)
                   ?? throw new InvalidDataException("users.json konnte nicht deserialisiert werden.");
        }

        /// <summary>Liest die Turnierdaten ein – MatchResults, Bingo-Events, KO-Teams, usw.</summary>
        public TournamentData LoadTournamentData(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"tournament_data.json nicht gefunden: {filePath}");

            return JsonSerializer.Deserialize<TournamentData>(File.ReadAllText(filePath), Options)
                   ?? throw new InvalidDataException("tournament_data.json konnte nicht deserialisiert werden.");
        }

        // Schreibt die aktuelle User-Liste zurück – überschreibt die Datei komplett
        public void SaveUsers(List<User> users, string filePath)
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(users, Options), System.Text.Encoding.UTF8);
        }

        /// <summary>Speichert den fertigen Report als JSON – legt das Output-Verzeichnis an falls nötig.</summary>
        public void ExportRanking(RankingReport report, string filePath)
        {
            string? dir = Path.GetDirectoryName(filePath);
            // CreateDirectory macht nichts wenn der Ordner schon existiert – kein Extra-Check nötig
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(filePath, JsonSerializer.Serialize(report, Options));
        }
    }
}

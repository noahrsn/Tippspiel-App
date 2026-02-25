using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>Gemeinsame Schnittstelle für alle Punkt-Evaluatoren.</summary>
    public interface IEvaluator
    {
        void Evaluate(User user, TournamentData data);
    }
}

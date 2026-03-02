using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>Interface für alle Evaluatoren – jeder muss Evaluate() implementieren.</summary>
    public interface IEvaluator
    {
        void Evaluate(User user, TournamentData data);
    }
}

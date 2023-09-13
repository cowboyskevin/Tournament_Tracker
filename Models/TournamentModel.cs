using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// represents one tournament with all of the rounds,matchups, prizes and outcomes
    /// </summary>
    public class TournamentModel
    {
        public event EventHandler<DateTime> OntournamentComplete;

        /// <summary>
        /// The name given to id that returns from SQL
        /// </summary>
        public int Id { get; set;}
        /// <summary>
        /// The name given to the tournament
        /// </summary>
        public string TournamentName { get; set; }
        /// <summary>
        /// The fee to enter the tournament
        /// </summary>
        public decimal EntryFee { get; set; }
        /// <summary>
        ///  The set of teams that have entered
        /// </summary>
        public List<TeamModel> EnteredTeams { get; set; } = new List<TeamModel>();
        /// <summary>
        /// The list of prizes for the various teams
        /// </summary>
        public List<PrizeModel> Prizes { get; set; } = new List<PrizeModel>();
        /// <summary>
        /// The matchups per round.
        /// </summary>
        public List<List<MatchupModel>> Rounds { get; set; } = new List<List<MatchupModel>>();

        public void CompleteTournament()
        {
            OntournamentComplete?.Invoke(this, DateTime.Now);
        }
    }
}

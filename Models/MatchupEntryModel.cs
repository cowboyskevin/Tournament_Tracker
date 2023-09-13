using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{ /// <summary>
/// Represents one team in the matchup
/// </summary>
   public class MatchupEntryModel
    {
        /// <summary>
        /// Has unique identifier for the matchup entry
        /// </summary>
        public int Id { get; set;}
        /// <summary>
        /// The unique identifier for the team
        /// </summary>
        public int TeamCompetingId  { get; set; }
        /// <summary>
        /// Represents one team in the matchup
        /// </summary>
        public TeamModel TeamCompeting { get; set; }
        /// <summary>
        /// Represents the score that this team came from
        /// </summary>
        public double Score { get; set; }
        /// <summary>
        /// THe unique identifier for the parentMatchup
        /// </summary>
        public int ParentMatchupId { get; set; }
        /// <summary>
        ///   Represents the matchup that this team came from as the winner
        /// </summary>
        public MatchupModel ParentMatchup { get; set; }

    }
}

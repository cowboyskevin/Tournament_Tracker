using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackerlibrary.DataAccess.TextHelpers;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace Tournament_Tracker
{
    public static class TournamentLogic
    {
        // Order our list Randomly of teams
        //Check if it is big enough - if not, add in bytes 2*2*2*2 - 2^4
        //Create our first round of matchups
        // Create every round after that - 8 matchups -4 matchups - 2 matchups -1 matchup


        public static void UpdateTournamentResults(TournamentModel model)
        {

            int startingRound = model.CheckCurrentRound();
            List<MatchupModel> toScore = new List<MatchupModel>();
            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel rm in round)
                {
                    if (rm.Winner == null && rm.Entries.Any(x => x.Score != 0) || rm.Entries.Count == 1) // checking if any of the entries, that have a score not equal to 0
                    {
                        toScore.Add(rm);
                    }
                }
            }
            MarWinnerMatchups(toScore);
            AdvanceWinners(toScore, model); // 
            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x)); // updating the winner into sql

            int EndingRound = model.CheckCurrentRound();

            if(EndingRound > startingRound)
            {
                // Alert users
                // EmailLogic.SendEmail()
                model.AlertUsersToNewRound();
            }
        }

        public static void AlertUsersToNewRound(this TournamentModel model)
        {
            int currentRoundNumber = model.CheckCurrentRound();
            List<MatchupModel> currentRound = model.Rounds.Where(x => x.First().MatchupRound == currentRoundNumber).First(); 

            foreach(MatchupModel matchup in currentRound)
            {
                foreach(MatchupEntryModel matchupEntry in matchup.Entries)
                {
                    foreach(PersonModel p in matchupEntry.TeamCompeting.TeamMembers)
                    {
                        AlertPersonToNewRound(p, matchupEntry.TeamCompeting.TeamName, matchup.Entries.Where(x => x.TeamCompeting != matchupEntry.TeamCompeting).FirstOrDefault());
                    }
                }
            }
        }

        private static void AlertPersonToNewRound(PersonModel p, string teamName, MatchupEntryModel OpposingTeam)
        {

            if(p.EmailAddress.Length < 0)
            {
                return;
            }

            string from = "";
            string to = "";
            string subject = "";
            StringBuilder body = new StringBuilder();

            if(OpposingTeam != null)
            {
                subject = $"You have a mew matchup with {OpposingTeam.TeamCompeting.TeamName}";
                body.AppendLine("<h1You have a new matchup /h1>");
                body.Append("<strong Competitor: strong/>");
                body.AppendLine(OpposingTeam.TeamCompeting.TeamName);
                body.AppendLine();
                body.AppendLine();
                body.AppendLine("Have a good time!");
                body.AppendLine("Tournament Tracker");
            }
            else
            {
                subject = "You have a bye week this round";

                body.AppendLine("Enjoy your week off");
                body.AppendLine("~Tournament Tracker");
            }
            EmailLogic.SendEmail(p.EmailAddress, subject, body.ToString());
        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;

            foreach(List<MatchupModel> round in model.Rounds)
            {
                if(round.All(x => x.Winner != null))
                {
                    output += 1;
                }
                else
                {
                    return output;
                }
            }
            // Tournament is complete
            CompleteTournament(model);
            return -1;
        }

        public static void CompleteTournament(TournamentModel model)
        {
            GlobalConfig.Connection.CompleteTournament(model);

            TeamModel Winners = model.Rounds.Last().First().Winner;
            TeamModel Runnerup = model.Rounds.Last().First().Entries.Where(x => x.TeamCompeting != Winners).First().TeamCompeting;

            decimal WinnerPrize = 0;
            decimal RunnerUpPrize = 0;
            if(model.Prizes.Count > 0)
            {
                decimal totalIncome = model.EnteredTeams.Count * model.EntryFee;


                PrizeModel FirstPlace = model.Prizes.Where(x => x.PlaceNumber == 1).FirstOrDefault();
                PrizeModel SecondPlace = model.Prizes.Where(x => x.PlaceNumber == 2).FirstOrDefault();
                if (FirstPlace !=  null)
                {
                    WinnerPrize = CalculatePrizePayout(FirstPlace, totalIncome);
                }
                if (SecondPlace != null)
                {
                    RunnerUpPrize = CalculatePrizePayout(SecondPlace, totalIncome);
                }

            }
            // Send email to all tournament
            string from = "";
            string to = "";
            string subject = "";
            StringBuilder body = new StringBuilder();


            subject = $"In {model.TournamentName}, {Winners.TeamName} has won";
            body.AppendLine("<h1 We have a winnner /h1>");
            body.AppendLine("<p> Congratulations </p>");
            body.AppendLine("<br />");

            if(WinnerPrize > 0)
            {
                body.AppendLine($"<p> {Winners.TeamName} will receieve ${WinnerPrize}</p> ");
            }
            if(RunnerUpPrize > 0)
            {
                body.AppendLine($"<p> {Runnerup.TeamName} will receieve ${RunnerUpPrize}</p> ");
            }

            body.AppendLine("<p> Thanks for a great tournament everyone!</p>");
            body.AppendLine("Tournament Tracker");


            List<string> bcc = new List<string>();

            foreach(PersonModel p in Winners.TeamMembers)
            {
                if(p.EmailAddress.Length > 0)
                {
                    bcc.Add(p.EmailAddress);
                }
            }


            EmailLogic.SendEmail(new List<string>(), bcc, subject, body.ToString());

            model.CompleteTournament();
        }
        private static decimal CalculatePrizePayout(PrizeModel prize, decimal totalIncome)
        {
            decimal output = 0; 
           if(prize.PrizeAmount > 0)
           {
                output = prize.PrizeAmount;
           }
           else
           {
                output = Decimal.Multiply(totalIncome, Convert.ToDecimal(prize.PrizePercentage / 100));
           }
            return output;
        }


        private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
        {
            foreach (MatchupModel m  in models)
            {
                foreach (List<MatchupModel> round in tournament.Rounds)
                {
                    foreach (MatchupModel rm in round)
                    {
                        foreach (MatchupEntryModel me in rm.Entries)
                        {
                            if (me.ParentMatchup != null)
                            {
                                if (me.ParentMatchup.Id == m.Id)
                                {
                                    me.TeamCompeting = m.Winner;
                                    GlobalConfig.Connection.UpdateMatchup(rm);
                                }
                            }
                        }
                    }
                } 
            }
        }
        private static void MarWinnerMatchups(List<MatchupModel> models)
        {
            // greater or lesser
            string greaterWins = ConfigurationManager.AppSettings["winnerDetermination"];

            // 0 means false, or low score wins
            foreach(MatchupModel m in models)
            {
                if (m.Entries.Count == 1)
                {
                    m.Winner = m.Entries[0].TeamCompeting;
                    continue;
                }
                // 0 means low score wins
                if (greaterWins == "0")
                {
                    if(m.Entries[0].Score < m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if(m.Entries[1].Score < m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else
                    {
                        throw new FormatException("We do not allow ties in this application");
                    }
                }
                else
                {
                    // 1 mean true or high score wins
                    if (m.Entries[0].Score > m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if (m.Entries[1].Score > m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else
                    {
                        throw new FormatException("We do not allow ties in this application");
                    }

                } 
            }
        }
        public static void CreateRounds(TournamentModel model) // Quarterback method
        {
            List<TeamModel> randomizedTeams = RandomizeTeamOrder(model.EnteredTeams);
            int rounds = findNumberOfRounds(randomizedTeams.Count);
            int byes = NumberOfByes(rounds,randomizedTeams.Count);

            model.Rounds.Add(CreateFirstRound(byes, randomizedTeams));
            createOtherRounds(model, rounds);

            UpdateTournamentResults(model);
        }
        private static void createOtherRounds(TournamentModel model, int rounds) 
        {
            int round = 2; // This variable is for current round
            List<MatchupModel> previousRound = model.Rounds[0];
            List<MatchupModel> currentRound = new List<MatchupModel>();
            MatchupModel currMatchup = new MatchupModel();

            
            while(round <= rounds)
            {
                foreach(MatchupModel match in previousRound)
                {
                    currMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match });

                    if(currMatchup.Entries.Count > 1)
                    {
                        currMatchup.MatchupRound = round;
                        currentRound.Add(currMatchup);
                        currMatchup = new MatchupModel();
                    }
                }
                model.Rounds.Add(currentRound);
                previousRound = currentRound;

                currentRound = new List<MatchupModel>();
                round += 1;
            }
        }
        private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchupModel> output = new List<MatchupModel>();
            MatchupModel curr = new MatchupModel();

            foreach(TeamModel team in teams)
            {
                curr.Entries.Add(new MatchupEntryModel { TeamCompeting = team });
                if(byes > 0 || curr.Entries.Count > 1)
                {
                    curr.MatchupRound = 1; // This is hard coded since this is the first round
                    output.Add(curr);
                    curr = new MatchupModel(); // Reset for a new instance
                    if (byes > 0)
                    {
                        byes -= 1; // If the bye is how we got into this, get rid of the bye
                    }
                }
            }
            return output;
        }
        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            int output = 0;
            int totalTeams = 1;

            for(int i = 1; i <= rounds; i++)
            {
                totalTeams *= 2;
            }
            output = totalTeams - numberOfTeams;
            return output;
        }

        private static int findNumberOfRounds(int teamcount)
        {
            int output = 1;
            int val = 2;

            while (val < teamcount)  // Figure out how many rounds I need to go
            {
                output += 1;
                val *= 2; // if 4 team enter this value is = 4 since it will exist the for loop since teamCount isn't bigger
            }

            return output; // this is 1 round if teamcount is not bigger then 2
        }

        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
           return teams.OrderBy(a => Guid.NewGuid()).ToList(); // This sorts the element, and orders them  by teams
        }
       


    }
}

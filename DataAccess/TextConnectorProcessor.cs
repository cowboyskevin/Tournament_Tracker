using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace Trackerlibrary.DataAccess.TextHelpers
{
    public static class TextConnectorProcessor
    {
        public static string fullFilePath(this string fileName) // prizeModel.csv
        {
            //C:\data\TournamentTracker\PrizeModels.csv
            return $"{ ConfigurationManager.AppSettings["filepath"]}\\ {fileName}";
        }

        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }
            return File.ReadAllLines(file).ToList();
        }
        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            List<PersonModel> output = new List<PersonModel>();
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PersonModel p = new PersonModel();

                p.id = int.Parse(cols[0]);
                p.FirstName = cols[1];
                p.LastName = cols[2];
                p.EmailAddress = cols[3];
                p.CellPhoneNumber = cols[4];

                output.Add(p);
            }
            return output;
        }
        public static List<PrizeModel> ConverToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new List<PrizeModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PrizeModel p = new PrizeModel();

                p.id = int.Parse(cols[0]);
                p.PlaceName = cols[2];
                p.PlaceNumber = int.Parse(cols[1]);
                p.PrizeAmount = decimal.Parse(cols[3]);
                p.PrizePercentage = double.Parse(cols[4]);
                output.Add(p);
            }
            return output;

        }
        public static List<TeamModel> ConvertToTeamModels(this List<string> lines)
        {
          List<TeamModel> output = new List<TeamModel>();
          List<PersonModel> people = GlobalConfig.PersonFile.fullFilePath().LoadFile().ConvertToPersonModels();
            foreach (string line in lines )
            {
                string[] cols = line.Split(',');

                TeamModel p = new TeamModel();

                p.Id = int.Parse(cols[0]);
                p.TeamName = cols[1];

                string[] personIds = cols[2].Split('|');

                foreach(string id in personIds)
                {
                    p.TeamMembers.Add(people.Where(x => x.id == int.Parse(id)).First());
                }
                output.Add(p);
            }
            return output;

        }
        public static List<TournamentModel> ConvertToTournamentModel(
            this List<string> lines)
        {
            // id = 0, 
            //tournamentName = 1,
            // EntryFee = 2,
            //Enteredteams = 3
            // Prizes = 4,
            //Rounds = 5
            //id, TournamentName, EntryFee, (id|id|id - entered teams), (id|id|id - Prizes), (Rounds- id^id^id|id^id^id|id^id^id)
            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> team = GlobalConfig.teamFile.fullFilePath().LoadFile().ConvertToTeamModels();
            List<PrizeModel> prize = GlobalConfig.PrizesFile.fullFilePath().LoadFile().ConverToPrizeModels();
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.fullFilePath().LoadFile().ConverToMatchupModels();
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TournamentModel tm = new TournamentModel();

                tm.Id = int.Parse(cols[0]);
                tm.TournamentName = cols[1];
                tm.EntryFee = decimal.Parse(cols[2]);

                string[] teamIds = cols[3].Split('|');

                foreach(string id in teamIds)
                {
                    tm.EnteredTeams.Add(team.Where(x => x.Id == int.Parse(id)).First()); // adds the team ids
                }
                if (cols[4].Length > 0)
                {
                    string[] prizeIds = cols[4].Split('|');
                    foreach (string id in prizeIds)
                    {
                        tm.Prizes.Add(prize.Where(x => x.id == int.Parse(id)).First()); // adds the prize ids
                    } 
                }

                // Capture Rounds information
                string[] rounds = cols[5].Split('|');
                List<MatchupModel> ms = new List<MatchupModel>();
                foreach (string round in rounds)
                {
                    string[] msText = round.Split('^');

                    foreach(string matchupModelTextId in msText)
                    {
                        ms.Add(matchups.Where(x => x.Id == int.Parse(matchupModelTextId)).First());
                    }
                    tm.Rounds.Add(ms); // This adds the list of rounds
                }

                output.Add(tm); 
            }
            return output;
        }
        public static void SaveToPrizeFile(this List<PrizeModel> models)
        {
            List<string> lines = new List<string>();

            foreach (PrizeModel p in models)
            {
                lines.Add($"{ p.id},{p.PlaceNumber},{p.PlaceName},{p.PrizeAmount},{p.PrizePercentage}"); // a comma seperated line creation
            }
            File.WriteAllLines(GlobalConfig.PrizesFile.fullFilePath(), lines); //overwriting the file that is already there
        }

        public static void SaveToPeopleFile(this List<PersonModel> models)
        {
            List<string> lines = new List<string>();

            foreach(PersonModel p in models)
            {
                lines.Add($"{ p.id},{p.FirstName},{p.LastName},{p.EmailAddress},{p.CellPhoneNumber}");
            }

            File.WriteAllLines(GlobalConfig.PersonFile.fullFilePath(), lines); //overwriting the file that is already there
        }

        public static void SaveToTeamFile(this List<TeamModel> models)
        {
            List<string> lines = new List<string>();
            
            foreach(TeamModel t in models)
            {
                lines.Add($"{t.Id},{t.TeamName},{ConvertToPeopleList(t.TeamMembers)}"); 
            }
            File.WriteAllLines(GlobalConfig.teamFile.fullFilePath(), lines); //overwriting the file that is already there
        }

        public static List<MatchupEntryModel> ConvertToMatchupEntryModel(this List<string> input)
        {
            // id = 0. TeamCompeting = 1, score = 2, ParentMatchup = 3
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();
            foreach (string line in input)
            {
                string[] cols = line.Split(',');

                MatchupEntryModel me = new MatchupEntryModel();

                me.Id = int.Parse(cols[0]);
                if(cols[1].Length == 0)
                {
                    me.TeamCompeting = null;
                }
                else
                {
                    me.TeamCompeting = LookupTeamById(int.Parse(cols[1]));
                }
                me.Score = double.Parse(cols[2]);
                int parentId = 0;
                if(int.TryParse(cols[3], out parentId))
                { 
                    me.ParentMatchup = LookupMatchupById(parentId); 
                }
                else
                {
                    me.ParentMatchup = null;
                }
                output.Add(me);
            }
            return output;
        }
        public static void SaveRoundToFile(this TournamentModel model)
        {
            // Loop through each round
            // Loop through each matchup
            // Get the ID for the new matchup and save the record
            //Loop through each entry, get the id and save it

            foreach(List<MatchupModel> round in model.Rounds)
            {
                foreach(MatchupModel matchup in round)
                {
                    //load all the matchups from file
                    // Get the top id and add one
                    // Store the ID
                    // Save the Matchup Record
                    matchup.SaveMatchupToFile();
                }
            }
        }
        public static List<MatchupEntryModel> ConvertStringToMatchupEntryModels(string input)
        {
            string[] ids = input.Split('|');
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();
            List<string> entries = GlobalConfig.MatchupEntryFile.fullFilePath().LoadFile();
            List<string> matchingEntries = new List<string>();

            foreach(string id in ids)
            {
                foreach (string entry  in entries)
                {
                    string[] cols = entry.Split(',');

                    if(cols[0] == id)
                    {
                        matchingEntries.Add(entry);
                    }
                } 
            }
            output = matchingEntries.ConvertToMatchupEntryModel();
            return output;
        }
        private static TeamModel LookupTeamById(int id)
        {
            List<string> teams = GlobalConfig.teamFile.fullFilePath().LoadFile();

            foreach(string team in teams)
            {
                string[] cols = team.Split(',');
                if(cols[0] == id.ToString())
                {
                    List<string> matchingTeams = new List<string>();
                    matchingTeams.Add(team);
                    return matchingTeams.ConvertToTeamModels().First();
                }

            }

            return null;
        }
        private static MatchupModel LookupMatchupById(int id)
        {
            List<string> matchups = GlobalConfig.MatchupFile.fullFilePath().LoadFile();

            foreach (string matchup in matchups)
            {
                string[] cols = matchup.Split(',');
                if (cols[0] == id.ToString())
                {
                    List<string> matchingMatchups = new List<string>();
                    matchingMatchups.Add(matchup);
                    return matchingMatchups.ConverToMatchupModels().First();
                }

            }

            return null;
        }
        public static List<MatchupModel> ConverToMatchupModels(this List<string> lines)
        {
            // id = 0, entries = 1(pipe delimited), winner = 2, MatchupRound = 3, 
            List<MatchupModel> output = new List<MatchupModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchupModel m = new MatchupModel();

                m.Id = int.Parse(cols[0]);
                m.Entries = ConvertStringToMatchupEntryModels(cols[1]);
                if(cols[2]== " ")
                {
                    m.Winner = null;
                }
                else
                {
                    m.Winner = LookupTeamById(int.Parse(cols[2]));
                }
                m.MatchupRound = int.Parse(cols[3]);
                output.Add(m);
            }
            return output;

        }
        public static void SaveMatchupToFile(this MatchupModel matchup)
        {
            // Need to add a method to turn the entries into a string
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.fullFilePath().LoadFile().ConverToMatchupModels();
            int currentId = 1;

            if(matchups.Count > 0)
            {
                currentId = matchups.OrderByDescending(x => x.Id).First().Id+1;
            }
            matchup.Id = currentId;
            matchups.Add(matchup); 
            foreach (MatchupEntryModel entry in matchup.Entries)
            {
                entry.SaveEntryToFile(GlobalConfig.MatchupEntryFile);
            }
            // After saving all the matchup entry files

            List<string> lines = new List<string>();

            // id = 0, entries=1(pipe delimited by Id), winner = 2, matchupRound = 3
            foreach(MatchupModel m in matchups)
            {
                string winner = "";
                if(m.Winner != null)
                {
                    winner = m.Winner.ToString();
                }
               lines.Add($"{m.Id}, {ConvertMatchupEntryLevelListToString(m.Entries)}, {winner}, {m.MatchupRound}");
            }
            File.WriteAllLines(GlobalConfig.MatchupFile.fullFilePath(), lines);

        }
        public static void UpdateMatchupToFile(this MatchupModel matchup)
        {
            // Need to add a method to turn the entries into a string
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.fullFilePath().LoadFile().ConverToMatchupModels();

            MatchupModel oldMatchup = new MatchupModel();

            foreach(MatchupModel m in matchups)
            {
                if(m.Id == matchup.Id)
                {
                    oldMatchup = m;
                }
            }
            matchups.Remove(oldMatchup);

            matchups.Add(matchup);
            foreach (MatchupEntryModel entry in matchup.Entries)
            {
                entry.UpdateEntryToFile();
            }
            // After saving all the matchup entry files

            List<string> lines = new List<string>();

            // id = 0, entries=1(pipe delimited by Id), winner = 2, matchupRound = 3
            foreach (MatchupModel m in matchups)
            {
                string winner = "";
                if (m.Winner != null)
                {
                    winner = m.Winner.ToString();
                }
                lines.Add($"{m.Id}, {ConvertMatchupEntryLevelListToString(m.Entries)}, {winner}, {m.MatchupRound}");
            }
            File.WriteAllLines(GlobalConfig.MatchupFile.fullFilePath(), lines);
        }
        public static void UpdateEntryToFile(this MatchupEntryModel entry)
        {
            // id = 0, TeamCompeting = 1, Score = 2, ParentMatchup = 3
            List<MatchupEntryModel> entries = GlobalConfig.MatchupFile.fullFilePath().LoadFile().ConvertToMatchupEntryModel();
            MatchupEntryModel oldEntry = new MatchupEntryModel();

            foreach(MatchupEntryModel e in entries)
            {
                if(e.Id == oldEntry.Id)
                {
                    oldEntry = e;
                }
            }
            entries.Remove(oldEntry);

            entries.Add(entry);

            // Save to file
            List<string> lines = new List<string>();

            foreach (MatchupEntryModel e in entries)
            {
                string parent = "";
                if (e.ParentMatchup != null)
                {
                    parent = e.ParentMatchup.ToString();
                }
                string teamCompeting = "";
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{e.Id}, {teamCompeting}, {e.Score}, {parent}");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.fullFilePath(), lines);
        }


        public static void SaveEntryToFile(this MatchupEntryModel entry, string matchupEntryFile)
        {
            // id = 0, TeamCompeting = 1, Score = 2, ParentMatchup = 3
            List<MatchupEntryModel> entries = GlobalConfig.MatchupFile.fullFilePath().LoadFile().ConvertToMatchupEntryModel();

            int currentId = 1;
            if (entries.Count > 0)
            {
                currentId = entries.OrderByDescending(x => x.Id).First().Id + 1;
            }
            entry.Id = currentId;
            entries.Add(entry);

            // Save to file
            List<string> lines = new List<string>();

            foreach(MatchupEntryModel e in entries)
            {
                string parent = "";
                if(e.ParentMatchup != null)
                {
                    parent = e.ParentMatchup.ToString();
                }
                string teamCompeting = "";
                if(e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{e.Id}, {teamCompeting}, {e.Score}, {parent}");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.fullFilePath(), lines);
        }

        public static void SaveToTournamentFile(this List<TournamentModel> models)
        {
            List<string> lines = new List<string>();

            foreach(TournamentModel t in models)
            {
                lines.Add($"{t.Id},{t.TournamentName},{t.EntryFee}, {ConvertToTeamList(t.EnteredTeams)},{ConvertToPrizeList(t.Prizes)},{ConvertToRoundListToString (t.Rounds) }");
            }
        }
        public static string ConvertToRoundListToString(List<List<MatchupModel>> rounds)
        {
            // Rounds - id^id^id|id^id^id|id^id^id
            string output = "";
            if (rounds.Count > 0)
            {
                foreach (List<MatchupModel> r in rounds)
                {
                    output += $"{ ConvertToMatchupListToString(r) }|";
                }
                output = output.Substring(0, output.Length - 1); // We want to get rid of the last |
            }

            return output;
        }

        public static string ConvertMatchupEntryLevelListToString(List<MatchupEntryModel> entries)
        {
            string output = "";
            if (entries.Count > 0)
            {

                foreach (MatchupEntryModel me in entries)
                {
                    output += $"{ me.Id }|";
                }
                output = output.Substring(0, output.Length - 1); // We want to get rid of the last |
            }

            return output;
        }
        public static string ConvertToMatchupListToString(List<MatchupModel> matchups)
        {
            string output = "";
            if (matchups.Count > 0)
            {
                foreach(MatchupModel m in matchups)
                {
                    output += $"{ m.Id}|";
                }
                output = output.Substring(0, output.Length - 1); // We want to get rid of the last |
            }

            return output;
        }
        public static string ConvertToPrizeList(List<PrizeModel> prizes)
        {
            string output = "";
            if (prizes.Count > 0)
            {

                foreach (PrizeModel p in prizes)
                {
                    output += $"{ p.id }|";
                }
                output = output.Substring(0, output.Length - 1); // We want to get rid of the last |
            }

            return output;
        }
        public static string ConvertToTeamList(List<TeamModel> teams)
        {
            string output = "";
            if (teams.Count > 0)
            { 

                foreach (TeamModel t in teams)
                {
                    output += $"{ t.Id }|";
                }
                output = output.Substring(0, output.Length - 1); // We want to get rid of the last |
            }

            return output;
        }

        private static string ConvertToPeopleList(List<PersonModel> people)
        {
            string output = "";
            
            foreach(PersonModel p in people)
            {
                output += $"{ p.id }|";
            }
            output = output.Substring(0, output.Length - 1); // We want to get rid of the last |

            return output;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;
using Trackerlibrary.DataAccess.TextHelpers;
using Tournament_Tracker;

namespace TrackerLibrary.DataAccess
{
    public class TextConnector: IDataConnection
    {
        //private const string PrizesFile = "PrizeModels.csv";
        //private const string PersonFile = "PersonModels.csv";
        //private const string teamFile = "TeamModels.csv";
        //private const string TournamentFile = "TournamentModels.csv";
        //private const string MatchupFile = "MatchupModels.csv";
        //private const string MatchupEntryFile = "MatchupFile.csv";

        public void CreatePerson(PersonModel model)
        {
            // Load the text file and convert the test to List<PersonModel>
            List<PersonModel> person = GlobalConfig.PersonFile.fullFilePath().LoadFile().ConvertToPersonModels();   // load the text file
            //Find the max Id
            int currentId = 1;
            if (person.Count > 0)
            {
                currentId = person.OrderByDescending(x => x.id).First().id + 1; //Which gives the first ID which is the highest
            }
            model.id = currentId;

            //add the new record with the new id(max + 1)
            person.Add(model);

            person.SaveToPeopleFile();

        }

        // TODO - Wire up the CreatePrize for text files

        public void CreatePrize(PrizeModel model)
        {

          // Load the text file and convert the test to List<prizeModel>
          List<PrizeModel> prizes = GlobalConfig.PrizesFile.fullFilePath().LoadFile().ConverToPrizeModels();   //load the Text File

            //Find the max Id
            int currentId = 1;
            if(prizes.Count > 0)
            {
                currentId = prizes.OrderByDescending(x => x.id).First().id + 1; //Which gives the first ID which is the highest
            }
            model.id = currentId;

            //add the new record with the new id(max + 1)
            prizes.Add(model);


            //Convert the prizes to list<string>
            //Save the list<string>
            //Save the list<string> to the text file
            prizes.SaveToPrizeFile();

        }

        public void CreateTeam(TeamModel model)
        {
            List<TeamModel> teams = GlobalConfig.teamFile.fullFilePath().LoadFile().ConvertToTeamModels(); //load the text file

            int currentId = 1;
            if (teams.Count > 0)
            {
                currentId = teams.OrderByDescending(x => x.Id).First().Id + 1; //Which gives the first ID which is the highest
            }
            model.Id = currentId;

            teams.Add(model);

            teams.SaveToTeamFile();

        }

        public void CreateTournament(TournamentModel model)
        {
            List<TournamentModel> tournaments = GlobalConfig.TournamentFile.
                fullFilePath().
                LoadFile().
                ConvertToTournamentModel();
            int currentId = 1; 
            if(tournaments.Count > 0)
            {
                currentId = tournaments.OrderByDescending(x => x.Id).First().Id + 1; //Which gives the first ID which is the highest
            }
            model.Id = currentId;
            model.SaveRoundToFile();
            tournaments.Add(model);
            tournaments.SaveToTournamentFile();

            TournamentLogic.UpdateTournamentResults(model);

        }

        public List<PersonModel> GetPerson_All()
        {
            return GlobalConfig.PersonFile.fullFilePath().LoadFile().ConvertToPersonModels();
        }

        public List<TeamModel> GetTeam_All()
        {
            return GlobalConfig.teamFile.fullFilePath().LoadFile().ConvertToTeamModels();
        }

        public List<TournamentModel> GetTournament_All()
        {
            return GlobalConfig.TournamentFile.
            fullFilePath().
            LoadFile().
            ConvertToTournamentModel();
        }

        public void UpdateMatchup(MatchupModel model)
        {
            model.UpdateMatchupToFile();
        }

        public void CompleteTournament(TournamentModel model)
        {
            List<TournamentModel> tournaments = GlobalConfig.TournamentFile
                .fullFilePath()
                .LoadFile()
                .ConvertToTournamentModel();

            tournaments.Add(model);

            tournaments.SaveToTournamentFile();

            TournamentLogic.UpdateTournamentResults(model);
        }
    }
}

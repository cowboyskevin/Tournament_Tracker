using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Tracker;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string db = "Tournaments";
        // TODO - Make the CreatePrize method actually save to the address
        /// <summary>
        /// Saves a new prize to the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns>The prize information including the unique identifier</returns>
        public void CreatePrize(PrizeModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
                p.Add("@PlaceNumber", model.PlaceNumber);
                p.Add("@PlaceName", model.PlaceName);
                p.Add("@PrizeAmount", model.PrizeAmount);
                p.Add("@PrizePercentage", model.PrizePercentage);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("Tournaments.dbo.spInsert_Prizes", p, commandType: CommandType.StoredProcedure);


                model.id = p.Get<int>("@id");

            }

        }
        public void CreatePerson(PersonModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
                p.Add("@FirstName", model.FirstName);
                p.Add("@LastName", model.LastName);
                p.Add("@EmailAddress", model.EmailAddress);
                p.Add("CellPhoneNumber", model.CellPhoneNumber);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                connection.Execute("Tournaments.dbo.spPeople_Insert", p, commandType: CommandType.StoredProcedure);


                model.id = p.Get<int>("@id");

            }

        }

        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<PersonModel>("Tournaments.dbo.spPeople_getAll").ToList();
            }
            return output;
        }

        public void CreateTeam(TeamModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {

                var p = new DynamicParameters();
                p.Add("@TeamName", model.TeamName);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                connection.Execute("Tournaments.dbo.spTeams_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                foreach (PersonModel per in model.TeamMembers)
                {
                    p = new DynamicParameters();
                    p.Add("@TeamId", model.Id);
                    p.Add("@PersonId", per.id);
                    connection.Execute("Tournaments.dbo.spTeamMembers_Insert", p, commandType: CommandType.StoredProcedure);
                }

            }
        }

        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<TeamModel>("Tournaments.dbo.spTeams_getAll").ToList();

                foreach (TeamModel team in output) // adds team members to each of the teams in output
                {
                    var p = new DynamicParameters();
                    p.Add("@TeamId", team.Id);
                    team.TeamMembers = connection.Query<PersonModel>("Tournaments.dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            return output;

        }

        public void CreateTournament(TournamentModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                saveTournament(connection, model);
                saveTournamentPrizes(connection, model);
                saveTournamentEntries(connection, model);
                saveTournamentRounds(connection, model);

                TournamentLogic.UpdateTournamentResults(model);
        
            }
        }

        private void saveTournament(IDbConnection connection, TournamentModel model)
        {
            var p = new DynamicParameters();
            p.Add("@TournamentName", model.TournamentName);
            p.Add("@EntryFee", model.EntryFee);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("Tournaments.dbo.spTournament_Insert ", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");
        }
        private void saveTournamentPrizes(IDbConnection connection, TournamentModel model)
        {
            foreach (PrizeModel pz in model.Prizes)
            {
                var p = new DynamicParameters();
                p.Add("@PrizesId", pz.id);
                p.Add("@TournamentId", model.Id);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                connection.Execute("Tournaments.dbo.spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }
        private void saveTournamentEntries(IDbConnection connection, TournamentModel model)
        {
            foreach (TeamModel tm in model.EnteredTeams)
            {
                var p = new DynamicParameters();
                p.Add("@TeamId", tm.Id);
                p.Add("TournamentId", model.Id);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                connection.Execute("Tournaments.dbo.spTournamentEntries_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }
        private void saveTournamentRounds(IDbConnection connection, TournamentModel model)
        {
            // List<MatchupModel>> Rounds
            //List<MatchupEntryModel> Entries // Can not save an entrymodel if it does not have an id of its matchup

            // loop through the rounds
            //Loop through the matchups
            // Save the matchup
            // Loop through the entries and save them
            foreach(List<MatchupModel> round in model.Rounds)
            {
                foreach(MatchupModel matchup in round)
                {
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", model.Id);
                    p.Add("MatchupRound", matchup.MatchupRound);
                    p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    connection.Execute("Tournaments.dbo.spMatchups_Insert", p, commandType: CommandType.StoredProcedure);

                    matchup.Id = p.Get<int>("@id"); // This gets matchup id of the two teams playing each other

                    foreach(MatchupEntryModel entry in matchup.Entries)
                    {
                        p = new DynamicParameters();
                        p.Add("@MatchupId", matchup.Id);
                        if(entry.ParentMatchup == null)
                        {
                            p.Add("@ParentMatchupId", null);
                        }
                        else
                        {
                            p.Add("@ParentMatchupId", entry.ParentMatchup.Id);
                        }
                        if (entry.TeamCompeting == null)
                        {
                            p.Add("@TeamCompetingId", null);
                        }
                        else
                        {
                            p.Add("@TeamCompetingId", entry.TeamCompeting.Id);
                        }
                        p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                        connection.Execute("Tournaments.dbo.spMatchupEntries_Insert", p, commandType: CommandType.StoredProcedure);

                    }
                }
                
            }
        }
        public List<TournamentModel> GetTournament_All()
        {
            List<TournamentModel> output;
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<TournamentModel>("Tournaments.dbo.spTournaments_GetAll").ToList();
                var p = new DynamicParameters();

                // Populate Prizes
                foreach (TournamentModel t in output)
                {

                    // Populate Prizes
                    p = new DynamicParameters();
                    p.Add("@TournamentId", t.Id);
                    t.Prizes = connection.Query<PrizeModel>("Tournaments.dbo.spPrizes_GetByTournament", p, commandType:CommandType.StoredProcedure).ToList();

                    p = new DynamicParameters();
                    p.Add("@TournamentId", t.Id);
                    t.EnteredTeams = connection.Query<TeamModel>("Tournaments.dbo.spTeams_GetByTournament", p, commandType:CommandType.StoredProcedure).ToList();

                    //Populate Teams
                    foreach (TeamModel team in t.EnteredTeams) // adds team members to each of the teams in output
                    {
                        p = new DynamicParameters();
                        p.Add("@TeamId", team.Id);
                        team.TeamMembers = connection.Query<PersonModel>("Tournaments.dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                    }
                    p = new DynamicParameters();
                    p.Add("@TournamentId", t.Id);
                    List<MatchupModel> matchups = connection.Query<MatchupModel>("Tournaments.dbo.spMatchups_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();
                    foreach(MatchupModel m in matchups)
                    {
                            p = new DynamicParameters();
                            p.Add("@MatchupId", m.Id);
                            m.Entries = connection.Query<MatchupEntryModel>("Tournaments.dbo.spMatchupEntries_GetByMatchup ", p, commandType: CommandType.StoredProcedure).ToList();
                            // Populate each entry(2 models)
                            // POpulate each matchup(1 model)
                            List<TeamModel> allteams = GetTeam_All();
                            if(m.WinnerId> 0)
                            {
                                m.Winner = allteams.Where(x => x.Id == m.WinnerId).First();
                            }
                            foreach(var me in m.Entries)
                            {
                                if(me.TeamCompetingId > 0)
                                {
                                    me.TeamCompeting = allteams.Where(x => x.Id == me.TeamCompetingId).First();
                                }
                                if(me.ParentMatchupId > 0)
                                {
                                    me.ParentMatchup= matchups.Where(x => x.Id == me.ParentMatchupId).First();
                                }
                            }
                    }
                    //List<List<MatchupModel>>;
                    List<MatchupModel> currRow = new List<MatchupModel>();
                    int currRound = 1;

                    foreach(MatchupModel m in matchups)
                    {
                        if(m.MatchupRound > currRound)
                        {
                            t.Rounds.Add(currRow);
                            currRow = new List<MatchupModel>();
                            currRound += 1;
                        }
                        currRow.Add(m);
                    }
                    t.Rounds.Add(currRow);


                 }
                //Populate Teams
                // Populate Rounds
            }
            return output;
        }

        public void UpdateMatchup(MatchupModel model)
        {
            // Need to store matchup, and matchup Entries
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
                if (model.Winner != null)
                {
                    p.Add("@id", model.Id);
                    p.Add("@WinnerId", model.Winner.Id);
                    connection.Execute("Tournaments.dbo.spMatchups_Update", p, commandType: CommandType.StoredProcedure); 
                }

                //SpMatchupEntries_Update id, TeamCompetingId, Score

            foreach(MatchupEntryModel me in model.Entries)
                {
                    if (me.TeamCompeting != null)
                    {
                        p = new DynamicParameters();
                        p.Add("@id", me.Id);
                        p.Add("@TeamCompetingId", me.TeamCompeting.Id);
                        p.Add("@Score", me.Score);
                        connection.Execute("Tournaments.dbo.spEntries_Update", p, commandType: CommandType.StoredProcedure); 
                    }
                }
            }
            {

            }

        }
        public void CompleteTournament(TournamentModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
                p.Add("@id", model.Id);


                connection.Execute("Tournaments.dbo.spTournaments_Complete", p, commandType: CommandType.StoredProcedure);
            }
        }
    }
    
}

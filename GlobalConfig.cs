using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.DataAccess;

namespace TrackerLibrary
{
    public static class GlobalConfig
    {
        public const string PrizesFile = "PrizeModels.csv";
        public const string PersonFile = "PersonModels.csv";
        public const string teamFile = "TeamModels.csv";
        public const string TournamentFile = "TournamentModels.csv";
        public const string MatchupFile = "MatchupModels.csv";
        public const string MatchupEntryFile = "MatchupFile.csv";
        
        public enum DatabaseType
        {
            sql,
            textFile
        }

        public static IDataConnection Connection { get; private set; } 


        public static void InitializeConnections(bool database, bool textFiles)
        {
            if(database)
            {
                //TODO- Create the Sql Connection
                SqlConnector sql = new SqlConnector();
                Connection = sql; 
            }
            if(textFiles)
            {
                // TODO - Create the text connection
                TextConnector text = new TextConnector();
                Connection = text;
            }
        }
        public static void InitializeConnections(DatabaseType databasetype)
        {
            if(databasetype == DatabaseType.sql)
            {
                //TODO- Create the Sql Connection
                SqlConnector sql = new SqlConnector();
                Connection = sql;
            }
            if(databasetype == DatabaseType.textFile)
            {
                TextConnector text = new TextConnector();
                Connection = text;
            }

        }
        public static string CnnString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
        public static string AppKeyLookup(string key)
        {
            return ConfigurationManager.AppSettings[key];
    }

    }
}

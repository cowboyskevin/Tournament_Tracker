using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary
{
    public static class GlobalConfig
    {
        public static List<IDataConnection> Connections { get; private set; } = new List<IDataConnection>();

        public static void InitializeConnections(bool database, bool textFiles)
        {
            if(database)
            {
                //TODO- Create the Sql Connection
                SqlConnector sql = new SqlConnector();
                Connections.Add(sql); 
            }
            if(textFiles)
            {
                // TODO - Create the text connection
                textConnection text = new textConnection();
                Connections.Add(text);
            }
        }
    }
}

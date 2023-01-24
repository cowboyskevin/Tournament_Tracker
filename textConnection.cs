using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary
{
    public class textConnection : IDataConnection
    {
        // TODO - Wire up the CreatePrize for text files

        public PrizeModel CreatePrize(PrizeModel model)
        {
            model.id = 1;

            return model;
        }

    }
}

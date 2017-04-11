using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace CollabClient
{
    interface TableExit
    {
        // This function should define how the datatable input gets output to a specific location, eg, file, database, SQLite
        void outputTableThroughExit(DataTable dt);
    }
}

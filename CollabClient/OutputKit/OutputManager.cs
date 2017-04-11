using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CsvHelper;


namespace CollabClient
{
    // This class will be used to handle three ways to output data.-.0 Two ways are pretty much the same, suggesting that I can reuse the code
    // The other way of outputting data is through the csv writer.

    class OutputManager
    {
        private DataTable _dt;
        private List<TableExit> _exits; // TableExit is an interface, which requires the function
                                        // outputTableThroughExit


        public OutputManager(DataTable dt)
        {
            _dt = dt;
            _exits = new List<TableExit>();
        }


        public void outputResultTable(){
            foreach (TableExit exit in _exits)
            {
                exit.outputTableThroughExit(_dt);
            }
        }

        public void addExit(TableExit exit)
        {
            _exits.Add(exit);
        }

        // For more flexibility, more manipulation on the list needs to be added.
    }
}

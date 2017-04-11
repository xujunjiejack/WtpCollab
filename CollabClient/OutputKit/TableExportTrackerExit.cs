using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace CollabClient
{
    /// <summary>
    /// This one actually doesn't output whole datatable. It only records that this table needs to be loaded back to the wtp_data.
    /// 
    /// </summary>
    class TableExportTrackerExit: TableExit
    {
        private String _filePath;

        public TableExportTrackerExit(String filePath)
        {
            _filePath = filePath;
        }

        public void outputTableThroughExit(DataTable dt)
        {
            //Append the table name to the file
            if (!File.Exists(_filePath))
            {
                using (StreamWriter sw = File.CreateText(_filePath))
                {
                    // write header and other things
                    sw.WriteLine(dt.TableName);
                }

                return;
            }

            using (StreamWriter sw = File.AppendText(_filePath))
            {
                // Write something
                sw.WriteLine(dt.TableName);
                return;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CsvHelper;
using System.IO;

namespace CollabClient
{
    class CSVExit: TableExit
    {
        private String _outputFilePath;
        private const String TEST_FILE_PATH = "csvfile2.csv";
        public CSVExit()
        {    
        }

        public CSVExit(String outputFilePath){
            _outputFilePath = outputFilePath;
        }

        public void outputTableThroughExit(DataTable dt)
        {
            writeToCSVFromDataTable(dt);
        }

        private void writeToCSVFromDataTable(DataTable dt)
        {
            TextWriter textWriter = new StreamWriter(_outputFilePath);
            CsvWriter writer = new CsvWriter(textWriter);

            foreach (DataColumn column in dt.Columns)
            {
                writer.WriteField(column.ColumnName);
            }

            writer.NextRecord();

            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    writer.WriteField(row[column].ToString());
                }
                writer.NextRecord();
            }

            textWriter.Flush();

        }
    }

}

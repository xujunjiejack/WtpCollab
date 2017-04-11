using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using CsvHelper;
using System.IO;
using System.Diagnostics;
using System.Data.Odbc;
using System.Data;

namespace CollabClient
{
    class MainController
    {
        private String _userTableName;
        private String[] _dataTables;
        private SQLiteConnection _sqliteConn;
        private OdbcConnection _odbcConn;
        private bool _isAllFieldsMode;
        public const String CSV_OUT_PATH = "O:/user_make_tables/";
        private CollabClientViewModel _viewModel;

        public bool IsAllFieldsMode
        {
            get
            {
                return _isAllFieldsMode;
            }
            set
            {
                _isAllFieldsMode = value;
                // Populate each table with the mode. I think I need to refactor the code
            }
        }


        public MainController(CollabClientViewModel viewModel)
        {
            _sqliteConn = ConnectionCoordinater.SharedConnectionCoordinator.sqliteConn;
            //_odbcConn = cc.odbcConn;
            // What if the sqliteConn is not opening?
            IsAllFieldsMode = true;
            _viewModel = viewModel;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userTableName"></param>
        /// <param name="tableLists"></param>
        /// <returns></returns>
        public String run(String userTableName, DataTables tableLists)
        {
            _userTableName = userTableName;
            // I need to change code here to incooperate the subset of the tableslist 
            _dataTables = tableLists.getAllTableNames();

            //Decide whether this is a family table or twin table
            DataFileBuilder builder = new DataFileBuilder(_sqliteConn, _viewModel);
            DataTable dt = builder.buildDataFile(tableLists, _userTableName);

            // Decide how to output different things
            // We probably have three ways to output it. CSV file, Collab database, and Main database (This can be tricky)
            OutputManager outputManager = prepareOutput(dt);
            outputManager.outputResultTable();

            UserTableTracker tracker = new UserTableTracker(_sqliteConn);
            tracker.addDataTable(dt);

            //record table into sqlite
            _sqliteConn.Close();
            return userTableName;
        }

        private OutputManager prepareOutput(DataTable dt)
        {
            OutputManager outputManager = new OutputManager(dt);

            CSVExit csvExit = new CSVExit(CSV_OUT_PATH + dt.TableName+".csv");
            SQLiteDatabaseExit sqliteExit = new SQLiteDatabaseExit(_sqliteConn);

            try
            {
                _odbcConn.Open();
                //MySQLDatabaseExit mysqlExit = new MySQLDatabaseExit(_odbcConn);
                //  outputManager.addExit(mysqlExit);
            }
            catch (Exception e)
            {

            }
            TableExportTrackerExit fileExit = new TableExportTrackerExit("TablesNeedToBeExported.txt");

            outputManager.addExit(fileExit);
            outputManager.addExit(sqliteExit);
            outputManager.addExit(csvExit);

            return outputManager;
        }

        public void openCSVFile(String table_name)
        {
            Process.Start(CSV_OUT_PATH + table_name + ".csv");
        }

        public void openCSVFolder() => Process.Start(CSV_OUT_PATH);
    }
}

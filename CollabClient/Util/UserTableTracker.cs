using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;


namespace CollabClient
{
    class UserTableTracker
    {
        public const string TRACKER_TABLE_NAME = "USER_TABLE_TRACKER";
        private SQLiteConnection _sqliteConn;
        private ConnectionCoordinater _cc;

        private List<String> _userTableNames;
        public List<String> UserTableNames
        {
            get
            {
                _userTableNames = _userTableNames ?? getUserTableNames(); 
                return _userTableNames;
            } 
        }

        public bool tablenameIsDuplicate(String candidate)
        {
            if (UserTableNames.Contains(candidate))
            {
                return true;
            }
            return false;
        }

        private List<String> getUserTableNames()
        {
            // get the memory representation of the tracker
            SQLiteDataAdapter dataadapter = new SQLiteDataAdapter(String.Format("SELECT * FROM {0}", TRACKER_TABLE_NAME), _cc.useSqliteConn());
            DataSet ds = new DataSet();
            dataadapter.Fill(ds, TRACKER_TABLE_NAME);
            DataTable trackerTable = new DataTable();
            trackerTable = ds.Tables[TRACKER_TABLE_NAME];

            List<String> userTableNames = new List<String>();
            foreach (DataRow row in trackerTable.Rows)
            {
                userTableNames.Add((String)row["TableName"]);
            }
            return userTableNames;
        }

        public void updateUserTableNames() => _userTableNames = getUserTableNames();

        public UserTableTracker(ConnectionCoordinater cc) => _cc = cc;

        public UserTableTracker(SQLiteConnection sqliteConn) => _sqliteConn = sqliteConn;
    

        /// <summary>
        /// This method creates a corresponding record in the USER_TABLE_TRACKER. The new record includes the creation date of the table, its
        /// tablename, its creator and the machine that his creator for generating this table. 
        /// 
        /// </summary>
        /// <param name="newDataTable">
        ///     The new table that has been built by DataFileBuilder based on the user's lists of tables.
        /// </param>
        public void addDataTable(DataTable newDataTable)
        {
            SQLiteDataAdapter dataadapter = new SQLiteDataAdapter(String.Format("SELECT * FROM {0}", TRACKER_TABLE_NAME), _sqliteConn);
            DataSet ds = new DataSet();
            dataadapter.Fill(ds, TRACKER_TABLE_NAME);
            DataTable trackerTable= new DataTable();
            trackerTable = ds.Tables[TRACKER_TABLE_NAME];

            DataRow newDataTableRow = trackerTable.NewRow();
            newDataTableRow["TableName"] = newDataTable.TableName;
            newDataTableRow["CreationTime"] = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
            newDataTableRow["User"] = Environment.UserName;
            newDataTableRow["ComputerName"] = Environment.MachineName;
            newDataTableRow["isInWTPData"] = 0;
            trackerTable.Rows.Add(newDataTableRow);
            
            // Need to create key for user table tracker
            SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(dataadapter);
            SQLiteCommand cmd = cmdBuilder.GetUpdateCommand();
            dataadapter.Update(ds, TRACKER_TABLE_NAME);
        }

        
    }
}

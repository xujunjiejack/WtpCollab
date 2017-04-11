using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

namespace CollabClient
{
    /// <summary>
    /// This view model is used to store the decision made by user in the user interface
    /// It will be passed to other dialog to faciliate the communication of data
    /// </summary>
    public class CollabClientViewModel
    {

        private ConnectionCoordinater _cc;
        private bool _isAllFieldsMode;
        private bool _appendGender;
        private bool _appendRdSelect;
        private bool _userTableNames;

        public bool AppendGender { get; set;}
        public bool AppendRdSelect{get; set;}
        public bool AppendZyg { get; set; }

        public bool IsAllFieldsMode {
            get
            {
                return _isAllFieldsMode;
            }
            set
            {
                _isAllFieldsMode = value;

                if (UserTables != null)
                {
                    foreach (Table t in UserTables)
                    {
                        t.AllFullFieldMode = _isAllFieldsMode;
                    }
                }
            }
        }
        public DataTables AvailableTables
        {get;set;}

        public DataTables UserTables
        {get;set;}

        public CollabClientViewModel()
        {
            IsAllFieldsMode = true;
            AppendGender = false;
            AppendRdSelect = false;
            _cc = ConnectionCoordinater.SharedConnectionCoordinator;
            this.UserTables = new DataTables();
            this.AvailableTables = getAllDataTables();
        }

        /// <summary>
        /// 
        /// </summary>
        public ConnectionCoordinater cc
        {
            get
            {
                return _cc;
            }
        }

        public DataTables getAllDataTables()
        {
            DataTable dt = _cc.useSqliteConn().GetSchema("TABLES", new String[] { null, null });
            DataTables dbTables = new DataTables();
            foreach (DataRow row in dt.Rows)
            {
                dbTables.Add(new Table(row["TABLE_NAME"].ToString(), _cc.sqliteConn)
                {
                    AllFullFieldMode = IsAllFieldsMode
                });
            }
            return dbTables;
        }

        public void addTableToUserTables(Table t)
        {
            t.AllFullFieldMode = IsAllFieldsMode;
            t.prepareForUse();
            UserTables.Add(t);
        }

        // TODO: Need more implementation
        public void resetBuilder()
        {
            IsAllFieldsMode = true;
            AvailableTables = getAllDataTables();
            UserTables = new DataTables();
            AppendGender = false;
            AppendRdSelect = false;
        }

        private void moveTableWithSpecificFieldFromAvailableToUserTable(String tableName, String columnName)
        {
            Table dtAvailable = getTableFromDataTable(AvailableTables, tableName);
            Table dtUser = getTableFromDataTable(UserTables, tableName);
            // Check where the table is.

            if (dtAvailable == null && dtUser == null)
            {
                throw new ArgumentException("The table is not available");
            }
            if (dtAvailable != null && dtUser != null)
            {
                throw new ArgumentException("The table is so weird");
            }
            if (dtAvailable != null)
            {
                AvailableTables.Remove(dtAvailable);
                UserTables.Add(dtAvailable);
                // If it's the first time this table gets loaded, prepare it
                dtAvailable.prepareForUse();
                // check a specific column is in user table
                dtAvailable.useSpecificColumn(columnName);
                return;
            }

            if (dtUser != null)
            {
                dtUser.useSpecificColumn(columnName);
                return;
            }
        }

        // I was wrong about the code... I need to inner join it...... 
        private void deleteSpecificFieldFromTableInUserTable(string tableName, string colName)
        {
            Table dtUser = getTableFromDataTable(UserTables, tableName);

            // This raises a question, how do I prevent user to screw up the table that the system managered?
            if (dtUser == null)
            {
                return;
            }

            if (dtUser != null)
            {
                // delete the col if it's in the table
                dtUser.discardSpecficColumn(colName);
                return;
            }
        }

        private Table getTableFromDataTable(DataTables dts, String tableName)
        {
            foreach (Table t in dts)
            {
                if (t.TableName.Equals(tableName))
                    return t;
            }
            return null;
        }
    }
}

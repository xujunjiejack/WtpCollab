using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace CollabClient
{
    // The representation for storing the information of tables which user uses to build datafile. 
    // It saves the status for each table.


    /// <summary>
    /// The class representation for each table which user uses to build datafile. It contains the information
    /// that allows data file builder to decide what columns and what action it should take to build with the file
    /// It saves the status for each table, such as TableName, its type, table columns.
    /// </summary>
    public class Table: INotifyPropertyChanged
    {
        /// <summary>
        /// This enum represents the category of the table
        /// TwinTable has the column of "familyid", "twin", suggesting twins are the subject of measurement
        /// FamilyTable only has the column of "familyid", suggesting caregiver or the whole family is the center of the measurement
        /// GeneralTable is the table that doesn't have "familyid"
        /// </summary>
        public enum TableType { TwinTable, FamilyTable, TwinDataTable, FamilyDataTable, GeneralTable }
        private bool _doUseFullFields;

        public Table()
        {
        }

        public String TableName { get;  set; }
        public TableType Type { get; set; }
        public bool AllFullFieldMode { get; set;} // Used by the upper level to change the data
        private ObservableCollection<String> _userSpecifiedFields = null; // Null stands for that user hasn't chosen any field to add
        private ObservableCollection<String> _availableColumns = null;// It will be loaded in the run time
        private SQLiteConnection _sqliteConn = null;
        public bool DoUseFullFields
        {
            get => _doUseFullFields;
            set
            {
                _doUseFullFields = value;
                OnPropertyChanged("DoUseFullFields");
            }
        }

        /// <summary>
        /// The fields that user wants this table to join with other tables. 
        /// If the user wants to use all of the field (AllFullFieldMode), it will return'
        /// all of the columns, but not change any other state of the table
        /// </summary>
        public ObservableCollection<String> UserSpecifiedFields
        {
            get
            {
                // If the user decides that every table has to use all field, then the AllFullField will get overriden
                if (this.AllFullFieldMode)
                    return getAllCols(this._sqliteConn);

                // If the user hasn't specified 
                if (_userSpecifiedFields == null)
                    //Family id and twin will be forced to be initialized in the user fields
                    _userSpecifiedFields = initializeUserFields();

                return _userSpecifiedFields;
            }
            set
            {
                _userSpecifiedFields = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void prepareForUse(){
            // If the available columns have not been loaded, load it, and decide the table type
            // (which can costs a lot of for 900 tables to be loaded at once
            if (_availableColumns == null)
            {
                _availableColumns = getAllCols(_sqliteConn);

                List<String> TwinDataTableField = new List<string>() { "familyid", "twin", "datamode"};
                List<String> FamilyDataTableField = new List<string>() { "familyid", "datamode" };

                if (TwinDataTableField.All(_availableColumns.Contains)) this.Type = TableType.TwinDataTable;
                else if (FamilyDataTableField.All(_availableColumns.Contains)) this.Type = TableType.FamilyDataTable;
                else if (_availableColumns.Contains<String>("twin") && _availableColumns.Contains<String>("familyid"))
                {
                    this.Type = TableType.TwinTable;
                }
                else if (_availableColumns.Contains<String>("familyid"))
                {
                    this.Type = TableType.FamilyTable;
                }
                else
                {
                    this.Type = TableType.GeneralTable;
                }
            }
        }


        private ObservableCollection<String> initializeUserFields()
        {
            //Family id or twin will be forced to be chosen for the table
            ObservableCollection<String> list = new ObservableCollection<String>();
            // Move the key of the table from available lists to user list as initialization
            switch (this.Type)
            {
                case TableType.FamilyTable:
                    list.Add("familyid");
                    _availableColumns.Remove("familyid");
                    break;
                case TableType.TwinTable:
                    _availableColumns.Remove("familyid");
                    list.Add("familyid");
                    list.Add("twin");
                    _availableColumns.Remove("twin");
                    break;
                case TableType.TwinDataTable:
                    list.Add("familyid");
                    _availableColumns.Remove("familyid");
                    list.Add("twin");
                    _availableColumns.Remove("twin");
                    list.Add("datamode");
                    _availableColumns.Remove("datamode");
                    break;

                case TableType.FamilyDataTable:
                    list.Add("familyid");
                    _availableColumns.Remove("familyid");
                    list.Add("datamode");
                    _availableColumns.Remove("datamode");
                    break;

                default:
                    break;
            }
            return list;
        }

        public ObservableCollection<String> AvailableColumns
        {
            get
            {   
                if (_availableColumns == null)
                {   
                    _availableColumns = getAllCols(_sqliteConn);

                    List<String> TwinDataTableField = new List<string>() { "familyid", "twin", "datamode" };
                    List<String> FamilyDataTableField = new List<string>() { "familyid", "datamode" };

                    if (TwinDataTableField.All(_availableColumns.Contains)) this.Type = TableType.TwinDataTable;
                    else if (FamilyDataTableField.All(_availableColumns.Contains)) this.Type = TableType.FamilyDataTable;
                    // Decide whether the table is family table or not
                    else if (_availableColumns.Contains<String>("twin") && _availableColumns.Contains<String>("familyid"))
                    {
                        this.Type = TableType.TwinTable;
                    }
                    else if (_availableColumns.Contains<String>("familyid"))
                    {
                        this.Type = TableType.FamilyTable;
                    } else{
                        this.Type = TableType.GeneralTable;
                    }
                    return _availableColumns;
                }
                return _availableColumns;
            }
        }

        public Table(String tableName)
        {
            this.TableName = tableName;
        }

        public Table(String tableName, SQLiteConnection sqlconn)
        {
            this.TableName = tableName;
            this._sqliteConn = sqlconn;
        }

        
        /// <summary>
        /// Gets all of the column for this table through sql connection, and return it as a list of String
        /// </summary>
        /// <param name="sqlconn"></param>
        /// <returns> 
        ///     ObservableCollection of String of all of the columns in the list
        /// </returns>
        private ObservableCollection<String> getAllCols(SQLiteConnection sqlconn)
        {   
            // It's possible that sqlconn can't be opened
            DataTable dt = ConnectionCoordinater.SharedConnectionCoordinator.useSqliteConn().GetSchema("Columns", new String[] { null, null, TableName });
            ObservableCollection<String> cols = new ObservableCollection<String>();

            foreach (DataRow row in dt.Rows)
                cols.Add(row["column_name"].ToString());
             
            return cols;
        }

        public override string ToString()
        {
            return TableName;
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public bool useSpecificColumn(String colName)
        {
            String colAvailable = findColumnInColumns(AvailableColumns, colName);
            String colInUse = findColumnInColumns(UserSpecifiedFields, colName);
            if (colAvailable == null && colInUse == null)
            {
                throw new ArgumentException("No column in this table");
            }
            if (colAvailable != null && colInUse != null)
            {
                throw new ArgumentException("Weird column");
            }

            if (colAvailable != null)
            {
                // move col from available to user specified
                AvailableColumns.Remove(colAvailable);
                UserSpecifiedFields.Add(colAvailable);
                return true;
            }
            if (colInUse != null)
            {
                return false;
            }
            return false;
        }

        public bool discardSpecficColumn(String colName)
        {
            String colInUse = findColumnInColumns(UserSpecifiedFields, colName);
            if (colInUse == null)
            {
                return false;
            }

            UserSpecifiedFields.Remove(colInUse);
            AvailableColumns.Add(colInUse);
            return true;
        }

        private String findColumnInColumns(ObservableCollection<String> cols, String colName)
        {
            if (cols.Contains(colName))
                return colName;
            return null;
        }
    }
}

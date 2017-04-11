using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using CsvHelper;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace CollabClient
{
    // Next step, 
    // Add Exception to this table to show there are duplicate in key
    // Clean Trash 
    class DataFileBuilder
    {
        public class DataFileBuilderException : Exception
        {
            public DataFileBuilderException() { }

            public DataFileBuilderException(string message) : base(message) { }

            public DataFileBuilderException(string message, Exception inner) : base(message, inner) { }
        }
        private const String SQLITE_DB_LOCATION = "C:/Users/jxu259/Desktop/sqlite/exampleBack.db";
        private IDbConnection _sqliteConn;
        private SQLiteDataAdapter _adapter;
        private DataSet _workingDataSet;
        private CollabClientViewModel _viewModel;
        private DataTable[] _twinTables;
        private DataTable[] _familyTables;
        private DataTable[] _genTables;

        public DataFileBuilder(SQLiteConnection sqlite, CollabClientViewModel viewModel)
        {
            _sqliteConn = sqlite;
            _adapter = new SQLiteDataAdapter();
            _adapter.SelectCommand = new SQLiteCommand((SQLiteConnection)this._sqliteConn);
            _workingDataSet = new DataSet();
            _viewModel = viewModel;
        }

        public void connectToDB()
        {
            this._sqliteConn.Open();
        }

        // Several data tables will get merged together and form a large table with all of their fields. 
        // That large table will be returned, with the user specified tablename to the main controller for further process
        public DataTable buildDataFile_origin(DataTables dataTables, String outputTableName)
        {
            
            //Make it easy to manipulate data
            DataTable[] dts = this.getCorrespondingDataTablesFromDataTables(dataTables);

            // add twin column to the family table, which doesn't have column twin, making it possible to merge with a static primary key
            this.addTwinColumnToFamilyTable(dts);

            DataTable outputDataTable = new DataTable(outputTableName);
            outputDataTable = this.mergeAllDataTablesToOutputTable_origin(dts, outputDataTable);
            // the tablename has to be the user specificed name. It's an essential step for output manager.
            outputDataTable.TableName = outputTableName;
            _workingDataSet.Tables.Add(outputDataTable);

            // Append fields to the data
            outputDataTable = appendColumns(outputDataTable);
            return outputDataTable;
        }

        // Several data tables will get merged together and form a large table with all of their fields. 
        // That large table will be returned, with the user specified tablename to the main controller for further process
        public DataTable buildDataFile(DataTables dataTables, String outputTableName)
        {

            // Rewrite the code 
            // Seperate tables into 3 lists. Twin tables, Family tables and General tables based on the type property in Table class 
            //Make it easy to manipulate data
            this.populateCorrespondingDataTableListsFromDataTables(dataTables);

            // Check whether there are the same fields among some tables, append data // Exclude familyid and twin
            this.differentiateDuplicateColumnNameForDataTables();

            // add twin column to the family table, which doesn't have column twin, making it possible to merge with a static primary key
            //this.addTwinColumnToFamilyTable(dts);
            DataTable outputDataTable = new DataTable(outputTableName);
            outputDataTable = this.mergeAllDataTablesToOutputTable();

            // the tablename has to be the user specificed name. It's an essential step for output manager.
            outputDataTable.TableName = outputTableName;
            _workingDataSet.Tables.Add(outputDataTable);

            // Append fields to the data
            outputDataTable = appendColumns(outputDataTable);
            return outputDataTable;
        }

        /// <summary>
        /// Populate to different lists provided in the data table
        /// </summary>
        /// <param name="dataTables"></param>
        public void populateCorrespondingDataTableListsFromDataTables(DataTables dataTables)
        {
            List<DataTable> twinTables = new List<DataTable>();
            List<DataTable> familyTables = new List<DataTable>();
            List<DataTable> genTables = new List<DataTable>();

            foreach (Table t in dataTables)
            {
                switch (t.Type){
                    case Table.TableType.FamilyTable:
                        familyTables.Add(getTable(t));
                        break;

                    case Table.TableType.TwinTable:
                        twinTables.Add(getTable(t));
                        break;

                    case Table.TableType.GeneralTable:
                        genTables.Add(getTable(t));
                        break;
                }   
            }
            _twinTables = twinTables.ToArray<DataTable>();
            _familyTables = familyTables.ToArray<DataTable>();
            _genTables = genTables.ToArray<DataTable>();
        }

        private void differentiateDuplicateColumnNameForDataTables()
        {
            HashSet<String> colPool = new HashSet<String>();
            DataTable[][] tableLists = {_twinTables, _familyTables, _genTables};
            foreach (DataTable[] tableList in tableLists)
            {
                // iterator each table list to check duplicate 
                foreach (DataTable t in tableList){
                    differentiateDuplicateColumnForEachTableExceptKey(t, colPool);
                }
            }
        }

        private void differentiateDuplicateColumnForEachTableExceptKey(DataTable t, HashSet<String> colPool)
        {
            // HashSet is fast for checking duplicate item
            HashSet<String> duplicateAllowedCols = new HashSet<String>();
            duplicateAllowedCols.Add("twin");
            duplicateAllowedCols.Add("familyid");
            
            foreach (DataColumn col in t.Columns)
            {
                if (duplicateAllowedCols.Contains(col.ColumnName))
                    continue;
                else
                {
                    // If the duplicate key is found, append table name after the origin table 
                    if (colPool.Contains(col.ColumnName))
                        col.ColumnName = $"{col.ColumnName}_{t.TableName}";
                    //col.ColumnName = String.Format("{0}_{1}", col.ColumnName, t.TableName);
                    colPool.Add(col.ColumnName);        
                }
            }
        }
        private DataTable appendColumns(DataTable outputDataTable)
        {
            if (_viewModel.AppendGender)
                outputDataTable = appendSpecificColumn(outputDataTable, "gender", "gen_twins", new String[]{ "familyid", "twin"});
            if (_viewModel.AppendRdSelect)
                outputDataTable = appendSpecificColumn(outputDataTable, "rdselect", "gen_twins", new String[] { "familyid", "twin"});
            if (_viewModel.AppendZyg)
                outputDataTable = appendSpecificColumn(outputDataTable, "zygclean", "user_zyg_final_qu_observation", new String[] {"familyid"});
            return outputDataTable;
        }

        private DataTable appendSpecificColumn(DataTable outputDataTable, string column, String sourceTableName, String[] join_keys)
        {
            _adapter.SelectCommand.CommandText = String.Format($"Select * from {sourceTableName}");
            if (!_workingDataSet.Tables.Contains(sourceTableName))
                _adapter.Fill(_workingDataSet, sourceTableName);

            DataTable sourceDataTable = _workingDataSet.Tables[sourceTableName];
            DataTable newDataTable = new DataTable(outputDataTable.TableName);
           
            // Need to hard code the key for join. For gen_twins, we need to keys 
            // For other tables, user needs to specify the key.
            DataRelation drel = relationBasedOnKeys(sourceDataTable, outputDataTable, join_keys);
            _workingDataSet.Relations.Add(drel);
        
            // Add column to the new table
            foreach (DataColumn col in outputDataTable.Columns)
                newDataTable.Columns.Add(col.ColumnName, col.DataType);
            DataColumn targetCol = sourceDataTable.Columns[column];
            newDataTable.Columns.Add(targetCol.ColumnName, targetCol.DataType);

            // Join two table by using data relationship. 
            foreach (DataRow row in outputDataTable.Rows)
            {
                DataRow joint = row.GetParentRow(drel);
                
                DataRow current = newDataTable.NewRow();
                foreach (DataColumn col in outputDataTable.Columns)
                {
                    current[col.ColumnName] = row[col.ColumnName];
                }

                if (joint != null)
                    current[column] = joint[column];
                newDataTable.Rows.Add(current);
            }
            _workingDataSet.Relations.Remove(drel);
            _workingDataSet.Tables.Remove(outputDataTable);
            _workingDataSet.Tables.Add(newDataTable);
            return newDataTable;

            DataRelation relationBasedOnKeys(DataTable parent, DataTable child, String[] keys)
            {
                DataColumn[] parentColumns = keys.Select(key => parent.Columns[key]).ToArray<DataColumn>();
                DataColumn[] childColumns = keys.Select(key => child.Columns[key]).ToArray<DataColumn>();
                // this variable "column" comes from the argument of the out function
                return new DataRelation(column, parentColumns, childColumns, false);
            }
        }

        private DataTable[] getCorrespondingDataTablesFromDataTables(DataTables userTables)
        {
            return userTables.Select(table => getTable(table)).ToArray();
        }

        private DataTable getTable(Table table)
        {
            _adapter.SelectCommand.CommandText = getSelectSql(table);
            _adapter.Fill(_workingDataSet, table.TableName);
            return _workingDataSet.Tables[table.TableName];
        }

        private String getSelectSql(Table dataTable)
        {

            if (dataTable.DoUseFullFields)
                return String.Format("SELECT * FROM {0}", dataTable.TableName);
            
            // TODO: interact with the fields in Table to generate the select command for subset of fields
            return getSelectSqlForSpecificFields(dataTable.UserSpecifiedFields, dataTable.TableName);

            //return String.Format("SELECT * FROM {0}", dataTable.TableName);
        }

        private String getSelectSqlForSpecificFields(ObservableCollection<String> fields, String tableName)
        {
            return String.Format("SELECT {0} FROM {1} ", String.Join(", ", fields.ToArray()), tableName);
        }

        private void addTwinColumnToFamilyTable(DataTable[] dts)
        {
            foreach (DataTable dt in dts)
            {
                if (!isTwinTable(dt))
                {   
                    //The twin column will have default value as 0. 
                    //Thus, in the final file, the twin 0 stands for the data from family table(potentially information about the caregiver)
                    addTwinColToTable(dt);
                }
            }
        }

        private void addTwinColToTable(DataTable dt)
        {
            DataColumn twinCol = new DataColumn("twin", typeof(Int32));
            twinCol.DefaultValue = 0;
            dt.Columns.Add(twinCol);
        }

        private bool isTwinTable(DataTable dt)
        {
            List<String> columns = new List<String>();
            foreach (DataColumn col in dt.Columns)
            {
                columns.Add(col.ColumnName);
            }

            if (columns.Contains("familyid") && columns.Contains("twin"))
            {
                Debug.WriteLine("This is a twin table");
                return true;
            }
            else if (columns.Contains("familyid") && !columns.Contains("twin"))
            {
                Debug.WriteLine("This is a family table");
                return false;
            }

            throw new DataFileBuilderException("The data table doesn't have familyid");
        }
        /// <summary>
        ///  This require a big change, by implementing a inner join function in C# (Anticipate it to be really slow =.=)
        /// </summary>
        /// <returns></returns>
        private DataTable mergeAllDataTablesToOutputTable()
        {
            
            DataTable twinTable = this.mergeTwinTable();
            DataTable familyTable = this.mergeFamilyTable();

            if (twinTable == null)
                return familyTable;

            if (familyTable == null)
                return twinTable;

            // now merge family table by only using familyid as primary key
       /*     if (outputTable != null)
            {
                this.setFamilyidAsPrimaryKey(outputTable);
            }*/

            return innerJoinTwinWithFamily(twinTable,familyTable);
        }

        private DataTable mergeTwinTable()
        {
            DataTable outputTable = null;
            int count = 0;

            // Set the primary keys for twin tables. Then merge
            foreach (DataTable t in _twinTables)
            {
                if (outputTable == null)
                {
                    outputTable = t;
                    this.setFamilyidAndTwinAsPrimaryKey(outputTable);
                    continue;
                }
                this.setFamilyidAndTwinAsPrimaryKey(t);
                outputTable.Merge(t, true, MissingSchemaAction.AddWithKey);
            }
            return outputTable != null ? outputTable.Copy() : null;
        }

        private DataTable mergeFamilyTable()
        {
            DataTable outputTable = null;
            foreach (DataTable t in _familyTables)
            {
                if (outputTable == null)
                {
                    outputTable = t;
                    this.setFamilyidAsPrimaryKey(outputTable);
                    continue;
                }
                this.setFamilyidAndTwinAsPrimaryKey(t);
                outputTable.Merge(t, true, MissingSchemaAction.AddWithKey);
            }

            return outputTable != null ? outputTable.Copy() : null;
        }

        private DataTable innerJoinTwinWithFamily(DataTable twinTable, DataTable familyTable)
        {
            DataTable newDataTable = new DataTable();
            newDataTable.TableName = "newTable";
            twinTable.TableName = "twinTable";
            familyTable.TableName = "familyTable";

            // Prepare for Inner join Data
            _workingDataSet.Tables.Add(twinTable);
            _workingDataSet.Tables.Add(familyTable);
            DataRelation drel = new DataRelation("sameFamily", new DataColumn[] { familyTable.Columns["familyid"]},
                 new DataColumn[] { twinTable.Columns["familyid"]}, false);
            _workingDataSet.Relations.Add(drel);

            // TODO: finish the inner join
            // add columns 
            foreach (DataColumn col in twinTable.Columns)
                newDataTable.Columns.Add(col.ColumnName, col.DataType);

            foreach (DataColumn col in familyTable.Columns)
            {
                if (col.ColumnName!= "familyid")
                    newDataTable.Columns.Add(col.ColumnName, col.DataType);
            }

            foreach (DataRow row in twinTable.Rows)
            {
                DataRow newRow = newDataTable.NewRow();
                foreach (DataColumn col in twinTable.Columns) 
                    newRow[col.ColumnName] = row[col.ColumnName];
                DataRow parentRow = row.GetParentRow("sameFamily");
                if (parentRow != null)
                    foreach (DataColumn col in familyTable.Columns)
                        newRow[col.ColumnName] = parentRow[col.ColumnName];
                newDataTable.Rows.Add(newRow);
            }

            return newDataTable;
        }
        private DataTable mergeAllDataTablesToOutputTable_origin(DataTable[] dts, DataTable outputDT)
        {           

            for (int i = 0; i < dts.Length; i++)
            {
                // Use the first table as the base table
                if (i == 0)
                {
                    outputDT = dts[0];
                    this.setFamilyidAndTwinAsPrimaryKey(outputDT);
                    continue;
                }

                this.setFamilyidAndTwinAsPrimaryKey(dts[i]);
                outputDT.Merge(dts[i], true, MissingSchemaAction.AddWithKey);
            }
            return outputDT.Copy();
        }

        public DataTable setFamilyidAndTwinAsPrimaryKey(DataTable dt)
        {
            // TODO: More about different table. Some tables have weird way of setting primary key. e.g data_3_bc_s. Most of them are fine
            try {
                dt.PrimaryKey = new DataColumn[] { dt.Columns["familyid"], dt.Columns["twin"] };
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(String.Format("This data table: {0} has duplicate primary key.", dt.TableName));
            }
            
            return dt;
        }

        public DataTable setFamilyidAsPrimaryKey(DataTable dt)
        {
            // TODO: More about different table. Some tables have weird way of setting primary key. e.g data_3_bc_s. Most of them are fine
            try
            {
                dt.PrimaryKey = new DataColumn[] { dt.Columns["familyid"]};
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(String.Format("This data table: {0} has duplicate primary key.", dt.TableName));
            }
            return dt;
        }


    }
}

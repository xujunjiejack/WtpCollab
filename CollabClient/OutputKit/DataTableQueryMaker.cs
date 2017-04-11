using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Collections;

namespace CollabClient
{
    class DataTableQueryMaker
    {
        private DataTable _dt;
        private String[] _keys; // To store the primary keys for the table
        private String _stringEnclosing;

        public DataTableQueryMaker(): this(null, null)
        {
        }

        public DataTableQueryMaker(DataTable dt): this(dt, null)
        {
        }

        public DataTableQueryMaker(DataTable dt, String[] keys)
        {
            _dt = dt;
            _keys = keys;
        }

        public String makeCreateStatementFor(String tableName){
            String createStmt = String.Format("CREATE TABLE IF NOT EXISTS `{0}` ({1}, {2});",
                                                tableName, 
                                                createFieldsStatements(), 
                                                getPrimaryKey());  // "CREATE TABLE tablename (Column stmt, primary key stmt);";
            return createStmt;
        }

        private String createFieldsStatements()
        {
            List<String> colStmts = new List<String>();
            foreach (DataColumn col in _dt.Columns)
            {
                colStmts.Add(getOneColStmt(col));
            }

            return String.Join(", ", colStmts.ToArray());
        }

        private String getOneColStmt(DataColumn col)
        {
            String fieldType = getColFieldType(col.DataType);
            String constraint = "";

            // Key can't be null
            if (_keys.Contains<String>(col.ColumnName))
            {
                constraint = "NOT NULL";
            }

            return String.Format("`{0}` {1} {2}", col.ColumnName, fieldType, constraint);
        }

        private String getColFieldType(Type dataType)
        {
            if (dataType.Equals(typeof(System.Int32)))
            {
                return "int(10)";
            }

            if (dataType.Equals(typeof(String)))
            {
                return "varchar(50)";
            }

            if (dataType.Equals(typeof(Double))){
                return "float";
            }

            throw new ArgumentException("We currently don't support this data type: " + dataType.ToString());
        }

        private String getPrimaryKey()
        {
            return String.Format("PRIMARY KEY ({0})",string.Join(",", _keys));
        }

        public String getInsertStatmentForEachRow(DataRow row)
        {
            List<String> fieldNames = new List<String>();
            List<String> fieldData = new List<String>();

            foreach (DataColumn col in _dt.Columns)
            {
                fieldNames.Add("\""+ col.ColumnName + "\"");
                fieldData.Add(getDataInStringBasedOnDifferentType(row, col));
            }

            return String.Format("INSERT INTO `{0}` ({1}) VALUES ({2});", _dt.TableName, 
                                                                        string.Join(",", fieldNames.ToArray()),
                                                                        string.Join(",", fieldData.ToArray()));

        }

        public void setEnclosingString(String delimiter)
        {
            _stringEnclosing = delimiter;
        }

        public String getDataInStringBasedOnDifferentType(DataRow row, DataColumn col){
            
            if (row[col.ColumnName].GetType().Equals(typeof(DBNull)))
            {
                return "NULL";
            }

            if (col.DataType.Equals(typeof(String)))
            {
                // TODO: Need data validation for cleaning double quote ""
                // Need to change the closing symbol for string.
                return String.Format(_stringEnclosing+ "{0}" + _stringEnclosing, row[col.ColumnName]);
            }

            if (col.DataType.Equals(typeof(Int32)) || col.DataType.Equals(typeof(double)))
            {
                return row[col.ColumnName].ToString();
            }

            throw new Exception(String.Format("The data can't be casted to String: Col: {0}, Data: {1}", col.ColumnName, row[col.ColumnName]));
        }
    }
}

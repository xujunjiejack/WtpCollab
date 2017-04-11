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
    abstract class DatabaseExit: TableExit
    {
        // Place holder
        // executeSql is a function that needs to be implemented for other part of the work to work
 
        protected IDbTransaction _transaction; // Need to be defined with the connection to improve the speed of database writing.

        protected String databaseName =""; // Used for debugging information

        public void outputTableThroughExit(DataTable dt)
        {
            Debug.WriteLine(String.Format("Data is writing into {0}", databaseName));
            // More error handling needs to be done. 
            DataTableQueryMaker maker = new DataTableQueryMaker(dt, primaryKeyBasedOnTable(dt));
            setUpMaker(maker);
            executeSql(maker.makeCreateStatementFor(dt.TableName));
            insertDataTable(dt, maker);
        }

        public String[] primaryKeyBasedOnTable(DataTable dt)
        {
            var candidates = new String[]{ "familyid", "twin" };
            return candidates.Where(x => dt.Columns.Contains(x)).ToArray();
        }

        /// <summary>
        /// Configure the DataTableQueryMaker for different kinds of Database
        /// </summary>
        /// <param name="maker"></param>
        abstract protected void setUpMaker(DataTableQueryMaker maker);

        /// <summary
        ///  executeSql is a function that needs to be implemented for other part of the class to work.
        ///  It needs to be able to receive a query, and execute the query.
        /// </summary>
        /// <param name="sqlQuery"></param>
        abstract protected void executeSql(String sqlQuery);
        
        /// <summary>
        /// Use QueryMaker to insert the statment into the sql database given a DataTable
        /// </summary>
        /// <param name="dt">DataTable with the data</param>
        /// <param name="maker"> Query Maker</param>
        private void insertDataTable(DataTable dt, DataTableQueryMaker maker)
        {   
            // Improve performance
            int rowNum = 0;
            foreach (DataRow row in dt.Rows)
            {
                Debug.WriteLine(rowNum++);
                try
                {
                     executeSql(maker.getInsertStatmentForEachRow(row));
                }
                // TODO: ADD MORE ERROR HANDLING.
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    continue;
                }
            }
            _transaction.Commit();

        }
    }
}

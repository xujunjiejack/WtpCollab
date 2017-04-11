using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Odbc;

namespace CollabClient
{
    class MySQLDatabaseExit: DatabaseExit
    {
        private OdbcConnection _odbcConn;

        public MySQLDatabaseExit(OdbcConnection odbcConn)
        {
            _odbcConn = odbcConn;
            _transaction = odbcConn.BeginTransaction();
            databaseName = "MySQL";
        }

        protected override void setUpMaker(DataTableQueryMaker maker)
        {
            maker.setEnclosingString("\'");
        }

        protected override void executeSql(String sqlQuery)
        {
            OdbcCommand odbcCmd = new OdbcCommand(sqlQuery, _odbcConn, (OdbcTransaction)_transaction);
            odbcCmd.ExecuteNonQuery();
        }
    }
}

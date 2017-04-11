using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

/// <summary>
/// Subclass for Database Exit to support the SQLite. Most of the logic function is in Database Exit
/// </summary>
namespace CollabClient
{
    class SQLiteDatabaseExit: DatabaseExit
    {
        private SQLiteConnection _sqliteCon;
        

        public SQLiteDatabaseExit(SQLiteConnection sqliteConn)
        {
            _sqliteCon = ConnectionCoordinater.SharedConnectionCoordinator.useSqliteConn();
            _transaction = _sqliteCon.BeginTransaction();
            databaseName = "SQLite";
        }

        protected override void setUpMaker(DataTableQueryMaker maker)
        {
            maker.setEnclosingString("\"");
        }

        protected override void executeSql(String sqlQuery){
            SQLiteCommand cmd = new SQLiteCommand(sqlQuery, _sqliteCon, (SQLiteTransaction)_transaction);
            cmd.ExecuteNonQuery();
        }
    }
}

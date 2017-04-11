using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Data.Odbc;

namespace CollabClient
{
    /// <summary>
    /// This class regulates the database connection for both sqlite and mysql.
    /// It will does some error handling. If wtp_data can't be reached, then it will return the signal to the 
    /// main function.
    /// </summary>
    /// 

    public class FailureSqliteConnectionException : Exception
    {
         public FailureSqliteConnectionException()
    {
    }

         public FailureSqliteConnectionException(string message)
        : base(message)
    {
    }

            public FailureSqliteConnectionException(string message, Exception inner)
        : base(message, inner)
    {
    }
    }

    public class ConnectionCoordinater
    {
        private SQLiteConnection _sqliteConn;
        private OdbcConnection _odbcConn;
        private const String SQLITE_DB_LOCATION = "O:/wtp_collab.db";

        // Just be careful, it's not safe to open odbc here, because we are exposing our password and id here
        private const String MYSQL_DB_DSN = "wtp_data";
        private const String MYSQL_DB_UID = "wtpadmin";
        private const String MYSQL_DB_PWD = "Mh&g@1U";

        private static readonly ConnectionCoordinater singleton = new ConnectionCoordinater();
        public static ConnectionCoordinater SharedConnectionCoordinator
        {
            get
            {
                return singleton;
            }       
        }

        public ConnectionCoordinater()
        {
            _sqliteConn = new SQLiteConnection(String.Format("Data source = {0}", SQLITE_DB_LOCATION));
            _odbcConn = new OdbcConnection(String.Format("DSN={0};UID={1};PWD={2}", MYSQL_DB_DSN, MYSQL_DB_UID, MYSQL_DB_PWD)); 
        }

        public SQLiteConnection sqliteConn
        {
            get
            {
                return _sqliteConn;
            }
        }
        
        public OdbcConnection odbcConn 
        {
            get
            {
                return _odbcConn;
            }
        }

        public SQLiteConnection useSqliteConn()
        {
            if (_sqliteConn.State == ConnectionState.Closed && _sqliteConn.State != ConnectionState.Connecting)
            {
                /*try
                {
                    _sqliteConn.Open();
                }
                catch (InvalidOperationException e)
                {
                    throw new FailureSqliteConnectionException("Sqlite Connection is lost. Check the data source");
                }*/
                _sqliteConn.Open();
            }
            return _sqliteConn;
        }

        /// <summary>
        /// This method will check whether it's possible to make connection for both database. 
        /// Or do I need it?
        /// </summary>
        public void checkConnection()
        {

        }
    }
}

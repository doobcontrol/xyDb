
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.SQLite;
using System.Xml;

namespace xy.Db.SQLite64
{
    public class SQLite64DbAccess : IDbAccess
    {
        static private SQLiteConnection conn;

        static private readonly object lockObj = new object();
        private SQLiteConnection getConn()
        {
            lock (lockObj)
            {
                if (conn == null || conn.State == ConnectionState.Closed)
                {
                    conn = new SQLiteConnection(connectionString);
                    conn.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "PRAGMA foreign_keys=ON;";
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return conn;
        }

        private string connectionString;

        #region IDbAccess Members
        public async Task OpenAsync(string ConnectionString)
        {
            this.connectionString = ConnectionString;
        }

        public async Task OpenForAdminAsync(Dictionary<string, string> dpPars)
        {
        }

        public async Task Close()
        {
        }

        public bool DbExist(Dictionary<string, string> dpPars)
        {
            return File.Exists(dpPars[DbService.pn_dbName]);
        }

        public string DbCreate(Dictionary<string, string> dpPars)
        {
            if (File.Exists(dpPars[DbService.pn_dbName]))
            {
                File.Delete(dpPars[DbService.pn_dbName]);
            }

            string directoryName =
                System.IO.Path.GetDirectoryName(
                new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            
            string ConnectionString = "Data Source=" + directoryName + "/" 
                + dpPars[DbService.pn_dbName] + ";";

            //create database
            connectionString = ConnectionString;
            exeSql(dpPars[DbService.pn_dbScript]);

            return ConnectionString;
        }

        public void BeginTrans()
        {
            throw new NotImplementedException();
        }

        public void CommitTrans()
        {
            throw new NotImplementedException();
        }

        public void RollbackTrans()
        {
            throw new NotImplementedException();
        }

        public async Task exeSql(string strSql)
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    SQLiteConnection tempConn = getConn();
                    cmd.Connection = tempConn;
                    cmd.CommandText = strSql;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqliteException ex)
            {
                throw new Exception(ex.Message + "：" + strSql);
            }
        }

        public async Task<DataSet> exeSqlForDataSet(string QueryString)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    //using (SQLiteConnection tempConn = getConn())   //会对tempComm执行Dispose
                    //{
                    SQLiteConnection tempConn = getConn();
                    cmd.CommandTimeout = 300;  //大数据量查询时会超时
                    cmd.Connection = tempConn;
                    SQLiteDataAdapter ad = new SQLiteDataAdapter();
                    cmd.CommandText = QueryString;//.ToUpper();  //为何加ToUpper()？？？
                    ad.SelectCommand = cmd;
                    ad.Fill(ds);
                }
            }
            catch (SqliteException ex)
            {
                throw new Exception(ex.Message + "：" + QueryString);
            }

            return ds;
        }
        #endregion

    }
}

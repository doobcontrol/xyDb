using System.Data;
using System.Data.SqlClient;

namespace xy.Db.SQLServer
{
    public class SQLServerDbAccess : IDbAccess
    {
        SqlConnection conn;

        #region IDbAccess Members
        public async Task OpenAsync(string ConnectionString)
        {
            // Connect to the PostgreSQL server
            conn = new SqlConnection(ConnectionString);
            //await conn.OpenAsync();
            await conn.OpenAsync();
        }

        public async Task OpenForAdminAsync(Dictionary<string, string> dpPars)
        {
            // Connect to the SQLServer server for admin tasks(create new database)
            string connString =
                "Server=" + dpPars[DbService.pn_dbServer] + ";"
                + "database=" + dpPars[DbService.pn_dbName] + ";"
                + "uid=" + dpPars[DbService.pn_dbUser] + ";"
                + "pwd=" + dpPars[DbService.pn_dbPassword] + ";";

            conn = new SqlConnection(connString);

            await conn.OpenAsync();
        }

        public async Task Close()
        {
            if (conn != null)
            {
                await conn.CloseAsync();
            }
        }

        public bool DbExist(Dictionary<string, string> dpPars)
        {
            return false;
        }

        public async Task<string> DbCreate(Dictionary<string, string> dpPars)
        {
            //create user and database (check exist before?)
            string sqlStr;

            //create database
            sqlStr = " use master; ";
            sqlStr += " create database " 
                + dpPars[DbService.pn_dbName] + "; ";
            //create user
            sqlStr += " USE master; ";
            sqlStr += " if exists (select 1 from  syslogins where name='" 
                + dpPars[DbService.pn_dbUser] + "') ";
            sqlStr += " begin ";
            sqlStr += " 	EXEC sp_droplogin '" 
                + dpPars[DbService.pn_dbUser] + "' ";
            sqlStr += " end; ";
            sqlStr += " EXEC sp_addlogin " 
                + dpPars[DbService.pn_dbUser] + "," 
                + dpPars[DbService.pn_dbPassword] + "," 
                + dpPars[DbService.pn_dbName] + ";  ";
            await exeSql(sqlStr);

            sqlStr = " USE " + dpPars[DbService.pn_dbName] + "; ";
            sqlStr += " EXEC sp_changedbowner '" 
                + dpPars[DbService.pn_dbUser] + "'; ";
            await exeSql(sqlStr);

            string ConnectionString = "";
            ConnectionString += "Server=" 
                + dpPars[DbService.pn_dbServer] + ";";
            ConnectionString += "uid=" 
                + dpPars[DbService.pn_dbUser] + ";";
            ConnectionString += "pwd=" 
                + dpPars[DbService.pn_dbPassword] + ";";
            ConnectionString += "database=" 
                + dpPars[DbService.pn_dbName] + ";";
            ConnectionString += "Packet Size=8192;Max Pool Size=1000;Connect Timeout=30;"; 

            //create tabes
            await Close();
            await OpenAsync(ConnectionString);
            await exeSql(dpPars[DbService.pn_dbScript]);

            return ConnectionString;
        }

        public void BeginTrans()
        {
        }

        public void CommitTrans()
        {
            ;
        }

        public void RollbackTrans()
        {
        }

        public async Task exeSql(string strSql)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlConnection tempConn = conn;
                    cmd.Connection = tempConn;
                    cmd.CommandText = strSql;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message + "：" + strSql);
            }
        }

        public async Task<DataSet> exeSqlForDataSet(string QueryString)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlConnection tempConn = conn;
                    cmd.CommandTimeout = 300;
                    cmd.Connection = tempConn;
                    SqlDataAdapter ad = new SqlDataAdapter();
                    cmd.CommandText = QueryString;
                    ad.SelectCommand = cmd;
                    ad.Fill(ds);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message + "：" + QueryString);
            }

            return ds;
        }
        #endregion
    }
}

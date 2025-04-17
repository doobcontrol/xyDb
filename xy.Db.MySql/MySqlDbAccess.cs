using MySql.Data.MySqlClient;
using System.Data;

namespace xy.Db.MySql
{
    public class MySqlDbAccess : IDbAccess
    {
        MySqlConnection conn;

        #region IDbAccess Members
        public async Task OpenAsync(string ConnectionString)
        {
            // Connect to the MySql server
            conn = new MySqlConnection(ConnectionString);
            await conn.OpenAsync();
        }

        public async Task OpenForAdminAsync(Dictionary<string, string> dpPars)
        {
            // Connect to the PostgreSQL server for admin tasks(create new database)
            string connString =
                "server=" + dpPars[DbService.pn_dbServer] + ";"
                + "database=" + dpPars[DbService.pn_dbName] + ";"
                + "uid=" + dpPars[DbService.pn_dbUser] + ";"
                + "pwd=" + dpPars[DbService.pn_dbPassword] + ";";
            conn = new MySqlConnection(connString);

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
            sqlStr = " create database "
                + dpPars[DbService.pn_dbName] + "; ";
            //create user
            sqlStr += " CREATE USER '"
                + dpPars[DbService.pn_dbUser]
                + "'@'" + dpPars[DbService.pn_dbServer]
                + "' IDENTIFIED BY '" + dpPars[DbService.pn_dbPassword] + "'; ";
            sqlStr += " GRANT ALL ON "
                + dpPars[DbService.pn_dbName] + ".* TO '"
                + dpPars[DbService.pn_dbUser] 
                + "'@'" + dpPars[DbService.pn_dbServer] 
                + "'; ";
            await exeSql(sqlStr);

            string ConnectionString = "";
            ConnectionString += "server="
                + dpPars[DbService.pn_dbServer] + ";";
            ConnectionString += "uid="
                + dpPars[DbService.pn_dbUser] + ";";
            ConnectionString += "pwd="
                + dpPars[DbService.pn_dbPassword] + ";";
            ConnectionString += "database="
                + dpPars[DbService.pn_dbName] + ";";

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
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    MySqlConnection tempConn = conn;
                    cmd.Connection = tempConn;
                    cmd.CommandText = strSql;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(ex.Message + "：" + strSql);
            }
        }

        public async Task<DataSet> exeSqlForDataSet(string QueryString)
        {
            DataSet ds = new DataSet();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    MySqlConnection tempConn = conn;
                    cmd.CommandTimeout = 300;
                    cmd.Connection = tempConn;
                    MySqlDataAdapter ad = new MySqlDataAdapter();
                    cmd.CommandText = QueryString;
                    ad.SelectCommand = cmd;
                    ad.Fill(ds);
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(ex.Message + "：" + QueryString);
            }

            return ds;
        }


        #region Called by the client to determine how to build script

        public bool createForeignKeyWhenCreateTable()
        {
            return false;
        }
        public bool createForeignKeyAfterCreateTable()
        {
            return true;
        }

        #endregion

        #endregion
    }
}

using Npgsql;
using System.Data;
using System.Threading.Tasks;
using static Npgsql.FSharp.Sql.ExecutionTarget;

namespace xy.Db.PostgreSQL
{
    public class PostgreSQLDbAccess : IDbAccess
    {
        NpgsqlConnection conn;

        #region IDbAccess Members
        public async Task OpenAsync(string ConnectionString)
        {
            // Connect to the PostgreSQL server
            conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
        }

        public async Task OpenForAdminAsync(Dictionary<string, string> dpPars)
        {
            // Connect to the PostgreSQL server for admin tasks(create new database)
            string connString =
                "Server=" + dpPars[DbService.pn_dbServer] + ";"
                + "Database=" + dpPars[DbService.pn_dbName] + ";"
                + "User Id=" + dpPars[DbService.pn_dbUser] + ";"
                + "Password=" + dpPars[DbService.pn_dbPassword] + ";";
            conn = new NpgsqlConnection(connString);

            //await conn.OpenAsync();
            try
            {
                conn.Open();
            }
            catch(Exception e)
            {
                throw e;
            }
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

        public string DbCreate(Dictionary<string, string> dpPars)
        {
            return null;
        }

        public void BeginTrans()
        {
        }

        public void CommitTrans()
        {;
        }

        public void RollbackTrans()
        {
        }

        public async Task exeSql(string strSql)
        {
            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand())
                {
                    NpgsqlConnection tempConn = conn;
                    cmd.Connection = tempConn;
                    cmd.CommandText = strSql;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (NpgsqlException ex)
            {
                throw new Exception(ex.Message + "：" + strSql);
            }
        }

        public async Task<DataSet> exeSqlForDataSet(string QueryString)
        {
            DataSet ds = new DataSet();
            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand())
                {
                    NpgsqlConnection tempConn = conn;
                    cmd.CommandTimeout = 300;
                    cmd.Connection = tempConn;
                    NpgsqlDataAdapter ad = new NpgsqlDataAdapter();
                    cmd.CommandText = QueryString;
                    ad.SelectCommand = cmd;
                    ad.Fill(ds);
                }
            }
            catch (NpgsqlException ex)
            {
                throw new Exception(ex.Message + "：" + QueryString);
            }

            return ds;
        }
        #endregion
    }
}

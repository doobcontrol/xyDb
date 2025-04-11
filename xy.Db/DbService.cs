using System.Data;

namespace xy.Db
{
    public class DbService
    {
        private IDbAccess db;

        public static string pn_dbName = "dbName";
        public static string pn_dbScript = "dbScript";

        public static string pn_dbConnStr = "dbConnStr";
        public static string pn_dbServer = "dbServer";
        public static string pn_dbUser = "dbUser";
        public static string pn_dbPassword = "dbPassword";

        private string connectionString;
        public DbService(string connStr, IDbAccess db)
        {
            this.db = db;
            connectionString = connStr;
        }
        public DbService(IDbAccess db)
        {
            this.db = db;
            connectionString = "";
        }
        public async Task openAsync()
        {
            await db.OpenAsync(connectionString);
        }
        public async Task create(Dictionary<string, string> dpPars)
        {
            await db.OpenForAdminAsync(dpPars);
            //await db.exeSql(dpPars[pn_dbScript]);
        }

        public bool DbExist(Dictionary<string, string> dpPars)
        {
            return db.DbExist(dpPars);
        }
        public string DbCreate(Dictionary<string, string> dpPars)
        {
            return db.DbCreate(dpPars);
        }


        public async Task<DataTable> exeSqlForDataSetAsync(string SqlStr)
        {
            return (await db.exeSqlForDataSet(SqlStr)).Tables[0];
        }

        public async Task exeSqlAsync(string SqlStr)
        {
            await db.exeSql(SqlStr);
        }

    }
}

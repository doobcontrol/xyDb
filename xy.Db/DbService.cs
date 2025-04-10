using System.Data;

namespace xy.Db
{
    public class DbService
    {
        private readonly IDbAccess db;

        public static string pn_dbName = "dbName";
        public static string pn_dbScript = "dbScript";

        public DbService(string connStr, IDbAccess db)
        {
            this.db = db;
            db.Open(connStr);
        }

        public bool DbExist(Dictionary<string, string> dpPars)
        {
            return db.DbExist(dpPars);
        }
        public string DbCreate(Dictionary<string, string> dpPars)
        {
            return db.DbCreate(dpPars);
        }


        public DataTable exeSqlForDataSet(string SqlStr)
        {
            return db.exeSqlForDataSet(SqlStr).Tables[0];
        }

        public void exeSql(string SqlStr)
        {
            db.exeSql(SqlStr);
        }

    }
}

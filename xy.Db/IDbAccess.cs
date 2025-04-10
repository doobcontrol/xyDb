using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xy.Db
{
    public interface IDbAccess
    {
        void Open(string ConnectionString);
        void OpenForAdmin(Dictionary<string, string> dpPars);
        void Close();

        bool DbExist(Dictionary<string, string> dpPars);
        string DbCreate(Dictionary<string, string> dpPars);

        void BeginTrans();
        void CommitTrans();
        void RollbackTrans();
        void exeSql(string strSql);
        DataSet exeSqlForDataSet(string QueryString);
    }
}

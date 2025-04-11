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
        Task OpenAsync(string ConnectionString);
        Task OpenForAdminAsync(Dictionary<string, string> dpPars);
        Task Close();

        bool DbExist(Dictionary<string, string> dpPars);
        Task<string> DbCreate(Dictionary<string, string> dpPars);

        void BeginTrans();
        void CommitTrans();
        void RollbackTrans();
        Task exeSql(string strSql);
        Task<DataSet> exeSqlForDataSet(string QueryString);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;

namespace com.xiyuansoft.DataBasePro
{
    public interface IDbAccess
    {
        void Open(string ConnectionString);                    //打开数据库连接（连接字符串）
        void OpenForAdmin(System.Collections.Hashtable dpPars);                    //打开数据库连接
        void OpenForAdmin(Dictionary<string, string> dpPars);                    //打开数据库连接



        void Close();                   //关闭数据库连接

        //数据定义
        //检查数据库是否存在
        bool DbExist(System.Collections.Hashtable dpPars);
        //创建数据库
        string DbCreate(System.Collections.Hashtable dpPars);
        string DbCreate(Dictionary<string, string> dpPars);
        //创建数据表
        void TableCreate(System.Xml.XmlDocument myXmlTableData);

        //数据操作
        void BeginTrans();              //开始一个事务
        void CommitTrans();             //提交一个事务
        void RollbackTrans();           //回滚一个事务
        void exeSql(string strSql, string[] strParams, object[] objValues);//执行Sql语句，没有返回值
        void exeSql(string strSql);
        DataSet exeSqlForDataSet(string QueryString, string[] strParams, object[] objValues);//执行Sql，返回DataSet
        DataSet exeSqlForDataSet(string QueryString);

        void exeSql(System.Xml.XmlDocument xmlSql);
        DataSet exeSqlForDataSet(System.Xml.XmlDocument xmlSql);
        string createSqlString(System.Xml.XmlDocument xmlSql);
    }
}

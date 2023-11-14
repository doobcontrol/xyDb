using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;

namespace com.xiyuansoft.DataBasePro
{
    public class DbService
    {
        static public string DBType = "DBType";
        static public string ConnStr = "ConnStr";

        static public string ConnectionString;
        static public string DataBaseType;

        private IDbAccess db;
        public DbService()
        {
            //子类被客户端程序使用时，无此参数，也不能做数据库操作（将来考虑把数据库操作，本类，与子类的数字定义分开）
            if (DataBaseType != null)
            {
                db = (IDbAccess)System.Activator.CreateInstance(System.Type.GetType(DataBaseType));
                db.Open(ConnectionString);
            }
        }

        public DbService(System.Collections.Hashtable dpPars)
        {
            db = (IDbAccess)System.Activator.CreateInstance(System.Type.GetType(DataBaseType));
            db.OpenForAdmin(dpPars);
        }

        public DbService(Dictionary<string, string> dpPars)
        {
            if (dpPars.ContainsKey(ConnStr))
            {
                db = (IDbAccess)System.Activator.CreateInstance(System.Type.GetType(dpPars[DBType]));
                db.Open(dpPars[ConnStr]);
            }
            else
            {
                db = (IDbAccess)System.Activator.CreateInstance(System.Type.GetType(dpPars[DBType]));
                db.OpenForAdmin(dpPars);
            }
        }

        public DbService(string oCString, string oDType)
        {
            db = (IDbAccess)System.Activator.CreateInstance(System.Type.GetType(oDType));
            db.Open(oCString);
        }

        ~DbService()
        {
            if (db != null)
            {
                db.Close();
            }
        }

        public void resetAccess()
        {
            db = (IDbAccess)System.Activator.CreateInstance(System.Type.GetType(DataBaseType));
            db.Open(ConnectionString);
        }

        //数据库是否存在
        public bool DbExist(System.Collections.Hashtable dpPars)
        {
            return db.DbExist(dpPars);
        }

        //创建数据库,返回，生成的数据库的连接字符串
        public string DbCreate(System.Collections.Hashtable dpPars)
        {
            return db.DbCreate(dpPars);
        }
        public string DbCreate(Dictionary<string, string> dpPars)
        {
            return db.DbCreate(dpPars);
        }

        //创建数据表
        public void TableCreate(System.Xml.XmlDocument myXmlTableData)
        {
            db.TableCreate(myXmlTableData);
        }

        public DataTable exeSqlForDataSet(string SqlStr)
        {
            return db.exeSqlForDataSet(SqlStr).Tables[0];
        }

        public void exeSql(string SqlStr)
        {
            db.exeSql(SqlStr);
        }

        public DataTable exeSqlForDataSet(System.Xml.XmlDocument xmlSql)
        {
            return db.exeSqlForDataSet(xmlSql).Tables[0];
        }

        public void exeSql(System.Xml.XmlDocument xmlSql)
        {
            db.exeSql(xmlSql);
        }

        //20151207   临时解决  非可靠代码
        public void BeginTrans()
        {
            db.BeginTrans();
        }

        public void CommitTrans()
        {
            db.CommitTrans();
        }
    }
}

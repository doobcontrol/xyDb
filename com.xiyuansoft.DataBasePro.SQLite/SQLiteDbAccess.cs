using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.xiyuansoft.DataBasePro.SQLite
{
    public class SQLiteDbAccess : com.xiyuansoft.DataBasePro.IDbAccess
    {
        static private SQLiteConnection conn;         //数据库连接，//直接对conn的调用在并发情况下可能出：
        //错误：There is already an open DataReader associated with this Command which must be closed first.

        static private readonly object lockObj = new object();
        private SQLiteConnection getConn()
        {
            //if (inTransaction)
            //{
            //    return getTransConn();
            //}

            //SQLiteConnection tempConn;
            //tempConn = new SQLiteConnection(connectionString);
            //tempConn.Open();
            lock (lockObj)
            {
                if (conn == null || conn.State == ConnectionState.Closed)
                {
                    conn = new SQLiteConnection(connectionString);
                    conn.Open();

                    //打开外键约束
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

        //事务的此解决方案是临时的   并发时会出问题
        private SQLiteConnection transconn; 
        private SQLiteConnection getTransConn()
        {
            //if (conn == null || conn.State == ConnectionState.Closed)
            //{
            //    conn = new SQLiteConnection(connectionString);
            //    conn.Open();
            //}

            return getConn();

            //if (transconn == null)
            //{
            //    SQLiteConnection tempConn;
            //    tempConn = new SQLiteConnection(connectionString);
            //    tempConn.Open();
            //    transconn = tempConn;
            //}
            //else if (transconn.State == ConnectionState.Closed)
            //{
            //    //怎么销毁？？

            //    SQLiteConnection tempConn;
            //    tempConn = new SQLiteConnection(connectionString);
            //    tempConn.Open();
            //    transconn = tempConn;
            //}

            //return transconn;
        }

        private string connectionString;

        private SQLiteTransaction trans;       //事务处理类
        private bool inTransaction = false;   //指示当前是否正处于事务中

        #region IDbAccess Members

        void IDbAccess.Open(string ConnectionString)
        {
            this.connectionString = ConnectionString;
        }

        void IDbAccess.OpenForAdmin(System.Collections.Hashtable dpPars)
        {
        }

        void IDbAccess.OpenForAdmin(Dictionary<string, string> dpPars)
        {
        }

        void IDbAccess.Close()
        {
            if (conn != null)
            {
                try //在WEB服务器中出错后导致服务器出错
                {
                    //conn.Close();
                    //conn.Dispose();
                }
                catch (Exception e)
                {
                    //???
                }
            }
            GC.Collect();
        }

        bool IDbAccess.DbExist(System.Collections.Hashtable dpPars)
        {
            return File.Exists(dpPars["dbName"].ToString());
        }

        string IDbAccess.DbCreate(System.Collections.Hashtable dpPars)
        {
            //先删除数据库（调试，有误删除用户其它系统数据库的危险）
            if (File.Exists(dpPars["dbName"].ToString()))
            {
                File.Delete(dpPars["dbName"].ToString());
            }

            string directoryName =
                System.IO.Path.GetDirectoryName(
                new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string ConnectionString = "";
            ConnectionString += "Data Source=" + directoryName + "/" + dpPars["dbName"].ToString() + ";Version=3;";

            return ConnectionString;
        }

        string IDbAccess.DbCreate(Dictionary<string, string> dpPars)
        {
            //先删除数据库（调试，有误删除用户其它系统数据库的危险）
            if (File.Exists(dpPars["dbName"].ToString()))
            {
                File.Delete(dpPars["dbName"].ToString());
            }

            string directoryName =
                System.IO.Path.GetDirectoryName(
                new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string ConnectionString = "";
            ConnectionString += "Data Source=" + directoryName + "/" + dpPars["dbName"].ToString() + ";Version=3;";

            return ConnectionString;
        }

        void IDbAccess.TableCreate(System.Xml.XmlDocument myXmlTableData)
        {
            string sqlStr = "";              //生成表的sql串
            string foreignsqlStr = "";     //生成外键的sql串
            string writeInfosqlStr = "";   //生成对象记录的sql串
            string initRecordsqlStr = "";  //写初始记录的sql串
            string fieldString;            //构建字段的sql串
            string primaryString;            //主键串

            //生成表
            foreach (System.Xml.XmlElement xmlTable in myXmlTableData.ChildNodes[0].ChildNodes)
            {
                primaryString = "";
                writeInfosqlStr += " insert into tObjInfo(fItemID,fObjName,fObjCode) values('"
                    + xmlTable.Attributes["bizModelId"].Value + "','"
                    + xmlTable.Attributes["name"].Value + "','"
                    + xmlTable.Attributes["code"].Value + "') ";

                sqlStr += " create table ";
                sqlStr += xmlTable.Attributes["code"].Value;
                sqlStr += " ( ";
                fieldString = "";
                foreach (System.Xml.XmlElement xmlRield in xmlTable.ChildNodes)
                {
                    if (xmlRield.Name == "field")
                    {
                        writeInfosqlStr += " insert into tPropertyInfo(fItemID,fObjInfoID,fFieldName,fFieldCode,fDataType,fIndex,fShowInList,fShowInForm,fEditable,fEdittype,fCommListID,fDataLength,fIsPrimaryKey,fIsForeignKey,fForeignKeyTable,fForeignKeyField,fForeignTableClass) values('";
                        writeInfosqlStr += System.Guid.NewGuid().ToString() + "','";
                        writeInfosqlStr += xmlTable.Attributes["bizModelId"].Value + "','";
                        writeInfosqlStr += xmlRield.Attributes["name"].Value + "','";
                        writeInfosqlStr += xmlRield.Attributes["code"].Value + "','";
                        writeInfosqlStr += xmlRield.Attributes["dataType"].Value + "','";
                        writeInfosqlStr += xmlRield.Attributes["index"].Value + "','";
                        writeInfosqlStr += xmlRield.Attributes["showinlist"].Value + "','";
                        writeInfosqlStr += xmlRield.Attributes["showinform"].Value + "','";
                        writeInfosqlStr += xmlRield.Attributes["editable"].Value + "','";
                        writeInfosqlStr += xmlRield.Attributes["edittype"].Value + "','";
                        writeInfosqlStr += xmlRield.Attributes["listname"].Value + "',";

                        if (fieldString != "")
                        {
                            fieldString += ", ";
                        }
                        fieldString += xmlRield.Attributes["code"].Value + " ";
                        switch (xmlRield.Attributes["dataType"].Value)
                        {
                            case "text":
                                fieldString += " char varying(";
                                fieldString += xmlRield.Attributes["dataLength"].Value + " ";
                                fieldString += ")";
                                writeInfosqlStr += xmlRield.Attributes["dataLength"].Value + ",";
                                break;
                            case "int":
                                fieldString += " int ";
                                writeInfosqlStr += "null,";
                                break;
                            case "float":
                                fieldString += " float ";
                                writeInfosqlStr += "null,";
                                break;
                            case "date":
                                fieldString += " datetime ";
                                writeInfosqlStr += "null,";
                                break;
                            default:
                                fieldString += xmlRield.Attributes["dataType"].Value;
                                writeInfosqlStr += "null,";
                                break;
                        }
                        if (xmlRield.Attributes["primary"] != null && xmlRield.Attributes["primary"].Value == "true")
                        {
                            //fieldString += " not null,constraint PK_"
                            //    + xmlTable.Attributes["code"].Value
                            //    + xmlRield.Attributes["code"].Value
                            //    + " primary key (" + xmlRield.Attributes["code"].Value + ") ";
                            fieldString += " not null ";
                            if (primaryString == "")
                            {
                                primaryString = xmlRield.Attributes["code"].Value;
                            }
                            else
                            {
                                primaryString += "," + xmlRield.Attributes["code"].Value;
                            }
                            writeInfosqlStr += "'1',";
                        }
                        else
                        {
                            fieldString += " null ";
                            writeInfosqlStr += "'0',";
                        }

                        //外键信息（由调用方确保被引用表已建立）
                        if (xmlRield.Attributes["foreign"] != null && xmlRield.Attributes["foreign"].Value == "true")
                        {
                            foreignsqlStr += "alter table ";
                            foreignsqlStr += xmlTable.Attributes["code"].Value;
                            foreignsqlStr += " add constraint FK_"
                                + System.Guid.NewGuid().ToString("N");
                            foreignsqlStr += " foreign key (";
                            foreignsqlStr += xmlRield.Attributes["code"].Value;
                            foreignsqlStr += ") ";
                            foreignsqlStr += " references ";
                            foreignsqlStr += xmlRield.Attributes["foreignTable"].Value;
                            foreignsqlStr += " (";
                            foreignsqlStr += xmlRield.Attributes["foreignField"].Value;
                            foreignsqlStr += ") ";

                            writeInfosqlStr += "'1','"
                                + xmlRield.Attributes["foreignTable"].Value + "','"
                                + xmlRield.Attributes["foreignField"].Value + "','"
                                + xmlRield.Attributes["foreignTableClass"].Value + "'";
                        }
                        else
                        {
                            writeInfosqlStr += "'0',null,null,null";
                        }
                        writeInfosqlStr += ") ";
                    }
                    else if (xmlRield.Name == "initRecords")
                    {
                        //初始记录
                        foreach (System.Xml.XmlElement xmlinitRecord in xmlRield.ChildNodes)
                        {
                            string initFieldStr = "";
                            string initvaluesStr = "";
                            foreach (System.Xml.XmlAttribute initValAtt in xmlinitRecord.Attributes)
                            {
                                if (initFieldStr != "")
                                {
                                    initFieldStr += ",";
                                    initvaluesStr += ",";
                                }
                                initFieldStr += initValAtt.Name;
                                initvaluesStr += "'" + initValAtt.Value + "'";
                            }
                            initRecordsqlStr += " insert into "
                                + xmlTable.Attributes["code"].Value
                                + "(" + initFieldStr + ") values(" + initvaluesStr + ") ";
                        }
                    }
                }
                sqlStr += fieldString;
                sqlStr += ",constraint PK_"
                                + System.Guid.NewGuid().ToString("N")
                                + " primary key (" + primaryString + ") ) ";
            }

            //生成表
            ((IDbAccess)this).exeSql(sqlStr);

            //生成外键引用
            if (foreignsqlStr != "")
            {
                ((IDbAccess)this).exeSql(foreignsqlStr);
            }

            //写表信息表对象信息记录表
            ((IDbAccess)this).exeSql(writeInfosqlStr);

            //写初始记录
            if (initRecordsqlStr != "")
            {
                ((IDbAccess)this).exeSql(initRecordsqlStr);
            }
        }

        void IDbAccess.BeginTrans()
        {
            trans = getTransConn().BeginTransaction();
            inTransaction = true;
        }

        void IDbAccess.CommitTrans()
        {
            
            trans.Commit();

            inTransaction = false;

            //try
            //{
            //    transconn.Close();
            //}
            //catch (Exception e)
            //{

            //}
            //transconn = null;
            trans = null;
        }

        void IDbAccess.RollbackTrans()
        {
        }

        void IDbAccess.exeSql(string strSql, string[] strParams, object[] objValues)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = getConn();
            if (inTransaction)
                cmd.Transaction = trans;
            //if ((strParams != null) && (strParams.Length != objValues.Length))
            //throw new ParamValueNotMatchException("查询参数和值不对应!");
            cmd.CommandText = strSql;
            if (strParams != null)
            {
                for (int i = 0; i < strParams.Length; i++)
                    cmd.Parameters.AddWithValue(strParams[i], objValues[i]);
            }

            cmd.ExecuteNonQuery();
        }

        void IDbAccess.exeSql(string strSql)
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    //using (SQLiteConnection tempConn = getConn())   //会对tempComm执行Dispose
                    //{
                    SQLiteConnection tempConn = getConn();
                        cmd.Connection = tempConn;
                        cmd.CommandText = strSql;
                        cmd.ExecuteNonQuery();

                        if (!inTransaction)
                        {
                            //tempConn.Close();
                            //tempConn.Dispose();
                            //cmd.Dispose();
                        }
                    //}
                }
            }
            catch (SQLiteException ex)
            {
                throw new Exception(ex.Message + "：" + strSql);
            }
            finally
            {

            }
        }

        System.Data.DataSet IDbAccess.exeSqlForDataSet(string QueryString, string[] strParams, object[] objValues)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = getConn();
            if (inTransaction)
                cmd.Transaction = trans;
            DataSet ds = new DataSet();
            SQLiteDataAdapter ad = new SQLiteDataAdapter();
            cmd.CommandText = QueryString;
            if (strParams != null)
            {
                for (int i = 0; i < strParams.Length; i++)
                    cmd.Parameters.AddWithValue(strParams[i], objValues[i]);
            }

            ad.SelectCommand = cmd;
            ad.Fill(ds);

            return ds;
        }

        System.Data.DataSet IDbAccess.exeSqlForDataSet(string QueryString)
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

                        if (!inTransaction)
                        {
                            //tempConn.Close();
                            //tempConn.Dispose();
                            //cmd.Dispose();
                        }
                        //ad.Dispose();
                        //throw new Exception("测试错误");
                    //}
                }
            }
            catch (SQLiteException ex)
            {
                throw new Exception(ex.Message + "：" + QueryString);
            }

            return ds;
        }

        void IDbAccess.exeSql(System.Xml.XmlDocument xmlSql)
        {
            try
            {
                ((IDbAccess)this).exeSql(((IDbAccess)this).createSqlString(xmlSql));
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                if (ex.Number == 156)
                {
                    throw new Exception("SQL语法错误：" + ((IDbAccess)this).createSqlString(xmlSql));
                }
                else
                {
                    throw ex;
                }
            }
        }

        System.Data.DataSet IDbAccess.exeSqlForDataSet(System.Xml.XmlDocument xmlSql)
        {
            DataSet retDs = null;

            try
            {
                retDs = ((IDbAccess)this).exeSqlForDataSet(((IDbAccess)this).createSqlString(xmlSql));
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                if (ex.Number == 156)
                {
                    throw new Exception("SQL语法错误：" + ((IDbAccess)this).createSqlString(xmlSql));
                }
                else
                {
                    throw ex;
                }
            }

            return retDs;
        }

        string IDbAccess.createSqlString(System.Xml.XmlDocument xmlSql)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

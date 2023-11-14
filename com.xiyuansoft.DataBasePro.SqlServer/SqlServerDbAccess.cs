using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace com.xiyuansoft.DataBasePro.SqlServer
{
    public class SqlServerDbAccess : com.xiyuansoft.DataBasePro.IDbAccess 
    {
        private SqlConnection conn;         //数据库连接，//直接对conn的调用在并发情况下可能出：
        //错误：There is already an open DataReader associated with this Command which must be closed first.
        private SqlConnection getConn()
        {
            SqlConnection tempConn;

            //if (conn != null && conn.State == ConnectionState.Open)
            //{
            //    tempConn = conn;  //可能导致并发问题
            //}
            //else
            //{
            //    if (this.conn != null)
            //    {
            //        try //在WEB服务器中出错后导致服务器出错
            //        {
            //            this.conn.Close();
            //            this.conn.Dispose();
            //        }
            //        catch (Exception e)
            //        {
            //            //???
            //        }
            //    }
            //    tempConn = new SqlConnection(connectionString);
            //    tempConn.Open();
            //    conn = tempConn;
            //}

            tempConn = new SqlConnection(connectionString);
            tempConn.Open();
            return tempConn;
        }
        private SqlTransaction trans;       //事务处理类
        private bool inTransaction = false;   //指示当前是否正处于事务中

        private string connectionString; 

        #region IDbAccess Members

        void IDbAccess.Open(string ConnectionString)
        {
            this.connectionString = ConnectionString;
            //this.conn = new SqlConnection(ConnectionString);
            //this.conn.Open();
        }

        void IDbAccess.OpenForAdmin(System.Collections.Hashtable dpPars)
        {
            string ConnectionString = "";

            ConnectionString += "Server=" + dpPars["dbAddr"].ToString() + ";";
            ConnectionString += "uid=sa;";
            ConnectionString += "pwd=" + dpPars["dbPassword"].ToString() + ";";
            ConnectionString += "database=master";

            this.connectionString = ConnectionString;

            //this.conn = new SqlConnection(ConnectionString);
            //this.conn.Open();
        }

        void IDbAccess.Close()
        {
            if (this.conn != null)
            {
                try //在WEB服务器中出错后导致服务器出错
                {
                    this.conn.Close();
                    this.conn.Dispose();
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
            string sqlStr = "select 1 From sysdatabases where name='" + dpPars["dbName"].ToString() + "'";

            DataSet tempDs = ((IDbAccess)this).exeSqlForDataSet(sqlStr);

            if (tempDs.Tables[0].Rows.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 生成数据库
        /// </summary>
        /// <param name="context">Web应用的环境参数，从中读取登陆名等数据</param>
        /// <returns>目标数据库的连接串</returns>
        string IDbAccess.DbCreate(System.Collections.Hashtable dpPars)
        {
            string sqlStr;

            //先删除数据库（调试，有误删除用户其它系统数据库的危险）
                sqlStr = " use master ";
                sqlStr = " if exists (select 1 From sysdatabases where name='" + dpPars["dbName"].ToString() + "') ";
                sqlStr += " begin ";
                sqlStr += " 	drop database " + dpPars["dbName"].ToString() + " ";
                sqlStr += " end ";
                ((IDbAccess)this).exeSql(sqlStr);

                //先删除用户（调试，有误删除用户其它系统用户的危险）
                sqlStr = " if exists (select 1 ";
                sqlStr += "             from  syslogins where name='" + dpPars["dbUser"].ToString() + "') ";
                sqlStr += " begin ";
                sqlStr += " 	EXEC sp_droplogin '" + dpPars["dbUser"].ToString() + "' ";
                sqlStr += " end ";
                ((IDbAccess)this).exeSql(sqlStr);

            sqlStr = " use master; ";
            sqlStr += " create database " + dpPars["dbName"].ToString() + "; ";
            /*生成用户*/
            sqlStr += " USE master; ";
            sqlStr += " if exists (select 1 from  syslogins where name='" + dpPars["dbUser"].ToString() + "') ";
            sqlStr += " begin ";
            sqlStr += " 	EXEC sp_droplogin '" + dpPars["dbUser"].ToString() + "' ";
            sqlStr += " end; ";
            sqlStr += " EXEC sp_addlogin " + dpPars["dbUser"].ToString() + "," + dpPars["dbUserPassword"].ToString() + "," + dpPars["dbName"].ToString() + ";  ";
            ((IDbAccess)this).exeSql(sqlStr);
            //由于在c#中不能用go，若连接执行，此时数据并未建立，因需需要断开执行

            sqlStr = " USE " + dpPars["dbName"].ToString() + "; ";

            //sqlStr += " EXEC sp_revokedbaccess 'dbo'; ";  //删除sa以便把新用户设置为dbo，以免在查询分析器通过sa登陆时不能直接查询新用户生成的表，导致调试时不方便

            sqlStr += " EXEC sp_changedbowner '" + dpPars["dbUser"].ToString() + "'; "; 
            //sqlStr += " EXEC sp_grantdbaccess '" + dpPars["dbUser"].ToString() + "','dbo'";//+ "', '" + dpPars["dbUser"].ToString() + "'; ";
            //sqlStr += " --EXEC sp_changegroup 'db_owner', '" + context.Request["dbUser"] + "' ";
            //sqlStr += " EXEC sp_addrolemember 'db_owner', '" + dpPars["dbUser"].ToString() + "'; ";
            ((IDbAccess)this).exeSql(sqlStr);

            string ConnectionString = "";
            ConnectionString += "Server=" + dpPars["dbAddr"].ToString() + ";";
            ConnectionString += "uid=" + dpPars["dbUser"].ToString() + ";";
            ConnectionString += "pwd=" + dpPars["dbUserPassword"].ToString() + ";";
            ConnectionString += "database=" + dpPars["dbName"].ToString() + ";";
            ConnectionString += "Packet Size=8192;Max Pool Size=1000;Connect Timeout=30;";   //解决连接池中连接不够的问题  Connect Timeout=600导致数据库不可用时客户端得不到错误信息

            return ConnectionString;
        }

        /// <summary>
        /// 生成数据表
        /// </summary>
        /// <param name="myXmlTableData">表及字段信息</param>
        void IDbAccess.TableCreate(System.Xml.XmlDocument myXmlTableData)
        {
            string sqlStr="";              //生成表的sql串
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
                                primaryString += ","+xmlRield.Attributes["code"].Value;
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
                                initvaluesStr += "'"+initValAtt.Value+"'";
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
            //并发怎么解决？？
            this.trans = this.conn.BeginTransaction();
            this.inTransaction = true; 
        }

        void IDbAccess.CommitTrans()
        {
            //并发怎么解决？？
            this.trans.Commit();
            this.inTransaction = false;
        }

        void IDbAccess.RollbackTrans()
        {
            //并发怎么解决？？
            this.trans.Rollback();
            this.inTransaction = false; 
        }

        void IDbAccess.exeSql(string strSql, string[] strParams, object[] objValues)
        {
            SqlCommand cmd = new SqlCommand();
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
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection tempConn = getConn())
                    {
                        cmd.Connection = tempConn;
                        cmd.CommandText = strSql;
                        cmd.ExecuteNonQuery();
                        
                        tempConn.Close();
                        tempConn.Dispose();
                        cmd.Dispose();
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                if (ex.Number == 156
                    || ex.Number == 170
                    || ex.Number == 515
                    )
                {
                    throw new Exception("SQL语法错误：" + strSql);
                }
                else if (ex.Number == 2627)
                {
                    throw new Exception("SQL主键重复错误：" + strSql);
                }
                else if (ex.Number == 8152)
                {
                    throw new Exception("输入数据超出数据库允许长度：" + strSql);
                }
                else
                {
                    throw new Exception(ex.Message + "：" + strSql);
                    //throw ex;
                }
            }
            finally
            {

            }
        }

        DataSet IDbAccess.exeSqlForDataSet(string QueryString, string[] strParams, object[] objValues)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = getConn();
            if (inTransaction)
                cmd.Transaction = trans;
            DataSet ds = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
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

        DataSet IDbAccess.exeSqlForDataSet(string QueryString)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection tempConn = getConn())
                    {
                        cmd.CommandTimeout = 300;  //大数据量查询时会超时
                        cmd.Connection = tempConn;
                        SqlDataAdapter ad = new SqlDataAdapter();
                        cmd.CommandText = QueryString;
                        ad.SelectCommand = cmd;
                        ad.Fill(ds);

                        tempConn.Close();
                        tempConn.Dispose();
                        cmd.Dispose();
                        //ad.Dispose();
                        //throw new Exception("测试错误");
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                if (ex.Number == 156 
                    || ex.Number == 170
                    || ex.Number == 515
                    )
                {
                    throw new Exception("SQL语法错误：" + QueryString);
                }
                else
                {
                    throw new Exception(ex.Message + "：" + QueryString);
                    //throw ex;
                }
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

        DataSet IDbAccess.exeSqlForDataSet(System.Xml.XmlDocument xmlSql)
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

        void IDbAccess.OpenForAdmin(Dictionary<string, string> dpPars)
        {
            string ConnectionString = "";

            ConnectionString += "Server=" + dpPars["dbAddr"].ToString() + ";";
            ConnectionString += "uid=sa;";
            ConnectionString += "pwd=" + dpPars["dbPassword"].ToString() + ";";
            ConnectionString += "database=master";

            this.connectionString = ConnectionString;
        }

        string IDbAccess.DbCreate(Dictionary<string, string> dpPars)
        {

            string sqlStr;

            //先删除数据库（调试，有误删除用户其它系统数据库的危险）
            sqlStr = " use master ";
            sqlStr = " if exists (select 1 From sysdatabases where name='" + dpPars["dbName"].ToString() + "') ";
            sqlStr += " begin ";
            sqlStr += " 	drop database " + dpPars["dbName"].ToString() + " ";
            sqlStr += " end ";
            ((IDbAccess)this).exeSql(sqlStr);

            //先删除用户（调试，有误删除用户其它系统用户的危险）
            sqlStr = " if exists (select 1 ";
            sqlStr += "             from  syslogins where name='" + dpPars["dbUser"].ToString() + "') ";
            sqlStr += " begin ";
            sqlStr += " 	EXEC sp_droplogin '" + dpPars["dbUser"].ToString() + "' ";
            sqlStr += " end ";
            ((IDbAccess)this).exeSql(sqlStr);

            sqlStr = " use master; ";
            sqlStr += " create database " + dpPars["dbName"].ToString() + "; ";
            /*生成用户*/
            sqlStr += " USE master; ";
            sqlStr += " if exists (select 1 from  syslogins where name='" + dpPars["dbUser"].ToString() + "') ";
            sqlStr += " begin ";
            sqlStr += " 	EXEC sp_droplogin '" + dpPars["dbUser"].ToString() + "' ";
            sqlStr += " end; ";
            sqlStr += " EXEC sp_addlogin " + dpPars["dbUser"].ToString() + "," + dpPars["dbUserPassword"].ToString() + "," + dpPars["dbName"].ToString() + ";  ";
            ((IDbAccess)this).exeSql(sqlStr);
            //由于在c#中不能用go，若连接执行，此时数据并未建立，因需需要断开执行

            sqlStr = " USE " + dpPars["dbName"].ToString() + "; ";

            //sqlStr += " EXEC sp_revokedbaccess 'dbo'; ";  //删除sa以便把新用户设置为dbo，以免在查询分析器通过sa登陆时不能直接查询新用户生成的表，导致调试时不方便

            sqlStr += " EXEC sp_changedbowner '" + dpPars["dbUser"].ToString() + "'; ";
            //sqlStr += " EXEC sp_grantdbaccess '" + dpPars["dbUser"].ToString() + "','dbo'";//+ "', '" + dpPars["dbUser"].ToString() + "'; ";
            //sqlStr += " --EXEC sp_changegroup 'db_owner', '" + context.Request["dbUser"] + "' ";
            //sqlStr += " EXEC sp_addrolemember 'db_owner', '" + dpPars["dbUser"].ToString() + "'; ";
            ((IDbAccess)this).exeSql(sqlStr);

            string ConnectionString = "";
            ConnectionString += "Server=" + dpPars["dbAddr"].ToString() + ";";
            ConnectionString += "uid=" + dpPars["dbUser"].ToString() + ";";
            ConnectionString += "pwd=" + dpPars["dbUserPassword"].ToString() + ";";
            ConnectionString += "database=" + dpPars["dbName"].ToString() + ";";
            ConnectionString += "Packet Size=8192;Max Pool Size=1000;Connect Timeout=30;";   //解决连接池中连接不够的问题  Connect Timeout=600导致数据库不可用时客户端得不到错误信息

            return ConnectionString;
        }

        #endregion
    }
}

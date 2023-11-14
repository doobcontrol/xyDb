using com.xiyuansoft.bormodel;
using com.xiyuansoft.bormodel.metadata;
using com.xiyuansoft.DataBasePro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.xiyuansoft.DataBaseUpdate
{
    public class DbUpdateManager
    {
        static public void addTabls(List<BaseModel> BaseModelList)
        {
            DbService Ds = new DbService();
            Ds.BeginTrans();            //*，生成物理表
            foreach (BaseModel BaseModelForCreateTable in BaseModelList)
            {
                BaseModelForCreateTable.createTable(false);
            }

            //*，生成外键约束
            foreach (BaseModel BaseModelForCreateTable in BaseModelList)
            {
                BaseModelForCreateTable.createFKey();
            }
            //*，生成元数据信息（表信息及字段信息）
            foreach (BaseModel BaseModelForCreateTable in BaseModelList)
            {
                BaseModelForCreateTable.createMetaData();
            }

            Ds.CommitTrans();
        }
        static public void addFields(Dictionary<BaseModel, List<string>> bmFieldDic)
        {
            DbService Ds = new DbService();
            Ds.BeginTrans();
            foreach (BaseModel bm in bmFieldDic.Keys)
            {
                //生成本模型物理表    	
                String tempSql = "";

                foreach (string fieldsCode in bmFieldDic[bm])
                {
                    tempSql = "";
                    tempSql += " alter  table ";
                    tempSql += bm.getTableCode();
                    tempSql += " add  ";
                    String fieldString = "";            //构建字段的sql串
                    String foreignString = "";            //主键串
                    if (fieldString != "")
                    {
                        fieldString += ", ";
                    }

                     Hashtable fieldHt = bm.getFieldsHt()[fieldsCode];
                    fieldString += fieldHt[Field.fFieldCode].ToString() + " ";
                    String tDataType = fieldHt[Field.fDataType].ToString();

                    if (tDataType == Field.DataType_text)
                    {
                        fieldString += " char varying(";
                        fieldString += fieldHt[Field.fDataLength].ToString() + " ";
                        fieldString += ")";
                    }
                    else if (tDataType == Field.DataType_int)
                    {
                        fieldString += " int ";
                    }
                    else if (tDataType == Field.DataType_float)
                    {
                        fieldString += " float ";
                    }
                    else if (tDataType == Field.DataType_date)
                    {
                        fieldString += " datetime ";
                    }
                    else
                    {
                        fieldString += tDataType;
                    }

                    fieldString += " null ";

                    //if (DataBaseType == "com.xiyuansoft.DataBasePro.SQLite.SQLiteDbAccess,com.xiyuansoft.DataBasePro.SQLite")
                    //{
                        if (fieldHt[Field.fIsForeignKey].ToString() == "1")
                        {
                            //SQLite不支持ALTER TABLE加外键，需在此处支持，但需本方法递归避免生成表顺序不正确
                            //foreignString += ", ";
                            foreignString += "FOREIGN KEY ("
                                + fieldHt[Field.fFieldCode].ToString() + ") REFERENCES "
                                + fieldHt[Field.fForeignKeyTable].ToString() + "("
                                + fieldHt[Field.fForeignKeyField].ToString() + ")";
                        }
                        //}


                    string exSql;
                    exSql = tempSql + fieldString + "  ";
                    Ds.exeSql(exSql); 
                    //exSql = tempSql + foreignString + "  ";
                    //Ds.exeSql(exSql);
                }
            }
            Ds.CommitTrans();
        }

        static public void ExeSql(string sqlStr)
        {

            DbService Ds = new DbService();
            Ds.exeSql(sqlStr);
        }
        
        /// <summary>
        /// 一次性更新，不考虑连续版本检查
        /// </summary>
        static public void SimpleUpdate(ISimpleUpdater updater)
        {
            updater.doUpdate();
        }
    }
}

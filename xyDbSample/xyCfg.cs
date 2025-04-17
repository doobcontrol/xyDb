using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace xyDbSample
{
    class xyCfg
    {
        private xyCfg()
        {
            if (!File.Exists(cfgFile))
            {
                JsonArray JsonObj = new JsonArray(); 
                JsonObject keyValuePairs = new JsonObject();

                //SQlite
                keyValuePairs = new JsonObject();
                keyValuePairs[dbType] = dT_SQLite;
                keyValuePairs[dbCeated] = false;
                keyValuePairs[connStr] = "";
                JsonObj.Add(keyValuePairs);

                //PostgreSQL
                keyValuePairs = new JsonObject();
                keyValuePairs[dbType] = dT_PostgreSQL;
                keyValuePairs[dbCeated] = false;
                keyValuePairs[connStr] = "";
                JsonObj.Add(keyValuePairs);

                //SQLServer
                keyValuePairs = new JsonObject();
                keyValuePairs[dbType] = dT_SQLServer;
                keyValuePairs[dbCeated] = false;
                keyValuePairs[connStr] = "";
                JsonObj.Add(keyValuePairs);

                //MySql
                keyValuePairs = new JsonObject();
                keyValuePairs[dbType] = dT_MySql;
                keyValuePairs[dbCeated] = false;
                keyValuePairs[connStr] = "";
                JsonObj.Add(keyValuePairs);

                string jsonString = JsonSerializer.Serialize(JsonObj);
                File.WriteAllText(cfgFile, jsonString);
            }
        }
        private string getPar(string dType, string pName)
        {
            string jsonString = File.ReadAllText(cfgFile);
            JsonArray JsonObj = JsonSerializer.Deserialize<JsonArray>(jsonString);
            foreach (JsonObject item in JsonObj)
            {
                if (item[dbType].ToString() == dType)
                {
                    return item[connStr].ToString();
                }
            }
            return "";
        }
        private List<DictionaryEntry> getList()
        {
            string jsonString = File.ReadAllText(cfgFile);
            JsonArray JsonObj = JsonSerializer.Deserialize<JsonArray>(jsonString);
            List<DictionaryEntry> dbList = new List<DictionaryEntry>();
            foreach (JsonObject item in JsonObj)
            {
                DictionaryEntry de = new DictionaryEntry();
                de.Key = item[dbType].ToString();
                de.Value = item;
                dbList.Add(de);
            }
            return dbList;
        }
        private void setPar(string dType, string pName, string pValue)
        {
            string jsonString = File.ReadAllText(cfgFile);
            JsonArray JsonObj = JsonSerializer.Deserialize<JsonArray>(jsonString);
            foreach (JsonObject item in JsonObj)
            {
                if (item[dbType].ToString() == dType)
                {
                    item[connStr] = pValue;
                }
            }
            string newJsonString = JsonSerializer.Serialize(JsonObj);
            File.WriteAllText(cfgFile, newJsonString);
        }
        private void setPar(string dType, Dictionary<string, string> pValues)
        {
            string jsonString = File.ReadAllText(cfgFile);
            JsonArray JsonObj = JsonSerializer.Deserialize<JsonArray>(jsonString);
            foreach (JsonObject item in JsonObj)
            {
                if (item[dbType].ToString() == dType)
                {
                    foreach(var kvp in pValues)
                    {
                        if(kvp.Key == dbCeated)
                        {
                            item[kvp.Key] = bool.Parse(kvp.Value);
                        }
                        else
                        {
                            item[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            string newJsonString = JsonSerializer.Serialize(JsonObj);
            File.WriteAllText(cfgFile, newJsonString);
        }

        public const string cfgFile = "xyCfg.json";
        public const string dbType = "dbType";
        public const string dbCeated = "dbCeated";
        public const string connStr = "connStr";
        public const string dT_SQLite = "SQLite";
        public const string dT_PostgreSQL = "PostgreSQL";
        public const string dT_SQLServer = "SQLServer";
        public const string dT_MySql = "MySql";

        static private xyCfg instance = new xyCfg();
        static public string get(string dType, string pName)
        {
            return instance.getPar(dType, pName);
        }
        static public void set(string dType, string pName, string pValue)
        {
            instance.setPar(dType, pName, pValue);
        }
        static public void set(string dType, Dictionary<string, string> pValues)
        {
            instance.setPar(dType, pValues);
        }
        static public List<DictionaryEntry> getDbList()
        {
            return instance.getList();
        }
    }
}

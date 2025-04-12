using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using xy.Db;
using xy.Db.PostgreSQL;
using xy.Db.SQLite64;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace xyDbSample
{
    public partial class FrmDbSelector : Form
    {
        string dbName = "testDb";
        string dbUser = "testUser";
        string dbPassword = "testPassword";
        string dbScript = "CREATE TABLE COMPANY("
                    + "ID INT PRIMARY KEY     NOT NULL,"
                    + "NAME           TEXT    NOT NULL,"
                    + "AGE            INT     NOT NULL,"
                    + "ADDRESS        CHAR(50),"
                    + "SALARY         REAL);";
        private DbService dbService;
        public DbService DbService { get => dbService; set => dbService = value; }

        public string DbType
        {
            get
            {
                return ((DictionaryEntry)comboBox1.SelectedItem)
                    .Key.ToString();
            }
        }
        public bool CreateNewDb
        {
            get
            {
                return checkBox1.Checked;
            }
        }
        public FrmDbSelector()
        {
            InitializeComponent();
            this.Text = "Select Database Type";
            button1.Text = "Ok";
            button1.Enabled = false;

            checkBox1.Text = "Create new database";
            checkBox1.Enabled = false;
            checkBox1.Visible = false;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.SelectedIndexChanged += (s, e) =>
            {
                JsonObject sCfg = comboBox1.SelectedValue as JsonObject;
                if (sCfg != null)
                {
                    checkBox1.Checked = !sCfg[xyCfg.dbCeated].GetValue<bool>();
                    checkBox1.Visible = true;
                    button1.Enabled = true;
                }
            };
            comboBox1.DataSource = xyCfg.getDbList();
            comboBox1.DisplayMember = "Key";
            comboBox1.ValueMember = "Value";
            comboBox1.SelectedIndex = -1;
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            try
            {
                IDbAccess? dbAccess = null;

                //the params for create connection string
                //to an admin account of the dbms
                Dictionary<string, string> adminPars =
                    new Dictionary<string, string>();

                //the params for create database(dbname, user, table etc.) 
                Dictionary<string, string> dbCreatePars =
                    new Dictionary<string, string>();
                dbCreatePars.Add(DbService.pn_dbName, dbName);
                dbCreatePars.Add(DbService.pn_dbUser, dbUser);
                dbCreatePars.Add(DbService.pn_dbPassword, dbPassword);
                dbCreatePars.Add(DbService.pn_dbScript, dbScript);

                switch (DbType)
                {
                    case xyCfg.dT_SQLite:
                        dbAccess = new SQLite64DbAccess();
                        break;
                    case xyCfg.dT_PostgreSQL:
                        dbAccess = new PostgreSQLDbAccess();
                        adminPars.Add(DbService.pn_dbServer, "localhost");
                        adminPars.Add(DbService.pn_dbName, "postgres");
                        adminPars.Add(DbService.pn_dbUser, "postgres");
                        adminPars.Add(DbService.pn_dbPassword, "123456");
                        break;
                }
                if (dbAccess != null)
                {
                    if (CreateNewDb)
                    {
                        DbService = new DbService(dbAccess);
                        await DbService.OpenForAdminAsync(adminPars);
                        string createdConnectString =
                            await DbService.DbCreateAsync(dbCreatePars);

                        xyCfg.set(DbType,
                            new Dictionary<string, string>() {
                        { xyCfg.dbCeated, true.ToString() },
                        { xyCfg.connStr, createdConnectString}
                            });
                    }
                    else
                    {
                        string ConnectionString =
                            xyCfg.get(DbType, xyCfg.connStr);
                        DbService = new DbService(
                            ConnectionString, dbAccess);
                        await DbService.openAsync();
                    }
                }
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}

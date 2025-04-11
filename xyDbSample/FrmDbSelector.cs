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
                }
                checkBox1.Visible = true;
                button1.Enabled = true;
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
                switch (DbType)
                {
                    case "SQLite":
                        await initSQLite(CreateNewDb);
                        break;
                    case "PostgreSQL":
                        await initPostgreSQLAsync(CreateNewDb);
                        break;
                }
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                DialogResult = DialogResult.Cancel;
            }
        }

        private async Task initSQLite(bool CreateNewDb)
        {
            string directoryName =
                System.IO.Path.GetDirectoryName(
                new System.Uri(System.Reflection.Assembly.
                GetExecutingAssembly().CodeBase).LocalPath);
            string ConnectionString = "Data Source=" + directoryName + "/"
                + dbName + ";";
            DbService = new DbService(
                ConnectionString,
                new SQLite64DbAccess());
            DbService.openAsync();
        }
        private async Task initPostgreSQLAsync(bool CreateNewDb)
        {
            if (CreateNewDb)
            {
                Dictionary<string, string> dpPars = new Dictionary<string, string>();
                dpPars.Add(DbService.pn_dbServer, "localhost");
                dpPars.Add(DbService.pn_dbName, "postgres");
                dpPars.Add(DbService.pn_dbUser, "postgres");
                dpPars.Add(DbService.pn_dbPassword, "123456");
                DbService = new DbService(
                    new PostgreSQLDbAccess());
                await DbService.OpenForAdminAsync(dpPars);
                dpPars = new Dictionary<string, string>();
                dpPars.Add(DbService.pn_dbName, dbName);
                dpPars.Add(DbService.pn_dbUser, dbUser);
                dpPars.Add(DbService.pn_dbPassword, dbPassword);
                dpPars.Add(DbService.pn_dbScript,
                    "CREATE TABLE COMPANY("
                    + "ID INT PRIMARY KEY     NOT NULL,"
                    + "NAME           TEXT    NOT NULL,"
                    + "AGE            INT     NOT NULL,"
                    + "ADDRESS        CHAR(50),"
                    + "SALARY         REAL);"
                    );
                string createdConnectString =
                    await DbService.DbCreateAsync(dpPars);

                xyCfg.set(xyCfg.dT_PostgreSQL,
                    new Dictionary<string, string>() {
                        { xyCfg.dbCeated, true.ToString() },
                        { xyCfg.connStr, createdConnectString}
                    });
            }
            else
            {
                string ConnectionString =
                    xyCfg.get(xyCfg.dT_PostgreSQL, xyCfg.connStr);
                DbService = new DbService(
                    ConnectionString,
                    new PostgreSQLDbAccess());
                await DbService.openAsync();
            }
        }
    }
}

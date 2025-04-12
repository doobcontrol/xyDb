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
using xy.Db.SQLServer;
using xySoft.log;

namespace xyDbSample
{
    public partial class FrmDbSelector : Form
    {
        string dbServer = "localhost";
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
                    switch (DbType)
                    {
                        case xyCfg.dT_SQLite:
                            showAdminConnParPanel(false);
                            break;
                        case xyCfg.dT_PostgreSQL:
                            showAdminConnParPanel(checkBox1.Checked);
                            lbdbName.Text = "Database Name";
                            lbdbUser.Text = "User Name";
                            lbdbPassword.Text = "Password";
                            lbdbServer.Text = "Server Name";
                            txtdbName.Text = "postgres";
                            txtdbUser.Text = "postgres";
                            txtdbPassword.Text = "123456";
                            txtdbServer.Text = "localhost";
                            break;
                            break;
                        case xyCfg.dT_SQLServer:
                            showAdminConnParPanel(checkBox1.Checked);
                            lbdbName.Text = "Database Name";
                            lbdbUser.Text = "User Name";
                            lbdbPassword.Text = "Password";
                            lbdbServer.Text = "Server Name";
                            txtdbName.Text = "master";
                            txtdbUser.Text = "sa";
                            txtdbPassword.Text = "123456";
                            txtdbServer.Text = @"localhost\SQLEXPRESS";
                            break;
                    }
                }
                else
                {
                    showAdminConnParPanel(false);
                }
            };
            comboBox1.DataSource = xyCfg.getDbList();
            comboBox1.DisplayMember = "Key";
            comboBox1.ValueMember = "Value";
            comboBox1.SelectedIndex = -1;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = button1;

            showAdminConnParPanel(false);
        }
        private void showAdminConnParPanel(bool toShow)
        {
            int parPanelHeight = 0;
            if (toShow)
            {
                parPanelHeight = panelAdminConnPar.Height;
                panelAdminConnPar.Visible = true;
                panelAdminConnPar.BringToFront();
                panelAdminConnPar.Dock = DockStyle.Fill;
            }
            else
            {
                parPanelHeight = 0;
                panelAdminConnPar.Visible = false;
            }
            Size size = this.ClientSize;
            size.Height = panelTop.Height + panelTottom.Height + parPanelHeight;
            this.ClientSize = size;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            this.Text += " - processing ...";
            this.ControlBox = false;
            this.panelTop.Enabled = false;
            this.panelTottom.Enabled = false;
            this.panelAdminConnPar.Enabled = false;
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
                dbCreatePars.Add(DbService.pn_dbServer, dbServer);
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
                        adminPars.Add(DbService.pn_dbServer,
                            txtdbServer.Text);
                        dbCreatePars[DbService.pn_dbServer]
                            = txtdbServer.Text;
                        adminPars.Add(DbService.pn_dbName, 
                            txtdbName.Text);
                        adminPars.Add(DbService.pn_dbUser, 
                            txtdbUser.Text);
                        adminPars.Add(DbService.pn_dbPassword, 
                            txtdbPassword.Text);
                        break;
                    case xyCfg.dT_SQLServer:
                        dbAccess = new SQLServerDbAccess();
                        adminPars.Add(DbService.pn_dbServer,
                            txtdbServer.Text);
                        dbCreatePars[DbService.pn_dbServer]
                            = txtdbServer.Text;
                        adminPars.Add(DbService.pn_dbName,
                            txtdbName.Text);
                        adminPars.Add(DbService.pn_dbUser,
                            txtdbUser.Text);
                        adminPars.Add(DbService.pn_dbPassword,
                            txtdbPassword.Text);
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
                XyLog.log(ex.Message + "\r\n" + ex.StackTrace);
                if(ex.InnerException != null)
                {
                    XyLog.log(ex.InnerException.Message + "\r\n" 
                        + ex.InnerException.StackTrace);
                }
                MessageBox.Show(ex.Message + "\r\nCheck detail in the log");
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}

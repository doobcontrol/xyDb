using System.Data;
using xy.Db;
using xy.Db.PostgreSQL;
using xy.Db.SQLite64;

namespace xyDbSample
{
    public partial class Form1 : Form
    {
        string dbName = "testDb";
        string dbUser = "testUser";
        DbService dbService;
        public Form1(string dbType, bool CreateNewDb)
        {
            InitializeComponent();

            switch (dbType)
            {
                case "SQLite":
                    initSQLite(CreateNewDb);
                    break;
                case "PostgreSQL":
                    initPostgreSQLAsync(CreateNewDb);
                    break;
            }

            var dbPar = new Dictionary<string, string>();
            dbPar.Add(DbService.pn_dbName, dbName);

            bool dbExist = dbService.DbExist(dbPar);
            if (dbExist)
            {
                button1.Visible = false;

                loadeData();
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
            dbService = new DbService(
                ConnectionString,
                new SQLite64DbAccess());
            dbService.openAsync();
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
                dbService = new DbService(
                    new PostgreSQLDbAccess());
                await dbService.create(dpPars);
                await dbService.exeSqlAsync("CREATE USER " + dbUser + " WITH PASSWORD 'jw8s0F4';");
                await dbService.exeSqlAsync("CREATE DATABASE " + dbName + " OWNER " + dbUser + ";");
                await dbService.close();

                try
                {
                    string ConnectionString =
                        "Server=localhost;"
                        + "Database=" + dbName.ToLower() + ";" //Why lower()?
                        + "User Id=" + dbUser.ToLower() + ";"
                        + "Password=jw8s0F4;";
                    dbService = new DbService(
                        ConnectionString, new PostgreSQLDbAccess());
                    await dbService.openAsync();
                    await dbService.exeSqlAsync("CREATE TABLE COMPANY("
                        + "ID INT PRIMARY KEY     NOT NULL,"
                        + "NAME           TEXT    NOT NULL,"
                        + "AGE            INT     NOT NULL,"
                        + "ADDRESS        CHAR(50),"
                        + "SALARY         REAL);");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            else
            {
                try
                {
                    string ConnectionString =
                        "Server=localhost;"
                        + "Database=" + dbName.ToLower() + ";" //Why lower()?
                        + "User Id=" + dbUser.ToLower() + ";"
                        + "Password=jw8s0F4;";
                    dbService = new DbService(
                        ConnectionString,
                        new PostgreSQLDbAccess());
                    await dbService.openAsync();
                    loadeData();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }
        private void loadeData()
        {
            string sql = "SELECT * FROM COMPANY";
            var dt = dbService.exeSqlForDataSetAsync(sql);
            if (dt != null)
            {
                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Add("ID", "ID");
                dataGridView1.Columns.Add("NAME", "NAME");
                dataGridView1.Columns.Add("AGE", "AGE");
                dataGridView1.Columns.Add("ADDRESS", "ADDRESS");
                dataGridView1.Columns.Add("SALARY", "SALARY");
                foreach (DataRow row in dt.Result.Rows)
                {
                    dataGridView1.Rows.Add(row.ItemArray);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dbPar = new Dictionary<string, string>();
            dbPar.Add(DbService.pn_dbName, dbName);

            bool dbExist = dbService.DbExist(dbPar);
            textBox1.AppendText("dbExist: " + dbExist + "\r\n");

            if (!dbExist)
            {
                dbPar.Add(DbService.pn_dbScript, "CREATE TABLE COMPANY(\r\n   ID INT PRIMARY KEY     NOT NULL,\r\n   NAME           TEXT    NOT NULL,\r\n   AGE            INT     NOT NULL,\r\n   ADDRESS        CHAR(50),\r\n   SALARY         REAL\r\n)");
                string dbCreate = dbService.DbCreate(dbPar);
                textBox1.AppendText("dbCreate: " + dbCreate + "\r\n");
            }
            else
            {
                textBox1.AppendText("db already exists\r\n");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO COMPANY(ID,NAME,AGE,ADDRESS,SALARY) "
                + "VALUES(0,'Mock',22,'Here there',34.33)";
            dbService.exeSqlAsync(sql);
            loadeData();
        }
    }
}
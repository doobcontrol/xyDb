using xy.Db;
using xy.Db.SQLite64;

namespace xyDbSample
{
    public partial class Form1 : Form
    {
        string dbName = "testDb";
        DbService dbService;
        public Form1()
        {
            InitializeComponent();

            string directoryName =
                System.IO.Path.GetDirectoryName(
                new System.Uri(System.Reflection.Assembly.
                GetExecutingAssembly().CodeBase).LocalPath);
            string ConnectionString = "Data Source=" + directoryName + "/"
                + dbName + ";";
            dbService = new DbService(
                ConnectionString,
                new SQLite64DbAccess());

            var dbPar = new Dictionary<string, string>();
            dbPar.Add(DbService.pn_dbName, dbName);

            bool dbExist = dbService.DbExist(dbPar);
            if (dbExist)
            {
                button1.Visible = false;

                loadeData();
            }
        }

        private void loadeData()
        {
            string sql = "SELECT * FROM COMPANY";
            var dt = dbService.exeSqlForDataSet(sql);
            if (dt != null)
            {
                dataGridView1.DataSource = dt;
                dataGridView1.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill;
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
            dbService.exeSql(sql);
            loadeData();
        }
    }
}
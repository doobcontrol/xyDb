using System.Data;
using xy.Db;
using xy.Db.PostgreSQL;
using xy.Db.SQLite64;

namespace xyDbSample
{
    public partial class Form1 : Form
    {
        DbService dbService;
        public Form1(DbService DbService)
        {
            InitializeComponent();

            dbService = DbService;

            loadeData();
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
            //var dbPar = new Dictionary<string, string>();
            //dbPar.Add(DbService.pn_dbName, dbName);

            //bool dbExist = dbService.DbExist(dbPar);
            //textBox1.AppendText("dbExist: " + dbExist + "\r\n");

            //if (!dbExist)
            //{
            //    dbPar.Add(DbService.pn_dbScript, "CREATE TABLE COMPANY(\r\n   ID INT PRIMARY KEY     NOT NULL,\r\n   NAME           TEXT    NOT NULL,\r\n   AGE            INT     NOT NULL,\r\n   ADDRESS        CHAR(50),\r\n   SALARY         REAL\r\n)");
            //    string dbCreate = await dbService.DbCreateAsync(dbPar);
            //    textBox1.AppendText("dbCreate: " + dbCreate + "\r\n");
            //}
            //else
            //{
            //    textBox1.AppendText("db already exists\r\n");
            //}
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
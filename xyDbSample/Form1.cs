using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
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
            dataGridView1.AllowUserToAddRows = false;

            dbService = DbService;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await loadeDataAsync();
        }

        int maxId = 0;
        private async Task loadeDataAsync()
        {
            string sql = "SELECT * FROM COMPANY ORDER BY ID";
            DataTable dt = await dbService.exeSqlForDataSetAsync(sql);
            if (dt != null)
            {
                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Add("ID", "ID");
                dataGridView1.Columns.Add("NAME", "NAME");
                dataGridView1.Columns.Add("AGE", "AGE");
                dataGridView1.Columns.Add("ADDRESS", "ADDRESS");
                dataGridView1.Columns.Add("SALARY", "SALARY");
                dataGridView1.Columns["ID"].ReadOnly = true;

                foreach (DataRow row in dt.Rows)
                {
                    int i = dataGridView1.Rows.Add(row.ItemArray);
                    dataGridView1.Rows[i].Tag = row;
                    if (Convert.ToInt32(row["ID"]) > maxId)
                    {
                        maxId = Convert.ToInt32(row["ID"]);
                    }
                }
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            maxId++;
            string sql = "INSERT INTO COMPANY(ID, NAME, AGE) "
                + "VALUES(" + maxId + ", '', 0)";
            await dbService.exeSqlAsync(sql);
            sql = "SELECT * From COMPANY WHERE ID=" + maxId;
            DataTable dt = await dbService.exeSqlForDataSetAsync(sql);
            int i = dataGridView1.Rows.Add(dt.Rows[0].ItemArray);
            dataGridView1.Rows[i].Tag = dt.Rows[0];
        }

        private async void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            string cellValue =
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            string rowID =
                dataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString();
            string sql = "UPDATE COMPANY SET " + colName + "='" + cellValue + "' "
                + " WHERE ID=" + rowID;
            await dbService.exeSqlAsync(sql);
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows.Count == 1)
            {
                int rowIndex = dataGridView1.SelectedRows[0].Index;
                int rowID = Convert.ToInt32(dataGridView1.Rows[rowIndex].Cells["ID"].Value);
                string sql = "DELETE FROM COMPANY "
                    + " WHERE ID=" + rowID;
                await dbService.exeSqlAsync(sql);
                dataGridView1.Rows.RemoveAt(rowIndex);
            }
            else
            {
                MessageBox.Show("Please select one row to delete.");
            }
        }
    }
}
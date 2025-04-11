using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xyDbSample
{
    public partial class FrmDbSelector : Form
    {
        public string DbType
        {
            get
            {
                return comboBox1.SelectedItem.ToString();
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

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Items.Add("SQLite");
            comboBox1.Items.Add("PostgreSQL");
            comboBox1.SelectedIndexChanged += (s, e) =>
            {
                button1.Enabled = true;
            };

            checkBox1.Text = "Create new database";
            checkBox1.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}

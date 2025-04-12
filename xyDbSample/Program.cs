using xy.Db;

namespace xyDbSample
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            FrmDbSelector frmDbSelector = new FrmDbSelector();
            if (frmDbSelector.ShowDialog() == DialogResult.OK)
            {
                DbService dbService = frmDbSelector.DbService;
                frmDbSelector.DbService = null; // Dispose of the form to free up resources
                Form1 form1 = new Form1(dbService);
                form1.Text = "xyDbSample - " + frmDbSelector.DbType;
                Application.Run(form1);
            }
        }
    }
}
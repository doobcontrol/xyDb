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
                Application.Run(new Form1(
                    frmDbSelector.DbType,
                    frmDbSelector.CreateNewDb
                    ));
            }
        }
    }
}
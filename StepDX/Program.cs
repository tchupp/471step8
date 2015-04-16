using System;
using System.Windows.Forms;

namespace StepDX
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var game = new Game();
            game.Show();

            do
            {
                game.Advance();
                game.Render();
                Application.DoEvents();
            } while (game.Created);
        }
    }
}
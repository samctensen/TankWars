// Main entry point for the application.
// Created Nov 2021 by Sam Christensen and Bryce Gillespie
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            GameController gameController = new GameController();
            Application.Run(new TankWars(gameController));
        }
    }
}

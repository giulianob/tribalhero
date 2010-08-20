using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Game.Setup;
using Game;
using Game.Battle;

namespace Simulator {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Factory.initAll();
            BattleReport.WriterInit();
            Global.dbManager.Pause();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
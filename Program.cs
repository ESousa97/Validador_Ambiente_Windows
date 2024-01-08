using System;
using System.Windows.Forms;

namespace Padronizador_ambiente_v0._1
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para a aplicação.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); // Inicia o seu Form1
        }
    }
}

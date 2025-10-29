using System;
using System.Windows.Forms;

namespace MongolianPhonetic
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ensure only one instance is running
            using (var mutex = new System.Threading.Mutex(false, "MongolianPhoneticKeyboard"))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Mongolian Phonetic Keyboard is already running!",
                                    "Already Running",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    return;
                }

                Application.Run(new TrayApplication());
            }
        }
    }
}

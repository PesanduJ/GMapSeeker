using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gmapseeker
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (CheckInternetConnection())
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GMapSeeker());
            }
            else
            {
                MessageBox.Show("Please connect to the internet to run this application.", "No Internet Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        static bool CheckInternetConnection()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
    }
}

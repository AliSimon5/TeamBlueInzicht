using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace HDSignaturesTool.Data
{
    internal partial class DataManager
    {
        /// <summary>
        /// Maakt een startup value aan in het register
        /// </summary>
        /// <returns></returns>
        public static bool CreateStartupValue()
        {
            try
            {

                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (key?.GetValue("HDSignatures")?.ToString() == null)
                {
                    key.SetValue("HDSignatures", "\"" + Application.ExecutablePath + "\"/silent");
                    Log.Information("HDSignatures wordt nu gestart bij het starten van uw apparaat");
                    return true;
                }
                else
                {
                    Log.Information("HDSignatures zit al in Startup");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Verwijderd de startup value in het register
        /// </summary>
        /// <returns></returns>
        public static bool DeleteStartupValue() 
        {
            try
            {

                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (key?.GetValue("HDSignatures")?.ToString() != null)
                {
                    key.DeleteValue("HDSignatures");
                    Log.Information("HDSignatures wordt nu niet meer gestart bij het starten van uw apparaat");
                    return true;
                }
                else
                {
                    Log.Information("HDSignatures zit niet in Startup");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
        }
    }
}

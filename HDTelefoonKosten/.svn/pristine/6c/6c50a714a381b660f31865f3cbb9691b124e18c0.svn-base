using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTelefoonKosten.DataManager
{
    internal partial class DataManager
    {
        public static bool ExecuteCommand(string argCommand, string argArgument)
        {
            try
            {
                Process cmd = new Process();
                // lijst van opties                
                // de pad om daarna een Argument aan toe te voegen
                cmd.StartInfo.FileName = argCommand;
                cmd.StartInfo.Arguments = argArgument;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                //zorgt ervoor dat er niet extra Command Prompt windows open komen te staan
                cmd.StartInfo.CreateNoWindow = true;
                //er wordt geen Shell gebruikt
                cmd.StartInfo.UseShellExecute = false;
                //start de cmd met de opties
                cmd.Start();
                cmd.WaitForExit(15000);
                if (cmd.ExitCode == 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Command niet kunnen uitvoeren \n\r {argCommand} {argArgument}");
                Log.Error(ex.Message);
                return false;
            }
        }
    }
}

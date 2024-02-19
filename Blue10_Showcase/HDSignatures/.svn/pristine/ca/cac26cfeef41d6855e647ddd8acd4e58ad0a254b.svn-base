using HDSignaturesTool.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignaturesTool.Data
{
    internal partial class DataManager
    {
        public static void CreateSignaturesDirectory(List<UserData> tempProcessedUserDataList)
        {
            Log.Information("Maakt nieuwe Directory aan bij elke user");

            foreach (UserData tempUser in tempProcessedUserDataList)
            {
                if (!Directory.Exists($"{tempUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                {
                    Log.Information($"Nieuwe Directory gemaakt voor {tempUser.UserName}");
                    Directory.CreateDirectory($"{tempUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}");
                }
            }
        }
    }
}

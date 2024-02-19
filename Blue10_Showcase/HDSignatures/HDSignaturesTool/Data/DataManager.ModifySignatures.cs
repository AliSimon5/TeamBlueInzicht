using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;
using HDSignaturesTool.Logic;
using HDSignaturesTool.Types;
using System.IO;
using Microsoft.VisualBasic;
using HDSignaturesTool.Helpers;

namespace HDSignaturesTool.Data
{

    internal partial class DataManager
    {
        /// <summary>
        /// Pakt alle variables waar de userdata en gebruikt ze om Nieuwe Signatures te maken voor elke User
        /// </summary>
        /// <param name="tempProcessedUsers">Een list met alle Info over de Users</param>
        /// <param name="tempActiveUserList">Alle Active Users</param>
        public static void ModifySignatures(List<UserData> tempProcessedUsers, List<UserData> tempActiveUserList)
        {
            foreach (UserData tempUserData in tempProcessedUsers)
            {                
                if (!Settings.blnAdminOnly)
                {
                    var tempLoggingReport = new LoggingReport();

                    ModifyTxt(tempUserData, tempActiveUserList, tempLoggingReport);
                    ModifyRtf(tempUserData, tempActiveUserList, tempLoggingReport);
                    ModifyHtm(tempUserData, tempActiveUserList, tempLoggingReport);
                    ModifyFiles(tempUserData, tempActiveUserList, tempLoggingReport);
                    ModifyBestanden(tempUserData, tempActiveUserList, tempLoggingReport);

                    CheckIfSignatureIsMade(tempUserData, tempLoggingReport);
                }
                else
                {
                    ModifySignaturesOfAdmin(tempProcessedUsers);
                }
            }
        }

        public static void ModifyTxt(UserData tempUserData, List<UserData> tempActiveUserList, LoggingReport argLoggingReport)
        {
            try
            {
                if (Settings.blnCreateTxt)
                {
                    //TXT bestanden
                    // Pad naar het Admin bestand
                    string tempAdminPathTxt = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.txt";
                    // Pad naar de users zijn Signature bestand
                    string tempUserPathTxt = $"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.txt";
                    // Pad naar het orginele bestand
                    string tempOriginelePathTxt = $"{Settings.strAdminPath}\\{Settings.strOrigineleFile}.txt";
                    if (!Directory.Exists($"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                        return;
                    if (!File.Exists(tempOriginelePathTxt))
                    {
                        Log.Error("Orginele .txt Bestand niet kunnen vinden");
                        return;
                    }

                    // Als het bestand niet bestaat maakt hij hem aan en voegt hij de orginele data erin
                    if (!File.Exists(tempUserPathTxt))
                    {
                        Log.Information($"SIGNATURE VOOR {tempUserData.UserName} WORDT GEMAAKT");
                        File.Copy(tempOriginelePathTxt, tempUserPathTxt, true);
                        // Leest alle text die in het bestand zit
                        string tempSignatureContextTxt = string.Empty;
                        using (StreamReader reader = new StreamReader(tempUserPathTxt, Encoding.Unicode, true))
                        {
                            tempSignatureContextTxt = reader.ReadToEnd();
                        }
                        // Veranderd de Naam, Voorletters, Mail en Functie in het bestand
                        foreach (var tempKeyValuePair in tempUserData.UserValueDictionary)
                        {
                            // Checkt of de key een mail is of niet
                            if (Regex.IsMatch(tempKeyValuePair.Key, "mail", RegexOptions.IgnoreCase) || Regex.IsMatch(tempKeyValuePair.Key, "@", RegexOptions.IgnoreCase))
                                tempSignatureContextTxt = StrCI.Replace(tempSignatureContextTxt, $"#{tempKeyValuePair.Key}#", tempKeyValuePair.Value.ToLower());
                            else
                                tempSignatureContextTxt = StrCI.Replace(tempSignatureContextTxt, $"#{tempKeyValuePair.Key}#", tempKeyValuePair.Value);
                        }
                        foreach (Match match in Regex.Matches(tempSignatureContextTxt, @"(?<=)\#(.*?)\#(?=)"))
                        {
                            tempSignatureContextTxt = tempSignatureContextTxt.Replace(match.Value, "");
                        }
                        StreamWriter sw = new StreamWriter(tempUserPathTxt, false, Encoding.Unicode);
                        sw.Write(tempSignatureContextTxt);
                        sw.Close();
                        argLoggingReport.blnTxtCreated = true;
                        if (tempUserPathTxt.Contains("Â"))
                        {
                            tempUserPathTxt = StrCI.Replace(tempUserPathTxt, "Â", "");
                        }
                    }
                    if (!File.Exists(tempAdminPathTxt))
                        File.Copy(tempUserPathTxt, tempAdminPathTxt, true);

                    if (Settings.blnUserGetsAllSignatures)
                    {
                        // Checkt voor elke active User of er signatures missen van andere Users
                        foreach (var tempActiveUser in tempActiveUserList)
                        {
                            if (!tempActiveUser.Description.ToUpper().Contains(Settings.strBasisNaam.ToUpper()))
                                continue;
                            // pad naar de Directory waar de tempActiveUser zijn signatures opslaat
                            string tempActiveUserPath = $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}";
                            if (!Directory.Exists($"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                                continue;
                            if (!Directory.Exists(tempActiveUserPath))
                            {
                                // Als het niet bestaat maakt hij een nieuwe aan
                                Directory.CreateDirectory(tempActiveUserPath);
                            }

                            // pad naar de Users signature
                            string tempActiveUserPathTxt = $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.txt";
                            if (!Directory.Exists($"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                                continue;
                            if (!File.Exists(tempActiveUserPathTxt))
                            {
                                Log.Information($"{Settings.strFilePrefix}{tempUserData.UserName}.txt is toegevoegd -> {tempActiveUser.UserName}");
                                argLoggingReport.blnSignatureCopied = true;
                                // Als hij niet bestaat bij de User dan kopieert hij hem
                                File.Copy(tempAdminPathTxt, $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.txt", true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public static void ModifyRtf(UserData tempUserData, List<UserData> tempActiveUserList, LoggingReport argLoggingReport)
        {
            try
            {
                if (Settings.blnCreateRtf)
                {
                    //RTF bestanden
                    // Pad naar het Admin bestand
                    string tempAdminPathRTF = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.rtf";
                    // Pad naar de users zijn Signature bestand
                    string tempUserPathRtf = $"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.rtf";
                    // Pad naar het orginele bestand
                    string tempOriginelePathRtf = $"{Settings.strAdminPath}\\{Settings.strOrigineleFile}.rtf";
                    if (!File.Exists(tempOriginelePathRtf))
                    {
                        Log.Error("Orginele .rtf Bestand niet kunnen vinden");
                        return;
                    }
                    if (!Directory.Exists($"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                        return;

                    // Als het bestand niet bestaat maakt hij hem aan en voegt hij de orginele data erin
                    if (!File.Exists(tempUserPathRtf))
                    {
                        File.Copy(tempOriginelePathRtf, tempUserPathRtf, true);
                        // Leest alle text die in het bestand zit
                        string tempSignatureContextRtf = string.Empty;
                        using (StreamReader reader = new StreamReader(tempUserPathRtf, Encoding.Default, true))
                        {
                            tempSignatureContextRtf = reader.ReadToEnd();
                        }
                        // Veranderd de Naam, Voorletters, Mail en Functie in het bestand
                        foreach (var tempKeyValuePair in tempUserData.UserValueDictionary)
                        {
                            // Checkt of de key een mail is of niet
                            if (Regex.IsMatch(tempKeyValuePair.Key, "mail", RegexOptions.IgnoreCase) || Regex.IsMatch(tempKeyValuePair.Key, "@", RegexOptions.IgnoreCase))
                            {
                                var tempValueEscaped = RtfUnicode.GetRtfUnicodeEscapedString(tempKeyValuePair.Value.ToLower());
                                tempSignatureContextRtf = StrCI.Replace(tempSignatureContextRtf, $"#{tempKeyValuePair.Key}#", tempValueEscaped);
                            }
                            else
                                tempSignatureContextRtf = StrCI.Replace(tempSignatureContextRtf, $"#{tempKeyValuePair.Key}#", RtfUnicode.GetRtfUnicodeEscapedString(tempKeyValuePair.Value));
                        }
                        foreach (Match match in Regex.Matches(tempSignatureContextRtf, @"(?<=)\#(.*?)\#(?=)"))
                        {
                            tempSignatureContextRtf = tempSignatureContextRtf.Replace(match.Value, "");
                        }
                        StreamWriter sw = new StreamWriter(tempUserPathRtf, false, Encoding.Default);
                        sw.Write(tempSignatureContextRtf);
                        sw.Close();
                        argLoggingReport.blnRtfCreated = true;
                    }
                    if (!File.Exists(tempAdminPathRTF))
                        File.Copy(tempUserPathRtf, tempAdminPathRTF, true);

                    if (Settings.blnUserGetsAllSignatures)
                    {
                        // Checkt voor elke active tempActiveUser of er signatures missen van andere Users
                        foreach (var tempActiveUser in tempActiveUserList)
                        {
                            if (!tempActiveUser.Description.ToUpper().Contains(Settings.strBasisNaam.ToUpper()))
                                continue;
                            // pad naar de Users signature
                            string tempActiveUserPathRtf = $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.rtf";
                            if (!Directory.Exists($"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                                continue;
                            if (!File.Exists(tempActiveUserPathRtf))
                            {
                                Log.Information($"{Settings.strFilePrefix}{tempUserData.UserName}.rtf is toegevoegd -> {tempActiveUser.UserName}");
                                argLoggingReport.blnSignatureCopied = true;
                                // Als hij niet bestaat bij de tempActiveUser dan kopieert hij hem
                                File.Copy(tempAdminPathRTF, $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.rtf", true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public static void ModifyHtm(UserData tempUserData, List<UserData> tempActiveUserList, LoggingReport argLoggingReport)
        {
            try
            {
                if (Settings.blnCreateHtm)
                {
                    //HTM bestanden
                    // Pad naar het Admin bestand
                    string tempAdminPathHTM = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.htm";
                    // Pad naar de users zijn Signature bestand
                    string tempUserPathHtm = $"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.htm";
                    // Pad naar het orginele bestand
                    string tempOriginelePathHtm = $"{Settings.strAdminPath}\\{Settings.strOrigineleFile}.htm";
                    if (!File.Exists(tempOriginelePathHtm))
                    {
                        Log.Error("Orginele .htm Bestand niet kunnen vinden");
                        return;
                    }
                    if (!Directory.Exists($"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                        return;

                    // Als het bestand niet bestaat maakt hij hem aan en voegt hij de orginele data erin
                    if (!File.Exists(tempUserPathHtm))
                    {
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        File.Copy(tempOriginelePathHtm, tempUserPathHtm, true);
                        string tempSignatureContextHtm = string.Empty;
                        // Leest alle text die in het bestand zit, met encoding 1252
                        using (StreamReader reader = new StreamReader(tempUserPathHtm, Encoding.GetEncoding(1252), true))
                        {
                            tempSignatureContextHtm = reader.ReadToEnd();
                        }

                        // Veranderd de Naam, Voorletters, Mail en Functie in het bestand
                        foreach (var tempKeyValuePair in tempUserData.UserValueDictionary)
                        {
                            // Checkt of de key een mail is of niet
                            if (Regex.IsMatch(tempKeyValuePair.Key, "mail", RegexOptions.IgnoreCase) || Regex.IsMatch(tempKeyValuePair.Key, "@", RegexOptions.IgnoreCase))
                                tempSignatureContextHtm = tempSignatureContextHtm.Replace($"#{tempKeyValuePair.Key}#", HttpUtility.HtmlEncode(tempKeyValuePair.Value)).ToLower();
                            else
                                tempSignatureContextHtm = tempSignatureContextHtm.Replace($"#{tempKeyValuePair.Key}#", HttpUtility.HtmlEncode(tempKeyValuePair.Value));
                        }

                        if (Settings.blnCreateFolderFiles)
                            tempSignatureContextHtm = StrCI.Replace(tempSignatureContextHtm, $"{Settings.strOrigineleFile}_files", $"{Settings.strFilePrefix}{HttpUtility.HtmlEncode(tempUserData.UserName).Replace(" ", "")}_files");

                        if (Settings.blnCreateFolderBestanden)
                            tempSignatureContextHtm = StrCI.Replace(tempSignatureContextHtm, $"{Settings.strOrigineleFile}_bestanden", $"{Settings.strFilePrefix}{HttpUtility.HtmlEncode(tempUserData.UserName).Replace(" ", "")}_bestanden");
                        foreach (Match match in Regex.Matches(tempSignatureContextHtm, @"(?<=)\#(.*?)\#(?=)"))
                        {
                            if (match.ToString() == "#default#" || match.ToString().Contains("<"))
                                continue;
                            tempSignatureContextHtm = tempSignatureContextHtm.Replace(match.Value, "");
                        }
                        // Schrijven met Encoding 1252
                        StreamWriter sw = new StreamWriter(tempUserPathHtm, false, Encoding.GetEncoding(1252));
                        sw.Write(tempSignatureContextHtm);
                        sw.Close();
                        argLoggingReport.blnHtmlCreated = true;
                    }

                    if (!File.Exists(tempAdminPathHTM))
                        File.Copy(tempUserPathHtm, tempAdminPathHTM, true);

                    if (Settings.blnUserGetsAllSignatures)
                    {
                        // Checkt voor elke active User of er signatures missen van andere Users
                        foreach (var tempActiveUser in tempActiveUserList)
                        {
                            if (!tempActiveUser.Description.ToUpper().Contains(Settings.strBasisNaam.ToUpper()))
                                continue;
                            // pad naar de Users signature
                            string tempActiveUserPathHtm = $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.htm";
                            if (!Directory.Exists($"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                                continue;
                            if (!File.Exists(tempActiveUserPathHtm))
                            {
                                Log.Information($"{Settings.strFilePrefix}{tempUserData.UserName}.htm is toegevoegd -> {tempActiveUser.UserName}");
                                argLoggingReport.blnSignatureCopied = true;
                                // Als hij niet bestaat bij de tempActiveUser dan kopieert hij hem
                                File.Copy(tempAdminPathHTM, $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.htm", true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public static void ModifyFiles(UserData tempUserData, List<UserData> tempActiveUserList, LoggingReport argLoggingReport)
        {
            try
            {
                // Checkt in de Settings.ini of Deze functie op True staat
                if (Settings.blnCreateFolderFiles)
                {
                    //_files/_bestanden directories
                    // Het pad naar de signatures zelf
                    string tempOriginalPath = $"{Settings.strAdminPath}";
                    // Het Orginele pad naar _files bestand/directory
                    string tempOriginalFolderPath = $"{Settings.strAdminPath}\\{Settings.strOrigineleFile}_files";
                    // Het pad met de Users _files bestand/directory
                    string tempUserPathFile = $"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_files";
                    if (!Directory.Exists(tempOriginalFolderPath))
                    {
                        Log.Error("Orginele _files Bestand niet kunnen vinden");
                        return;
                    }
                    if (!Directory.Exists($"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                        return;

                    // Checkt of de bestand bestaast zo niet maakt hij een nieuwe aan met alle files erin
                    if (!Directory.Exists(tempUserPathFile))
                    {
                        // Maakt de Directory
                        Directory.CreateDirectory(tempUserPathFile);
                        // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                        foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                        {
                            File.Copy(file, file.Replace(tempOriginalFolderPath, tempUserPathFile), true);
                        }
                        argLoggingReport.blnBestanden_filesCreated = true;
                    }
                    // Pad naar de bestand in de Admin folder
                    string tempAdminPathFile = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_files";
                    
                    if (!Directory.Exists(tempAdminPathFile))
                    {
                        Directory.CreateDirectory(tempAdminPathFile);
                        // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                        foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                        {
                            File.Copy(file, file.Replace(tempOriginalFolderPath, tempAdminPathFile), true);
                        }
                    }
                    if (Settings.blnUserGetsAllSignatures)
                    {
                        foreach (var tempActiveUser in tempActiveUserList)
                        {
                            if (!tempActiveUser.Description.ToUpper().Contains(Settings.strBasisNaam.ToUpper()))
                                continue;
                            // Pad naar de bestand in de tempActiveUser folder
                            string tempActiveUserPathFile = $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_files";
                            if (!Directory.Exists($"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                                continue;
                            if (!Directory.Exists(tempActiveUserPathFile))
                            {
                                Directory.CreateDirectory(tempActiveUserPathFile);
                                // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                                foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                                {
                                    File.Copy(file, file.Replace(tempOriginalFolderPath, tempActiveUserPathFile), true);
                                }
                                Log.Information($"{Settings.strFilePrefix}{tempUserData.UserName}_files is toegevoegd -> {tempActiveUser.UserName}");
                                argLoggingReport.blnSignatureCopied = true;
                            }
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public static void ModifyBestanden(UserData tempUserData, List<UserData> tempActiveUserList, LoggingReport argLoggingReport)
        {
            try
            {
                // Checkt in de Settings.ini of Deze functie op True staat
                if (Settings.blnCreateFolderBestanden)
                {
                    //_files/_bestanden directories
                    // Het pad naar de signatures zelf
                    string tempOriginalPath = $"{Settings.strAdminPath}";
                    // Het Orginele pad naar _bestanden bestand/directory
                    string tempOriginalFolderPath = $"{Settings.strAdminPath}\\{Settings.strOrigineleFile}_bestanden";
                    // het pad met de Users _bestanden bestand/directory
                    string tempUserPathBestand = $"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_bestanden";
                    if (!Directory.Exists(tempOriginalFolderPath))
                    {
                        Log.Error("Orginele _files Bestand niet kunnen vinden");
                        return;
                    }
                    if (!Directory.Exists(tempUserPathBestand))
                    {
                        // Maakt de Directory
                        Directory.CreateDirectory(tempUserPathBestand);
                        // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                        foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                        {
                            File.Copy(file, file.Replace(tempOriginalFolderPath, tempUserPathBestand), true);
                        }
                        argLoggingReport.blnBestanden_bestandenCreated = true;
                    }
                    // Pad naar de bestand in de Admin folder
                    string tempAdminPathBestand = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_bestanden";
                    if (!Directory.Exists(tempAdminPathBestand))
                    {
                        Directory.CreateDirectory(tempAdminPathBestand);
                        // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                        foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                        {
                            File.Copy(file, file.Replace(tempOriginalFolderPath, tempAdminPathBestand), true);
                        }
                    }
                    if (Settings.blnUserGetsAllSignatures)
                    {
                        foreach (var tempActiveUser in tempActiveUserList)
                        {
                            if (!tempActiveUser.Description.ToUpper().Contains(Settings.strBasisNaam.ToUpper()))
                                continue;
                            // Pad naar de bestand in de tempActiveUser folder
                            string tempActiveUserPathBestand = $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_bestanden";
                            if (!Directory.Exists($"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                                continue;
                            if (!Directory.Exists(tempActiveUserPathBestand))
                            {
                                Directory.CreateDirectory(tempActiveUserPathBestand);
                                // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                                foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                                {
                                    File.Copy(file, file.Replace(tempOriginalFolderPath, tempActiveUserPathBestand), true);
                                }
                                Log.Information($"{Settings.strFilePrefix}{tempUserData.UserName}_bestanden is toegevoegd -> {tempActiveUser.UserName}");
                                argLoggingReport.blnSignatureCopied = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}

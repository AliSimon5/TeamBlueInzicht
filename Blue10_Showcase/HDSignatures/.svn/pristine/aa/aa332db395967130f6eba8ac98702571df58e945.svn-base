using HDSignatures.Logic;
using HDSignatures.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace HDSignatures.Data
{
    internal partial class DataManager
    {
        /// <summary>
        /// Pakt alle variables waar de info in zit en gebruikt ze om Nieuwe Signatures te maken voor elke User
        /// </summary>
        /// <param name="tempDescOfUsers">Een list met alle Info over de Users</param>
        public static void ModifySignaturesOfAdmin(List<UserData> tempDescOfUsers)
        {
            foreach (UserData tempUserData in tempDescOfUsers)
            {
                var tempLoggingReport = new LoggingReport();

                ModifyTxtOfAdmin(tempUserData, tempLoggingReport);
                ModifyRtfOfAdmin(tempUserData, tempLoggingReport);
                ModifyHtmOfAdmin(tempUserData, tempLoggingReport);
                ModifyFilesOfAdmin(tempUserData, tempLoggingReport);
                ModifyBestandenOfAdmin(tempUserData, tempLoggingReport);

                CheckIfSignatureIsMade(tempUserData, tempLoggingReport);
            }
        }
        public static void ModifyTxtOfAdmin(UserData tempUserData, LoggingReport argLoggingReport)
        {
            try
            {
                if (Settings.blnCreateTxt)
                {
                    //TXT bestanden
                    // Pad naar het Admin bestand
                    string tempAdminPathTxt = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.txt";
                    // Pad naar de users zijn Signature bestand
                    string tempUserPathTxt = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.txt";
                    // Pad naar het orginele bestand
                    string tempOriginelePathTxt = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strOrigineleFile}.txt";
                    if (!File.Exists(tempOriginelePathTxt))
                    {
                        Log.Error("Orginele .txt Bestand niet kunnen vinden");
                        return;
                    }

                    // Als het bestand niet bestaat maakt hij hem aan en voegt hij de orginele data erin
                    if (!File.Exists(tempUserPathTxt))
                    {
                        Console.WriteLine("\n\r");
                        Log.Information($"SIGNATURE VOOR {Settings.strAdminNaam} WORDT GEMAAKT");
                        File.Copy(tempOriginelePathTxt, tempUserPathTxt, true);
                        // Leest alle text die in het bestand zit
                        string tempSignatureContextTxt = File.ReadAllText(tempUserPathTxt);
                        // Veranderd de Naam, Voorletters, Mail en Functie in het bestand
                        foreach (var tempKeyValuePair in tempUserData.UserValueDictionary)
                        {
                            // Checkt of de key een mail is of niet
                            if (Regex.IsMatch(tempKeyValuePair.Key, "mail", RegexOptions.IgnoreCase) || Regex.IsMatch(tempKeyValuePair.Key, "@", RegexOptions.IgnoreCase))
                                tempSignatureContextTxt = tempSignatureContextTxt.Replace($"#{tempKeyValuePair.Key}#", tempKeyValuePair.Value.ToLower(), StringComparison.OrdinalIgnoreCase);
                            else
                                tempSignatureContextTxt = tempSignatureContextTxt.Replace($"#{tempKeyValuePair.Key}#", tempKeyValuePair.Value, StringComparison.OrdinalIgnoreCase);
                        }
                        foreach (Match match in Regex.Matches(tempSignatureContextTxt, @"(?<=)\#(.*?)\#(?=)"))
                        {
                            tempSignatureContextTxt = tempSignatureContextTxt.Replace(match.Value, "");
                        }
                        File.WriteAllText(tempUserPathTxt, tempSignatureContextTxt);
                        argLoggingReport.blnTxtCreated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public static void ModifyRtfOfAdmin(UserData tempUserData, LoggingReport argLoggingReport)
        {
            try
            {
                if (Settings.blnCreateRtf)
                {
                    //RTF bestanden
                    // Pad naar het Admin bestand
                    string tempAdminPathRTF = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.rtf";
                    // Pad naar de users zijn Signature bestand
                    string tempUserPathRtf = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.rtf";
                    // Pad naar het orginele bestand
                    string tempOriginelePathRtf = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strOrigineleFile}.rtf";
                    if (!File.Exists(tempOriginelePathRtf))
                    {
                        Log.Error("Orginele .rtf Bestand niet kunnen vinden");
                        return;
                    }
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
                            if (Regex.IsMatch(tempKeyValuePair.Key, "mail", RegexOptions.IgnoreCase) || Regex.IsMatch(tempKeyValuePair.Key, "@", RegexOptions.IgnoreCase))
                            {
                                var tempValueEscaped = LogicManager.GetRtfUnicodeEscapedString(tempKeyValuePair.Value.ToLower());
                                tempSignatureContextRtf = tempSignatureContextRtf.Replace($"#{tempKeyValuePair.Key}#", tempValueEscaped, StringComparison.OrdinalIgnoreCase);
                            }
                            else
                                tempSignatureContextRtf = tempSignatureContextRtf.Replace($"#{tempKeyValuePair.Key}#", LogicManager.GetRtfUnicodeEscapedString(tempKeyValuePair.Value), StringComparison.OrdinalIgnoreCase);
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
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public static void ModifyHtmOfAdmin(UserData tempUserData, LoggingReport argLoggingReport)
        {
            try
            {
                if (Settings.blnCreateHtm)
                {
                    //HTM bestanden
                    // Pad naar het Admin bestand
                    string tempAdminPathHTM = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.htm";
                    // Pad naar de users zijn Signature bestand
                    string tempUserPathHtm = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.htm";
                    // Pad naar het orginele bestand
                    string tempOriginelePathHtm = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strOrigineleFile}.htm";
                    if (!File.Exists(tempOriginelePathHtm))
                    {
                        Log.Error("Orginele .htm Bestand niet kunnen vinden");
                        return;
                    }
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
                                tempSignatureContextHtm = tempSignatureContextHtm.Replace($"#{tempKeyValuePair.Key}#", HttpUtility.HtmlEncode(tempKeyValuePair.Value).ToLower(), StringComparison.OrdinalIgnoreCase);
                            else
                                tempSignatureContextHtm = tempSignatureContextHtm.Replace($"#{tempKeyValuePair.Key}#", HttpUtility.HtmlEncode(tempKeyValuePair.Value), StringComparison.OrdinalIgnoreCase);
                        }
                        if (Settings.blnCreateFolderFiles)
                            tempSignatureContextHtm = tempSignatureContextHtm.Replace($"{Settings.strOrigineleFile}_files", $"{Settings.strFilePrefix}{HttpUtility.HtmlEncode(tempUserData.UserName).Replace(" ", "")}_files");

                        if (Settings.blnCreateFolderBestanden)
                            tempSignatureContextHtm = tempSignatureContextHtm.Replace($"{Settings.strOrigineleFile}_bestanden", $"{Settings.strFilePrefix}{HttpUtility.HtmlEncode(tempUserData.UserName).Replace(" ", "")}_bestanden");

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
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public static void ModifyFilesOfAdmin(UserData tempUserData, LoggingReport argLoggingReport)
        {
            try
            {
                //_files/_bestanden directories
                // Het pad naar de signatures zelf
                string tempOriginalPath = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}";
                // Het Orginele pad naar _files bestand/directory
                string tempOriginalFolderPathFiles = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strOrigineleFile}_files";
                // Het pad met de Users _files bestand/directory
                string tempUserPathFile = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_files";
                if (Settings.blnCreateFolderFiles)
                {
                    if (!Directory.Exists(tempOriginalFolderPathFiles))
                    {
                        Log.Error("Orginele _files Bestand niet kunnen vinden");
                        return;
                    }
                    // Checkt in de Settings.ini of Deze functie op True staat

                    // Checkt of de bestand bestaast zo niet maakt hij een nieuwe aan met alle files erin
                    if (!Directory.Exists(tempUserPathFile))
                    {
                        // Maakt de Directory
                        Directory.CreateDirectory(tempUserPathFile);
                        // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                        foreach (string file in Directory.GetFiles(tempOriginalFolderPathFiles, "*.*", SearchOption.AllDirectories))
                        {
                            File.Copy(file, file.Replace(tempOriginalFolderPathFiles, tempUserPathFile), true);
                        }
                        argLoggingReport.blnBestanden_filesCreated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public static void ModifyBestandenOfAdmin(UserData tempUserData, LoggingReport argLoggingReport)
        {
            try
            {
                //_files/_bestanden directories
                // Het pad naar de signatures zelf
                string tempOriginalPath = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}";
                // Het Orginele pad naar _bestanden bestand/directory
                string tempOriginalFolderPathBestanden = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strOrigineleFile}_bestanden";
                // het pad met de Users _bestanden bestand/directory
                string tempUserPathBestand = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_bestanden";

                // Checkt in de Settings.ini of Deze functie op True staat
                if (Settings.blnCreateFolderBestanden)
                {
                    if (!Directory.Exists(tempOriginalFolderPathBestanden))
                    {
                        Log.Error("Orginele _files Bestand niet kunnen vinden");
                        return;
                    }
                    if (!Directory.Exists(tempUserPathBestand))
                    {
                        // Maakt de Directory
                        Directory.CreateDirectory(tempUserPathBestand);
                        // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                        foreach (string file in Directory.GetFiles(tempOriginalFolderPathBestanden, "*.*", SearchOption.AllDirectories))
                        {
                            File.Copy(file, file.Replace(tempOriginalFolderPathBestanden, tempUserPathBestand), true);
                        }
                        argLoggingReport.blnBestanden_bestandenCreated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public static bool CheckIfSignatureIsMade(UserData tempUserData, LoggingReport argLoggingReport)
        {
            var tempTest = $"{tempUserData.UserName} - Volgende bestanden zijn toegevoegd: ";
            if (argLoggingReport.blnTxtCreated)
            {
                tempTest += ".txt";
                argLoggingReport.blnComma = true;
            }
            if (argLoggingReport.blnRtfCreated)
            {
                if (argLoggingReport.blnComma) tempTest += ", ";
                tempTest += ".rtf";
                argLoggingReport.blnComma = true;
            }
            if (argLoggingReport.blnHtmlCreated)
            {
                if (argLoggingReport.blnComma) tempTest += ", ";
                tempTest += $".htm";
                argLoggingReport.blnComma = true;
            }
            if (argLoggingReport.blnBestanden_filesCreated)
            {
                if (argLoggingReport.blnComma) tempTest += ", ";
                tempTest += $"_files";
                argLoggingReport.blnComma = true;
            }
            if (argLoggingReport.blnBestanden_bestandenCreated)
            {
                if (argLoggingReport.blnComma) tempTest += ", ";
                tempTest += $"_bestanden";
                argLoggingReport.blnComma = true;
            }
            if (argLoggingReport.blnTxtCreated || argLoggingReport.blnRtfCreated || argLoggingReport.blnHtmlCreated || argLoggingReport.blnBestanden_filesCreated || argLoggingReport.blnBestanden_bestandenCreated)
                Log.Information(tempTest);
            else if (!argLoggingReport.blnSignatureCopied)
                Log.Information($"Geen nieuwe Signatures gemaakt - {tempUserData.UserName}");
            return false;
        }
    }
}

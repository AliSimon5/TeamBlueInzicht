﻿using HDSignaturesTool.Helpers;
using HDSignaturesTool.Logic;
using HDSignaturesTool.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace HDSignaturesTool.Data
{
    internal partial class DataManager
    {
        public static void UpdateSignatures(List<UserData> tempProcessedUserList, List<UserData> tempActiveUserList)
        {

            if (!Settings.blnAdminOnly)
            {
                UpdateTxtSignatures(tempProcessedUserList, tempActiveUserList);
                UpdateRtfSignatures(tempProcessedUserList, tempActiveUserList);
                UpdateHtmSignatures(tempProcessedUserList, tempActiveUserList);
                UpdateFolderFiles(tempProcessedUserList, tempActiveUserList);
                UpdateFolderBestanden(tempProcessedUserList, tempActiveUserList);
            }
            else
            {
                UpdateTxtSignaturesAdminOnly(tempProcessedUserList);
                UpdateRtfSignaturesAdminOnly(tempProcessedUserList);
                UpdateHtmSignaturesAdminOnly(tempProcessedUserList);
                UpdateFolderFilesAdminOnly(tempProcessedUserList);
                UpdateFolderBestandenAdminOnly(tempProcessedUserList);
            }

        }

        public static bool CheckIfUpdateIsNeeded()
        {
            if (Settings.intCount > 4)
                Settings.intCount = 0;
            Settings.intCount++;
            string tempOriginelePathTxt = $"{Settings.strAdminPath}\\{Settings.strOrigineleFile}.htm";
            DateTime tempLastWriteTime = File.GetLastWriteTime(tempOriginelePathTxt);
            if (Settings.dtLastWriteTime == tempLastWriteTime)
            {
                if (Settings.intCount >= 3)
                    Settings.intCount = 0;
                return false;
            }
            else
            {
                if (Settings.intCount >= 3)
                {
                    Settings.dtLastWriteTime = tempLastWriteTime;
                    Settings.intCount = 0;
                }
                return true;
            }
        }


        /// <summary>
        /// Update de Txt Signatures bij iedereen
        /// </summary>
        /// <param name="tempDescOfUsers"></param>
        /// <param name="tempActiveUserList"></param>
        public static void UpdateTxtSignatures(List<UserData> tempDescOfUsers, List<UserData> tempActiveUserList)
        {
            foreach (UserData tempUserData in tempDescOfUsers)
            {
                try
                {
                    Log.Verbose("UpdateTxt bereikt");
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
                        {
                            Log.Verbose("Continued bij checken of directory bestaat");
                            continue;
                        }
                        if (!File.Exists(tempOriginelePathTxt))
                        {
                            Log.Error("Orginele .txt Bestand niet kunnen vinden");
                            break;
                        }

                        // Als het bestand niet bestaat maakt hij hem aan en voegt hij de orginele data erin
                        if (File.Exists(tempUserPathTxt))
                        {
                            Log.Information($"Signature van {tempUserData.UserName} wordt bijgewerkt");
                            string tempOrigineleSignatureContextTxt = string.Empty;
                            using (StreamReader reader = new StreamReader(tempOriginelePathTxt, Encoding.Unicode, true))
                            {
                                tempOrigineleSignatureContextTxt = reader.ReadToEnd();
                            }

                            StreamWriter sw = new StreamWriter(tempUserPathTxt, false, Encoding.Unicode);
                            sw.Write(tempOrigineleSignatureContextTxt);
                            sw.Close();
                            if (!File.Exists(tempAdminPathTxt))
                                File.Copy(tempUserPathTxt, tempAdminPathTxt, true);
                            else
                            {
                                StreamWriter sw2 = new StreamWriter(tempAdminPathTxt, false, Encoding.Unicode);
                                sw2.Write(tempOrigineleSignatureContextTxt);
                                sw2.Close();
                            }

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
                            StreamWriter sw3 = new StreamWriter(tempUserPathTxt, false, Encoding.Unicode);
                            sw3.Write(tempSignatureContextTxt);
                            sw3.Close();
                            if (!File.Exists(tempAdminPathTxt))
                                File.Copy(tempUserPathTxt, tempAdminPathTxt, true);
                            else
                            {
                                StreamWriter sw4 = new StreamWriter(tempAdminPathTxt, false, Encoding.Unicode);
                                sw4.Write(tempSignatureContextTxt);
                                sw4.Close();
                            }
                            if (tempUserPathTxt.Contains("Â"))
                            {
                                tempUserPathTxt = StrCI.Replace(tempUserPathTxt, "Â", "");
                            }
                        }

                        if (Settings.blnUserGetsAllSignatures)
                        {
                            // Checkt voor elke active User of er signatures missen van andere Users
                            foreach (var tempActiveUser in tempActiveUserList)
                            {
                                if (!tempActiveUser.Description.ToUpper().Contains(Settings.strBasisNaam.ToUpper()))
                                    continue;
                                // pad naar de Users signature
                                string tempActiveUserPathTxt = $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.txt";
                                if (!Directory.Exists($"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                                    continue;
                                if (File.Exists(tempActiveUserPathTxt))
                                {
                                    string tempOrigineleSignatureContextTxt = string.Empty;
                                    using (StreamReader reader = new StreamReader(tempOriginelePathTxt, Encoding.Unicode, true))
                                    {
                                        tempOrigineleSignatureContextTxt = reader.ReadToEnd();
                                    }
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
        }

        /// <summary>
        /// Update de Rtf Signatures bij iedereen
        /// </summary>
        /// <param name="tempDescOfUsers"></param>
        /// <param name="tempActiveUserList"></param>
        public static void UpdateRtfSignatures(List<UserData> tempDescOfUsers, List<UserData> tempActiveUserList)
        {
            foreach (UserData tempUserData in tempDescOfUsers)
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
                            break;
                        }
                        if (!Directory.Exists($"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                            continue;

                        // Als het bestand niet bestaat maakt hij hem aan en voegt hij de orginele data erin
                        if (File.Exists(tempUserPathRtf))
                        {
                            string tempOrigineleSignatureContextRtf = string.Empty;
                            using (StreamReader reader = new StreamReader(tempOriginelePathRtf, Encoding.Default, true))
                            {
                                tempOrigineleSignatureContextRtf = reader.ReadToEnd();
                            }
                            StreamWriter sw = new StreamWriter(tempUserPathRtf, false, Encoding.Default);
                            sw.Write(tempOrigineleSignatureContextRtf);
                            sw.Close();

                            if (!File.Exists(tempAdminPathRTF))
                                File.Copy(tempUserPathRtf, tempAdminPathRTF, true);
                            else
                            {
                                StreamWriter sw2 = new StreamWriter(tempAdminPathRTF, false, Encoding.Default);
                                sw2.Write(tempOrigineleSignatureContextRtf);
                                sw2.Close();
                            }

                            // Leest alle text die in het bestand zitcx
                            string tempSignatureContextRtf = string.Empty;
                            using (StreamReader reader = new StreamReader(tempUserPathRtf, Encoding.Default, true))
                            {
                                tempSignatureContextRtf = reader.ReadToEnd();
                            }
                            // Veranderd de Naam, Voorletters, Mail en Functie in het bestand
                            foreach (var tempKeyValuePair in tempUserData.UserValueDictionary)
                            {
                                // string strValue = LogicManager.GetRtfUnicodeEscapedString(tempKeyValuePair.Value);
                                // string strKey = LogicManager.GetRtfUnicodeEscapedString(tempKeyValuePair.Key);
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
                            StreamWriter sw3 = new StreamWriter(tempUserPathRtf, false, Encoding.Default);
                            sw3.Write(tempSignatureContextRtf);
                            sw3.Close();
                            if (!File.Exists(tempAdminPathRTF))
                                File.Copy(tempUserPathRtf, tempAdminPathRTF, true);
                            else
                            {
                                StreamWriter sw4 = new StreamWriter(tempAdminPathRTF, false, Encoding.Default);
                                sw4.Write(tempSignatureContextRtf);
                                sw4.Close();
                            }
                        }

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
                                if (File.Exists(tempActiveUserPathRtf))
                                {
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
        }

        /// <summary>
        /// Update de Htm Signatures bij iedereen
        /// </summary>
        /// <param name="tempDescOfUsers"></param>
        /// <param name="tempActiveUserList"></param>
        public static void UpdateHtmSignatures(List<UserData> tempDescOfUsers, List<UserData> tempActiveUserList)
        {
            foreach (UserData tempUserData in tempDescOfUsers)
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
                            break;
                        }
                        if (!Directory.Exists($"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                            continue;

                        // Als het bestand niet bestaat maakt hij hem aan en voegt hij de orginele data erin
                        if (File.Exists(tempUserPathHtm))
                        {
                            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                            // Leest alle text die in het bestand zit, met encoding 1252
                            string tempOrigineleSignatureContextHtm = string.Empty;
                            using (StreamReader reader = new StreamReader(tempOriginelePathHtm, Encoding.GetEncoding(1252), true))
                            {
                                tempOrigineleSignatureContextHtm = reader.ReadToEnd();
                            }

                            StreamWriter sw2 = new StreamWriter(tempUserPathHtm, false, Encoding.GetEncoding(1252));
                            sw2.Write(tempOrigineleSignatureContextHtm);
                            sw2.Close();

                            string tempSignatureContextHtm = string.Empty;
                            using (StreamReader reader = new StreamReader(tempUserPathHtm, Encoding.GetEncoding(1252), true))
                            {
                                tempSignatureContextHtm = reader.ReadToEnd();
                            }

                            // Veranderd de Naam, Voorletters, Mail en Functie in het bestand
                            foreach (var tempKeyValuePair in tempUserData.UserValueDictionary)
                            {
                                // Checkt of de key een mail is of niet
                                if (Regex.IsMatch(tempKeyValuePair.Key, "mail", RegexOptions.IgnoreCase) || Regex.IsMatch(tempKeyValuePair.Key, "@", RegexOptions.IgnoreCase))
                                    tempSignatureContextHtm = StrCI.Replace(tempSignatureContextHtm, $"#{tempKeyValuePair.Key}#", HttpUtility.HtmlEncode(tempKeyValuePair.Value).ToLower());
                                else
                                    tempSignatureContextHtm = StrCI.Replace(tempSignatureContextHtm, $"#{tempKeyValuePair.Key}#", HttpUtility.HtmlEncode(tempKeyValuePair.Value));
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
                            if (!File.Exists(tempAdminPathHTM))
                                File.Copy(tempUserPathHtm, tempAdminPathHTM, true);
                            else
                            {
                                StreamWriter sw3 = new StreamWriter(tempAdminPathHTM, false, Encoding.GetEncoding(1252));
                                sw3.Write(tempSignatureContextHtm);
                                sw3.Close();
                            }
                        }

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
                                if (File.Exists(tempActiveUserPathHtm))
                                {
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
        }

        /// <summary>
        /// Update de folder Files Signatures bij iedereen
        /// </summary>
        /// <param name="tempDescOfUsers"></param>
        /// <param name="tempActiveUserList"></param>
        public static void UpdateFolderFiles(List<UserData> tempDescOfUsers, List<UserData> tempActiveUserList)
        {
            foreach (UserData tempUserData in tempDescOfUsers)
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
                            break;
                        }
                        if (!Directory.Exists($"{tempUserData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                            continue;

                        // Checkt of de bestand bestaast zo niet maakt hij een nieuwe aan met alle files erin
                        if (Directory.Exists(tempUserPathFile))
                        {
                            // Maakt de Directory
                            Directory.Delete(tempUserPathFile, true);
                            Directory.CreateDirectory(tempUserPathFile);
                            // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                            foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                            {
                                File.Copy(file, file.Replace(tempOriginalFolderPath, tempUserPathFile), true);
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
                                if (Directory.Exists(tempActiveUserPathFile))
                                {
                                    Directory.Delete(tempActiveUserPathFile, true);
                                    Directory.CreateDirectory(tempActiveUserPathFile);
                                    // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                                    foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                                    {
                                        File.Copy(file, file.Replace(tempOriginalFolderPath, tempActiveUserPathFile), true);
                                    }
                                }
                                // Pad naar de bestand in de Admin folder
                                string tempAdminPathFile = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_files";
                                if (!Directory.Exists($"{Settings.strAdminPath}"))
                                    continue;
                                if (Directory.Exists(tempAdminPathFile))
                                {
                                    Directory.Delete(tempAdminPathFile, true);
                                    Directory.CreateDirectory(tempAdminPathFile);
                                    // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                                    foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                                    {
                                        File.Copy(file, file.Replace(tempOriginalFolderPath, tempAdminPathFile), true);
                                    }
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

        /// <summary>
        /// Update de folder Bestanden Signatures van iedereen
        /// </summary>
        /// <param name="tempDescOfUsers"></param>
        /// <param name="tempActiveUserList"></param>
        public static void UpdateFolderBestanden(List<UserData> tempDescOfUsers, List<UserData> tempActiveUserList)
        {
            foreach (UserData tempUserData in tempDescOfUsers)
            {
                try
                {
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
                            break;
                        }
                        if (Directory.Exists(tempUserPathBestand))
                        {
                            // Maakt de Directory
                            Directory.Delete(tempUserPathBestand, true);
                            Directory.CreateDirectory(tempUserPathBestand);
                            // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                            foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                            {
                                File.Copy(file, file.Replace(tempOriginalFolderPath, tempUserPathBestand), true);
                            }
                        }
                        // Pad naar de bestand in de Admin folder
                        string tempAdminPathBestand = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_bestanden";
                        if (Directory.Exists(tempAdminPathBestand))
                        {
                            Directory.Delete(tempAdminPathBestand, true);
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
                                    if (!tempActiveUser.Description.ToUpper().Contains(Settings.strBasisNaam.ToUpper()))
                                        continue;
                                // Pad naar de bestand in de tempActiveUser folder
                                string tempActiveUserPathBestand = $"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_bestanden";
                                if (!Directory.Exists($"{tempActiveUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                                    continue;
                                if (Directory.Exists(tempActiveUserPathBestand))
                                {
                                    Directory.Delete(tempActiveUserPathBestand, true);
                                    Directory.CreateDirectory(tempActiveUserPathBestand);
                                    // Loopje om elke file in het originele bestand in de nieuw gemaakte bestand te zetten
                                    foreach (string file in Directory.GetFiles(tempOriginalFolderPath, "*.*", SearchOption.AllDirectories))
                                    {
                                        File.Copy(file, file.Replace(tempOriginalFolderPath, tempActiveUserPathBestand), true);
                                    }
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
}

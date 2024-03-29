﻿using HDSignaturesTool.Logic;
using HDSignaturesTool.Types;
using HDSignaturesTool.Helpers;
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
        /// <summary>
        /// Update de Txt signatures bij aleen de signatures die in de admin folder zitten
        /// </summary>
        /// <param name="tempDescOfUsers"></param>
        public static void UpdateTxtSignaturesAdminOnly(List<UserData> tempDescOfUsers)
        {
            foreach (UserData tempUserData in tempDescOfUsers)
            {
                try
                {
                    if (Settings.blnCreateTxt)
                    {
                        //TXT bestanden
                        // Pad naar de users zijn Signature bestand
                        string tempUserPathTxt = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.txt";
                        // Pad naar het orginele bestand
                        string tempOriginelePathTxt = $"{Settings.strAdminPath}\\{Settings.strOrigineleFile}.txt";
                        if (!Directory.Exists($"{Settings.strAdminPath}"))
                            continue;
                        if (!File.Exists(tempOriginelePathTxt))
                        {
                            Log.Error("Orginele .txt Bestand niet kunnen vinden");
                            break;
                        }

                        // Als het bestand niet bestaat maakt hij hem aan en voegt hij de orginele data erin
                        if (File.Exists(tempUserPathTxt))
                        {
                            string tempOrigineleSignatureContextTxt = string.Empty;
                            using (StreamReader reader = new StreamReader(tempOriginelePathTxt, Encoding.Unicode, true))
                            {
                                tempOrigineleSignatureContextTxt = reader.ReadToEnd();
                            }

                            StreamWriter sw = new StreamWriter(tempUserPathTxt, false, Encoding.Unicode);
                            sw.Write(tempOrigineleSignatureContextTxt);
                            sw.Close();

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

                            StreamWriter sw2 = new StreamWriter(tempUserPathTxt, false, Encoding.Unicode);
                            sw2.Write(tempSignatureContextTxt);
                            sw2.Close();
                            if (tempUserPathTxt.Contains("Â"))
                            {
                                tempUserPathTxt = StrCI.Replace(tempUserPathTxt, "Â", "");
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
        /// Update de Rtf signatures bij aleen de signatures die in de admin folder zitten
        /// </summary>
        /// <param name="tempDescOfUsers"></param>
        public static void UpdateRtfSignaturesAdminOnly(List<UserData> tempDescOfUsers)
        {
            foreach (UserData tempUserData in tempDescOfUsers)
            {
                try
                {
                    if (Settings.blnCreateRtf)
                    {
                        //RTF bestanden
                        // Pad naar de admin zijn Signature bestand
                        string tempUserPathRtf = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.rtf";
                        // Pad naar het orginele bestand
                        string tempOriginelePathRtf = $"{Settings.strAdminPath}\\{Settings.strOrigineleFile}.rtf";
                        if (!File.Exists(tempOriginelePathRtf))
                        {
                            Log.Error("Orginele .rtf Bestand niet kunnen vinden");
                            break;
                        }
                        if (!Directory.Exists($"{Settings.strAdminPath}"))
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

                            // Leest alle text die in het bestand zitcx
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
                            StreamWriter sw3 = new StreamWriter(tempUserPathRtf, false, Encoding.Default);
                            sw3.Write(tempSignatureContextRtf);
                            sw3.Close();
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
        /// Update de Htm signatures bij aleen de signatures die in de admin folder zitten
        /// </summary>
        /// <param name="tempDescOfUsers"></param>
        public static void UpdateHtmSignaturesAdminOnly(List<UserData> tempDescOfUsers)
        {
            foreach (UserData tempUserData in tempDescOfUsers)
            {
                try
                {
                    if (Settings.blnCreateHtm)
                    {
                        //HTM bestanden
                        // Pad naar de Admin zijn Signature bestand
                        string tempUserPathHtm = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}.htm";
                        // Pad naar het orginele bestand
                        string tempOriginelePathHtm = $"{Settings.strAdminPath}\\{Settings.strOrigineleFile}.htm";
                        if (!File.Exists(tempOriginelePathHtm))
                        {
                            Log.Error("Orginele .htm Bestand niet kunnen vinden");
                            break;
                        }
                        if (!Directory.Exists($"{Settings.strAdminPath}"))
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
                                if (match.ToString() == "#default#" || match.ToString().Contains("<") || match.ToString().Contains(">"))
                                    continue;
                                tempSignatureContextHtm = tempSignatureContextHtm.Replace(match.Value, "");
                            }
                            // Schrijven met Encoding 1252
                            StreamWriter sw = new StreamWriter(tempUserPathHtm, false, Encoding.GetEncoding(1252));
                            sw.Write(tempSignatureContextHtm);
                            sw.Close();

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
        /// Update de folder Files signatures bij aleen de signatures die in de admin folder zitten
        /// </summary>
        /// <param name="tempDescOfUsers"></param>
        public static void UpdateFolderFilesAdminOnly(List<UserData> tempDescOfUsers)
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
                        string tempUserPathFile = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_files";
                        if (!Directory.Exists(tempOriginalFolderPath))
                        {
                            Log.Error("Orginele _files Bestand niet kunnen vinden");
                            break;
                        }
                        if (!Directory.Exists($"{Settings.strAdminPath}"))
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
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }
        }

        /// <summary>
        /// Update de folder Bestanden signatures bij aleen de signatures die in de admin folder zitten
        /// </summary>
        /// <param name="tempDescOfUsers"></param>
        public static void UpdateFolderBestandenAdminOnly(List<UserData> tempDescOfUsers)
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
                        string tempUserPathBestand = $"{Settings.strAdminPath}\\{Settings.strFilePrefix}{tempUserData.UserName.Replace(" ", "")}_bestanden";
                        if (!Directory.Exists(tempOriginalFolderPath))
                        {
                            Log.Error("Orginele _bestanden Bestand niet kunnen vinden");
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

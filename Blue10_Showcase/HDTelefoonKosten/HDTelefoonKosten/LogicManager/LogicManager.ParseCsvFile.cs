﻿using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using HDTelefoonKosten.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace HDTelefoonKosten.LogicManager
{
    internal partial class LogicManager
    {
        /// <summary>
        /// Parsed verschillende soorten excel bestanden (converted naar List string (die voor 1 bestand staat)) naar List CallData
        /// </summary>
        /// <param name="tempAllRowList"></param>
        /// <returns></returns>
        public static List<CallData> ParseAllRowsFromFile(List<string> tempAllRowList)
        {
            var tempFirstRow = tempAllRowList.First();

            // Controleer welke parser gebruikt moet worden
            if ((tempFirstRow.ToLower().StartsWith("direction".ToLower()) || tempFirstRow.ToLower().StartsWith("dir".ToLower())) && !tempFirstRow.ToLower().Contains("reseller".ToLower()))
                return ParseAllRowsTypeA(tempAllRowList);

            if (tempFirstRow.ToLower().StartsWith("klant".ToLower()))
                return ParseAllRowsTypeB(tempAllRowList);

            if (tempFirstRow.ToLower().StartsWith("YYYY".ToLower()))
                return ParseAllRowsTypeC(tempAllRowList);

            if (tempFirstRow.ToLower().StartsWith("Enduser".ToLower()))
                return ParseAllRowsTypeD(tempAllRowList);

            if (tempFirstRow.ToLower().StartsWith("direction".ToLower()) && tempFirstRow.ToLower().Contains("reseller".ToLower()))
                return ParseAllRowsTypeE(tempAllRowList);

            if (tempFirstRow.ToLower().StartsWith("Timestamp".ToLower()) && !tempFirstRow.ToLower().Contains("out".ToLower()))
                return ParseAllRowsTypeF(tempAllRowList);

            if (tempFirstRow.ToLower().StartsWith("traffic".ToLower()) && !tempFirstRow.ToLower().Contains("dir".ToLower()))
                return ParseAllRowsTypeG(tempAllRowList);

            if (tempFirstRow.ToLower().StartsWith("Verbruik".ToLower()) && !tempFirstRow.ToLower().Contains("dir".ToLower()))
                return ParseAllRowsTypeH(tempAllRowList);

            throw new Exception("Parsen van Excel data is mislukt, geen herkenningspunt gevonden voor parsen.");
        }

        
        public static List<CompanyMargesType> ParseAllDataRowsFromFile(List<string> tempAllRowList)
        {
            var tempFirstRow = tempAllRowList.First();

            if (tempFirstRow.ToLower().StartsWith("Voip".ToLower()))
                return ParseAllRowsTypeData(tempAllRowList);

            throw new Exception("Parsen van Excel data is mislukt, geen herkenningspunt gevonden voor parsen.");
        }

        public static List<CallData> ParseAllRowsTypeA(List<string> tempAllRowsList)
        {
            List<CallData> tempParsedRowsList = new List<CallData>();
            int temptest = 0;
            try
            {
                foreach (var tempRowRaw in tempAllRowsList)
                {
                    DateTime dateTime;
                    var tempRow = tempRowRaw;
                    var tempRegex = new Regex(Regex.Escape(","));
                    int tempCountOfCommand = 0;


                    // Header overslaan
                    if (tempRow.ToLower().Contains("direction".ToLower()) || tempRow.ToLower().Contains("dir".ToLower()) || tempRow.ToLower().Contains("subscriber".ToLower()))
                        continue;

                    // Als regel footer is (met totaal opgeteld, skippen)
                    if (tempRow.ToLower().Contains("Total".ToLower()))
                        continue;

                    // Tekst die leeg is aanpassen naar null
                    if (tempRow.Contains(",,"))
                        tempRow = tempRow.Replace(",,", ",null,");

                    // Regel splitten op komma's en teksten in quotes respecteren
                    var tempSplitRow = Regex.Matches(tempRow, @"(?<="")[^""]+?(?=""(?:\s*?,|\s*?$))|(?<=(?:^|,)\s*?)(?:[^,""\s][^,""]*[^,""\s])|(?:[^,""\s])(?![^""]*?""(?:\s*?,|\s*?$))(?=\s*?(?:,|$))").Cast<Match>().Select(m => m.Value).ToArray();
                    if (tempSplitRow.Length == 0 || tempSplitRow.Length < 7)
                        continue;

                    // CallData invullen
                    CallData callData = new CallData();
                    if (!string.IsNullOrEmpty(tempSplitRow[0])) callData.Direction = tempSplitRow[0];
                    if (!string.IsNullOrEmpty(tempSplitRow[1])) callData.Bedrijf = tempSplitRow[1];
                    if (!string.IsNullOrEmpty(tempSplitRow[2])) callData.Subscriber = tempSplitRow[2];
                    if (!string.IsNullOrEmpty(tempSplitRow[3])) callData.Originator = tempSplitRow[3];
                    if (!string.IsNullOrEmpty(tempSplitRow[4])) callData.Target = tempSplitRow[4];
                    if (!string.IsNullOrEmpty(tempSplitRow[5]) && DateTime.TryParse(tempSplitRow[5], out dateTime)) callData.Date = dateTime;
                    if (!string.IsNullOrEmpty(tempSplitRow[6]) && !tempSplitRow[6].Contains("-"))
                    {
                        if (tempSplitRow[6].Contains(","))
                        {
                            foreach (char c in tempSplitRow[6])
                                if (c == ',') tempCountOfCommand++;
                            if (tempCountOfCommand > 1)
                                tempSplitRow[6] = tempRegex.Replace(tempSplitRow[6], "", 1);
                            if (!tempSplitRow[6].Contains("."))
                                tempSplitRow[6] = tempRegex.Replace(tempSplitRow[6], ".");
                        }
                        callData.Duration = double.Parse(tempSplitRow[6], CultureInfo.InvariantCulture);
                    }
                    else callData.Duration = 0;

                    if (!string.IsNullOrEmpty(tempSplitRow[7]))
                    {
                        if (!tempSplitRow[7].Contains("-"))
                            callData.Cost = double.Parse(tempSplitRow[7], CultureInfo.InvariantCulture);
                        else callData.Cost = 0;
                    }
                    tempParsedRowsList.Add(callData);
                }
            }
            catch (Exception ex)
            {
                //10045
                temptest++;
                Log.Error(ex.Message);
            }
            return tempParsedRowsList;
        }

        public static List<CallData> ParseAllRowsTypeB(List<string> tempAllRowsList)
        {
            List<CallData> tempParsedRowsList = new List<CallData>();
            try
            {
                foreach (var Row in tempAllRowsList)
                {
                    DateTime dateTime;
                    var tempRow = Row;

                    if (tempRow.Contains("direction") || tempRow.Contains("dir") || tempRow.Contains("subscriber"))
                        continue;
                    if (tempRow.Contains(",,"))
                        tempRow = tempRow.Replace(",,", ",null,");
                    var tempSplitRow = Regex.Matches(tempRow, @"(?<="")[^""]+?(?=""(?:\s*?,|\s*?$))|(?<=(?:^|,)\s*?)(?:[^,""\s][^,""]*[^,""\s])|(?:[^,""\s])(?![^""]*?""(?:\s*?,|\s*?$))(?=\s*?(?:,|$))").Cast<Match>().Select(m => m.Value).ToArray();
                    if (tempSplitRow.Length == 0 || tempSplitRow.Length < 6)
                        continue;

                    CallData callData = new CallData();
                    MonthlyCostData monthlyCostData = new MonthlyCostData();
                    if (!string.IsNullOrEmpty(tempSplitRow[0]))
                        callData.Bedrijf = tempSplitRow[0];
                    if (!string.IsNullOrEmpty(tempSplitRow[1]))
                    {
                        callData.Subscriber = tempSplitRow[1];
                        monthlyCostData.Subscriber = tempSplitRow[1];
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[2]))
                        callData.Originator = tempSplitRow[2];
                    if (!string.IsNullOrEmpty(tempSplitRow[3]))
                        callData.Target = tempSplitRow[3];
                    if (!string.IsNullOrEmpty(tempSplitRow[4]))
                        if (DateTime.TryParse(tempSplitRow[4], CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                            callData.Date = dateTime;
                    if (!string.IsNullOrEmpty(tempSplitRow[5]))
                    {
                        if (!tempSplitRow[5].Contains("-"))
                            callData.Duration = double.Parse(tempSplitRow[5], CultureInfo.InvariantCulture);
                        else callData.Duration = 0;
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[6]))
                    {
                        if (!tempSplitRow[6].Contains("-"))
                            callData.Cost = double.Parse(tempSplitRow[6], CultureInfo.InvariantCulture);
                        else callData.Cost = 0;
                        tempParsedRowsList.Add(callData);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return tempParsedRowsList;
        }


        public static List<CallData> ParseAllRowsTypeC(List<string> tempAllRowsList)
        {
            List<CallData> tempParsedRowsList = new List<CallData>();
            try
            {
                foreach (var Row in tempAllRowsList)
                {
                    DateTime dateTime;
                    var tempRow = Row;
                    string[] format = new string[] { "MM/dd/yyyy HH:mm" };

                    if (tempRow.ToLower().Contains("direction".ToLower()) || tempRow.ToLower().Contains("dir".ToLower()) || tempRow.ToLower().Contains("subscriber".ToLower()))
                        continue;
                    if (tempRow.Contains(",,"))
                        tempRow = tempRow.Replace(",,", ",null,");
                    var tempSplitRow = Regex.Matches(tempRow, @"(?<="")[^""]+?(?=""(?:\s*?,|\s*?$))|(?<=(?:^|,)\s*?)(?:[^,""\s][^,""]*[^,""\s])|(?:[^,""\s])(?![^""]*?""(?:\s*?,|\s*?$))(?=\s*?(?:,|$))").Cast<Match>().Select(m => m.Value).ToArray();
                    if (tempSplitRow.Length == 0 || tempSplitRow.Length < 7)
                        continue;

                    CallData callData = new CallData();
                    MonthlyCostData monthlyCostData = new MonthlyCostData();
                    if (!string.IsNullOrEmpty(tempSplitRow[2]))
                        callData.Direction = tempSplitRow[2];
                    if (!string.IsNullOrEmpty(tempSplitRow[4]))
                        callData.Bedrijf = tempSplitRow[4];
                    if (!string.IsNullOrEmpty(tempSplitRow[5]))
                    {
                        callData.Subscriber = tempSplitRow[5];
                        monthlyCostData.Subscriber = tempSplitRow[5];
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[6]))
                        callData.Originator = tempSplitRow[6];
                    if (!string.IsNullOrEmpty(tempSplitRow[7]))
                        callData.Target = tempSplitRow[7];
                    if (!string.IsNullOrEmpty(tempSplitRow[8]))
                        if (DateTime.TryParseExact(tempSplitRow[8], "M'/'d'/'yyyy H:m",
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None, out dateTime))
                            callData.Date = dateTime;
                    if (callData.Date.Month != 10)
                    {
                        callData.Date.AddDays(1);
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[9]))
                    {
                        if (!tempSplitRow[9].Contains("-"))
                            callData.Duration = double.Parse(tempSplitRow[9], CultureInfo.InvariantCulture);
                        else callData.Duration = 0;
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[10]))
                    {
                        if (!tempSplitRow[10].Contains("-"))
                            callData.Cost = double.Parse(tempSplitRow[10], CultureInfo.InvariantCulture);
                        else callData.Cost = 0;
                        tempParsedRowsList.Add(callData);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return tempParsedRowsList;
        }
        public static List<CallData> ParseAllRowsTypeD(List<string> tempAllRowsList)
        {
            List<CallData> tempParsedRowsList = new List<CallData>();
            try
            {
                foreach (var Row in tempAllRowsList)
                {
                    DateTime dateTime;
                    var tempRow = Row;
                    string[] format = new string[] { "MM/dd/yyyy HH:mm" };

                    if (tempRow.ToLower().Contains("enduser".ToLower()) || tempRow.ToLower().Contains("trunk".ToLower()))
                        continue;
                    if (tempRow.Contains(",,"))
                        tempRow = tempRow.Replace(",,", ",null,");
                    var tempSplitRow = Regex.Matches(tempRow, @"(?<="")[^""]+?(?=""(?:\s*?,|\s*?$))|(?<=(?:^|,)\s*?)(?:[^,""\s][^,""]*[^,""\s])|(?:[^,""\s])(?![^""]*?""(?:\s*?,|\s*?$))(?=\s*?(?:,|$))").Cast<Match>().Select(m => m.Value).ToArray();
                    if (tempSplitRow.Length == 0 || tempSplitRow.Length < 6)
                        continue;

                    CallData callData = new CallData();
                    MonthlyCostData monthlyCostData = new MonthlyCostData();
                    if (!string.IsNullOrEmpty(tempSplitRow[0]))
                        callData.Bedrijf = tempSplitRow[0];
                    if (!string.IsNullOrEmpty(tempSplitRow[1]))
                    {
                        callData.Subscriber = tempSplitRow[1];
                        monthlyCostData.Subscriber = tempSplitRow[1];
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[2]))
                        callData.Originator = tempSplitRow[2];
                    if (!string.IsNullOrEmpty(tempSplitRow[3]))
                        callData.Target = tempSplitRow[3];
                    if (!string.IsNullOrEmpty(tempSplitRow[4]))
                        if (DateTime.TryParseExact(tempSplitRow[4], "M'/'d'/'yyyy H:m",
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None, out dateTime))
                            callData.Date = dateTime;
                    if (!string.IsNullOrEmpty(tempSplitRow[5]))
                    {
                        if (!tempSplitRow[5].Contains("-"))
                            callData.Duration = double.Parse(tempSplitRow[5], CultureInfo.InvariantCulture);
                        else callData.Duration = 0;
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[6]))
                    {
                        if (!tempSplitRow[6].Contains("-"))
                            callData.Cost = double.Parse(tempSplitRow[6], CultureInfo.InvariantCulture);
                        else callData.Cost = 0;
                        tempParsedRowsList.Add(callData);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return tempParsedRowsList;
        }


        public static List<CallData> ParseAllRowsTypeE(List<string> tempAllRowsList)
        {
            List<CallData> tempParsedRowsList = new List<CallData>();
            try
            {
                foreach (var Row in tempAllRowsList)
                {
                    DateTime dateTime;
                    var tempRow = Row;

                    if (tempRow.ToLower().Contains("direction".ToLower()) || tempRow.ToLower().Contains("dir".ToLower()) || tempRow.ToLower().Contains("subscriber".ToLower()))
                        continue;
                    if (tempRow.Contains(",,"))
                        tempRow = tempRow.Replace(",,", ",null,");
                    var tempSplitRow = Regex.Matches(tempRow, @"(?<="")[^""]+?(?=""(?:\s*?,|\s*?$))|(?<=(?:^|,)\s*?)(?:[^,""\s][^,""]*[^,""\s])|(?:[^,""\s])(?![^""]*?""(?:\s*?,|\s*?$))(?=\s*?(?:,|$))").Cast<Match>().Select(m => m.Value).ToArray();
                    if (tempSplitRow.Length == 0 || tempSplitRow.Length < 7)
                        continue;

                    CallData callData = new CallData();
                    MonthlyCostData monthlyCostData = new MonthlyCostData();
                    if (!string.IsNullOrEmpty(tempSplitRow[0]))
                        callData.Direction = tempSplitRow[0];
                    if (!string.IsNullOrEmpty(tempSplitRow[2]))
                        callData.Bedrijf = tempSplitRow[2];
                    if (!string.IsNullOrEmpty(tempSplitRow[3]))
                    {
                        callData.Subscriber = tempSplitRow[3];
                        monthlyCostData.Subscriber = tempSplitRow[3];
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[4]))
                        callData.Originator = tempSplitRow[4];
                    if (!string.IsNullOrEmpty(tempSplitRow[5]))
                        callData.Target = tempSplitRow[5];
                    if (!string.IsNullOrEmpty(tempSplitRow[6]))
                        if (DateTime.TryParseExact(tempSplitRow[6], "M'/'d'/'yyyy H:m",
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None, out dateTime))
                            callData.Date = dateTime;
                    if (!string.IsNullOrEmpty(tempSplitRow[7]))
                    {
                        if (!tempSplitRow[7].Contains("-"))
                            callData.Duration = double.Parse(tempSplitRow[7], CultureInfo.InvariantCulture);
                        else callData.Duration = 0;
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[8]))
                    {
                        if (!tempSplitRow[8].Contains("-"))
                            callData.Cost = double.Parse(tempSplitRow[8], CultureInfo.InvariantCulture);
                        else callData.Cost = 0;
                        tempParsedRowsList.Add(callData);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return tempParsedRowsList;
        }
        public static List<CallData> ParseAllRowsTypeF(List<string> tempAllRowsList)
        {
            List<CallData> tempParsedRowsList = new List<CallData>();
            List<CallData> tempTargetList = new List<CallData>();
            List<CompanyMargesType> tempNewAndSavedCompaniesList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempNeedsToBeAddedToDataFileList = new List<CompanyMargesType>();

            int test = 0;
            try
            {
                foreach (var Row in tempAllRowsList)
                {
                    CallData callData = new CallData();
                    MonthlyCostData monthlyCostData = new MonthlyCostData();
                    DateTime dateTimeForDate;
                    TimeSpan dateTimeForDuration;
                    double doubleForDuration;
                    var tempRow = Row;

                    if (tempRow.ToLower().Contains("Timestamp".ToLower()) || tempRow.ToLower().Contains("cli".ToLower()))
                        continue;

                    if (tempRow.Contains(", ,"))
                        tempRow = tempRow.Replace(", ,", ",null,");

                    var tempSplitRow = Regex.Matches(tempRow, @"(?<="")[^""]+?(?=""(?:\s*?,|\s*?$))|(?<=(?:^|,)\s*?)(?:[^,""\s][^,""]*[^,""\s])|(?:[^,""\s])(?![^""]*?""(?:\s*?,|\s*?$))(?=\s*?(?:,|$))").Cast<Match>().Select(m => m.Value).ToArray();

                    if (tempSplitRow.Length == 0 || tempSplitRow.Length < 7)
                        continue;

                    if (!string.IsNullOrEmpty(tempSplitRow[1]))
                        callData.Bedrijf = tempSplitRow[1];
                    if (!string.IsNullOrEmpty(tempSplitRow[1]))
                    {
                        callData.Subscriber = tempSplitRow[1];
                        monthlyCostData.Subscriber = tempSplitRow[1];
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[2]))
                        callData.Target = tempSplitRow[2];
                    if (!string.IsNullOrEmpty(tempSplitRow[0]) && DateTime.TryParse(tempSplitRow[0], out dateTimeForDate)) callData.Date = dateTimeForDate;

                    if (!string.IsNullOrEmpty(tempSplitRow[5]))
                    {
                        test++;

                        TimeSpan.TryParseExact(tempSplitRow[5], "g",
                        CultureInfo.InvariantCulture,
                           TimeSpanStyles.AssumeNegative, out dateTimeForDuration);
                        if (!tempSplitRow[5].Contains("-")) ;// tijd omzetten in seconden
                          //tempCompanyMargesType.Duration = double.Parse(doubleForDuration.ToString(), CultureInfo.InvariantCulture);
                        else callData.Duration = 0;
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[7]))
                    {
                        if (!tempSplitRow[7].Contains("-"))
                            callData.Cost = double.Parse(tempSplitRow[7], CultureInfo.InvariantCulture);
                        else callData.Cost = 0;
                        tempParsedRowsList.Add(callData);
                    }
                }
                
                var tempSavedDataList = DataManager.DataManager.ReadCompanyDataFile();
                if (tempSavedDataList != null)
                {
                    foreach (var tempParsedRow in tempParsedRowsList)
                    {
                        var tempFound = tempSavedDataList.Find(x => x.CompanyName == tempParsedRow.Target);
                        if (tempFound != null)
                        {
                            tempParsedRow.ID = tempFound.CompanyId;
                            if (tempParsedRow.ID == null)
                            {
                                var tempFoundTargetList = tempSavedDataList.FindAll(x => x.CompanyName == tempParsedRow.Target);
                                if (tempFoundTargetList != null)
                                {
                                    foreach (var tempTarget in tempFoundTargetList)
                                    {
                                        if (tempTarget.CompanyName.StartsWith("318") || tempTarget.CompanyName.StartsWith("319") || tempTarget.CompanyId == null)
                                            continue;
                                         tempParsedRow.ID = tempTarget.CompanyId;
                                        if (tempParsedRow.ID != null)
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    List<CallData> tempAllCompaniesList = tempParsedRowsList
                     .GroupBy(p => p.Bedrijf)
                      .Select(g => g.First())
                       .ToList();

                    foreach (var tempCompany in tempAllCompaniesList)
                    {
                        var tempFoundCompany = tempSavedDataList.Find(x => x.CompanyName == tempCompany.Bedrijf);
                        if (tempFoundCompany != null)
                        {
                            // object bestaat al
                            tempNewAndSavedCompaniesList.Add(tempFoundCompany);
                        }
                        else
                        {
                            //object bestaat nog niet
                            tempNeedsToBeAddedToDataFileList.Add(new CompanyMargesType() { CompanyName = tempCompany.Bedrijf, CompanyId = tempCompany.ID});
                            tempNewAndSavedCompaniesList.Add(new CompanyMargesType() { CompanyName = tempCompany.Bedrijf, CompanyId = tempCompany.ID });
                        }
                    }

                    tempSavedDataList.AddRange(tempNeedsToBeAddedToDataFileList);
                    DataManager.DataManager.WriteCompanyDataToFile(tempSavedDataList);
                
                }
            }
            catch (Exception ex)
            {
                Log.Error(test.ToString());
            }
            return tempParsedRowsList;
        }

        public static List<CompanyMargesType> ParseAllRowsTypeData(List<string> tempAllRowsList)
        {
            List<CompanyMargesType> tempParsedRowsList = new List<CompanyMargesType>();
            try
            {
                foreach (var Row in tempAllRowsList)
                {
                    DateTime dateTime;
                    var tempRow = Row;

                    if (tempRow.ToLower().Contains("voip".ToLower()) || tempRow.ToLower().Contains("voip systeem".ToLower()) || tempRow.ToLower().Contains("naam klant".ToLower()))
                        continue;

                    if (tempRow.Contains(",,"))
                        tempRow = tempRow.Replace(",,", ",null,");

                    var tempSplitRow = Regex.Matches(tempRow, @"(?<="")[^""]+?(?=""(?:\s*?,|\s*?$))|(?<=(?:^|,)\s*?)(?:[^,""\s][^,""]*[^,""\s])|(?:[^,""\s])(?![^""]*?""(?:\s*?,|\s*?$))(?=\s*?(?:,|$))").Cast<Match>().Select(m => m.Value).ToArray();
                    if (tempSplitRow.Length == 0 || tempSplitRow.Length < 7)
                        continue;

                    CompanyMargesType tempCompanyMargesType = new CompanyMargesType();
                    if (!string.IsNullOrEmpty(tempSplitRow[0]))
                        if (double.TryParse(tempSplitRow[5], out var tempVoorschot))
                        {
                            tempCompanyMargesType.CompanyVoorschot = tempVoorschot;
                            if (!string.IsNullOrEmpty(tempSplitRow[0]))
                                tempCompanyMargesType.CompanyId = tempSplitRow[0];
                            if (!string.IsNullOrEmpty(tempSplitRow[1]))
                                tempCompanyMargesType.CompanyName = tempSplitRow[1];

                            tempParsedRowsList.Add(tempCompanyMargesType);
                        }
                        else if (!tempSplitRow[5].Contains("null"))
                        {
                            Log.Error($"Van ID {tempSplitRow[0]}, Bedrijf {tempSplitRow[1]} is de voorschot ({tempSplitRow[5]}) onleesbaar ");
                        }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return tempParsedRowsList;
        }

        public static List<CallData> ParseAllRowsTypeG(List<string> tempAllRowsList)
        {
            List<CallData> tempParsedRowsList = new List<CallData>();
            try
            {
                foreach (var Row in tempAllRowsList)
                {
                    DateTime dateTime;
                    var tempRow = Row;

                    if (tempRow.ToLower().Contains("traffic".ToLower()) || tempRow.ToLower().Contains("enduser".ToLower()))
                        continue;

                    if (tempRow.Contains(",,"))
                        tempRow = tempRow.Replace(",,", ",null,");

                    var tempSplitRow = Regex.Matches(tempRow, @"(?<="")[^""]+?(?=""(?:\s*?,|\s*?$))|(?<=(?:^|,)\s*?)(?:[^,""\s][^,""]*[^,""\s])|(?:[^,""\s])(?![^""]*?""(?:\s*?,|\s*?$))(?=\s*?(?:,|$))").Cast<Match>().Select(m => m.Value).ToArray();
                    if (tempSplitRow.Length == 0 || tempSplitRow.Length < 7)
                        continue;

                    CallData callData = new CallData();
                    MonthlyCostData monthlyCostData = new MonthlyCostData();
                    if (!string.IsNullOrEmpty(tempSplitRow[0]))
                        callData.Direction = tempSplitRow[0];
                    if (!string.IsNullOrEmpty(tempSplitRow[1]))
                        callData.Bedrijf = tempSplitRow[1];
                    if (!string.IsNullOrEmpty(tempSplitRow[2]))
                    {
                        callData.Subscriber = tempSplitRow[2];
                        monthlyCostData.Subscriber = tempSplitRow[2];
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[3]))
                        callData.Originator = tempSplitRow[3];
                    if (!string.IsNullOrEmpty(tempSplitRow[4]))
                        callData.Target = tempSplitRow[4];

                    if (!string.IsNullOrEmpty(tempSplitRow[5]))
                        if (DateTime.TryParseExact(tempSplitRow[5], "d'/'M'/'yy H:m",
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None, out dateTime))
                            callData.Date = dateTime;
                    if (!string.IsNullOrEmpty(tempSplitRow[6]))
                    {
                        if (!tempSplitRow[6].Contains("-"))
                            callData.Duration = double.Parse(tempSplitRow[6], CultureInfo.InvariantCulture);
                        else callData.Duration = 0;
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[7]))
                    {
                        if (!tempSplitRow[7].Contains("-"))
                            callData.Cost = double.Parse(tempSplitRow[7], CultureInfo.InvariantCulture);
                        else callData.Cost = 0;
                    }
                    tempParsedRowsList.Add(callData);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return tempParsedRowsList;
        }
        public static List<CallData> ParseAllRowsTypeH(List<string> tempAllRowsList)
        {
            List<CallData> tempParsedRowsList = new List<CallData>();
            try
            {
                DateTime tempMonth = new DateTime();
                foreach (var Row in tempAllRowsList)
                {
                    var tempRow = Row;

                    if (tempRow.ToLower().Contains("Verbruik van".ToLower()))
                    {
                        var tempMonthOutOfText = LogicManager.GetMonthOutOfText(tempRow);
                        tempMonth = tempMonthOutOfText;

                        tempMonth = tempMonth.AddYears(DateTime.Now.Year - 1);
                        tempMonth = tempMonth.AddDays(22);
                        tempMonth = tempMonth.AddMinutes(22);
                        continue;
                    }

                    if (tempRow.Contains(",,"))
                        tempRow = tempRow.Replace(",,", ",null,");

                    var tempSplitRow = Regex.Matches(tempRow, @"(?<="")[^""]+?(?=""(?:\s*?,|\s*?$))|(?<=(?:^|,)\s*?)(?:[^,""\s][^,""]*[^,""\s])|(?:[^,""\s])(?![^""]*?""(?:\s*?,|\s*?$))(?=\s*?(?:,|$))").Cast<Match>().Select(m => m.Value).ToArray();
                    if (tempSplitRow.Length == 0)
                        continue;

                    CallData callData = new CallData();
                    MonthlyCostData monthlyCostData = new MonthlyCostData();
                    if (!string.IsNullOrEmpty(tempSplitRow[0]))
                    {
                        callData.Bedrijf = tempSplitRow[0];
                    }
                    if (!string.IsNullOrEmpty(tempSplitRow[0]))
                            callData.Date = tempMonth;
                    if (!string.IsNullOrEmpty(tempSplitRow[1]))
                    {
                        if (!tempSplitRow[1].Contains("-"))
                            callData.Cost = double.Parse(tempSplitRow[1], CultureInfo.InvariantCulture);
                        else callData.Cost = 0;
                    }
                    tempParsedRowsList.Add(callData);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return tempParsedRowsList;
        }
    }
}
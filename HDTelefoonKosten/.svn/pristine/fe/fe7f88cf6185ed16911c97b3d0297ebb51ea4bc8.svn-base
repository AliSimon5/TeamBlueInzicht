using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;
using System.IO;
using HDTelefoonKosten.Types;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;

namespace HDTelefoonKosten.DataManager
{
    internal partial class DataManager
    {
        /// <summary>
        /// Opent Exceldocument en converteert deze naar tijdelijke CSV, vervolgens wordt deze ingelezen en als een Lijst met regels (strings) gereturned
        /// </summary>
        /// <param name="tempFilePath"></param>
        /// <param name="argSheet"></param>
        /// <returns></returns>
        public static List<string> ReadExcelFileAndFindUsableWorkSheet(string tempFilePath, int argSheet)
        {
            ExecuteCommand("taskkill", "/f /im excel.exe");

            string tempWorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var tempCsvPath = Path.Combine(tempWorkingDirectory, "tempCsvFile.csv");

            if (File.Exists(tempCsvPath))
                File.Delete(tempCsvPath);

            List<string> tempAllRowList = new List<string>();

            string tempExcelPath = tempFilePath;

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(tempExcelPath);

            int tempSheetCount = 0;
            List<string> tempAllCreatedCSVFilesList = new List<string>();
            foreach (Excel.Worksheet tempWorkSheet in xlWorkbook.Worksheets)
            {
                var tempCsvPathSheet = Path.Combine(tempWorkingDirectory, $"tempCsvFile{tempSheetCount}.csv");
                tempWorkSheet.SaveAs(tempCsvPathSheet, XlFileFormat.xlCSV, true);
                tempAllCreatedCSVFilesList.Add(tempCsvPathSheet);

                tempSheetCount++;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            xlWorkbook.Close(false, Type.Missing, Type.Missing);


            // Juiste CSV Sheet opzoeken
            for (int i = 0; i < tempSheetCount; i++)
            {
                var tempCsvPathSheets = Path.Combine(tempWorkingDirectory, $"tempCsvFile{i}.csv");
                var tempAllLinesFromCSVSheet = File.ReadAllLines(tempCsvPathSheets, System.Text.Encoding.GetEncoding("iso-8859-1"));

                if (tempAllLinesFromCSVSheet.Length > 10000) // <- 10000 nog variabel maken in settings file
                {
                    foreach (var line in tempAllLinesFromCSVSheet)
                    {
                        tempAllRowList.Add(line);
                    }

                    break;
                }
                else if (tempSheetCount == 1)
                {
                    foreach (var line in tempAllLinesFromCSVSheet)
                    {
                        tempAllRowList.Add(line);
                    }
                    break;
                }
                else if (tempAllLinesFromCSVSheet[0].ToLower().Contains("Voip".ToLower()))
                {
                    foreach (var line in tempAllLinesFromCSVSheet)
                    {
                        tempAllRowList.Add(line);
                    }
                    break;
                }
            }

            // Verwijderen CSV bestanden
            foreach (var file in tempAllCreatedCSVFilesList)
            {
                File.Delete(file);
            }

            // Excel afsluiten
            ExecuteCommand("taskkill", "/f /im excel.exe");

            return tempAllRowList;
        }

        public static List<CompanyMargesType> ReadCompanyDataFile()
        {
            try
            {
                var tempSavedCompanyMarges = GetSavedCompanyDataPath();

                var tempJSON = File.ReadAllText(tempSavedCompanyMarges);
                var tempListCompanyMargesType = JsonConvert.DeserializeObject<List<CompanyMargesType>>(tempJSON);

                return tempListCompanyMargesType;
            }
            catch (Exception ex)
            {
                return new List<CompanyMargesType>();
            }
        }
        public static void WriteCompanyDataToFile(List<CompanyMargesType> argListCompanyMarginType)
        {
            var tempSavedCompanyMarges = GetSavedCompanyDataPath();

            var tempJSON = JsonConvert.SerializeObject(argListCompanyMarginType, Formatting.Indented);

            File.WriteAllText(tempSavedCompanyMarges, tempJSON);
        }
        public static void ClearCompanyMargeFile()
        {
            var tempSavedCompanyMarges = GetSavedCompanyDataPath();
            File.WriteAllText(tempSavedCompanyMarges, "");
        }
        public static string GetSavedCompanyDataPath()
        {
            var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var tempSavedCompanyMarges = Path.Combine(startupPath, "HDTelefoonKosten.SavedCompanyData.json");
            return tempSavedCompanyMarges;
        }
    }
}

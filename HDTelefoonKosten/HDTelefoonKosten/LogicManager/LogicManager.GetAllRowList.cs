using HDTelefoonKosten.Types;
using M.Core.Application.WPF.MessageBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTelefoonKosten.LogicManager
{
    internal partial class LogicManager
    {
        public static List<CallData> GetAllParsedRowsList(string tempFilePath)
        {
            List<string> tempAllRowList = DataManager.DataManager.ReadExcelFileAndFindUsableWorkSheet(tempFilePath, 1);

            if (tempAllRowList == null) throw new Exception($"File: {tempFilePath} Niet kunnen lezen");

            return ParseAllRowsFromFile(tempAllRowList);
        }
        public static List<CompanyMargesType> GetAllParsedDataRowsList(string tempFilePath)
        {
            List<string> tempAllRowList = DataManager.DataManager.ReadExcelFileAndFindUsableWorkSheet(tempFilePath, 1);

            if (tempAllRowList == null) throw new Exception($"File: {tempFilePath} Niet kunnen lezen");

            return ParseAllDataRowsFromFile(tempAllRowList);
        }
    }
}

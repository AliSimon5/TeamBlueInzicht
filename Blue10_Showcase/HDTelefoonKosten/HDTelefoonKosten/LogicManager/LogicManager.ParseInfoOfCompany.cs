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
        public static string ParseCompanyTimeToReadableText(TimeSpan argCompanyCallTime)
        {
            var tempTotalDays = argCompanyCallTime.Days;
            var tempHours = argCompanyCallTime.Hours;
            var tempMinutes = argCompanyCallTime.Minutes;
            var tempSeconds = argCompanyCallTime.Seconds;
            return $"{tempTotalDays}d {tempHours.ToString("00")}:{tempMinutes.ToString("00")}:{tempSeconds.ToString("00")}";
        }

        public static string ParseCompanyAmountToReadableText(double argCompanyTotalAmount, bool argMultipleDecimals)
        {
            if (argMultipleDecimals)
            {
                string tempResult = String.Format("€ {0:N3}", argCompanyTotalAmount);
                if (tempResult == null || tempResult == "€ 0,000")
                    return null;
                return tempResult;
            }
            if (!argMultipleDecimals)
            {
                string tempResult = String.Format("€ {0:N2}", argCompanyTotalAmount);
                if (tempResult == null || tempResult == "€ 0,00")
                    return null;
                return tempResult;
            }
            return null;
        }
        public static string ParseMonthNumberToText(string argMonthNumber)
        {
            string tempFinalResult = string.Empty;
            if (argMonthNumber == "1")
                tempFinalResult = "Januari";
            if (argMonthNumber == "2")
                tempFinalResult = "Februari";
            if (argMonthNumber == "3")
                tempFinalResult = "Maart";
            if (argMonthNumber == "4")
                tempFinalResult = "April";
            if (argMonthNumber == "5")
                tempFinalResult = "Mei";
            if (argMonthNumber == "6")
                tempFinalResult = "Juni";
            if (argMonthNumber == "7")
                tempFinalResult = "Juli";
            if (argMonthNumber == "8")
                tempFinalResult = "Augustus";
            if (argMonthNumber == "9")
                tempFinalResult = "September";
            if (argMonthNumber == "10")
                tempFinalResult = "Oktober";
            if (argMonthNumber == "11")
                tempFinalResult = "November";
            if (argMonthNumber == "12")
                tempFinalResult = "December";

            return tempFinalResult;
        }
        public static DateTime GetMonthOutOfText(string argMonthText)
        {
            DateTime tempFinalResult = new DateTime();
            if (argMonthText.Contains("jan"))
                tempFinalResult = tempFinalResult.AddMonths(0); 
            if (argMonthText.Contains("Feb"))
                tempFinalResult = tempFinalResult.AddMonths(1);
            if (argMonthText.Contains("Maa") || argMonthText.Contains("Maart") || argMonthText.Contains("Mar"))
                tempFinalResult = tempFinalResult.AddMonths(2);
            if (argMonthText.Contains("Apr") || argMonthText.Contains("April"))
                tempFinalResult = tempFinalResult.AddMonths(3);
            if (argMonthText.Contains("Mei"))
                tempFinalResult = tempFinalResult.AddMonths(4);
            if (argMonthText.Contains("Juni") || argMonthText.Contains("Jun"))
                tempFinalResult = tempFinalResult.AddMonths(5);
            if (argMonthText.Contains("Juli") || argMonthText.Contains("Jul"))
                tempFinalResult = tempFinalResult.AddMonths(6);
            if (argMonthText.Contains("Aug"))
                tempFinalResult = tempFinalResult.AddMonths(7);
            if (argMonthText.Contains("Sep"))
                tempFinalResult = tempFinalResult.AddMonths(8);
            if (argMonthText.Contains("Okt"))
                tempFinalResult = tempFinalResult.AddMonths(9);
            if (argMonthText.Contains("Nov"))
                tempFinalResult = tempFinalResult.AddMonths(10);
            if (argMonthText.Contains("Dec"))
                tempFinalResult = tempFinalResult.AddMonths(11);
            if (tempFinalResult.Month == 0)
                MBox.ShowWarning($"Nucall Maand is niet goed aangegeven {argMonthText}");

            return tempFinalResult;
        }
    }
}

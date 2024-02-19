using HDTelefoonKosten.Core;
using HDTelefoonKosten.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTelefoonKosten.LogicManager
{
    internal partial class LogicManager
    {
        public static string GetCompanyName(List<CallData> argList)
        {
            foreach (var item in argList)
            {
                return item.Bedrijf.ToString();
            }
            return null;
        }
        public static string GetTotalCalls(List<CallData> argList)
        {
            int TotalAmount = 0;
            foreach (var item in argList)
                TotalAmount++;

            return TotalAmount.ToString();
        }
        public static string CountIns(List<CallData> argList)
        {
            int TotalAmount = 0;
            foreach (var item in argList)
                if (item.Direction == "in")
                    TotalAmount++;

            return TotalAmount.ToString();
        }
        public static string CountOuts(List<CallData> argList)
        {
            int TotalAmount = 0;
            foreach (var item in argList)
                if (item.Direction == "out")
                    TotalAmount++;

            return TotalAmount.ToString();
        }
        public static TimeSpan GetCallDataTotalCallTime(List<CallData> argList)
        {
            double TotalAmount = 0;
            double tempParsedAmount = 0;

            foreach (var item in argList)
            {
                if (!item.Duration.ToString().Contains(",") && !item.Duration.ToString().Contains("."))
                {
                    if (item.Duration > 1000)
                        tempParsedAmount = item.Duration / 1000;

                    TotalAmount += tempParsedAmount;
                }
                else
                    TotalAmount += item.Duration;
            }

            TimeSpan timeSpan = TimeSpan.FromSeconds(TotalAmount);

            return timeSpan;
        }
        public static double GetCallDataTotalCost(List<CallData> argList)
        {
            double TotalAmount = 0;
            foreach (var item in argList)
                TotalAmount += item.Cost;

            return TotalAmount;
        }
        public static double GetCompanyCostPerMinute(List<CallData> argList)
        {
            double TotalAmount = 0;
            double TotalCallTime = 0;
            foreach (var item in argList)
            {
                if (item.Direction == "out")
                {
                    TotalAmount += item.Cost;
                    TotalCallTime += item.Duration;
                }
            }
            TimeSpan timeSpan = TimeSpan.FromSeconds(TotalCallTime);
            var tempMinutes = timeSpan.TotalMinutes;
            double CostPerMinute = TotalAmount / tempMinutes;
            return CostPerMinute;
        }
        public static string GetCallDataFirstDate(List<CallData> argCallDataList)
        {
            var tempDateList = argCallDataList.Select(x => x.Date).ToList();
            
            var tempFirstDate = tempDateList.Min(x =>
            {
                if (x != DateTime.Parse("1-1-0001 00:00:00")) return x;
                else return DateTime.MaxValue;
            });

            if (tempFirstDate == DateTime.MaxValue) return string.Empty;
            return tempFirstDate.ToString();
        }
        public static string GetCompanyLastDate(List<CallData> argList)
        {
            var tempDateList = argList.Select(x => x.Date).ToList();

            var tempFirstDate = tempDateList.Max(x =>
            {
                if (x != DateTime.Parse("1-1-0001 00:00:00")) return x;
                else return DateTime.MinValue;
            });

            if (tempFirstDate == DateTime.MinValue) return string.Empty;

            return tempFirstDate.ToString();

        }
        public static TimeSpan GetCompanyTimePeriod(List<CallData> argList)
        {
            List<DateTime> tempDateList = new List<DateTime>();
            foreach (var item in argList)
            {
                tempDateList.Add(item.Date);
            }
            int skipCount = 0;
            DateTime FirstDate = DateTime.Parse("1-1-0001 00:00:00");
            foreach (DateTime dateTime in tempDateList)
            {
                DateTime tempFirstDate = tempDateList.OrderBy(a => a.Date).Skip(skipCount).First();
                if (tempFirstDate == DateTime.Parse("1-1-0001 00:00:00"))
                    skipCount++;
                else
                {
                    FirstDate = tempFirstDate;
                    break;
                }
            }
            if (FirstDate == DateTime.Parse("1-1-0001 00:00:00"))
                return TimeSpan.Parse("00:00:00");
            DateTime LastDate = tempDateList.OrderBy(a => a.Date).Last();
            TimeSpan tempTimeBetween = LastDate.Subtract(FirstDate);

            var tempTotalDays = Math.Floor(tempTimeBetween.TotalDays);
            var tempHours = tempTimeBetween.Hours;
            var tempMinutes = tempTimeBetween.Minutes;
            var tempSeconds = tempTimeBetween.Seconds;

            return tempTimeBetween;
        }

        public static SortedPhoneCalls GetCompanyCallRates(List<CallData> argCompanyCallsList)
        {
            var tempSortedPhoneCalls = new SortedPhoneCalls();

            foreach (var tempCall in argCompanyCallsList)
            {
                if (tempCall.Direction == "out")
                {
                    if (tempCall.Target.StartsWith("31") || tempCall.Target.StartsWith("0031"))
                    {
                        // Mobiel nummer
                        if (tempCall.Target.StartsWith("316") || tempCall.Target.StartsWith("00316"))
                        {
                            tempSortedPhoneCalls.MobielNummers.Add(tempCall);
                            continue;
                        }
                        // Kantoor nummer
                        if (tempCall.Target.StartsWith("313") || tempCall.Target.StartsWith("00313"))
                        {
                            tempSortedPhoneCalls.KantoorNummers.Add(tempCall);
                            continue;
                        }
                        // 0900 nummer
                        else if (tempCall.Target.StartsWith("319") || tempCall.Target.StartsWith("00319"))
                        {
                            tempSortedPhoneCalls.BetaaldNummers.Add(tempCall);
                            continue;
                        }
                        // 0800 nummer
                        else if (tempCall.Target.StartsWith("318") || tempCall.Target.StartsWith("00318"))
                        {
                            tempSortedPhoneCalls.GratisNummers.Add(tempCall);
                            continue;
                        }
                        //Binnenlands nummer
                        else
                        {
                            tempSortedPhoneCalls.BinnenLandNummers.Add(tempCall);
                            continue;
                        }
                    }
                    //Buitelands nummer
                    else
                    {
                        tempSortedPhoneCalls.BuitlandNummers.Add(tempCall);
                        continue;
                    }
                }
            }

            var tempHighestMobile = GetHighestNumber(tempSortedPhoneCalls.MobielNummers);
            var tempBinnenland = GetHighestNumber(tempSortedPhoneCalls.BinnenLandNummers);

            if (tempHighestMobile != null)
            {
                try
                {
                    // Functie aanroepen om beltarieven uit te reken.
                    tempSortedPhoneCalls.belTariefMobiel = BelTarievenManager.CalculateTarieven(tempHighestMobile[0].Duration, tempHighestMobile[0].Cost, tempHighestMobile[1].Duration, tempHighestMobile[1].Cost);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }
            if (tempBinnenland != null)
            {
                try
                {
                    // Functie aanroepen om beltarieven uit te reken.
                    tempSortedPhoneCalls.belTariefBinnenland = BelTarievenManager.CalculateTarieven(tempBinnenland[0].Duration, tempBinnenland[0].Cost, tempBinnenland[1].Duration, tempBinnenland[1].Cost);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }

            }

            return tempSortedPhoneCalls;
        }

        private static List<CallData> GetHighestNumber(List<CallData> callDatasList)
        {
            if (callDatasList == null || callDatasList.Count < 2)
                return null;

            var tempDescendedCallList = callDatasList.OrderByDescending(x => x.Cost).ToList();

            return tempDescendedCallList.GetRange(0, 2);
        }
    }
}

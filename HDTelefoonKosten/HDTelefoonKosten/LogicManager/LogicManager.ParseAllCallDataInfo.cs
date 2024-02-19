using HDTelefoonKosten.Types;
using M.Core.Application.WPF.MessageBox;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HDTelefoonKosten.LogicManager
{
    internal partial class LogicManager
    {

        /// <summary>
        /// Tel alle geselecteerde calldata op en maak er een MonthlyCostData object van;
        /// </summary>
        /// <param name="tempSelectedCompanySubscriberCallData"></param>
        /// <param name="argMarge"></param>
        /// <param name="argVoorschot"></param>
        /// <returns></returns>
        public async static Task<MonthlyCostData> ParseAllCallData(List<CallData> tempSelectedCompanySubscriberCallData, double argMarge, double argVoorschot)
        {
            return await Task.Run(() =>
            {
                MonthlyCostData monthlyCostData = new MonthlyCostData();

                string tempCompanyName = string.Empty;
                string tempCompanyTotalCalls = string.Empty;
                string tempCompanyIns = string.Empty;
                string tempCompanyOuts = string.Empty;
                TimeSpan tempCompanyTotalCallTime = TimeSpan.Parse("00:00:00");
                double tempCompanyTotalAmount = 0;
                double tempCompanyVoorschot = 0;
                double tempCompanyTekortVoorschot = 0;
                string tempCompanyFirstDate = string.Empty;
                string tempCompanyLastDate = string.Empty;
                DateTime tempYearDate;
                TimeSpan tempCompanyTimePeriod = TimeSpan.Parse("00:00:00");
                double tempCompanyCostPerMinute = 0;
                SortedPhoneCalls tempCompanyCallRates = new SortedPhoneCalls();

                string tempCompanyParsedCallTime = string.Empty;
                string tempCompanyParsedStartRateBinnenLand = string.Empty;
                string tempCompanyParsedStartRateMobiel = string.Empty;
                string tempCompanyParsedTotalAmount = string.Empty;
                string tempCompanyParsedTimePeriod = string.Empty;
                string tempCompanyParsedCostPerMinute = string.Empty;

                string tempCompanyParsedVoorschot = string.Empty;
                string tempCompanyParsedTekortVoorschot = string.Empty;


                tempCompanyTotalCalls = LogicManager.GetTotalCalls(tempSelectedCompanySubscriberCallData);
                tempCompanyIns = LogicManager.CountIns(tempSelectedCompanySubscriberCallData);
                tempCompanyOuts = LogicManager.CountOuts(tempSelectedCompanySubscriberCallData);
                tempCompanyTotalCallTime = LogicManager.GetCallDataTotalCallTime(tempSelectedCompanySubscriberCallData);
                tempCompanyTotalAmount = LogicManager.GetCallDataTotalCost(tempSelectedCompanySubscriberCallData);
                tempCompanyFirstDate = LogicManager.GetCallDataFirstDate(tempSelectedCompanySubscriberCallData);

                tempCompanyLastDate = LogicManager.GetCompanyLastDate(tempSelectedCompanySubscriberCallData);
                tempCompanyTimePeriod = LogicManager.GetCompanyTimePeriod(tempSelectedCompanySubscriberCallData);
                tempCompanyCostPerMinute = LogicManager.GetCompanyCostPerMinute(tempSelectedCompanySubscriberCallData);
                tempCompanyName = LogicManager.GetCompanyName(tempSelectedCompanySubscriberCallData);
                tempCompanyCallRates = LogicManager.GetCompanyCallRates(tempSelectedCompanySubscriberCallData);


                if (DateTime.TryParse(tempCompanyFirstDate, out tempYearDate))
                    monthlyCostData.YearOrMonth = tempYearDate.Year.ToString();

                if (argMarge > 0)
                {
                    tempCompanyCostPerMinute = tempCompanyCostPerMinute * ((argMarge + 100) / 100);
                    tempCompanyTotalAmount = tempCompanyTotalAmount * ((argMarge + 100) / 100);
                    tempCompanyCallRates.belTariefMobiel.dMinuutTarief = tempCompanyCallRates.belTariefMobiel.dMinuutTarief * ((argMarge + 100) / 100);
                    tempCompanyCallRates.belTariefMobiel.dStartTarief = tempCompanyCallRates.belTariefMobiel.dStartTarief * ((argMarge + 100) / 100);
                    tempCompanyCallRates.belTariefBinnenland.dStartTarief = tempCompanyCallRates.belTariefBinnenland.dStartTarief * ((argMarge + 100) / 100);
                    tempCompanyCallRates.belTariefBinnenland.dMinuutTarief = tempCompanyCallRates.belTariefBinnenland.dMinuutTarief * ((argMarge + 100) / 100);
                }
                if (argVoorschot > 0)
                {
                    tempCompanyVoorschot = argVoorschot;
                    if (tempCompanyTotalAmount > tempCompanyVoorschot)
                    {
                        tempCompanyTekortVoorschot = tempCompanyVoorschot - tempCompanyTotalAmount;
                        Log.Information($"\n{tempCompanyName}\nis over de voorschot van {LogicManager.ParseCompanyAmountToReadableText(tempCompanyVoorschot, false)} gegaan.\nEr moet {LogicManager.ParseCompanyAmountToReadableText(tempCompanyTekortVoorschot, false).Replace("-","")} betaald worden!\n");
                    }
                }

                tempCompanyParsedCallTime = LogicManager.ParseCompanyTimeToReadableText(tempCompanyTotalCallTime);
                tempCompanyParsedTimePeriod = LogicManager.ParseCompanyTimeToReadableText(tempCompanyTimePeriod);
                tempCompanyParsedTotalAmount = LogicManager.ParseCompanyAmountToReadableText(tempCompanyTotalAmount, false);
                tempCompanyParsedVoorschot = LogicManager.ParseCompanyAmountToReadableText(tempCompanyVoorschot, false);
                tempCompanyParsedTekortVoorschot = LogicManager.ParseCompanyAmountToReadableText(tempCompanyTekortVoorschot, false);

                if (tempCompanyCallRates.belTariefMobiel.dMinuutTarief != null && tempCompanyCallRates.belTariefMobiel.dMinuutTarief != 0)
                    tempCompanyParsedCostPerMinute = LogicManager.ParseCompanyAmountToReadableText(tempCompanyCallRates.belTariefMobiel.dMinuutTarief, true);
                else tempCompanyParsedCostPerMinute = LogicManager.ParseCompanyAmountToReadableText(tempCompanyCostPerMinute, true);

                if (tempCompanyCallRates.belTariefMobiel.dStartTarief != null && tempCompanyCallRates.belTariefMobiel.dStartTarief != 0)
                    tempCompanyParsedCostPerMinute = LogicManager.ParseCompanyAmountToReadableText(tempCompanyCallRates.belTariefMobiel.dStartTarief, true);
                else tempCompanyParsedCostPerMinute = LogicManager.ParseCompanyAmountToReadableText(tempCompanyCostPerMinute, true);

                if (tempCompanyCallRates.belTariefBinnenland.dStartTarief != null && tempCompanyCallRates.belTariefBinnenland.dStartTarief != 0)
                    tempCompanyParsedStartRateMobiel = LogicManager.ParseCompanyAmountToReadableText(tempCompanyCallRates.belTariefBinnenland.dStartTarief, true);

                if (tempCompanyCallRates.belTariefBinnenland.dMinuutTarief != null && tempCompanyCallRates.belTariefBinnenland.dMinuutTarief != 0)
                    tempCompanyParsedStartRateBinnenLand = LogicManager.ParseCompanyAmountToReadableText(tempCompanyCallRates.belTariefBinnenland.dMinuutTarief, true);


                monthlyCostData.CompanyName = tempCompanyName;
                monthlyCostData.TotalCalls = tempCompanyTotalCalls;
                monthlyCostData.TotalIns = tempCompanyIns;
                monthlyCostData.TotalOuts = tempCompanyOuts;
                monthlyCostData.TotalCallTime = tempCompanyParsedCallTime;
                monthlyCostData.TotalAmount = tempCompanyParsedTotalAmount;
                monthlyCostData.FirstDate = tempCompanyFirstDate;
                monthlyCostData.LastDate = tempCompanyLastDate;
                monthlyCostData.TimePeriod = tempCompanyParsedTimePeriod;
                monthlyCostData.CostPerMinuteMobiel = tempCompanyParsedCostPerMinute;
                monthlyCostData.StartRateMobiel = tempCompanyParsedStartRateMobiel;
                monthlyCostData.CostPerMinuteBinnenLand = tempCompanyParsedCostPerMinute;
                monthlyCostData.StartRateBinnenLand = tempCompanyParsedStartRateBinnenLand;
                monthlyCostData.Voorschot = tempCompanyParsedVoorschot;
                monthlyCostData.TekortVoorschot = tempCompanyParsedTekortVoorschot;

                return monthlyCostData;
            });
        }
    }
}

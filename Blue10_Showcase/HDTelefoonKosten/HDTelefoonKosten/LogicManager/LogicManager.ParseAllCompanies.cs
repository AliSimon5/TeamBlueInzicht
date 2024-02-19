﻿using DocumentFormat.OpenXml.Office.CustomUI;
using HDTelefoonKosten.Types;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace HDTelefoonKosten.LogicManager
{
    internal partial class LogicManager
    {
        public static List<CompanyType> GetUniqueCompanies(List<CallData> tempAllParsedRowsList)
        {
            List<CallData> tempAllBussinessesList = tempAllParsedRowsList
            .GroupBy(p => p.Bedrijf)
            .Select(g => g.First())
            .ToList();

            List<CompanyType> tempCompanyList = new List<CompanyType>();
            foreach (var item in tempAllBussinessesList)
            {                
                if (item.Subscriber == item.Bedrijf)
                    tempCompanyList.Add(new CompanyType() { strCompany = item.Bedrijf, strSubscriber = item.Subscriber, blnFileDoesntHaveCompanyName = true, strId = item.ID });
                else
                    tempCompanyList.Add(new CompanyType() { strCompany = item.Bedrijf, strSubscriber = item.Subscriber, strId = item.ID });
            }

            return tempCompanyList;
        }

        public static List<MonthType> GetUniqueMonths(List<CallData> tempAllParsedRowsList)
        {
            List<MonthType> tempMonthList = new List<MonthType>();

            List<CallData> tempAllBussinessesList = tempAllParsedRowsList
            .GroupBy(p => p.Date.Month)
            .Select(g => g.First())
            .ToList();

            foreach (var item in tempAllBussinessesList)
            {
                if (item.Date != DateTime.Parse("1-1-0001 00:00:00"))
                    tempMonthList.Add(new MonthType() { intMonth = item.Date.Month });
            }

            return tempMonthList;
        }


        public static List<CompanyType> ParseListAllCompanies(List<CompanyType> argAllCompaniesList)
        {
            List<CompanyType> tempAlltobeParsedList = argAllCompaniesList
            .GroupBy(p => p.strCompany)
            .Select(g => g.First())
            .ToList();

            return tempAlltobeParsedList;
        }
        public static List<SubscriberData> GetUniqueSubscribers(List<CallData> argAllCompaniesList)
        {
            List<SubscriberData> tempAllSubscribersList = new List<SubscriberData>();

            List<CallData> tempAllBussinessesList = argAllCompaniesList
           .GroupBy(p => p.Subscriber)
           .Select(g => g.First())
           .ToList();

            foreach (var item in tempAllBussinessesList)
                tempAllSubscribersList.Add(new SubscriberData() { Subscriber = item.Subscriber });

            return tempAllSubscribersList;
        }
        public static List<CompanyIdType> GetUniqueCompanyIds()
        {
            List<CompanyIdType> tempCompanyIdsList = new List<CompanyIdType>();
            var tempSavedIdsList = DataManager.DataManager.ReadCompanyDataFile();

            foreach (var tempID in tempSavedIdsList)
            {
                if (tempID.CompanyId != null)
                    tempCompanyIdsList.Add(new CompanyIdType() { CompanyId = tempID.CompanyId, CompanyName = tempID.CompanyName, CompanyMarge = tempID.CompanyMarge, CompanyVoorschot = tempID.CompanyVoorschot });
            }
            return tempCompanyIdsList;
        }
    }
}

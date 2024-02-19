﻿using DocumentFormat.OpenXml.Spreadsheet;
using HDTelefoonKosten.DataManager;
using HDTelefoonKosten.Types;
using M.Core.Application.ControlHelpers;
using M.Core.Application.WPF.MessageBox;
using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HDTelefoonKosten.Windows
{
    /// <summary>
    /// Interaction logic for MargeManagerWindow.xaml
    /// </summary>
    public partial class MargeManagerWindow : Window
    {
        private int intMarge = 0;
        private double intVoorschot = 0.0;
        private string strNickname = "";
        private bool blnShowCompaniesOfId = false;
        private string strId = "";
        private List<CompanyMargesType> SelectedCompanyList = new List<CompanyMargesType>();
        private List<CompanyIdType> SelectedCompanyIdList = new List<CompanyIdType>();
        private List<CompanyIdType> SavedSelectedID = new List<CompanyIdType>();
        ListBoxControl<CompanyMargesType> _lbCompanies;
        ListBoxControl<CompanyIdType> _lbCompanyIds;
        public MargeManagerWindow()
        {
            InitializeComponent();

            _lbCompanies = new ListBoxControl<CompanyMargesType>(lbAllCompanies);
            _lbCompanyIds = new ListBoxControl<CompanyIdType>(lbAllCompanyIds);
            _lbCompanies.EventSelectionChanged += _lbCompanies_EventSelectionChanged;


            SetJsonInListBox();
            this.tbPermanentMarge.MaxLength = 3;
        }

        private void _lbCompanies_EventSelectionChanged(object sender, List<CompanyMargesType> e)
        {
            var tempSelection = e.FirstOrDefault();
            if (tempSelection == null) return;

            if (tempSelection.CompanyMarge != null)
                tbPermanentMarge.Text = tempSelection.CompanyMarge.ToString();
            else
                tbPermanentMarge.Text = "";

            if (tempSelection.CompanyVoorschot != null)
                tbVoorschot.Text = tempSelection.CompanyVoorschot.ToString();
            else tbVoorschot.Text = "";

            if (tempSelection.CompanyNickname != null)
                tbNickName.Text = tempSelection.CompanyNickname.ToString();
            else
                tbNickName.Text = "";

            if (tempSelection.CompanyId != null)
                tbCompanyId.Text = tempSelection.CompanyId.ToString();
            else
                tbCompanyId.Text = "";
        }

        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void tbPermanentMarge_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsTextAllowed(e.Text))
            {
                e.Handled = true;
            }
        }

        public void SetJsonInListBox()
        {
            List<CompanyMargesType> tempCompanyMargesFromFileList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempSavedCompanyDataList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempNewCompanyDataList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempCompanyMargesList = new List<CompanyMargesType>();
            List<CompanyIdType> tempAllCompanyIdsList = new List<CompanyIdType>();
            List<CompanyIdType> tempAllCompanyIdsDataList = new List<CompanyIdType>();

            List<CompanyMargesType> tempNewCompanyMargesList = new List<CompanyMargesType>();
            foreach (var item in MainWindow._listAllCompanies)
                tempNewCompanyMargesList.Add(new CompanyMargesType() { CompanyName = item.strCompany });

            // pakt de data van de save file
            tempCompanyMargesFromFileList = DataManager.DataManager.ReadCompanyDataFile();

            // Lijst met CompanieMarginTypes, indien oude al bestaan in saved file, die gebruiken, anders de nieuwe company marges types gebruiken;
            var tempListCompaniesSavedAndNew = new List<CompanyMargesType>();
            var tempNeedsToBeAddedToDataFileList = new List<CompanyMargesType>();

            // checkt of de Saved data file leeg is of niet
            if (tempCompanyMargesFromFileList != null && tempCompanyMargesFromFileList.Count > 0)
            {
                // kijkt of er companies zijn die in het verleden zijn opgeslagen
                foreach (var tempCompany in tempNewCompanyMargesList)
                {
                    var tempFoundCompany = tempCompanyMargesFromFileList.Find(x => x.CompanyName == tempCompany.CompanyName);
                    if (tempFoundCompany != null)
                    {
                        // object bestaat al
                        tempListCompaniesSavedAndNew.Add(tempFoundCompany);
                    }
                    else
                    {
                        //object bestaat nog niet
                        tempNeedsToBeAddedToDataFileList.Add(tempCompany);
                        tempListCompaniesSavedAndNew.Add(tempCompany);
                    }
                }
                tempCompanyMargesFromFileList.AddRange(tempNeedsToBeAddedToDataFileList);

                foreach (var tempCompanyTarget in tempCompanyMargesFromFileList)
                {
                    var tempFoundTargetMatch = MainWindow._listAllCompanies.Find(x => x.strTarget == tempCompanyTarget.CompanyName);
                    if (tempFoundTargetMatch == null)
                        continue;
                    var tempFoundTargetAndSourceMatch = tempCompanyMargesFromFileList.Find(x => x.CompanyName == tempFoundTargetMatch.strCompany);
                    if (tempFoundTargetAndSourceMatch.CompanyId == null)
                        tempFoundTargetAndSourceMatch.CompanyId = tempFoundTargetMatch.strId;
                }

                // schrijft het naar bestand
                DataManager.DataManager.WriteCompanyDataToFile(tempCompanyMargesFromFileList);

                foreach (var item in tempListCompaniesSavedAndNew)
                {
                    if (!string.IsNullOrEmpty(item.CompanyId))
                        tempAllCompanyIdsList.Add(new CompanyIdType() { CompanyId = item.CompanyId, CompanyNickName = item.CompanyNickname });
                }

                List<CompanyIdType> tempCompanyIdsList = tempAllCompanyIdsList
                      .GroupBy(p => p.CompanyId)
                      .Select(g => g.First())
                      .ToList(); 
                
                try
                {
                    _lbCompanyIds.SetItemsSource(tempCompanyIdsList.OrderBy(x => x.CompanyId).ToList());
                }
                catch (Exception ex)
                {
                    Log.Error($"Een ID kon niet gelezen worden en is ook niet getoont\r\nWaarschijnlijk heeft de ID een letter.");
                }

                if (SelectedCompanyIdList.Count > 0)
                    SavedSelectedID = SelectedCompanyIdList;

                if (!(bool)cbShowOnlyCompaniesOfId.IsChecked || SelectedCompanyIdList == null || SelectedCompanyIdList.Count == 0)
                    _lbCompanies.SetItemsSource(tempListCompaniesSavedAndNew);
                else
                {
                    foreach (var tempSelectedID in SelectedCompanyIdList)
                    {
                        var tempList = tempListCompaniesSavedAndNew.FindAll(x => x.CompanyId == tempSelectedID.CompanyId);
                        _lbCompanies.SetItemsSource(tempList);
                        break;
                    }
                }
            }
            else
            {
                _lbCompanies.SetItemsSource(tempNewCompanyMargesList);
                DataManager.DataManager.WriteCompanyDataToFile(tempNewCompanyMargesList);
            }
        }

        private void btnSetMargeForCompany_Click(object sender, RoutedEventArgs e)
        {
            List<CompanyMargesType> tempCompanyMargesList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempSavedCompanyMargeList = new List<CompanyMargesType>();
            CompanyMargesType companyMargesType = new CompanyMargesType();
            SelectedCompanyList = _lbCompanies.GetSelections();
            SelectedCompanyIdList = _lbCompanyIds.GetSelections();

            if (SelectedCompanyList == null && SelectedCompanyIdList == null)
            {
                MBox.ShowWarning("Geen bedrijf of ID gekozen om marge aan te passen");
                return;
            }

            if (SelectedCompanyList != null)
            {
                foreach (var SelectedCompany in SelectedCompanyList)
                {
                    if (string.IsNullOrEmpty(tbPermanentMarge.Text))
                    {
                        intMarge = 0;
                        companyMargesType.CompanyName = SelectedCompany.CompanyName.ToString();
                        companyMargesType.CompanyMarge = intMarge;
                    }
                    else
                    {
                        intMarge = int.Parse(tbPermanentMarge.Text);
                        companyMargesType.CompanyName = SelectedCompany.CompanyName.ToString();
                        companyMargesType.CompanyMarge = intMarge;
                    }
                    tempCompanyMargesList.Add(companyMargesType);
                    CompanyMargesType tempFoundChangedCompanyMarge = new CompanyMargesType();
                    tempSavedCompanyMargeList = DataManager.DataManager.ReadCompanyDataFile();
                    if (tempSavedCompanyMargeList != null)
                    {
                        tempFoundChangedCompanyMarge = tempSavedCompanyMargeList.Find(x => x.CompanyName == SelectedCompany.CompanyName);

                        tempFoundChangedCompanyMarge.CompanyMarge = companyMargesType.CompanyMarge;
                    }
                    else
                    {
                        tempSavedCompanyMargeList = new List<CompanyMargesType>();
                        tempFoundChangedCompanyMarge.CompanyName = SelectedCompany.CompanyName.ToString();
                        tempFoundChangedCompanyMarge.CompanyMarge = companyMargesType.CompanyMarge;
                        tempSavedCompanyMargeList.Add(tempFoundChangedCompanyMarge);
                    }
                    DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyMargeList);
                    SetJsonInListBox();
                }
            }

            if (SelectedCompanyIdList != null && SelectedCompanyList.Count == 0)
            {
                tempSavedCompanyMargeList = DataManager.DataManager.ReadCompanyDataFile();

                foreach (var tempSelectedID in SelectedCompanyIdList)
                {
                    var tempFoundCompaniesOfId = tempSavedCompanyMargeList.FindAll(x => x.CompanyId == tempSelectedID.CompanyId);

                    foreach (var tempSelectedCompany in tempFoundCompaniesOfId)
                    {
                        if (string.IsNullOrEmpty(tbPermanentMarge.Text))
                        {
                            intMarge = 0;
                            companyMargesType.CompanyName = tempSelectedCompany.CompanyName.ToString();
                            companyMargesType.CompanyMarge = intMarge;
                        }
                        else
                        {
                            intMarge = int.Parse(tbPermanentMarge.Text);
                            companyMargesType.CompanyName = tempSelectedCompany.CompanyName.ToString();
                            companyMargesType.CompanyMarge = intMarge;
                        }
                        tempCompanyMargesList.Add(companyMargesType);
                        CompanyMargesType tempFoundChangedCompanyMarge = new CompanyMargesType();
                        tempSavedCompanyMargeList = DataManager.DataManager.ReadCompanyDataFile();
                        if (tempSavedCompanyMargeList != null)
                        {
                            tempFoundChangedCompanyMarge = tempSavedCompanyMargeList.Find(x => x.CompanyName == tempSelectedCompany.CompanyName);

                            tempFoundChangedCompanyMarge.CompanyMarge = companyMargesType.CompanyMarge;
                        }
                        else
                        {
                            tempSavedCompanyMargeList = new List<CompanyMargesType>();
                            tempFoundChangedCompanyMarge.CompanyName = tempSelectedCompany.CompanyName.ToString();
                            tempFoundChangedCompanyMarge.CompanyMarge = companyMargesType.CompanyMarge;
                            tempSavedCompanyMargeList.Add(tempFoundChangedCompanyMarge);
                        }
                        DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyMargeList);
                        SetJsonInListBox();
                    }
                }
            }
            SetJsonInListBox();
        }

        private void btnDeleteAllCompanyMarges_Click(object sender, RoutedEventArgs e)
        {
            if (MBox.ShowQuestionWarning("Weet u zeker dat u alle bewaarde gegevens wilt verwijderen?\rHier is geen backup van na het drukken van \"Yes\" wordt alles gewist!"))
            {
                _lbCompanies.SetItemsSource(null);
                DataManager.DataManager.ClearCompanyMargeFile();
            }
        }

        private void btnSetVoorschot_Click(object sender, RoutedEventArgs e)
        {
            List<CompanyMargesType> tempCompanyVoorschotenList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempSavedCompanyVoorschotenList = new List<CompanyMargesType>();
            CompanyMargesType companyVoorschotenType = new CompanyMargesType();
            SelectedCompanyList = _lbCompanies.GetSelections();
            SelectedCompanyIdList = _lbCompanyIds.GetSelections();

            if (SelectedCompanyList == null && SelectedCompanyIdList == null)
            {
                MBox.ShowWarning("Geen bedrijf of ID gekozen om voorschot aan te passen");
                return;
            }
            if (SelectedCompanyList != null)
            {
                foreach (var SelectedCompany in SelectedCompanyList)
                {
                    if (string.IsNullOrEmpty(tbVoorschot.Text))
                    {
                        intVoorschot = 0;
                        companyVoorschotenType.CompanyName = SelectedCompany.CompanyName.ToString();
                        companyVoorschotenType.CompanyVoorschot = intVoorschot;
                    }
                    else
                    {
                        intVoorschot = double.Parse(tbVoorschot.Text);
                        companyVoorschotenType.CompanyName = SelectedCompany.CompanyName.ToString();
                        companyVoorschotenType.CompanyVoorschot = intVoorschot;
                    }
                    tempCompanyVoorschotenList.Add(companyVoorschotenType);
                    CompanyMargesType tempFoundChangedCompanyVoorSchot = new CompanyMargesType();
                    tempSavedCompanyVoorschotenList = DataManager.DataManager.ReadCompanyDataFile();
                    if (tempSavedCompanyVoorschotenList != null)
                    {
                        tempFoundChangedCompanyVoorSchot = tempSavedCompanyVoorschotenList.Find(x => x.CompanyName == SelectedCompany.CompanyName);

                        tempFoundChangedCompanyVoorSchot.CompanyVoorschot = companyVoorschotenType.CompanyVoorschot;
                    }
                    else
                    {
                        tempSavedCompanyVoorschotenList = new List<CompanyMargesType>();
                        tempFoundChangedCompanyVoorSchot.CompanyName = SelectedCompany.CompanyName.ToString();
                        tempFoundChangedCompanyVoorSchot.CompanyVoorschot = companyVoorschotenType.CompanyVoorschot;
                        tempSavedCompanyVoorschotenList.Add(tempFoundChangedCompanyVoorSchot);
                    }
                    DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyVoorschotenList);
                    SetJsonInListBox();
                }
            }

            if (SelectedCompanyIdList != null && SelectedCompanyList.Count == 0)
            {
                tempSavedCompanyVoorschotenList = DataManager.DataManager.ReadCompanyDataFile();

                foreach (var tempSelectedID in SelectedCompanyIdList)
                {
                    var tempFoundCompaniesOfId = tempSavedCompanyVoorschotenList.FindAll(x => x.CompanyId == tempSelectedID.CompanyId);

                    foreach (var tempSelectedCompany in tempFoundCompaniesOfId)
                    {

                        if (string.IsNullOrEmpty(tbVoorschot.Text))
                        {
                            intVoorschot = 0;
                            companyVoorschotenType.CompanyName = tempSelectedCompany.CompanyName.ToString();
                            companyVoorschotenType.CompanyVoorschot = intVoorschot;
                        }
                        else
                        {
                            intVoorschot = double.Parse(tbVoorschot.Text);
                            companyVoorschotenType.CompanyName = tempSelectedCompany.CompanyName.ToString();
                            companyVoorschotenType.CompanyVoorschot = intVoorschot;
                        }
                        tempCompanyVoorschotenList.Add(companyVoorschotenType);
                        CompanyMargesType tempFoundChangedCompanyVoorSchot = new CompanyMargesType();
                        tempSavedCompanyVoorschotenList = DataManager.DataManager.ReadCompanyDataFile();
                        if (tempSavedCompanyVoorschotenList != null)
                        {
                            tempFoundChangedCompanyVoorSchot = tempSavedCompanyVoorschotenList.Find(x => x.CompanyName == tempSelectedCompany.CompanyName);

                            tempFoundChangedCompanyVoorSchot.CompanyVoorschot = companyVoorschotenType.CompanyVoorschot;
                        }
                        else
                        {
                            tempSavedCompanyVoorschotenList = new List<CompanyMargesType>();
                            tempFoundChangedCompanyVoorSchot.CompanyName = tempSelectedCompany.CompanyName.ToString();
                            tempFoundChangedCompanyVoorSchot.CompanyVoorschot = companyVoorschotenType.CompanyVoorschot;
                            tempSavedCompanyVoorschotenList.Add(tempFoundChangedCompanyVoorSchot);
                        }
                        DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyVoorschotenList);
                        SetJsonInListBox();
                    }
                }

            }

            SetJsonInListBox();
        }

        private void btnSetCompanyId_Click(object sender, RoutedEventArgs e)
        {
            List<CompanyMargesType> tempCompanyIdList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempSavedCompanyIdList = new List<CompanyMargesType>();
            CompanyMargesType companyIdType = new CompanyMargesType();
            SelectedCompanyList = _lbCompanies.GetSelections();

            if (SelectedCompanyList == null)
            {
                MBox.ShowWarning("Geen bedrijf gekozen");
                return;
            }

            foreach (var SelectedCompany in SelectedCompanyList)
            {
                if (string.IsNullOrEmpty(tbCompanyId.Text))
                {
                    strId = "";
                    companyIdType.CompanyName = SelectedCompany.CompanyName.ToString();
                    companyIdType.CompanyId = strId;
                }
                else
                {
                    strId = tbCompanyId.Text;
                    companyIdType.CompanyName = SelectedCompany.CompanyName.ToString();
                    companyIdType.CompanyId = strId;
                }

                tempCompanyIdList.Add(companyIdType);

                CompanyMargesType tempFoundChangedCompanyId = new CompanyMargesType();
                tempSavedCompanyIdList = DataManager.DataManager.ReadCompanyDataFile();

                if (tempSavedCompanyIdList != null)
                {
                    tempFoundChangedCompanyId = tempSavedCompanyIdList.Find(x => x.CompanyName == SelectedCompany.CompanyName);

                    tempFoundChangedCompanyId.CompanyId = companyIdType.CompanyId;
                }
                else
                {
                    tempSavedCompanyIdList = new List<CompanyMargesType>();
                    tempFoundChangedCompanyId.CompanyName = SelectedCompany.CompanyName.ToString();
                    tempFoundChangedCompanyId.CompanyId = companyIdType.CompanyId;
                    tempSavedCompanyIdList.Add(tempFoundChangedCompanyId);
                }
                DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyIdList);
                SetJsonInListBox();
            }
        }

        private void lbAllCompanyIds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!blnShowCompaniesOfId) return;

            SelectedCompanyIdList = _lbCompanyIds.GetSelections();

            List<CompanyMargesType> tempSavedCompanyIdList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempCompanyNamesList = new List<CompanyMargesType>();
            List<CompanyIdType> tempCompanyNamesOfIdList = new List<CompanyIdType>();

            if (SelectedCompanyIdList.Count == 0)
            {
                SetJsonInListBox();
                return;
            }

            foreach (var tempCompantId in SelectedCompanyIdList)
            {
                var tempCompanyIdList = MainWindow.GetCompaniesWithId(tempCompantId);
                tempCompanyNamesOfIdList.AddRange(tempCompanyIdList);
            }

            foreach (var tempCompanyName in tempCompanyNamesOfIdList)
            {
                tempCompanyNamesList.Add(new CompanyMargesType() { CompanyName = tempCompanyName.CompanyName, CompanyMarge = tempCompanyName.CompanyMarge, CompanyVoorschot = tempCompanyName.CompanyVoorschot });
            }

            _lbCompanies.SetItemsSource(tempCompanyNamesList);
        }

        private void btnLinkCompaniesToId_Click(object sender, RoutedEventArgs e)
        {
            List<CompanyMargesType> tempCompanyIdList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempSavedCompanyIdList = new List<CompanyMargesType>();
            CompanyMargesType companyIdType = new CompanyMargesType();

            SelectedCompanyList = _lbCompanies.GetSelections();
            SelectedCompanyIdList = _lbCompanyIds.GetSelections();

            if (SelectedCompanyList == null || SelectedCompanyIdList == null)
            {
                MBox.ShowWarning("Geen bedrijf of ID gekozen");
                return;
            }

            foreach (var SelectedID in SelectedCompanyIdList)
            {
                foreach (var SelectedCompany in SelectedCompanyList)
                {
                    SelectedCompany.CompanyId = SelectedID.CompanyId;

                    CompanyMargesType tempFoundChangedCompanyId = new CompanyMargesType();

                    tempSavedCompanyIdList = DataManager.DataManager.ReadCompanyDataFile();

                    if (tempSavedCompanyIdList != null)
                    {
                        tempFoundChangedCompanyId = tempSavedCompanyIdList.Find(x => x.CompanyName == SelectedCompany.CompanyName);
                        tempFoundChangedCompanyId.CompanyId = SelectedID.CompanyId;
                    }
                    else
                    {
                        tempSavedCompanyIdList = new List<CompanyMargesType>();
                        tempFoundChangedCompanyId.CompanyName = SelectedCompany.CompanyName.ToString();
                        tempFoundChangedCompanyId.CompanyId = SelectedID.CompanyId;
                        tempSavedCompanyIdList.Add(tempFoundChangedCompanyId);
                    }
                    DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyIdList);
                    SetJsonInListBox();
                }
            }
        }

        private void cbShowOnlyCompaniesOfId_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cbShowOnlyCompaniesOfId.IsChecked)
                blnShowCompaniesOfId = true;
            else
            {
                blnShowCompaniesOfId = false;
                SetJsonInListBox();
            }
            lbAllCompanyIds.UnselectAll();
        }

        private void btnDeleteCompanyOfId_Click(object sender, RoutedEventArgs e)
        {
            List<CompanyMargesType> tempCompanyIdList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempSavedCompanyIdList = new List<CompanyMargesType>();
            CompanyMargesType companyIdType = new CompanyMargesType();

            SelectedCompanyList = _lbCompanies.GetSelections();
            SelectedCompanyIdList = _lbCompanyIds.GetSelections();

            if (SelectedCompanyList == null || SelectedCompanyIdList == null)
            {
                MBox.ShowWarning("Geen bedrijf of id gekozen");
                return;
            }

            foreach (var SelectedCompany in SelectedCompanyList)
            {
                SelectedCompany.CompanyId = null;

                CompanyMargesType tempFoundChangedCompanyId = new CompanyMargesType();

                tempSavedCompanyIdList = DataManager.DataManager.ReadCompanyDataFile();

                if (tempSavedCompanyIdList != null)
                {
                    tempFoundChangedCompanyId = tempSavedCompanyIdList.Find(x => x.CompanyName == SelectedCompany.CompanyName);
                    tempFoundChangedCompanyId.CompanyId = null;
                }
                else
                {
                    tempSavedCompanyIdList = new List<CompanyMargesType>();
                    tempFoundChangedCompanyId.CompanyName = SelectedCompany.CompanyName.ToString();
                    tempFoundChangedCompanyId.CompanyId = null;
                    tempSavedCompanyIdList.Add(tempFoundChangedCompanyId);
                }
                DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyIdList);
                SetJsonInListBox();
            }
        }

        private void btnSetCompanyNickName_Click(object sender, RoutedEventArgs e)
        {
            List<CompanyMargesType> tempCompanyNicknamesList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempSavedCompanyNicknamesList = new List<CompanyMargesType>();
            CompanyMargesType companyNicknamesType = new CompanyMargesType();
            SelectedCompanyList = _lbCompanies.GetSelections();
            SelectedCompanyIdList = _lbCompanyIds.GetSelections();

            if (SelectedCompanyList == null)
            {
                MBox.ShowWarning("Geen bedrijf gekozen om een nickname te geven");
                return;
            }

            foreach (var tempSelectedCompany in SelectedCompanyList)
            {
                if (string.IsNullOrEmpty(tbNickName.Text))
                {
                    strNickname = "";
                    companyNicknamesType.CompanyName = tempSelectedCompany.CompanyName.ToString();
                    companyNicknamesType.CompanyNickname = strNickname;
                }
                else
                {
                    strNickname = tbNickName.Text;
                    companyNicknamesType.CompanyName = tempSelectedCompany.CompanyName.ToString();
                    companyNicknamesType.CompanyNickname = strNickname;
                }
                tempCompanyNicknamesList.Add(companyNicknamesType);
                CompanyMargesType tempFoundChangedCompanyNicknames = new CompanyMargesType();
                tempSavedCompanyNicknamesList = DataManager.DataManager.ReadCompanyDataFile();
                if (tempSavedCompanyNicknamesList != null)
                {
                    tempFoundChangedCompanyNicknames = tempSavedCompanyNicknamesList.Find(x => x.CompanyName == tempSelectedCompany.CompanyName);

                    tempFoundChangedCompanyNicknames.CompanyNickname = companyNicknamesType.CompanyNickname;
                }
                else
                {
                    tempSavedCompanyNicknamesList = new List<CompanyMargesType>();
                    tempFoundChangedCompanyNicknames.CompanyName = tempSelectedCompany.CompanyName.ToString();
                    tempFoundChangedCompanyNicknames.CompanyNickname = companyNicknamesType.CompanyNickname;
                    tempSavedCompanyNicknamesList.Add(tempFoundChangedCompanyNicknames);
                }
                DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyNicknamesList);
                SetJsonInListBox();
            }
        }

        private void btnDeleteNickname_Click(object sender, RoutedEventArgs e)
        {
            List<CompanyMargesType> tempCompanyIdList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempSavedCompanyNicknameList = new List<CompanyMargesType>();
            CompanyMargesType companyIdType = new CompanyMargesType();

            SelectedCompanyList = _lbCompanies.GetSelections();

            if (SelectedCompanyList == null)
            {
                MBox.ShowWarning("Geen bedrijf gekozen");
                return;
            }

            foreach (var tempSelectedCompany in SelectedCompanyList)
            {
                tempSelectedCompany.CompanyNickname = null;

                CompanyMargesType tempFoundChangedCompanyNickname = new CompanyMargesType();

                tempSavedCompanyNicknameList = DataManager.DataManager.ReadCompanyDataFile();

                if (tempSavedCompanyNicknameList != null)
                {
                    tempFoundChangedCompanyNickname = tempSavedCompanyNicknameList.Find(x => x.CompanyName == tempSelectedCompany.CompanyName);
                    tempFoundChangedCompanyNickname.CompanyNickname = null;
                }
                else
                {
                    tempSavedCompanyNicknameList = new List<CompanyMargesType>();
                    tempFoundChangedCompanyNickname.CompanyName = tempSelectedCompany.CompanyName.ToString();
                    tempFoundChangedCompanyNickname.CompanyNickname = null;
                    tempSavedCompanyNicknameList.Add(tempFoundChangedCompanyNickname);
                }
                DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyNicknameList);
                SetJsonInListBox();
            }
        }

        private void btnImportCompanyData_Click(object sender, RoutedEventArgs e)
        {
            var tempOpenFileDialog = new OpenFileDialog();
            if (!(bool)tempOpenFileDialog.ShowDialog()) return;


            List<CompanyMargesType> tempSavedDataList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempListCompaniesSavedAndNew = new List<CompanyMargesType>();
            var tempNeedsToBeAddedToDataFileList = new List<CompanyMargesType>();
            this.IsEnabled = false;
            this.Cursor = Cursors.Wait;
            try
            {

                var tempFilePath = tempOpenFileDialog.FileName;
                tempSavedDataList = DataManager.DataManager.ReadCompanyDataFile();

                List<CompanyMargesType> tempAllRowsList = LogicManager.LogicManager.GetAllParsedDataRowsList(tempFilePath);

                foreach (var tempRow in tempAllRowsList)
                {
                    var tempFoundCompany = tempSavedDataList.Find(x => x.CompanyName == tempRow.CompanyName);
                    if (tempFoundCompany != null)
                    {
                        tempFoundCompany.CompanyVoorschot = tempRow.CompanyVoorschot;
                        tempFoundCompany.CompanyId = tempRow.CompanyId;

                        //tempListCompaniesSavedAndNew.Add(tempFoundCompany);
                    }
                    else
                    {
                        tempNeedsToBeAddedToDataFileList.Add(tempRow);
                        // tempListCompaniesSavedAndNew.Add(tempRow);
                    }
                }
                tempSavedDataList.AddRange(tempNeedsToBeAddedToDataFileList);

                DataManager.DataManager.WriteCompanyDataToFile(tempSavedDataList);


                SetJsonInListBox();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                this.IsEnabled = true;
                this.Cursor = null;
                return;
            }
            this.IsEnabled = true;
            this.Cursor = null;
        }

        private void btnSelectAllCompanyIds_Click(object sender, RoutedEventArgs e)
        {
            lbAllCompanyIds.SelectAll();
        }

        private void btnDeselectAllCompanyIds_Click(object sender, RoutedEventArgs e)
        {
            lbAllCompanyIds.UnselectAll();
        }

        private void btnSelectAllCompanies_Click(object sender, RoutedEventArgs e)
        {
            lbAllCompanies.SelectAll();
        }

        private void btnDeselectAllCompanies_Click(object sender, RoutedEventArgs e)
        {
            lbAllCompanies.UnselectAll();
        }

        private void btnShowCompanyWithNoId_Click(object sender, RoutedEventArgs e)
        {
            List<CompanyMargesType> tempCompanyWithNoIdList = new List<CompanyMargesType>();

            // Leest de bestand met alle opgeslagen data van de companies
            var tempSavedIdsList = DataManager.DataManager.ReadCompanyDataFile();
            if (tempSavedIdsList == null) return;

            // loop om door elke saved ID/data van een company om te zoeken welke geen ID hebben
            foreach (var tempCompanyData in tempSavedIdsList)
            {
                if (tempCompanyData.CompanyId == null)
                {
                    if (MainWindow._listAllCompanies.Any(x => x.strCompany == tempCompanyData.CompanyName))
                        tempCompanyWithNoIdList.Add(new CompanyMargesType() { CompanyName = tempCompanyData.CompanyName });
                }
            }
            _lbCompanies.SetItemsSource(tempCompanyWithNoIdList);
        }

        private void btnShowAllCompanies_Click(object sender, RoutedEventArgs e)
        {
            SetJsonInListBox();
            lbAllCompanyIds.UnselectAll();
        }
    }
}

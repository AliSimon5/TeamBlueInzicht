using DocumentFormat.OpenXml.Office2019.Excel.RichData2;
using HDTelefoonKosten.Types;
using M.Core.Application.ControlHelpers;
using M.Core.Application.WPF.MessageBox;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HDTelefoonKosten.Windows
{
    /// <summary>
    /// Interaction logic for TargetManagerWindow.xaml
    /// </summary>
    public partial class TargetManagerWindow : Window
    {
        public ListBoxControl<TargetType> _lbTargets;
        List<CallData> listAllCallData = MainWindow._listAllCallData;
        public ListBoxControl<CompanyIdType> _lbTargetIds;
        private bool blnShowCompaniesOfId = false;
        private List<TargetType> SelectedTargetList = new List<TargetType>();
        private List<CompanyIdType> SelectedTargetIdList = new List<CompanyIdType>();
        public TargetManagerWindow()
        {
            InitializeComponent();

            _lbTargets = new ListBoxControl<TargetType>(lbAllTargets);
            _lbTargetIds = new ListBoxControl<CompanyIdType>(lbAllIds);
            SetJsonInListBox();
        }
        public void SetJsonInListBox()
        {
            var tempListCompaniesSavedAndNew = new List<CompanyMargesType>();
            var tempNeedsToBeAddedToDataFileList = new List<CompanyMargesType>();
            var tempTargetWithoutID = new List<TargetType>();
            List<CompanyIdType> tempAllCompanyIdsList = new List<CompanyIdType>();
            var tempSavedDataFileList = DataManager.DataManager.ReadCompanyDataFile();
            List<CallData> tempCompanyWithoutInfoList = new List<CallData>();

            List<CompanyMargesType> tempNewCompanyMargesList = new List<CompanyMargesType>();
            foreach (var item in MainWindow._listAllCompanies)
                tempNewCompanyMargesList.Add(new CompanyMargesType() { CompanyName = item.strCompany });

            foreach (var tempCompany in tempNewCompanyMargesList)
            {
                var tempFoundCompany = tempSavedDataFileList.Find(x => x.CompanyName == tempCompany.CompanyName);
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
            tempSavedDataFileList.AddRange(tempNeedsToBeAddedToDataFileList);

            foreach (var tempCompanyTarget in tempSavedDataFileList)
            {
                var tempFoundTargetMatch = MainWindow._listAllCompanies.Find(x => x.strTarget == tempCompanyTarget.CompanyName);
                if (tempFoundTargetMatch == null)
                    continue;
                var tempFoundTargetAndSourceMatch = tempSavedDataFileList.Find(x => x.CompanyName == tempFoundTargetMatch.strCompany);
                if (tempFoundTargetAndSourceMatch.CompanyId == null)
                    tempFoundTargetAndSourceMatch.CompanyId = tempFoundTargetMatch.strId;
            }

            DataManager.DataManager.WriteCompanyDataToFile(tempSavedDataFileList);

            foreach (var tempCompany in listAllCallData)
            {
                if (tempCompany.ID == null && tempCompany.Subscriber == tempCompany.Bedrijf)
                {
                    tempCompanyWithoutInfoList.Add(tempCompany);
                }
            }
            foreach (var item in tempListCompaniesSavedAndNew)
            {
                if (!string.IsNullOrEmpty(item.CompanyId))
                    tempAllCompanyIdsList.Add(new CompanyIdType() { CompanyId = item.CompanyId, CompanyNickName = item.CompanyNickname });
            }

            List<CompanyIdType> tempCompanyIdsList = tempAllCompanyIdsList
                  .GroupBy(p => p.CompanyId)
                  .Select(g => g.First())
                  .ToList();

            List<CallData> tempGroupedTargetsList = tempCompanyWithoutInfoList
              .GroupBy(p => p.Target)
              .Select(s => s.First())
              .ToList();

            try
            {
                _lbTargetIds.SetItemsSource(tempCompanyIdsList.OrderBy(x => x.CompanyId).ToList());
            }
            catch (Exception ex)
            {
                Log.Error($"Een ID kon niet gelezen worden en is ook niet getoont\r\nWaarschijnlijk heeft de ID een letter.");
            }

            foreach (var tempTarget in tempGroupedTargetsList)
            {
                var tempFoundTarget = tempSavedDataFileList.Find(x => x.CompanyName == tempTarget.Target);
                if (tempFoundTarget == null)
                    tempTargetWithoutID.Add(new TargetType() { Target = tempTarget.Target });
            }
            _lbTargets.SetItemsSource(tempTargetWithoutID);
        }
        private void btnSetTargetId_Click(object sender, RoutedEventArgs e)
        {
            SelectedTargetList = _lbTargets.GetSelections();

            if (SelectedTargetList != null && SelectedTargetList.Count == 0)
            {
                MBox.ShowWarning("Selecteer eerst een Target");
                return;
            }
            foreach (var tempSelectedTarget in SelectedTargetList)
            {
                tempSelectedTarget.ID = tbTargetId.Text;
                
                List<CompanyMargesType> tempCompanyIdList = new List<CompanyMargesType>();
                List<CompanyMargesType> tempSavedCompanyIdList = new List<CompanyMargesType>();
                CompanyMargesType companyIdType = new CompanyMargesType();

                CompanyMargesType tempFoundChangedCompanyId = new CompanyMargesType();

                tempSavedCompanyIdList = DataManager.DataManager.ReadCompanyDataFile();
                if (tempSavedCompanyIdList != null)
                {
                    var tempFoundSourceOfTarget = listAllCallData.Find(x => x.Target == tempSelectedTarget.Target);
                    tempFoundChangedCompanyId = tempSavedCompanyIdList.Find(x => x.CompanyName == tempFoundSourceOfTarget.Bedrijf);
                    tempFoundChangedCompanyId.CompanyId = tempSelectedTarget.ID;
                    tempSavedCompanyIdList.Add(tempFoundChangedCompanyId);


                    tempFoundChangedCompanyId = tempSavedCompanyIdList.Find(x => x.CompanyName == tempSelectedTarget.Target);
                    if (tempFoundChangedCompanyId != null)
                    {
                        tempFoundChangedCompanyId.CompanyId = tbTargetId.Text;
                        tempSavedCompanyIdList.Add(tempFoundChangedCompanyId);
                    }
                }
                else
                {
                    tempSavedCompanyIdList = new List<CompanyMargesType>();
                    tempFoundChangedCompanyId.CompanyName = tempSelectedTarget.Target.ToString();
                    tempFoundChangedCompanyId.CompanyId = tbTargetId.Text;
                    tempSavedCompanyIdList.Add(tempFoundChangedCompanyId);
                }
                DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyIdList);
                SetJsonInListBox();
            }
        }


        private void btnLinkTargetToId_Click(object sender, RoutedEventArgs e)
        {
            List<CompanyMargesType> tempCompanyIdList = new List<CompanyMargesType>();
            List<CompanyMargesType> tempSavedCompanyIdList = new List<CompanyMargesType>();
            CompanyMargesType companyIdType = new CompanyMargesType();

            SelectedTargetList = _lbTargets.GetSelections();
            SelectedTargetIdList = _lbTargetIds.GetSelections();

            if (SelectedTargetList.Count == 0 || SelectedTargetIdList.Count == 0)
            {
                MBox.ShowWarning("Geen bedrijf of ID gekozen");
                return;
            }

            foreach (var SelectedID in SelectedTargetIdList)
            {
                foreach (var tempSelectedTarget in SelectedTargetList)
                {
                    tempSelectedTarget.ID = SelectedID.CompanyId;

                    CompanyMargesType tempFoundChangedCompanyId = new CompanyMargesType();

                    tempSavedCompanyIdList = DataManager.DataManager.ReadCompanyDataFile();
                    if (tempSavedCompanyIdList != null)
                    {
                        var tempFoundSourceOfTarget = listAllCallData.Find(x => x.Target == tempSelectedTarget.Target);
                        tempFoundChangedCompanyId = tempSavedCompanyIdList.Find(x => x.CompanyName == tempFoundSourceOfTarget.Bedrijf);
                        tempFoundChangedCompanyId.CompanyId = tempSelectedTarget.ID;
                        tempSavedCompanyIdList.Add(tempFoundChangedCompanyId);


                        tempFoundChangedCompanyId = tempSavedCompanyIdList.Find(x => x.CompanyName == tempSelectedTarget.Target);
                        if (tempFoundChangedCompanyId != null)
                        {
                            tempFoundChangedCompanyId.CompanyId = SelectedID.CompanyId;
                            tempSavedCompanyIdList.Add(tempFoundChangedCompanyId);
                        }
                    }
                    else
                    {
                        tempSavedCompanyIdList = new List<CompanyMargesType>();
                        tempFoundChangedCompanyId.CompanyName = tempSelectedTarget.Target.ToString();
                        tempFoundChangedCompanyId.CompanyId = SelectedID.CompanyId;
                        tempSavedCompanyIdList.Add(tempFoundChangedCompanyId);
                    }
                    DataManager.DataManager.WriteCompanyDataToFile(tempSavedCompanyIdList);
                    SetJsonInListBox();
                }
            }
        }

        private void btnDeselectAllIds_Click(object sender, RoutedEventArgs e)
        {
            lbAllIds.SelectAll();
        }

        private void btnSelectAllIds_Click(object sender, RoutedEventArgs e)
        {
            lbAllIds.UnselectAll();
        }

        private void btnSelectAllTargets_Click(object sender, RoutedEventArgs e)
        {
            lbAllTargets.SelectAll();
        }

        private void btnDeselectAllTargets_Click(object sender, RoutedEventArgs e)
        {
            lbAllTargets.UnselectAll();
        }

        private void cbShowOnlyTargetsOfId_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cbShowOnlyTargetsOfId.IsChecked)
                blnShowCompaniesOfId = true;
            else
            {
                blnShowCompaniesOfId = false;
                SetJsonInListBox();
            }
            lbAllIds.UnselectAll();
        }

        private void lbAllIds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!blnShowCompaniesOfId) return;

            SelectedTargetIdList = _lbTargetIds.GetSelections();

            List<TargetType> tempTargetNamesList = new List<TargetType>();
            List<CompanyIdType> tempTargetNamesOfIdList = new List<CompanyIdType>();

            if (SelectedTargetIdList.Count == 0)
            {
                SetJsonInListBox();
                return;
            }

            foreach (var tempCompantId in SelectedTargetIdList)
            {
                var tempCompanyIdList = MainWindow.GetCompaniesWithId(tempCompantId);
                tempTargetNamesOfIdList.AddRange(tempCompanyIdList);
            }

            foreach (var tempCompanyName in tempTargetNamesOfIdList)
            {
                tempTargetNamesList.Add(new TargetType() { Target = tempCompanyName.CompanyName });
            }

            _lbTargets.SetItemsSource(tempTargetNamesList);
        }
    }
}

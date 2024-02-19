﻿using DocumentFormat.OpenXml.ExtendedProperties;
using HDTelefoonKosten.Types;
using HDTelefoonKosten.Windows;
using M.Core.Application.ControlHelpers;
using M.Core.Application.WPF.MessageBox;
using M.NetStandard.SinkToAction;
using Microsoft.Win32;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HDTelefoonKosten
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        StringBuilder _stringBuilderLogging = new StringBuilder();

        private double dMarge = 0;
        private double dVoorschot = 0;

        public static ProgressBar referenceProgressBar;

        ListBoxControl<CompanyType> _lbCompanies;
        ListBoxControl<SubscriberData> _lbSubscribers;
        ListBoxControl<CompanyIdType> _lbCompanyIds;
        private DataGridControl<CallData> _dgCallData;
        private DataGridControl<MonthlyCostData> _dgMonthlyCostData;

        private List<CompanyType> _selectedCompanyType = null;
        private List<SubscriberData> _selectedSubscriberType = null;
        private List<CompanyIdType> _selectedCompanyIdType = null;
        public static List<CompanyType> _listAllCompanies = new List<CompanyType>(); // aleen strCompany
        public static List<CallData> _listAllCallData = new List<CallData>(); // alle info
        private List<MonthType> _listAllMonths = new List<MonthType>(); // alleen maanden
        private List<SubscriberData> _listAllSubscriber = new List<SubscriberData>(); // alle subscribers
        private List<CompanyIdType> _listAllCompanyIds = new List<CompanyIdType>(); // alle Company ids
        public List<string> _listAllFilePaths = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            referenceProgressBar = pbProgress;

            #region Serilog logt naar een textbox
            //zoekt waar de applicatie is opgestart en maakt een bestand aan die "HDTelefoonKosten_Logs" heet
            var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var logPath = Path.Combine(startupPath, "HDTelefoonKosten_Logs");

            //Logger
#if DEBUG

            Log.Logger = new LoggerConfiguration()
                              //Wat de bestanden hun naam zijn en waar ze naartoe moeten
                              .WriteTo.Sink(new ActionSink(LogToTextBox))
                            .WriteTo.File(Path.Combine(logPath, $"[{DateTime.Now:yyyy-MM-dd}] HDTelefoonKosten.log"),
                                rollingInterval: RollingInterval.Day,
                                fileSizeLimitBytes: 10000000,
                                retainedFileCountLimit: 10,
                                rollOnFileSizeLimit: true,
                                buffered: true,
                                flushToDiskInterval: TimeSpan.FromMilliseconds(150))
                            .MinimumLevel.Verbose()
                            //maakt de .log bestand aan
                            .CreateLogger();
#else
            Log.Logger = new LoggerConfiguration()
                            //Wat de bestanden hun naam zijn en waar ze naartoe moeten
                            .WriteTo.Sink(new ActionSink(LogToTextBox))
                            .WriteTo.File(Path.Combine(logPath, $"[{DateTime.Now:yyyy-MM-dd}] HDTelefoonKosten.log"),
                                rollingInterval: RollingInterval.Day,
                                fileSizeLimitBytes: 10000000,
                                retainedFileCountLimit: 10,
                                rollOnFileSizeLimit: true,
                                buffered: true,
                                flushToDiskInterval: TimeSpan.FromMilliseconds(150))
                            .MinimumLevel.Information()
                            //maakt de .log bestand aan
                            .CreateLogger();
#endif
            #endregion

            #region CallData DataGrid
            // DataGrid met alle bedrijven en hun data die uit de document is gehaald
            _dgCallData = new DataGridControl<CallData>(dgCallData, true);
            _dgCallData.CreateTextColumn($"Direction", nameof(CallData.Direction), 111);
            _dgCallData.CreateTextColumn($"Bedrijf", nameof(CallData.Bedrijf), 230);
            _dgCallData.CreateTextColumn($"Subscriber", nameof(CallData.Subscriber), 150);
            _dgCallData.CreateTextColumn($"Originator", nameof(CallData.Originator), 150);
            _dgCallData.CreateTextColumn($"Target", nameof(CallData.Target), 150);
            _dgCallData.CreateTextColumn($"Datum", nameof(CallData.Date), 200);
            _dgCallData.CreateTextColumn($"Bel tijd", nameof(CallData.Duration), 150);
            _dgCallData.CreateTextColumn($"Kosten", nameof(CallData.Cost), 150);
            _dgCallData.CreateTextColumn($"Kosten met marge", nameof(CallData.CostWithMarge), 150);
            _dgCallData.UseVisualTemplateLines();
            #endregion

            #region MonthlyCostData DataGrid
            // DataGrid met alle bedrijven en maandelijkse info
            _dgMonthlyCostData = new DataGridControl<MonthlyCostData>(dgMonthlyCostData, true);
            _dgMonthlyCostData.CreateTextColumn($"Jaar/Maand", nameof(MonthlyCostData.YearOrMonth), 100);
            _dgMonthlyCostData.CreateTextColumn($"Bedrijf", nameof(MonthlyCostData.CompanyName), 230);
            _dgMonthlyCostData.CreateTextColumn($"Subscriber", nameof(MonthlyCostData.Subscriber), 95);
            _dgMonthlyCostData.CreateTextColumn($"Belletjes", nameof(MonthlyCostData.TotalCalls), 70);
            _dgMonthlyCostData.CreateTextColumn($"Ins", nameof(MonthlyCostData.TotalIns), 50);
            _dgMonthlyCostData.CreateTextColumn($"Outs", nameof(MonthlyCostData.TotalOuts), 50);
            _dgMonthlyCostData.CreateTextColumn($"Bel tijd", nameof(MonthlyCostData.TotalCallTime), 90);
            _dgMonthlyCostData.CreateTextColumn($"Bedrag", nameof(MonthlyCostData.TotalAmount), 65);
            _dgMonthlyCostData.CreateTextColumn($"Start tarief mobiel", nameof(MonthlyCostData.StartRateMobiel), 120);
            _dgMonthlyCostData.CreateTextColumn($"Per minuut mobiel", nameof(MonthlyCostData.CostPerMinuteMobiel), 120);
            _dgMonthlyCostData.CreateTextColumn($"Start tarief binnenland", nameof(MonthlyCostData.StartRateBinnenLand), 135);
            _dgMonthlyCostData.CreateTextColumn($"Per minuut binnenland", nameof(MonthlyCostData.CostPerMinuteBinnenLand), 135);
            _dgMonthlyCostData.CreateTextColumn($"Vanaf", nameof(MonthlyCostData.FirstDate), 130);
            _dgMonthlyCostData.CreateTextColumn($"Tot", nameof(MonthlyCostData.LastDate), 130);
            _dgMonthlyCostData.CreateTextColumn($"Tijds periode", nameof(MonthlyCostData.TimePeriod), 90);
            _dgMonthlyCostData.CreateTextColumn($"Voorschot", nameof(MonthlyCostData.Voorschot), 80);
            _dgMonthlyCostData.CreateTextColumn($"Tekort voorschot", nameof(MonthlyCostData.TekortVoorschot), 110);
            _dgMonthlyCostData.UseVisualTemplateLines();
            _dgMonthlyCostData.ActionRowColoring = ColorMonthlyRow;
            #endregion

            _lbCompanies = new ListBoxControl<CompanyType>(lbAllCompanies);
            _lbSubscribers = new ListBoxControl<SubscriberData>(lbSubscribersOfCompany);
            _lbCompanyIds = new ListBoxControl<CompanyIdType>(lbAllCompanyIds);

            btnCompanyMarges.IsEnabled = false;
            btnPerMaand.IsEnabled = false;
            cbUseMarge.IsChecked = true;
            btnPerJaar.IsEnabled = false;

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        /// <summary>
        /// Werkte niet
        /// </summary>
        /// <param name="argDataRow"></param>
        public void ColorMonthlyRow(DataGridRow argDataRow)
        {
            var tempMonthlyCost = (MonthlyCostData)argDataRow.Item;

            if (tempMonthlyCost.TekortVoorschot != "")
            {
                argDataRow.Background = Brushes.Red;
                argDataRow.Foreground = Brushes.White;
            }
        }

        /// <summary>
        /// Logging naar het venster
        /// </summary>
        /// <param name="argText"></param>
        public void LogToTextBox(string argText)
        {
            if (string.IsNullOrEmpty(argText)) return;

            App.Current.Dispatcher?.Invoke(() =>
            {
                _stringBuilderLogging.Append(argText);

                tbLogs.Text = _stringBuilderLogging.ToString();
            });
            tbLogs.ScrollToEnd();
        }

        /// <summary>
        /// Functie die drie dingen tegelijk doet en verdeeld moet worden;
        /// Pakt geselecteerde bedrijven / subscribers;
        /// Marges uitrekenen voor calldata gerelateerd aan bedrijven / subscribers;
        /// Samenvatting van belkosten maken per telefoonnummer per bedrijf
        /// </summary>
        private async void ProcessCompanySubscribersWithMarge()
        {
            // Selectie companies / subscribers opvragen
            _selectedCompanyType = _lbCompanies.GetSelections();
            _selectedSubscriberType = _lbSubscribers.GetSelections();


            if (_selectedSubscriberType.Count == 0)
            {
                MBox.ShowWarning("Geen Subscriber gekozen");
                return;
            }

            if (_selectedCompanyType.Count == 0)
            {
                MBox.ShowWarning("Geen Bedrijf gekozen");
                return;
            }

            btnPerJaar.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF707070");
            btnPerMaand.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF707070");

            int tempFirstRunCount = 0;
            var tempAllSelectedCompanyCallData = new List<CallData>();
            var tempAllCompanySubscriberCallData = new List<CallData>();


            // Filter op alleen geselecteerde bedrijven
            foreach (var tempCompany in _selectedCompanyType)
            {
                var tempCompanyCallData = _listAllCallData.FindAll(x => x.Bedrijf == tempCompany.strCompany);
                tempAllSelectedCompanyCallData.AddRange(tempCompanyCallData);
            }

            // Filter vervolgens nog op alleen geselecteerde telefoonnummers
            foreach (var tempCompanySubscriber in _selectedSubscriberType)
            {
                var tempCompanyCallData = tempAllSelectedCompanyCallData.FindAll(x => x.Subscriber == tempCompanySubscriber.Subscriber);
                tempAllCompanySubscriberCallData.AddRange(tempCompanyCallData);
            }


            // Marges uitrekenen en samenvatting maken

            string tempFirstDate = string.Empty;
            string tempLastDate = string.Empty;
            TimeSpan tempTimePeriod = TimeSpan.Parse("00:00:00");

            List<MonthlyCostData> tempMontlyCostDataList = new List<MonthlyCostData>();

            double tempMarge = 0;

            foreach (var Company in _selectedCompanyType)
            {


                // Voor elk bedrijf
                if (tempFirstRunCount == 0)
                    tbLogs.Clear();
                tempFirstRunCount++;
                if (tempMarge > 0 && (bool)cbUseMarge.IsChecked)
                {
                    foreach (var tempCallDataCostWithMarge in tempAllSelectedCompanyCallData)
                    {
                        tempCallDataCostWithMarge.Marge = tempMarge;
                        tempCallDataCostWithMarge.CostWithMarge = tempCallDataCostWithMarge.Cost * ((tempMarge + 100) / 100);
                    }
                }

                // Samenvatting maken per bedrijf per telefoonnummer

                // Per bedrijf de CallData opzoeken
                var tempSelectedCompanyCallData = tempAllCompanySubscriberCallData.FindAll(x => x.Bedrijf == Company.strCompany);

                // Zoek unieke telefoonnummers op
                var tempUniqueSelectedPhoneNumbers = LogicManager.LogicManager.GetUniqueSubscribers(tempSelectedCompanyCallData);

                this.IsEnabled = false;
                this.Cursor = Cursors.Wait;
                foreach (var tempSelectedPhoneNumber in tempUniqueSelectedPhoneNumbers)
                {
                    // Vind alle calldata voor unieke telefoonnummer.
                    var tempSelectedCompanySubscriberCallData = tempSelectedCompanyCallData.FindAll(x => x.Subscriber == tempSelectedPhoneNumber.Subscriber);

                    MonthlyCostData monthlyCostData = await LogicManager.LogicManager.ParseAllCallData(tempSelectedCompanySubscriberCallData, tempMarge, dVoorschot);

                    tempMontlyCostDataList.Add(monthlyCostData);
                }
                this.Cursor = null;
                this.IsEnabled = true;
            }

            _dgCallData.SetDataSource(tempAllSelectedCompanyCallData);

            btnCompanyMarges.IsEnabled = true;
            btnPerMaand.IsEnabled = true;
            btnPerJaar.IsEnabled = true;
        }

        private async void ProcessCompaniesWithMarge()
        {
            // Selectie companies / subscribers opvragen
            _selectedCompanyType = _lbCompanies.GetSelections();

            if (_selectedCompanyType.Count == 0)
            {
                MBox.ShowWarning("Geen Bedrijf gekozen");
                return;
            }

            btnPerJaar.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF707070");
            btnPerMaand.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF707070");

            int tempFirstRunCount = 0;
            var tempAllSelectedCompanyCallData = new List<CallData>();

            // Filter op alleen geselecteerde bedrijven
            foreach (var tempCompany in _selectedCompanyType)
            {
                var tempCompanyCallData = _listAllCallData.FindAll(x => x.Bedrijf == tempCompany.strCompany);
                tempAllSelectedCompanyCallData.AddRange(tempCompanyCallData);
            }

            // Marges uitrekenen en samenvatting maken

            string tempFirstDate = string.Empty;
            string tempLastDate = string.Empty;
            TimeSpan tempTimePeriod = TimeSpan.Parse("00:00:00");

            List<MonthlyCostData> tempMontlyCostDataList = new List<MonthlyCostData>();

            double tempMarge = 0;

            foreach (var Company in _selectedCompanyType)
            {
                // Voor elk bedrijf
                if (tempFirstRunCount == 0)
                    tbLogs.Clear();
                tempFirstRunCount++;
                if (tempMarge > 0 && (bool)cbUseMarge.IsChecked)
                {
                    foreach (var tempCallDataCostWithMarge in tempAllSelectedCompanyCallData)
                    {
                        tempCallDataCostWithMarge.Marge = tempMarge;
                        tempCallDataCostWithMarge.CostWithMarge = tempCallDataCostWithMarge.Cost * ((tempMarge + 100) / 100);
                    }
                }

                // Samenvatting maken per bedrijf per telefoonnummer

                // Per bedrijf de CallData opzoeken
                var tempSelectedCompanyCallData = tempAllSelectedCompanyCallData.FindAll(x => x.Bedrijf == Company.strCompany);

                // Zoek unieke telefoonnummers op
                var tempUniqueSelectedCompanies = LogicManager.LogicManager.GetUniqueCompanies(tempSelectedCompanyCallData);

                this.IsEnabled = false;
                this.Cursor = Cursors.Wait;
                foreach (var tempSelectedPhoneNumber in tempUniqueSelectedCompanies)
                {
                    // Vind alle calldata voor unieke telefoonnummer.
                    var tempSelectedCompanySubscriberCallData = tempSelectedCompanyCallData.FindAll(x => x.Bedrijf == tempSelectedPhoneNumber.strCompany);

                    MonthlyCostData monthlyCostData = await LogicManager.LogicManager.ParseAllCallData(tempSelectedCompanySubscriberCallData, tempMarge, dVoorschot);

                    tempMontlyCostDataList.Add(monthlyCostData);
                }
                this.Cursor = null;
                this.IsEnabled = true;
            }
            _dgCallData.SetDataSource(tempAllSelectedCompanyCallData);

            btnCompanyMarges.IsEnabled = true;
            btnPerMaand.IsEnabled = true;
            btnPerJaar.IsEnabled = true;
        }
        public static void SaveCompanyMarges(List<CompanyType> argSelectedCompanyList, double tempMarge)
        {
            List<CompanyMargesType> ListSavedCompanyMarges = new List<CompanyMargesType>();

            ListSavedCompanyMarges = DataManager.DataManager.ReadCompanyDataFile() ?? new List<CompanyMargesType>();

            foreach (var tempCompany in argSelectedCompanyList)
            {
                if (ListSavedCompanyMarges != null)
                {
                    var tempFoundCompanyMarge = ListSavedCompanyMarges.Find(x => x.CompanyName == tempCompany.strCompany);

                    if (tempFoundCompanyMarge != null)
                        tempFoundCompanyMarge.CompanyMarge = tempMarge;
                    else
                    {
                        ListSavedCompanyMarges.Add(new CompanyMargesType()
                        {
                            CompanyName = tempCompany.strCompany,
                            CompanyMarge = tempMarge
                        });
                    }
                }
                else
                {
                    ListSavedCompanyMarges.Add(new CompanyMargesType()
                    {
                        CompanyName = tempCompany.strCompany,
                        CompanyMarge = tempMarge
                    });
                }

                DataManager.DataManager.WriteCompanyDataToFile(ListSavedCompanyMarges);
            }
        }

        public static void SetProgress(int argValue, int argMaximum = -1)
        {
            // Nodig indien je de functie aanroept vanaf een andere thread.
            App.Current.Dispatcher?.Invoke(() =>
            {
                if (argMaximum > 0)
                    MainWindow.referenceProgressBar.Maximum = argValue;

                MainWindow.referenceProgressBar.Value = argValue;
            });
        }

        /// <summary>
        /// Selecteer Excel bestanden om te verwerken in applicatie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tempOpenFileDialog = new OpenFileDialog();
                tempOpenFileDialog.Multiselect = true;
                if (!(bool)tempOpenFileDialog.ShowDialog()) return;

                this.IsEnabled = false;
                this.Cursor = Cursors.Wait;

                _listAllFilePaths.Clear();
                DeleteAllData();
                pbProgress.Value = 0;
                pbProgress.Minimum = 0;
                pbProgress.Maximum = tempOpenFileDialog.FileNames.Count();
                _listAllFilePaths.AddRange(tempOpenFileDialog.FileNames);

                int tempProgressCount = 0;

                await Task.Run(() =>
                {
                    foreach (var file in tempOpenFileDialog.FileNames)
                    {
                        // Lijst met geselecteerde bestanden zichtbaar maken in Listbox
                        App.Current.Dispatcher?.Invoke(() =>
                        {
                            lbAllSelectedFiles.Items.Add(file.Substring(file.LastIndexOf(@"\") + 1));
                        });

                        var tempFilePath = file;
                        List<CallData> tempCallDataList = LogicManager.LogicManager.GetAllParsedRowsList(tempFilePath);
                        List<CompanyType> tempCompaniesList = LogicManager.LogicManager.GetUniqueCompanies(tempCallDataList);
                        List<MonthType> tempMonthsList = LogicManager.LogicManager.GetUniqueMonths(tempCallDataList);
                        List<SubscriberData> tempSubscribersList = LogicManager.LogicManager.GetUniqueSubscribers(tempCallDataList);

                        // Verwerkte data van bestand toevoegen aan de hoofdlijsten
                        _listAllCallData.AddRange(tempCallDataList);
                        _listAllCompanies.AddRange(tempCompaniesList);
                        _listAllMonths.AddRange(tempMonthsList.Where(x => _listAllMonths.FirstOrDefault(y => y.intMonth == x.intMonth) == null).ToList());
                        _listAllSubscriber.AddRange(tempSubscribersList);

                        tempProgressCount++;
                        SetProgress(tempProgressCount);
                    }
                });
                var tempAllSavedCompanyData = DataManager.DataManager.ReadCompanyDataFile();
                if (tempAllSavedCompanyData != null && tempAllSavedCompanyData.Count > 0)
                {
                    var tempListCompaniesSavedAndNew = new List<CompanyMargesType>();
                    var tempListCompaniesWithNickname = new List<CompanyMargesType>();
                    List<CompanyIdType> tempAllFoundIdsList = new List<CompanyIdType>();

                    foreach (var tempCompany in _listAllCompanies)
                    {
                        if (tempCompany.strCompany == tempCompany.strSubscriber)
                        {
                            var tempFoundCompany = tempAllSavedCompanyData.Find(x => x.CompanyName == tempCompany.strCompany);

                            if (tempFoundCompany == null)
                                continue;

                            foreach (var tempCall in _listAllCallData)
                            {
                                if (tempCall.Bedrijf == tempFoundCompany.CompanyName)
                                {
                                    if (tempFoundCompany.CompanyId == null)
                                        tempFoundCompany.CompanyId = tempCall.ID;
                                    if (tempCompany.strId == null)
                                    {
                                        tempCompany.strId = tempCall.ID;
                                        tempCompany.strTarget = tempCall.Target;
                                    }
                                    else break;
                                }
                            }
                        }
                    }

                    foreach (var tempCompany in _listAllCompanies)
                    {
                        var tempFoundCompany = tempAllSavedCompanyData.Find(x => x.CompanyName == tempCompany.strCompany);
                        if (tempFoundCompany != null)
                        {
                            // object bestaat al
                            tempListCompaniesSavedAndNew.Add(tempFoundCompany);
                        }
                        else
                        {
                            tempListCompaniesSavedAndNew.Add(new CompanyMargesType() { CompanyName = tempCompany.strCompany, CompanyId = tempCompany.strId, CompanyNickname = tempCompany.strNickname });
                        }
                    }
                    foreach (var tempCompany in _listAllCompanies)
                    {
                        var tempFoundCompany = tempListCompaniesSavedAndNew.Find(x => x.CompanyName == tempCompany.strCompany && !string.IsNullOrEmpty(x.CompanyNickname));
                        if (tempFoundCompany != null)
                        {
                            tempCompany.strNickname = tempFoundCompany.CompanyNickname;
                            tempCompany.strId = tempFoundCompany.CompanyId;
                            //  tempListCompaniesWithNickname.Add(tempFoundCompany);
                        }
                    }

                    int tempNoIdCount = 0;
                    List<CompanyMargesType> tempNoIdList = new List<CompanyMargesType>();
                    foreach (var tempCompany in tempListCompaniesSavedAndNew)
                    {
                        if (string.IsNullOrEmpty(tempCompany.CompanyId))
                        {
                            tempNoIdCount++;
                            tempNoIdList.Add(tempCompany);
                        }
                    }
                    if (tempNoIdCount > 0)
                    {
                        if (tempNoIdCount > 5)
                        {
                            MBox.ShowInformation("Er zijn meerdere bedrijven zonder een ID!");
                        }
                        else
                        {
                            foreach (var tempNoName in tempNoIdList)
                            {
                                MBox.ShowInformation($"{tempNoName.CompanyName} \nHeeft geen ID.");
                            }
                        }
                    }

                    foreach (var tempSavedCompany in tempListCompaniesSavedAndNew)
                    {
                        if (tempSavedCompany.CompanyId != null)
                            tempAllFoundIdsList.Add(new CompanyIdType() { CompanyId = tempSavedCompany.CompanyId, CompanyNickName = tempSavedCompany.CompanyNickname });
                    }

                    List<CompanyIdType> tempCompanyIdsList = tempAllFoundIdsList
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
                }
                /*
                foreach (var Company in _listAllCompanies)
                {
                    var tempMissingCompaniesInFile = tempAllSavedCompanyData.FindAll(x => x.CompanyName == Company.strCompany);
                }*/
                SetProgress(0);


                _listAllCompanies = LogicManager.LogicManager.ParseListAllCompanies(_listAllCompanies);
                _listAllCompanies = _listAllCompanies.OrderBy(x => x.strCompany).ToList();
                _lbCompanies.SetItemsSource(_listAllCompanies);

                btnPerJaar.IsEnabled = true;
                btnPerMaand.IsEnabled = true;
                btnCompanyMarges.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MBox.ShowWarning(ex.Message);
            }
            finally
            {
                this.Cursor = null;
                this.IsEnabled = true;
            }
        }
        /// <summary>
        /// Verwijderd alle verwerkte data van een eerdere sessie
        /// </summary>
        public void DeleteAllData()
        {
            _listAllCallData.Clear();
            _listAllCompanies.Clear();
            _listAllMonths.Clear();
            _listAllCompanyIds.Clear();
            _listAllMonths.Clear();
            _listAllSubscriber.Clear();

            lbAllSelectedFiles.Items.Clear();
            _lbSubscribers.SetItemsSource(null);
            _lbCompanies.SetItemsSource(null);
            _lbCompanyIds.SetItemsSource(null);
            _dgCallData.SetDataSource(null);
            _dgMonthlyCostData.SetDataSource(null);

            tbLogs.Clear();
        }
        private void lbAllBusinesses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var SelectedCompanyList = _lbCompanies.GetSelections();
            if (SelectedCompanyList.Count == 0)
                _lbSubscribers.SetItemsSource(null);

            SetButtonsBorderRed();

            foreach (var tempCompany in SelectedCompanyList)
            {
                List<SubscriberData> CompanySubscribersList = GetSubscribersOfCompany(tempCompany);
                _lbSubscribers.SetItemsSource(CompanySubscribersList);
            }
        }
        public List<SubscriberData> GetSubscribersOfCompany(CompanyType Company)
        {
            _selectedCompanyType = _lbCompanies.GetSelections();

            var tempAllSelectedCompanyCallData = new List<CallData>();

            foreach (var selectedCompany in _selectedCompanyType)
            {
                var tempCompanyCallData = _listAllCallData.FindAll(x => x.Bedrijf == selectedCompany.strCompany);
                tempAllSelectedCompanyCallData.AddRange(tempCompanyCallData);
            }

            // Zoek unieke telefoonnummers op
            return LogicManager.LogicManager.GetUniqueSubscribers(tempAllSelectedCompanyCallData);

        }
        private async void btnPerMaand_Click(object sender, RoutedEventArgs e)
        {
            _selectedCompanyType = _lbCompanies.GetSelections();
            _selectedSubscriberType = _lbSubscribers.GetSelections();
            if (_selectedSubscriberType == null)
            {
                MBox.ShowWarning("Geen telefoon nummer gekozen.");
                return;
            }
            _stringBuilderLogging.Clear();

            ProcessCompanySubscribersWithMarge();

            List<CallData> tempAllCompanyCallData = new List<CallData>();
            List<CallData> tempAllCompanySubscriberCallData = new List<CallData>();
            List<MonthlyCostData> listMonthlyCompanyCallData = new List<MonthlyCostData>();

            var tempSavedCompanyDataList = DataManager.DataManager.ReadCompanyDataFile();

            int totalCount = _selectedCompanyType.Count() + _selectedSubscriberType.Count() + _selectedCompanyType.Count();
            pbProgress.Maximum = totalCount;
            pbProgress.Value = 0;
            pbProgress.Minimum = 0;
            int tempProgress = 0;

            foreach (var tempCompany in _selectedCompanyType)
            {
                this.IsEnabled = false;
                this.Cursor = Cursors.Wait;
                tempProgress++;
                var tempCompanyCallData = _listAllCallData.FindAll(x => x.Bedrijf == tempCompany.strCompany);
                tempAllCompanyCallData.AddRange(tempCompanyCallData);
                SetProgress(tempProgress);

                await Task.Delay(50);
            }

            foreach (var CompanySubscriber in _selectedSubscriberType)
            {
                var tempCompanyCall = tempAllCompanyCallData.FindAll(x => x.Subscriber == CompanySubscriber.Subscriber);
                tempAllCompanySubscriberCallData.AddRange(tempCompanyCall);
            }

            foreach (var Company in _selectedCompanyType)
            {
                tempProgress++;

                if ((bool)cbUseMarge.IsChecked)
                {
                    var templist = DataManager.DataManager.ReadCompanyDataFile();
                    if (templist != null && templist.Count > 0)
                    {
                        var tempCompany = templist.Find(x => x.CompanyName == Company.strCompany);
                        if (tempCompany != null)
                        {
                            dMarge = tempCompany.CompanyMarge;
                            dVoorschot = tempCompany.CompanyVoorschot;
                        }
                        else
                        {
                            dMarge = 1;
                            dVoorschot = 0;
                        }
                    }
                    else
                    {
                        dMarge = 1;
                        dVoorschot = 0;
                    }
                }
                if (dMarge > 0 && (bool)cbUseMarge.IsChecked)
                {
                    foreach (var tempCostWithMarge in tempAllCompanyCallData)
                    {
                        tempCostWithMarge.CostWithMarge = tempCostWithMarge.Cost * ((dMarge + 100) / 100);
                        _dgCallData.SetDataSource(tempAllCompanyCallData);
                    }
                }
                else
                    _dgCallData.SetDataSource(tempAllCompanyCallData);

                // Per bedrijf de CallData opzoeken
                var tempSelectedCompanyCallData = tempAllCompanySubscriberCallData.FindAll(x => x.Bedrijf == Company.strCompany);

                // Zoek unieke telefoonnummers op
                var tempUniqueSelectedPhoneNumbers = LogicManager.LogicManager.GetUniqueSubscribers(tempSelectedCompanyCallData);


                foreach (var tempSelectedPhoneNumber in tempUniqueSelectedPhoneNumbers)
                {
                    // Vind alle calldata voor unieke telefoonnummer.
                    var tempSelectedCompanySubscriberCallData = tempSelectedCompanyCallData.FindAll(x => x.Subscriber == tempSelectedPhoneNumber.Subscriber);

                    var templistMonthlyCompanyCallData2 = await LogicManager.LogicManager.GetMonthlyCostDataForCompany(tempSelectedPhoneNumber, _listAllMonths, tempSelectedCompanyCallData, dMarge, dVoorschot);


                    listMonthlyCompanyCallData.AddRange(templistMonthlyCompanyCallData2);
                }
                SetProgress(tempProgress);

                await Task.Delay(50);
            }
            this.Cursor = null;
            this.IsEnabled = true;
            _dgMonthlyCostData.SetDataSource(listMonthlyCompanyCallData);
            SetProgress(0);
        }
        private async void btnPerJaar_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cbNucall.IsChecked)
            {
                PerJaarNucall();
                return;
            }
            ProcessCompanySubscribersWithMarge();
            _selectedSubscriberType = _lbSubscribers.GetSelections();
            if (_selectedSubscriberType == null)
            {
                MBox.ShowWarning("Geen telefoon nummer gekozen.");
            }
            _stringBuilderLogging.Clear();
            int tempFirstRunCount = 0;
            var tempAllSelectedCompanyCallData = new List<CallData>();
            var tempAllCompanySubscriberCallData = new List<CallData>();

            // Filter op allene geselecteerde bedrijven
            foreach (var Company in _selectedCompanyType)
            {
                var tempCompanyCallData = _listAllCallData.FindAll(x => x.Bedrijf == Company.strCompany);
                tempAllSelectedCompanyCallData.AddRange(tempCompanyCallData);
            }

            // Filter vervolgens nog op alleen geselecteerde telefoonnummers
            foreach (var CompanySubscriber in _selectedSubscriberType)
            {
                var tempCompanyCall = tempAllSelectedCompanyCallData.FindAll(x => x.Subscriber == CompanySubscriber.Subscriber);
                tempAllCompanySubscriberCallData.AddRange(tempCompanyCall);
            }

            string tempFirstDate = string.Empty;
            string tempLastDate = string.Empty;
            TimeSpan tempTimePeriod = TimeSpan.Parse("00:00:00");

            List<MonthlyCostData> tempMontlyCostDataList = new List<MonthlyCostData>();
            int tempProgress = 0;
            pbProgress.Value = 0;
            pbProgress.Minimum = 0;
            pbProgress.Maximum = _selectedCompanyType.Count();


            foreach (var Company in _selectedCompanyType)
            {
                this.IsEnabled = false;
                this.Cursor = Cursors.Wait;
                tempProgress++;
                // Voor elk bedrijf
                if (tempFirstRunCount == 0)
                    tbLogs.Clear();
                tempFirstRunCount++;
                if ((bool)cbUseMarge.IsChecked)
                {
                    var templist = DataManager.DataManager.ReadCompanyDataFile();
                    if (templist != null && templist.Count > 0)
                    {
                        var tempCompany = templist.Find(x => x.CompanyName == Company.strCompany);
                        if (tempCompany != null)
                        {
                            dMarge = tempCompany.CompanyMarge;
                            dVoorschot = tempCompany.CompanyVoorschot;
                        }
                        else
                        {
                            dMarge = 1;
                            dVoorschot = 0;
                        }
                    }
                    else
                    {
                        dMarge = 1;
                        dVoorschot = 0;
                    }
                }

                if (dMarge > 0 && (bool)cbUseMarge.IsChecked)
                {
                    foreach (var tempCostWithMarge in tempAllSelectedCompanyCallData)
                    {
                        tempCostWithMarge.CostWithMarge = tempCostWithMarge.Cost * ((dMarge + 100) / 100);
                        _dgCallData.SetDataSource(tempAllSelectedCompanyCallData);
                    }
                }
                else
                    _dgCallData.SetDataSource(tempAllSelectedCompanyCallData);

                if (dVoorschot > 0)
                {
                    dVoorschot = dVoorschot * 12;
                }
                // Per bedrijf de CallData opzoeken
                var tempSelectedCompanyCallData = tempAllCompanySubscriberCallData.FindAll(x => x.Bedrijf == Company.strCompany);

                // Zoek unieke telefoonnummers op
                var tempUniqueSelectedPhoneNumbers = LogicManager.LogicManager.GetUniqueSubscribers(tempSelectedCompanyCallData);



                foreach (var tempSelectedPhoneNumber in tempUniqueSelectedPhoneNumbers)
                {
                    // Vind alle calldata voor unieke telefoonnummer.
                    var tempSelectedCompanySubscriberCallData = tempSelectedCompanyCallData.FindAll(x => x.Subscriber == tempSelectedPhoneNumber.Subscriber);

                    MonthlyCostData monthlyCostData = await LogicManager.LogicManager.ParseAllCallData(tempSelectedCompanySubscriberCallData, dMarge, dVoorschot);

                    monthlyCostData.Subscriber = tempSelectedPhoneNumber.Subscriber;

                    tempMontlyCostDataList.Add(monthlyCostData);
                }
                SetProgress(tempProgress);
                await Task.Delay(50);
            }
            this.Cursor = null;
            this.IsEnabled = true;
            Thread.Sleep(1000);
            SetProgress(0);
            _dgMonthlyCostData.SetDataSource(tempMontlyCostDataList);
        }

        private async void PerJaarNucall()
        {
            ProcessCompaniesWithMarge();
            _selectedCompanyType = _lbCompanies.GetSelections();
            if (_selectedCompanyType == null)
            {
                MBox.ShowWarning("Geen bedrijf gekozen.");
            }
            _stringBuilderLogging.Clear();
            int tempFirstRunCount = 0;
            var tempAllSelectedCompanyCallData = new List<CallData>();

            // Filter op allene geselecteerde bedrijven
            foreach (var Company in _selectedCompanyType)
            {
                var tempCompanyCallData = _listAllCallData.FindAll(x => x.Bedrijf == Company.strCompany);
                tempAllSelectedCompanyCallData.AddRange(tempCompanyCallData);
            }

            string tempFirstDate = string.Empty;
            string tempLastDate = string.Empty;
            TimeSpan tempTimePeriod = TimeSpan.Parse("00:00:00");

            List<MonthlyCostData> tempMontlyCostDataList = new List<MonthlyCostData>();
            int tempProgress = 0;
            pbProgress.Value = 0;
            pbProgress.Minimum = 0;
            pbProgress.Maximum = _selectedCompanyType.Count();

            foreach (var Company in _selectedCompanyType)
            {
                this.IsEnabled = false;
                this.Cursor = Cursors.Wait;
                tempProgress++;
                // Voor elk bedrijf
                if (tempFirstRunCount == 0)
                    tbLogs.Clear();
                tempFirstRunCount++;
                if ((bool)cbUseMarge.IsChecked)
                {
                    var templist = DataManager.DataManager.ReadCompanyDataFile();
                    if (templist != null && templist.Count > 0)
                    {
                        var tempCompany = templist.Find(x => x.CompanyName == Company.strCompany);
                        if (tempCompany != null)
                        {
                            dMarge = tempCompany.CompanyMarge;
                            dVoorschot = tempCompany.CompanyVoorschot;
                        }
                        else
                        {
                            dMarge = 1;
                            dVoorschot = 0;
                        }
                    }
                    else
                    {
                        dMarge = 1;
                        dVoorschot = 0;
                    }
                }

                if (dMarge > 0 && (bool)cbUseMarge.IsChecked)
                {
                    foreach (var tempCostWithMarge in tempAllSelectedCompanyCallData)
                    {
                        tempCostWithMarge.CostWithMarge = tempCostWithMarge.Cost * ((dMarge + 100) / 100);
                        _dgCallData.SetDataSource(tempAllSelectedCompanyCallData);
                    }
                }
                else
                    _dgCallData.SetDataSource(tempAllSelectedCompanyCallData);

                if (dVoorschot > 0)
                {
                    dVoorschot = dVoorschot * 12;
                }
                // Per bedrijf de CallData opzoeken
                var tempSelectedCompanyCallData = tempAllSelectedCompanyCallData.FindAll(x => x.Bedrijf == Company.strCompany);

                // Zoek unieke telefoonnummers op
                var tempUniqueSelectedPhoneNumbers = LogicManager.LogicManager.GetUniqueCompanies(tempSelectedCompanyCallData);

                foreach (var tempSelectedPhoneNumber in tempUniqueSelectedPhoneNumbers)
                {
                    // Vind alle calldata voor unieke telefoonnummer.
                    var tempSelectedCompanySubscriberCallData = tempSelectedCompanyCallData.FindAll(x => x.Bedrijf == tempSelectedPhoneNumber.strCompany);

                    MonthlyCostData monthlyCostData = await LogicManager.LogicManager.ParseAllCallData(tempSelectedCompanySubscriberCallData, dMarge, dVoorschot);

                    monthlyCostData.CompanyName = tempSelectedPhoneNumber.strCompany;

                    tempMontlyCostDataList.Add(monthlyCostData);
                }
                SetProgress(tempProgress);
                await Task.Delay(50);
            }
            this.Cursor = null;
            this.IsEnabled = true;
            Thread.Sleep(1000);
            SetProgress(0);
            _dgMonthlyCostData.SetDataSource(tempMontlyCostDataList);
        }

        private void cbUseMarge_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cbUseMarge.IsChecked)
            {
                CallData callData = new CallData();
                callData.Marge = dMarge;
            }
            if (!(bool)cbUseMarge.IsChecked)
            {
                dMarge = 0;
            }
        }

        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void btnCompanyMarges_Click(object sender, RoutedEventArgs e)
        {
            MargeManagerWindow tempWindow = new MargeManagerWindow() { Owner = this };
            tempWindow.ShowDialog();
        }

        private void btnTargets_Click(object sender, RoutedEventArgs e)
        {
            TargetManagerWindow tempWindow = new TargetManagerWindow() { Owner = this };
            tempWindow.ShowDialog();
        }
        private void lbSubscribersOfCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetButtonsBorderRed();
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            var tempMonthlyCostDataList = _dgMonthlyCostData.GetDataSource();
            var tempAllSelectedCompanyCallData = _dgCallData.GetDataSource();
            bool MakeCallData = false;
            bool MakeMonthly = false;
            bool ResizeCollums = false;

            if (cbExportCallData.IsChecked == true)
                MakeCallData = true;

            if (cbExportMonthlyData.IsChecked == true)
                MakeMonthly = true;

            if (cbResizeCollums.IsChecked == true)
                ResizeCollums = true;

            if (MakeMonthly == true || MakeCallData == true)
                DataManager.DataManager.ExportCallDataToExcelFile(tempAllSelectedCompanyCallData, tempMonthlyCostDataList, MakeCallData, MakeMonthly, ResizeCollums);
        }

        #region Select and Deselect all
        private async void btnSelectAllCompanies_Click(object sender, RoutedEventArgs e)
        {
            await SelectAllCompanies();
        }
        private async Task SelectAllCompanies()
        {
            lbAllCompanies.SelectAll();
        }
        private void btnDeselectAllCompanies_Click(object sender, RoutedEventArgs e)
        {
            lbAllCompanies.UnselectAll();
        }

        private void btnSelectAllSubscribers_Click(object sender, RoutedEventArgs e)
        {
            lbSubscribersOfCompany.SelectAll();
        }

        private void btnDeselectAllSubscribers_Click(object sender, RoutedEventArgs e)
        {
            lbSubscribersOfCompany.UnselectAll();
        }

        private void btnSelectAllCompanyIds_Click(object sender, RoutedEventArgs e)
        {
            lbAllCompanyIds.SelectAll();
        }

        private void btnDeselectAllCompanyIds_Click(object sender, RoutedEventArgs e)
        {
            lbAllCompanyIds.UnselectAll();
            _lbCompanies.SetItemsSource(_listAllCompanies);
        }

        #endregion

        /// <summary>
        /// Functie voor als er een selectie is veranderd de knoppen per jaar / per maand een rode outline geeft.
        /// </summary>
        private void SetButtonsBorderRed()
        {
            btnPerJaar.BorderBrush = Brushes.Red;
            btnPerMaand.BorderBrush = Brushes.Red;
        }

        /// <summary>
        /// Als de selectie van de Company ID listbox changed dan krijg je aleen de company van die id te zien
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbAllCompanyIds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<CompanyType> tempCompanyNamesList = new List<CompanyType>();
            List<CompanyIdType> tempCompanyNamesOfIdList = new List<CompanyIdType>();

            SetButtonsBorderRed();

            _selectedCompanyIdType = _lbCompanyIds.GetSelections();

            // Als er geen selectie is dan zet de originele companies in de listbox
            if (_selectedCompanyIdType.Count == 0)
            {
                _lbCompanies.SetItemsSource(_listAllCompanies);
                return;
            }

            // loop om door alle geselecteerde company ids te gaan en voegen aan een list
            foreach (var tempCompantId in _selectedCompanyIdType)
            {
                var tempCompanyIdList = GetCompaniesWithId(tempCompantId);
                tempCompanyNamesOfIdList.AddRange(tempCompanyIdList);
            }

            // Extra controle dat elke bedrijfsnaam wel aanwezig is in de bedrijfsnamen geselecteerd in de bestanden
            foreach (var tempCompanyName in tempCompanyNamesOfIdList)
            {
                if (_listAllCompanies.Any(x => x.strCompany == tempCompanyName.CompanyName))
                    tempCompanyNamesList.Add(new CompanyType() { strCompany = tempCompanyName.CompanyName });
            }

            // Toon companies in ListBox
            _lbCompanies.SetItemsSource(tempCompanyNamesList);
        }

        /// <summary>
        /// Haalt alle unieke ID's op, zoekt vervolgens alle bedrijven daarvan met dezelfde ID als argCompanyID;
        /// Als laatste wordt gefilterd op dat bedrijf wel aanwezig is in geselecteerde Excel bestanden.
        /// </summary>
        /// <param name="argCompanyId"></param>
        /// <returns></returns>
        public static List<CompanyIdType> GetCompaniesWithId(CompanyIdType argCompanyId)
        {
            List<CompanyIdType> tempFoundCompanyNamesOfId = new List<CompanyIdType>();
            var tempSavedCompanyData = DataManager.DataManager.ReadCompanyDataFile();
            // Functie die alle unieke IDs pakt
            var tempAllUniqueCompanyIdsList = LogicManager.LogicManager.GetUniqueCompanyIds();

            // Loop om door alle ids te gaan en alle company names te pakken en daarna in een list te zetten
            foreach (var tempId in tempAllUniqueCompanyIdsList)
            {
                if (tempId.CompanyId == argCompanyId.CompanyId)
                {
                    if (_listAllCompanies.Any(x => x.strCompany == tempId.CompanyName))
                    {
                        if (tempFoundCompanyNamesOfId.Any(x => x.CompanyName == tempId.CompanyName))
                            continue;
                        tempFoundCompanyNamesOfId.Add(new CompanyIdType() { CompanyName = tempId.CompanyName, CompanyMarge = tempId.CompanyMarge, CompanyVoorschot = tempId.CompanyVoorschot });
                    }
                    if (_listAllCompanies.Any(x => x.strTarget == tempId.CompanyName))
                    {
                        foreach (var tempCompany in _listAllCompanies)
                        {
                            if (tempCompany.strTarget != tempId.CompanyName)
                                continue;
                            tempId.CompanyName = tempCompany.strCompany;
                            tempFoundCompanyNamesOfId.Add(new CompanyIdType() { CompanyName = tempId.CompanyName, CompanyMarge = tempId.CompanyMarge, CompanyVoorschot = tempId.CompanyVoorschot });
                        }
                    }
                }

            }
            return tempFoundCompanyNamesOfId;
        }

        /// <summary>
        /// Button om alle companies zonder een ID te laten zien
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowCompanyWithNoId_Click(object sender, RoutedEventArgs e)
        {
            List<CompanyType> tempCompanyWithNoIdList = new List<CompanyType>();

            // Leest de bestand met alle opgeslagen data van de companies
            var tempSavedIdsList = DataManager.DataManager.ReadCompanyDataFile();
            if (tempSavedIdsList == null) return;

            // loop om door elke saved ID/data van een company om te zoeken welke geen ID hebben
            foreach (var tempCompanyData in tempSavedIdsList)
            {
                if (tempCompanyData.CompanyId == null)
                {
                    if (_listAllCompanies.Any(x => x.strCompany == tempCompanyData.CompanyName))
                        tempCompanyWithNoIdList.Add(new CompanyType() { strCompany = tempCompanyData.CompanyName });
                }
            }
            _lbCompanies.SetItemsSource(tempCompanyWithNoIdList);
        }

        private void btnShowAllCompanies_Click(object sender, RoutedEventArgs e)
        {
            _lbCompanies.SetItemsSource(_listAllCompanies);
            lbAllCompanyIds.UnselectAll();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteAllData();
                pbProgress.Value = 0;
                pbProgress.Minimum = 0;
                pbProgress.Maximum = _listAllFilePaths.Count();
                int tempProgressCount = 0;
                this.IsEnabled = false;
                this.Cursor = Cursors.Wait;

                await Task.Run(() =>
                {
                    foreach (var file in _listAllFilePaths)
                    {
                        // Lijst met geselecteerde bestanden zichtbaar maken in Listbox
                        App.Current.Dispatcher?.Invoke(() =>
                        {
                            lbAllSelectedFiles.Items.Add(file.Substring(file.LastIndexOf(@"\") + 1));
                        });

                        var tempFilePath = file;
                        List<CallData> tempCallDataList = LogicManager.LogicManager.GetAllParsedRowsList(tempFilePath);
                        List<CompanyType> tempCompaniesList = LogicManager.LogicManager.GetUniqueCompanies(tempCallDataList);
                        List<MonthType> tempMonthsList = LogicManager.LogicManager.GetUniqueMonths(tempCallDataList);
                        List<SubscriberData> tempSubscribersList = LogicManager.LogicManager.GetUniqueSubscribers(tempCallDataList);

                        // Verwerkte data van bestand toevoegen aan de hoofdlijsten
                        _listAllCallData.AddRange(tempCallDataList);
                        _listAllCompanies.AddRange(tempCompaniesList);
                        _listAllMonths.AddRange(tempMonthsList.Where(x => _listAllMonths.FirstOrDefault(y => y.intMonth == x.intMonth) == null).ToList());
                        _listAllSubscriber.AddRange(tempSubscribersList);

                        tempProgressCount++;
                        SetProgress(tempProgressCount);
                    }
                });

                var tempAllSavedCompanyData = DataManager.DataManager.ReadCompanyDataFile();
                if (tempAllSavedCompanyData != null && tempAllSavedCompanyData.Count > 0)
                {
                    var tempListCompaniesSavedAndNew = new List<CompanyMargesType>();
                    var tempListCompaniesWithNickname = new List<CompanyMargesType>();
                    List<CompanyIdType> tempAllFoundIdsList = new List<CompanyIdType>();

                    foreach (var tempCompany in _listAllCompanies)
                    {
                        if (tempCompany.strCompany == tempCompany.strSubscriber)
                        {
                            var tempFoundCompany = tempAllSavedCompanyData.Find(x => x.CompanyName == tempCompany.strCompany);

                            if (tempFoundCompany == null)
                                continue;

                            foreach (var tempCall in _listAllCallData)
                            {
                                if (tempCall.Bedrijf == tempFoundCompany.CompanyName)
                                {
                                    if (tempFoundCompany.CompanyId == null)
                                        tempFoundCompany.CompanyId = tempCall.ID;
                                    if (tempCompany.strId == null)
                                    {
                                        tempCompany.strId = tempCall.ID;
                                        tempCompany.strTarget = tempCall.Target;
                                    }
                                    else break;
                                }
                            }
                        }
                    }

                    foreach (var tempCompany in _listAllCompanies)
                    {
                        var tempFoundCompany = tempAllSavedCompanyData.Find(x => x.CompanyName == tempCompany.strCompany);
                        if (tempFoundCompany != null)
                        {
                            // object bestaat al
                            tempListCompaniesSavedAndNew.Add(tempFoundCompany);
                        }
                        else
                        {
                            tempListCompaniesSavedAndNew.Add(new CompanyMargesType() { CompanyName = tempCompany.strCompany, CompanyId = tempCompany.strId, CompanyNickname = tempCompany.strNickname });
                        }
                    }
                    foreach (var tempCompany in _listAllCompanies)
                    {
                        var tempFoundCompany = tempListCompaniesSavedAndNew.Find(x => x.CompanyName == tempCompany.strCompany && !string.IsNullOrEmpty(x.CompanyNickname));
                        if (tempFoundCompany != null)
                        {
                            tempCompany.strNickname = tempFoundCompany.CompanyNickname;
                            tempCompany.strId = tempFoundCompany.CompanyId;
                            //  tempListCompaniesWithNickname.Add(tempFoundCompany);
                        }
                    }

                    int tempNoIdCount = 0;
                    List<CompanyMargesType> tempNoIdList = new List<CompanyMargesType>();
                    foreach (var tempCompany in tempListCompaniesSavedAndNew)
                    {
                        if (string.IsNullOrEmpty(tempCompany.CompanyId))
                        {
                            tempNoIdCount++;
                            tempNoIdList.Add(tempCompany);
                        }
                    }
                    if (tempNoIdCount > 0)
                    {
                        if (tempNoIdCount > 5)
                        {
                            MBox.ShowInformation("Er zijn meerdere bedrijven zonder een ID!");
                        }
                        else
                        {
                            foreach (var tempNoName in tempNoIdList)
                            {
                                MBox.ShowInformation($"{tempNoName.CompanyName} \nHeeft geen ID.");
                            }
                        }
                    }

                    foreach (var tempSavedCompany in tempListCompaniesSavedAndNew)
                    {
                        if (tempSavedCompany.CompanyId != null)
                            tempAllFoundIdsList.Add(new CompanyIdType() { CompanyId = tempSavedCompany.CompanyId, CompanyNickName = tempSavedCompany.CompanyNickname });
                    }

                    List<CompanyIdType> tempCompanyIdsList = tempAllFoundIdsList
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
                }
                /*
                foreach (var Company in _listAllCompanies)
                {
                    var tempMissingCompaniesInFile = tempAllSavedCompanyData.FindAll(x => x.CompanyName == Company.strCompany);
                }*/
                SetProgress(0);


                _listAllCompanies = LogicManager.LogicManager.ParseListAllCompanies(_listAllCompanies);
                _listAllCompanies = _listAllCompanies.OrderBy(x => x.strCompany).ToList();
                _lbCompanies.SetItemsSource(_listAllCompanies);

                btnPerJaar.IsEnabled = true;
                btnPerMaand.IsEnabled = true;
                btnCompanyMarges.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MBox.ShowWarning(ex.Message);
            }
            finally
            {
                this.Cursor = null;
                this.IsEnabled = true;
            }
        }


    }
}

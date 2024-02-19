﻿using HDSignaturesTool.Data;
using HDSignaturesTool.Logic;
using HDSignaturesTool.Types;
using M.Core.Application.ControlHelpers;
using M.Core.Application.WPF.MessageBox;
using M.Core.Mail;
using M.NetStandard.Helpers;
using M.NetStandard.Settings;
using M.NetStandard.SinkToAction;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Log = Serilog.Log;
using Path = System.IO.Path;
using Window = System.Windows.Window;

namespace HDSignaturesTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int intCount = 0;
        private int OphalenGelukt = 0;
        private int intOpslaanIpvStop = 0;
        private int intDataGridUpdate = 0;

        private double time = Settings.intSeconds;
        private double PausedTime;
        private DispatcherTimer Timer;
        private DispatcherTimer Timer2;
        DateTime startTime = DateTime.Now;
        DateTime dtTimeNowForRun;
        private ListBoxControl<UserData> _lbAllUsers;
        private ListBoxControl<UserData> _lbInactiveUsers;
        private ListBoxControl<UserData> _lbActiveUsers;
        private DataGridControl<UserDataWrapper> _dgUserInformation;

        List<UserDataWrapper> listSavedUserDatas = new List<UserDataWrapper>();


        public MainWindow()
        {
            InitializeComponent();


            _ = InitWindow();
        }

        private async Task InitWindow()
        {
            try
            {
                // InitMail voor mailing
                await MailManager.InitMail("hdsignatures@hdservices.nl", @"@V8w6>", "HD Signatures", "Bedrijfsnaam", true, "bedrijf@hdservices.nl", "HD Signatures");
                // AutoSettings voor het opslaan van de gekozen settings
                await AutoSettings.CreateDefaultOrLoadSettingsForAllAutoSettingsClassesAsync();

                this.Title += " - " + QuickTools.GetVersionNumber();

                #region Listboxes
                // ListBox met alle users
                _lbAllUsers = new ListBoxControl<UserData>(lbAllUsers);
                // ListBox met inactieve users
                _lbInactiveUsers = new ListBoxControl<UserData>(lbInactiveUsers);
                // ListBox met geaccepteerde users
                _lbActiveUsers = new ListBoxControl<UserData>(lbActiveUsers);
                #endregion

                #region DataGrid
                // DataGrid met de geaccepteerde users zijn Naam, Description, Pad en een Checkbox voor als de user moet aangemaakt worden of niet
                _dgUserInformation = new DataGridControl<UserDataWrapper>(dgUserInformation, false);
                _dgUserInformation.CreateTextColumn("Aanmaken", nameof(UserDataWrapper.blnCreate_dg), 70).IsReadOnly = true;
                _dgUserInformation.CreateTextColumn("Naam", nameof(UserDataWrapper.UserName_dg), 100).IsReadOnly = true;
                _dgUserInformation.CreateTextColumn("Description", nameof(UserDataWrapper.Description_dg), 200).IsReadOnly = true;
                _dgUserInformation.CreateTextColumn("Pad", nameof(UserDataWrapper.Path_dg), 455).IsReadOnly = true;
                _dgUserInformation.EventRowEdited += _dgUserInformation_EventRowEdited;
                _dgUserInformation.UseVisualTemplateLines();
                #endregion

                #region Serilog logt naar een textbox
                //zoekt waar de applicatie is opgestart en maakt een bestand aan die "HDSignatures_Logs" heet
                var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                var logPath = Path.Combine(startupPath, "HDSignatures_Logs");

                //Logger
#if DEBUG

                Log.Logger = new LoggerConfiguration()
                                  //Wat de bestanden hun naam zijn en waar ze naartoe moeten
                                  .WriteTo.Sink(new ActionSink(LogToTextBox))
                                .WriteTo.File(Path.Combine(logPath, $"[{DateTime.Now:yyyy-MM-dd}] HDSignatures.log"),
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
                                .WriteTo.File(Path.Combine(logPath, $"[{DateTime.Now:yyyy-MM-dd}] HDSignatures.log"),
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

                #region Startup settings checks

                btnStart.IsEnabled = false;
                tbCopyPath.IsReadOnly = true;
                tbCopyPath.IsEnabled = false;
                MailSettings.Applicatienaam = "HD Signatures";
                MailSettings.Backup3000ApplicationName = "HD Signatures";
                await LoadSettings();
                ReadSavedCreateUsers();
                if (cbDomain.IsChecked == false && cbLokaal.IsChecked == false)
                    intCount++;
                btnOpslaan.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");
                if (cbLokaal.IsChecked == true)
                    cbDomain.IsEnabled = false;
                else cbDomain.IsEnabled = true;
                if (cbDomain.IsChecked == true)
                    cbLokaal.IsEnabled = false;
                else cbLokaal.IsEnabled = true;
                if (cbAdminOnly.IsChecked == true)
                {
                    cbUserGetsAllSignatures.IsChecked = false;
                    cbCreateSignatureDirectory.IsChecked = false;
                    cbUserGetsAllSignatures.IsEnabled = false;
                    cbCreateSignatureDirectory.IsEnabled = false;
                }
                else
                {
                    cbUserGetsAllSignatures.IsEnabled = true;
                    cbCreateSignatureDirectory.IsEnabled = true;
                }
                if (btnStart.IsEnabled == true)
                {
                    btnStartTimer.IsEnabled = true;
                    btnResetTimer.IsEnabled = true;
                    btnOpslaan.IsEnabled = false;
                }
                else
                {
                    btnStartTimer.IsEnabled = false;
                    btnPauseTimer.IsEnabled = false;
                    btnResetTimer.IsEnabled = false;
                }
                if (cbCopySignatures.IsChecked == true)
                {
                    tbCopyPath.IsEnabled = true;
                    tbCopyPath.IsReadOnly = false;
                }
                if (cbDomain.IsChecked == false && cbLokaal.IsChecked == false)
                    btnUsersOphalen.IsEnabled = false;
                else btnUsersOphalen.IsEnabled = true;
                if (cbDeleteAllSignatures.IsChecked == true)
                    cbDeleteAllSignatures.IsChecked = false;

                await CheckIfLocalOrDomainIsChecked();
                ProcessMinutesForTimerPreview();

                this.tbMinutes.MaxLength = 4;
                Timer = new DispatcherTimer();
                Timer.Interval = new TimeSpan(0, 0, 0, 0, 250);
                Timer.Tick += Timer_Tick;

                Timer2 = new DispatcherTimer();
                Timer2.Interval = new TimeSpan(0, 0, 0, 0, 250);
                Timer2.Tick += Timer_Tick2;

                if (Settings.blnRunApplicationOnStartup)
                {
                    dtTimeNowForRun = DateTime.Now.AddMinutes(5);
                    RunAfter5Minutes();
                }
                if (Settings.blnStartTimerOnStartup)
                    btnStartTimer_Click(null, null);
                else
                {
                    cbStartTimerOnStartup.IsEnabled = false;
                    cbRunOnStartup.IsEnabled = false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private void Timer_Tick2(object sender, EventArgs e)
        {
            if (dtTimeNowForRun.Minute == DateTime.Now.Minute)
            {
                btnStart_Click(null, null);
                Timer2.Stop();
            }
        }

        public void RunAfter5Minutes()
        {
            Timer2.Start();
        }

        #region Timer
        /// <summary>
        /// Voor elke seconde die in de Timer zit gaat hij deze functie doen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Timer_Tick(object sender, EventArgs e)
        {
            if (time == 0)
            {
                time = Settings.intSeconds;
                return;
            }
            var tempTotalSeconds = time * 60;
            var tempDateNow = DateTime.Now;
            var tempTimeSpan = tempDateNow.Subtract(startTime);
            double tempTimeLeft = tempTotalSeconds - tempTimeSpan.TotalSeconds;
            var tempTimeSpanTimeLeft = TimeSpan.FromSeconds(tempTimeLeft);

            PausedTime = tempTimeLeft / 60;

            double tempMinutes = Math.Floor((tempTimeLeft / 60));
            var tempSeconds = tempTimeLeft - (tempMinutes * 60);

            tbTimerCounter.Text = tempTimeSpanTimeLeft.ToString("hh\\:mm\\:ss");
            if (tempTimeLeft <= 0)
            {
                Timer.Stop();
                btnStart_Click(null, null);
                time = Settings.intSeconds;
                startTime = DateTime.Now;
                Timer.Start();
            }
            // extra manier om mails te sturen
            /*
            DateTime tempDateTimeNow = DateTime.Now;
            int tempSavedNow = tempDateTimeNow.Hour;
            if (tempDateTimeNow.Second == 0)
                if (tempDateTimeNow.Minute == 54)
                    if (tempDateTimeNow.Hour == 13)
                    {
                        LogicManager.SendMail(Settings.strMailFrom, Settings.strMailTo, MailSettings.Applicatienaam, Settings.strBedrijfsNaam);
                    }
            */
        }

        /// <summary>
        /// Button om de Timer te starten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (tbTimerCounter.Text == "00:00:00")
                time = Settings.intSeconds;
            if (intOpslaanIpvStop >= 1)
            {
                intOpslaanIpvStop = 0;
                time = int.Parse(tbMinutes.Text);
            }
            startTime = DateTime.Now;
            btnStartTimer.IsEnabled = false;
            Timer.Start();
            grMain.IsEnabled = false;
            btnPauseTimer.IsEnabled = true;
            btnResetTimer.IsEnabled = true;
        }

        /// <summary>
        /// Button om de Timer te pauzeren
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPauseTimer_Click(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            time = PausedTime;
            grMain.IsEnabled = false;
            btnStartTimer.IsEnabled = true;
        }

        /// <summary>
        /// Button om de Timer te resetten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResetTimer_Click(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            time = Settings.intSeconds;
            grMain.IsEnabled = true;
            btnPauseTimer.IsEnabled = false;
            btnStartTimer.IsEnabled = true;
            try
            {
                if (tbMinutes.Text == "0" || string.IsNullOrEmpty(tbMinutes.Text))
                    throw new Exception();
                var tempTotalSeconds = int.Parse(tbMinutes.Text) * 60;
                var tempDateNow = DateTime.Now;
                startTime = DateTime.Now;
                var tempTimeSpan = tempDateNow.Subtract(startTime);
                double tempTimeLeft = tempTotalSeconds - tempTimeSpan.TotalSeconds;
                var tempTimeSpanTimeLeft = TimeSpan.FromSeconds(tempTimeLeft);
                PausedTime = tempTimeLeft / 60;
                double tempMinutes = Math.Floor((tempTimeLeft / 60));
                var tempSeconds = tempTimeLeft - (tempMinutes * 60);
                if (tempTimeSpanTimeLeft.ToString("hh\\:mm\\:ss") == "00:00:00")
                    tbTimerCounter.Text = "24:00:00";
                else tbTimerCounter.Text = tempTimeSpanTimeLeft.ToString("hh\\:mm\\:ss");
            }
            catch
            {
                tbTimerCounter.Text = "00:00:00";
            }

        }
        #endregion

        /// <summary>
        /// Functie die alle logs schrijft naar een textbox
        /// </summary>
        /// <param name="argText"></param>
        public void LogToTextBox(string argText)
        {
            if (string.IsNullOrEmpty(argText)) return;

            App.Current.Dispatcher?.Invoke(() =>
            {
                tbLogs.Text += argText;
            });
            tbLogs.ScrollToEnd();
        }

        private void cbStartAppOnStartup_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cbStartAppOnStartup.IsChecked)
                if (DataManager.CreateStartupValue())
                {
                    cbStartTimerOnStartup.IsEnabled = true;
                    Settings.blnStartAppOnStartup = true;
                    cbRunOnStartup.IsEnabled = true;
                }

            if ((bool)!cbStartAppOnStartup.IsChecked)
                if (DataManager.DeleteStartupValue())
                {
                    cbStartTimerOnStartup.IsEnabled = false;
                    Settings.blnStartAppOnStartup = false;
                    cbStartTimerOnStartup.IsChecked = false;
                    cbRunOnStartup.IsEnabled = false;
                    cbRunOnStartup.IsChecked = false;
                    Settings.blnRunApplicationOnStartup = false;
                }
            SettingChanged();
        }

        private void cbStartTimerOnStartup_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cbStartTimerOnStartup.IsChecked)
                Settings.blnStartTimerOnStartup = true;

            if ((bool)!cbStartTimerOnStartup.IsChecked)
                Settings.blnStartTimerOnStartup = false;
            SettingChanged();
        }

        /// <summary>
        /// Check voor als de Datagrid is aangepast
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dgUserInformation_EventRowEdited(object sender, UserDataWrapper e)
        {
            var tempList = _dgUserInformation.GetDataSource();

            var tempAllCreateList = tempList.FindAll(x => x.blnCreate);
        }

        /// <summary>
        /// Button om de BackupWindow te openen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBackups_Click(object sender, RoutedEventArgs e)
        {
            BackupsWindow secondWindow = new BackupsWindow() { Owner = this };
            secondWindow.ShowDialog();
        }

        /// <summary>
        /// Button om Lokaal of Domain Users op te halen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnUsersOphalen_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;

            await UsersOphalen();

            this.IsEnabled = true;
        }

        private async Task UsersOphalen()
        {
            tbBasisNaam.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");
            tbPrefix.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");
            tbSignatureDirectoryName.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");
            btnUsersOphalen.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");

            if ((bool)cbDomain.IsChecked || (bool)cbLokaal.IsChecked)
            {
                if ((bool)cbDomain.IsChecked)
                {
                    await SaveSettings();
                    if (intCount > 0 && string.IsNullOrEmpty(Settings.strBasisNaam) && string.IsNullOrEmpty(Settings.strSignatureDirectoryName))
                    {
                        MBox.ShowWarning("Vul eerst de Basis Naam en Directory Naam in");

                        tbBasisNaam.BorderBrush = Brushes.Red;
                        tbSignatureDirectoryName.BorderBrush = Brushes.Red;

                        return;
                    }
                    if (intCount > 0 && string.IsNullOrEmpty(Settings.strBasisNaam) || Settings.strBasisNaam == "")
                    {
                        MBox.ShowWarning("Vul Eerst de Basis Naam in");

                        tbBasisNaam.BorderBrush = Brushes.Red;

                        return;
                    }
                    if (intCount > 0 && string.IsNullOrEmpty(Settings.strSignatureDirectoryName) || Settings.strSignatureDirectoryName == "")
                    {
                        MBox.ShowWarning("Vul Eerst de Directory Naam in");

                        tbSignatureDirectoryName.BorderBrush = Brushes.Red;

                        return;
                    }

                    List<UserData> tempAllADUserList = await DataManager.GetAllADUsersList();
                    DataManager.GetPathFromSIDNumber(tempAllADUserList);
                    _lbAllUsers.SetItemsSource(tempAllADUserList);

                    List<UserData> tempInactiveADUserList = DataManager.GetInactiveADUsersList();
                    DataManager.GetPathFromSIDNumber(tempInactiveADUserList);
                    _lbInactiveUsers.SetItemsSource(tempInactiveADUserList);

                    List<UserData> tempActiveADUserList = DataManager.GetActiveADUsersList();
                    DataManager.GetPathFromSIDNumber(tempActiveADUserList);
                    _lbActiveUsers.SetItemsSource(tempActiveADUserList);

                    List<UserData> tempProcessedUserList = LogicManager.ParseUsersDescription(tempActiveADUserList);
                    DataManager.GetPathFromSIDNumber(tempProcessedUserList);

                    List<UserDataWrapper> tempUserDataWrapperList = new List<UserDataWrapper>();

                    foreach (var tempUserData in tempProcessedUserList)
                    {
                        var tempUserDataWrapper = new UserDataWrapper();
                        tempUserDataWrapper.userData = tempUserData;
                        tempUserDataWrapperList.Add(tempUserDataWrapper);
                    }

                    _dgUserInformation.SetDataSource(tempUserDataWrapperList);

                    ReadSavedCreateUsers();

                    btnOpslaan.IsEnabled = true;
                    OphalenGelukt++;
                    listSavedUserDatas.AddRange(tempUserDataWrapperList);
                }
                if (cbLokaal.IsChecked == true)
                {
                    await SaveSettings();
                    if (intCount > 0 && string.IsNullOrEmpty(Settings.strBasisNaam.Trim()) && string.IsNullOrEmpty(Settings.strSignatureDirectoryName.Trim()))
                    {
                        MBox.ShowWarning("Vul eerst de Basis Naam en Directory Naam in");
                        tbBasisNaam.BorderBrush = Brushes.Red;
                        tbSignatureDirectoryName.BorderBrush = Brushes.Red;
                        return;
                    }
                    if (intCount > 0 && string.IsNullOrEmpty(Settings.strBasisNaam))
                    {
                        MBox.ShowWarning("Vul Eerst de Basis Naam in");
                        tbBasisNaam.BorderBrush = Brushes.Red;
                        return;
                    }
                    if (intCount > 0 && string.IsNullOrEmpty(Settings.strSignatureDirectoryName))
                    {
                        MBox.ShowWarning("Vul Eerst de Directory Naam in");
                        tbSignatureDirectoryName.BorderBrush = Brushes.Red;
                        return;
                    }
                    List<UserData> tempAllLocalUserList = DataManager.GetAllLocalUsersList();
                    DataManager.GetPathFromSIDNumber(tempAllLocalUserList);
                    _lbAllUsers.SetItemsSource(tempAllLocalUserList);

                    List<UserData> tempInactiveLocalUserList = DataManager.GetInactiveLocalUsersList();
                    DataManager.GetPathFromSIDNumber(tempInactiveLocalUserList);
                    _lbInactiveUsers.SetItemsSource(tempInactiveLocalUserList);

                    List<UserData> tempActiveLocalUserList = DataManager.GetActiveLocalUsersList();
                    DataManager.GetPathFromSIDNumber(tempActiveLocalUserList);
                    _lbActiveUsers.SetItemsSource(tempActiveLocalUserList);

                    List<UserData> tempProcessedUserList = LogicManager.ParseUsersDescription(tempActiveLocalUserList);
                    DataManager.GetPathFromSIDNumber(tempProcessedUserList);

                    List<UserDataWrapper> tempUserDataWrapperList = new List<UserDataWrapper>();

                    foreach (var tempUserData in tempProcessedUserList)
                    {
                        var tempUserDataWrapper = new UserDataWrapper();
                        tempUserDataWrapper.userData = tempUserData;
                        tempUserDataWrapperList.Add(tempUserDataWrapper);
                    }

                    _dgUserInformation.SetDataSource(tempUserDataWrapperList);

                    ReadSavedCreateUsers();

                    btnOpslaan.IsEnabled = true;
                    OphalenGelukt++;
                    listSavedUserDatas.AddRange(tempUserDataWrapperList);
                }
            }
            else
            {
                MBox.ShowWarning("Selecteer eerst Lokaal of Domain");
            }
        }

        /// <summary>
        /// Functie die alle settings uit de GUI opslaat naar de settingsclass en naar settings.json
        /// </summary>
        private async Task SaveSettings()
        {
            if (!string.IsNullOrEmpty(tbMinutes.Text))
            {
                if (!IsValid(tbMinutes.Text))
                {
                    MBox.ShowWarning("Minuten kan niet hoger dan 24 uur zijn! \n\r (1440 minuten)");
                    Settings.intSeconds = 1440;
                    tbMinutes.Text = "1440";
                }
                else
                    Settings.intSeconds = int.Parse(tbMinutes.Text);
            }
            else
            {
                btnStartTimer.IsEnabled = false;
                btnPauseTimer.IsEnabled = false;
                btnResetTimer.IsEnabled = false;
            }
            MailSettings.Backup3000FictionalFromMail = tbMailFrom.Text;
            MailSettings.FromBedrijfsnaam = tbBedrijfsNaam.Text;
            // Settings.strMailTo = tbMailTo.Text;
            Settings.strAdminPath = tbBasisPath.Text;
            Settings.strFilePrefix = tbPrefix.Text;
            Settings.strOrigineleFile = tbBasisBestand.Text;
            Settings.strSignatureDirectoryName = tbSignatureDirectoryName.Text;
            Settings.strBasisNaam = tbBasisNaam.Text;
            Settings.blnStartTimerOnStartup = (bool)cbStartTimerOnStartup.IsChecked;
            Settings.blnStartAppOnStartup = (bool)cbStartAppOnStartup.IsChecked;
            Settings.blnUsersInLocal = (bool)cbLokaal.IsChecked;
            Settings.blnUsersInAD = (bool)cbDomain.IsChecked;
            Settings.blnCreateHtm = (bool)cbHtm.IsChecked;
            Settings.blnCreateRtf = (bool)cbRtf.IsChecked;
            Settings.blnCreateTxt = (bool)cbTxt.IsChecked;
            Settings.blnCreateFolderFiles = (bool)cbFiles.IsChecked;
            Settings.blnCreateFolderBestanden = (bool)cbBestanden.IsChecked;
            Settings.blnAdminOnly = (bool)cbAdminOnly.IsChecked;
            Settings.blnUserGetsAllSignatures = (bool)cbUserGetsAllSignatures.IsChecked;
            Settings.blnMakeSignatureDirectory = (bool)cbCreateSignatureDirectory.IsChecked;
            Settings.blnDeleteInactiveSignatures = (bool)cbDeleteInactiveSignature.IsChecked;
            Settings.blnDeleteAllSignatures = (bool)cbDeleteAllSignatures.IsChecked;
            Settings.blnCopySignatures = (bool)cbCopySignatures.IsChecked;
            Settings.strCopySignaturesToPath = tbCopyPath.Text;
            Settings.blnRunApplicationOnStartup = (bool)cbRunOnStartup.IsChecked;
            DataManager.GetSchijfFromPath(tbBasisPath.Text);

            await AutoSettings.SaveAllSettingsAsync();
            await MailSettings.SaveMailSettings();

            UpdateDataGrid();
        }

        /// <summary>
        /// Button die bepaald of Start gedrukt kan worden slaat alles op en checkt voor missende values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnOpslaan_Click(object sender, RoutedEventArgs e)
        {
            tbBasisPath.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");
            tbBasisBestand.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");
            tbSignatureDirectoryName.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");
            tbBasisNaam.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");
            btnOpslaan.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");

            if (cbHtm.IsChecked == true && cbFiles.IsChecked == false && cbBestanden.IsChecked == false)
                MBox.ShowWarning("Htm is gekozen maar geen Files of Bestanden. \nDe meeste Htm handtekeningen hebben of een Files of Bestanden erbij voor logos of stylings.");

            if (cbHtm.IsChecked == false && cbTxt.IsChecked == false && cbRtf.IsChecked == false && cbFiles.IsChecked == false && cbBestanden.IsChecked == false)
                if (!MBox.ShowQuestionWarning("Geen soort handtekening gekozen. \nEr gaat niks gemaakt of verwijderd kunnen worden. \nWilt u door gaan?"))
                    return;

            await SaveSettings();
            await UsersOphalen();
            btnStart.IsEnabled = true;
            btnOpslaan.IsEnabled = false;
            if (!string.IsNullOrEmpty(tbBasisPath.Text))
            {
                if (!Directory.Exists(tbBasisPath.Text))
                {
                    // Log Pad niet kunnen vinden
                    // Verzinnen wat nog gedaan moet worden
                    MBox.ShowWarning("Basis Pad bestaat niet of is niet gevonden.");
                    tbBasisPath.BorderBrush = Brushes.Red;
                    btnStart.IsEnabled = false;
                }
            }
            else
            {
                MBox.ShowWarning("Geen Basis Pad gegeven.");
                tbBasisPath.BorderBrush = Brushes.Red;
                btnStart.IsEnabled = false;
            }
            if (string.IsNullOrEmpty(tbBasisBestand.Text))
            {
                MBox.ShowWarning("Geen Template Naam gegeven");
                tbBasisBestand.BorderBrush = Brushes.Red;
                btnStart.IsEnabled = false;
            }
            //Directory naam
            if (string.IsNullOrEmpty(tbSignatureDirectoryName.Text))
            {
                MBox.ShowWarning("Geen Directory Naam gegeven");
                tbSignatureDirectoryName.BorderBrush = Brushes.Red;
                btnStart.IsEnabled = false;
            }
            //Basis naam
            if (string.IsNullOrEmpty(tbBasisNaam.Text))
            {
                MBox.ShowWarning("Geen Basis Naam gegeven");
                tbBasisNaam.BorderBrush = Brushes.Red;
                btnStart.IsEnabled = false;
            }
            if (string.IsNullOrEmpty(tbPrefix.Text))
            {
                if (!MBox.ShowQuestionWarning("Geen Prefix gegeven\nWilt u nogsteeds doorgaan?"))
                {
                    tbPrefix.BorderBrush = Brushes.Red;
                    btnStart.IsEnabled = false;
                }
            }
            if (btnStart.IsEnabled == true && !string.IsNullOrEmpty(tbMinutes.Text))
            {
                intOpslaanIpvStop++;
                btnStartTimer.IsEnabled = true;
                btnResetTimer.IsEnabled = true;
                btnOpslaan.IsEnabled = false;
            }
            else
            {
                btnStartTimer.IsEnabled = false;
                btnPauseTimer.IsEnabled = false;
                btnResetTimer.IsEnabled = false;
                btnOpslaan.IsEnabled = true;
            }
            if (tbMinutes.Text == "0")
            {
                btnStartTimer.IsEnabled = false;
                btnPauseTimer.IsEnabled = false;
                btnResetTimer.IsEnabled = false;
                btnOpslaan.IsEnabled = false;
            }
            if ((bool)cbStartTimerOnStartup.IsChecked && (string.IsNullOrEmpty(tbMinutes.Text) || tbMinutes.Text == "0"))
            {
                MBox.ShowWarning("\"Start timer bij het opstarten\" is aangevinkt maar er zijn geen minuten gegeven");
                btnStartTimer.IsEnabled = false;
                btnPauseTimer.IsEnabled = false;
                btnResetTimer.IsEnabled = false;
                btnOpslaan.IsEnabled = true;
                cbStartTimerOnStartup.IsChecked = false;
                cbRunOnStartup.IsChecked = false;
            }
        }

        /// <summary>
        /// Button die de applicatie start met alle gegevens die opgeslagen zijn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            await UsersOphalen();

            grMain.IsEnabled = false;
            var tempUserDataList = GetCreateSignatureUserDataList();

            var tempUserList = new UserLists()
            {
                ProcessedUsersList = tempUserDataList,
                ActiveUsersLists = _lbActiveUsers.GetItemsSource(),
                InactiveUsersList = _lbInactiveUsers.GetItemsSource(),
                AllUsersList = _lbAllUsers.GetItemsSource()
            };

            DataManager.BackupSignatures();
            if (DataManager.CheckIfUpdateIsNeeded() || DataManager.CheckForChanges(tempUserList.ProcessedUsersList))
            {
                Settings.strSignatureDirectoryName = "Handtekeningen";
                if (Settings.blnUsersInAD) LogicManager.ModifySignaturesForAD(tempUserList);
                if (Settings.blnUsersInLocal) LogicManager.ModifySignaturesForLocal(tempUserList);

                Settings.strSignatureDirectoryName = "Signatures";
                if (Settings.blnUsersInAD) LogicManager.ModifySignaturesForAD(tempUserList);
                if (Settings.blnUsersInLocal) LogicManager.ModifySignaturesForLocal(tempUserList);
            }
            grMain.IsEnabled = true;
        }

        private void btnEnableUser_Click(object sender, RoutedEventArgs e)
        {
            var tempSelectedUsersList = _dgUserInformation.GetSelectedItems();
            if (tempSelectedUsersList.Count == 0)
            {
                MBox.ShowWarning("Selecteer eerst een of meerdere users");
                return;
            }
            foreach (var tempEnable in tempSelectedUsersList)
            {
                tempEnable.blnCreate = true;
            }
            SettingChanged();
            var tempUserDataWrapperList = _dgUserInformation.GetDataSource();
            _dgUserInformation.SetDataSource(null);
            _dgUserInformation.SetDataSource(tempUserDataWrapperList);
        }

        private void btnDisableUser_Click(object sender, RoutedEventArgs e)
        {
            var tempSelectedUsersList = _dgUserInformation.GetSelectedItems();
            if (tempSelectedUsersList.Count == 0)
            {
                MBox.ShowWarning("Selecteer eerst een of meerdere users");
                return;
            }

            foreach (var tempEnable in tempSelectedUsersList)
            {
                tempEnable.blnCreate = false;
            }

            SettingChanged();

            var tempUserDataWrapperList = _dgUserInformation.GetDataSource();
            _dgUserInformation.SetDataSource(null);
            _dgUserInformation.SetDataSource(tempUserDataWrapperList);
        }

        private void btnSelectAllUsers_Click(object sender, RoutedEventArgs e)
        {
            var tempUserDataWrapperList = _dgUserInformation.GetDataSource();
            _dgUserInformation.SelectRows(tempUserDataWrapperList);
            _dgUserInformation.Focus();
        }
        private void btnDeselectAllUsers_Click(object sender, RoutedEventArgs e)
        {
            List<UserDataWrapper> tempEmptyList = new List<UserDataWrapper>();
            _dgUserInformation.SelectRows(tempEmptyList);
        }

        /// <summary>
        /// Functie die Checkt bij het opstarten of er Settings gesaved zijn van de laatste keer dat er op Opslaan gedrukt is
        /// </summary>
        public async Task LoadSettings()
        {
            await AutoSettings.LoadSettingsOfTypeAsync<Settings>();
            await MailSettings.LoadMailSettings();

            tbBasisPath.Text = Settings.strAdminPath;
            tbPrefix.Text = Settings.strFilePrefix;
            tbBasisBestand.Text = Settings.strOrigineleFile;
            tbSignatureDirectoryName.Text = Settings.strSignatureDirectoryName;

            if (string.IsNullOrEmpty(Settings.strBasisNaam))
            {
                cbLokaal.IsChecked = false;
                cbDomain.IsChecked = false;
            }
            else
            {
                tbBasisNaam.Text = Settings.strBasisNaam;
                cbLokaal.IsChecked = Settings.blnUsersInLocal;
                cbDomain.IsChecked = Settings.blnUsersInAD;
            }

            tbBedrijfsNaam.Text = MailSettings.FromBedrijfsnaam;
            tbMailFrom.Text = MailSettings.Backup3000FictionalFromMail;
            // tbMailTo.Text = Settings.strMailTo;
            cbStartAppOnStartup.IsChecked = Settings.blnStartAppOnStartup;
            cbStartTimerOnStartup.IsChecked = Settings.blnStartTimerOnStartup;
            cbRunOnStartup.IsChecked = Settings.blnStartAppOnStartup;
            tbCopyPath.Text = Settings.strCopySignaturesToPath;
            cbHtm.IsChecked = Settings.blnCreateHtm;
            cbRtf.IsChecked = Settings.blnCreateRtf;
            cbTxt.IsChecked = Settings.blnCreateTxt;
            cbFiles.IsChecked = Settings.blnCreateFolderFiles;
            cbBestanden.IsChecked = Settings.blnCreateFolderBestanden;
            cbAdminOnly.IsChecked = Settings.blnAdminOnly;
            cbUserGetsAllSignatures.IsChecked = Settings.blnUserGetsAllSignatures;
            cbCreateSignatureDirectory.IsChecked = Settings.blnMakeSignatureDirectory;
            cbDeleteInactiveSignature.IsChecked = Settings.blnDeleteInactiveSignatures;
            cbDeleteAllSignatures.IsChecked = Settings.blnDeleteAllSignatures;
            cbCopySignatures.IsChecked = Settings.blnCopySignatures;
            tbMinutes.Text = Settings.intSeconds.ToString();

            if (!string.IsNullOrEmpty(tbBasisPath.Text))
                if (Directory.Exists(tbBasisPath.Text))
                    btnStart.IsEnabled = true;
        }

        /// <summary>
        /// Checkbox die Alle Signatures delete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDeleteAllSignatures_Click(object sender, RoutedEventArgs e)
        {
            if (cbDeleteAllSignatures.IsChecked == true)
                if (!MBox.ShowQuestionWarning("Weet u zeker dat u de optie \"Verwijder alle signatures\" wilt selecteren?"))
                {
                    cbDeleteAllSignatures.IsChecked = false;
                    return;
                }
            btnOpslaan.BorderBrush = Brushes.Red;
            btnStart.IsEnabled = false;
            btnOpslaan.IsEnabled = true;
            btnStartTimer.IsEnabled = false;
            btnPauseTimer.IsEnabled = false;
            btnResetTimer.IsEnabled = false;
            if (cbDeleteAllSignatures.IsChecked == true)
            {
                cbUserGetsAllSignatures.IsEnabled = false;
                cbCreateSignatureDirectory.IsEnabled = false;
                cbAdminOnly.IsEnabled = false;
                cbDeleteInactiveSignature.IsEnabled = false;
                cbCopySignatures.IsEnabled = false;
                cbHtm.IsEnabled = false;
                cbRtf.IsEnabled = false;
                cbTxt.IsEnabled = false;
                cbFiles.IsEnabled = false;
                cbBestanden.IsEnabled = false;
                tbCopyPath.IsReadOnly = true;
                tbCopyPath.IsEnabled = false;
                cbUserGetsAllSignatures.IsChecked = false;
                cbCreateSignatureDirectory.IsChecked = false;
                cbAdminOnly.IsChecked = false;
                cbDeleteInactiveSignature.IsChecked = false;
                cbCopySignatures.IsChecked = false;
            }
            else
            {
                cbUserGetsAllSignatures.IsEnabled = true;
                cbCreateSignatureDirectory.IsEnabled = true;
                cbUserGetsAllSignatures.IsEnabled = true;
                cbCreateSignatureDirectory.IsEnabled = true;
                cbDeleteInactiveSignature.IsEnabled = true;
                cbCopySignatures.IsEnabled = true;
                cbHtm.IsEnabled = true;
                cbRtf.IsEnabled = true;
                cbTxt.IsEnabled = true;
                cbFiles.IsEnabled = true;
                cbBestanden.IsEnabled = true;
                cbAdminOnly.IsEnabled = true;
                if ((bool)cbAdminOnly.IsChecked)
                {
                    cbUserGetsAllSignatures.IsEnabled = false;
                    cbCreateSignatureDirectory.IsEnabled = false;
                }
                if ((bool)cbCopySignatures.IsChecked)
                {
                    tbCopyPath.IsReadOnly = false;
                    tbCopyPath.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Functie die checkt of Lokaal of Domain gechecked is 
        /// </summary>
        public async Task CheckIfLocalOrDomainIsChecked()
        {
            if (cbLokaal.IsChecked == true)
                await UsersOphalen();
            if (cbDomain.IsChecked == true)
                await UsersOphalen();
        }

        /// <summary>
        /// Functie die een userdata list teruggeeft met users waar een signature voor moet worden gemaakt.
        /// </summary>
        /// <returns></returns>
        public List<UserData> GetCreateSignatureUserDataList()
        {
            /*
            var tempUserDataWrapperCreateList = new List<UserDataWrapper>();
            foreach(var tempUserDataWrapper in tempUserDataWrapperList)
            {
                if(tempUserDataWrapper.blnCreate) tempUserDataWrapperCreateList.Add(tempUserDataWrapper);
            }

            var tempUserDataList2 = new List<UserData>();
            foreach(var tempUserDataWrapper in tempUserDataWrapperCreateList)
            {
                tempUserDataList2.Add(tempUserDataWrapper.userData);
            }*/
            var tempUserDataWrapperList = _dgUserInformation.GetDataSource();
            List<UserData> tempUserDataList = tempUserDataWrapperList.FindAll(x => x.blnCreate == true).Select(x => x.userData).ToList();

            return tempUserDataList;
        }

        /// <summary>
        /// Functie die elke true of false opslaat van de user zijn checkbox in de datagrid
        /// </summary>
        public void UpdateDataGrid()
        {
            var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var tempBackupPath = Path.Combine(startupPath, "HDSignatures.SavedUsers.txt");
            if (!File.Exists(tempBackupPath))
                File.Create(tempBackupPath).Close();
            if (intDataGridUpdate > 0)
                File.WriteAllText(tempBackupPath, "");
            else
            {
                intDataGridUpdate++;
                return;
            }

            var tempUserDataWrapperList = _dgUserInformation.GetDataSource();
            if (tempUserDataWrapperList == null)
                return;
            foreach (var tempUser in tempUserDataWrapperList)
            {
                File.AppendAllText(tempBackupPath, tempUser.UserName_dg + "=" + tempUser.blnCreate + Environment.NewLine);
            }
        }

        /// <summary>
        /// Checkt in de opgeslagen bestand voor de user zijn naam en daarna of er True of False achter staat (wel / niet een signature aanmaken)
        /// </summary>
        public void ReadSavedCreateUsers()
        {
            try
            {
                var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                var tempBackupPath = Path.Combine(startupPath, "HDSignatures.SavedUsers.txt");
                if (!File.Exists(tempBackupPath))
                    return;
                var tempUserDataWrapperList = _dgUserInformation.GetDataSource();
                if (tempUserDataWrapperList == null) return;
                string[] tempAllLinesOfSavedUsers = File.ReadAllLines(tempBackupPath);
                foreach (var tempUser in tempUserDataWrapperList)
                {
                    foreach (var line in tempAllLinesOfSavedUsers)
                    {
                        if (line.Contains(tempUser.UserName_dg))
                        {
                            var tempDictionary = new Dictionary<string, string>();

                            string[] tempSplittedLine = line.ToString().Split('=');

                            tempDictionary.Add(tempSplittedLine[0], tempSplittedLine[1]);

                            if (tempSplittedLine[1].ToString().Contains("True"))
                            {
                                tempUser.blnCreate = true;
                                break;
                            }
                            if (tempSplittedLine[1].ToString().Contains("False"))
                            {
                                tempUser.blnCreate = false;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Functie die checkt of er minuten zijn ingevuld voor de timer en daarna slaat hij ze op of past hij de timer aan
        /// </summary>
        public void ProcessMinutesForTimerPreview()
        {
            try
            {
                if (string.IsNullOrEmpty(tbMinutes.Text) || tbMinutes.Text == "0")
                {
                    tbTimerCounter.Text = "00:00:00";
                    return;
                }

                var tempTotalSeconds = int.Parse(tbMinutes.Text) * 60;
                var tempDateNow = DateTime.Now;
                startTime = DateTime.Now;

                var tempTimeSpan = tempDateNow.Subtract(startTime);
                double tempTimeLeft = tempTotalSeconds - tempTimeSpan.TotalSeconds;
                var tempTimeSpanTimeLeft = TimeSpan.FromSeconds(tempTimeLeft);
                double tempMinutes = Math.Floor((tempTimeLeft / 60));
                var tempSeconds = tempTimeLeft - (tempMinutes * 60);

                if (tempTimeSpanTimeLeft.ToString("hh\\:mm\\:ss") == "00:00:00")
                    tbTimerCounter.Text = "24:00:00";
                else tbTimerCounter.Text = tempTimeSpanTimeLeft.ToString("hh\\:mm\\:ss");
            }
            catch
            {
                tbTimerCounter.Text = "00:00:00";
            }
        }

        #region Oplsaan update
        public void SettingChanged()
        {
            btnOpslaan.BorderBrush = Brushes.Red;
            btnStart.IsEnabled = false;
            btnOpslaan.IsEnabled = true;
            btnStartTimer.IsEnabled = false;
            btnPauseTimer.IsEnabled = false;
            btnResetTimer.IsEnabled = false;
        }
        private void cbHtm_Checked(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbRtf_Checked(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbTxt_Checked(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbFiles_Checked(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbBestanden_Checked(object sender, RoutedEventArgs e)
        {
            btnOpslaan.BorderBrush = Brushes.Red;
            btnStart.IsEnabled = false;
            btnOpslaan.IsEnabled = true;
            btnStartTimer.IsEnabled = false;
            btnPauseTimer.IsEnabled = false;
            btnResetTimer.IsEnabled = false;
        }

        /// <summary>
        /// Checkbox die ervoor zorgt dat alle signatures aleen bij de admin path komen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbAdminOnly_Click(object sender, RoutedEventArgs e)
        {
            SettingChanged();
            if (cbAdminOnly.IsChecked == true)
            {
                cbUserGetsAllSignatures.IsEnabled = false;
                cbCreateSignatureDirectory.IsEnabled = false;
                cbUserGetsAllSignatures.IsChecked = false;
                cbCreateSignatureDirectory.IsChecked = false;
            }
            else
            {
                cbUserGetsAllSignatures.IsEnabled = true;
                cbCreateSignatureDirectory.IsEnabled = true;
            }
        }
        private void cbAdminOnly_Checked(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbUserGetsAllSignatures_Checked(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbHtm_Click(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbRtf_Click(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbTxt_Click(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbFiles_Click(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbBestanden_Click(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbUserGetsAllSignatures_Click(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbCreateSignatureDirectory_Click(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbDeleteInactiveSignature_Checked(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbDeleteInactiveSignature_Click(object sender, RoutedEventArgs e)
        {
            SettingChanged();
        }

        private void cbCopySignatures_Click(object sender, RoutedEventArgs e)
        {
            SettingChanged();
            if (cbCopySignatures.IsChecked == true)
            {
                tbCopyPath.IsReadOnly = false;
                tbCopyPath.IsEnabled = true;
            }
            else
            {
                tbCopyPath.IsReadOnly = true;
                tbCopyPath.IsEnabled = false;
            }
        }
        private void tbCopyPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnOpslaan.BorderBrush = Brushes.Red;
            btnStart.IsEnabled = false;
            btnOpslaan.IsEnabled = true;
            btnStartTimer.IsEnabled = false;
            btnPauseTimer.IsEnabled = false;
            btnResetTimer.IsEnabled = false;
        }

        private void tbPrefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (OphalenGelukt >= 1)
                btnOpslaan.BorderBrush = Brushes.Red;
            btnUsersOphalen.BorderBrush = Brushes.Red;
            btnStart.IsEnabled = false;
            btnOpslaan.IsEnabled = true;
            btnStartTimer.IsEnabled = false;
            btnPauseTimer.IsEnabled = false;
            btnResetTimer.IsEnabled = false;
        }

        private void tbBasisBestand_TextChanged(object sender, TextChangedEventArgs e)
        {
            SettingChanged();
        }

        private void tbSignatureDirectoryName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (OphalenGelukt >= 1)
                btnOpslaan.BorderBrush = Brushes.Red;
            btnUsersOphalen.BorderBrush = Brushes.Red;
            btnStart.IsEnabled = false;
            btnOpslaan.IsEnabled = true;
            btnStartTimer.IsEnabled = false;
            btnPauseTimer.IsEnabled = false;
            btnResetTimer.IsEnabled = false;
        }

        private void tbBasisNaam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (OphalenGelukt >= 1)
                btnOpslaan.BorderBrush = Brushes.Red;
            btnUsersOphalen.BorderBrush = Brushes.Red;
            btnStart.IsEnabled = false;
            btnOpslaan.IsEnabled = true;
            btnStartTimer.IsEnabled = false;
            btnPauseTimer.IsEnabled = false;
            btnResetTimer.IsEnabled = false;
        }

        private void tbBasisPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            SettingChanged();
        }

        private void cbLokaal_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            if (cbLokaal.IsChecked == true)
            {
                cbDomain.IsEnabled = false;
                btnUsersOphalen.IsEnabled = true;
            }
            else
            {
                cbDomain.IsEnabled = true;
                btnUsersOphalen.IsEnabled = false;
            }
            btnOpslaan.IsEnabled = true;
            btnStartTimer.IsEnabled = false;
            btnPauseTimer.IsEnabled = false;
            btnResetTimer.IsEnabled = false;
        }

        private void cbDomain_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            if (cbDomain.IsChecked == true)
            {
                cbLokaal.IsEnabled = false;
                btnUsersOphalen.IsEnabled = true;
            }
            else
            {
                cbLokaal.IsEnabled = true;
                btnUsersOphalen.IsEnabled = false;
            }
            btnOpslaan.IsEnabled = true;
            btnStartTimer.IsEnabled = false;
            btnPauseTimer.IsEnabled = false;
            btnResetTimer.IsEnabled = false;
        }

        private void tbMinutes_TextChanged(object sender, TextChangedEventArgs e)
        {
            SettingChanged();
            if (!string.IsNullOrEmpty(tbMinutes.Text))
                Settings.intSeconds = int.Parse(tbMinutes.Text);
            ProcessMinutesForTimerPreview();
        }
        private void tbMinutes_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsTextAllowed(e.Text))
            {
                e.Handled = true;
            }
        }
        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
        public static bool IsValid(string str)
        {
            int i;
            if (str == "0")
                return true;
            return int.TryParse(str, out i) && i >= 1 && i <= 1440;
        }

        private void tbMailTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            SettingChanged();
        }

        private void tbMailFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            SettingChanged();
        }

        private void tbBedrijfsNaam_TextChanged(object sender, TextChangedEventArgs e)
        {
            SettingChanged();
        }

        #endregion

        private void cbRunOnStartup_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cbRunOnStartup.IsChecked)
                Settings.blnRunApplicationOnStartup = true;

            if ((bool)!cbRunOnStartup.IsChecked)
                Settings.blnRunApplicationOnStartup = false;
            SettingChanged();
        }
    }
}

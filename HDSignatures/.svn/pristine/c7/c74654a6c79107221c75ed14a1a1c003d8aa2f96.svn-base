using HDSignaturesTool.Data;
using HDSignaturesTool.Types;
using M.Core.Application.ControlHelpers;
using M.Core.Application.WPF.MessageBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HDSignaturesTool
{
    /// <summary>
    /// Interaction logic for BackupsWindow.xaml
    /// </summary>
    public partial class BackupsWindow : Window
    {
        private ListBoxControl<BackupType> _lbTempBackups;
        private ListBoxControl<BackupType> _lbPermBackups;
        public BackupsWindow()
        {
            InitializeComponent();

            _lbTempBackups = new ListBoxControl<BackupType>(lbTempBackups);
            _lbTempBackups.EventDoubleClick += _lbTempBackups_EventDoubleClick;

            _lbPermBackups = new ListBoxControl<BackupType>(lbPermBackups);
            _lbPermBackups.EventDoubleClick += _lbPermBackups_EventDoubleClick;
            UpdateBothBackupLists();
        }
        public void UpdateBothBackupLists()
        {
            UpdateTempBackupsList();
            UpdatePermBackupsList();
        }
        public void UpdateTempBackupsList()
        {
            var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string tempCheckIfExists = Path.Combine(startupPath + "\\HDSignatures_TempBackups");
            if (!Directory.Exists(tempCheckIfExists))
                Directory.CreateDirectory(tempCheckIfExists);
            var tempBackupFolders = Directory.GetDirectories(startupPath + "\\HDSignatures_TempBackups").ToList();
            List<BackupType> tempBackupsList = new List<BackupType>();

            foreach (string tempFolder in tempBackupFolders)
            {
                var tempBackupType = new BackupType(tempFolder);

                tempBackupsList.Add(tempBackupType);
            }
            _lbTempBackups.SetItemsSource(tempBackupsList);
        }
        public void UpdatePermBackupsList()
        {
            var startupPathPerm = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string tempCheckIfExistsPerm = Path.Combine(startupPathPerm + "\\HDSignatures_PermBackups");
            if (!Directory.Exists(tempCheckIfExistsPerm))
                Directory.CreateDirectory(tempCheckIfExistsPerm);
            var tempBackupFoldersPerm = Directory.GetDirectories(startupPathPerm + "\\HDSignatures_PermBackups").ToList();
            List<BackupType> tempBackupsListPerm = new List<BackupType>();

            foreach (string tempFolderPerm in tempBackupFoldersPerm)
            {
                var tempBackupTypePerm = new BackupType(tempFolderPerm);

                tempBackupsListPerm.Add(tempBackupTypePerm);
            }
            _lbPermBackups.SetItemsSource(tempBackupsListPerm);
        }
        private void _lbTempBackups_EventDoubleClick(object sender, BackupType e)
        {
            if (e != null)
                Process.Start("explorer.exe", e.strFolderPath);
            UpdateBothBackupLists();
        }
        private void _lbPermBackups_EventDoubleClick(object sender, BackupType e)
        {
            if (e != null)
                Process.Start("explorer.exe", e.strFolderPath);
            UpdateBothBackupLists();
        }
        private void btnTempBackup_Click(object sender, RoutedEventArgs e)
        {
            var tempSelected = _lbTempBackups.GetSelection();
            if (tempSelected != null)
                Process.Start("explorer.exe", tempSelected.strFolderPath);
            else
                MBox.ShowWarning("Geen backup geselecteerd.");
            UpdateBothBackupLists();
        }
        private void btnPermBackup_Click(object sender, RoutedEventArgs e)
        {
            var tempSelected = _lbPermBackups.GetSelection();
            if (tempSelected != null)
                Process.Start("explorer.exe", tempSelected.strFolderPath);
            else
                MBox.ShowWarning("Geen backup geselecteerd.");
            UpdateBothBackupLists();
        }
        private void btnMakeBackupPermanent_Copy_Click(object sender, RoutedEventArgs e)
        {
            var tempSelected = _lbTempBackups.GetSelection();
            if (tempSelected != null)
                DataManager.CopyTempBackupToPermBackup(tempSelected.strFolderPath);
            else
                MBox.ShowWarning("Geen backup geselecteerd.");
            UpdateBothBackupLists();
        }
        private void btnDeletePermanentBackup_Click(object sender, RoutedEventArgs e)
        {
            var tempSelected = _lbPermBackups.GetSelection();
            if (tempSelected != null)
            {
                if (MBox.ShowQuestionWarning($"Weet u zeker dat {tempSelected.strFolderName} verwijderd moet worden?"))
                    DataManager.DeletePermanentBackupSignature(tempSelected.strFolderPath);
            }
            else
                MBox.ShowWarning("Geen backup geselecteerd.");
            UpdateBothBackupLists();
        }

        private void btnForceCreateBackup_Click(object sender, RoutedEventArgs e)
        {
            DataManager.CreateBackupOfSignatures();
        }
    }
}

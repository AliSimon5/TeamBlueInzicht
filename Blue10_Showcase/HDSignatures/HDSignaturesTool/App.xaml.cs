﻿using HDSignaturesTool.Types;
using M.Core.Application.SystemTray;
using M.NetStandard.Windows.SingletonApp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HDSignaturesTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SingletonAppManager.EventOpenApplication += SingletonAppManager_EventOpenApplication;
            SingletonAppManager.EventOpenApplicationSilent += SingletonAppManager_EventOpenApplicationSilent;

            SingletonAppManager.StartApplication("HDSignatures", Environment.GetCommandLineArgs(), false, true);
        }
        private void SingletonAppManager_EventOpenApplicationSilent(object sender, string[] e)
        {
            // Gebruik /silent als parameter om de applicatie stil op te starten
            App.Current.Dispatcher?.Invoke(async () =>
            {
                MainWindow = new MainWindow();

                CreateSystemTray();
            });

        }
        private void SingletonAppManager_EventReactivateApplication(object sender, EventArgs e)
        {
            App.Current.Dispatcher?.Invoke(async () =>
            {
                MainWindow?.Show();
            });
        }
        private void SingletonAppManager_EventOpenApplication(object sender, string[] e)
        {
            App.Current.Dispatcher?.Invoke(async () =>
            {

                MainWindow = new MainWindow();
                MainWindow.Show();

                CreateSystemTray();
            });

        }
        private void CreateSystemTray()
        {
            var tempNotificationTray = SystemTrayManager.InitSystemTray(MainWindow, "HD Signatures Tool", SystemTrayManager.GetIconFromApplicationResource("Resources/HDSignaturesIcon.ico"), false, false);

            // tempNotificationTray.AddLineContextMenu();
            tempNotificationTray.AddItemToContextMenu("Open HD Signatures...", OpenMainWindow);
            tempNotificationTray.AddVersionNumberOnTop();
        }

        private void OpenMainWindow()
        {
            MainWindow?.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception.Message);
            Log.Error(e.Exception.StackTrace);

            Log.CloseAndFlush();
        }
    }
}

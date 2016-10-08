Skip to content
This repository
Search
Pull requests
Issues
Gist
 @flanker22
You don’t have any verified emails. We recommend verifying at least one email.
Email verification helps our support team verify ownership if you lose account access and allows you to receive all the notifications you ask for.
 Watch 19
  Star 151
  Fork 21 xenolightning/AudioSwitcher_v1
 Code  Issues 24  Pull requests 0  Projects 0  Wiki  Pulse  Graphs
Tree: f90ab1f053 Find file Copy pathAudioSwitcher_v1/FortyOne.AudioSwitcher/Program.cs
f90ab1f  on Jun 22, 2015
@xenolightning xenolightning Code Clean
1 contributor
RawBlameHistory     
140 lines (119 sloc)  5.03 KB
using System;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Windows.Forms;
using FortyOne.AudioSwitcher.Configuration;
using FortyOne.AudioSwitcher.Properties;

namespace FortyOne.AudioSwitcher
{
    internal static class Program
    {
        public static ConfigurationSettings Settings { get; private set; }

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.ThreadException += WinFormExceptionHandler.OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += WinFormExceptionHandler.OnUnhandledCLRException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Environment.OSVersion.Version.Major < 6)
            {
                MessageBox.Show("Audio Switcher only supports Windows Vista and Windows 7",
                    "Unsupported Operating System");
                return;
            }

            Application.ApplicationExit += Application_ApplicationExit;

            //Delete the old updater
            try
            {
                var updaterPath = Application.StartupPath + "AutoUpdater.exe";
                if (File.Exists(updaterPath))
                    File.Delete(updaterPath);
            }
            catch
            {
                //This shouldn't prevent the application from running
            }

            //Delete the new updater
            try
            {
                var updaterPath = Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName,
                    "AutoUpdater.exe");
                if (File.Exists(updaterPath))
                    File.Delete(updaterPath);
            }
            catch
            {
                //This shouldn't prevent the application from running
            }

            var settingsPath = "";
            try
            {
                //Load/Create default settings

                var oldSettingsPath = Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName,
                    Resources.OldConfigFile);

                settingsPath = oldSettingsPath;

                //1. Provide early notification that the user does not have permission to write.
                new FileIOPermission(FileIOPermissionAccess.Write, settingsPath).Demand();

                settingsPath = Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName,
                    Resources.ConfigFile);
                new FileIOPermission(FileIOPermissionAccess.Write, settingsPath).Demand();

                //Open and close the settings file to ensure write access
                File.Open(settingsPath, FileMode.OpenOrCreate, FileAccess.ReadWrite).Close();

                ISettingsSource jsonSource = new JsonSettings();
                jsonSource.SetFilePath(settingsPath);

                Settings = new ConfigurationSettings(jsonSource);

                if (File.Exists(oldSettingsPath))
                {
                    try
                    {
                        //Load old settings and copy them to json
                        ISettingsSource iniSource = new IniSettings();
                        iniSource.SetFilePath(oldSettingsPath);

                        var oldSettings = new ConfigurationSettings(iniSource);
                        Settings.LoadFrom(oldSettings);
                    }
                    catch
                    {
                        Settings.CreateDefaults();
                    }
                    finally
                    {
                        File.Delete(oldSettingsPath);
                    }
                }

                Settings.CreateDefaults();
            }
            catch
            {
                MessageBox.Show(
                    string.Format(
                        "Error creating/reading settings file [{0}]. Make sure you have read/write access to this file.\r\nOr try running as Administrator",
                        settingsPath),
                    "Setings File - Cannot Access");
                return;
            }

            try
            {
                Application.Run(AudioSwitcher.Instance);
            }
            catch (Exception ex)
            {
                var title = "An Unexpected Error Occurred";
                var text = ex.Message + Environment.NewLine + Environment.NewLine + ex;

                var edf = new ExceptionDisplayForm(title, ex);
                edf.ShowDialog();
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            //Ensure the icon disappears from tray
            AudioSwitcher.Instance.TrayIconVisible = false;
        }
    }
}
Contact GitHub API Training Shop Blog About
© 2016 GitHub, Inc. Terms Privacy Security Status Help

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using NLog;
using System.Collections;
using Markdig;
using Markdig.Wpf;

namespace PluginManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string RootDir = Directory.GetCurrentDirectory();
        public string DeviceProviderDir = RootDir + "\\DeviceProvider";
        public string X86Dir = RootDir + "\\x86";
        public string X64Dir = RootDir + "\\x64";
        public JsonIndex packageIndex;
        public Settings settings;
        public bool restartMainExe;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void OpenUrl(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Process.Start(e.Parameter.ToString());
        }

        public MainWindow()
        {
            InitializeComponent();

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "PluginManager.log" };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;

            try
            {
                Logger.Info("Trying to read settings file...");
                string settingsJson = File.ReadAllText("PluginManager.Settings.json");
                settings = JsonConvert.DeserializeObject<Settings>(settingsJson);
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to read settings file.");
                Logger.Error(ex);
                MessageBox.Show("Unable to read settings file.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                settings = new Settings();
                string settingsJson = JsonConvert.SerializeObject(settings);
                File.WriteAllText("PluginManager.Settings.json", settingsJson);
                Logger.Info("New settings file has been created.");
            }


            Process[] processes = Process.GetProcessesByName(settings.MainExe);
            if (processes.Length == 0)
            {
                Logger.Info("Main exe is not running.");
                restartMainExe = false;
            }
            else
            {
                Logger.Info("Trying to kill " + settings.MainExe);
                foreach (var process in processes)
                {
                    process.Kill();
                }
                restartMainExe = true;
            }
                


            if (!Directory.Exists(DeviceProviderDir))
            {
                Logger.Error("No DeviceProvider directory found, creating one.");
                Directory.CreateDirectory(DeviceProviderDir);
            }
            if (!Directory.Exists(X86Dir))
            {
                Logger.Error("No x86 directory found, creating one.");
                Directory.CreateDirectory(X86Dir);
            }
            if (!Directory.Exists(X64Dir))
            {
                Logger.Error("No x64 directory found, creating one.");
                Directory.CreateDirectory(X64Dir);
            }


            try
            {
                Logger.Info("Trying to read package index...");
                WebClient webClient = new WebClient();
                string jsonString = webClient.DownloadString(settings.IndexURL);
                packageIndex = JsonConvert.DeserializeObject<JsonIndex>(jsonString);
                Logger.Info("Package index read succesfully.");
                MarketplaceTitle.Text = packageIndex.MarketplaceName;
                AppWindow.Title = packageIndex.MarketplaceName;

               if(packageIndex.AdditionalPackageURLs != null)
                {
                    foreach (string url in packageIndex.AdditionalPackageURLs)
                    {
                        string PluginJson = webClient.DownloadString(url);
                        Plugin pluginToAdd = JsonConvert.DeserializeObject<Plugin>(PluginJson);
                        packageIndex.Packages.Add(pluginToAdd);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Unable to read package index.");
                Logger.Error(ex);
                MessageBox.Show("Unable to download package index.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }



            foreach (Plugin plugin in packageIndex.Packages)
            {
                int FilesFound = 0;
                Logger.Info("Plugin " + plugin.Name + " should have " + plugin.TotalFiles.ToString() + " files.");

                if (plugin.RootFiles != null)
                {
                    foreach (ResourceFile file in plugin.RootFiles)
                    {
                        string path = RootDir + "\\" + file.LocalFile;
                        if (File.Exists(path))
                        {
                            FilesFound += 1;
                        }
                    }
                }

                if (plugin.DPFiles != null)
                {
                    foreach (ResourceFile file in plugin.DPFiles)
                    {
                        string path = DeviceProviderDir + "\\" + file.LocalFile;
                        if (File.Exists(path))
                        {
                            FilesFound += 1;
                        }
                    }
                }

                if (plugin.x86Files != null)
                {
                    foreach (ResourceFile file in plugin.x86Files)
                    {
                        string path = X86Dir + "\\" + file.LocalFile;
                        if (File.Exists(path))
                        {
                            FilesFound += 1;
                        }
                    }
                }

                if (plugin.x64Files != null)
                {
                    foreach (ResourceFile file in plugin.x64Files)
                    {
                        string path = X64Dir + "\\" + file.LocalFile;
                        if (File.Exists(path))
                        {
                            FilesFound += 1;
                        }
                    }
                }

                Logger.Info(FilesFound.ToString() + " files have been found.");

                if (FilesFound == plugin.TotalFiles)
                {
                    plugin.IsInstalled = true;
                } else
                {
                    plugin.IsInstalled = false;
                }
            }
            PluginBox.ItemsSource = packageIndex.Packages;
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            Button cmd = (Button)sender;
            if (cmd.DataContext is Plugin)
            {
                Plugin currentPlugin = (Plugin)cmd.DataContext;

                Logger.Info("Installing plugin " + currentPlugin.Name);

                if (currentPlugin.Warning)
                {
                    if (MessageBox.Show(currentPlugin.WarningText, "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        Logger.Info("Installation of plugin " + currentPlugin.Name + "aborted by user.");
                    }
                    else
                    {
                        InstallPlugin(currentPlugin);
                    }
                } 
                else
                {
                    InstallPlugin(currentPlugin);
                }

                

            }
            PluginBox.ItemsSource = null;
            PluginBox.ItemsSource = packageIndex.Packages;
        }

        private void InstallPlugin(Plugin currentPlugin)
        {
            WebClient webClient = new WebClient();

            if (currentPlugin.RootFiles != null)
            {
                foreach (ResourceFile file in currentPlugin.RootFiles)
                {
                    string path = RootDir + "\\" + file.LocalFile;
                    if (!File.Exists(path))
                    {
                        try
                        {
                            webClient.DownloadFile(file.RemoteURL, path);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            Logger.Error("URL: " + file.RemoteURL);
                        }
                    }
                }
            }

            if (currentPlugin.DPFiles != null)
            {
                foreach (ResourceFile file in currentPlugin.DPFiles)
                {
                    string path = DeviceProviderDir + "\\" + file.LocalFile;
                    if (!File.Exists(path))
                    {
                        try
                        {
                            webClient.DownloadFile(file.RemoteURL, path);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            Logger.Error("URL: " + file.RemoteURL);
                        }
                    }
                }
            }

            if (currentPlugin.x64Files != null)
            {
                foreach (ResourceFile file in currentPlugin.x64Files)
                {
                    string path = X64Dir + "\\" + file.LocalFile;
                    if (!File.Exists(path))
                    {
                        try
                        {
                            webClient.DownloadFile(file.RemoteURL, path);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }
            }

            if (currentPlugin.x86Files != null)
            {
                foreach (ResourceFile file in currentPlugin.x86Files)
                {
                    string path = X86Dir + "\\" + file.LocalFile;
                    if (!File.Exists(path))
                    {
                        try
                        {
                            webClient.DownloadFile(file.RemoteURL, path);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }
            }
            currentPlugin.IsInstalled = true;
            Logger.Info("Plugin " + currentPlugin.Name + " has been installed.");
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Button cmd = (Button)sender;
            if (cmd.DataContext is Plugin)
            {
                Plugin currentPlugin = (Plugin)cmd.DataContext;
                Logger.Info("Removing plugin " + currentPlugin.Name);

                if (currentPlugin.RootFiles != null)
                {
                    foreach (ResourceFile file in currentPlugin.RootFiles)
                    {
                        string path = RootDir + "\\" + file.LocalFile;
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }

                if (currentPlugin.DPFiles != null)
                {
                    foreach (ResourceFile file in currentPlugin.DPFiles)
                    {
                        string path = DeviceProviderDir + "\\" + file.LocalFile;
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }

                if (currentPlugin.x64Files != null)
                {
                    foreach (ResourceFile file in currentPlugin.x64Files)
                    {
                        string path = X64Dir + "\\" + file.LocalFile;
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }

                if (currentPlugin.x86Files != null)
                {
                    foreach (ResourceFile file in currentPlugin.x86Files)
                    {
                        string path = X86Dir + "\\" + file.LocalFile;
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
                currentPlugin.IsInstalled = false;
                Logger.Info("Plugin " + currentPlugin.Name + " has been removed.");
            }
            PluginBox.ItemsSource = null;
            PluginBox.ItemsSource = packageIndex.Packages;
        }

        private void Github_link(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Fanman03/RGBPluginManager");
        }

        private void Website_link(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.rgbsync.com");
        }

        private void AppWindow_Closed(object sender, EventArgs e)
        {
            if (restartMainExe)
            {
                Process[] processes = Process.GetProcessesByName(settings.MainExe);
                if (processes.Length == 0)
                {
                    string ExeName = settings.MainExe + ".exe";
                    Process.Start(ExeName);
                }
            }
        }

        private void AppWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (restartMainExe)
            {
                Process[] processes = Process.GetProcessesByName(settings.MainExe);
                if (processes.Length == 0)
                {
                    string ExeName = settings.MainExe + ".exe";
                    Process.Start(ExeName);
                }
            }
        }
    }
}

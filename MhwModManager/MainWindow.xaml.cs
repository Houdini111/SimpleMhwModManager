using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace MhwModManager
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string tmpFolder = Path.Combine(Path.GetTempPath(), "SMMMaddMod");

        public MainWindow()
        {
            InitializeComponent();

            UpdateModsList();

            App.Updater();
        }

        private void UpdateModsList()
        {
            modListBox.Items.Clear();
            App.GetMods();

            foreach (var mod in App.Mods)
            {
                // Increase the order count if didn't exist
                if (mod.order >= App.Mods.Count())
                    mod.order = App.Mods.Count() - 1;

                var modItem = new CheckBox
                {
                    Tag = mod.order,
                    Content = mod.name
                };
                modItem.IsChecked = mod.activated;
                modItem.Checked += itemChecked;
                modItem.Unchecked += itemChecked;
                // Adding the context menu
                var style = Application.Current.FindResource("CheckBoxListItem") as Style;
                modItem.Style = style;
                foreach (MenuItem item in modItem.ContextMenu.Items)
                {
                    if (item.Tag.ToString() == "rem")
                    {
                        item.Click -= remModContext_Click;
                        item.Click += remModContext_Click;
                    }
                    else if (item.Tag.ToString() == "edit")
                    {
                        item.Click -= editModContext_Click;
                        item.Click += editModContext_Click;
                    }
                }

                modListBox.Items.Add(modItem);
            }

            App.Settings.SaveSettingsJSON();

            // Check if there's mods conflicts
            bool conflict = false;
            for (int i = 0; i < App.Mods.Count() - 1; i++)
            {
                for (int j = 0; j < App.Mods.Count() - 1; j++)
                {
                    if (i == j) { continue; }
                    if (!CheckFiles(Path.Combine(App.ModsPath, App.Mods[i].path), Path.Combine(App.ModsPath, App.Mods[i + 1].path)))
                    {
                        conflict = true;
                        var firstModItem = modListBox.Items[App.Mods[i].order];
                        var secondModItem = modListBox.Items[App.Mods[i + 1].order];
                        (firstModItem as CheckBox).Foreground = Brushes.Red;
                        (firstModItem as CheckBox).ToolTip = "Conflict with " + App.Mods[i + 1].name;
                        (secondModItem as CheckBox).Foreground = Brushes.Red;
                        (secondModItem as CheckBox).ToolTip = "Conflict with " + App.Mods[i].name;
                    }
                }
            }
            if (!conflict)
            {
                //foreach(ModInfo mod in App.Mods)
                //{
                //    DirectoryCopy( App.Settings.settings.mhw_path)
                //}
            }
            else
            {
                string path = Path.Combine(App.Settings.settings.mhw_path, "nativePC");
                if(Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                Directory.CreateDirectory(path);
            }
        }

        private bool CheckFiles(string pathFirstMod, string pathSecondMod)
        {
            // Get the subdirectories for the mod directory.
            DirectoryInfo dirFirstMod = new DirectoryInfo(pathFirstMod);
            DirectoryInfo[] dirsFirstMod = dirFirstMod.GetDirectories();

            // Get the files in the directory
            FileInfo[] filesFirstMod = dirFirstMod.GetFiles();

            // Get the subdirectories for the nativePC directory.
            DirectoryInfo dirSecondMod = new DirectoryInfo(pathSecondMod);
            DirectoryInfo[] dirsSecondMod = dirSecondMod.GetDirectories();

            // Get the files in the directory
            FileInfo[] filesSecondMod = dirSecondMod.GetFiles();

            foreach (FileInfo firstFile in filesFirstMod)
            {
                foreach (FileInfo secondFile in filesSecondMod)
                { 
                    if (firstFile.Name == secondFile.Name && firstFile.Name != "mod.info")
                    {
                        return false; // return false if conflict
                    }
                }
            }

            foreach (DirectoryInfo subdirFirstMod in dirsFirstMod)
            {
                foreach (DirectoryInfo subdirSecondMod in dirsSecondMod)
                {
                    return CheckFiles(subdirFirstMod.FullName, subdirSecondMod.FullName);
                }
            }

            return true; // return true if everything's fine
        }

        private void addMod_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WinForms.OpenFileDialog();

            if (!Directory.Exists(tmpFolder))
            {
                Directory.CreateDirectory(tmpFolder);
            }

            // Dialog to select a mod archive
            dialog.DefaultExt = "zip";
            dialog.Filter = "Mod Archives (*.zip)|*.zip|all files|*";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                foreach (string file in dialog.FileNames)
                {
                    InstallMod(file);
                }
            }
            UpdateModsList();
        }

        private void InstallMod(string path)
        {
            // Separate the path and unzip mod
            string[] splitPath = path.Split('\\');
            string folderName = splitPath[splitPath.GetLength(0) - 1].Split('.')[0];
            if (path.EndsWith(".zip"))
            {
                ZipFile.ExtractToDirectory(path, tmpFolder);
            }
            else
            {
                DirectoryCopy(path, Path.Combine(tmpFolder, folderName), true);
            }

            // Get the name of the extracted folder (without the .zip at the end), not the full path
            if (!InstallMod(tmpFolder, folderName))
            {
                if (Directory.Exists(Path.Combine(App.ModsPath, folderName)))
                {
                    MessageBox.Show("This mod is already installed", "Simple MHW Mod Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Directory.Move(Path.Combine(tmpFolder, folderName), Path.Combine(App.ModsPath, folderName));
                    MessageBox.Show("nativePC not found, interpreting root as nativePC, this mod may not work correctly.", "Simple MHW Mod Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // If the install failed
                //MessageBox.Show("nativePC not found... Please check if it's exist in the mod...", "Simple MHW Mod Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Directory.Delete(tmpFolder, true);

            App.GetMods(); // Refresh the modlist
        }

        private bool InstallMod(string path, string name)
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                if (dir.Equals(Path.Combine(path, "nativePC"), StringComparison.OrdinalIgnoreCase))
                {
                    if(Directory.GetParent(dir).Name.Equals("ings")) { continue; } //Not the right nativePC
                    if (!Directory.Exists(Path.Combine(App.ModsPath, name)))
                    {
                        // If the mod isn't installed
                        Directory.Move(dir, Path.Combine(App.ModsPath, name));
                    }
                    else
                    {
                        MessageBox.Show("This mod is already installed", "Simple MHW Mod Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return true;
                }
                else
                {
                    if(InstallMod(dir, name)) { return true; } 
                    //Successfully installed in this variant, end
                    //Otherwise, keep going
                }
            }
            return false;
        }

        private void remMod_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mod in modListBox.SelectedItems)
            {
                var caller = (mod as CheckBox);
                var index = int.Parse(caller.Tag.ToString());
                Directory.Delete(Path.Combine(App.ModsPath, App.Mods[index].path), true);
                App.Mods.RemoveAt(index);
                for (int i = index; i < App.Mods.Count(); i++)
                {
                    App.Mods[i].order = i;
                }
            }
            UpdateModsList();
        }

        private void startGame_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.Combine(App.Settings.settings.mhw_path, "MonsterHunterWorld.exe"));
        }

        private void refreshMod_Click(object sender, RoutedEventArgs e)
        {
            UpdateModsList();
        }

        private void webMod_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.nexusmods.com/monsterhunterworld");
        }

        private void settingsMod_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsDialog();
            settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();
        }

        private void itemChecked(object sender, RoutedEventArgs e)
        {
            // Get the full path of the mod
            var index = int.Parse((sender as CheckBox).Tag.ToString());
            var mod = Path.Combine(App.ModsPath, App.Mods[index].path);

            if ((sender as CheckBox).IsChecked.Value == true)
            {
                // Install the mod
                DirectoryCopy(mod, Path.Combine(App.Settings.settings.mhw_path, "nativePC"), true);
            }
            else
            {
                // Desinstall the mod
                DeleteMod(mod, Path.Combine(App.Settings.settings.mhw_path, "nativePC"));
                CleanFolder(Path.Combine(App.Settings.settings.mhw_path, "nativePC"));
            }

            var info = App.Mods[index];
            info.GenInfo(mod);
            info.activated = (sender as CheckBox).IsChecked.Value;
            info.SaveSettingsJSON(mod);
        }

        // Credits to https://docs.microsoft.com/fr-fr/dotnet/standard/io/how-to-copy-directories
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (!file.Name.Equals("mod.info"))
                {
                    if (!File.Exists(temppath))
                    {
                        file.CopyTo(temppath, false);
                    }
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static void DeleteMod(string modPath, string folder)
        {
            // Get the subdirectories for the mod directory.
            DirectoryInfo modDir = new DirectoryInfo(modPath);
            DirectoryInfo[] modDirs = modDir.GetDirectories();

            // Get the files in the directory
            FileInfo[] modFiles = modDir.GetFiles();

            // Get the subdirectories for the nativePC directory.
            DirectoryInfo dir = new DirectoryInfo(folder);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Get the files in the directory
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo modfile in modFiles)
            {
                foreach (FileInfo file in files)
                {
                    if (modfile.Name == file.Name)
                    {
                        file.Delete();
                        break;
                    }
                }
            }

            foreach (DirectoryInfo submoddir in modDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    DeleteMod(submoddir.FullName, subdir.FullName);
                }
            }
        }

        private static void CleanFolder(string folder)
        {
            DirectoryInfo dir = new DirectoryInfo(folder);
            DirectoryInfo[] dirs = dir.GetDirectories();

            foreach (DirectoryInfo subdir in dirs)
            {
                CleanFolder(subdir.FullName);
                if (!Directory.EnumerateFileSystemEntries(subdir.FullName).Any())
                {
                    // If the directory is empty
                    Directory.Delete(subdir.FullName);
                }
            }
        }

        private void remModContext_Click(object sender, RoutedEventArgs e)
        {
            var caller = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as CheckBox);
            var index = int.Parse(caller.Tag.ToString());
            Directory.Delete(Path.Combine(App.ModsPath, App.Mods[index].path), true);
            App.Mods.RemoveAt(index);
            for (int i = index; i < App.Mods.Count(); i++)
            {
                App.Mods[i].order = i;
                App.Mods[i].SaveSettingsJSON(Path.Combine(App.ModsPath, App.Mods[i].path));
            }

            UpdateModsList();
        }

        private void editModContext_Click(object sender, RoutedEventArgs e)
        {
            var caller = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as CheckBox);
            editMod(App.Mods[(int)caller.Tag]);
        }
        
        private void editMod(ModInfo mod)
        {
            var editWindow = new EditWindow(mod);
            editWindow.Owner = Application.Current.MainWindow;
            editWindow.ShowDialog();
            UpdateModsList();
        }

        private void DropHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach(string file in files)
                {
                    if(file.EndsWith(".zip") || Directory.Exists(Path.Combine(file, "nativePC")))
                    {
                        InstallMod(file);
                    }
                }
            }
            UpdateModsList();
        }
    }
}
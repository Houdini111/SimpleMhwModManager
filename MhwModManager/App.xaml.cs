using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using WinForms = System.Windows.Forms;
using Newtonsoft.Json;
using System.Diagnostics;
using common;

namespace MhwModManager
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string AppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SMMM");
        public static string ModsPath = Path.Combine(AppData, "mods");
        public static Setting Settings = new Setting();
        public static string SettingsPath = Path.Combine(AppData, "settings.json");
        public static List<ModInfo> Mods;
        public static Dictionary<Armor.ARMOR_SLOT, List<CombinedArmor>> armors;
        public static List<Weapon> weapons;

        public App()
        {
            Settings.GenConfig();
            if (!Directory.Exists(Settings.settings.mhw_path))
            {
                MessageBox.Show("The path to MHW is not found, you have to install the game first, or if the game is already installed, open it", "Simple MHW Mod Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                var dialog = new WinForms.FolderBrowserDialog();
                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    Settings.settings.mhw_path = dialog.SelectedPath;
                    Settings.SaveSettingsJSON();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            if(!File.Exists("armor.json") || !File.Exists("weapon.json"))
            {
                ProcessStartInfo psi = new ProcessStartInfo("WebScraperToJson.exe");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                Process p = new Process();
                p.StartInfo = psi;
                p.Start();
                p.WaitForExit();
            }
            if (!File.Exists("armor.json") || !File.Exists("weapon.json"))
            {
                MessageBox.Show("Cannot find or scrape for armor and weapon data."+Environment.NewLine+"Editing changed equipment will be disabled.", "Simple MHW Mod Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                using (StreamReader file = new StreamReader("armor.json"))
                {
                    List<Armor> preArmors = JsonConvert.DeserializeObject<List<Armor>>(file.ReadToEnd());
                    armors = CombineArmors(preArmors);
                }
                using (StreamReader file = new StreamReader("weapon.json"))
                {
                    weapons = JsonConvert.DeserializeObject<List<Weapon>>(file.ReadToEnd());
                }
            }
        }

        public static void GetMods()
        {
            // This list contain the ModInfos and the folder name of each mod
            Mods = new List<ModInfo>();

            if (!Directory.Exists(ModsPath))
                Directory.CreateDirectory(ModsPath);

            var modFolder = new DirectoryInfo(ModsPath);

            int i = 0;
            foreach (var mod in modFolder.GetDirectories())
            {
                var info = new ModInfo();
                info.GenInfo(mod.FullName);
                // If the order change the generation of the list
                if (info.order >= Mods.Count)
                {
                    Mods.Add(info);
                }
                else
                {
                    if (i > 0)
                    {
                        if (info.order == Mods[i - 1].order)
                        {
                            info.order++;
                            info.SaveSettingsJSON(mod.FullName);
                        }
                    }
                    Mods.Insert(info.order, info);
                }
                i++;
            }
        }

        public async static void Updater()
        {
            /* Credits to WildGoat07 : https://github.com/WildGoat07 */
            var github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("SimpleMhwModManager"));
            var lastRelease = await github.Repository.Release.GetLatest(234864718);
            var current = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (new Version(lastRelease.TagName) > current)
            {
                var result = MessageBox.Show("A new version is available, do you want to download it now ?", "SMMM", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start("https://github.com/oxypomme/SimpleMhwModManager/releases/latest");
                }
            }
        }

        private static Dictionary<Armor.ARMOR_SLOT, List<CombinedArmor>> CombineArmors(List<Armor> armor)
        {
            //Some armors use the same file location, and thus can't be separated
            //e.g., Brigade Suit uses 
            //  pl pl/m_equip/pl036_0000
            //and
            //  pl/f_equip/pl036_0000
            //which is shared with both Brigade Suit α and Brigade Suit β.
            //This is to combine them so that I don't present redundant and impossible choices

            armor.RemoveAll(a => a.name.Contains("dummy"));
            
            Dictionary<Armor.ARMOR_SLOT, Dictionary<string, List<Armor>>> matches = new Dictionary<Armor.ARMOR_SLOT, Dictionary<string, List<Armor>>>();
            matches.Add(Armor.ARMOR_SLOT.HEAD, new Dictionary<string, List<Armor>>());
            matches.Add(Armor.ARMOR_SLOT.CHEST, new Dictionary<string, List<Armor>>());
            matches.Add(Armor.ARMOR_SLOT.ARMS, new Dictionary<string, List<Armor>>());
            matches.Add(Armor.ARMOR_SLOT.WAIST, new Dictionary<string, List<Armor>>());
            matches.Add(Armor.ARMOR_SLOT.LEGS, new Dictionary<string, List<Armor>>());
            List<Armor> exceptions = new List<Armor>();
            foreach(Armor piece in armor)
            {
                Dictionary<string, List<Armor>> dict = matches[piece.type];
                List<Armor> male;
                List<Armor> female;

                if(dict.ContainsKey(piece.male_location)) { male = dict[piece.male_location]; }
                else
                {
                    male = new List<Armor>();
                    dict[piece.male_location] = male;
                }
                if(dict.ContainsKey(piece.female_location)) { female = dict[piece.female_location]; }
                else
                {
                    female = new List<Armor>();
                    dict[piece.female_location] = female;
                }

                if(!male.Contains(piece)) { male.Add(piece); }
                if(!female.Contains(piece)) { female.Add(piece); }
            }

            Dictionary<Armor.ARMOR_SLOT, Dictionary<string, List<Armor>>> combinedMatches = new Dictionary<Armor.ARMOR_SLOT, Dictionary<string, List<Armor>>>();
            combinedMatches.Add(Armor.ARMOR_SLOT.HEAD, new Dictionary<string, List<Armor>>());
            combinedMatches.Add(Armor.ARMOR_SLOT.CHEST, new Dictionary<string, List<Armor>>());
            combinedMatches.Add(Armor.ARMOR_SLOT.ARMS, new Dictionary<string, List<Armor>>());
            combinedMatches.Add(Armor.ARMOR_SLOT.WAIST, new Dictionary<string, List<Armor>>());
            combinedMatches.Add(Armor.ARMOR_SLOT.LEGS, new Dictionary<string, List<Armor>>());
            foreach(KeyValuePair<Armor.ARMOR_SLOT, Dictionary<string, List<Armor>>> slot in matches)
            {
                foreach(KeyValuePair<string, List<Armor>> location in slot.Value)
                {
                    Dictionary<string, List<Armor>> dict = combinedMatches[slot.Key];
                    string loc = location.Key;
                    if(loc.Equals("None", StringComparison.InvariantCultureIgnoreCase)) { continue; } //Because the armors with a "None" have a valid location in the other, they're already handled there
                    string pairLoc;

                    if(loc.Contains("m_equip")) { pairLoc = loc.Replace("m_equip", "f_equip"); }
                    else { pairLoc = loc.Replace("f_equip", "m_equip"); }

                    bool missingPair = false;
                    foreach(Armor a in location.Value)
                    {
                        //Don't try to combine the matched pair if the "match" is "None"
                        if(loc.Contains("m_equip") && a.female_location.Equals("None", StringComparison.InvariantCultureIgnoreCase)) { continue; } 
                        else if(loc.Contains("f_equip") && a.male_location.Equals("None", StringComparison.InvariantCultureIgnoreCase)) { continue; } 

                        //Sanity check to ensure that male and female always just pair exactly
                        if(!slot.Value[pairLoc].Contains(a))
                        {
                            MessageBox.Show($"Male/Female variant not found at expected matched location for {a.name} with location {loc}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            missingPair = true;
                            break;
                        }
                    }
                    if(!missingPair)
                    {
                        string m_loc = location.Value[0].male_location; //I'm just standardizing to male location. Shouldn't matter as logng as it is consistent
                        List<Armor> list;
                        if(dict.ContainsKey(m_loc)) { list = dict[m_loc]; }
                        else
                        {
                            list = new List<Armor>();
                            dict.Add(m_loc, list);
                        }
                        dict[m_loc] =  dict[m_loc].Union(location.Value).ToList();
                    }
                }
            }

            Dictionary<Armor.ARMOR_SLOT, List<CombinedArmor>> finalized = new Dictionary<Armor.ARMOR_SLOT, List<CombinedArmor>>();
            finalized.Add(Armor.ARMOR_SLOT.HEAD, new List<CombinedArmor>());
            finalized.Add(Armor.ARMOR_SLOT.CHEST, new List<CombinedArmor>());
            finalized.Add(Armor.ARMOR_SLOT.ARMS, new List<CombinedArmor>());
            finalized.Add(Armor.ARMOR_SLOT.WAIST, new List<CombinedArmor>());
            finalized.Add(Armor.ARMOR_SLOT.LEGS, new List<CombinedArmor>());
            foreach(KeyValuePair<Armor.ARMOR_SLOT, Dictionary<string, List<Armor>>> slotDict in combinedMatches)
            {
                List<CombinedArmor> list = finalized[slotDict.Key];
                
                foreach(KeyValuePair<string, List<Armor>> armorList in slotDict.Value)
                {
                    string combinedName = "";

                    if(armorList.Value.Count == 1)
                    {
                        combinedName = armorList.Value[0].name;
                    }
                    else
                    {
                        List<string> names = armorList.Value.Select(a => a.name).ToList();

                        int minLen = names.Select(name => name.Length).ToList().Min();
                        int i = 0;
                        for(; i < minLen; i++)
                        {
                            char thisChar = names[0][i];
                            bool bad = false;
                            foreach(string name in names)
                            {
                                if(name[i] != thisChar)
                                {
                                    bad = true;
                                    break;
                                }
                            }
                            if(bad) { break; }
                        }
                        if(i >= 0)
                        {
                            combinedName = names[0].Substring(0, i);
                        }
                        
                        List<string> unique = new List<string>();
                        foreach(string name in names)
                        {
                            string thisUnique = name.Substring(combinedName.Length).Trim();
                            if(thisUnique.Length == 0) { thisUnique = "base"; }
                            unique.Add(thisUnique);
                        }

                        if(combinedName.Length > 0)
                        {
                            combinedName += " (" + String.Join(", ", unique) + ")";
                        }
                        else
                        {
                            combinedName = String.Join(", ", unique);
                        }
                    }
                    list.Add(new CombinedArmor(combinedName, armorList.Value)); //Pair armors under their combined name
                }
            }

            return finalized;
        }
    }
    
    public class ModInfo
    {
        public string path { get; set; }
        public bool activated { get; set; }
        public int order { get; set; }
        public string name { get; set; }
        public List<Armor> originalReplacedArmors { get; set; }
        public List<Weapon> originalReplacedWeapons { get; set; }
        public List<Armor> replacedArmors { get; set; }
        public List<Weapon> replacedWeapons { get; set; }

        public void GenInfo(string path, int? index = null)
        {
            if (!File.Exists(Path.Combine(path, "mod.info")))
            {
                this.path = path;
                activated = false;
                if (index != null)
                {
                    order = index.Value;
                }
                else
                {
                    order = App.Mods.Count();
                }

                // Get the name of the extracted folder (without the .zip at the end), not the full path
                var foldName = path.Split('\\');
                name = foldName[foldName.GetLength(0) - 1].Split('.')[0];
                
                replacedArmors = App.armors.FindAll(a => 
                    (Directory.Exists(Path.Combine(path, a.male_location)) && !String.IsNullOrWhiteSpace(a.male_location)) || 
                    (Directory.Exists(Path.Combine(path, a.female_location)) && !String.IsNullOrWhiteSpace(a.female_location))).ToList();
                replacedWeapons = App.weapons.FindAll(w => 
                    Directory.Exists(Path.Combine(path, w.main_model)) || 
                    (Directory.Exists(Path.Combine(path, w.part_model)) && !String.IsNullOrWhiteSpace(w.part_model))).ToList();
                originalReplacedArmors = replacedArmors;
                originalReplacedWeapons = replacedWeapons;

                SaveSettingsJSON(path);
            }
            else
            {
                using (StreamReader file = new StreamReader(Path.Combine(path, "mod.info")))
                {
                    ModInfo sets = JsonConvert.DeserializeObject<ModInfo>(file.ReadToEnd());
                    this.path = sets.path;
                    this.activated = sets.activated;
                    this.order = sets.order;
                    this.name = sets.name;
                    this.replacedArmors = sets.replacedArmors;
                    this.replacedWeapons = sets.replacedWeapons;
                }
            }
        }

        public void SaveSettingsJSON(string path)
        {
            using (StreamWriter file = new StreamWriter(Path.Combine(path, "mod.info")))
            {
                file.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }
    }
}
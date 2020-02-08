using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using common;

namespace MhwModManager
{
    /// <summary>
    /// Logique d'interaction pour EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private string originalName;
        private int originalOrder;

        #region Collections
        public ObservableCollection<CombinedArmor> allHeads { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> allChests { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> allArms { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> allWaists { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> allLegs { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<Weapon> allMain { get; set; } = new ObservableCollection<Weapon>();
        public ObservableCollection<Weapon> allPart { get; set; } = new ObservableCollection<Weapon>();

        public ObservableCollection<CombinedArmor> displayedAllHeads { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> displayedAllChests { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> displayedAllArms { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> displayedAllWaists { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> displayedAllLegs { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<Weapon> displayedAllMain { get; set; } = new ObservableCollection<Weapon>();
        public ObservableCollection<Weapon> displayedAllPart { get; set; } = new ObservableCollection<Weapon>();

        public ObservableCollection<CombinedArmor> addedHeads { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> addedChests { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> addedArms { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> addedWaists { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> addedLegs { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<Weapon> addedMain { get; set; } = new ObservableCollection<Weapon>();
        public ObservableCollection<Weapon> addedPart { get; set; } = new ObservableCollection<Weapon>();

        public ObservableCollection<CombinedArmor> displayedAddedHeads { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> displayedAddedChests { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> displayedAddedArms { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> displayedAddedWaists { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<CombinedArmor> displayedAddedLegs { get; set; } = new ObservableCollection<CombinedArmor>();
        public ObservableCollection<Weapon> displayedAddedMain { get; set; } = new ObservableCollection<Weapon>();
        public ObservableCollection<Weapon> displayedAddedPart { get; set; } = new ObservableCollection<Weapon>();
        #endregion


        public EditWindow(ModInfo info)
        {
            InitializeComponent();
            originalName = info.name;
            originalOrder = info.order;
            nameTB.Text = info.name;
            orderTB.Text = info.order.ToString();

            this.DataContext = this;

            #region Adding items
            info.replacedArmors.Where(a => a.type == Armor.ARMOR_SLOT.HEAD).ToList().ForEach(a => { addedHeads.Add(a); displayedAddedHeads.Add(a); });
            info.replacedArmors.Where(a => a.type == Armor.ARMOR_SLOT.CHEST).ToList().ForEach(a => { addedChests.Add(a); displayedAddedChests.Add(a); });
            info.replacedArmors.Where(a => a.type == Armor.ARMOR_SLOT.ARMS).ToList().ForEach(a => { addedArms.Add(a); displayedAddedArms.Add(a); });
            info.replacedArmors.Where(a => a.type == Armor.ARMOR_SLOT.WAIST).ToList().ForEach(a => { addedWaists.Add(a); displayedAddedWaists.Add(a); });
            info.replacedArmors.Where(a => a.type == Armor.ARMOR_SLOT.LEGS).ToList().ForEach(a => { addedLegs.Add(a); displayedAddedLegs.Add(a); });
            info.replacedWeapons.Where(w => !String.IsNullOrWhiteSpace(w.main_model)).ToList().ForEach(w => { addedMain.Add(w); displayedAddedMain.Add(w); });
            info.replacedWeapons.Where(w => !String.IsNullOrWhiteSpace(w.part_model)).ToList().ForEach(w => { addedPart.Add(w); displayedAddedPart.Add(w); });

            App.armors[Armor.ARMOR_SLOT.HEAD].Where(!addedHeads.Select(x => x.ID).Contains(a.ID)).ToList().ForEach(a => { allHeads.Add(a); displayedAllHeads.Add(a); });
            App.armors[Armor.ARMOR_SLOT.CHEST].Where(!addedChests.Select(x => x.ID).Contains(a.ID)).ToList().ForEach(a => { allChests.Add(a); displayedAllChests.Add(a); });
            App.armors[Armor.ARMOR_SLOT.ARMS].Where(!addedArms.Select(x => x.ID).Contains(a.ID)).ToList().ForEach(a => { allArms.Add(a); displayedAllArms.Add(a); });
            App.armors[Armor.ARMOR_SLOT.WAIST].Where(!addedWaists.Select(x => x.ID).Contains(a.ID)).ToList().ForEach(a => { allWaists.Add(a); displayedAllWaists.Add(a); });
            App.armors[Armor.ARMOR_SLOT.LEGS].Where(!addedLegs.Select(x => x.ID).Contains(a.ID)).ToList().ForEach(a => { allLegs.Add(a); displayedAllLegs.Add(a); });
            App.weapons.Where(w => !String.IsNullOrWhiteSpace(w.main_model) && !addedMain.Select(x => x.ID).Contains(w.ID)).ToList().ForEach(w => { allMain.Add(w); displayedAllMain.Add(w); });
            App.weapons.Where(w => !String.IsNullOrWhiteSpace(w.part_model) && !addedPart.Select(x => x.ID).Contains(w.ID)).ToList().ForEach(w => { allPart.Add(w); displayedAllPart.Add(w); });
            #endregion

            #region Tab visibility
            if (addedHeads.Count < 1 && addedChests.Count < 1 && addedArms.Count < 1 && addedWaists.Count < 1 && addedLegs.Count < 1)
            {
                PossibleArmorTab.Visibility = Visibility.Collapsed;
                AddedArmorTab.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (addedHeads.Count < 1)
                {
                    AddedHeadTab.Visibility = Visibility.Collapsed;
                    AddedHeadTab.Visibility = Visibility.Collapsed;
                }
                if (addedChests.Count < 1)
                {
                    PossibleChestTab.Visibility = Visibility.Collapsed;
                    AddedChestTab.Visibility = Visibility.Collapsed;
                }
                if (addedArms.Count < 1)
                {
                    PossibleArmTab.Visibility = Visibility.Collapsed;
                    AddedArmTab.Visibility = Visibility.Collapsed;
                }
                if (addedWaists.Count < 1)
                {
                    PossibleWaistTab.Visibility = Visibility.Collapsed;
                    AddedWaistTab.Visibility = Visibility.Collapsed;
                }
                if (addedLegs.Count < 1)
                {
                    PossibleLegTab.Visibility = Visibility.Collapsed;
                    AddedLegTab.Visibility = Visibility.Collapsed;
                }
            }

            if (addedMain.Count < 1 && addedPart.Count < 1)
            {
                PossibleWeaponTab.Visibility = Visibility.Collapsed;
                AddedWeaponTab.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (addedMain.Count < 1)
                {
                    PossibleMainTab.Visibility = Visibility.Collapsed;
                    AddedMainTab.Visibility = Visibility.Collapsed;
                }
                if (addedPart.Count < 1)
                {
                    PossiblePartialTab.Visibility = Visibility.Collapsed;
                    AddedPartialTab.Visibility = Visibility.Collapsed;
                }
            }
            #endregion
        }

        private void saveBTN_Click(object sender, RoutedEventArgs e)
        {
            int order = Int32.Parse(orderTB.Text);
            ModInfo mod = App.Mods[originalOrder];
            mod.name = nameTB.Text;
            //TODO 
            //App.Mods[order].replacedArmors = selectedItems.Items;
            if (order != originalOrder)
            {
                mod.order = order;
                //Number already exists, swap them
                if(App.Mods[order] != null)
                {
                    ModInfo swappedWith = App.Mods[order];
                    swappedWith.order = originalOrder;
                    App.Mods[order] = mod;
                    App.Mods[originalOrder] = swappedWith;
                }
            }
            mod.SaveSettingsJSON(System.IO.Path.Combine(App.ModsPath, mod.path));
            Close();
        }

        private void cancelBTN_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnSearchTextChanged(object sender, RoutedEventArgs e)
        {
            UpdatePossibleList();
            UpdateAddedList();
        }
        
        private void UpdatePossibleList()
        {
            string text = SearchBox.Text;

            displayedAllHeads.Clear();
            allHeads.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(a => displayedAllHeads.Add(a));
            displayedAllChests.Clear();
            allChests.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(a => displayedAllChests.Add(a));
            displayedAllArms.Clear();
            allArms.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(a => displayedAllArms.Add(a));
            displayedAllWaists.Clear();
            allWaists.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(a => displayedAllWaists.Add(a));
            displayedAllLegs.Clear();
            allLegs.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(a => displayedAllLegs.Add(a));

            displayedAllMain.Clear();
            allMain.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(w => displayedAllMain.Add(w));
            displayedAllPart.Clear();
            allPart.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(w => displayedAllPart.Add(w));
        }

        private void UpdateAddedList()
        {
            string text = SearchBox.Text;

            displayedAddedHeads.Clear();
            addedHeads.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(a => displayedAddedHeads.Add(a));
            displayedAddedChests.Clear();
            addedChests.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(a => displayedAddedChests.Add(a));
            displayedAddedArms.Clear();
            addedArms.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(a => displayedAddedArms.Add(a));
            displayedAddedWaists.Clear();
            addedWaists.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(a => displayedAddedWaists.Add(a));
            displayedAddedLegs.Clear();
            addedLegs.Where(a => a.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(a => displayedAddedLegs.Add(a));

            displayedAddedMain.Clear();
            addedMain.Where(w => w.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(w => displayedAddedMain.Add(w));
            displayedAddedPart.Clear();
            addedPart.Where(w => w.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList().ForEach(w => displayedAddedPart.Add(w));
        }

        private void AddSetBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void addBTN_Click(object sender, RoutedEventArgs e)
        {
            TabControl childTabControl = PossibleTabControl.SelectedContent as TabControl;
            ListView list = childTabControl.SelectedContent as ListView;
            List<Item> items = list.SelectedItems.Cast<Item>().ToList();

            if(items.Count > 0)
            {
                if(items[0] is Armor)
                {
                    Armor.ARMOR_SLOT slot = ((Armor)items[0]).type;
                    ObservableCollection<CombinedArmor> toAddTo = null; //Remember, this is a shallow (reference) copy
                    ObservableCollection<CombinedArmor> toRemoveFrom = null;
                    if(slot == Armor.ARMOR_SLOT.HEAD) { toAddTo = addedHeads; toRemoveFrom = allHeads; }
                    else if(slot == Armor.ARMOR_SLOT.CHEST) { toAddTo = addedChests; toRemoveFrom = allChests; }
                    else if(slot == Armor.ARMOR_SLOT.ARMS) { toAddTo = addedArms; toRemoveFrom = allArms; }
                    else if(slot == Armor.ARMOR_SLOT.WAIST) { toAddTo = addedWaists; toRemoveFrom = allWaists; }
                    else if(slot == Armor.ARMOR_SLOT.LEGS) { toAddTo = addedLegs; toRemoveFrom = allLegs; }

                    if(toAddTo != null)
                    {
                        items.ForEach(a => toAddTo.Add((Armor)a));
                        items.ForEach(a => toRemoveFrom.Remove((Armor)a));

                        UpdateAddedList();
                        UpdatePossibleList();
                    }
                }
                else if(items[0] is Weapon)
                {
                    //TODO
                    //((Weapon)items[0]).weapon_type
                }
            }
        }

        private void removeBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveSetBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DefaultBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TabControlSelectionChanged(object sender, RoutedEventArgs e)
        {
            TabControl from = sender as TabControl;
            TabControl to = null;
            if(from == PossibleTabControl) { to = AddedTabControl; }
            else if(from == AddedTabControl) { to = PossibleTabControl; }
            else
            {
                TabItem parent = from.Parent as TabItem;
                if(parent.Parent == PossibleTabControl) { to = ((TabItem)AddedTabControl.SelectedItem).Content as TabControl; }
                else if(parent.Parent == AddedTabControl) { to = ((TabItem)PossibleTabControl.SelectedItem).Content as TabControl; }
            }

            if(from != null && to != null)
            {
                to.SelectionChanged -= TabControlSelectionChanged; //Prevent it from calling this function when it is changed from this code
                to.SelectedIndex = from.SelectedIndex;
                to.SelectionChanged += TabControlSelectionChanged;
            }
        }

        private void NumericInputPreviewer(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            try
            {
                int parsed = Int32.Parse(e.Text);
            }
            catch (Exception ex)
            {
                e.Handled = false;
            }
        }
    }
}
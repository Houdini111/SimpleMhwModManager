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

        public ObservableCollection<Armor> possibleHeads { get; set; } = new ObservableCollection<Armor>();
        public ObservableCollection<Armor> possibleChests { get; set; } = new ObservableCollection<Armor>();
        public ObservableCollection<Armor> possibleArms { get; set; } = new ObservableCollection<Armor>();
        public ObservableCollection<Armor> possibleWaists { get; set; } = new ObservableCollection<Armor>();
        public ObservableCollection<Armor> possibleLegs { get; set; } = new ObservableCollection<Armor>();

        public ObservableCollection<Weapon> possibleMain { get; set; } = new ObservableCollection<Weapon>();
        public ObservableCollection<Weapon> possiblePart { get; set; } = new ObservableCollection<Weapon>();

        public ObservableCollection<Armor> addedHeads { get; set; } = new ObservableCollection<Armor>();
        public ObservableCollection<Armor> addedChests { get; set; } = new ObservableCollection<Armor>();
        public ObservableCollection<Armor> addedArms { get; set; } = new ObservableCollection<Armor>();
        public ObservableCollection<Armor> addedWaists { get; set; } = new ObservableCollection<Armor>();
        public ObservableCollection<Armor> addedLegs { get; set; } = new ObservableCollection<Armor>();
        
        public ObservableCollection<Weapon> addedMain { get; set; } = new ObservableCollection<Weapon>();
        public ObservableCollection<Weapon> addedPart { get; set; } = new ObservableCollection<Weapon>();
        

        public EditWindow(ModInfo info)
        {
            InitializeComponent();
            originalName = info.name;
            originalOrder = info.order;
            nameTB.Text = info.name;
            orderTB.Text = info.order.ToString();

            this.DataContext = this;

            #region Adding items
            App.armors.Where(a => a.type == Armor.ARMOR_SLOT.HEAD).ToList().ForEach(a => possibleHeads.Add(a));
            App.armors.Where(a => a.type == Armor.ARMOR_SLOT.CHEST).ToList().ForEach(a => possibleChests.Add(a));
            App.armors.Where(a => a.type == Armor.ARMOR_SLOT.ARMS).ToList().ForEach(a => possibleArms.Add(a));
            App.armors.Where(a => a.type == Armor.ARMOR_SLOT.WAIST).ToList().ForEach(a => possibleWaists.Add(a));
            App.armors.Where(a => a.type == Armor.ARMOR_SLOT.LEGS).ToList().ForEach(a => possibleLegs.Add(a));

            App.weapons.Where(w => !String.IsNullOrWhiteSpace(w.main_model)).ToList().ForEach(w => possibleMain.Add(w));
            App.weapons.Where(w => !String.IsNullOrWhiteSpace(w.part_model)).ToList().ForEach(w => possiblePart.Add(w));
            

            info.replacedArmors.Where(a => a.type == Armor.ARMOR_SLOT.HEAD).ToList().ForEach(a => addedHeads.Add(a));
            info.replacedArmors.Where(a => a.type == Armor.ARMOR_SLOT.CHEST).ToList().ForEach(a => addedChests.Add(a));
            info.replacedArmors.Where(a => a.type == Armor.ARMOR_SLOT.ARMS).ToList().ForEach(a => addedArms.Add(a));
            info.replacedArmors.Where(a => a.type == Armor.ARMOR_SLOT.WAIST).ToList().ForEach(a => addedWaists.Add(a));
            info.replacedArmors.Where(a => a.type == Armor.ARMOR_SLOT.LEGS).ToList().ForEach(a => addedLegs.Add(a));

            info.replacedWeapons.Where(w => !String.IsNullOrWhiteSpace(w.main_model)).ToList().ForEach(w => addedMain.Add(w));
            info.replacedWeapons.Where(w => !String.IsNullOrWhiteSpace(w.part_model)).ToList().ForEach(w => addedPart.Add(w));
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

        private void OnPossibleTextChanged(object sender, RoutedEventArgs e)
        {
            string text = ((TextBox)sender).Text;
            //possible = allChoices.FindAll(c => c.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void OnAddedTextChanged(object sender, RoutedEventArgs e)
        {
            string text = ((TextBox)sender).Text;
            //selected = selectedChoices.FindAll(c => c.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void addBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void removeBTN_Click(object sender, RoutedEventArgs e)
        {

        }
        
        private void NumericInputPreviewer(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            try
            {
                int parsed = Int32.Parse(e.Text);
            }
            catch(Exception ex)
            {
                e.Handled = false;
            }
        }
    }
}
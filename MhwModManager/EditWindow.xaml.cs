using System;
using System.Collections.Generic;
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
    public partial class EditWindow : Window, INotifyPropertyChanged
    {
        private static List<Item> allChoices;
        private List<Item> selectedChoices;

        private static List<Item> _selected;
        public List<Item> selected
        {
            get { return _selected; }
            set { _selected = value; OnPropertyChanged("selected"); }
        }

        private static List<Item> _possible;
        public List<Item> possible
        {
            get { return _possible; }
            set { _possible = value; OnPropertyChanged("possible"); }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string prop)
        {
            if(this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        private string originalName;
        private int originalOrder;


        public EditWindow(ModInfo info)
        {
            InitializeComponent();
            originalName = info.name;
            originalOrder = info.order;
            nameTB.Text = info.name;
            orderTB.Text = info.order.ToString();

            this.DataContext = this;
            
            selectedChoices = new List<Item>();
            selectedChoices.AddRange(info.replacedArmors);
            selectedChoices.AddRange(info.replacedWeapons);
            this.selected = selectedChoices;

            allChoices = new List<Item>();
            if (info.replacedArmors != null && info.replacedArmors.Count > 0) { allChoices.AddRange(App.armors); }
            if (info.replacedWeapons != null && info.replacedWeapons.Count > 0) { allChoices.AddRange(App.weapons); }
            allChoices.RemoveAll(c => selectedChoices.Contains(c));
            this.possible = allChoices;

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
            possible = allChoices.FindAll(c => c.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void OnAddedTextChanged(object sender, RoutedEventArgs e)
        {
            string text = ((TextBox)sender).Text;
            selected = selectedChoices.FindAll(c => c.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
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
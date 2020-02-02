using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using common;

namespace MhwModManager
{
    /// <summary>
    /// Logique d'interaction pour EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window, INotifyPropertyChanged
    {
        private static List<Item> _choices;
        public List<Item> choices
        {
            get { return _choices; }
            set { _choices = value; OnPropertyChanged("choices"); }
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

            List<Item> temp = new List<Item>();
            temp.AddRange(App.armors);
            temp.AddRange(App.weapons);
            this.choices = temp;
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
        }

        private void cancelBTN_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
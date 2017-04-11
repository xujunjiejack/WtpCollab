using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace CollabClient
{
    /// <summary>
    /// Interaction logic for FieldCustomization.xaml
    /// </summary>
    public partial class FieldCustomization : Window
    {
        public FieldCustomization()
        {
            InitializeComponent();
        }

        public Table CurrentTable
        {
            get;
            set;
        }

        private void UserTable_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                CollectionViewSource.GetDefaultView(AvailableField.ItemsSource).Filter = TableFilter;
            }
            catch (NullReferenceException exception)
            {
                
            }
        }

        private void AddField_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine((String)AvailableField.SelectedItem);
                String selectedColumn = (String)AvailableField.SelectedItem;
                Table selectedTable = (Table)UserTable.SelectedItem;
                selectedTable.AvailableColumns.Remove(selectedColumn);
                selectedTable.UserSpecifiedFields.Add(selectedColumn);
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Please select a table in Available Table List");
            }
        }

        private void RemoveField_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine((String)AvailableField.SelectedItem);
                String victim = (String)UsedField.SelectedItem;
                Table selectedTable = (Table)UserTable.SelectedItem;
                selectedTable.UserSpecifiedFields.Remove(victim);
                selectedTable.AvailableColumns.Add(victim);

            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Please select a table in Your Table List");
            }

        }

        private void ImportAllTables_Click(object sender, RoutedEventArgs e)
        {

            Table selectedTable;
            try
            {
                selectedTable = (Table)UserTable.SelectedItem;
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Please select a table in Your Table List");
                return;
            }

            if (selectedTable.AvailableColumns == null)
            {
                return;
            }

            foreach (String col in selectedTable.AvailableColumns.ToList<String>()){
                selectedTable.AvailableColumns.Remove(col);
                selectedTable.UserSpecifiedFields.Add(col);
            }
        }

        private void ExportAllColumns_Click(object sender, RoutedEventArgs e)
        {
            Table selectedTable;
            try
            {
                selectedTable = (Table)UserTable.SelectedItem;
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Please select a table in Your Table List");
                return;
            }

            if (selectedTable.UserSpecifiedFields == null)
            {
                return;
            }

            foreach (String col in selectedTable.UserSpecifiedFields.ToList<String>())
            {
                selectedTable.UserSpecifiedFields.Remove(col);
                selectedTable.AvailableColumns.Add(col);
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CollectionViewSource.GetDefaultView(AvailableField.ItemsSource) != null)
            {
                CollectionViewSource.GetDefaultView(AvailableField.ItemsSource).Refresh();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(AvailableField.ItemsSource).Filter = TableFilter;
        }

        private bool TableFilter(object item)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(AvailableField.ItemsSource);
            if (String.IsNullOrEmpty(SearchBar.Text))
            {
                return true;
            }
            else
            {
                String col = item as String;
                return col.Contains(SearchBar.Text);
            }
        }

    }
}

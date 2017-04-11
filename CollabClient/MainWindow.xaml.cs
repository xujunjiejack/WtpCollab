using System;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using CsvHelper;
using System.IO;
using System.Diagnostics;
using System.Data.Odbc;
using System.Windows.Controls.Primitives;
using System.Diagnostics;

namespace CollabClient
{
    public enum TableType {Twin, Family};

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private String _userTableName = "";
        private MainController _mc;
        private CollabClientViewModel _viewModel;
        UserTableTracker _userTableTracker;
        MainController MC
        {
            get
            {
                return _mc;
            }
        }

        public CollabClientViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = new CollabClientViewModel();
                }
                return _viewModel;
            }
        }

        private bool _doUseAllFields = true;
        private DataTables _availableTables;
        private DataTables _userTables;

        DataTables AvailableDataTables
        {
            get
            {
                return _availableTables;
            }
        }

        DataTables UserDataTables
        {
            get
            {
                return _userTables;
            }
        }

        public bool doUseAllFields
        {
            get
            {
                return _doUseAllFields;
            }

            set
            {
                _doUseAllFields = value;   
            }
        }
    
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {   
            
            this._mc = new MainController(ViewModel);
            this._availableTables = ViewModel.AvailableTables;
            AvailableTables.ItemsSource = this._availableTables;

            // Get an empty datatable
            this._userTables = ViewModel.UserTables;
            UserTables.ItemsSource = this._userTables;
            CollectionViewSource.GetDefaultView(AvailableTables.ItemsSource).Filter = TableFilter;
            this._userTableTracker = new UserTableTracker(ConnectionCoordinater.SharedConnectionCoordinator);
        }

        // User table name check for duplicate. 
        // I'm thinking about make it possible to have a mode for just throw away stuff. 
        // One thing is to differentiate between inner user and guest
         
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (_userTableName.Equals(""))
            {
                MessageBox.Show("Please enter your table name");
                return;
            }

            if (_userTableTracker.tablenameIsDuplicate(_userTableName))
            {
                MessageBox.Show("Table name already existed. Please change a user table name");
                return;
            }

            String finalUserTableName;
            try
            {
                finalUserTableName =_mc.run(_userTableName, _userTables);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            performJobDone(finalUserTableName);
        }

        private void performJobDone(String tablename)
        {
            
            MessageBoxResult result = MessageBox.Show("Data file has been built. Do you want to open file and the folder?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _mc.openCSVFile(tablename);
                _mc.openCSVFolder();
            }
            else
            {

            }
            _availableTables.Add(new Table(tablename));
        }

        // For user reset data
        private string beforeString = "";
        private string laterString = "";

        private void newTableNameEntrance_TextChanged(object sender, TextChangedEventArgs e)
        {
            beforeString = laterString;
            // Change whether the name is available
            if (newTableNameEntrance.Text.Contains(" "))
            {
                MessageBox.Show("Please don't have space in table name");
                newTableNameEntrance.Text = beforeString;
                return;
            }
            laterString = newTableNameEntrance.Text;
            _userTableName = "user_" + laterString;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine(((Table)AvailableTables.SelectedItem).TableName);
                Table selectedTable = (Table)AvailableTables.SelectedItem;
                _availableTables.Remove(selectedTable);
                // This will set the selected table to the right field mode 
                ViewModel.addTableToUserTables(selectedTable);

            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Please select a table in Available Table List");
            }
        }

        private void TableLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void UserTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Debug.WriteLine(((Table)UserTables.SelectedItem).TableName);
                Table victim = (Table)UserTables.SelectedItem;
                _userTables.Remove(victim);
                this._availableTables.Add(victim);

            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Please select a table in Your Table List");
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {   
            if (CollectionViewSource.GetDefaultView(AvailableTables.ItemsSource) != null){
                CollectionViewSource.GetDefaultView(AvailableTables.ItemsSource).Refresh();
            }
        }

        private bool TableFilter(object item)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(AvailableTables.ItemsSource);
            if (String.IsNullOrEmpty(SearchBar.Text))
            {
                return true;
            }
            else
            {
                Table table = item as Table;
                return table.TableName.Contains(SearchBar.Text);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CheckBox_checked gets called");
        }

        private void DoUseAllFields_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        private void CustomizationButton_Click(object sender, RoutedEventArgs e)
        {
            FieldCustomization fieldCustiomization = new FieldCustomization();
            fieldCustiomization.DataContext = UserDataTables;
            fieldCustiomization.Show();
        }

        private void CheckDB_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("\\\\wcs-cifs\\wc\\wtp_collab\\SqliteReader\\DB Browser for SQLite.exe");
        }

        private void ElasticSearchEntrance_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.waisman.wisc.edu/twinresearch/playground/testingpage.html");
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
           // I'm still thinking about the performance issue about the reset button, and the implementation of it since I'm using the data binding.
            // I want to reset everything as quickly as possible, and update the UI responsively.
            // I'll probably want that 
           //ViewModel.resetBuilder();
        }

    }
}
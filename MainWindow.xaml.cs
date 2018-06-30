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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Sql;
namespace WpfDatabase
{
    // connect string 
    //Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\HyperReap\Documents\MyDatabase.mdf;Integrated Security=True;Connect Timeout=30

 


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            InitializeComponent();
           
        }

        /// <summary>
        /// opens package control panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageChooseBtn_Click(object sender, RoutedEventArgs e)
        {
            Package package = new Package();
            package.Show();
        }        

        /// <summary>
        /// opens Storage control panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StorageChooseBtn_Click(object sender, RoutedEventArgs e)
        {
            Storage storage = new Storage();
            storage.Show();
            
            
        }

        /// <summary>
        /// opens Storage control panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void APStoringChooseBtn_Click(object sender, RoutedEventArgs e)
        {
            APStorage airplaneStorage = new APStorage();
            airplaneStorage.Show();
            
        }
    }
}

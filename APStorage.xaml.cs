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
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace WpfDatabase
{
    /// <summary>
    /// Interaction logic for APStorage.xaml
    /// </summary>
    public partial class APStorage : Window
    {
        /// <summary>
        /// List of all Packages and AirPlanes
        /// Stores/loades Packages from/to AP
        /// checks weight limits while loading AP
        /// </summary>
        public APStorage()
        {
            InitializeComponent();
        }


        #region helpful variables
        string ConStr = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
//      string ConStr = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\HyperReap\\Documents\\MyDatabase.mdf;Integrated Security=True;Connect Timeout=30";
        SqlConnection conn = null;

        string Letadlo = ""; //selected airplane
        string packId = ""; //selected package ID

        #endregion

        #region components
        /// <summary>
        /// sets basic enviroment after start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ListBoxLetUpdate();
        }




        #region Buttons

        /// <summary>
        /// opens new window to add Package to Location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StorageLBLet.SelectedItem == null)
            {
                MessageBox.Show("Nejprve je nutné určit letadlo, do kterého nakládáme");
                return;
            }
            APStorageADD APstorageADD = new APStorageADD(Letadlo);
            APstorageADD.ShowDialog();
        }

        /// <summary>
        /// Takes selected Package in Location and sets its Location to NULL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RmvBtn_Click(object sender, RoutedEventArgs e)
        {
            using (conn = new SqlConnection(ConStr))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "Update Package Set Letadlo = @nullLokace Where Id = @packId";
                cmd.CommandType = CommandType.Text;
                try
                {
                    cmd.Parameters.AddWithValue("packId", SqlDbType.Int).Value = packId;
                    cmd.Parameters.AddWithValue("nullLokace", SqlDbType.NChar).Value = "";
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Odstraněno");
                }
                catch { MessageBox.Show("Remove neprobehl"); };
            }
            RmvBtn.IsEnabled = false;

            // just to clear storagelbPack
            ResetLB(StorageLBLet);
        }

        #endregion

        #region ListBoxes

        /// <summary>
        /// when selection in 1st LB is changed, shows Names of packages with this Location
        /// </summary>
        /// <param name="sender"> Listbox </param>
        /// <param name="e"></param>
        public void StorageLBLet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;

            if (lb.SelectedItem != null)
                Letadlo = lb.SelectedItem.ToString();
            // list for Names
            List<string> stringie = new List<string>();
            // List for IDs
            List<int> intie = new List<int>();

            using (conn = new SqlConnection(ConStr))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "Select  Jmeno,Id from Package where Letadlo = @newLetadlo";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("newLetadlo", SqlDbType.NChar).Value = Letadlo;
                conn.Open();
                try
                {
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        stringie.Add(dr.GetString(0)); //Names
                        intie.Add(dr.GetInt32(1)); //IDs
                    }

                    //Checks if multiple pakcages has same name
                    if (intie.Count >= 2)
                    {
                        IsStringEqual(stringie, intie);
                    }

                    StorageLBPacks.ItemsSource = stringie;
                    dr.Close();

                }
                catch { };
            }
        }

        /// <summary>
        /// gives back ID and Name of Selected item of second ListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StorageLBPacks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            string selectedName = "";
            try
            {
                selectedName = lb.SelectedItem.ToString();

                // 1stly splits selected name into Name and ID
                // if ID is not written behind Name use sqlCommand (cuz' there can be only one of the same name in this Location) to get packID
                // else splits ID_part string into 2 strings and var packID is the 2nd one 

                string[] words = selectedName.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                string Name = words[0]; //gets name

                //pokud dostanu Name a jeste #ID
                if (words.Length > 1)
                {
                    string[] ids = words[1].Split(':');
                    packId = ids[1]; //gets id
                }

                //TODO vymyslet jak získat ID když to tam není napsaný //DONE
                else
                    using (conn = new SqlConnection(ConStr))
                    {
                        SqlCommand cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT Id FROM Package WHERE Letadlo = @newLetadlo AND Jmeno = @newJmeno";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("newJmeno", SqlDbType.NChar).Value = Name;
                        cmd.Parameters.AddWithValue("newLetadlo", SqlDbType.NChar).Value = Letadlo;
                        conn.Open();
                        try
                        {
                            SqlDataReader dr = cmd.ExecuteReader();
                            while (dr.Read())
                                packId = dr.GetInt32(0).ToString(); //gets ID
                            dr.Close();
                        }
                        catch { };
                    }
            }
            catch { };

            RmvBtn.IsEnabled = true;
        }
        #endregion

        #endregion

        #region basic SQLhelpers
        /// <summary>
        /// gets columns/rows into list of strings and puts them into ListBox
        /// </summary>
        /// <param name="query">SQL cmd</param>
        /// <param name="LB">ListBox type item</param>
        private void ExSqlCmdLB(string query, ListBox LB)
        {
            List<string> data = new List<string>();

            using (conn = new SqlConnection(ConStr))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    data.Add(dr.GetString(0));

                LB.ItemsSource = data;
                dr.Close();
            }
        }


        /// <summary>
        /// updates ListBox with locations after changes
        /// </summary>
        private void ListBoxLetUpdate()
        {
            ExSqlCmdLB("SELECT Letadlo FROM Letadla ORDER BY Letadlo",StorageLBLet);
        }



        /// <summary>
        ///  Checks if 2 strings are equal, if yes gives them ID  
        /// </summary>
        /// <param name="strings"> List of strings to compare</param>
        /// <param name="IDs"> List of ints representing ID</param>
        private void IsStringEqual(List<string> strings, List<int> IDs)
        {
            for (int i = 0, firstAppearance = 0; i < strings.Count - 1; i++)
            {
                if (Equals(strings[i], strings[i + 1]))
                {
                    if (firstAppearance == 0)
                    {
                        MessageBox.Show("Pozor: nalezlo se více Packů se stejným jménem ");
                        firstAppearance++;
                    }
                    strings[i] += "#ID:" + IDs[i].ToString();

                    if (i == strings.Count - 2)
                        strings[i + 1] += "#ID:" + IDs[i + 1].ToString();
                }
            }
        }

        /// <summary>
        /// resets ListBox's selection, shows changes right after
        /// </summary>
        /// <param name="LB"></param>
        private void ResetLB(ListBox LB)
        {
            int tmp = LB.SelectedIndex;
            LB.SelectedItem = null;
            LB.SelectedIndex = tmp;
        }


        #endregion
    }
}

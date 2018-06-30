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
using System.Configuration; //config manager
using System.Data;
using System.Data.SqlClient;


namespace WpfDatabase
{
    /// <summary>
    /// Interaction logic for Package.xaml
    /// </summary>
    public partial class Package : Window
    {
        /// <summary>
        /// DataGrid of all Packages (DG cuz we need to know exactly not just name)
        /// 4 btns forr ADD, EDIT, Reset and Remove
        /// </summary>
        public Package()
        {
            InitializeComponent();
        }

        #region helpful variables

        string ConStr = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
//        string ConStr = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\HyperReap\\Documents\\MyDatabase.mdf;Integrated Security=True;Connect Timeout=30";
        SqlConnection conn = null;

        #endregion

        #region components
        /// <summary>
        /// sets basic Datagrid to shwo data after start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataGridUpdate();
            IdTBox.Text = get1stID().ToString();
        }

        #region Buttons

        /// <summary>
        /// Adds new Package into DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButt_Click(object sender, RoutedEventArgs e)
        {
  
            string query = "INSERT INTO Package(Id, Jmeno, Vaha, Letadlo, Lokace)" +
                "VALUES(@newId, @newJmeno, @newVaha, @newLetadlo, @newLokace)";
            ChooseExecute(query,0);

            AddButt.IsEnabled = true;
            EditButt.IsEnabled = false;
            RemButt.IsEnabled = false;
        }

        /// <summary>
        /// resets the frame as it was at the beginning of App
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e"></param>
        private void ResetButt_Click(object sender, RoutedEventArgs e)
        {
            ResetAllBtns();

        }

        /// <summary>
        /// Edits selected Package in DB
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e"></param>
        private void EditButt_Click(object sender, RoutedEventArgs e)
        {
            string query = "UPDATE Package SET Jmeno = @newJmeno, Vaha = @newVaha, Letadlo = @newLetadlo, Lokace = @newLokace WHERE Id = @newId";
            ChooseExecute(query,1);
        }


        /// <summary>
        /// removes selected Package from DB
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e"></param>
        private void RemButt_Click(object sender, RoutedEventArgs e)
        {
            string query = "DELETE FROM Package WHERE Id = @newId";
            ChooseExecute(query,2);
            ResetAllBtns(); 
        }
        #endregion

        #region Datagrid 

        /// <summary>
        /// Gets the selected Datagrid item into TextBoxes
        /// </summary>
        /// <param name="sender">Datagrid</param>
        /// <param name="e"></param>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            DataRowView dr = dg.SelectedItem as DataRowView;
            if (dr != null)
            {
                IdTBox.Text = dr["Id"].ToString();
                NameTBox.Text = dr["Jmeno"].ToString();
                WeightTBox.Text = dr["Vaha"].ToString();
                APTBox.Text = dr["Letadlo"].ToString();
                LocTBox.Text = dr["Lokace"].ToString();

                IdTBox.IsEnabled = false;
                AddButt.IsEnabled = false;
                EditButt.IsEnabled = true;
                RemButt.IsEnabled = true;
            }
        }

        #endregion

        #endregion

        #region helpers

        /// <summary>
        /// updates Datagrid after changes
        /// </summary>
        private void DataGridUpdate()
        {
            using(conn = new SqlConnection(ConStr))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Package ORDER BY Id";
                cmd.CommandType = CommandType.Text;

                DataTable dt = new DataTable();

                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                DataGrid.ItemsSource = dt.DefaultView;
                dr.Close();
            }
        }



        /// <summary>
        /// sets all TextBoxes to NULL and enable/disable btns
        /// </summary>
        private void ResetAllBtns()
        {
            IdTBox.Text = get1stID().ToString();
            NameTBox.Text = "";
            WeightTBox.Text = "";
            APTBox.Text = "";
            LocTBox.Text = "";

            IdTBox.IsEnabled = true;
            AddButt.IsEnabled = true;
            EditButt.IsEnabled = false;
            RemButt.IsEnabled = false;


        }



        /// <summary>
        /// chooses which button we used and executes sql cmds - states 0-Add, 1-Edit, 2-Rem
        /// </summary>
        /// <param name="query">sql cmd</param>
        /// <param name="state">whcih btn we used</param>
        private void ChooseExecute(string query, int state)
        {
            string msg = "";
            int n = 0;

            using (conn = new SqlConnection(ConStr))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                #region switch
                switch (state)
                {
//          ADD
                    case 0:
                        try
                        {
                            cmd.Parameters.AddWithValue("newId", SqlDbType.Int).Value = Int32.Parse(IdTBox.Text);
                            //if (newId==ID) MessageBox.Show("TOTO ID je jiz pouzivane"); // ale ve sprave bz si to mohl hldiat zamestnanec
                            cmd.Parameters.AddWithValue("newJmeno", SqlDbType.NText).Value = NameTBox.Text;
                            cmd.Parameters.AddWithValue("newVaha", SqlDbType.Int).Value = Int32.Parse(WeightTBox.Text);

                            //Letadlo
                            if (APTBox.Text == "")
                                cmd.Parameters.AddWithValue("newLetadlo", SqlDbType.NText).Value = "";
                            else if (LocTBox.Text == "")
                                cmd.Parameters.AddWithValue("newLetadlo", SqlDbType.NText).Value = APTBox.Text;
                            else
                            {
                                MessageBox.Show("Nemůže být balík v letadle a zároveň na skladě");
                                break;
                            }
                            //Lokace
                            if (LocTBox.Text == "")
                                cmd.Parameters.AddWithValue("newLokace", SqlDbType.NText).Value = "";
                            else if (APTBox.Text == "")
                                cmd.Parameters.AddWithValue("newLokace", SqlDbType.NText).Value = LocTBox.Text;
                            else
                            {
                                MessageBox.Show("Nemůže být balík v letadle a zároveň na skladě");
                                break;
                            }
                            msg = "Řádek úspěšně přidán";
                        }
                        catch { MessageBox.Show("Oblasti : 'Id' 'Jmeno' 'Vaha' nesmí být prázdné"); };
                        break;
//          EDIT
                    case 1:
                        try
                        {
                            cmd.Parameters.Add("newId", SqlDbType.Int).Value = Int32.Parse(IdTBox.Text);
                            cmd.Parameters.Add("newJmeno", SqlDbType.NText).Value = NameTBox.Text;
                            cmd.Parameters.Add("newVaha", SqlDbType.Int).Value = Int32.Parse(WeightTBox.Text);
                            //Letadlo
                            if (APTBox.Text == "")
                                cmd.Parameters.AddWithValue("newLetadlo", SqlDbType.NText).Value = "";
                            else if (LocTBox.Text == "")
                                cmd.Parameters.AddWithValue("newLetadlo", SqlDbType.NText).Value = APTBox.Text;
                            else
                            {
                                MessageBox.Show("Nemůže být balík v letadle a zároveň na skladě");
                                break;
                            }
                            //Lokace
                            if (LocTBox.Text == "")
                                cmd.Parameters.AddWithValue("newLokace", SqlDbType.NText).Value = "";
                            else if (APTBox.Text == "")
                                cmd.Parameters.AddWithValue("newLokace", SqlDbType.NText).Value = LocTBox.Text;
                            else
                            {
                                MessageBox.Show("Nemůže být balík v letadle a zároveň na skladě");
                                break;
                            }

                            msg = "Řádek úspěšně upraven";
                        }
                        catch { MessageBox.Show("Oblasti : 'Id' 'Jmeno' 'Vaha' nesmí být prázdné"); };
                        break;
//          REMOVE
                    case 2:
                        cmd.Parameters.Add("newId", SqlDbType.Int).Value = Int32.Parse(IdTBox.Text);
                        msg = "Úspěšně odstraněno";
                        break;

                    default:
                        msg = "nic se nestalo";
                        break;
                }
                #endregion


                try
                {
                    conn.Open();
                    n = cmd.ExecuteNonQuery();
                }
                catch { };
            }
            if (n > 0)
            {
                this.DataGridUpdate();
                MessageBox.Show(msg);
            }
            
        }
        /// <summary>
        /// Finds maximum ID from Package and returns it+1
        /// </summary>
        /// <returns>ID+1</returns>
        private int get1stID()
        {
            int packID=1;
            using(conn = new SqlConnection(ConStr))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT MAX(Id) FROM Package ";
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while(dr.Read())
                    packID = dr.GetInt32(0);
            }           
            return ++packID;
        }

        #endregion


    }

}

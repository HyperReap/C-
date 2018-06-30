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
    public partial class APStorageADD : Window
    {
#region helpful variables
        string ConStr = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        //string ConStr = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\HyperReap\\Documents\\MyDatabase.mdf;Integrated Security=True;Connect Timeout=30";
        SqlConnection conn = null;

        string Letadlo = "";
        string packId = "";
        int Cap = 0;
#endregion
        /// <summary>
        /// Constructor - AddingWindow to selected Airplane
        /// </summary>
        /// <param name="Letadlo"></param>
        public APStorageADD(string Letadlo)
        {
            InitializeComponent();
            this.Letadlo = Letadlo;
        }

        #region components

        /// <summary>
        /// sets basic enviroment after start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataGridUpdate();
            this.IsCapacityOK(this.CapacityCount());
            // Storage.StorageLBLoc.SelectedIndex = 0; neco takoveho pri closed a takz udelat metodu na obnovu tabulek
        }

        /// <summary>
        /// Confirms selected Package to laod to Airplane
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            int n = 0;

            DataGridSelectionCHECK(DGstorage);
            
            if ( !IsCapacityOK( CapacityCount() ) )
            {
                MessageBox.Show("Tento balik je jiz prilis tezky");
                return;
            }

            using (conn = new SqlConnection(ConStr))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Package SET Letadlo = @newLetadlo WHERE Id = @packId";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("newLetadlo", SqlDbType.NVarChar).Value = Letadlo;
                cmd.Parameters.AddWithValue("packId", SqlDbType.NVarChar).Value = packId;
                conn.Open();
                n = cmd.ExecuteNonQuery();
            }
            if (n > 0)
                MessageBox.Show("Přidáno");

            this.DataGridUpdate();
        }

        /// <summary>
        /// Gets ID and Vaha from Datagrid and saves it into Global Vars
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DGstorage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            if (dg.SelectedItem == null)
                return;
            DataRowView dr = dg.SelectedItem as DataRowView;
            if (dr == null)
                return;
            packId = dr["Id"].ToString();
            Cap = Int32.Parse(dr["Vaha"].ToString());
        }

        #endregion

        #region helpers

        /// <summary>
        /// updates Datagrid after changes
        /// </summary>
        private void DataGridUpdate()
        {
            using (conn = new SqlConnection(ConStr))
            {
                SqlCommand cmd = conn.CreateCommand();
                // ukaž všechny balíky který nemaj námi vybrané letadlo a nejsou nikde uloženy
                cmd.CommandText = "SELECT * FROM Package WHERE Letadlo != @newLetadlo AND Lokace = @nullLokace ORDER BY Id";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("newLetadlo", SqlDbType.NVarChar).Value = Letadlo;
                cmd.Parameters.AddWithValue("nullLokace", SqlDbType.NVarChar).Value = "";
                DataTable dt = new DataTable();

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                DGstorage.ItemsSource = dt.DefaultView;
                dr.Close();
            }
        }

        /// <summary>
        /// Checks if selection isVALID
        /// </summary>
        /// <param name="DGstorage"></param>
        private void DataGridSelectionCHECK(DataGrid DGstorage)
        {
            if (DGstorage.SelectedItem == null)
                MessageBox.Show("Nejprve je nutné vybrat, co přidat, poté kliknout 'OK'");
        }

        /// <summary>
        /// Counts the Weight of Packs loaded in selected plane
        /// </summary>
        private int CapacityCount()
        {
            using (conn = new SqlConnection(ConStr))
            {
                int Capacity=0;
                SqlCommand cmd = conn.CreateCommand(); //poladit sql dotaz u SUM
                cmd.CommandText = "SELECT SUM(Vaha) FROM Package WHERE Letadlo = @newLetadlo";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("newLetadlo", SqlDbType.NChar).Value = Letadlo;
                conn.Open();
                try
                {
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                        Capacity = dr.GetInt32(0);
                }
                catch { };
                Capacity += Cap;
                return Capacity;
                    
                                    
            }
        }

        /// <summary>
        /// checks if capacity < Database.Capacity
        /// </summary>
        private bool IsCapacityOK(int Capacity)
        {
            using (conn = new SqlConnection(ConStr))
            {
                int MaxCapacity=0;
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Capacity FROM Letadla WHERE Letadlo = @Letadlo";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("Letadlo", SqlDbType.NChar).Value = Letadlo;
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while(dr.Read())
                     MaxCapacity = dr.GetInt32(0);
                MaxCapTB.Text = MaxCapacity.ToString();

                if (Capacity > MaxCapacity)
                    return false;
                else
                {
                    CapTB.Text = Capacity.ToString();
                    return true;
                }
            }

        }

        
        #endregion

    }


}

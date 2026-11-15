
using B2BWholesale.Data;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using B2BWholesale.Data;

namespace B2BWholesale
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Run Form1_Load when the form opens
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Database db = new Database();

                // Display database items in the DataGridView
                dgvItems.DataSource = db.GetItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading items: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Database db = new Database();

                using (SqlConnection connection = db.GetConnection())
                {
                    connection.Open();

                    MessageBox.Show("Database connected successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        private void dataGridView1_CellContentClick(
            object sender, DataGridViewCellEventArgs e)
        {
            // We don't need this event yet.
        }
    }
}

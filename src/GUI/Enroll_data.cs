using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Recorder.GUI
{
    public partial class Enroll_data : Form
    {
        private bool isNew;        
        private string connectionString;
        public Enroll_data()
        {
            InitializeComponent();
            ID_label.Visible = false;
            ID_box.Visible = false;
            Save_button.Enabled = false;
           
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Directory.GetParent(baseDirectory).Parent.Parent.FullName;
            string dbPath = Path.Combine(projectRoot, "GUI", "voice_enrollment_data.mdf");
            connectionString = $@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        }

        private void user_button_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(user_box.Text))
            {
                DialogResult result = MessageBox.Show(
                    "Are you new?", // message
                    "Confirmation",            // title
                    MessageBoxButtons.YesNo,   // buttons
                    MessageBoxIcon.Question    // icon
                );

                if (result == DialogResult.Yes)
                {
                    isNew = true;
                    string username = user_box.Text;
                    //int userid = Convert.ToInt32(ID_box.SelectedItem);              
                    // Create the SQL insert command

                    string query = @"
                                INSERT INTO voice_enrollment_final (user_name) 
                                VALUES (@Username); 
                                SELECT SCOPE_IDENTITY();";

                    try
                    {
                        // Open the connection
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            // Create the command                        
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                // Add parameters to avoid SQL injection                            
                                cmd.Parameters.AddWithValue("@Username", username);

                                object resultId = cmd.ExecuteScalar();
                                int newId = Convert.ToInt32(resultId);
                                ID_box.Text = newId.ToString();
                                MessageBox.Show($"Record inserted successfully!\nUsername: {username}\nUser ID: {newId}");
                            }
                        }
                        Save_button.Enabled = isNew;
                        user_button.Enabled = false;
                    }
                    catch (Exception ex)
                    {
                        // Handle any errors
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
                else if (result == DialogResult.No)
                {
                    user_button.Enabled = false;
                    string username = user_box.Text;
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();

                            string query = "SELECT id FROM voice_enrollment_final WHERE user_name = @Username";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@Username", username);

                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    ID_box.Items.Clear();

                                    while (reader.Read())
                                    {
                                        ID_box.Items.Add(reader["id"].ToString());
                                    }
                                }
                            }
                        }

                        if (ID_box.Items.Count == 0)
                        {
                            MessageBox.Show("No IDs found for the entered username.");
                            user_button.Enabled = true;
                        }
                        else
                        {
                            ID_box.SelectedIndex = 0; // Optionally select the first ID
                        }
                        isNew = true;
                        ID_label.Visible = true;
                        ID_box.Visible = true;
                        Save_button.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please Fill the Username Field!");
            }
        }

        private void Save_button_Click(object sender, EventArgs e)
        {
            Enrollment mainForm = new Enrollment(user_box.Text, int.Parse(ID_box.SelectedItem.ToString()));
            mainForm.Show();
            this.Hide();
        }

        private void ID_box_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

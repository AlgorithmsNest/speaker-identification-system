//using Recorder;
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

namespace Recorder
{
    public partial class GUI : Form
    {
        public GUI()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Enroll_data mainForm = new Enroll_data();
            mainForm.Show();
            this.Hide();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Directory.GetParent(baseDirectory).Parent.Parent.FullName;
            string dbPath = Path.Combine(projectRoot, "GUI", "voice_enrollment_data.mdf");
            string selectQuery = "SELECT template_sequence FROM voice_templates WHERE user_name = @userName ORDER BY user_id DESC";
            string connectionString = $@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                /*using (var selectCmd = new SqlCommand(selectQuery, conn))
                {
                    selectCmd.Parameters.AddWithValue("@userName", "001");

                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string insertedTemplate = reader["template_sequence"].ToString();
                            Console.WriteLine("Inserted template: " + insertedTemplate);
                        }
                        else
                        {
                            Console.WriteLine("No inserted template found.");
                        }
                    }
                }*/
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 mainForm = new Form1();
            mainForm.Show();
            this.Hide();
        }
    }
}

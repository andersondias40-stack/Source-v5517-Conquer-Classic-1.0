using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using COServer.Database;
namespace COServer.Panels
{
    public partial class AccountsForm : Form
    {
        public AccountsForm()
        {
            InitializeComponent();
        }

        private void AccountsForm_Load(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            button1.Enabled = false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var cmd = new MySqlCommand(MySqlCommandType.UPDATE);
            cmd.Update("accounts").Set("Password", textBox2.Text).Set("Email", textBox3.Text)
            .Where("EntityID", textBox1.Text);
            if (cmd.Execute2() > 0)
            {
                System.Windows.Forms.MessageBox.Show("Done Save");
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
            {
                cmd.Select("accounts").Where("Username", textBox5.Text);
                using (MySqlReader rdr = new MySqlReader(cmd, true))
                {
                    if (rdr.Read())
                    {
                        textBox1.Text = rdr.ReadUInt32("EntityID").ToString();
                        textBox2.Text = rdr.ReadString("Password");
                        textBox3.Text = rdr.ReadString("Email");
                        textBox4.Text = rdr.ReadString("IP");

                        textBox2.Enabled = true;
                        textBox3.Enabled = true;
                        textBox5.Enabled = false;

                        button1.Enabled = true;
                        button2.Enabled = false;

                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Username not found");
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox5.Enabled = true;

            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();

            button1.Enabled = false;
            button2.Enabled = true;
        }

    }
}

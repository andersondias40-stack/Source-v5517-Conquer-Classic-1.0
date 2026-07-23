using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COServer
{
    public partial class Chi : Form
    {
        public object sync = new object();
        private string MyName = "";
        public Chi()
        {
            InitializeComponent();
            comboBox10.Items.Add("Dragon");
            comboBox10.Items.Add("Phoenix");
            comboBox10.Items.Add("Tiger");
            comboBox10.Items.Add("Turtle");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock (sync)
            {
                MyName = comboBox1.Text;
                
            }
        }
        private void ChiLoad(object sender, EventArgs e)
        {
            foreach (var c in Database.Server.GamePoll.Values)
            {
                comboBox1.Items.Add(c.Player.Name);
            }
        }

        public string[] AtributesType = 
            {
               "CriticalStrike",
               "SkillCriticalStrike",
               "Immunity",
               "Breakthrough",
               "Counteraction",
               "MaxLife",
               "AddAttack",
               "AddMagicAttack",
               "AddMagicDefense",
               "FinalAttack",
               "FinalMagicAttack",
               "FinalDefense",
               "FinalMagicDefense",
            };

        private void Star_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox btn = sender as ComboBox;
            if (Dont)
                return;
            var xx = btn.Name.Length - 1;
            var i2 = (byte.Parse(btn.Name.Substring(xx, 1)));
            if (MyName == "")
            {
                System.Windows.Forms.MessageBox.Show("Select Character First");
                return;
            }
            if (btn.Text == "" || btn.SelectedText == "None")
            {

                if (btn.SelectedText == "None")
                {
                    System.Windows.Forms.MessageBox.Show("Select Att First");
                }
                return;
            }
            var c = Client.GameClient.CharacterFromName(MyName);
            if (c != null)
            {
                var PowerType = (Game.MsgServer.MsgChiInfo.ChiPowerType)Enum.Parse(typeof(Game.MsgServer.MsgChiInfo.ChiPowerType), comboBox10.Text);
                var Power = c.Player.MyChi.Where(p => p.Type == PowerType).FirstOrDefault();
                if (Power.UnLocked)
                {
                    var attribte = (Role.Instance.Chi.ChiAttributeType)Enum.Parse(typeof(Role.Instance.Chi.ChiAttributeType), btn.Text);
                    var Value = Role.Instance.Chi.MaxPower(attribte);
                    Power.Fields[i2 - 1] = Tuple.Create((Role.Instance.Chi.ChiAttributeType)attribte, (int)((byte)attribte * 10000 + Value));
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Unlock the power first");
                }
                Star1.Items.Clear();
                Star2.Items.Clear();
                Star3.Items.Clear();
                Star4.Items.Clear();
                foreach (var att in AtributesType)
                {
                    if (Power.Fields[0].Item1.ToString() != att && Power.Fields[1].Item1.ToString() != att && Power.Fields[2].Item1.ToString() != att && Power.Fields[3].Item1.ToString() != att)
                    {
                        Star1.Items.Add(att);
                        Star2.Items.Add(att);
                        Star3.Items.Add(att);
                        Star4.Items.Add(att);
                    }
                }
                Dont = true;
                textBox1.Text = Power.Fields[0].Item1.ToString();
                textBox2.Text = Power.Fields[1].Item1.ToString();
                textBox3.Text = Power.Fields[2].Item1.ToString();
                textBox4.Text = Power.Fields[3].Item1.ToString();
                Dont = false;
                Role.Instance.Chi.ComputeStatus(c.Player.MyChi);
                c.Equipment.QueryEquipment(c.Equipment.Alternante, false);
                Game.MsgServer.MsgChiInfo.MsgHandleChi.SendInfo(c, Game.MsgServer.MsgChiInfo.Action.Send);
            }
        }
        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock (sync)
            {
                Star1.Enabled = true;
                Star2.Enabled = true;
                Star3.Enabled = true;
                Star4.Enabled = true;

                Star1.Items.Clear();
                Star2.Items.Clear();
                Star3.Items.Clear();
                Star4.Items.Clear();

                Star1.SelectedText = "";
                Star2.SelectedText = "";
                Star3.SelectedText = "";
                Star4.SelectedText = "";
                ComboBox btn = sender as ComboBox;
                if (MyName == "")
                {
                    System.Windows.Forms.MessageBox.Show("Select Character First");
                    return;
                }

                var c = Client.GameClient.CharacterFromName(MyName);
                if (c != null)
                {
                    var PowerType = (Game.MsgServer.MsgChiInfo.ChiPowerType)Enum.Parse(typeof(Game.MsgServer.MsgChiInfo.ChiPowerType), comboBox10.Text);
                    var Power = c.Player.MyChi.Where(p => p.Type == PowerType).FirstOrDefault();
                    if (Power.UnLocked)
                    {
                        foreach (var att in AtributesType)
                        {
                            if (Power.Fields[0].Item1.ToString() != att && Power.Fields[1].Item1.ToString() != att && Power.Fields[2].Item1.ToString() != att && Power.Fields[3].Item1.ToString() != att)
                            {
                                Star1.Items.Add(att);
                                Star2.Items.Add(att);
                                Star3.Items.Add(att);
                                Star4.Items.Add(att);
                            }
                        }
                        textBox1.Text = Power.Fields[0].Item1.ToString();
                        textBox2.Text = Power.Fields[1].Item1.ToString();
                        textBox3.Text = Power.Fields[2].Item1.ToString();
                        textBox4.Text = Power.Fields[3].Item1.ToString();
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Unlock the power first");
                    }
                   
                }

            }
        }

        public bool Dont { get; set; }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

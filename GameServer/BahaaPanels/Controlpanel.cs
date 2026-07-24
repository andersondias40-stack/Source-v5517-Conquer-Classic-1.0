using COServer.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//  using SubSonic;


namespace COServer
{
    public partial class Controlpanel : Form
    {
        public Controlpanel()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            CPs.Text = client.Player.ConquerPoints.ToString();
            Money.Text = client.Player.Money.ToString();
            Level.Text = client.Player.Level.ToString();
            textBox4.Text = client.Player.SecurityPassword.ToString();
            textBox2.Text = client.Player.VipLevel.ToString();
            switch (client.Player.Class)
            {
                #region Get Class
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    {
                        Class.Text = "Trojan";
                        break;
                    }
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                    {
                        Class.Text = "Warrior";
                        break;
                    }
                case 40:
                case 41:
                case 42:
                case 43:
                case 44:
                case 45:
                    {
                        Class.Text = "Archer";
                        break;
                    }
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                    {
                        Class.Text = "Ninja";
                        break;
                    }
                case 60:
                case 61:
                case 62:
                case 63:
                case 64:
                case 65:
                    {
                        Class.Text = "Monk";
                        break;
                    }
                case 130:
                case 131:
                case 132:
                case 133:
                case 134:
                case 135:
                    {
                        Class.Text = "Water";
                        break;
                    }
                case 140:
                case 141:
                case 142:
                case 143:
                case 144:
                case 145:
                    {
                        Class.Text = "Fire";
                        break;
                    }
                default: Class.Text = "Taoist"; break;
                    #endregion
            }
            switch (client.Player.Reborn)
            {
                case 2: Reborn.Text = "2nd Reborn"; break;
                case 1: Reborn.Text = "1st Reborn"; break;
                default: Reborn.Text = "Nono"; break;
            }
            double x = 0;
            if ((client.Player.ExpireVip > DateTime.Now))
            {
                x = (client.Player.ExpireVip - DateTime.Now).TotalDays;
            }
            textBox3.Text = x.ToString();
            textBox2.Text = client.Player.VipLevel.ToString();

        }
        private void button3_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            client.Socket.Disconnect();

        }
        private void button1_Click_2(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            client.Player.ConquerPoints = uint.Parse(CPs.Text);
            //client.Player.DonationPoints = uint.Parse(DNpoints.Text);
            client.Player.Money = uint.Parse(Money.Text);
            client.Player.ExpireVip = DateTime.Now.AddDays(double.Parse(textBox3.Text));
            client.Player.VipLevel = byte.Parse(textBox2.Text);
            client.Player.SecurityPassword = uint.Parse(textBox4.Text);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                //client.Player.SubClass.AddStudyPoints(client, ushort.Parse(textBox8.Text), stream);
                client.Player.SendUpdate(stream, client.Player.VipLevel, Game.MsgServer.MsgUpdate.DataType.VIPLevel);
                client.Player.UpdateVip(stream);
                client.UpdateLevel(stream, byte.Parse(Level.Text));
            }
        }
        private void Control_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            foreach (var user in Database.Server.GamePoll.Values)
            {
                comboBox1.Items.Add(user.Player.Name);
            }
            foreach (var item in Database.Server.ItemsBase.Values)
            {
                comboBox3.Items.Add(item.Name + " " + item.ID);
            }
            foreach (var ban in Database.SystemBannedAccount.BannedPoll.Values)
            {
                comboBox2.Items.Add(ban.Name);
            }
            LoadPCListban();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            Entityidbox.Text = client.Player.UID.ToString();
            CPs.Text = client.Player.ConquerPoints.ToString();
            //DNpoints.Text = client.Player.DonationPoints.ToString();
            Money.Text = client.Player.Money.ToString();
            Level.Text = client.Player.Level.ToString();
            textBox2.Text = client.Player.VipLevel.ToString();
            //textBox8.Text = client.Player.SubClass.StudyPoints.ToString();
            textBox4.Text = client.Player.SecurityPassword.ToString();
            switch (client.Player.Class)
            {
                #region Get Class
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    {
                        Class.Text = "Trojan";
                        break;
                    }
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                    {
                        Class.Text = "Warrior";
                        break;
                    }
                case 40:
                case 41:
                case 42:
                case 43:
                case 44:
                case 45:
                    {
                        Class.Text = "Archer";
                        break;
                    }
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                    {
                        Class.Text = "Ninja";
                        break;
                    }
                case 60:
                case 61:
                case 62:
                case 63:
                case 64:
                case 65:
                    {
                        Class.Text = "Monk";
                        break;
                    }
                case 130:
                case 131:
                case 132:
                case 133:
                case 134:
                case 135:
                    {
                        Class.Text = "Water";
                        break;
                    }
                case 140:
                case 141:
                case 142:
                case 143:
                case 144:
                case 145:
                    {
                        Class.Text = "Fire";
                        break;
                    }
                default: Class.Text = "Taoist"; break;
                    #endregion
            }
            switch (client.Player.Reborn)
            {
                case 2: Reborn.Text = "2nd Reborn"; break;
                case 1: Reborn.Text = "1st Reborn"; break;
                default: Reborn.Text = "Nono"; break;
            }
            double x = 0;
            if ((client.Player.ExpireVip > DateTime.Now))
            {
                x = (client.Player.ExpireVip - DateTime.Now).TotalDays;
            }
            textBox3.Text = x.ToString();
            textBox2.Text = client.Player.VipLevel.ToString();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            Database.ItemType.DBItem DBItem;
            string[] id = comboBox3.Text.Split(' ').ToArray();
            if (Database.Server.ItemsBase.TryGetValue(uint.Parse(id[1]), out DBItem))
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    client.Inventory.Add(uint.Parse(id[1]), byte.Parse(this.Plus.Text), DBItem, stream);
                    //client.Inventory.AddSoul(uint.Parse(id[1]), 0, 0, 0, byte.Parse(this.Plus.Text), byte.Parse(this.Soc1.Text), byte.Parse(this.Soc2.Text), byte.Parse(this.HP.Text), byte.Parse(this.Bless.Text), byte.Parse(this.textBox7.Text), stream, false);//Necklace
                }
            }

        }
        private void button5_Click_1(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            foreach (var user in Database.Server.GamePoll.Values)
            {
                comboBox1.Items.Add(user.Player.Name);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Player.Name.ToLower() == comboBox1.Text.ToLower())
                {
                    Database.SystemBannedAccount.AddBan(user.Player.UID, user.Player.Name, uint.Parse(textBox1.Text));
                    user.Socket.Disconnect();
                    break;
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //JiangHu cp = new JiangHu();
            //cp.ShowDialog();
        }

        //private void button4_Click(object sender, EventArgs e)
        //{
        //    Chi cp = new Chi();
        //    cp.ShowDialog();
        //}

        private void button8_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;


            if (!client.Player.Name.Contains("[PM]"))
            {
                Game.MsgServer.MsgNameChange.ChangeName(client, client.Player.Name + "[PM]", true);
            }
            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.ProjectManager)
            {
                //client.ProjectManager = true;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(9))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 9 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120249,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150249,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot   
                client.Inventory.Add(stream, 117099,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Erring                                   
                client.Inventory.Add(stream, 420339,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //SquallSword                                    
                client.Inventory.Add(stream, 480339,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //NirvanaClub                                    
                client.Inventory.Add(stream, 130109,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //ObsidianArmor                                    
                client.Inventory.Add(stream, 118109,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //PeerlessCoronet 


            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(9))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 9 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120249,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150249,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot
                client.Inventory.Add(stream, 900109,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //CelestialShield                                    
                client.Inventory.Add(stream, 561339,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //OccultWand                                    
                client.Inventory.Add(stream, 560439,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //SpearOfWrath                                    
                client.Inventory.Add(stream, 131109,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //ImperiousArmor                                    
                client.Inventory.Add(stream, 111109,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //SteelHelmet
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(9))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 12 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120249,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150249,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot 
                client.Inventory.Add(stream, 500329,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Bow                                    
                client.Inventory.Add(stream, 133109,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //WelkinCoat                                    
                client.Inventory.Add(stream, 117099,  1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Erring
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(9))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 9 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120249, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150249, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot
                client.Inventory.Add(stream, 610389, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //LazuritePrayerBeads                                    
                client.Inventory.Add(stream, 610389, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //LazuritePrayerBeads                                    
                client.Inventory.Add(stream, 136209, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //WhiteLotusFrock                                    
                client.Inventory.Add(stream, 143259, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //XumiCap                                   

            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(9))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 9 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120249, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150249, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot   
                client.Inventory.Add(stream, 612439, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //LordPistol                                    
                client.Inventory.Add(stream, 611439, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //CaptainRapier                                    
                client.Inventory.Add(stream, 139309, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //DarkDragonCoat                                    
                client.Inventory.Add(stream, 144309, 1, 9, 0, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //DominatorHat                                   
              
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(9))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 9 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 121249, 1, 9, 0, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //NiftyBag                                    
                client.Inventory.Add(stream, 152259, 1, 9, 0, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //WyvernBracelet                                    
                client.Inventory.Add(stream, 160249, 1, 9, 0, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //Boot   
                client.Inventory.Add(stream, 421339, 1, 9, 0, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //kingofbacksword                                    
                client.Inventory.Add(stream, 134109, 1, 9, 0, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //PineRobe                                    
                client.Inventory.Add(stream, 114109, 1, 9, 0, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //UltimateCap                                   
              
            }

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button20_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (client.ProjectManager)
            {
                //client.ProjectManager = false;
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                Database.SystemBannedAccount.RemoveBan(comboBox2.Text);
                comboBox2.Items.Clear();
                foreach (var ban in Database.SystemBannedAccount.BannedPoll.Values)
                {
                    comboBox2.Items.Add(ban.Name);
                }
            }
            catch
            {
                comboBox2.Items.Clear();
                foreach (var ban in Database.SystemBannedAccount.BannedPoll.Values)
                {
                    comboBox2.Items.Add(ban.Name);
                }
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void button21_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            //  client.ConquerPiraTes9 = true;

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button22_Click(object sender, EventArgs e)
        {
            COServer.Panels.AccountsForm cp = new Panels.AccountsForm();
            cp.ShowDialog();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(10))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 10 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 710562, 2);
                client.Inventory.Add(stream, 721080, 2);
                client.Inventory.Add(stream, 1200006, 2);
                client.Inventory.Add(stream, 181120, 1);
                client.Inventory.Add(stream, 198705, 1);
                client.Inventory.Add(stream, 193705, 1);
                client.Inventory.Add(stream, 700013, 2);
                client.Inventory.Add(stream, 1080001, 1);
                client.Inventory.Add(stream, 1088001, 1);
                client.Inventory.Add(stream, 721259, 1);

            }

        }

        private void AccinFo_Click(object sender, EventArgs e)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
            {
                cmd.Select("accounts").Where("ID", Entityidbox.Text);
                using (MySqlReader rdr = new MySqlReader(cmd, true))
                {
                    if (rdr.Read())
                    {
                        //textBox1.Text = rdr.ReadUInt32("EntityID").ToString();
                        //textBox2.Text = rdr.ReadString("Password");
                        //textBox3.Text = rdr.ReadString("Email");
                        //textBox4.Text = rdr.ReadString("IP");
                        textBox5.Text = rdr.ReadUInt32("Username").ToString();
                        textBox6.Text = rdr.ReadUInt32("Password").ToString();
                        textBox5.Enabled = true;
                        textBox6.Enabled = true;
                        //textBox2.Enabled = true;
                        //textBox3.Enabled = true;
                        //textBox5.Enabled = false;

                        //button1.Enabled = true;
                        //button2.Enabled = false;

                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Username not found");
                    }
                }
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(1))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 1 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 780057, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //VIP 30 day

            }

        }
        private void button70_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(1))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 1 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 780057, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //3kk CPS

            }

        }
        private void button71_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(1))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 1 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 780057, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //1kk CPS

            }

        }
        private void button74_Click(object sender, EventArgs e)
        {
            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Player.Name.ToLower() == comboBox1.Text.ToLower())
                {
                    //Database.SystemBannedAccount.AddBan(user.Player.UID, user.Player.Name, uint.Parse(textBox1.Text));
                    user.Teleport(428, 378, 1002);
                    // user.Socket.Disconnect();
                    break;
                }
            }
        }

        private void button75_Click(object sender, EventArgs e)
        {
            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Player.Name.ToLower() == comboBox1.Text.ToLower())
                {
                    //Database.SystemBannedAccount.AddBan(user.Player.UID, user.Player.Name, uint.Parse(textBox1.Text));
                    user.Teleport(032, 074, 6000);
                    // user.Socket.Disconnect();
                    break;
                }
            }
        }

        private void DNpoints_TextChanged(object sender, EventArgs e)
        {

        }

        private void CPs_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void button4_Click_1(object sender, EventArgs e)//scan
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            //client.Send(MsgGuardShield.RequestOpenedProcesses());

        }

        private void button7_Click_1(object sender, EventArgs e)//pcinfo
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            //client.Send(MsgGuardShield.RequestMachineInfo());
        }

        private void button22_Click_1(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            //client.Send(MsgGuardShield.TerminateLoader(Program.ServerConfig.ServerName, "your Client Closed !"));
        }

        //private void button24_Click(object sender, EventArgs e)
        //{
        //    //Chi cp = new Chi();
        //    cp.ShowDialog();
        //}

        private void button25_Click(object sender, EventArgs e)
        {
            //bool AllPlayer = checkBox1.Checked == true;
            //string bar = textBox9.Text;

            //if (AllPlayer)
            //{
            //    var targets = Server.GamePoll.Values;
            //    foreach (var target in targets)
            //    {
            //        if (target != null)
            //        {
            //            target.Send(MsgGuardShield.PingStatuesLoader(bar));
            //        }
            //    }
            //}
            //else
            //{
            //    Client.GameClient client = null;
            //    client = Client.GameClient.CharacterFromName(comboBox1.Text);
            //    if (client == null)
            //        return;
            //    client.Send(MsgGuardShield.PingStatuesLoader(bar));
            //}
        }
        public void LoadPCListban()
        {
            comboBox4.Text = "";
            //foreach (var item in SystemBannedPC.BannedPoll.Values)
            //{
            //    comboBox4.Items.Add(item.PlayerName);
            //}
        }
        private void button26_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            //if (SystemBannedPC.AddBan(client))
            //{
            //    //client.Send(MsgGuardShield.TerminateLoader(Program.ServerConfig.ServerName, "your Are Ban FROM GAME !"));
            //    client.Socket.Disconnect();
            //    LoadPCListban();
            //}
        }

        private void button27_Click(object sender, EventArgs e)
        {
            var target = comboBox4.Text;
            if (string.IsNullOrEmpty(target))
                return;
            //if (SystemBannedPC.RemoveBan(target))
            //    LoadPCListban();
        }
    }
}
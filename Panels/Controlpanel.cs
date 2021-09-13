using LightConquer_Project.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//  using SubSonic;


namespace LightConquer_Project
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
            client.Player.Money = long.Parse(Money.Text);
            client.Player.ExpireVip = DateTime.Now.AddDays(double.Parse(textBox3.Text));
            client.Player.VipLevel = byte.Parse(textBox2.Text);
           client.Player.SecurityPassword = uint.Parse(textBox4.Text);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
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
                comboBox3.Items.Add(item.Name + " "+ item.ID);
            }
            foreach (var ban in Database.SystemBannedAccount.BannedPoll.Values)
            {
                comboBox2.Items.Add(ban.Name);
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            Entityidbox.Text = client.Player.UID.ToString();
            CPs.Text =  client.Player.ConquerPoints.ToString();
            Money.Text = client.Player.Money.ToString();
            Level.Text = client.Player.Level.ToString();
            textBox2.Text = client.Player.VipLevel.ToString();
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
            if(Database.Server.ItemsBase.TryGetValue(uint.Parse(id[1]), out DBItem))
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    client.Inventory.Add(uint.Parse(id[1]), byte.Parse(this.Plus.Text), DBItem, stream);
                     //client.Inventory.Add(stream,uint.Parse(id[1]), 0, 0, 0, byte.Parse(this.Plus.Text), byte.Parse(this.Soc1.Text), byte.Parse(this.Soc2.Text), byte.Parse(this.HP.Text), byte.Parse(this.Bless.Text), byte.Parse(this.textBox7.Text), false);//Necklace
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
            JiangHu cp = new JiangHu();
            cp.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Chi cp = new Chi();
            cp.ShowDialog();
        }

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
            if (!client.HelpDesk)
            {
                client.HelpDesk = true;

                client.Player.Level = 255;
                System.Windows.Forms.MessageBox.Show(client.Player.Name + " is now helpdesk");
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
                client.ProjectManager = true;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(13))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 13 free slots into inventory");
                return;
            }
            //using (var rec = new ServerSockets.RecycledPacket())
            //{
            //    var stream = rec.GetStream();
            //    client.Inventory.Add(stream, 3004247);
            //    client.Inventory.Add(stream, 3004247);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);

            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //}
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot   
                client.Inventory.Add(stream, 410439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //SkyBlade                                    
                client.Inventory.Add(stream, 420439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //SquallSword                                    
                client.Inventory.Add(stream, 480439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //NirvanaClub                                    
                client.Inventory.Add(stream, 130309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //ObsidianArmor                                    
                client.Inventory.Add(stream, 118309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //PeerlessCoronet                                   
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                client.Inventory.Add(stream, 201009, 1, 12, 1, 0, Role.Flags.Gem.SuperThunderGem, Role.Flags.Gem.SuperThunderGem, false); //Fan                                    
                client.Inventory.Add(stream, 202009, 1, 12, 1, 0, Role.Flags.Gem.SuperGloryGem, Role.Flags.Gem.SuperGloryGem, false); //Tower                                                   
                client.Inventory.Add(stream, 300000, 1, 12, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Steed                                    
                client.Inventory.Add(stream, 203009, 1, 12, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Crop
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(36))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 36 free slots into inventory");
                return;
            }
            //using (var rec = new ServerSockets.RecycledPacket())
            //{
            //    var stream = rec.GetStream();
            //    client.Inventory.Add(stream, 3004247);
            //    client.Inventory.Add(stream, 3004247);

            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);

            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //}
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot   
                client.Inventory.Add(stream, 900109, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //CelestialShield                                    
                client.Inventory.Add(stream, 561439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //OccultWand                                    
                client.Inventory.Add(stream, 560439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //SpearOfWrath                                    
                client.Inventory.Add(stream, 131309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //ImperiousArmor                                    
                client.Inventory.Add(stream, 111309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //SteelHelmet                                   
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                client.Inventory.Add(stream, 201009, 1, 12, 1, 0, Role.Flags.Gem.SuperThunderGem, Role.Flags.Gem.SuperThunderGem, false); //Fan                                    
                client.Inventory.Add(stream, 202009, 1, 12, 1, 0, Role.Flags.Gem.SuperGloryGem, Role.Flags.Gem.SuperGloryGem, false); //Tower                                                   
                client.Inventory.Add(stream, 300000, 1, 12, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Steed                                    
                client.Inventory.Add(stream, 203009, 1, 12, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Crop
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(36))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 36 free slots into inventory");
                return;
            }
            //using (var rec = new ServerSockets.RecycledPacket())
            //{
            //    var stream = rec.GetStream();
            //    client.Inventory.Add(stream, 3004247);
            //    client.Inventory.Add(stream, 3004247);

            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);

            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //}
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot   
                client.Inventory.Add(stream, 500429, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //HeavenlyBow                                    
                client.Inventory.Add(stream, 133309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //WelkinCoat                                    
                client.Inventory.Add(stream, 113309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //WhiteTigerHat                                   
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                client.Inventory.Add(stream, 201009, 1, 12, 1, 0, Role.Flags.Gem.SuperThunderGem, Role.Flags.Gem.SuperThunderGem, false); //Fan                                    
                client.Inventory.Add(stream, 202009, 1, 12, 1, 0, Role.Flags.Gem.SuperGloryGem, Role.Flags.Gem.SuperGloryGem, false); //Tower                                                   
                client.Inventory.Add(stream, 300000, 1, 12, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Steed                                    
                client.Inventory.Add(stream, 203009, 1, 12, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Crop
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(36))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 36 free slots into inventory");
                return;
            }
            //using (var rec = new ServerSockets.RecycledPacket())
            //{
            //    var stream = rec.GetStream();
            //    client.Inventory.Add(stream, 3004247);
            //    client.Inventory.Add(stream, 3004247);

            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);

            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //}
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot   
                client.Inventory.Add(stream, 601439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //HanzoKatana                                    
                client.Inventory.Add(stream, 601439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //HanzoKatana                                    
                client.Inventory.Add(stream, 511439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //SilenceScythe                                    
                client.Inventory.Add(stream, 135309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //NightmareVest                                    
                client.Inventory.Add(stream, 112309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //RambleVeil                                   
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                client.Inventory.Add(stream, 201009, 1, 12, 1, 0, Role.Flags.Gem.SuperThunderGem, Role.Flags.Gem.SuperThunderGem, false); //Fan                                    
                client.Inventory.Add(stream, 202009, 1, 12, 1, 0, Role.Flags.Gem.SuperGloryGem, Role.Flags.Gem.SuperGloryGem, false); //Tower                                                   
                client.Inventory.Add(stream, 300000, 1, 12, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Steed                                    
                client.Inventory.Add(stream, 203009, 1, 12, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Crop
            }
        }

        private void button14_Click(object sender, EventArgs e)//monk
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(36))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 36 free slots into inventory");
                return;
            }
            //using (var rec = new ServerSockets.RecycledPacket())
            //{
            //    var stream = rec.GetStream();
            //    client.Inventory.Add(stream, 3004247);
            //    client.Inventory.Add(stream, 3004247);

            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);

            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //}
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot   
                client.Inventory.Add(stream, 610439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //LazuritePrayerBeads                                    
                client.Inventory.Add(stream, 610439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //LazuritePrayerBeads                                    
                client.Inventory.Add(stream, 136309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //WhiteLotusFrock                                    
                client.Inventory.Add(stream, 143309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //XumiCap                                   
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                client.Inventory.Add(stream, 201009, 1, 12, 1, 0, Role.Flags.Gem.SuperThunderGem, Role.Flags.Gem.SuperThunderGem, false); //Fan                                    
                client.Inventory.Add(stream, 202009, 1, 12, 1, 0, Role.Flags.Gem.SuperGloryGem, Role.Flags.Gem.SuperGloryGem, false); //Tower                                                   
                client.Inventory.Add(stream, 300000, 1, 12, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Steed                                    
                client.Inventory.Add(stream, 203009, 1, 12, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Crop
            }
        }

        private void button15_Click(object sender, EventArgs e)//pirate
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(36))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 36 free slots into inventory");
                return;
            }
            //using (var rec = new ServerSockets.RecycledPacket())
            //{
            //    var stream = rec.GetStream();
            //    client.Inventory.Add(stream, 3004247);
            //    client.Inventory.Add(stream, 3004247);

            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);

            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //}
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 120269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Necklace                                    
                client.Inventory.Add(stream, 150269, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Ring                                    
                client.Inventory.Add(stream, 160249, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //Boot   
                client.Inventory.Add(stream, 612439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //LordPistol                                    
                client.Inventory.Add(stream, 611439, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //CaptainRapier                                    
                client.Inventory.Add(stream, 139309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //DarkDragonCoat                                    
                client.Inventory.Add(stream, 144309, 1, 12, 7, 255, Role.Flags.Gem.SuperDragonGem, Role.Flags.Gem.SuperDragonGem, false); //DominatorHat                                   
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                client.Inventory.Add(stream, 201009, 1, 12, 1, 0, Role.Flags.Gem.SuperThunderGem, Role.Flags.Gem.SuperThunderGem, false); //Fan                                    
                client.Inventory.Add(stream, 202009, 1, 12, 1, 0, Role.Flags.Gem.SuperGloryGem, Role.Flags.Gem.SuperGloryGem, false); //Tower                                                   
                client.Inventory.Add(stream, 300000, 1, 12, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Steed                                    
                client.Inventory.Add(stream, 203009, 1, 12, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Crop
            }
        }

        private void button16_Click(object sender, EventArgs e) //lee
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(36))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 36 free slots into inventory");
                return;
            }
            //using (var rec = new ServerSockets.RecycledPacket())
            //{
            //    var stream = rec.GetStream();
            //    client.Inventory.Add(stream, 3004247);
            //    client.Inventory.Add(stream, 3004247);

            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);

            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //}
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 121249, 1, 12, 7, 255, Role.Flags.Gem.SuperTortoiseGem, Role.Flags.Gem.SuperTortoiseGem, false); //NiftyBag                                    
                client.Inventory.Add(stream, 152259, 1, 12, 7, 255, Role.Flags.Gem.SuperTortoiseGem, Role.Flags.Gem.SuperTortoiseGem, false); //WyvernBracelet                                     
                client.Inventory.Add(stream, 160249, 1, 12, 7, 255, Role.Flags.Gem.SuperTortoiseGem, Role.Flags.Gem.SuperTortoiseGem, false); //Boot   
                client.Inventory.Add(stream, 421439, 1, 12, 7, 255, Role.Flags.Gem.SuperTortoiseGem, Role.Flags.Gem.SuperTortoiseGem, false); //SupremeSword                                    
                client.Inventory.Add(stream, 134309, 1, 12, 7, 255, Role.Flags.Gem.SuperTortoiseGem, Role.Flags.Gem.SuperTortoiseGem, false); //EternalRobe                                    
                client.Inventory.Add(stream, 114309, 1, 12, 7, 255, Role.Flags.Gem.SuperTortoiseGem, Role.Flags.Gem.SuperTortoiseGem, false); //DistinctCap                                   
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                client.Inventory.Add(stream, 201009, 1, 12, 1, 0, Role.Flags.Gem.SuperThunderGem, Role.Flags.Gem.SuperThunderGem, false); //Fan                                    
                client.Inventory.Add(stream, 202009, 1, 12, 1, 0, Role.Flags.Gem.SuperGloryGem, Role.Flags.Gem.SuperGloryGem, false); //Tower                                                   
                client.Inventory.Add(stream, 300000, 1, 12, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Steed                                    
                client.Inventory.Add(stream, 203009, 1, 12, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Crop
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(36))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 36 free slots into inventory");
                return;
            }
            //using (var rec = new ServerSockets.RecycledPacket())
            //{
            //    var stream = rec.GetStream();
            //    client.Inventory.Add(stream, 3004247);
            //    client.Inventory.Add(stream, 3004247);

            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);
            //    client.Inventory.Add(stream, 3004248);

            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //    client.Inventory.Add(stream, 3004249);
            //}
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Inventory.Add(stream, 121249, 1, 12, 7, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //NiftyBag                                    
                client.Inventory.Add(stream, 152259, 1, 12, 7, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //WyvernBracelet                                    
                client.Inventory.Add(stream, 160249, 1, 12, 7, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //Boot   
                client.Inventory.Add(stream, 421439, 1, 12, 7, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //SupremeSword                                    
                client.Inventory.Add(stream, 134309, 1, 12, 7, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //EternalRobe                                    
                client.Inventory.Add(stream, 114309, 1, 12, 7, 255, Role.Flags.Gem.SuperPhoenixGem, Role.Flags.Gem.SuperPhoenixGem, false); //DistinctCap                                   
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                client.Inventory.Add(stream, 201009, 1, 12, 1, 0, Role.Flags.Gem.SuperThunderGem, Role.Flags.Gem.SuperThunderGem, false); //Fan                                    
                client.Inventory.Add(stream, 202009, 1, 12, 1, 0, Role.Flags.Gem.SuperGloryGem, Role.Flags.Gem.SuperGloryGem, false); //Tower                                                   
                client.Inventory.Add(stream, 300000, 1, 12, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Steed                                    
                client.Inventory.Add(stream, 203009, 1, 12, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Crop
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
                client.ProjectManager = false;
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
            LightConquer_Project.Panels.AccountsForm cp = new Panels.AccountsForm();
            cp.ShowDialog();
        }

        private void button18_Click(object sender, EventArgs e) //wind
        {
            Client.GameClient client = null;
            client = Client.GameClient.CharacterFromName(comboBox1.Text);
            if (client == null)
                return;
            if (!client.Inventory.HaveSpace(36))
            {
                System.Windows.Forms.MessageBox.Show("Character must have 36 free slots into inventory");
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                client.Inventory.Add(stream, 2100245, 1, 0, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //HolyPhoenixCup
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

        private void button8_Click_1(object sender, EventArgs e)
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

        private void button9_Click_1(object sender, EventArgs e)
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

                client.Inventory.Add(stream, 3303756, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //VIP 30 day
            }
        }
    }
}
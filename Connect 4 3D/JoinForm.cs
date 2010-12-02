using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace Connect_4_3D
{
    internal partial class JoinForm : Form
    {
        static JoinForm _Instance;

        internal JoinForm()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(JoinForm_FormClosing);
        }

        internal static void ShowJoinScreen()
        {
            if (_Instance == null)
                _Instance = new JoinForm();

            _Instance.Box_IP.Text = Options.Option_JoinIPAdress;
            _Instance.Box_Port.Text = Options.Option_JoinPort.ToString();

            _Instance.ShowDialog(MainForm._Instance);
        }

        void JoinForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Button_JoinGame_Click(object sender, EventArgs e)
        {
            IPAddress JoinIp;
            try
            {
                JoinIp = IPAddress.Parse(Box_IP.Text);
            }
            catch
            {
                MessageBox.Show(String.Format("{0} is not a valid ip address", Box_IP.Text));
                return;
            }
            int JoinPort;
            try
            {
                JoinPort = Convert.ToInt32(Box_Port.Text);
            }
            catch
            {
                MessageBox.Show(String.Format("{0} is not a valid number", Box_Port.Text));
                return;
            }
            if (JoinPort < 100 || JoinPort > 65535)
            {
                MessageBox.Show("Please choose a port number greater than 100 and less than 65535.", Box_Port.Text);
                return;
            }
            Options.SaveJoinIpAndPortToRegistry(Box_IP.Text, JoinPort);

            Game.NewGame(Game.GAMETYPE_INTERNETJOIN);

            Networking.Join(JoinIp, JoinPort);

            this.Hide();

        }

        private void Button_CancelJoin_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void Box_Port_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar))
                return;
            e.Handled = true;
        }        
    }
}

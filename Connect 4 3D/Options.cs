using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Connect_4_3D
{
    internal partial class Options : Form
    {
        static Options _Instance;

        const string REGISTRYKEY = @"HKEY_CURRENT_USER\Software\Netriak\Connect4 3D\";

        internal static int Option_AntiAliasing = 4;
        internal static int Option_Anisotropic = 4;
        internal static int Option_AIDifficulty = 4;
        internal static int Option_Hostport = 6002;
        internal static bool Option_VSynch = true;
        internal static bool Option_Skybox = true;
        internal static string Option_JoinIPAdress = "";
        internal static int Option_JoinPort = 6002;
        internal static bool Option_Shaders = true;

        internal static void LoadOptionsFromRegistry()
        {
            if (Microsoft.Win32.Registry.GetValue(REGISTRYKEY, "Test", true) != null)
            {
                Option_AntiAliasing = (int)Microsoft.Win32.Registry.GetValue(REGISTRYKEY, "Antialiasing", 4);
                Option_Anisotropic = (int)Microsoft.Win32.Registry.GetValue(REGISTRYKEY, "Anisotropy", 4);
                Option_AIDifficulty = (int)Microsoft.Win32.Registry.GetValue(REGISTRYKEY, "AIDifficulty", 4);
                Option_Hostport = (int)Microsoft.Win32.Registry.GetValue(REGISTRYKEY, "HostPort", 6002);
                Option_JoinIPAdress = (string)Microsoft.Win32.Registry.GetValue(REGISTRYKEY, "JoinIPAdress", "");
                Option_JoinPort = (int)Microsoft.Win32.Registry.GetValue(REGISTRYKEY, "JoinPort", 6002);
                Option_VSynch = Convert.ToBoolean(Microsoft.Win32.Registry.GetValue(REGISTRYKEY, "VSynch", true));
                Option_Skybox = Convert.ToBoolean(Microsoft.Win32.Registry.GetValue(REGISTRYKEY, "RenderSkybox", true));
                Option_Shaders = Convert.ToBoolean(Microsoft.Win32.Registry.GetValue(REGISTRYKEY, "UseShaders", true));

                if (Option_AntiAliasing != 2 && Option_AntiAliasing != 4) Option_AntiAliasing = 0;
                if (Option_Anisotropic != 2 && Option_Anisotropic != 4) Option_Anisotropic = 0;
                if (Option_AIDifficulty > 4 || Option_AIDifficulty < 1) Option_AIDifficulty = 4;
            }
        }

        static void SaveOptionsToRegistry()
        {
            Microsoft.Win32.Registry.SetValue(REGISTRYKEY, "Antialiasing", Option_AntiAliasing);
            Microsoft.Win32.Registry.SetValue(REGISTRYKEY, "Anisotropy", Option_Anisotropic);
            Microsoft.Win32.Registry.SetValue(REGISTRYKEY, "AIDifficulty", Option_AIDifficulty);
            Microsoft.Win32.Registry.SetValue(REGISTRYKEY, "HostPort", Option_Hostport);
            Microsoft.Win32.Registry.SetValue(REGISTRYKEY, "VSynch", Option_VSynch);
            Microsoft.Win32.Registry.SetValue(REGISTRYKEY, "RenderSkybox", Option_Skybox);
            Microsoft.Win32.Registry.SetValue(REGISTRYKEY, "UseShaders", Option_Shaders);
        }

        internal static void SaveJoinIpAndPortToRegistry(string sIp, int nPort)
        {
            Microsoft.Win32.Registry.SetValue(REGISTRYKEY, "JoinPort", nPort);
            Microsoft.Win32.Registry.SetValue(REGISTRYKEY, "JoinIPAdress", sIp);
        }

        internal static void ShowOptionsScreen()
        {
            if (_Instance == null)
                _Instance = new Options();
            _Instance.Show();
            _Instance.Focus();
            _Instance.Check_VSynch.Checked = Option_VSynch;
            _Instance.Check_Skybox.Checked = Option_Skybox;
            _Instance.Box_Antialiasing.SelectedIndex = Option_AntiAliasing / 2;
            _Instance.Box_AI_Difficulty.SelectedIndex = Option_AIDifficulty - 1;
            _Instance.Box_Anisotropic.SelectedIndex = Option_Anisotropic / 2;
            _Instance.Text_Hostport.Text = Option_Hostport.ToString();
            _Instance.Check_shaders.Checked = Option_Shaders;

            if (!Engine.Device_CanUseShaders)
                _Instance.Check_shaders.Enabled = false;
            else
                _Instance.Check_shaders.Enabled = true;

        }

        internal Options()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(Options_FormClosing);
            this.Box_AI_Difficulty.Items.AddRange(new object[]{ "Beginner", "Intermediate", "Skilled", "Expert"});
            this.Box_Anisotropic.Items.AddRange(new object[] { "None", "2x", "4x"});
            this.Box_Antialiasing.Items.AddRange(new object[] { "None", "2x", "4x" });
        }

        void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void Button_Save_Click(object sender, EventArgs e)
        {
            int nPort;
            try
            {
                nPort = Convert.ToInt32(Text_Hostport.Text);
            }
            catch
            {
                MessageBox.Show(String.Format("{0} is not a valid number", Text_Hostport.Text));
                return;
            }
            if (nPort < 100 || nPort > 65535)
            {
                MessageBox.Show("Please choose a port number greater than 100 and less than 65535.", Text_Hostport.Text);
                return;
            }
            Option_Hostport = nPort;
            Option_AntiAliasing = Box_Antialiasing.SelectedIndex * 2;
            Option_Anisotropic = Box_Anisotropic.SelectedIndex * 2;
            Option_AIDifficulty = Box_AI_Difficulty.SelectedIndex + 1;
            Option_VSynch = Check_VSynch.Checked;
            Option_Skybox = Check_Skybox.Checked;
            Option_Shaders = Check_shaders.Checked;
            SaveOptionsToRegistry();
            this.Hide();
            Engine.ResetDevice();
        }

        private void Text_Hostport_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar))
                return;
            e.Handled = true;
        }
    }
}

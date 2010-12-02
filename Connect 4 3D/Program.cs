using System;
using System.Collections.Generic;
using System.Linq;
using SlimDX;
using System.Drawing;

namespace Connect_4_3D
{
    static class Program
    {
        static void Main(string[] Args)
        {
            Options.LoadOptionsFromRegistry();

            if (Args.Length > 0 && Args[0].ToLowerInvariant() == "-noshaders")
            {
                Engine.Device_CanUseShaders = false;
                Options.Option_Shaders = false;
            }
            
            MainForm.InitForm();
#if !DEBUG
            try
            {
#endif
                Engine.InitRenderDevice();
#if !DEBUG            
            }
            catch (Exception RenderException)
            {
                System.Windows.Forms.MessageBox.Show(RenderException.Message, "Direct3d Initialization failed.");
                return;
            }
#endif

            Game.NewGame(Game.GAMETYPE_NOTSTARTED);

            MainForm.Start();

            foreach (var item in ObjectTable.Objects)
                item.Dispose();
        }
    }
}

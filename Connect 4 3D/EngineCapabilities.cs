using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;


namespace Connect_4_3D
{
    static partial class Engine
    {
        internal static bool Device_CanUseShaders = true;


        static void CheckEngineCapabilites()
        {
            if (device.Capabilities.PixelShaderVersion.Major < 3
                || device.Capabilities.VertexShaderVersion.Major < 3)
            {
                Device_CanUseShaders = false;
                Options.Option_Shaders = false;
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Connect_4_3D
{
    static class Resource_Manager
    {
        // Gets the stream from a local file, or from within the assembly if it is not found.
        internal static StreamReader GetResourceStreamReader(string sFile)
        {
            try {
                return GetFileReader(sFile);
            } catch { }
            try {
                return new StreamReader(GetManifestStream(sFile));
            } catch { }
            
            throw new Exception(String.Format("The following resource was not found: {0}", sFile));
        }

        internal static Stream GetResourceStream(string sFile)
        {
            try
            {
                return GetFileReader(sFile).BaseStream;
            }
            catch { }
            try
            {
                return GetManifestStream(sFile);
            }
            catch { }

            throw new Exception(String.Format("The following resource was not found: {0}", sFile));
        }

        static Stream GetManifestStream(string sFile)
        {
            Stream ToReturn = Assembly.GetExecutingAssembly().GetManifestResourceStream("Connect_4_3D.Resources." + sFile);
            if (ToReturn != null) return ToReturn;

            return Assembly.GetExecutingAssembly().GetManifestResourceStream("Connect_4_3D." + sFile); 
        }

        static StreamReader GetFileReader(string sFile)
        {
            return new StreamReader("Resources\\" + sFile);
        }

    }
}

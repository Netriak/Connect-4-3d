using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Connect_4_3D
{
    static class Networking
    {

        [DllImport("user32.dll")]
        private static extern bool LockWindowUpdate(IntPtr hWndLock);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private static System.Timers.Timer ConnectionPendingTimer = new System.Timers.Timer();

        private static Socket Connection_attempt, Connection;
        internal static Boolean Connected = false;
        internal static Boolean Connecting = false;
        internal static byte[] DataBuffer = new byte[300]; // All must be in one burst.
        private static String DataReceivedBuffer = "";

        internal static void Disconnect()
        {
            if (Game._GameResult == Game.GAMERESULT_ONGOING || Game._GameResult == Game.GAMERESULT_WAITFORCONNECTION)
            {
                Game.SetGameResult(Game.GAMERESULT_DISCONNECTED);
                MainForm.UpdateHistoryButtons();
            }
            try
            {
                if (Connection_attempt != null)
                    Connection_attempt.Close();
                if (Connection != null)
                    Connection.Close();
                ConnectionPendingTimer.Stop();
            }
            catch { }
            Connecting = false;
            Connected = false;
        }

        internal static void Host(int nPort)
        {
            IPAddress ipHost = IPAddress.Any;
            try
            {
                Connection_attempt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                IPEndPoint ipLocal = new IPEndPoint(ipHost, nPort);
                Connection_attempt.Bind(ipLocal);
                Connection_attempt.Listen(1);
                Connection_attempt.BeginAccept(new AsyncCallback(OnClientConnect), null);
                Connecting = true;
            }
            catch
            {
                Game.SetGameResult(Game.GAMERESULT_CONNECTIONFAILED);
            }
        }

        internal static void Join(IPAddress ip, int nPort)
        {
            Connection_attempt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            IPEndPoint ipRemote = new IPEndPoint(ip, nPort);
            Connection_attempt.BeginConnect(ipRemote, new AsyncCallback(OnRemoteConnect), null);
            Connecting = true;
        }


        private static void OnRemoteConnect(IAsyncResult asyn)
        {
            try
            {
                Connection = Connection_attempt;
                Connection_attempt.EndConnect(asyn);
                Connected = true;
                SendVersion();
            }
            catch (ObjectDisposedException)
            {
                if (Game._GameResult == Game.GAMERESULT_WAITFORCONNECTION)
                    Game.SetGameResult(Game.GAMERESULT_CONNECTIONFAILED);
                Disconnect();
                return;
            }
            catch (SocketException se)
            {
                System.Windows.Forms.MessageBox.Show(se.Message);
                Game.SetGameResult(Game.GAMERESULT_CONNECTIONFAILED);
                Disconnect();
                return;
            }
            WaitForData();
        }

        static string GetProgramID()
        {
            System.Security.Cryptography.MD5CryptoServiceProvider Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            System.IO.Stream Executable = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().Location).BaseStream;

            byte[] hash = Hasher.ComputeHash(Executable);

            return BitConverter.ToString(hash);;
        }

        private static void SendVersion()
        {
            ConnectionPendingTimer.Elapsed += delegate
            {
                if (Connected && Connecting)
                {
                    Game.SetGameResult(Game.GAMERESULT_CONNECTIONFAILED);
                    Disconnect();
                }
            };
            ConnectionPendingTimer.Interval = 1000.0;
            ConnectionPendingTimer.Start();
            SendData("V" + GetProgramID());
        }

        private static void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                Connection = Connection_attempt.EndAccept(asyn);
                Connection_attempt.Close();
                string ip = ((IPEndPoint)Connection.RemoteEndPoint).Address.ToString();
                Connected = true;
                SendVersion();
            }
            catch (ObjectDisposedException)
            {
                if (Game._GameResult == Game.GAMERESULT_WAITFORCONNECTION)
                    Game.SetGameResult(Game.GAMERESULT_CONNECTIONFAILED);
                Disconnect();
                return;
            }
            catch (SocketException se)
            {
                System.Windows.Forms.MessageBox.Show(se.Message);
                Game.SetGameResult(Game.GAMERESULT_CONNECTIONFAILED);
                Disconnect();
                return;
            }
            WaitForData();
        }
        
        private static void SendData(string sData)
        {
            if (!Connected) return;
            if (sData.Contains("|") || sData.Contains("~"))
            {
                throw new Exception("Trying to send data with invalid characters!");
            }

            byte[] ByteArray = System.Text.Encoding.UTF8.GetBytes("|" + sData + "~"); // Notify as new data
            try
            {
                Connection.Send(ByteArray);
            }
            catch
            {
                Connected = false;
                Disconnect();
                return;
            }
        }

        internal static void SendTurn(int X, int Z)
        {
            SendData(String.Format("T{0},{1}", X, Z));
        }

        private static void WaitForData()
        {
            try
            {
                Connection.BeginReceive(DataBuffer, 0, DataBuffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), null);
            }
            catch
            {
                Disconnect();
            }
        }

        internal static void SendPlayerInfo()
        {
            SendData("P" + (Game._LocalPlayerSide ? "0" : "1"));
        }

        private static void OnDataReceived(IAsyncResult asyn)
        {
            //end receive...
            int iRx = 0;
            try
            {
                iRx = Connection.EndReceive(asyn);
            }
            catch
            {
                Connected = false;
                Disconnect();
                return;
            }
            char[] chars = new char[iRx + 1];
            System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
            int charLen = d.GetChars(DataBuffer, 0, iRx, chars, 0);

            String szRaw = DataReceivedBuffer + new String(chars).TrimEnd('\0'); // Add the buffered invalid data of the previous time.
            DataReceivedBuffer = "";
            // The data is now ready for interpetation.
            String[] szRawList = szRaw.Split('|');

            // The data is imported.
            String szData = "";
            for (int x = 0; x < szRawList.Length; x++)
            {
                if (szRawList[x].Length < 1) continue;
                if (Connecting && szRawList[x].Substring(0, 1) != "V") continue; // Version protection
                if (!szRawList[x].EndsWith("~"))
                {
                    if (x == szRawList.Length - 1)
                    {
                        // Incomplete transmission, put in buffer and wait for the rest.
                        DataReceivedBuffer = szRawList[x];
                        break;
                    }
                    else
                    {
                        // Garbage transmission, will never complete.
                        throw new Exception("Garbage transmission received");
                    }
                }

                szData = szRawList[x].Substring(1, szRawList[x].Length - 2);

                switch (szRawList[x].Substring(0, 1))
                {
                    case "V": // Version check.
                        if (szData == GetProgramID())
                        {
                            Connected = true;
                            Connecting = false;
                            if (Game._GameType == Game.GAMETYPE_INTERNETHOST)
                            {
                                Game.HostStart();
                            } // If not host, we wait for host to tell us which color we are.
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Remote connection failed. Incompatible versions");
                            Disconnect();
                            return;
                        }
                        break;
                    case "P": // Player info
                        if (Connecting || Game._GameType != Game.GAMETYPE_INTERNETJOIN)
                        {
                            System.Windows.Forms.MessageBox.Show("Illegal command: 'Player info', terminating connection.");
                            Disconnect();
                            return;
                        }
                        Game.JoinStart(szData);
                        break;
                    case "T":
                        if (Connecting || 
                            (Game._GameType != Game.GAMETYPE_INTERNETJOIN && Game._GameType != Game.GAMETYPE_INTERNETHOST)
                            || Game._LocalPlayerSide == Game._CurrentTurn || Game._GameResult != Game.GAMERESULT_ONGOING)
                        {
                            System.Windows.Forms.MessageBox.Show("Illegal command: 'New turn', terminating connection.");
                            Disconnect();
                            return;
                        }
                        string[] XZ = szData.Split(',');
                        Game.PerformMove(Convert.ToInt32(XZ[0]), Convert.ToInt32(XZ[1]), true);
                        break;
                    default:
                        throw new Exception("Unknown data received");
                }
            }
            WaitForData();
        }
    }
}
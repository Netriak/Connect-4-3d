using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Windows;
using System.Windows.Forms;
using System.Drawing;

namespace Connect_4_3D
{
    static class MainForm
    {
        static RenderForm GameForm;

        internal static RenderForm _Instance { get { return GameForm; } }

        internal static int MouseDragStartX;
        internal static int MouseDragStartY;
        internal static bool MouseDragging = false;

        internal static bool Resizing = false;

        internal static MainMenu _MainMenu;

        internal static void InitForm()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

            GameForm = new RenderForm();
            GameForm.SizeChanged += new EventHandler(Form_SizeChanged);
            GameForm.MouseDown += new MouseEventHandler(Form_MouseDown);

            _MainMenu = new MainMenu();

            _MainMenu.MenuItems.Add("New Game");
            _MainMenu.MenuItems[0].MenuItems.Add("Hotseat");
            _MainMenu.MenuItems[0].MenuItems[0].Click += new EventHandler(MainForm_NewGame_Hotseat);

            _MainMenu.MenuItems[0].MenuItems.Add("Singleplayer");
            _MainMenu.MenuItems[0].MenuItems[1].Click += new EventHandler(MainForm_NewGame_Singleplayer);

            _MainMenu.MenuItems[0].MenuItems.Add("-");

            _MainMenu.MenuItems[0].MenuItems.Add("Multiplayer Host");
            _MainMenu.MenuItems[0].MenuItems[3].Click += new EventHandler(MainForm_NewGame_Multiplayer_Host);

            _MainMenu.MenuItems[0].MenuItems.Add("Multiplayer Join");
            _MainMenu.MenuItems[0].MenuItems[4].Click += new EventHandler(MainForm_NewGame_Multiplayer_Join);

            _MainMenu.MenuItems.Add("Options");
            _MainMenu.MenuItems[1].Click +=new EventHandler(MainForm_Options);

            _MainMenu.MenuItems.Add("Disconnect");
            _MainMenu.MenuItems[2].Click += new EventHandler(MainForm_Disconnect);
            _MainMenu.MenuItems[2].Enabled = false;

            _MainMenu.MenuItems.Add("History");
            _MainMenu.MenuItems[3].MenuItems.Add("Undo move");
            _MainMenu.MenuItems[3].MenuItems[0].ShowShortcut = true;
            _MainMenu.MenuItems[3].MenuItems[0].Shortcut = Shortcut.CtrlZ;
            _MainMenu.MenuItems[3].MenuItems[0].Click += new EventHandler(MainForm_History_Undo);
            _MainMenu.MenuItems[3].MenuItems.Add("Redo move");
            _MainMenu.MenuItems[3].MenuItems[1].ShowShortcut = true;
            _MainMenu.MenuItems[3].MenuItems[1].Shortcut = Shortcut.CtrlQ;
            _MainMenu.MenuItems[3].MenuItems[1].Click += new EventHandler(MainForm_History_Redo);
            _MainMenu.MenuItems[3].MenuItems.Add("Resume Game");
            _MainMenu.MenuItems[3].MenuItems[2].Click += new EventHandler(MainForm_History_Resume);

#if DEBUG
            _MainMenu.MenuItems[0].MenuItems.Add("AI Test");
            _MainMenu.MenuItems[0].MenuItems[5].Click += new EventHandler(MainForm_AITest);
#endif

            GameForm.Menu = _MainMenu;

            GameForm.MinimumSize = new Size(640, 480);

            GameForm.Icon = new Icon(Resource_Manager.GetResourceStream("logo.ico"), 256, 256);

            GameForm.ResizeEnd += new EventHandler(GameForm_ResizeEnd);
            GameForm.ResizeBegin += new EventHandler(GameForm_ResizeBegin);
#if !DEBUG
            GameForm.Text = "Connect-4 3D";
#else
            GameForm.Text = "Connect-4 3D - Debug Mode";
#endif
            GameForm.FormClosing += new FormClosingEventHandler(GameForm_FormClosing);
        }

        internal static void UpdateHistoryButtons()
        {
            _MainMenu.MenuItems[3].MenuItems[0].Enabled = Game.CanUndo();
            _MainMenu.MenuItems[3].MenuItems[1].Enabled = Game.CanRedo();
            _MainMenu.MenuItems[3].MenuItems[2].Enabled = Game.CanRedo() && Game._GameResult != Game.GAMERESULT_ONGOING && (Game._GameType == Game.GAMETYPE_SINGLEPLAYER || Game._GameType == Game.GAMETYPE_HOTSEAT);
        }

        static void MainForm_History_Resume(object sender, EventArgs e)
        {
            Game.ResumeGame();
        }

        static void MainForm_History_Undo(object sender, EventArgs e)
        {
            if (!Game.CanUndo()) return;
            AI.StopAI();
            Game.UndoMove();

            if (Game._GameResult != Game.GAMERESULT_ONGOING || Game._GameType != Game.GAMETYPE_SINGLEPLAYER) return;
            if (Game._CurrentTurn != Game._LocalPlayerSide)
                Game.UndoMove();
        }

        static void MainForm_History_Redo(object sender, EventArgs e)
        {
            if (!Game.CanRedo()) return;
            Game.RedoMove();
            if (Game._GameResult != Game.GAMERESULT_ONGOING || Game._GameType != Game.GAMETYPE_SINGLEPLAYER) return;
            if (Game._CurrentTurn != Game._LocalPlayerSide)
            {
                if (Game.CanRedo())
                {
                    Game.RedoMove();
                }
                else
                {
                    AI.StartAI();
                }
            }
        }

        static void MainForm_AITest(object sender, EventArgs e)
        {
            Game.NewGame(Game.GAMETYPE_AIONLY);
        }

        static void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Game._GameResult == Game.GAMERESULT_ONGOING &&
                (Game._GameType == Game.GAMETYPE_INTERNETHOST ||
                Game._GameType == Game.GAMETYPE_INTERNETJOIN))
            {
                if (MessageBox.Show("Are you sure you wish to quit?", "Warning", MessageBoxButtons.YesNo) == DialogResult.No)
                    e.Cancel = true;
            }
        }

        static void MainForm_Disconnect(object sender, EventArgs e)
        {
            if (Game._GameResult == Game.GAMERESULT_ONGOING)
            {
                if (MessageBox.Show("Are you sure you wish to disconnect?", "Warning", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }
            Networking.Disconnect();
        }

        static void MainForm_Options(object sender, EventArgs e)
        {
            Options.ShowOptionsScreen();
        }

        static void MainForm_NewGame_Multiplayer_Host(object sender, EventArgs e)
        {
            Game.NewGame(Game.GAMETYPE_INTERNETHOST);
            Networking.Host(Options.Option_Hostport);
        }

        static void MainForm_NewGame_Multiplayer_Join(object sender, EventArgs e)
        {
            JoinForm.ShowJoinScreen();
        }

        static void GameForm_ResizeBegin(object sender, EventArgs e)
        {
            Resizing = true;
        }

        static void GameForm_ResizeEnd(object sender, EventArgs e)
        {
            Resizing = false;
            if (_RenderWindowSize.Width < _RenderWindowSize.Height)
            {
                GameForm.Width = _RenderWindowSize.Height + (GameForm.Width - _RenderWindowSize.Width); // Match width with height.
                return;
            }
            Engine.ResetDevice();
        }

        static void MainForm_NewGame_Singleplayer(object sender, EventArgs e)
        {
            Game.NewGame(Game.GAMETYPE_SINGLEPLAYER);
        }

        static void MainForm_NewGame_Hotseat(object sender, EventArgs e)
        {
            Game.NewGame(Game.GAMETYPE_HOTSEAT);
        }

        internal static void Start()
        {
            MessagePump.Run(GameForm, Engine.RenderThread);
        }

        static void Form_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                MouseDragStartX = Control.MousePosition.X;
                MouseDragStartY = Control.MousePosition.Y;
                MouseDragging = true;
            }
            else if (e.Button == MouseButtons.Left)
            {
                Engine.CheckMouse(e.X, e.Y);
            }
        }

        static void Form_SizeChanged(object sender, EventArgs e)
        {
            if (_RenderWindowSize.Height == 0 ||
                _RenderWindowSize.Width == 0)
            {
                return;
            }

            if (Resizing)
                return;

            if (_RenderWindowSize.Width < _RenderWindowSize.Height)
            {
                GameForm.Width = _RenderWindowSize.Height + (GameForm.Width - _RenderWindowSize.Width); // Match width with height.
                return;
            }

            Engine.ResetDevice();
        }

        internal static IntPtr _Handle { get { return GameForm.Handle; } }

        internal static Rectangle _RenderWindowSize { get { return GameForm.ClientRectangle; } }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect_4_3D
{
    class Game
    {
        internal const int GAMETYPE_NOTSTARTED = 0;
        internal const int GAMETYPE_HOTSEAT = 1;
        internal const int GAMETYPE_SINGLEPLAYER = 2;
        internal const int GAMETYPE_INTERNETHOST = 3;
        internal const int GAMETYPE_INTERNETJOIN = 4;
        internal const int GAMETYPE_AIONLY = 5;

        internal const byte POSITION_EMPTY = 0;
        internal const byte POSITION_LIGHT = 1;
        internal const byte POSITION_DARK = 2;

        internal const byte GAMERESULT_ONGOING = 0;
        internal const byte GAMERESULT_LIGHTWINS = 1;
        internal const byte GAMERESULT_DARKWINS = 2;
        internal const byte GAMERESULT_WAITFORCONNECTION = 3;
        internal const byte GAMERESULT_DISCONNECTED = 4;
        internal const byte GAMERESULT_CONNECTIONFAILED = 5;
        internal const byte GAMERESULT_DRAW = 6;

        internal const bool TURN_LIGHT = false;
        internal const bool TURN_DARK = true;

        VictoryRow CurrentVictoryRow;

        internal class VictoryRow
        {
            internal int X, Y, Z;

            internal VictoryRow(int X, int Y, int Z)
            {
                this.X = X;
                this.Y = Y;
                this.Z = Z;
            }
        }

        bool _CheckIsInVictoryRow(int X, int Y, int Z)
        {
            if (CurrentVictoryRow == null) return false;
            if (GameResult != GAMERESULT_DARKWINS && GameResult != GAMERESULT_LIGHTWINS) return false;
            if (HistoryPoint != MoveHistory.Count) return false;

            int nX = CurrentVictoryRow.X; int nY = CurrentVictoryRow.Y; int nZ = CurrentVictoryRow.Z; 

            for (int L = 0; L < 4; L++)
            {
                if (CurrentVictoryRow.X < 1) nX++; if (CurrentVictoryRow.Y < 1) nY++; if (CurrentVictoryRow.Z < 1) nZ++;
                if (CurrentVictoryRow.X > 4) nX--; if (CurrentVictoryRow.Y > 4) nY--; if (CurrentVictoryRow.Z > 4) nZ--;
                if (nX == X && nY == Y && nZ == Z) return true;
            }
            return false;
        }

        internal static bool CheckIsInVictoryRow(int X, int Y, int Z)
        {
            return CurrentGame._CheckIsInVictoryRow(X, Y, Z);
        }

        internal class Move
        {
            internal int X, Z;

            internal Move(int X, int Z)
            {
                this.X = X;
                this.Z = Z;
            }
        }

        List<Move> MoveHistory = new List<Move>();
        int HistoryPoint = 0;

        internal static Move LastMove
        {
            get
            {
                if (CurrentGame == null) return null;
                return CurrentGame._LastMove;
            }
        }

        internal Move _LastMove
        {
            get
            {
                if (HistoryPoint < 1) return null;
                return MoveHistory[HistoryPoint - 1];
            }
        }

        internal static void ResumeGame()
        {
            if (_GameResult == GAMERESULT_ONGOING) return;
            if (_GameType != GAMETYPE_SINGLEPLAYER && _GameType != GAMETYPE_HOTSEAT) return;
            if (!CanRedo()) return;

            CurrentGame.GameResult = GAMERESULT_ONGOING;
            MainForm.UpdateHistoryButtons();
            if (_GameType == GAMETYPE_SINGLEPLAYER && _CurrentTurn != _LocalPlayerSide)
            {
                AI.StartAI();
            }
        }

        void AddMoveToHistory(int X, int Z)
        {
            int nAmount = MoveHistory.Count;
            if (HistoryPoint < nAmount)
            {
                MoveHistory.RemoveRange(HistoryPoint, nAmount - HistoryPoint);
            }
            MoveHistory.Add(new Move(X, Z));
            HistoryPoint++;
            MainForm.UpdateHistoryButtons();
        }

        internal bool _CanUndo()
        {
            if (HistoryPoint < 1) return false; // No undo, first move.
            if (GameResult != GAMERESULT_ONGOING) return true; // You can always review a finished or interupted game.
            if (GameType == GAMETYPE_HOTSEAT) return true;
            if (GameType == GAMETYPE_SINGLEPLAYER)
            {
                if (HistoryPoint == 1 && CurrentTurn == _LocalPlayerSide) return false; // If the PC had the first move, you cannot undo that
                return true;
            }
            return false;
        }

        internal bool _CanRedo()
        {
            if (HistoryPoint >= MoveHistory.Count) return false; // Redo finished
            if (GameResult != GAMERESULT_ONGOING) return true; // You can always review a finished or interupted game.
            if (GameType == GAMETYPE_HOTSEAT) return true;
            if (GameType == GAMETYPE_SINGLEPLAYER) return true;
            return false;
        }

        internal static bool CanUndo()
        {
            return CurrentGame._CanUndo();
        }

        internal static bool CanRedo()
        {
            return CurrentGame._CanRedo();
        }

        internal static void UndoMove()
        {
            CurrentGame._UndoMove();
        }

        internal static void RedoMove()
        {
            CurrentGame._RedoMove();
        }

        internal void _UndoMove()
        {
            if (HistoryPoint < 1) return;
            Move ToUndo = MoveHistory[--HistoryPoint];
            for (int Y = 4; Y > 0; Y--)
            {
                if (_GetPosition(ToUndo.X, Y, ToUndo.Z) != POSITION_EMPTY)
                {
                    SetPosition(ToUndo.X, Y, ToUndo.Z, POSITION_EMPTY);
                    CurrentTurn = !CurrentTurn;
                    MainForm.UpdateHistoryButtons();
                    Engine.SetSpotLight();
                    return;
                }
            }
            throw new Exception("No move to undo at location");
        }

        internal void _RedoMove()
        {
            if (HistoryPoint >= MoveHistory.Count) return;
            Move ToRedo = MoveHistory[HistoryPoint++];
            for (int Y = 1; Y <= 4; Y++)
            {
                if (_GetPosition(ToRedo.X, Y, ToRedo.Z) == POSITION_EMPTY)
                {
                    SetPosition(ToRedo.X, Y, ToRedo.Z, CurrentTurn ? POSITION_DARK : POSITION_LIGHT);
                    MainForm.UpdateHistoryButtons();
                    Engine.SetSpotLight();
                    if (GameResult == GAMERESULT_ONGOING)
                        CheckForVictory(ToRedo.X, Y, ToRedo.Z);
                    CurrentTurn = !CurrentTurn;
                    return;
                }
            }
            throw new Exception("No move to redo at location");
        }

        static int GameType;

        bool CurrentTurn = TURN_LIGHT;

        static bool LocalPlayerSide;

        byte GameResult = GAMERESULT_ONGOING;

        static Game CurrentGame;

        internal static int _GameType { get { return GameType; } }
        internal static bool _CurrentTurn { get { return CurrentGame.CurrentTurn; } }
        internal static bool _LocalPlayerSide { get { return LocalPlayerSide; } }
        internal static byte _GameResult { get { return CurrentGame.GameResult; } }

        byte[] GameState = new byte[4 * 4 * 4];

        static Random RNG = new Random();

        internal static int DieRoll(int nMin, int nMax)
        {
            return RNG.Next(nMin, nMax + 1);
        }

        internal static void SetGameResult(byte nResult)
        {
            CurrentGame.GameResult = nResult;
        }

        internal static void HostStart()
        {
            CurrentGame.GameResult = GAMERESULT_ONGOING;
            LocalPlayerSide = DieRoll(0, 1) == 1 ? TURN_LIGHT : TURN_DARK;
            Networking.SendPlayerInfo();
        }

        internal static void JoinStart(string sData)
        {
            CurrentGame.GameResult = GAMERESULT_ONGOING;
            LocalPlayerSide = (sData == "0") ? TURN_LIGHT : TURN_DARK;
        }

        internal static void NewGame(int NewGameType)
        {
            AI.StopAI(); // The AI may not interfere with a new game.
            
            GameType = NewGameType;
            CurrentGame = new Game();
            LocalPlayerSide = false;

            if (GameType == GAMETYPE_INTERNETJOIN ||
                GameType == GAMETYPE_INTERNETHOST)
            {
                CurrentGame.GameResult = GAMERESULT_WAITFORCONNECTION;
                return;
            }
           
            if (GameType != GAMETYPE_HOTSEAT && GameType != GAMETYPE_AIONLY)
            {
                // Randomize who you are.
                LocalPlayerSide = DieRoll(0, 1) == 1 ? TURN_LIGHT : TURN_DARK;
            }

            MainForm.UpdateHistoryButtons();
            Engine.SetSpotLight();

            if ((GameType == GAMETYPE_SINGLEPLAYER &&
               LocalPlayerSide == TURN_DARK) || GameType == GAMETYPE_AIONLY)
                AI.StartAI();
        }

        internal int _GetPosition(int X, int Y, int Z)
        {
            return GameState[(X - 1) + (Y - 1) * 4 + (Z - 1) * 16];
        }

        void SetPosition(int X, int Y, int Z, byte NewPosition)
        {
            GameState[(X - 1) + (Y - 1) * 4 + (Z - 1) * 16] = NewPosition;
        }

        internal static int GetPosition(int X, int Y, int Z)
        {
            return CurrentGame._GetPosition(X, Y, Z);
        }

        void _PerformMove(int X, int Y, int Z)
        {
            SetPosition(X, Y, Z, CurrentTurn ? POSITION_DARK : POSITION_LIGHT);
            CheckForVictory(X, Y, Z);
            CurrentTurn = !CurrentTurn; // Switch turns

            if (this == CurrentGame)
            {
                if ((GameType == GAMETYPE_AIONLY || (GameType == GAMETYPE_SINGLEPLAYER && LocalPlayerSide != _CurrentTurn))
                && _GameResult == GAMERESULT_ONGOING)
                    AI.StartAI();

                if ((GameType == GAMETYPE_INTERNETJOIN ||
                    GameType == GAMETYPE_INTERNETHOST) &&
                    LocalPlayerSide != _CurrentTurn) // Was own turn
                {
                    Networking.SendTurn(X, Z);
                }
            }
        }

        // Parameters are: the first and last rings in the victory row
        // Warning, the last should be the one with the lowest Y, if any.
        void Victory(int X, int Y, int Z)
        {
            GameResult = CurrentTurn ? GAMERESULT_DARKWINS : GAMERESULT_LIGHTWINS;
            MainForm.UpdateHistoryButtons();
            CurrentVictoryRow = new VictoryRow(X, Y, Z);
            Engine.SetSpotLight();
        }

        // X, Y, Z of last move
        void CheckForVictory(int X, int Y, int Z)
        {
            byte Position = CurrentTurn ? POSITION_DARK : POSITION_LIGHT; 

            // Vary X
            if (_GetPosition(1, Y, Z) == Position
                && _GetPosition(2, Y, Z) == Position
                && _GetPosition(3, Y, Z) == Position
                && _GetPosition(4, Y, Z) == Position)
                Victory(0, Y, Z);
            // Vary Y
            if (_GetPosition(X, 1, Z) == Position
                && _GetPosition(X, 2, Z) == Position
                && _GetPosition(X, 3, Z) == Position
                && _GetPosition(X, 4, Z) == Position)
                Victory(X, 0, Z);
            // Vary Z
            if (_GetPosition(X, Y, 1) == Position
                && _GetPosition(X, Y, 2) == Position
                && _GetPosition(X, Y, 3) == Position
                && _GetPosition(X, Y, 4) == Position)
                Victory(X, Y, 0);

            // Cross Y static 1
            if (_GetPosition(1, Y, 1) == Position
                && _GetPosition(2, Y, 2) == Position
                && _GetPosition(3, Y, 3) == Position
                && _GetPosition(4, Y, 4) == Position)
                Victory(0, Y, 0);

            // Cross Y static 2
            if (_GetPosition(4, Y, 1) == Position
                && _GetPosition(3, Y, 2) == Position
                && _GetPosition(2, Y, 3) == Position
                && _GetPosition(1, Y, 4) == Position)
                Victory(5,Y,0);

            // Cross X static 1
            if (_GetPosition(X, 1, 1) == Position
                && _GetPosition(X, 2, 2) == Position
                && _GetPosition(X, 3, 3) == Position
                && _GetPosition(X, 4, 4) == Position)
                Victory(X,0,0);

            // Cross X static 2
            if (_GetPosition(X, 4, 1) == Position
                && _GetPosition(X, 3, 2) == Position
                && _GetPosition(X, 2, 3) == Position
                && _GetPosition(X, 1, 4) == Position)
                Victory(X,5,0);

            // Cross Z static 1
            if (_GetPosition(1, 1, Z) == Position
                && _GetPosition(2, 2, Z) == Position
                && _GetPosition(3, 3, Z) == Position
                && _GetPosition(4, 4, Z) == Position)
                Victory(0,0,Z);

            // Cross Z static 2
            if (_GetPosition(4, 1, Z) == Position
                && _GetPosition(3, 2, Z) == Position
                && _GetPosition(2, 3, Z) == Position
                && _GetPosition(1, 4, Z) == Position)
                Victory(5,0,Z);

            // MultiCross 1
            if (_GetPosition(1, 1, 1) == Position
                && _GetPosition(2, 2, 2) == Position
                && _GetPosition(3, 3, 3) == Position
                && _GetPosition(4, 4, 4) == Position)
                Victory(0,0,0);

            // MultiCross 2
            if (_GetPosition(1, 4, 1) == Position
                && _GetPosition(2, 3, 2) == Position
                && _GetPosition(3, 2, 3) == Position
                && _GetPosition(4, 1, 4) == Position)
                Victory(0,5,0);

            // MultiCross 3
            if (_GetPosition(4, 1, 1) == Position
                && _GetPosition(3, 2, 2) == Position
                && _GetPosition(2, 3, 3) == Position
                && _GetPosition(1, 4, 4) == Position)
                Victory(5,0,0);

            // MultiCross 4
            if (_GetPosition(4, 4, 1) == Position
                && _GetPosition(3, 3, 2) == Position
                && _GetPosition(2, 2, 3) == Position
                && _GetPosition(1, 1, 4) == Position)
                Victory(5,5,0);

            // Game can only end in draw if already over.
            if (Y == 4 && GameResult == GAMERESULT_ONGOING)
            {
                for (int _X = 1; _X < 5; _X++)
                {
                    for (int _Z = 1; _Z < 5; _Z++)
                    {
                        if (_GetPosition(_X, 4, _Z) == POSITION_EMPTY)
                            return;
                    }
                }
                // no moves left.
                GameResult = GAMERESULT_DRAW;
                MainForm.UpdateHistoryButtons();
            }
        }

        internal static Game GetStateCopy()
        {
            return GetStateCopy(CurrentGame);
        }

        internal static Game GetStateCopy(Game ToCopy)
        {
            Game NewGame = new Game();
            NewGame.GameState = (byte[])ToCopy.GameState.Clone();
            NewGame.CurrentTurn = ToCopy.CurrentTurn;
            return NewGame;
        }

        internal static bool PerformMove(int X, int Z, bool Remote)
        {
            if (GameType == GAMETYPE_NOTSTARTED) return false;
            if (_GameResult != GAMERESULT_ONGOING) return false;
            if (GameType == GAMETYPE_AIONLY && !Remote) return false;
            if (GameType != GAMETYPE_HOTSEAT &&  GameType != GAMETYPE_AIONLY)
            {
                if (Remote)
                {
                    if (_CurrentTurn == LocalPlayerSide)
                        return false;
                }
                else
                {
                    if (_CurrentTurn != LocalPlayerSide)
                        return false;
                }
            }
            CurrentGame.AddMoveToHistory(X, Z);

            if (Options.Option_Shaders)
                Engine.SetSpotLight();

            return CurrentGame._PerformMove(X, Z);
        }

        internal bool _PerformMove(int X, int Z)
        {
            if (X < 1 || X > 4 || Z < 1 || Z > 4)
                throw new Exception("Illegal Move");

            for (int Y = 1; Y < 5; Y++)
            {
                if (_GetPosition(X, Y, Z) == POSITION_EMPTY)
                {
                    _PerformMove(X, Y, Z);
                    return true;
                }
            }
            return false;
        }

        internal byte GetGameResult()
        {
            return GameResult;
        }

        internal bool GetCurrentPlayer()
        {
            return CurrentTurn;
        }

        internal static bool PerformMove(int X, int Z)
        {
            return PerformMove(X, Z, false);
        }
    }
}

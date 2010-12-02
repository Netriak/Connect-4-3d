using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Connect_4_3D
{
    class AIAction
    {
        internal int X;
        internal int Z;
        internal Game Result;
        internal decimal Value;
    }
    
    static class AI
    {
        static int AIStartTime;
        static int MaxDepth = 4;

        static Thread AIThread;
        internal static void StartAI()
        {
            MaxDepth = Options.Option_AIDifficulty;
            AIThread = new Thread(new ThreadStart(AIMain));
            AIThread.Start();
        }

        internal static void StopAI()
        {
            if (AIThread != null && AIThread.ThreadState == ThreadState.Running)
                AIThread.Abort();
        }

        static decimal EvaluateRow(int x, int y, int z, Game TestGame)
        {
            // setting x, y or z to zero, means vary upwards. Setting to 5, means vary downwards.
            byte VictoryResult = TestGame.GetCurrentPlayer() ? Game.GAMERESULT_DARKWINS : Game.GAMERESULT_LIGHTWINS;
            int nX = x; int nY = y; int nZ = z;
            int nResult;
            int nOwn = 0; int nOpponent = 0; int nGap = 0;
            for (int L = 0; L < 4; L++)
            {
                if (x < 1) nX++; if (y < 1) nY++; if (z < 1) nZ++;
                if (x > 4) nX--; if (y > 4) nY--; if (z > 4) nZ--;
                nResult = TestGame._GetPosition(nX, nY, nZ);
                if (nResult == VictoryResult)
                    nOwn++;
                else if (nResult != Game.POSITION_EMPTY)
                    nOpponent++;
                else if (nY > 1 && TestGame._GetPosition(nX, nY - 1, nZ) == Game.POSITION_EMPTY)
                    nGap++;
            }

            if (nOwn == 3 && nGap == 1)
                return 10.0m;
            if (nOpponent == 3 && nGap == 1)
                return -10.0m;

            if (MaxDepth > 2)
            {
                if (nOwn == 2 && nOpponent == 0)
                    return 1m;
                if (nOpponent == 2 && nOwn == 0)
                    return -1m;
            }

            if (MaxDepth > 3) // Subtle
            {
                if (nOpponent == 3 && nOwn == 0)
                    return -0.5m;
                if (nOwn == 3 && nOpponent == 0)
                    return 0.5m;
                if (nOwn == 1 && nOpponent == 0)
                    return 0.1m;
                if (nOpponent == 1 && nOwn == 0)
                    return -0.1m;
            }

            return 0.0m;
        }

        static decimal EvaluatePosition(Game TestGame)
        {
            decimal Total = 0;
            for (int y = 0; y < 6; y++)
            {
                for (int z = 0; z < 6; z++)
                {
                    Total += EvaluateRow(0, y, z, TestGame);
                }
            }
            for (int y = 0; y < 6; y++)
            {
                for (int x = 1; x < 5; x++)
                {
                    Total += EvaluateRow(x, y, 0, TestGame);
                }
            }
            if (Total > 40m) Total = 40m;
            if (Total < -40m) Total = -40m;

            return Total + 50m;
        }

        static void AIMove(int X, int Z)
        {
            int nDelay = Environment.TickCount - AIStartTime;

            if (nDelay < 500 && Game._GameType != Game.GAMETYPE_AIONLY)
            {
                Thread.Sleep(500 - nDelay);
            }

            Game.PerformMove(X, Z, true);
        }

        static decimal OptimizeMove(Game TestGame, int nDepth, int nMaxDepth)
        {
            if (MainForm._Instance.IsDisposed)
            {
                AIThread.Abort();
            }

            if (TestGame.GetGameResult() == Game.GAMERESULT_DRAW) return 0.0m;

            // First, check for victorious move. If any, return that. Store the results in a Game-Database.
            // If no moves are victorious, Optimize The result of the next move, read result, and do the same for every move.
            // Moves that result in enemy victory are discarded. Moves that cannot result in enemy victory are scanned further.
            // This is done depth first to preserve memory.
            // If one of the potential enemy reactions results in our victory, that first move gets points. The greater percentage of enemy
            // moves that result in us winning, the bigger the score, displayed in percentage.
            // For futher calculation, submoves that do not immediately result in either result, can be evaluated further, resulting in subpercentages.
            // We stop at a certain depth, the max depth.
            // This depends on difficulty setting.
            // From the final percentages we choose the best one.
            // If multiple best ones are available, we randomize.
            // So what does this function return? A decimal, with 0.0 representing no chance of victory, and 100.0 representing certainty.
            // If depth is lowest, we actually perform said best move.

            Game NewState;
            AIAction NewAction;

            byte VictoryResult = TestGame.GetCurrentPlayer() ? Game.GAMERESULT_DARKWINS : Game.GAMERESULT_LIGHTWINS;

            List<AIAction> ActionList = new List<AIAction>();

            for (int X = 1; X < 5; X++)
            {
                for (int Z = 1; Z < 5; Z++)
                {
                    NewState = Game.GetStateCopy(TestGame);

                    if (NewState._PerformMove(X, Z))
                    {
                        if (NewState.GetGameResult() == VictoryResult)
                        {
                            if (nDepth > 0) // This move results in certain victory. Return 1.0;
                            {
                                // Winning quicker is slightly better. Hint hint, delaying death is good.
                                return 100.0m - nDepth;
                            }
                            // It is our actual turn, and we can win. Do it.
                            AIMove(X, Z);
                            return 100.0m;
                        } // This move needs to be evaluated further.

                        NewAction = new AIAction();
                        NewAction.X = X;
                        NewAction.Z = Z;
                        NewAction.Result = NewState;
                        ActionList.Add(NewAction); // Store this possible move.
                    }
                }
            }

            if (nDepth == nMaxDepth)
            {
                if (nMaxDepth > 1)
                    return EvaluatePosition(TestGame);
                else
                    return 50m;
            }

            // We cannot win this turn.
            // Let's further examine our moves.

            decimal Result;
            decimal Best = 0m;

            foreach (AIAction PossibleMove in ActionList)
            {                                                                   
                Result = 100m - OptimizeMove(PossibleMove.Result, nDepth + 1, nMaxDepth);
                if (MainForm._Instance.IsDisposed)
                {
                    return 0.0m;
                }
                if (Result > 90.0m) // Defwin
                {
                    if (nDepth == 0) // Defwin, do it
                    {
                        AIMove(PossibleMove.X, PossibleMove.Z);
                    }
                    return Result;
                }
                else if (Best < Result)
                {
                    Best = Result;
                }
                PossibleMove.Value = Result;
            }

            if (nDepth == 0)
            {
                List<AIAction> EqualChoiceList = new List<AIAction>();

                foreach (AIAction PossibleMove in ActionList)
                {
                    if (Best > 20m)
                    {                      // Bias
                        if ((PossibleMove.Value + 1m) > Best)
                        {
                            EqualChoiceList.Add(PossibleMove);
                        }
                    }
                    else
                    { // If all moves are bad, less random.
                        if ((PossibleMove.Value + 0.1m) > Best)
                        {
                            EqualChoiceList.Add(PossibleMove);
                        }
                    }
                }

                AIAction FinalChoice = EqualChoiceList[Game.DieRoll(0, EqualChoiceList.Count() - 1)];
                AIMove(FinalChoice.X, FinalChoice.Z);
                return Best;
            }
            else if ((nDepth % 2 == 0) && (nDepth < nMaxDepth) && (nDepth > (nMaxDepth - 3)) && nDepth > 0) // Depth 2, if Max > 3, 4, 5 and Depth 4, if Max is 5.
            {
                ActionList.RemoveAll(delegate(AIAction Action) { return Action.Value < 10m; });
                if (ActionList.Count() == 1)
                {
                    Best = 0m;
                    // There is only 1 viable action. Very, dangerous, analyze further.
                    foreach (AIAction PossibleMove in ActionList)
                    {
                        Result = 100m - OptimizeMove(PossibleMove.Result, nDepth + 1, nDepth + 3);
                        if (MainForm._Instance.IsDisposed)
                        {
                            return 0.0m;
                        }
                        if (Result > 90.0m) // Defwin
                        {
                            if (nDepth == 0) // Defwin, do it
                            {
                                AIMove(PossibleMove.X, PossibleMove.Z);
                            }
                            return Result;
                        }
                        if (Result > Best) Best = Result;
                    }
                }
            }

            return Best;
        }

        static void AIMain()
        {
            // Logic test.
            try
            {
                AIStartTime = Environment.TickCount;
                OptimizeMove(Game.GetStateCopy(), 0, MaxDepth);
            } catch (ThreadAbortException) // Aborted
            {
                return;
            }
        }
    }
}

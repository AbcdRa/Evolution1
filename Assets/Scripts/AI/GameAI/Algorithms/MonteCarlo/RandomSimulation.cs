using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GameAI.GameInterfaces;
using SRandom = Unity.Mathematics.Random;

namespace GameAI.Algorithms.MonteCarlo
{
    /// <summary>
    /// A static class to run random Monte Carlo simulations on a game,
    /// with both parallel and single-threaded algorithms.
    /// </summary>
    public static class RandomSimulation<TGame, TMove, TPlayer>
        where TGame : RandomSimulation<TGame, TMove, TPlayer>.IGame
    {
        /// <summary>
        /// Interface implmented by games to use the MonteCarlo algorithm.
        /// </summary>
        public interface IGame :
            ICopyable<TGame>,
            IDoMove<TMove>,
            IGameOver,
            ICurrentPlayer<TPlayer>,
            ILegalMoves<TMove>,
            IWinner<TPlayer>
        { }
        
        /// <summary>
        /// Return the move with the highest win-rate after performing in parallel the specified number of simulations on the game.
        /// </summary>
        /// <param name="game">The starting gamestate.</param>
        /// <param name="simulations">The number of simulations to perform.</param>
        public static TMove ParallelSearch(TGame game, int simulations)
        {
            TPlayer aiPlayer = game.CurrentPlayer;
            List<TMove> legalMoves = game.GetLegalMoves();
            int count = legalMoves.Count;
            MoveStats[] moveStats = new MoveStats[count];

            Parallel.For(0, simulations,

                () => RandomFactory.Create(),

                (i, loop, localRandom) =>
                {
                    int moveIndex = localRandom.Next(0, count);
                    TGame copy = game.DeepCopy();
                    copy.DoMove(legalMoves[moveIndex]);
                    int atts = 0;
                    while (!copy.IsGameOver())
                    {
                        copy.DoMove(
                            copy.GetLegalMoves().RandomItem(localRandom));
                        atts++;
                        if (atts > 500) throw new Exception("Too many attempts");
                    }
                        

                    Interlocked.Add(ref moveStats[moveIndex].executions, 1);
                    if (copy.IsWinner(aiPlayer))
                        Interlocked.Add(ref moveStats[moveIndex].victories, 1);

                    return localRandom;
                },

                x => { }
            );
            
            int bestMoveFound = 0;
            double bestScoreFound = 0f;
            for (int i = 0; i < count; i++)
            {
                double score = moveStats[i].Score();
                if (score > bestScoreFound)
                {
                    bestScoreFound = score;
                    bestMoveFound = i;
                }
            }

            //for (int i = 0; i < legalMoves.Count; i++)
            //    Console.WriteLine("Move " + legalMoves[i] + " has " + moveStats[i].victories + " victories / " + moveStats[i].executions + " executions.");

            return legalMoves[bestMoveFound];

        }

        /// <summary>
        /// Return the move with the highest win-rate after performing the specified number of simulations on the game.
        /// </summary>
        /// <param name="game">The starting gamestate.</param>
        /// <param name="simulations">The number of simulations to perform.</param>
        public static TMove Search(TGame game, int simulations, SRandom rng)
        {
            TPlayer aiPlayer = game.CurrentPlayer;
            List<TMove> legalMoves = game.GetLegalMoves();
            int count = legalMoves.Count;
            MoveStats[] moveStats = new MoveStats[count];
            int moveIndex;
            TGame copy;

            for (int i = 0; i < simulations; i++)
            {
                moveIndex = rng.NextInt(0, count);
                copy = game.DeepCopy();
                TMove m = legalMoves[moveIndex];
                copy.DoMove(m);
                
                int k = 0;
                while (!copy.IsGameOver())
                {
                    k++;
                    try
                    {
                        //Некооректное удаление парных свойств, отстутствие обнуления пищи
                        List<TMove> moves = copy.GetLegalMoves();
                        m = moves.RandomItem(rng);
                        copy.DoMove(m);
                        if (k > 1000) throw new Exception("Inf game");
                    } catch (Exception e)
                    {
                        

                        throw new Exception($"Cath it {i} ~ {k}"+e.Message);
                    }

                }


                moveStats[moveIndex].executions++;
                if (copy.IsWinner(aiPlayer))
                    moveStats[moveIndex].victories++;
            }

            int bestMoveFound = 0;
            double bestScoreFound = 0f;
            for (int i = 0; i < count; i++)
            {
                double score = moveStats[i].Score();
                Debug.WriteLine(score);
                if (score > bestScoreFound)
                {
                    bestScoreFound = score;
                    bestMoveFound = i;
                }
            }

            return legalMoves[bestMoveFound];
        }

        private struct MoveStats
        {
            internal int executions;
            internal int victories;

            internal double Score() => (double)victories / executions;
        }
    }
}

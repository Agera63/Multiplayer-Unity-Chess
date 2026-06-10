using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace ChessClient
{
    public class StockFishChessClient
    {
        private static readonly string STOCKFISH_PATH = Application.streamingAssetsPath + "/Stockfish/stockfish-windows-x86-64";
        private static readonly int SEARCH_DEPTH = 15;
        private static readonly string[] UCI_INIT_COMMANDS = { "uci" };

        // Reuse process for better performance (optional enhancement)
        private static Process stockfishProcess;
        private static StreamWriter processWriter;
        private static StreamReader processReader;

        /// <summary>
        /// Get the best move from Stockfish for the current board position
        /// </summary>
        /// <param name="board">8x8 char array representing the chess board</param>
        /// <returns>Best move in format "e2-e4" or "No move found"</returns>
        public static string GetBestMoveFromBoard(char[,] board)
        {
            string fen = BoardToFen(board);
            try
            {
                InitializeStockfishProcess();
                return GetBestMoveFromFen(fen);
            }
            finally
            {
                CleanupProcess();
            }
        }

        private static void InitializeStockfishProcess()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = STOCKFISH_PATH;
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;

            stockfishProcess = Process.Start(psi);

            processWriter = stockfishProcess.StandardInput;
            processReader = stockfishProcess.StandardOutput;

            // Initialize UCI protocol
            SendCommand("uci");
            WaitForUciOk();
        }

        private static string GetBestMoveFromFen(string fen)
        {
            SendCommand("position fen " + fen);
            SendCommand("go depth " + SEARCH_DEPTH);

            string line;
            while ((line = processReader.ReadLine()) != null)
            {
                if (line.StartsWith("bestmove"))
                {
                    string[] parts = line.Split(' ');
                    if (parts.Length > 1 && !parts[1].Equals("(none)"))
                    {
                        return FormatMove(parts[1]);
                    }
                    break;
                }
            }
            return "No move found";
        }

        private static void SendCommand(string command)
        {
            processWriter.WriteLine(command);
            processWriter.Flush();
        }

        private static string FormatMove(string move)
        {
            if (move != null && move.Length >= 4)
            {
                Console.WriteLine("Stockfish plays " + move.Substring(0, 2) + " to " + move.Substring(2, 2) + ".");
                return move.Substring(0, 2) + "-" + move.Substring(2, 2);
            }
            return "No move found";
        }

        private static string BoardToFen(char[,] board)
        {
            System.Text.StringBuilder fen = new System.Text.StringBuilder(80); // Pre-allocate capacity

            // Convert board to FEN notation (rank 8 to rank 1)
            for (int row = 7; row >= 0; row--)
            {
                AppendRankToFen(fen, board, row);
                if (row > 0) fen.Append('/');
            }

            // Add FEN metadata: active color, castling, en passant, halfmove, fullmove
            bool stockfishIsWhite = !GameModeManager.instance.playerColor; // Stockfish plays opposite of player
            fen.Append(' ').Append(stockfishIsWhite ? 'w' : 'b').Append(" - - 0 1");

            return fen.ToString();
        }

        private static void AppendRankToFen(System.Text.StringBuilder fen, char[,] board, int row)
        {
            int emptyCount = 0;

            for (int col = 0; col < board.GetLength(1); col++)
            {
                char piece = board[row, col];
                if (piece == '\0')
                {
                    emptyCount++;
                }
                else
                {
                    if (emptyCount > 0)
                    {
                        fen.Append(emptyCount);
                        emptyCount = 0;
                    }
                    fen.Append(piece);
                }
            }

            if (emptyCount > 0)
            {
                fen.Append(emptyCount);
            }
        }

        private static void WaitForUciOk()
        {
            string line;
            while ((line = processReader.ReadLine()) != null)
            {
                if ("uciok".Equals(line))
                {
                    return;
                }
            }
            throw new IOException("Failed to initialize UCI protocol with Stockfish");
        }

        private static void CleanupProcess()
        {
            try
            {
                if (processWriter != null)
                {
                    processWriter.WriteLine("quit");
                    processWriter.Flush();
                    processWriter.Close();
                }
            }
            catch (IOException) { }

            try
            {
                if (processReader != null)
                {
                    processReader.Close();
                }
            }
            catch (IOException) { }

            if (stockfishProcess != null)
            {
                try
                {
                    if (!stockfishProcess.WaitForExit(1000))
                    {
                        stockfishProcess.Kill();
                    }
                }
                catch (Exception)
                {
                    stockfishProcess.Kill();
                }
            }
        }
    }
}
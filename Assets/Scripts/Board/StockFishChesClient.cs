using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace ChessClient
{
    public class StockFishChesClient
    {
        private static readonly string STOCKFISH_PATH = Application.streamingAssetsPath + "/Stockfish/stockfish-windows-x86-64";
        private static readonly int SEARCH_DEPTH = 15;
        private static readonly string[] UCI_INIT_COMMANDS = { "uci" };

        private static Process stockfishProcess;
        private static StreamWriter processWriter;
        private static StreamReader processReader;

        /// <summary>
        /// Gets the best move from Stockfish for the current board position.
        /// Initializes the Stockfish process, queries it, and cleans up afterward.
        /// </summary>
        /// <param name="board">An 8x8 char array representing the current chess board state.</param>
        /// <returns>The best move in the format "e2-e4", or "No move found" if Stockfish cannot determine one.</returns>
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

        /// <summary>
        /// Starts the Stockfish process and initializes the UCI protocol,
        /// setting up the standard input/output streams for communication.
        /// </summary>
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

            SendCommand("uci");
            WaitForUciOk();
        }

        /// <summary>
        /// Sends a FEN position to Stockfish and retrieves the best move at the configured search depth.
        /// </summary>
        /// <param name="fen">The FEN string representing the current board position.</param>
        /// <returns>The best move in the format "e2-e4", or "No move found" if none is returned.</returns>
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

        /// <summary>
        /// Sends a command string to the Stockfish process via standard input.
        /// </summary>
        /// <param name="command">The UCI command to send.</param>
        private static void SendCommand(string command)
        {
            processWriter.WriteLine(command);
            processWriter.Flush();
        }

        /// <summary>
        /// Converts a raw Stockfish move string (e.g. "e2e4") into the game's
        /// internal format (e.g. "e2-e4"), appending a promotion character if present.
        /// </summary>
        /// <param name="move">The raw move string returned by Stockfish.</param>
        /// <returns>The formatted move string, or "No move found" if the input is invalid.</returns>
        private static string FormatMove(string move)
        {
            if (move != null && move.Length >= 4)
            {
                string formatted = move.Substring(0, 2) + "-" + move.Substring(2, 2);
                if (move.Length == 5)
                    formatted += move[4];
                return formatted;
            }
            return "No move found";
        }

        /// <summary>
        /// Converts the internal 8x8 char board into a FEN string that Stockfish can interpret.
        /// </summary>
        /// <param name="board">An 8x8 char array representing the current chess board state.</param>
        /// <returns>A FEN string representing the board position and active color.</returns>
        private static string BoardToFen(char[,] board)
        {
            System.Text.StringBuilder fen = new System.Text.StringBuilder(80);

            for (int row = 7; row >= 0; row--)
            {
                AppendRankToFen(fen, board, row);
                if (row > 0) fen.Append('/');
            }

            bool stockfishIsWhite = !GameModeManager.instance.playerColor;
            fen.Append(' ').Append(stockfishIsWhite ? 'w' : 'b').Append(" - - 0 1");

            return fen.ToString();
        }

        /// <summary>
        /// Appends a single rank (row) of the board to the FEN string builder,
        /// encoding empty squares as numbers and pieces as their char icons.
        /// </summary>
        /// <param name="fen">The StringBuilder to append the rank to.</param>
        /// <param name="board">The full 8x8 char board.</param>
        /// <param name="row">The row index (0–7) to encode.</param>
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

        /// <summary>
        /// Blocks until Stockfish responds with "uciok", confirming the UCI protocol
        /// has been successfully initialized.
        /// </summary>
        /// <exception cref="IOException">Thrown if Stockfish does not respond with "uciok".</exception>
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

        /// <summary>
        /// Sends the quit command to Stockfish and closes all streams,
        /// then terminates the process if it does not exit within one second.
        /// </summary>
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
# Chess Game — Developer Documentation

> For user-facing setup and controls, see [README.md](README.md)

---

## Project Overview

This is a Unity chess game built in C# supporting three game modes: Player vs Bot (PvE), local Player vs Player (PvP), and online Player vs Player (PvP_Online). The bot is powered by Stockfish via a UCI subprocess. Online multiplayer uses Unity Netcode for GameObjects and Unity Relay.

---

## Project Structure

```
Scripts/
├── Board/
│   ├── BoardPos.cs              # Board coordinate system and tile utilities
│   ├── BoardTile.cs             # MonoBehaviour for individual board tiles
│   ├── SimulationClass.cs       # Move legality simulation (check/checkmate/stalemate)
│   └── StockFishChesClient.cs   # Stockfish process wrapper (UCI protocol)
│
├── Controllers/
│   ├── IPlayerController.cs     # Interface for all player controller types
│   ├── HumanController.cs       # Handles local human input
│   ├── StockfishController.cs   # Triggers Stockfish for bot moves
│   └── NetworkController.cs     # Receives moves from the network opponent
│
├── NetworkScripts/
│   ├── RelayManager.cs          # Unity Relay host/join flow and color assignment
│   ├── NetworkGameManager.cs    # NetworkBehaviour — RPC move sync and disconnect handling
│   └── ClientNetworkTransform.cs
│
├── Piece Helpers/               # Pure C# logic layer (no MonoBehaviour)
│   ├── Piece.cs                 # Abstract base class for all pieces
│   ├── PawnHelper.cs
│   ├── KingHelper.cs
│   ├── QueenHelper.cs
│   ├── RookHelper.cs
│   ├── BishopHelper.cs
│   └── KnightHelper.cs
│
├── Piece/                       # MonoBehaviour layer (visuals and animation)
│   ├── MonoBehaviorPiece.cs     # Base MonoBehaviour for all pieces
│   ├── Pawn.cs
│   ├── King.cs
│   ├── Queen.cs
│   ├── Rook.cs
│   ├── Bishop.cs
│   └── Knight.cs
│
├── GameManager.cs               # Central game loop and turn management
├── GameModeManager.cs           # Singleton — persists selected mode and player color
├── PieceManager.cs              # Static registry of all active pieces and the char board
├── MenuController.cs            # Main menu UI
├── GameMenuController.cs        # In-game UI
├── PauseMenuBehavior.cs
├── CameraScript.cs
├── ClickHelper.cs               # Raycasting helper for mouse clicks
├── GameMode.cs                  # Enum: PvE, PvP, PvP_Online
└── PieceType.cs                 # Enum: Pawn, Rook, Knight, Bishop, Queen, King
```

---

## Architecture

### Two-Layer Piece Design

Pieces are split into two layers to keep logic and Unity concerns separate.

**Helper classes** (`Piece Helpers/`) are plain C# objects that inherit from the abstract `Piece` base class. They own all chess logic: legal move calculation, check detection, castling flags, en passant state, and promotion. They hold a reference to their associated `GameObject` but are not themselves `MonoBehaviour`s.

**MonoBehaviour classes** (`Piece/`) are Unity components attached to the piece GameObjects. They handle rendering, animations, and user interaction (e.g. `ShowLegalMoves()`, `PromotePawn()`). Each one holds a reference back to its helper class via a `helperClass` field.

### Board Representation

`PieceManager` maintains two parallel representations of the board:

- `AllPieces` — a `HashSet<Piece>` of all active piece helper objects, used for logic queries.
- `Board` — an `8x8 char[,]` array where each cell holds the piece's icon character (e.g. `'P'` for white pawn, `'p'` for black pawn, `'\0'` for empty). This is rebuilt via `InitializeBoard()` after every move and passed to Stockfish as FEN.

### Coordinate System

Board positions use `BoardPos`, which stores `num` (row, 0–7) and `letter` (column, 0–7). Positions are serialized to and from algebraic notation strings like `"e4"` using `PosToString()` and `StringToPos()`.

Move strings throughout the codebase use the format `"e2-e4"`. Pawn promotions append a 6th character: `"e7-e8q"`.

### Controller Pattern

All player types implement `IPlayerController`:

```csharp
public interface IPlayerController
{
    bool IsHuman { get; }
    void StartTurn(Action<string> onMoveReady);
}
```

`GameManager` calls `StartTurn()` at the start of each turn and passes a callback. The controller invokes that callback with a move string when the move is ready. This means `GameManager` does not need to know whether it is waiting for a mouse click, a Stockfish response, or a network RPC — it just waits for the callback.

| Controller | Source of move |
|---|---|
| `HumanController` | Mouse click processed by `GameManager.Update()` |
| `StockfishController` | Stockfish subprocess via UCI |
| `NetworkController` | `ReceiveMove()` called by `NetworkGameManager` via RPC |

### Move Flow

```
GameManager.StartTurn()
    → currentController.StartTurn(callback)

[Human click / Stockfish response / network RPC]
    → callback(moveString)
    → GameManager.TryMovePiece(moveString)
        → [pawn promotion handling if needed]
        → GameManager.ExecuteMove()
            → Piece.IsValidMove()        // move rules + check simulation
            → PieceManager.Update()      // apply move, clear en passant flags
            → GameManager.IsGameOver()   // checkmate / stalemate check
            → GameManager.StartTurn()    // next turn
```

---

## Move Legality and Simulation

`SimulationClass` is responsible for validating moves without mutating the live game state. It deep-copies the current `char[,]` board and `List<Piece>` using each piece type's copy constructor, then simulates the move on the copies.

Two static entry points cover the two cases:

- `KingSim(piece, targetPos, king)` — used when the king is already in check. Returns `true` if the proposed move gets the king out of check.
- `WillMoveCheckKing(piece, targetPos)` — used when the king is not in check. Returns `true` if the proposed move does not result in the king being in check.

Checkmate detection iterates every active friendly piece against every board square and returns `true` only if no call to `KingSim` passes. Stalemate uses the same exhaustive scan but does not require the king to be in check first.

---

## Stockfish Integration

`StockFishChessClient` (in `ChessClient` namespace) manages the Stockfish process. It is spawned fresh for each move request and shut down immediately after. Communication follows the UCI protocol:

1. Start process, send `uci`, wait for `uciok`.
2. Send `position fen <fen>`.
3. Send `go depth 15`.
4. Read lines until `bestmove` is found.
5. Send `quit` and close streams.

The current board is converted to FEN via `BoardToFen()`. Note that castling availability, en passant target square, and move counters are not currently tracked in the FEN (they are hardcoded as `- - 0 1`), so Stockfish does not account for those rules in its search.

The Stockfish binary is expected at:
```
<Application.streamingAssetsPath>/Stockfish/stockfish-windows-x86-64
```

If `StreamingAssets` is missing or the binary is absent, `StockfishController` will fail silently (the coroutine returns no move).

---

## Online Multiplayer

Online play uses **Unity Netcode for GameObjects** with **Unity Relay** as the transport.

### Session Flow

1. Host clicks **Host** → `RelayManager.CreateRelay()` creates a Relay allocation, displays the join code, and starts the host.
2. Client clicks **Join** and enters the code → `RelayManager.JoinRelay()` joins the allocation and starts the client.
3. On client connection, the host randomly assigns colors, stores them in `GameModeManager`, and loads `GameScene` via `NetworkManager.SceneManager.LoadScene()`.
4. Once all clients finish loading, `NetworkGameManager.SetClientColorClientRpc()` is called to send the client their color, reposition their camera, and trigger `GameManager.InitializeOnlineGame()`.

### Move Synchronization

Moves are sent via RPCs in `NetworkGameManager`:

- **Host → Client:** `SyncMoveClientRpc(moveString)`
- **Client → Host:** `SyncMoveServerRpc(moveString)` (ownership not required)

The receiving side forwards the move string to `NetworkController.ReceiveMove()`, which fires the `onMoveReady` callback registered by `GameManager.StartTurn()`.

### Disconnect Handling

`NetworkGameManager` subscribes to `NetworkManager.OnClientDisconnectCallback`. If the disconnecting client is not the local player, `OnOpponentDisconnected` is fired. `GameManager` listens to this event, shuts down the network session, and triggers the game-over sequence with the local player as the winner.

---

## Game Mode and Color State

`GameModeManager` is a persistent singleton (`DontDestroyOnLoad`) that carries two pieces of state across scene loads:

- `selectedMode` — the active `GameMode` enum value.
- `playerColor` — `true` for white, `false` for black.
- `clientColor` — used during online setup to hold the color to send to the joining client before the scene loads.

---

## Key Gotchas

**En passant flag clearing** — After each move, `GameManager.ExecuteMove()` clears `canMove2Squares` on all pawns belonging to the side that just moved. This is intentional: the window to capture en passant only lasts one turn, and the flag must be cleared once the opponent's turn has ended, not the mover's.

**Promotion event deduplication** — `PawnHelper.CheckPromotionActions()` checks the `promote` event's invocation list before subscribing. This prevents duplicate handlers from being added when a pawn is selected multiple times before moving.

**Online client initialization** — The online client does not call `AssignControllers()` or `StartTurn()` in `GameManager.Start()`. It waits for `SetClientColorClientRpc` to arrive and then calls `InitializeOnlineGame()`. The `isOnlineClient` flag in `Start()` gates this.

**Simulation uses copy constructors** — Every `Piece` subclass defines a copy constructor that takes an instance of itself. `SimulationClass.DeepCopyPieces()` depends on all six piece types having this constructor. Adding a new piece type requires adding a copy constructor and a branch in `DeepCopyPieces()`.

**`PvP` mode** — `GameMode.PvP` exists in the enum but `AssignControllers()` has no `case` for it. Local two-player mode is not currently wired up.
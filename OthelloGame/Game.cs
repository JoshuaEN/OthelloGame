using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OthelloGame.Controllers;

namespace OthelloGame
{
    /// <summary>
    /// Core game code.
    /// </summary>
    public class Game : ICloneable
    {
        /// <summary>
        /// Initializes a game with the given board size.
        /// </summary>
        /// <param name="board_size">The length of one side of a board, overall board size will be board size * board size.</param>
        public Game(int board_size)
        {
            this.BoardSize = board_size;

            PlayerControllers = new Controllers.ControllerBase[2];
            Board = new int[BoardSize * BoardSize];
            BoardWeights = new int[Board.Length];
            ValidMoves = new List<int>(10);
            MoveHistory = new Stack<int>(60);

            SetupBoard();
            SetupBoardWeightTable();

            SetGameState(GameStates.Setup);
        }

        public int BoardSize { get; private set; }
        public int[] Board { get; private set; }
        public int[] BoardWeights { get; private set; }

        public List<int> ValidMoves { get; set; }
        public Stack<int> MoveHistory { get; private set; }

        public int ActivePlayer { get; private set; }
        public Controllers.ControllerBase ActivePlayerController { get; private set; }
        public Controllers.ControllerBase[] PlayerControllers { get; private set; }

        public int Winner { get; private set; }

        public GameStates GameState { get; private set; }

        #region Board Setup

        /// <summary>
        /// Clears off the board then add the four disks to their starting locations.
        /// </summary>
        private void SetupBoard()
        {
            for (var i = 0; i < this.Board.Length; i++)
            {
                var xy = this.GetXY(i);
                SetTile(xy.x, xy.y, -1);
            }

            int board_mid_top_left = (int)Math.Floor(Convert.ToDouble(BoardSize) / 2.0d) - 1;
            SetTile(board_mid_top_left, board_mid_top_left, (int)Disks.P1);
            SetTile(board_mid_top_left + 1, board_mid_top_left, (int)Disks.P0);
            SetTile(board_mid_top_left, board_mid_top_left + 1, (int)Disks.P0);
            SetTile(board_mid_top_left + 1, board_mid_top_left + 1, (int)Disks.P1);

        }

        /// <summary>
        /// Fills up the BoardWeights list with board weights for use by tiebreaking methods mainly.
        /// </summary>
        protected void SetupBoardWeightTable()
        {
            var max_pos = BoardSize - 1;

            Action<int, int, int> transpose = (int x, int y, int val) =>
            {
                BoardWeights[GetIndex(x, y)] = val;
                BoardWeights[GetIndex(x, max_pos - y)] = val;
                BoardWeights[GetIndex(max_pos - x, y)] = val;
                BoardWeights[GetIndex(max_pos - x, max_pos - y)] = val;
            };

            // Weights from http://www.samsoft.org.uk/reversi/strategy.htm

            transpose(0, 0, 99);

            transpose(1, 0, -8);
            transpose(0, 1, -8);

            transpose(2, 0, 8);
            transpose(0, 2, 8);

            transpose(3, 0, 6);
            transpose(0, 3, 6);

            transpose(1, 1, -24);

            transpose(2, 1, -4);
            transpose(1, 2, -4);

            transpose(3, 1, -3);
            transpose(1, 3, -3);

            transpose(3, 2, 4);
            transpose(2, 3, 4);

            transpose(2, 2, 7);

            transpose(3, 3, 0);
        }

        #endregion

        #region Game Controls

        public Game Start()
        {
            SetGameState(GameStates.Started);
            ActivePlayer = 1;
            NextTurn();

            return this;
        }

        public Game Reset()
        {
            SetupBoard();
            ActivePlayerController = null;
            ActivePlayer = -1;
            MoveHistory.Clear();

            SetGameState(GameStates.Setup);

            foreach (var controller in PlayerControllers)
            {
                controller.Reset();
            }

            return this;
        }

        public Game Restart()
        {
            Reset().Start();
            return this;
        }

        #endregion

        #region Making moves

        public bool Move(int i)
        {
            // Check if a move is valid before allowing it.
            if (ValidMoves.Contains(i))
            {
                PreviewOnMove(ActivePlayer, i, true);

                PlaceDisk(i);
                MoveHistory.Push(i);

                OnMove(ActivePlayer, i, true);
                
                NextTurn();
                
                return true;
            }
            else
            {
                PreviewOnMove(ActivePlayer, i, false);
                OnMove(ActivePlayer, i, false);
                return false;
            }
        }

        public Game PlaceDisk(int i)
        {
#if DEBUG
            if (FindValidMoves().IndexOf(i) == -1)
                throw new Exceptions.LogicError();
#endif


            var xy = GetXY(i);
            var x = xy.x;
            var y = xy.y;

            ExploreDirections(x, y, ActivePlayer, (_x, _y, _disk, _ours) =>
            {
                SetTile(_x, _y, _ours);
            }, null, null);

            SetTile(x, y, ActivePlayer);

            return this;
        }

        #endregion

        #region Turn Related Methods

        public Game NextTurn()
        {
            AdvanceBoard();

            OnGameBoardAdvanced();

            if (GameAtEnd())
                return this;

            if (SkipTurn())
            {
                NextTurn();
                return this;
            }

            this.ActivePlayerController.YourMove();

            OnGameTurnAdvanced();

            return this;
        }

        public Game AdvanceBoard()
        {
            NextPlayer();
            ProcessBoard();
            return this;
        }

        public Game ProcessBoard()
        {
            ValidMoves = FindValidMoves();
            return this;
        }

        public bool GameAtEnd()
        {
            if (GameStates.Deadlock == GameState)
                return true;

            if (ValidMoves.Count > 0)
                return false;

            // If there are currently no valid moves, advance the board and check again,
            // if there are still no valid moves, then the game is at an end.

            AdvanceBoard();

            if (ValidMoves.Count > 0)
            {
                AdvanceBoard();
                return false;
            }

            SetGameState(GameStates.Deadlock);
            return true;
        }

        public bool SkipTurn()
        {
            if (ValidMoves.Count == 0)
                return true;
            return false;
        }

        public Game NextPlayer()
        {
            ActivePlayer = InactivePlayer();
            ActivePlayerController = PlayerControllers[ActivePlayer];
            return this;
        }

        #endregion

        protected List<int> FindValidMoves(int ours)
        {
            var valid = new List<int>();

            for (var i = 0; i < this.Board.Length; i++)
            {
                var xy = GetXY(i);
                var x = xy.x;
                var y = xy.y;

                var disk = GetTile(x, y);

                // If the disk isn't empty then this space can't be a valid move.
                if (-1 != disk)
                {
                    continue;
                }

                if (ExploreDirectionsValidMove(x, y, ours))
                    valid.Add(i);

            }

            return valid;
        }

        protected List<int> FindValidMoves()
        {
            return FindValidMoves(ActivePlayer);
        }

        #region Explorer functions

        public Directions ExploreDirections(
            int x,
            int y,
            int ours,
            Action<int, int, int, int> on_valid,
            Action<int, int, int, int> on_all,
            Func<int, int, int, int, int> is_valid)
        {
            var d = new Directions();
            d.nw = ExploreDirection(x, y, -1, -1, ours, on_valid, on_all, is_valid); 	// North West
            d.n = ExploreDirection(x, y, 0, -1, ours, on_valid, on_all, is_valid); 		// North
            d.ne = ExploreDirection(x, y, 1, -1, ours, on_valid, on_all, is_valid); 		// North East
            d.e = ExploreDirection(x, y, 1, 0, ours, on_valid, on_all, is_valid); 		// East
            d.se = ExploreDirection(x, y, 1, 1, ours, on_valid, on_all, is_valid); 	// South East
            d.s = ExploreDirection(x, y, 0, 1, ours, on_valid, on_all, is_valid); 		// South
            d.sw = ExploreDirection(x, y, -1, 1, ours, on_valid, on_all, is_valid); 	// South West
            d.w = ExploreDirection(x, y, -1, 0, ours, on_valid, on_all, is_valid); 		// West

            return d;
        }

        public bool ExploreDirection(
            int x,
            int y,
            int mod_x,
            int mod_y,
            int ours,
            Action<int, int, int, int> on_valid,
            Action<int, int, int, int> on_all,
            Func<int, int, int, int, int> is_valid)
        {
            int mx = x + mod_x;
            int my = y + mod_y;
            int disk = GetTile(mx, my);

            if (null != on_all)
            {
                on_all(mx, my, disk, ours);
            }

            if (null != is_valid)
            {
                var res = is_valid(mx, my, disk, ours);

                if (0 == res)
                {
                    return false;
                }
                else if (1 == res)
                {
                    return true;
                }

            }
            else
            {
                if (disk == (int)Disks.Undefined ||
                    disk == (int)Disks.Null)
                    return false;

                if (disk == ours)
                    return true;
            }

            var deeper_result = ExploreDirection(mx, my, mod_x, mod_y, ours, on_valid, on_all, is_valid);

            if (null != on_valid && true == deeper_result)
            {
                on_valid(mx, my, disk, ours);
            }

            return deeper_result;
        }

        public bool ExploreDirectionsValidMove(int x, int y, int ours)
        {
            // A space is a valid place for a move if in any direction it is directly in contact with an
            // opposing disk, and pass that opposing disk there is any number of opposing disks followed by
            // another of its own disks.
            return
                ExploreDirectionValidMove(x, y, -1, -1, ours, 0) || 	// North West
                ExploreDirectionValidMove(x, y, 0, -1, ours, 0) ||		// North
                ExploreDirectionValidMove(x, y, 1, -1, ours, 0) || 		// North East
                ExploreDirectionValidMove(x, y, 1, 0, ours, 0) || 		// East
                ExploreDirectionValidMove(x, y, 1, 1, ours, 0) || 	// South East
                ExploreDirectionValidMove(x, y, 0, 1, ours, 0) || 		// South
                ExploreDirectionValidMove(x, y, -1, 1, ours, 0) || 	// South West
                ExploreDirectionValidMove(x, y, -1, 0, ours, 0); 		// West
        }

        public bool ExploreDirectionValidMove(int x, int y, int mod_x, int mod_y, int ours, int probe_depth)
        {
            int mx = x + mod_x;
            int my = y + mod_y;

            if (mx < 0 || mx >= this.BoardSize || my < 0 || my >= this.BoardSize)
                return false;

            int disk = Board[GetIndexUnchecked(mx, my)];

            if (disk == -1)
                return false;

            if (disk == ours)
            {
                if (probe_depth > 0)
                    return true;
                else
                    return false;
            }

            return ExploreDirectionValidMove(mx, my, mod_x, mod_y, ours, probe_depth + 1);
        }

        public bool ExploreDirectionsStableCounts(int x, int y, int ours, Dictionary<int, int> stable_statuses)
        {
            // A disk is stable if for every line which can pass through it (north to south, east to west, north east to south west, and north west to south east)
            // one end of the line terminates at the edge of the board while passing through only disks of the same color.
            return
                (ExploreDirectionStableCounts(x, y, -1, -1, ours, stable_statuses) || 	// North West
                ExploreDirectionStableCounts(x, y, 1, 1, ours, stable_statuses)) 	    // South East
                &&
                (ExploreDirectionStableCounts(x, y, 0, -1, ours, stable_statuses) ||		// North
                ExploreDirectionStableCounts(x, y, 0, 1, ours, stable_statuses)) 		// South
                &&
                (ExploreDirectionStableCounts(x, y, 1, -1, ours, stable_statuses) || 	// North East 
                ExploreDirectionStableCounts(x, y, -1, 1, ours, stable_statuses)) 	    // South West
                &&
                (ExploreDirectionStableCounts(x, y, 1, 0, ours, stable_statuses) || 		// East
                ExploreDirectionStableCounts(x, y, -1, 0, ours, stable_statuses)); 		// West
        }

        public bool ExploreDirectionStableCounts(int x, int y, int mod_x, int mod_y, int ours, Dictionary<int, int> stable_statuses)
        {
            int mx = x + mod_x;
            int my = y + mod_y;

            var _i = GetIndex(mx, my);

            if (_i < 0 || _i >= Board.Length)
                return true;

            int _value;

            if (stable_statuses.TryGetValue(_i, out _value) && _value == ours)
                return true;
            else
                return false;
        }

        public bool ExploreDirectionsFrontierCounts(int x, int y)
        {
            // A disk is on the frontier if any of the tiles on its border contain a blank space.
            return
                ExploreStepFrontierCounts(GetIndex(x - 1, y - 1)) || 	// North West
                ExploreStepFrontierCounts(GetIndex(x, y - 1)) ||		// North
                ExploreStepFrontierCounts(GetIndex(x + 1, y - 1)) || 		// North East
                ExploreStepFrontierCounts(GetIndex(x + 1, y)) || 		// East
                ExploreStepFrontierCounts(GetIndex(x + 1, y + 1)) || 	// South East
                ExploreStepFrontierCounts(GetIndex(x, y + 1)) || 		// South
                ExploreStepFrontierCounts(GetIndex(x - 1, y + 1)) || 	// South West
                ExploreStepFrontierCounts(GetIndex(x - 1, y)); 		// West
        }

        public bool ExploreStepFrontierCounts(int i)
        {
            if (i == -1)
                return false;

            int disk = Board[i];

            if (disk == -1)
                return true;
            else
                return false;
        }

        #endregion

        #region Tile getters and setters.

        public int GetTile(int x, int y)
        {
            var i = this.GetIndex(x, y);
            if (-1 == i)
                return -2;

            return this.Board[i];
        }

        public int GetTile(XY xy)
        {
            return GetTile(xy.x, xy.y);
        }

        public Game SetTile(int x, int y, int value)
        {
            var i = this.GetIndex(x, y);
            this.Board[i] = value;

            OnTileChanged(x, y, i, value);
            return this;
        }

        public XY GetXY(int i)
        {
            var x = i % this.BoardSize;
            var y = (i - x) / this.BoardSize;
            return new XY(x, y);
        }

        #endregion

        #region Index methods

        public int GetIndex(int x, int y)
        {
            if (!CheckXY(x, y))
                return -1;

            return this.BoardSize * y + x;
        }

        public int GetIndex(XY xy)
        {
            return GetIndex(xy.x, xy.y);
        }

        public int GetIndexUnchecked(int x, int y)
        {
            return this.BoardSize * y + x;
        }

        public bool CheckXY(int x, int y)
        {
            if (x >= 0 && x < this.BoardSize &&
                y >= 0 && y < this.BoardSize)
            {
                return true;
            }

            return false;
        }

        public bool CheckXY(XY xy)
        {
            return CheckXY(xy.x, xy.y);
        }

        #endregion

        #region Player Swap Methods

        public int InactivePlayer()
        {
            return OtherPlayer(ActivePlayer);
        }

        public int OtherPlayer(int player)
        {
            if (1 == player)
                return 0;
            else
                return 1;
        }

        #endregion

        #region Counters

        public Counts GetCounts()
        {
            var counts = new Counts(0, 0, 0);

            for (var i = 0; i < Board.Length; i++)
            {
                var tile = Board[i];
                counts[tile] += 1;
            }

            return counts;
        }

        #region Stable Count

        public Counts GetStableCounts()
        {
            var counts = new Counts(0, 0, 0);

            var stable_status = new Dictionary<int, int>(Board.Length);

            var max_pos = BoardSize - 1;
            GetStableCountsEvalCorner(0, 0, 1, 1, -2, ref stable_status, ref counts);
            GetStableCountsEvalCorner(0, max_pos, 1, -1, -2, ref stable_status, ref counts);
            GetStableCountsEvalCorner(max_pos, 0, -1, 1, -2, ref stable_status, ref counts);
            GetStableCountsEvalCorner(max_pos, max_pos, -1, -1, -2, ref stable_status, ref counts);

            return counts;
        }

        protected void GetStableCountsEvalCorner(int x, int y, int mod_x, int mod_y, int target, ref Dictionary<int, int> stable_statuses, ref Counts counts)
        {
            if (x < 0 || x >= this.BoardSize || y < 0 || y >= this.BoardSize)
                return;

            var disk = Board[GetIndexUnchecked(x, y)];

            if (-1 == disk)
                return;

            if (-2 == target)
                target = disk;

            if (target != disk)
                return;

            GetStableCountsCheckDirection(x, y, disk, ref stable_statuses, ref counts);

            int ix = x, iy = y, idisk = 0;
            int x_c = 0, y_c = 0;

            while (true)
            {
                ix += mod_x;
                idisk = GetTile(ix, iy);

                if (idisk != target)
                    break;

                x_c += 1;

                if (GetStableCountsCheckDirection(ix, iy, idisk, ref stable_statuses, ref counts))
                    break;
            }

            ix = x; iy = y;

            while (true)
            {
                iy += mod_y;

                idisk = GetTile(ix, iy);

                if (idisk != target)
                    break;

                y_c += 1;

                if (GetStableCountsCheckDirection(ix, iy, idisk, ref stable_statuses, ref counts))
                    break;
            }

            GetStableCountsEvalCorner(x + mod_x, y + mod_y, mod_x, mod_y, target, ref stable_statuses, ref counts);
        }

        protected bool GetStableCountsCheckDirection(int x, int y, int disk, ref Dictionary<int, int> stable_statuses, ref Counts counts)
        {
            var i = GetIndex(x, y);

            if (stable_statuses.ContainsKey(i))
            {
                return true;
            }
            else
            {
                if (ExploreDirectionsStableCounts(x, y, disk, stable_statuses))
                {
                    stable_statuses[i] = disk;
                    counts[disk] += 1;
                }

                return false;
            }
        }

        #endregion

        #region Frontier Count

        public Counts GetFrontierCounts()
        {
            var counts = new Counts(0, 0, 0);

            for (var i = 0; i < Board.Length; i++)
            {
                var disk = Board[i];
                var xy = GetXY(i);

                if (disk == -1)
                    continue;

                if (ExploreDirectionsFrontierCounts(xy.x, xy.y))
                    counts[disk] += 1;

            }

            return counts;
        }

        #endregion

        private static int[] edge_pieces = {
            1,2,3,4,5,6,
            8,16,24,32,40,48,
            15,23,31,39,47,55,
            57,58,59,60,61,62,
            0,7,56,63
        };
        public Counts GetEdgeCounts()
        {
            var counts = new Counts(0, 0, 0);

            foreach (var index in edge_pieces)
            {
                counts[Board[index]] += 1;
            }

            return counts;
        }

        #endregion

        #region Flippers

        public int[][] BoardPermutations()
        {
            return new int[][] {
                Board,
                BoardFlip180Dec(),
                BoardFlipH(),
                BoardFlipV()
            };
        }

        public int[] BoardFlip180Dec()
        {
            int[] b180 = new int[Board.Length];

            for (var i = 0; i < Board.Length; i++)
            {
                var op_i = Board.Length - 1 - i;
                b180[i] = Board[op_i];
                b180[op_i] = Board[i];
            }

            return b180;
        }

        public int[] BoardFlipH()
        {
            int[] bfh = new int[Board.Length];

            for (var i = 0; i < Board.Length; i++)
            {
                var xy = GetXY(i);
                var x = xy.x;
                var y = xy.y;

                var op_x = y;
                var op_y = x;

                bfh[i] = GetTile(y, x);
                bfh[GetIndex(y, x)] = Board[i];
            }

            return bfh;
        }

        public int[] BoardFlipV()
        {
            int[] bfv = new int[Board.Length];

            for (var i = 0; i < Board.Length; i++)
            {
                var xy = GetXY(i);
                var x = xy.x;
                var y = xy.y;

                var op_x = BoardSize - 1 - y;
                var op_y = BoardSize - 1 - x;

                bfv[i] = GetTile(op_x, op_y);
                bfv[GetIndex(op_x, op_y)] = Board[i];
            }

            return bfv;
        }

        #endregion

        #region Game Status

        public bool Running
        {
            get
            {
                if (GameState == GameStates.Started)
                    return true;
                return false;
            }
        }

        public bool Finished
        {
            get
            {
                if (GameState == GameStates.Deadlock)
                    return true;
                return false;
            }
        }

        protected Game SetGameState(GameStates game_state)
        {
            var old_state = GameState;

            GameState = game_state;

            if (GameState == GameStates.Deadlock)
            {
                Winner = GetWinner();
            }

            OnGameStateChanged(old_state, game_state);
            return this;
        }

        #endregion

        public int GetWinner()
        {
            var counts = GetCounts();

            if (counts.p0 == counts.p1)
                return -1;
            else if (counts.p0 > counts.p1)
                return 0;
            else if (counts.p1 > counts.p0)
                return 1;
            else
                throw new Exceptions.LogicError();
        }

        public string BoardString()
        {
            return Game.BoardString(Board);
        }

        public static string BoardString(int[] board)
        {
            return string.Join("", board.Select(x => x == -1 ? "_" : x.ToString()).ToArray());
        }

        public string BoardPrettyString()
        {
            var str = "";
            for (var x = 0; x < BoardSize - 1; x++)
            {
                for (var y = 0; y < BoardSize - 1; y++)
                {
                    var value = Board[GetIndex(y, x)];

                    if (value == -1)
                        str += " ";
                    else if (value == 0)
                        str += "X";
                    else
                        str += "O";

                }

                str += "\n";
            }

            return str;
        }

        public string JSBoardString()
        {
            return string.Join(",", Board.Select(x => (x == -1 ? "" : x.ToString())).ToArray());
        }

        #region Enums

        public enum Disks : int { Undefined = -2, Null = -1, P0 = 0, P1 = 1 }
        public enum GameStates { Setup, Starting, Started, Deadlock }

        #endregion

        #region Structs

        public struct XY
        {
            public XY(int _x, int _y)
            {
                this.x = _x;
                this.y = _y;
            }

            public int x;
            public int y;
        }

        public struct Counts
        {
            public Counts(int nil, int p0, int p1)
            {
                this.nil = nil;
                this.p0 = p0;
                this.p1 = p1;
            }

            public int this[int key]
            {
                get
                {
                    if (-1 == key)
                        return nil;
                    else if (0 == key)
                        return p0;
                    else if (1 == key)
                        return p1;
                    else
                        throw new ArgumentException("Expected key between -1 and 1, not " + key);
                }
                set
                {
                    if (-1 == key)
                        nil = value;
                    else if (0 == key)
                        p0 = value;
                    else if (1 == key)
                        p1 = value;
                    else
                        throw new ArgumentException("Expected key between -1 and 1, not " + key);
                }
            }

            public int nil;
            public int p0;
            public int p1;
        }

        public struct Directions
        {
            public Directions(bool nw, bool n, bool ne, bool e, bool se, bool s, bool sw, bool w)
            {
                this.nw = nw;
                this.n = n;
                this.ne = ne;
                this.e = e;
                this.se = se;
                this.s = s;
                this.sw = sw;
                this.w = w;
            }

            public bool nw;
            public bool n;
            public bool ne;
            public bool e;
            public bool se;
            public bool s;
            public bool sw;
            public bool w;
        }

        #endregion

        public object Clone()
        {
            var game = new Game(BoardSize);
            game.Board = this.Board.Clone() as int[];
            game.BoardWeights = this.BoardWeights.Clone() as int[];
            game.ActivePlayer = this.ActivePlayer;
            game.Winner = this.Winner;
            game.GameState = this.GameState;
            game.ValidMoves = new List<int>(this.ValidMoves);
            game.MoveHistory = new Stack<int>(this.MoveHistory);

            return game;
        }

        protected virtual void OnMove(int player, int tile_index, bool valid)
        {
            EventHandler<Events.MoveBaseEventArgs> handler = MoveMade;
            if (handler != null)
            {
                handler(this, new Events.MoveBaseEventArgs(player, tile_index, valid));
            }
        }

        protected virtual void PreviewOnMove(int player, int tile_index, bool valid)
        {
            EventHandler<Events.MoveBaseEventArgs> handler = PreviewMoveMade;
            if (handler != null)
            {
                handler(this, new Events.MoveBaseEventArgs(player, tile_index, valid));
            }
        }

        protected virtual void OnTileChanged(int x, int y, int i, int owner)
        {
            EventHandler<Events.TileChangedEventArgs> handler = TileChanged;
            if (handler != null)
            {
                handler(this, new Events.TileChangedEventArgs(x, y, i, owner));
            }
        }

        protected virtual void OnGameStateChanged(GameStates old_state, GameStates new_state)
        {
            EventHandler<Events.GameStateChangedEventArgs> handler = GameStateChanged;
            if (handler != null)
            {
                handler(this, new Events.GameStateChangedEventArgs(old_state, new_state));
            }
        }

        protected virtual void OnGameBoardAdvanced()
        {
            EventHandler<Events.GameBoardAdvancedEventArgs> handler = GameBoardAdvanced;
            if (handler != null)
            {
                handler(this, new Events.GameBoardAdvancedEventArgs());
            }
        }

        protected virtual void OnGameTurnAdvanced()
        {
            EventHandler<Events.GameTurnAdvancedEventArgs> handler = GameTurnAdvanced;
            if (handler != null)
            {
                handler(this, new Events.GameTurnAdvancedEventArgs());
            }
        }

        public event EventHandler<Events.MoveBaseEventArgs> PreviewMoveMade;
        public event EventHandler<Events.MoveBaseEventArgs> MoveMade;
        public event EventHandler<Events.TileChangedEventArgs> TileChanged;
        public event EventHandler<Events.GameStateChangedEventArgs> GameStateChanged;
        public event EventHandler<Events.GameBoardAdvancedEventArgs> GameBoardAdvanced;
        public event EventHandler<Events.GameTurnAdvancedEventArgs> GameTurnAdvanced;
    }
}

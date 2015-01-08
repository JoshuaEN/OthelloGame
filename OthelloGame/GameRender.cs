using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OthelloGame
{
    /// <summary>
    /// Class used to convert game and controller states into visual data and provide an interface between the user and the game.
    /// </summary>
    public class GameRender
    {
        public GameRender(MainWindow window, Grid grid, Label debugLabel, OthelloGame.Game game)
        {
            Grid = grid;
            DebugLabel = debugLabel;
            Game = game;
            Window = window;

            game.GameStateChanged += game_GameStateChanged;
            game.GameBoardAdvanced += game_GameBoardAdvanced;
            game.GameTurnAdvanced += game_GameTurnAdvanced;
            game.MoveMade += game_MoveMade;
            game.PreviewMoveMade += game_PreviewMoveMade;
            game.TileChanged += game_TileChanged;

            window.KeyUp += window_KeyUp;

            // Subscribe to controller events.
            for (var i = 0; i < Game.PlayerControllers.Length; i++)
            {
                var controller = Game.PlayerControllers[i];

                if (controller is Controllers.AIMinimax)
                {
                    var aiminimax_controller = (Controllers.AIMinimax)controller;
                    aiminimax_controller.PlayerCalculating += aiminimax_controller_PlayerCalculating;
                    aiminimax_controller.PlayerCalculated += aiminimax_controller_PlayerCalculated;
                }
            }

            GameTiles = new List<GameTile>(game.Board.Length);
            Waiting = false;
            HighlightValidMoves = true;

            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            // Generate board grid.
            for (var i = 0; i < game.BoardSize; i++)
            {
                var row_def = new RowDefinition();
                row_def.SharedSizeGroup = "SG1";
                grid.RowDefinitions.Add(row_def);

                var col_def = new ColumnDefinition();
                col_def.SharedSizeGroup = "SG1";
                grid.ColumnDefinitions.Add(col_def);
            }

            grid.Children.Clear();

            // Add game tiles to the board grid.
            for (var y = 0; y < game.BoardSize; y++)
            {
                for (var x = 0; x < game.BoardSize; x++)
                {
                    var tile = new GameTile();
                    var i = game.GetIndex(x, y);
                    tile.Owner = game.Board[i];
                    tile.FooterText = i.ToString();

                    Grid.SetColumn(tile, x);
                    Grid.SetRow(tile, y);

                    tile.MouseUp += tile_MouseUp;

                    grid.Children.Add(tile);
                    GameTiles.Add(tile);
                }
            }

            LastMoveIndex = -1;
            LastMoveEffects = new List<int>();

            Draw();
        }

        /// <summary>
        /// Target Game
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// Grid to draw the game on.
        /// </summary>
        public System.Windows.Controls.Grid Grid { get; set; }

        /// <summary>
        /// Window where the game is hosted.
        /// </summary>
        public MainWindow Window { get; set; }

        public Label DebugLabel { get; set; }

        /// <summary>
        /// List of game tiles, their index corresponds to their related place on the game board.
        /// </summary>
        public List<OthelloGame.GameTile> GameTiles { get; private set; }

        /// <summary>
        /// Is an AI controller currently running calculations.
        /// </summary>
        public bool Waiting { get; private set; }

        /// <summary>
        /// Should valid moves by highlighted?
        /// </summary>
        public bool HighlightValidMoves { get; set; }

        /// <summary>
        /// Should the active controller be given the chance to draw data?
        /// </summary>
        public bool DrawControllerData { get; set; }

        /// <summary>
        /// The last move that took place, used to highlight last move.
        /// </summary>
        private int LastMoveIndex { get; set; }

        /// <summary>
        /// The disk(s) effected by the last move, used to highlight disks captured by the last move.
        /// </summary>
        private List<int> LastMoveEffects { get; set; }

        /// <summary>
        /// Draws information on the grid.
        /// </summary>
        public void Draw()
        {
            // Clean up the grid.
            RedrawGrid();

            // Perhaps allow the active controller to draw on the grid, if it wants.
            if (Game.ActivePlayerController != null && Waiting == false && DrawControllerData == true && Game.Finished == false)
            {
                Game.ActivePlayerController.DrawBoard(this);
            }
        }

        /// <summary>
        /// Cleans up the grid and draws default information.
        /// </summary>
        public void RedrawGrid()
        {
            for (var i = 0; i < GameTiles.Count; i++)
            {
                var tile = GameTiles[i];
                tile.HeaderText = "";
                tile.FooterText = i + " | " + Game.BoardWeights[i];
                tile.HighlightMode = GameTile.HighlightModes.None;

                if (HighlightValidMoves && Game.ValidMoves.IndexOf(i) != -1)
                {
                    tile.HighlightMode = GameTile.HighlightModes.ValidMove;
                }
                else if (LastMoveIndex == i)
                {
                    tile.HighlightMode = GameTile.HighlightModes.LastMove;
                }
                else if (LastMoveEffects.IndexOf(i) != -1)
                {
                    tile.HighlightMode = GameTile.HighlightModes.LastMoveEffect;
                }
            }
        }

        /// <summary>
        /// Used to draw information about who's turn it is, and the result of the game if the game is over.
        /// </summary>
        public void DrawTurnInfo()
        {
            var counts = Game.GetCounts();
            Window.lblScore.Content = counts[0] + " : " + counts[1];

            Window.lblPlayer1GameInfo.Content = "Player One";
            Window.lblPlayer2GameInfo.Content = "Player Two";

            if (Game.Finished)
            {
                Window.lblScore.Visibility = System.Windows.Visibility.Visible;

                if (Game.Winner == -1)
                {
                    Window.lblPlayer1GameInfo.Content = "Tied - " + Window.lblPlayer1GameInfo.Content;
                    Window.lblPlayer2GameInfo.Content += " - Tied";
                }
                else if (Game.Winner == 0)
                {
                    Window.lblPlayer1GameInfo.Content = "Winner - " + Window.lblPlayer1GameInfo.Content;
                }
                else
                {
                    Window.lblPlayer2GameInfo.Content += " - Winner";
                }
            }
            else
            {
                Window.lblScore.Visibility = System.Windows.Visibility.Hidden;

                if (Game.ActivePlayer == 0)
                    Window.lblPlayer1GameInfo.Content = "Current Player - " + Window.lblPlayer1GameInfo.Content;
                else
                    Window.lblPlayer2GameInfo.Content += " - Current Player";
            }
        }

        #region Event Handlers

        void game_GameStateChanged(object sender, Events.GameStateChangedEventArgs e)
        {
            if (e.new_state == Game.GameStates.Deadlock)
            {
                Draw();
                DrawTurnInfo();
            }
            else
            {
                LastMoveEffects.Clear();
                LastMoveIndex = -1;
            }
        }

        void game_GameBoardAdvanced(object sender, Events.GameBoardAdvancedEventArgs e)
        {
            DrawTurnInfo();
        }

        // Used to allow human interaction with the board.
        void tile_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GameTile t = (GameTile)sender;
            var i = GameTiles.IndexOf(t);

            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                Game.Move(i);
            }

#if DEBUG || CHEATS
            else if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {
                var xy = Game.GetXY(i);
                var new_value = 0;
                switch (Game.Board[i])
                {
                    case -1:
                        new_value = 0;
                        break;
                    case 0:
                        new_value = 1;
                        break;
                    case 1:
                        new_value = -1;
                        break;
                    default:
                        throw new Exceptions.LogicError();
                }

                Game.SetTile(xy.x, xy.y, new_value);
                Game.AdvanceBoard();
                Game.NextTurn();

            }
#endif
        }

        // Used to advance to  the next move if the AI is set to Pause.
        void window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && Game.ActivePlayerController is Controllers.AIMinimax)
            {
                Controllers.AIMinimax ai = Game.ActivePlayerController as Controllers.AIMinimax;
                Game.Move(ai.BestMoveIdx);
            }
        }

        // Used to clear out last move data.
        void game_PreviewMoveMade(object sender, Events.MoveBaseEventArgs e)
        {
            if (e.valid)
            {
                LastMoveEffects.Clear();
                LastMoveIndex = -1;
            }
        }

        // Used to record last move index, also flashes a tile red if an invalid move is attempted.
        void game_MoveMade(object sender, Events.MoveBaseEventArgs e)
        {
            if (e.valid)
            {
                LastMoveIndex = e.tile_index;
                //this.GameTiles[e.tile_index].HighlightMode = GameTile.HighlightModes.LastMove;
                //AnimateTileColor(e.tile_index, 255, 234, 0, 500);
            }
            else
            {
                AnimateTileColor(e.tile_index, 255, 166, 166, 500);
            }
        }

        void aiminimax_controller_PlayerCalculated(object sender, Events.PlayerCalculatedEventArgs e)
        {
            Waiting = false;
            Window.Overlay = false;
            Draw();
        }

        void aiminimax_controller_PlayerCalculating(object sender, Events.PlayerCalculatingEventArgs e)
        {
            Waiting = true;
            Window.Overlay = true;
            RedrawGrid();
        }

        // Used to update the visual game tile in reply to updates to the underlying game state,
        // also used to record the effect of the last move.
        void game_TileChanged(object sender, Events.TileChangedEventArgs e)
        {
            this.GameTiles[e.i].Owner = e.new_owner;
            //this.GameTiles[e.i].HighlightMode = GameTile.HighlightModes.LastMoveEffect;
            LastMoveEffects.Add(e.i);
            //AnimateTileColor(e.i, 232, 129, 12, 500);
        }

        void game_GameTurnAdvanced(object sender, Events.GameTurnAdvancedEventArgs e)
        {
            Draw();
        }

        #endregion

        /// <summary>
        /// Animates the tile at the given index from the given color to transparent.
        /// </summary>
        /// <param name="i">Tile Index</param>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="time">Time</param>
        void AnimateTileColor(int i, byte r, byte g, byte b, int time)
        {
            // Why this has to be so involved is beyond me.
            var tile = this.GameTiles[i];
            this.GameTiles[i].Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            var animation = new ColorAnimation();
            animation.From = Color.FromRgb(r, g, b);
            animation.To = Colors.Transparent;
            animation.Duration = new System.Windows.Duration(TimeSpan.FromMilliseconds(time));

            Storyboard s = new Storyboard();
            s.Duration = new System.Windows.Duration(TimeSpan.FromMilliseconds(time));
            s.Children.Add(animation);
            Storyboard.SetTarget(animation, this.GameTiles[i]);
            Storyboard.SetTargetProperty(animation, new System.Windows.PropertyPath("Background.Color"));
            s.Begin();
        }

    }
}

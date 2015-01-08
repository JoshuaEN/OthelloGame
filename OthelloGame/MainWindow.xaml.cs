﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OthelloGame.MoveSelectors.RandomErrors;

using OthelloGame;

namespace OthelloGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Game game = new Game(8);

#if RUN_TESTS || AI_DEBUG_MODE
            var pause = true;
            var depth = 6;
            var optimizing_depth = 2;
            var trim_moves = true;
            var trim_moves_to = 6;
            var thread = true;
            var use_MTDf = false;

            var p0 = new Controllers.AIMinimax(game, 0);
            p0.Tiebreak = new Tiebreaks.TileWeight();
            p0.Weighting = new Weighting.TieredWeightingCompressed_R2();
            p0.EndgameWeighting = new EndgameWeighting.DiskMaximizing();
            p0.MoveSelector = new MoveSelectors.Best();
            p0.Pause = pause;
            p0.Depth = depth;
            p0.UseThreading = thread;
            p0.MoveTrimming = false;
            p0.MoveTrimTo = 6;

            var p1 = new Controllers.AIMinimax(game, 1);
            p1.Tiebreak = new Tiebreaks.TileWeight();
            p1.Weighting = new Weighting.TieredWeightingCompressed_R2();
            p1.EndgameWeighting = new EndgameWeighting.DiskMaximizing();
            p1.MoveSelector = new MoveSelectors.Best();
            p1.Pause = pause;
            p1.Depth = depth;
            p1.UseThreading = thread;
            p1.MoveTrimming = false;
            p1.MoveTrimTo = 6;

            //p1.AlterWeighting = p0.Weighting;
           // p1.Weighting = new Weighting.StableDiskRatio();

            game.PlayerControllers[0] = p0;
            game.PlayerControllers[1] = p1;

#endif

#if DEBUG_STATS_ONLY || DEBUG
            lblDebugInfo.Visibility = System.Windows.Visibility.Visible;
            btnTreeview.Visibility = System.Windows.Visibility.Visible;

#endif

#if RUN_TESTS

            var tester = 0;
            var teste = game.OtherPlayer(0);

            var weighting_tests = new List<Weighting.WeightingBase>()
            {
                new Weighting.DiskDifference(),
                new Weighting.FrontierDiskRatio(),
                new Weighting.StableDiskRatio(),
                new Weighting.TieredWeighting(),
                new Weighting.TieredWeightingCompressed_R2()
            };

            (game.PlayerControllers[tester] as Controllers.AIMinimax).MoveSelector = new MoveSelectors.Best();
            (game.PlayerControllers[tester] as Controllers.AIMinimax).Weighting = new Weighting.TieredWeightingCompressed_R2();
            (game.PlayerControllers[teste] as Controllers.AIMinimax).AlterWeighting = null;

            AITest.AIWeightingGauntlet(game, 100, teste, weighting_tests);

            (game.PlayerControllers[tester] as Controllers.AIMinimax).MoveSelector = new MoveSelectors.Adaptive_V2_R1_EM();
            (game.PlayerControllers[teste] as Controllers.AIMinimax).AlterWeighting = new Weighting.TieredWeightingCompressed_R2();

            //AITest.AIPerformanceTest(false, -1, game, 401);
            AITest.AIWeightingGauntlet(game, 100, teste, weighting_tests);

            (game.PlayerControllers[tester] as Controllers.AIMinimax).MoveSelector = new MoveSelectors.Adaptive_V2_R2();
            AITest.AIWeightingGauntlet(game, 100, teste, weighting_tests);

            //AITest.AIPerformanceTest(false, -1, game, 1);


            //AITest.AISupertest(game, 500, new Weighting.TieredWeighting());

            //(game.PlayerControllers[1] as Controllers.AIMinimax).MoveSelector = new MoveSelectors.Best();
            //(game.PlayerControllers[1] as Controllers.AIMinimax).Weighting = new Weighting.TieredWeighting();
            //(game.PlayerControllers[0] as Controllers.AIMinimax).AlterWeighting = null;// new Weighting.TieredWeighting();

            //AITest.AIWeightingGauntlet(game, 100, 0, new List<Weighting.WeightingBase>()
            //{
            //    new Weighting.DiskDifference(),
            //    new Weighting.StableDiskRatio(),
            //    new Weighting.TieredWeighting(),
            //    new Weighting.TieredWeightingCompressed_R2(),
            //    new Weighting.TieredWeightingCompressedV2_R4()
            //});

            //return;

            //(game.PlayerControllers[0] as Controllers.AIMinimax).MoveSelector = new MoveSelectors.Adaptive_R21();

            //AITest.AIWeightingGauntlet(game, 100, 1, new List<Weighting.WeightingBase>()
            //{
            //    new Weighting.DiskDifference(),
            //    new Weighting.StableDiskRatio(),
            //    new Weighting.TieredWeighting(),
            //    new Weighting.TieredWeightingCompressed_R2(),
            //    new Weighting.TieredWeightingCompressedV2_R4()
            //});

            //AITest.AIPerformanceTest(false, -1, game, 100);

            //AITest.AIMoveSelectorGauntlet(game, 100, new MoveSelectors.Best(), new List<MoveSelectors.IMoveSelector>() {
            //    new RandomError_Ra_T1(),
            //    new RandomError_Ra_T2(),
            //    new RandomError_Ra_T3(),
            //    new RandomError_Ra_T5(),
            //    new RandomError_Ra_T10(),
            //    new RandomError_Re_T1(),
            //    new RandomError_Re_T2(),
            //    new RandomError_Re_T3(),
            //    new RandomError_Rm_T1(),
            //    new RandomError_Rm_T2(),
            //    new RandomError_Rm_T3(),
            //    new RandomError_Rl_T1(),
            //    new RandomError_Rl_T2(),
            //    new RandomError_Rl_T3(),
            //});
            this.Close();
            return;
#endif

            //this.Background = Brushes.Black;
            //GameGrid.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#8911BF00");

            //p0.Pause = false;
            var game_render = new GameRender(this, GameGrid, lblDebugInfo, game);
            Game = game;
            GameRender = game_render;
            game_render.DrawControllerData = true;


#if AI_DEBUG_MODE
            Game.Start();
            return;
#endif

            if (Globals.Settings.UseSavedControllers)
                StartWithSettings();
            else
                ShowControllerPicker();

            //game.Start();
            // p1.Depth = 13;
            //            game.Move(19);

            //return;

            //p0.Pause = false;
            //p1.Pause = false;

            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            //for (var i = 0; i < 101; i++)
            //{
            //    game.Start();
            //    game.Restart();
            //}
            //sw.Stop();

            //MessageBox.Show("Game Over, winner:" + game.Winner.ToString() + "; finished in " + sw.ElapsedMilliseconds.ToString() + "ms");

            //this.Close();
        }

        public Game Game { get; private set; }
        public GameRender GameRender { get; private set; }

        public Settings Settings { get { return Globals.Settings;  } }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            if (Game.ActivePlayerController is Controllers.AIMinimax)
            {
                MinimaxTreeView viewer = new MinimaxTreeView();
                viewer.view_tree(((Controllers.AIMinimax)Game.ActivePlayerController).LastMinimaxResult);
                viewer.Show();
            }
            else
            {
                MessageBox.Show("Not available for active player controller!");
            }
#else
            MessageBox.Show("Not available; compile with DEBUG defined to enable this feature.");
    
#endif

        }

        public bool Overlay
        {
            set
            {
                grdOverlay.Visibility = (value ? Visibility.Visible : Visibility.Collapsed);
                this.IsEnabled = !value;
            }
        }

        private void ShowControllerPicker()
        {
            grdSetup.Visibility = System.Windows.Visibility.Visible;
            grdMainGame.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void StartWithSettings()
        {
            Game.PlayerControllers[0] = psP1.GetController(Game, 0);
            Game.PlayerControllers[1] = psP2.GetController(Game, 1);

            if ((Game.PlayerControllers[0] as Controllers.AIMinimax).Pause == false && (Game.PlayerControllers[1] as Controllers.AIMinimax).Pause == false)
            {
                (Game.PlayerControllers[0] as Controllers.AIMinimax).Pause = true;
                (Game.PlayerControllers[1] as Controllers.AIMinimax).Pause = true;

                MessageBox.Show("Computer Vs. Computer: Press Enter or click a valid move to advance the turn.");
            }

            Game.Restart();

            grdSetup.Visibility = System.Windows.Visibility.Collapsed;
            grdMainGame.Visibility = System.Windows.Visibility.Visible;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StartWithSettings();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Globals.Settings.Save();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            ShowControllerPicker();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Game.Restart();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            ctmMain.PlacementTarget = btn;
            ctmMain.IsOpen = true;
        }
    }
}
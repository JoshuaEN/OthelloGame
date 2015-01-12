using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OthelloGame
{
    /// <summary>
    /// Class used to hold code for AI testing.
    /// </summary>
    public static class AITest
    {

        public static void AIWeightingGauntlet(Game game, int times_per_test, int swap_side, List<Weighting.IWeighting> testers)
        {
            foreach (var weigher in testers)
            {
                (game.PlayerControllers[swap_side] as Controllers.AIMinimax).Weighting = weigher;
                AIPerformanceTest(false, -1, game, times_per_test);
            }
        }
        

        public static void AIMoveSelectorGauntlet(Game game, int times_per_test, MoveSelectors.IMoveSelector teste, List<MoveSelectors.IMoveSelector> testers)
        {
            foreach (MoveSelectors.IMoveSelector selector in testers)
            {
                (game.PlayerControllers[0] as Controllers.AIMinimax).MoveSelector = teste;
                (game.PlayerControllers[1] as Controllers.AIMinimax).MoveSelector = selector;
                AIPerformanceTest(false, -1, game, times_per_test);

                (game.PlayerControllers[1] as Controllers.AIMinimax).MoveSelector = teste;
                (game.PlayerControllers[0] as Controllers.AIMinimax).MoveSelector = selector;
                AIPerformanceTest(false, -1, game, times_per_test);
            }
        }

        public static void AIPerformanceTest(bool verification_run, int verification_set, Game game, int times)
        {
            // Need to pause to record results after each move.
            SetControllerPause(game, true);

            while (times > 0)
            {
                var result = new MatchResult();
                var move = 0;
                game.Restart();
                while (game.Finished == false)
                {
                    var active_controller = game.ActivePlayerController;

                    if (active_controller is Controllers.AIMinimax)
                    {
                        var active_ai_controller = (active_controller as Controllers.AIMinimax);

                        var pref = new MatchResult.MovePerformance();
                        pref.verification_run = verification_run;
                        pref.verification_set = verification_set;
                        pref.player = game.ActivePlayer;
                        pref.move = move;
                        move += 1;

#if DEBUG || DEBUG_STATS_ONLY
                        pref.time = active_ai_controller.LastResultTime;
                        pref.valid_moves = active_ai_controller.RealEvaluatedMoves;
#endif

                        result.performance.Add(pref);

                        game.Move(active_ai_controller.BestMoveIdx);
                    }
                    else
                    {
                        throw new ArgumentException("All controllers must be AI Controllers");
                    }
                }

                result.p0_controller = game.PlayerControllers[0].UniqueIdentString();
                result.p1_controller = game.PlayerControllers[1].UniqueIdentString();

                var count = game.GetCounts();
                result.p0_disks = count[0];
                result.p1_disks = count[1];
                result.board_size = game.BoardSize;
                result.final_board_state = game.BoardString();
                result.winner = game.Winner;
                AITest.SendMatchResult(result);

                times -= 1;
            }
        }

        /// <summary>
        /// Helper to ensure all player controllers, if AI, are set to pause.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pause"></param>
        private static void SetControllerPause(Game game, bool pause)
        {
            for (var i = 0; i < game.PlayerControllers.Length; i++)
            {
                var controller = game.PlayerControllers[i];
                if (controller is Controllers.AIMinimax)
                    (controller as Controllers.AIMinimax).Pause = pause;
            }
        }

        /// <summary>
        /// Sends the results of a match to the local server so that it can be saved.
        /// </summary>
        /// <param name="match_result">Match result to send.</param>
        public static void SendMatchResult(MatchResult match_result)
        {
            byte[] postBytes = new ASCIIEncoding().GetBytes(match_result.ToQueryString());

            var request = System.Net.HttpWebRequest.Create("http://localhost:3000/record_result");

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(postBytes, 0, postBytes.Length);
                stream.Flush();
            }
        }

        /// <summary>
        /// Match result to be sent.
        /// </summary>
        public class MatchResult
        {
            public MatchResult()
                : base()
            {
                performance = new List<MovePerformance>();
            }

            public string p0_controller { get; set; }
            public string p1_controller { get; set; }
            public int p0_disks { get; set; }
            public int p1_disks { get; set; }
            public int winner { get; set; }
            public string final_board_state { get; set; }
            public int board_size { get; set; }
            public List<MovePerformance> performance { get; set; }

            public class MovePerformance
            {
                public int move { get; set; }
                public long time { get; set; }
                public int player { get; set; }
                public bool verification_run { get; set; }
                public int verification_set { get; set; }
                public int valid_moves { get; set; }

                public String ToQueryString()
                {
                    NameValueCollection queryString = HttpUtility.ParseQueryString(String.Empty);
                    queryString.Add("move", move.ToString());
                    queryString.Add("time", time.ToString());
                    queryString.Add("player", player.ToString());
                    queryString.Add("verification_run", verification_run.ToString());
                    queryString.Add("verification_set", verification_set.ToString());
                    queryString.Add("valid_moves", valid_moves.ToString());

                    return queryString.ToString();
                }
            }

            public String ToQueryString()
            {
                NameValueCollection queryString = HttpUtility.ParseQueryString(String.Empty);
                queryString.Add("p0_controller", p0_controller);
                queryString.Add("p1_controller", p1_controller);
                queryString.Add("p0_disks", p0_disks.ToString());
                queryString.Add("p1_disks", p1_disks.ToString());
                queryString.Add("winner", winner.ToString());
                queryString.Add("final_board_state", final_board_state);
                queryString.Add("board_size", board_size.ToString());

                foreach (var item in performance)
                {
                    queryString.Add("performance[]", item.ToQueryString());
                }

                return queryString.ToString();
            }
        }
    }
}

using System.Collections.Generic;

namespace MainGame
{
    public class ConstantsList
    {
        public static Dictionary<string, string> Scenes = new Dictionary<string, string>
        {
            { "OpeningScene", "MainMenu" },
            { "GameScene", "BallBattle" },
            { "ARScene", "ARScene" }
        };

        public static readonly int MAXMATCHES = 5;
        public static readonly float MATCHDURATION = 140;
        public static readonly float BOARDWIDTH = 14;
        public static readonly float BOARDHEIGHT = 24;
        public static readonly int ATTACKERENERGYCOST = 2;
        public static readonly int DEFENDERENERGYCOST = 3;
    }
}

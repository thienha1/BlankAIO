using EnsoulSharp.SDK.MenuUI.Values;
namespace BlankAIO
{
    class Menubase
    {
        public class Pyke_Combat
        {
            public static readonly MenuSlider QhitC = new MenuSlider("qhit", "^ Q Hitchance = 1-Low ~ 4-Very High", 3, 1, 4);
            public static readonly MenuBool QC = new MenuBool("q", "Use [Q]");
            public static readonly MenuBool EC = new MenuBool("e", "Use [E]");
            public static readonly MenuBool EtowerC = new MenuBool("Etower", "^ Don't use if enemy is under turret");
            public static readonly MenuBool RC = new MenuBool("r", "Use [R]");
            public static readonly MenuBool RkillC = new MenuBool("rkill", "^Only if Enemy is Killable");
        }
        public class Pyke_Clear
        {
            public static readonly MenuBool Ec = new MenuBool("ec", "Use [Q]");
        }
        public class Pyke_Harass
        {
            public static readonly MenuSlider Qhit = new MenuSlider("qhit", "^ Q Hitchance = 1-Low ~ 4-Very High", 3, 1, 4);
            public static readonly MenuBool Q = new MenuBool("q", "Use [Q]");
            public static readonly MenuBool E = new MenuBool("e", "Use [E]");
            public static readonly MenuBool Etower = new MenuBool("Etower", "^ Don't use if enemy is under turret");
        }
        public class Pyke_KS
        {
            public static readonly MenuBool R = new MenuBool("rks", "Use [R]");
        }
        public class Pyke_misc
        {
            public static readonly MenuBool draw = new MenuBool("draw", "Draw Q Min/Max Range");
        }
    }
}

using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
using SPrediction;
using System;
using SharpDX;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp.SDK.Events;

namespace BlankAIO
{
    class Pyke
    {
        private static Spell Q, W, E, R;
        private static AIHeroClient player = ObjectManager.Player;

        public static void On_Load()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            Q.SetCharged("PykeQ", "PykeQ", 400, 1100, 1.0f);
            Q.SetSkillshot(0.25f, 120f, 1700, true, false, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 550);
            E.SetSkillshot(0.275f, 70f, 500f, false, false, SkillshotType.Line);
            R = new Spell(SpellSlot.R, 750);
            R.SetSkillshot(0.25f, 100f, float.MaxValue, false, false, SkillshotType.Circle);


            CreateMenu();
            Tick.OnTick += OnTick;
            Drawing.OnDraw += OnDraw;
        }

        private static void CreateMenu()
        {
            var geral = new Menu("menu.base", "thienha1.Pyke", true);

            var Combat = new Menu("Pyke_Combat", "Combo Settings");
            Combat.Add(Menubase.Pyke_Combat.QhitC);
            Combat.Add(Menubase.Pyke_Combat.QC);
            Combat.Add(Menubase.Pyke_Combat.EC);
            Combat.Add(Menubase.Pyke_Combat.EtowerC);
            Combat.Add(Menubase.Pyke_Combat.ECCC);
            Combat.Add(Menubase.Pyke_Combat.RC);
            Combat.Add(Menubase.Pyke_Combat.RkillC);

            var harass = new Menu("harass", "Harass Settings");
            harass.Add(Menubase.Pyke_Harass.Qhit);
            harass.Add(Menubase.Pyke_Harass.Q);
            harass.Add(Menubase.Pyke_Harass.E);
            harass.Add(Menubase.Pyke_Harass.Etower);

            var Clear = new Menu("Clear", "Clear Settings");
            Clear.Add(Menubase.Pyke_Clear.Ec);

            var ks = new Menu("killsteal", "KillSteal Settings");
            ks.Add(Menubase.Pyke_KS.R);

            var misc = new Menu("misc", "Misc Settings");
            misc.Add(Menubase.Pyke_misc.draw);
            misc.Add(Menubase.Pyke_misc.escEW);

            var pred = new Menu("spred", "Spred");

            geral.Add(Combat);
            geral.Add(harass);
            geral.Add(Clear);
            geral.Add(ks);
            geral.Add(misc);
            geral.Add(pred);
            Prediction.Initialize(pred);
            geral.Attach();

        }

        private static void OnTick(EventArgs args)
        {
            if (Menubase.Pyke_KS.R.Enabled)
                KS();
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    EAlies();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    break;
            }
            if (Menubase.Pyke_misc.escEW.Active)
            {
                player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                if (E.IsReady() && W.IsReady())
                {
                    E.Cast(Game.CursorPos);
                    W.Cast(Game.CursorPos);
                }
            }
        }


        private static void KS()
        {
            var al = GameObjects.EnemyHeroes.Where(x => !x.IsDead && x.IsEnemy && !x.IsInvulnerable && x.Health < R.GetDamage(x, DamageStage.Empowered) && x.DistanceToPlayer() < R.Range);
            var t = al.FirstOrDefault(x => x.IsValidTarget(R.Range));
            if (t == null) return;
            if (t != null && !ObjectManager.Player.IsRecalling())
            {
                if (Orbwalker.ActiveMode != OrbwalkerMode.Combo && !t.IsDead && !t.IsEnemy && t.IsVisible && t.IsHPBarRendered)
                {
                    R.SPredictionCast(t, HitChance.Medium);
                }
            }
        }

        private static void Harass()
        {
            var qvalue = Menubase.Pyke_Harass.Qhit.Value;
            var qhit = HitChance.High;
            switch (qvalue)
            {
                case 1:
                    qhit = HitChance.Low;
                    break;
                case 2:
                    qhit = HitChance.Medium;
                    break;
                case 3:
                    qhit = HitChance.High;
                    break;
                case 4:
                    qhit = HitChance.VeryHigh;
                    break;
            }
            if (Menubase.Pyke_Harass.Q.Enabled)
            {
                var target = TargetSelector.GetTarget(Q.Range);
                if (target == null) return;
                if (target != null && target.IsValidTarget(Q.Range))
                {
                    if (target.DistanceToPlayer() > 400)
                    {
                        if (Q.IsReady() && !Q.IsCharging)
                        {
                            Q.StartCharging();
                        }
                        if (Q.IsCharging || target.DistanceToPlayer() < 400)
                        {
                            var predi = Q.GetSPrediction(target);
                            if (predi.HitChance >= qhit)
                            {
                                Q.SPredictionCast(target, qhit);
                            }
                        }
                    }
                    else if (Q.IsReady() && Q.IsCharging || target.InAutoAttackRange())
                    {
                        Q.Cast(target.Position);
                    }
                }
            }
            if (!Q.IsReady() || !Q.IsCharging && E.IsReady() && Menubase.Pyke_Harass.E.Enabled)
            {
                var target = TargetSelector.GetTarget(E.Range);
                if (target == null) return;
                if (target != null && target.IsValidTarget(E.Range))
                {
                    if (target.IsUnderEnemyTurret() && Menubase.Pyke_Harass.Etower.Enabled) return;
                    if (player.Position.Extend(target.Position, Vector3.Distance(player.Position, target.Position)).IsUnderEnemyTurret() && Menubase.Pyke_Harass.Etower.Enabled) return;
                    var pred = E.GetSPrediction(target);
                    if (pred.HitChance >= qhit)
                    {
                        E.SPredictionCast(target, qhit);
                    }
                }
            }
        }

        private static void Combo()
        {
            var qvalue = Menubase.Pyke_Combat.QhitC.Value;
            var qhit = HitChance.High;
            switch (qvalue)
            {
                case 1:
                    qhit = HitChance.Low;
                    break;
                case 2:
                    qhit = HitChance.Medium;
                    break;
                case 3:
                    qhit = HitChance.High;
                    break;
                case 4:
                    qhit = HitChance.VeryHigh;
                    break;
            }
            if (Menubase.Pyke_Combat.QC.Enabled)
            {
                var target = TargetSelector.GetTarget(Q.Range);
                if (target == null) return;
                if (target != null && target.IsValidTarget(Q.Range))
                {
                    if (target.DistanceToPlayer() > 400)
                    {
                        if (Q.IsReady() && !Q.IsCharging || E.IsReady() && Menubase.Pyke_Combat.EC.Enabled)
                        {
                            Q.StartCharging();
                        }
                        if (Q.IsCharging || target.DistanceToPlayer() < 400)
                        {
                            var predi = Q.GetSPrediction(target);
                            if (predi.HitChance >= qhit)
                            {
                                Q.SPredictionCast(target, qhit);
                            }
                        }
                    }
                    else if (Q.IsReady() && Q.IsCharging || target.InAutoAttackRange())
                    {
                        Q.Cast(target.Position);
                    }
                }
            }
            if (!Q.IsReady() || !Q.IsCharging && E.IsReady() && Menubase.Pyke_Combat.EC.Enabled)
            {
                var target = TargetSelector.GetTarget(E.Range);
                if (target == null) return;
                if (target != null && target.IsValidTarget(E.Range))
                {
                    if (target.IsUnderEnemyTurret() && Menubase.Pyke_Combat.EtowerC.Enabled) return;
                    if (player.Position.Extend(target.Position, Vector3.Distance(player.Position, target.Position)).IsUnderEnemyTurret() && Menubase.Pyke_Combat.EtowerC.Enabled) return;
                    var pred = E.GetSPrediction(target);
                    if (pred.HitChance >= qhit)
                    {
                        E.SPredictionCast(target, qhit);
                    }
                }
            }
            if (R.IsReady() && Menubase.Pyke_Combat.RC.Enabled)
            {
                var rt = TargetSelector.GetTarget(R.Range, DamageType.True);
                if (rt == null) return;
                if (rt != null && rt.IsValidTarget(R.Range))
                {
                    if (Menubase.Pyke_Combat.RkillC.Enabled && rt.Health > R.GetDamage(rt, DamageStage.Empowered))
                    {
                        return;
                    }
                    if (!rt.IsDead && !rt.IsEnemy && rt.IsVisible && rt.IsHPBarRendered)
                    {
                        R.SPredictionCast(rt, HitChance.Medium);
                    }
                }
            }
        }
        private static void EAlies()
        {
            var swvalue = Menubase.Pyke_Combat.ECCC.Value;
            switch (swvalue)
            {
                case 0:
                    return;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    var target = TargetSelector.GetTarget(E.Range);
                    if (player.CountAllyHeroesInRange(Q.Range) > 1)
                    {
                        if (!Q.IsReady() || !Q.IsCharging && E.IsReady() && Menubase.Pyke_Combat.EC.Enabled)
                        {
                            if (target == null) return;
                            if (target != null && target.IsValidTarget(E.Range))
                            {
                                if (target.IsUnderEnemyTurret() && Menubase.Pyke_Combat.EtowerC.Enabled) return;
                                if (player.Position.Extend(target.Position, Vector3.Distance(player.Position, target.Position)).IsUnderEnemyTurret() && Menubase.Pyke_Combat.EtowerC.Enabled) return;
                                if (target.CountEnemyHeroesInRange(E.Range) >= Menubase.Pyke_Combat.ECCC.Value)
                                {
                                    E.CastIfWillHit(target, Menubase.Pyke_Combat.ECCC.Value);
                                }
                            }
                        }
                    }
                    break;
            }
            
        }
        internal static bool CanR(AIBaseClient tarR)
        {
            if (tarR.HasBuffOfType(BuffType.Invulnerability)
                                && tarR.HasBuffOfType(BuffType.SpellShield)
                                && tarR.HasBuff("kindredrnodeathbuff") //Kindred Ult
                                && tarR.HasBuff("BlitzcrankManaBarrierCD") //Blitz Passive
                                && tarR.HasBuff("ManaBarrier") //Blitz Passive
                                && tarR.HasBuff("FioraW") //Fiora W
                                && tarR.HasBuff("JudicatorIntervention") //Kayle R
                                && tarR.HasBuff("UndyingRage") //Trynd R
                                && tarR.HasBuff("BardRStasis") //Bard R
                                && tarR.HasBuff("ChronoShift") //Zilean R
                                )
            {
                return false;
            }
            if (tarR == null)
            {
                return false;
            }
            return true;
        }
        private static void Clear()
        {

            if (Menubase.Pyke_Clear.Ec && Q.IsReady())
            {
                var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range) && x.IsMinion())
                            .Cast<AIBaseClient>().ToList();

                if (minions.Any())
                {
                    var eFarmLocation = Q.GetLineFarmLocation(minions);
                    if (eFarmLocation.Position.IsValid())
                    {
                        Q.Cast(eFarmLocation.Position);
                    }
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling() || MenuGUI.IsChatOpen || ObjectManager.Player.IsWindingUp)
            {
                return;
            }
            if (!Menubase.Pyke_misc.draw.Enabled)
                return;
            if (Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.LightGreen);
            }
        }
    }
}
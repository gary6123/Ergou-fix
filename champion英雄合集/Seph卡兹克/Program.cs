using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace SephKhazix
{
    internal class Program
    {
        private const string ChampionName = "Khazix";
        private static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R, QE, WE, EE, RE, Wpred, WEpred, Wfarm, Epred, EEpred;

        private static float Wangle = 28*(float) Math.PI/180;

        private static Menu Config;
        private static Items.Item HDR;
        private static Items.Item TIA;
        private static Items.Item BKR;
        private static Items.Item BWC;
        private static Items.Item YOU;
        private static SpellSlot IgniteSlot;



        private static Obj_AI_Hero Player;
        private static bool Qnorm, Qevolved, Wnorm, Wevolved, Enorm, Eevolved, Rnorm, Revolved;



        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;


            Wpred = new Spell(SpellSlot.W, 1025);
            WEpred = new Spell(SpellSlot.W, 1025);
            Wfarm = new Spell(SpellSlot.W, 1025);

            Epred = new Spell(SpellSlot.E, 600);
            EEpred = new Spell(SpellSlot.E, 900);


            Wpred.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);
            WEpred.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);


            Epred.SetSkillshot(0.25f, 100f, 1000f, false, SkillshotType.SkillshotCircle);
            EEpred.SetSkillshot(0.25f, 100f, 1000f, false, SkillshotType.SkillshotCircle);
         

            Q = new Spell(SpellSlot.Q, 325f);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 600f);
            QE = new Spell(SpellSlot.Q, 375f);
            WE = new Spell(SpellSlot.W, 1000);
            EE = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 0);
            RE = new Spell(SpellSlot.R, 0);
            // W.SetSkillshot(0.25f, 100f, 1000, true, SkillshotType.SkillshotLine);
            //  W.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.225f, 80f, 828.5f, true, SkillshotType.SkillshotLine);

            WE.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);
            //  E.SetSkillshot(.250f, 100, 1000, true, SkillshotType.SkillshotCircle);
            //   EE.SetSkillshot(.250f, 100, 1000, true, SkillshotType.SkillshotCircle);


            HDR = new Items.Item(3074, 225f);
            TIA = new Items.Item(3077, 225f);
            BKR = new Items.Item(3153, 450f);
            BWC = new Items.Item(3144, 450f);
            YOU = new Items.Item(3142, 185f);


            IgniteSlot = Player.GetSpellSlot("summonerdot");


            Config = new Menu("Seph卡兹克", "Khazix", true);


            //TargetSelector
            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //config
            Config.AddSubMenu(new Menu("自动 骚扰/Poke", "autopoke"));
            Config.SubMenu("autopoke").AddItem(new MenuItem("AutoHarrass", "自动骚扰")).SetValue(true);
            Config.SubMenu("autopoke").AddItem(new MenuItem("AutoWI", "自动-W 不动的敌人")).SetValue(true);
            Config.SubMenu("autopoke").AddItem(new MenuItem("AutoWD", "自动 W")).SetValue(true);


            //
            Config.AddSubMenu(new Menu("封包 设置", "Packets"));
            Config.SubMenu("Packets").AddItem(new MenuItem("usePackets", "启用 封包").SetValue(true));

            //Combo
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseEGapclose", "使用 E 突进然后使用 Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseEGapcloseW", "使用 E 突进然后使用 W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRGapcloseW", "使用 R 突进时")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "使用 物品")).SetValue(true);
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));


            //Harass
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "使用 W")).SetValue(true);
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("X".ToCharArray()[0],
                        KeyBindType.Press)));

            //Farm
            Config.AddSubMenu(new Menu("清野/清线", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "使用 Q")).SetValue(true);
            Config.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "使用 E")).SetValue(false);
            Config.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "使用 W")).SetValue(true);
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("ActiveFarm", "清线键位").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("JungleFarm", "清野键位").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));


            //Kill Steal
            Config.AddSubMenu(new Menu("抢人头", "Ks"));
            Config.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "启用 抢人头")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "使用 Q")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseWKs", "使用 W")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseEKs", "使用 E")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseEQKs", "使用 EQ 抢人头")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseEWKs", "使用 EW 抢人头")).SetValue(false);

            Config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "使用 点燃")).SetValue(true);


            //Drawings
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "范围 Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "范围 W")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "范围 E")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "延迟自由圈").SetValue(true));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "圈 质量").SetValue(new Slider(100, 100, 10)));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "圈 厚度").SetValue(new Slider(1, 10, 1)));

            //Debug
            Config.AddSubMenu(new Menu("调试", "Debug"));
            Config.SubMenu("Debug").AddItem(new MenuItem("Debugon", "启用 调试").SetValue(true));
      
            Config.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Game.PrintChat("<font color='#1d87f2'>Seph|鍗″吂鍏媩 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847!.</font>");
        }


        private static void CastW(Obj_AI_Base unit, Vector2 unitPosition, int minTargets = 0)
        {
            var usePacket = Config.Item("usePackets").GetValue<bool>();
            var points = new List<Vector2>();
            var hitBoxes = new List<int>();

            Vector2 startPoint = ObjectManager.Player.ServerPosition.To2D();
            Vector2 originalDirection = Wpred.Range*(unitPosition - startPoint).Normalized();

            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget() && enemy.NetworkId != unit.NetworkId)
                {
                    PredictionOutput pos = Wpred.GetPrediction(enemy);
                    if (pos.Hitchance >= HitChance.Medium)
                    {
                        points.Add(pos.UnitPosition.To2D());
                        hitBoxes.Add((int) enemy.BoundingRadius);
                    }
                }
            }


            var posiblePositions = new List<Vector2>();

            for (int i = 0; i < 3; i++)
            {
                if (i == 0) posiblePositions.Add(unitPosition + originalDirection.Rotated(0));
                if (i == 1) posiblePositions.Add(startPoint + originalDirection.Rotated(Wangle));
                if (i == 2) posiblePositions.Add(startPoint + originalDirection.Rotated(-Wangle));
            }


            if (startPoint.Distance(unitPosition) < 900)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = posiblePositions[i];
                    Vector2 direction = (pos - startPoint).Normalized().Perpendicular();
                    float k = (2/3*(unit.BoundingRadius + Q.Width));
                    posiblePositions.Add(startPoint - k*direction);
                    posiblePositions.Add(startPoint + k*direction);
                }
            }

            var bestPosition = new Vector2();
            int bestHit = -1;

            foreach (Vector2 position in posiblePositions)
            {
                int hits = CountHits(position, points, hitBoxes);
                if (hits > bestHit)
                {
                    bestPosition = position;
                    bestHit = hits;
                }
            }

            if (bestHit + 1 <= minTargets)
                return;

            Wpred.Cast(bestPosition.To3D(), usePacket);
        }

        private static void CastWE(Obj_AI_Base unit, Vector2 unitPosition, int minTargets = 0)
        {
            var usePacket = Config.Item("usePackets").GetValue<bool>();
            var points = new List<Vector2>();
            var hitBoxes = new List<int>();

            Vector2 startPoint = ObjectManager.Player.ServerPosition.To2D();
            Vector2 originalDirection = WEpred.Range*(unitPosition - startPoint).Normalized();

            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget() && enemy.NetworkId != unit.NetworkId)
                {
                    PredictionOutput pos = WEpred.GetPrediction(enemy);
                    if (pos.Hitchance >= HitChance.Medium)
                    {
                        points.Add(pos.UnitPosition.To2D());
                        hitBoxes.Add((int) enemy.BoundingRadius);
                    }
                }
            }


            var posiblePositions = new List<Vector2>();

            for (int i = 0; i < 3; i++)
            {
                if (i == 0) posiblePositions.Add(unitPosition + originalDirection.Rotated(0));
                if (i == 1) posiblePositions.Add(startPoint + originalDirection.Rotated(Wangle));
                if (i == 2) posiblePositions.Add(startPoint + originalDirection.Rotated(-Wangle));
            }


            if (startPoint.Distance(unitPosition) < 900)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = posiblePositions[i];
                    Vector2 direction = (pos - startPoint).Normalized().Perpendicular();
                    float k = (2/3*(unit.BoundingRadius + Q.Width));
                    posiblePositions.Add(startPoint - k*direction);
                    posiblePositions.Add(startPoint + k*direction);
                }
            }

            var bestPosition = new Vector2();
            int bestHit = -1;

            foreach (Vector2 position in posiblePositions)
            {
                int hits = CountHits(position, points, hitBoxes);
                if (hits > bestHit)
                {
                    bestPosition = position;
                    bestHit = hits;
                }
            }

            if (bestHit + 1 <= minTargets)
                return;

            WEpred.Cast(bestPosition.To3D(), usePacket);
        }


        private static int CountHits(Vector2 position, List<Vector2> points, List<int> hitBoxes)
        {
            int result = 0;

            Vector2 startPoint = ObjectManager.Player.ServerPosition.To2D();
            Vector2 originalDirection = Q.Range*(position - startPoint).Normalized();
            Vector2 originalEndPoint = startPoint + originalDirection;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];

                for (int k = 0; k < 3; k++)
                {
                    var endPoint = new Vector2();
                    if (k == 0) endPoint = originalEndPoint;
                    if (k == 1) endPoint = startPoint + originalDirection.Rotated(Wangle);
                    if (k == 2) endPoint = startPoint + originalDirection.Rotated(-Wangle);

                    if (point.Distance(startPoint, endPoint, true, true) <
                        (W.Width + hitBoxes[i])*(W.Width + hitBoxes[i]))
                    {
                        result++;
                        break;
                    }
                }
            }

            return result;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            Player = ObjectManager.Player;
            //W = new Spell(SpellSlot.E, EE.Range);
            Orbwalker.SetAttack(true);

            CheckSpells();
            // var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            // foreach (var buff in eTarget.Buffs) { Game.PrintChat(buff.DisplayName); }
            // foreach (var buff in Player.Buffs) { Game.PrintChat(buff.DisplayName); }

            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Config.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                Harass();
            }
            if (Config.Item("AutoHarrass").GetValue<bool>())
            {
                AutoHarrass();
            }

            if (Config.Item("ActiveFarm").GetValue<KeyBind>().Active)
            {
                OnWaveClear();
            }
            if (Config.Item("JungleFarm").GetValue<KeyBind>().Active)
            {
                JungleFarm();
            }
            if (Config.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }
        }

        private static void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target != null)
            {
                var usePacket = Config.Item("usePackets").GetValue<bool>();
                if (Player.Distance(target) <= Q.Range && Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady() &&
                    Qnorm)
                {
                    Q.Cast(target, usePacket);
                }
                if (Player.Distance(target) <= Q.Range && Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady() &&
                    Qevolved)
                {
                    QE.Cast(target, usePacket);
                }

                if (Player.Distance(target) <= W.Range && Config.Item("UseWHarass").GetValue<bool>() && W.IsReady() &&
                    Wnorm)
                {
                    W.Cast(target, usePacket);
                }
                if (Player.Distance(target) <= W.Range && Config.Item("UseWHarass").GetValue<bool>() && W.IsReady() &&
                    Wevolved)
                {
                    WE.Cast(target, usePacket);
                }
            }
        }


        private static void JungleFarm()
        {
            var pos = new List<Vector2>();
            Obj_AI_Hero target = TargetSelector.GetTarget(QE.Range, TargetSelector.DamageType.Physical);
            List<Obj_AI_Base> mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.Health);
            foreach (Obj_AI_Base minion in mobs)
            {
                if (minion != null)
                {
                    pos.Add(minion.Position.To2D());
                }
                // Orbwalker.SetAttacks(!(Q.IsReady() || W.IsReady() || E.IsReady()) || TIA.IsReady() || HDR.IsReady());
                // Normal Farms
                if (Q.IsReady() && minion.IsValidTarget() && Qnorm && Player.Distance(minion) <= Q.Range &&
                    Config.Item("UseQFarm").GetValue<bool>())
                {
                    Q.Cast(minion);
                }
                if (W.IsReady() && minion.IsValidTarget() && Wnorm && Player.Distance(minion) <= W.Range &&
                    Config.Item("UseWFarm").GetValue<bool>() && (pos.Any()))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestLineFarmLocation(pos, 70, 1025);

                    Wpred.Cast(pred.Position);
                }
                if (E.IsReady() && minion.IsValidTarget() && Enorm && Player.Distance(minion) <= E.Range &&
                    Config.Item("UseEFarm").GetValue<bool>() && (pos.Any()))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestCircularFarmLocation(pos, 300, 600);

                    Epred.Cast(pred.Position);
                }
                //Evolved
                if (Q.IsReady() && minion.IsValidTarget() && Qevolved && Player.Distance(minion) <= QE.Range &&
                    Config.Item("UseQFarm").GetValue<bool>())
                {
                    QE.Cast(minion);
                }
                if (W.IsReady() && minion.IsValidTarget() && Wevolved && Player.Distance(minion) <= WE.Range &&
                    Config.Item("UseWFarm").GetValue<bool>() && (pos.Any()))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestLineFarmLocation(pos, 70, 1025);

                    WEpred.Cast(pred.Position);
                }
                if (E.IsReady() && minion.IsValidTarget() && Eevolved && Player.Distance(minion) <= EE.Range &&
                    Config.Item("UseEFarm").GetValue<bool>() && (pos.Any()))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestCircularFarmLocation(pos, 300, 600);

                    EEpred.Cast(pred.Position);
                }
                if (Config.Item("UseItems").GetValue<bool>())
                {
                    if (HDR.IsReady() && Player.Distance(minion) <= HDR.Range)
                    {
                        HDR.Cast();
                        // Items.UseItem(3077, ObjectManager.Player);
                    }
                    if (TIA.IsReady() && Player.Distance(minion) <= TIA.Range)
                    {
                        TIA.Cast();
                    }
                }
            }
        }


        private static void OnWaveClear()
        {
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                if (Qnorm && !Qevolved)
                {
                    foreach (
                        Obj_AI_Base minion in
                            allMinions.Where(
                                minion =>
                                    minion.IsValidTarget() &&
                                    HealthPrediction.GetHealthPrediction(minion,
                                        (int) (ObjectManager.Player.Distance(minion)*1000/1400))
                                    < 0.75*Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {
                        if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                            Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) &&
                            Player.Distance(minion) <= Q.Range)
                        {
                            Q.CastOnUnit(minion, false);
                            return;
                        }
                    }
                }
                if (Qevolved && !Qnorm)
                {
                    foreach (
                        Obj_AI_Base minion in
                            allMinions.Where(
                                minion =>
                                    minion.IsValidTarget() &&
                                    HealthPrediction.GetHealthPrediction(minion,
                                        (int) (ObjectManager.Player.Distance(minion)*1000/1400))
                                    < 0.75*Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {
                        if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                            Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) &&
                            Player.Distance(minion) <= QE.Range)
                        {
                            QE.CastOnUnit(minion, false);
                            return;
                        }
                    }
                }
            }
            if (Config.Item("UseWFarm").GetValue<bool>() && W.IsReady())
            {
                if (Wnorm && !Wevolved)
                {
                    MinionManager.FarmLocation farmLocation =
                        MinionManager.GetBestCircularFarmLocation(
                            MinionManager.GetMinions(Player.Position, Wpred.Range)
                                .Select(minion => minion.ServerPosition.To2D())
                                .ToList(), Wpred.Width, Wpred.Range);

                    if (Player.Distance(farmLocation.Position) <= W.Range)
                        Wpred.Cast(farmLocation.Position);
                }
                if (Wevolved && !Wnorm)
                {
                    MinionManager.FarmLocation farmLocation =
                        MinionManager.GetBestCircularFarmLocation(
                            MinionManager.GetMinions(Player.Position, WEpred.Range)
                                .Select(minion => minion.ServerPosition.To2D())
                                .ToList(), WEpred.Width, WEpred.Range);

                    if (Player.Distance(farmLocation.Position) <= WE.Range)
                        WEpred.Cast(farmLocation.Position);
                }
            }

            if (Config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                if (Enorm && !Eevolved)
                {
                    MinionManager.FarmLocation farmLocation =
                        MinionManager.GetBestCircularFarmLocation(
                            MinionManager.GetMinions(Player.Position, Epred.Range)
                                .Select(minion => minion.ServerPosition.To2D())
                                .ToList(), Epred.Width, Epred.Range);

                    if (Player.Distance(farmLocation.Position) <= W.Range)
                        Epred.Cast(farmLocation.Position);
                }
                if (Eevolved && !Enorm)
                {
                    MinionManager.FarmLocation farmLocation =
                        MinionManager.GetBestCircularFarmLocation(
                            MinionManager.GetMinions(Player.Position, EEpred.Range)
                                .Select(minion => minion.ServerPosition.To2D())
                                .ToList(), EEpred.Width, EEpred.Range);

                    if (Player.Distance(farmLocation.Position) <= WE.Range)
                        EEpred.Cast(farmLocation.Position);
                }
            }
            if (Config.Item("UseItems").GetValue<bool>())
            {
                MinionManager.FarmLocation farmLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(Player.Position, HDR.Range)
                            .Select(minion => minion.ServerPosition.To2D())
                            .ToList(), HDR.Range, HDR.Range);

                if (HDR.IsReady() && Player.Distance(farmLocation.Position) <= HDR.Range && farmLocation.MinionsHit >= 2)
                {
                    //  HDR.Cast(farmLocation.Position);
                    // HDR.Cast();
                    Items.UseItem(3074, ObjectManager.Player);
                }
                if (TIA.IsReady() && Player.Distance(farmLocation.Position) <= TIA.Range && farmLocation.MinionsHit >= 2)
                {
                    //TIA.Cast(farmLocation.Position);
                    // TIA.Cast();
                    Items.UseItem(3077, ObjectManager.Player);
                }
            }
        }


        private static void OnJungleClear()
        {
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.Health);
            if (Config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                if (Qnorm && !Qevolved)
                {
                    foreach (
                        Obj_AI_Base minion in
                            allMinions.Where(
                                minion =>
                                    minion.IsValidTarget() &&
                                    HealthPrediction.GetHealthPrediction(minion,
                                        (int) (ObjectManager.Player.Distance(minion)*1000/1400))
                                    < 0.75*Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {
                        if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                            Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) &&
                            Player.Distance(minion) <= Q.Range)
                        {
                            Q.CastOnUnit(minion, false);
                            return;
                        }
                    }
                }
                if (Qevolved && !Qnorm)
                {
                    foreach (
                        Obj_AI_Base minion in
                            allMinions.Where(
                                minion =>
                                    minion.IsValidTarget() &&
                                    HealthPrediction.GetHealthPrediction(minion,
                                        (int) (ObjectManager.Player.Distance(minion)*1000/1400))
                                    < 0.75*Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {
                        if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                            Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) &&
                            Player.Distance(minion) <= QE.Range)
                        {
                            QE.CastOnUnit(minion, false);
                            return;
                        }
                    }
                }
            }
            if (Config.Item("UseWFarm").GetValue<bool>() && W.IsReady())
            {
                if (Wnorm && !Wevolved)
                {
                    MinionManager.FarmLocation farmLocation =
                        MinionManager.GetBestCircularFarmLocation(
                            MinionManager.GetMinions(Player.Position, Wpred.Range)
                                .Select(minion => minion.ServerPosition.To2D())
                                .ToList(), Wpred.Width, Wpred.Range);

                    if (Player.Distance(farmLocation.Position) <= W.Range)
                        Wpred.Cast(farmLocation.Position);
                }
                if (Wevolved && !Wnorm)
                {
                    MinionManager.FarmLocation farmLocation =
                        MinionManager.GetBestCircularFarmLocation(
                            MinionManager.GetMinions(Player.Position, WEpred.Range)
                                .Select(minion => minion.ServerPosition.To2D())
                                .ToList(), WEpred.Width, WEpred.Range);

                    if (Player.Distance(farmLocation.Position) <= WE.Range)
                        WEpred.Cast(farmLocation.Position);
                }
            }

            if (Config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                if (Enorm && !Eevolved)
                {
                    MinionManager.FarmLocation farmLocation =
                        MinionManager.GetBestCircularFarmLocation(
                            MinionManager.GetMinions(Player.Position, Epred.Range)
                                .Select(minion => minion.ServerPosition.To2D())
                                .ToList(), Epred.Width, Epred.Range);

                    if (Player.Distance(farmLocation.Position) <= W.Range)
                        Epred.Cast(farmLocation.Position);
                }
                if (Eevolved && !Enorm)
                {
                    MinionManager.FarmLocation farmLocation =
                        MinionManager.GetBestCircularFarmLocation(
                            MinionManager.GetMinions(Player.Position, EEpred.Range)
                                .Select(minion => minion.ServerPosition.To2D())
                                .ToList(), EEpred.Width, EEpred.Range);

                    if (Player.Distance(farmLocation.Position) <= WE.Range)
                        EEpred.Cast(farmLocation.Position);
                }
            }
            if (Config.Item("UseItems").GetValue<bool>())
            {
                MinionManager.FarmLocation farmLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(Player.Position, HDR.Range)
                            .Select(minion => minion.ServerPosition.To2D())
                            .ToList(), HDR.Range, HDR.Range);

                if (HDR.IsReady() && Player.Distance(farmLocation.Position) <= HDR.Range && farmLocation.MinionsHit >= 2)
                {
                    // HDR.Cast(farmLocation.Position);
                    HDR.Cast();
                }
                if (TIA.IsReady() && Player.Distance(farmLocation.Position) <= TIA.Range && farmLocation.MinionsHit >= 2)
                {
                    //TIA.Cast(farmLocation.Position);
                    TIA.Cast();
                }
            }
        }

        private static void Farm()
        {
            var pos = new List<Vector2>();
            List<Obj_AI_Base> allminions = MinionManager.GetMinions(Player.ServerPosition, EE.Range, MinionTypes.All,
                MinionTeam.Enemy, MinionOrderTypes.Health);
            foreach (Obj_AI_Base minion in allminions)

            {
                //if (minion != null) { pos.Add(minion.Position.To2D()); }
                Orbwalker.SetAttack(!(Q.IsReady() || W.IsReady() || E.IsReady()) || TIA.IsReady() || HDR.IsReady());
                // Normal Farms
                if (Q.IsReady() && minion.IsValidTarget() && Qnorm && Player.Distance(minion) <= Q.Range &&
                    Config.Item("UseQFarm").GetValue<bool>() && (minion != null))
                {
                    Q.Cast(minion);
                }
                if (W.IsReady() && minion.IsValidTarget() && Wnorm && Player.Distance(minion) <= W.Range &&
                    Config.Item("UseWFarm").GetValue<bool>() && (minion != null))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestLineFarmLocation(pos, 70, 1025);
                    Wpred.Cast(pred.Position);
                }
                if (E.IsReady() && minion.IsValidTarget() && Enorm && Player.Distance(minion) <= E.Range &&
                    Config.Item("UseEFarm").GetValue<bool>() && (minion != null))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestCircularFarmLocation(pos, 300, 600);
                    Epred.Cast(pred.Position);
                }
                //Evolved
                if (Q.IsReady() && minion.IsValidTarget() && Qevolved && Player.Distance(minion) <= QE.Range &&
                    Config.Item("UseQFarm").GetValue<bool>() && (minion != null))
                {
                    QE.Cast(minion);
                }
                if (W.IsReady() && minion.IsValidTarget() && Wevolved && Player.Distance(minion) <= WE.Range &&
                    Config.Item("UseWFarm").GetValue<bool>() && (minion != null))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestLineFarmLocation(pos, 70, 1025);
                    WEpred.Cast(pred.Position);
                }
                if (E.IsReady() && minion.IsValidTarget() && Eevolved && Player.Distance(minion) <= EE.Range &&
                    Config.Item("UseEFarm").GetValue<bool>() && (minion != null)) //(pos.Any())
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestCircularFarmLocation(pos, 300, 900);
                    EEpred.Cast(pred.Position);
                }
                if (Config.Item("UseItems").GetValue<bool>() && minion.IsValidTarget() && (minion != null))
                {
                    if (HDR.IsReady() && Player.Distance(minion) <= HDR.Range)
                    {
                        HDR.Cast(minion);
                    }
                    if (TIA.IsReady() && Player.Distance(minion) <= TIA.Range)
                    {
                        TIA.Cast(minion);
                    }
                }
            }
        }


        private static void KillSteal()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            double igniteDmg = Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            double QDmg = Player.GetSpellDamage(target, SpellSlot.Q);
            double hydradmg = Player.GetItemDamage(target, Damage.DamageItems.Hydra);
            double tiamatdmg = Player.GetItemDamage(target, Damage.DamageItems.Tiamat);
            double WDmg = Player.GetSpellDamage(target, SpellSlot.W);
            double EDmg = Player.GetSpellDamage(target, SpellSlot.E);
            double QEVDmg = Player.GetSpellDamage(target, SpellSlot.Q);
            double WEVDmg = Player.GetSpellDamage(target, SpellSlot.W);
            double EEVDmg = Player.GetSpellDamage(target, SpellSlot.E);

            if (target != null && Config.Item("UseIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (igniteDmg > target.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, target);
                }
            }

            if (Q.IsReady() && Qnorm && Player.Distance(target) <= Q.Range && target != null &&
                Config.Item("UseQKs").GetValue<bool>())
            {
                if (target.Health <= QDmg)
                {
                    Q.Cast(target);
                }
            }

            if (Q.IsReady() && Qevolved && Player.Distance(target) <= QE.Range && target != null &&
                Config.Item("UseQKs").GetValue<bool>())
            {
                if (target.Health <= QDmg)
                {
                    QE.Cast(target);
                }
            }
            if (E.IsReady() && Enorm && Player.Distance(target) <= E.Range && target != null &&
                Config.Item("UseEKs").GetValue<bool>())
            {
                if (target.Health <= EDmg)
                {
                    PredictionOutput pred = Epred.GetPrediction(target);

                    Utility.DelayAction.Add(Game.Ping + 200, delegate
                    {
                        if (target != null)
                        {
                            Epred.Cast(pred.CastPosition);
                        }
                    });
                }
            }
            if (E.IsReady() && Eevolved && Player.Distance(target) <= EE.Range && target != null &&
                Config.Item("UseEKs").GetValue<bool>())
            {
                if (target.Health <= EEVDmg)
                {
                    PredictionOutput pred = EEpred.GetPrediction(target); //EEpred.Cast(pred.CastPosition);
                    Utility.DelayAction.Add(Game.Ping + 200, delegate
                    {
                        if (target != null)
                        {
                            EEpred.Cast(pred.CastPosition);
                        }
                    });
                }
            }

            if (W.IsReady() && Wnorm && Player.Distance(target) <= W.Range && target != null &&
                Config.Item("UseWKs").GetValue<bool>())
            {
                if (target.Health <= WDmg)
                {
                    // var pred = Wpred.GetPrediction(target); Wpred.Cast(pred.CastPosition);
                    if (Wpred.GetPrediction(target).Hitchance >= HitChance.Medium)
                    {
                        foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            PredictionOutput pred = Wpred.GetPrediction(target);
                            Wpred.Cast(pred.CastPosition);
                        }
                    }
                    if (Wpred.GetPrediction(target).Hitchance >= HitChance.Collision)
                    {
                        foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            List<Obj_AI_Base> PCollision = Wpred.GetPrediction(target).CollisionObjects;
                            foreach (
                                Obj_AI_Base PredCollisionChar in
                                    PCollision.Where(PredCollisionChar => PredCollisionChar.Distance(target) <= 50))
                            {
                                Wpred.Cast(PredCollisionChar.Position, Config.Item("usePackets").GetValue<bool>());
                            }
                        }
                    }
                    if (!(Wpred.GetPrediction(target).Hitchance >= HitChance.Medium) &&
                        !(Wpred.GetPrediction(target).Hitchance >= HitChance.Collision))
                    {
                        foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            PredictionOutput pred = Wpred.GetPrediction(target);
                            Wpred.Cast(pred.CastPosition);
                        }
                    }
                }
            }
            if (W.IsReady() && Wevolved && Player.Distance(target) <= WE.Range && target != null &&
                Config.Item("UseWKs").GetValue<bool>())
            {
                if (target.Health <= WDmg)
                {
                    // var pred = WEpred.GetPrediction(target); WEpred.Cast(pred.CastPosition); 
                    if (WEpred.GetPrediction(target).Hitchance >= HitChance.Medium)
                    {
                        foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            PredictionOutput pred = WEpred.GetPrediction(target);
                            WEpred.Cast(pred.CastPosition);
                        }
                    }
                    if (WEpred.GetPrediction(target).Hitchance >= HitChance.Collision)
                    {
                        foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            List<Obj_AI_Base> PCollision = WEpred.GetPrediction(target).CollisionObjects;
                            foreach (
                                Obj_AI_Base PredCollisionChar in
                                    PCollision.Where(PredCollisionChar => PredCollisionChar.Distance(target) <= 50))
                            {
                                WEpred.Cast(PredCollisionChar.Position, Config.Item("usePackets").GetValue<bool>());
                            }
                        }
                    }
                    if (!(WEpred.GetPrediction(target).Hitchance >= HitChance.Medium) &&
                        !(WEpred.GetPrediction(target).Hitchance >= HitChance.Collision))
                    {
                        foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            PredictionOutput pred = WEpred.GetPrediction(target);
                            WEpred.Cast(pred.CastPosition);
                        }
                    }
                }
            }

            // Mixed's EQ KS
            if (Q.IsReady() && E.IsReady() && Qnorm && Enorm && Player.Distance(target) <= E.Range + Q.Range &&
                (target != null && Config.Item("UseEQKs").GetValue<bool>()))
            {
                if ((target.Health <= QDmg + EDmg) || (target.Health <= QDmg))
                {
                    PredictionOutput pred = Epred.GetPrediction(target);
                    Epred.Cast(pred.CastPosition);
                }
            }
            if (Q.IsReady() && E.IsReady() && Qevolved && Eevolved && Player.Distance(target) <= EE.Range + QE.Range &&
                (target != null && Config.Item("UseEQKs").GetValue<bool>()))
            {
                if ((target.Health <= QEVDmg + EEVDmg) || (target.Health <= QEVDmg))
                {
                    PredictionOutput pred = EEpred.GetPrediction(target); //EEpred.Cast(pred.CastPosition);
                    Utility.DelayAction.Add(Game.Ping + 200, delegate { EEpred.Cast(pred.CastPosition); });
                }
            }

            if (Q.IsReady() && E.IsReady() && Qevolved && Enorm && Player.Distance(target) <= E.Range + Q.Range &&
                (target != null && Config.Item("UseEQKs").GetValue<bool>()))
            {
                if ((target.Health <= QEVDmg + EDmg) || (target.Health <= QEVDmg))
                {
                    PredictionOutput pred = Epred.GetPrediction(target); //Epred.Cast(pred.CastPosition);
                    Utility.DelayAction.Add(Game.Ping + 200, delegate { Epred.Cast(pred.CastPosition); });
                }
            }
            if (Q.IsReady() && E.IsReady() && Qnorm && Eevolved && Player.Distance(target) <= EE.Range + QE.Range &&
                (target != null && Config.Item("UseEQKs").GetValue<bool>()))
            {
                if ((target.Health <= QEVDmg + EEVDmg) || (target.Health <= QEVDmg))
                {
                    PredictionOutput pred = EEpred.GetPrediction(target); //EEpred.Cast(pred.CastPosition);
                    Utility.DelayAction.Add(Game.Ping + 200, delegate { EEpred.Cast(pred.CastPosition); });
                }
            }
            // MIXED EW KS
            if (W.IsReady() && E.IsReady() && Enorm && Wnorm && Player.Distance(target) <= W.Range + E.Range &&
                (target != null && Config.Item("UseEWKs").GetValue<bool>()))
            {
                if (target.Health <= WDmg)
                {
                    PredictionOutput pred = Epred.GetPrediction(target); //Epred.Cast(pred.CastPosition);
                    Utility.DelayAction.Add(Game.Ping + 200, delegate { Epred.Cast(pred.CastPosition); });
                }
            }
            if (W.IsReady() && E.IsReady() && Eevolved && Wevolved && Player.Distance(target) <= WE.Range + EE.Range &&
                (target != null && Config.Item("UseEWKs").GetValue<bool>()))
            {
                if (target.Health <= WDmg)
                {
                    PredictionOutput pred = EEpred.GetPrediction(target); //EEpred.Cast(pred.CastPosition);
                    Utility.DelayAction.Add(Game.Ping + 200, delegate { EEpred.Cast(pred.CastPosition); });
                }
            }
            if (W.IsReady() && E.IsReady() && Enorm && Wevolved && Player.Distance(target) <= WE.Range + E.Range &&
                (target != null && Config.Item("UseEWKs").GetValue<bool>()))
            {
                if (target.Health <= WDmg)
                {
                    PredictionOutput pred = Epred.GetPrediction(target); //Epred.Cast(pred.CastPosition);
                    Utility.DelayAction.Add(Game.Ping + 200, delegate { Epred.Cast(pred.CastPosition); });
                }
            }
            if (W.IsReady() && E.IsReady() && Eevolved && Wnorm && Player.Distance(target) <= W.Range + EE.Range &&
                (target != null && Config.Item("UseEWKs").GetValue<bool>()))
            {
                if (target.Health <= WDmg)
                {
                    PredictionOutput pred = EEpred.GetPrediction(target); //EEpred.Cast(pred.CastPosition);
                    Utility.DelayAction.Add(Game.Ping + 200, delegate { EEpred.Cast(pred.CastPosition); });
                }
            }

            //Tiamat/Hydra/BORK KSES

            if (Q.IsReady() && TIA.IsReady() && Qnorm && Player.Distance(target) <= Q.Range && target != null &&
                Config.Item("UseQKs").GetValue<bool>())
            {
                if (target.Health <= QDmg + tiamatdmg)
                {
                    Q.Cast(target);
                    TIA.Cast();
                }
            }
            if (Q.IsReady() && HDR.IsReady() && Qnorm && Player.Distance(target) <= Q.Range && target != null &&
                Config.Item("UseQKs").GetValue<bool>())
            {
                if (target.Health <= QDmg + hydradmg)
                {
                    Q.Cast();
                    HDR.Cast();
                }
            }
            if (Q.IsReady() && TIA.IsReady() && Qevolved && Player.Distance(target) <= QE.Range && target != null &&
                Config.Item("UseQKs").GetValue<bool>())
            {
                if (target.Health <= QDmg + tiamatdmg)
                {
                    Q.Cast(target);
                    TIA.Cast(target);
                }
            }
            if (Q.IsReady() && HDR.IsReady() && Qevolved && Player.Distance(target) <= QE.Range && target != null &&
                Config.Item("UseQKs").GetValue<bool>())
            {
                if (target.Health <= QDmg + hydradmg)
                {
                    Q.Cast();
                    HDR.Cast();
                }
            }
        }


        private static void CheckSpells()
        {
            //check for evolutions
            if (ObjectManager.Player.HasBuff("khazixqevo", true))
            {
                Qevolved = true;
                Qnorm = false;
            } // Game.PrintChat("Got Q Evolved");
            if (ObjectManager.Player.HasBuff("khazixwevo", true))
            {
                Wevolved = true;
                Wnorm = false;
            } //Game.PrintChat("Got W Evolved");
            if (ObjectManager.Player.HasBuff("khazixeevo", true))
            {
                Eevolved = true;
                Enorm = false;
            } //Game.PrintChat("Got W Evolved"); 

            //check if not evolved not needed but just for secondary ways
            if (!ObjectManager.Player.HasBuff("khazixqevo", true))
            {
                Qnorm = true;
                Qevolved = false;
            }
            if (!ObjectManager.Player.HasBuff("khazixwevo", true))
            {
                Wnorm = true;
                Wevolved = false;
            }
            if (!ObjectManager.Player.HasBuff("khazixeevo", true))
            {
                Enorm = true;
                Eevolved = false;
            }


            //older method didn't work my bad
            // if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "KhazixQ") { Qnorm = true;  Qevolved = false; }
            // if (Player.Spellbook.GetSpell(SpellSlot.W).Name == "KhazixW") { Wnorm = true; Wevolved = false; }
            // if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "KhazixE") { Enorm = true; Eevolved = false; }

            //  if (Player.Spellbook.GetSpell(SpellSlot.Q).Name != "KhazixQ") { Qevolved = true; Qnorm = false; }
            // if (Player.Spellbook.GetSpell(SpellSlot.W).Name != "KhazixW") { Wevolved = true; Wnorm = false; }
            // if (Player.Spellbook.GetSpell(SpellSlot.E).Name != "KhazixE") { Eevolved = true; Enorm = false; }
            // Game.PrintChat(Player.Spellbook.GetSpell(SpellSlot.Q).Name);

            //  Game.PrintChat(Player.Spellbook.GetSpell(SpellSlot.W).Name);
            // Game.PrintChat(Player.Spellbook.GetSpell(SpellSlot.E).Name);
        }

        private static void AutoHarrass()
        {
            if (!Config.Item("AutoHarrass").GetValue<bool>())
            {
                return;
            }
            Obj_AI_Hero target = TargetSelector.GetTarget(1025, TargetSelector.DamageType.Physical);
            var usePacket = Config.Item("usePackets").GetValue<bool>();

            var autoWI = Config.Item("AutoWI").GetValue<bool>();
            var autoWD = Config.Item("AutoWD").GetValue<bool>();
            if ((target != null) && (W.IsReady()) &&
                ((Wpred.GetPrediction(target).Hitchance >= HitChance.Medium) ||
                 Wpred.GetPrediction(target).Hitchance >= HitChance.High))
            {
                //possible atr fix W.isready^^

                if (Wnorm && Player.Distance(target) <= W.Range && Config.Item("AutoHarrass").GetValue<bool>() &&
                    W.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = Wpred.GetPrediction(target);
                        Wpred.Cast(pred.CastPosition, usePacket);
                    }
                }
                if (Wevolved && Player.Distance(target) <= WE.Range && Config.Item("AutoHarrass").GetValue<bool>() &&
                    W.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsValidTarget(WE.Range*2))
                        {
                            PredictionOutput pred = WEpred.GetPrediction(target);
                            if ((pred.Hitchance == HitChance.Immobile && autoWI) ||
                                (pred.Hitchance == HitChance.Dashing && autoWD))
                            {
                                CastWE(enemy, pred.UnitPosition.To2D());
                            }
                        }
                    }
                }
                if (Wevolved && Player.Distance(target) <= WE.Range && Config.Item("AutoHarrass").GetValue<bool>() &&
                    W.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsValidTarget(WE.Range*2))
                        {
                            PredictionOutput pred = WEpred.GetPrediction(target);
                            CastWE(enemy, pred.UnitPosition.To2D());
                        }
                    }
                }
            }
        }


        private static void Combo()
        {
            var usePacket = Config.Item("usePackets").GetValue<bool>();
            Obj_AI_Hero target = TargetSelector.GetTarget(1275, TargetSelector.DamageType.Physical);


            // Orbwalker.SetAttacks(!(Q.IsReady() || W.IsReady() || E.IsReady()) || TIA.IsReady() || HDR.IsReady());

            if ((target != null))
            {
                var pos = new List<Vector2>();
                pos.Add(target.Position.To2D());
                // Normal abilities
                if (Qnorm && Player.Distance(target) <= Q.Range && Config.Item("UseQCombo").GetValue<bool>() &&
                    Q.IsReady())
                {
                    Q.Cast(target);
                }
                //  if (Wnorm && Player.Distance(target) <= W.Range && Config.Item("UseWCombo").GetValue<bool>() && W.IsReady()) { foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>()) { if (enemy.IsValidTarget(W.Range * 2)) { var pred = Wpred.GetPrediction(enemy); if ((pred.Hitchance == HitChance.Immobile && autoQI) || (pred.Hitchance == HitChance.Dashing && autoQD)) { Wpred.Cast(pred.CastPosition); } } } }
                if (Wnorm && Player.Distance(target) <= W.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady() && Wpred.GetPrediction(target).Hitchance >= HitChance.Medium)
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = Wpred.GetPrediction(target);
                        Wpred.Cast(pred.CastPosition, usePacket);
                    }
                }
                if (Wnorm && Player.Distance(target) <= W.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady() && Wpred.GetPrediction(target).Hitchance >= HitChance.Collision)
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        List<Obj_AI_Base> PCollision = Wpred.GetPrediction(target).CollisionObjects;
                        foreach (
                            Obj_AI_Base PredCollisionChar in
                                PCollision.Where(PredCollisionChar => PredCollisionChar.Distance(target) <= 50))
                        {
                            Wpred.Cast(PredCollisionChar.Position, usePacket);
                        }
                    }
                }
                if (Wnorm && Player.Distance(target) <= W.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady() && !(Wpred.GetPrediction(target).Hitchance >= HitChance.Collision) &&
                    !(Wpred.GetPrediction(target).Hitchance >= HitChance.Medium))
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = Wpred.GetPrediction(target);
                        Wpred.Cast(pred.CastPosition, usePacket);
                    }
                }
                if (Enorm && Player.Distance(target) <= E.Range && Config.Item("UseECombo").GetValue<bool>() &&
                    E.IsReady() && (target != null) && Player.Distance(target) > Q.Range)
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = Epred.GetPrediction(target);
                        Epred.Cast(pred.CastPosition, usePacket);
                    }
                }

                // Use EQ AND EW Synergy
                if (Enorm && Qnorm && Player.Distance(target) <= E.Range + Q.Range && Player.Distance(target) > Q.Range
                    /* && Config.Item("UseECombo").GetValue<bool>() */&& E.IsReady() &&
                    Config.Item("UseEGapclose").GetValue<bool>() && (target != null))
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = Epred.GetPrediction(target);
                        Epred.Cast(pred.CastPosition, usePacket);
                    }
                }
                if (Enorm && Wnorm && Qnorm && Player.Distance(target) <= E.Range + W.Range &&
                    Player.Distance(target) > Q.Range /* && Config.Item("UseECombo").GetValue<bool>() */&& E.IsReady() &&
                    Config.Item("UseEGapcloseW").GetValue<bool>() && (target != null) && W.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = Epred.GetPrediction(target);
                        Epred.Cast(pred.CastPosition, usePacket);
                    }
                }
                if (Enorm && Wnorm && Qevolved && Player.Distance(target) <= E.Range + W.Range &&
                    Player.Distance(target) > QE.Range /* && Config.Item("UseECombo").GetValue<bool>() */&& E.IsReady() &&
                    Config.Item("UseEGapcloseW").GetValue<bool>() && (target != null) && W.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = Epred.GetPrediction(target);
                        Epred.Cast(pred.CastPosition, usePacket);
                    }
                }
                // Ult Usage
                if (R.IsReady() && !Q.IsReady() && !W.IsReady() && !E.IsReady() &&
                    Config.Item("UseRCombo").GetValue<bool>())
                {
                    R.Cast();
                    if (Config.Item("Debugon").GetValue<bool>())
                    {
                        Game.PrintChat("9 - Basic Ult Cast");
                    }
                }
                // Evolved
                if (Qevolved && Player.Distance(target) <= QE.Range && Config.Item("UseQCombo").GetValue<bool>() &&
                    Q.IsReady())
                {
                    QE.Cast(target);
                    if (Config.Item("Debugon").GetValue<bool>())
                    {
                        Game.PrintChat("10 - QE cast");
                    }

                }
                //  if (Wevolved && Player.Distance(target) <= WE.Range && Config.Item("UseWCombo").GetValue<bool>() && W.IsReady()) { foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>()) { if (enemy.IsValidTarget(WE.Range * 2)) { var pred = WEpred.GetPrediction(target); if ((pred.Hitchance == HitChance.Immobile && autoQI) || (pred.Hitchance == HitChance.Dashing && autoQD)) { CastWE(enemy, pred.UnitPosition.To2D()); } } } }
                //   if (Wnorm && Player.Distance(target) <= W.Range && Config.Item("UseWCombo").GetValue<bool>() && W.IsReady() && Wpred.GetPrediction(target).Hitchance >= HitChance.Medium) { foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>()) { var pred = Wpred.GetPrediction(target); Wpred.Cast(pred.CastPosition, usePacket); Game.PrintChat("2 - WpredCast Medium"); } }
                if (Wevolved && Player.Distance(target) <= WE.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady() && Wpred.GetPrediction(target).Hitchance >= HitChance.Medium)
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = WEpred.GetPrediction(target);
                        WEpred.Cast(pred.CastPosition, usePacket); /* CastWE(enemy, pred.UnitPosition.To2D()); */
                    }
                }
                if (Wevolved && Player.Distance(target) <= WE.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady() && Wpred.GetPrediction(target).Hitchance >= HitChance.Collision)
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        List<Obj_AI_Base> PCollision = Wpred.GetPrediction(target).CollisionObjects;
                        foreach (
                            Obj_AI_Base PredCollisionChar in
                                PCollision.Where(PredCollisionChar => PredCollisionChar.Distance(target) <= 50))
                        {
                            WEpred.Cast(PredCollisionChar.Position, usePacket);
                        }
                    }
                }
                if (Wevolved && Player.Distance(target) <= WE.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady() && !(Wpred.GetPrediction(target).Hitchance >= HitChance.Medium) &&
                    !(Wpred.GetPrediction(target).Hitchance >= HitChance.Medium))
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsValidTarget(WE.Range*2))
                        {
                            PredictionOutput pred = WEpred.GetPrediction(target);
                            CastWE(enemy, pred.UnitPosition.To2D());
                        }
                    }
                }
                if (Eevolved && Player.Distance(target) <= EE.Range && Qnorm && Player.Distance(target) > Q.Range &&
                    Config.Item("UseECombo").GetValue<bool>() && EE.IsReady() &&
                    /* !Config.Item("UseEGapclose").GetValue<bool>()  && */ (target != null))
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = EEpred.GetPrediction(target);
                        EEpred.Cast(pred.CastPosition, usePacket);
                    }
                }
                if (Eevolved && Player.Distance(target) <= EE.Range && Qevolved && Player.Distance(target) > QE.Range &&
                    Config.Item("UseECombo").GetValue<bool>() && EE.IsReady() &&
                    /* !Config.Item("UseEGapclose").GetValue<bool>()  && */ (target != null))
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = EEpred.GetPrediction(target);
                        EEpred.Cast(pred.CastPosition, usePacket);
                    }
                }
                // Evolved EQ AND EW SYNERGY 
                if (Eevolved && Qevolved && Player.Distance(target) <= EE.Range + QE.Range &&
                    Player.Distance(target) > QE.Range && /* Config.Item("UseECombo").GetValue<bool>()  && */
                    E.IsReady() && Config.Item("UseEGapclose").GetValue<bool>() && (target != null))
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = EEpred.GetPrediction(target);
                        EEpred.Cast(pred.CastPosition, usePacket);
                    }
                }
                if (Eevolved && Wevolved && Qevolved && Player.Distance(target) <= EE.Range + WE.Range &&
                    Player.Distance(target) > QE.Range && /* Config.Item("UseECombo").GetValue<bool>()  && */
                    E.IsReady() && Config.Item("UseEGapcloseW").GetValue<bool>() && (target != null) && W.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = EEpred.GetPrediction(target);
                        EEpred.Cast(pred.CastPosition, usePacket);
                    }
                }
                if (Eevolved && Wevolved && Qnorm && Player.Distance(target) <= EE.Range + WE.Range &&
                    Player.Distance(target) > Q.Range && /* Config.Item("UseECombo").GetValue<bool>()  && */ E.IsReady() &&
                    Config.Item("UseEGapcloseW").GetValue<bool>() && (target != null) && W.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = EEpred.GetPrediction(target);
                        EEpred.Cast(pred.CastPosition, usePacket);
                    }
                }
                // Use Ult When closing big gaps like E to get close for W
                if (Eevolved && Wevolved && Qevolved && Player.Distance(target) <= EE.Range + WE.Range &&
                    Player.Distance(target) > QE.Range && /* Config.Item("UseECombo").GetValue<bool>()  && */
                    E.IsReady() && Config.Item("UseEGapcloseW").GetValue<bool>() && (target != null) && W.IsReady() &&
                    Config.Item("UseRGapcloseW").GetValue<bool>() && R.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = EEpred.GetPrediction(target);
                        EEpred.Cast(pred.CastPosition, usePacket);
                        R.CastOnUnit(ObjectManager.Player);
                    }
                }
                if (Eevolved && Wevolved && Qnorm && Player.Distance(target) <= EE.Range + WE.Range &&
                    Player.Distance(target) > Q.Range && /* Config.Item("UseECombo").GetValue<bool>()  && */ E.IsReady() &&
                    Config.Item("UseEGapcloseW").GetValue<bool>() && (target != null) && W.IsReady() &&
                    Config.Item("UseRGapcloseW").GetValue<bool>() && R.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = EEpred.GetPrediction(target);
                        EEpred.Cast(pred.CastPosition, usePacket);
                        R.CastOnUnit(ObjectManager.Player);
                    }
                }
                if (Enorm && Wnorm && Qnorm && Player.Distance(target) <= E.Range + W.Range &&
                    Player.Distance(target) > Q.Range && /* Config.Item("UseECombo").GetValue<bool>()  && */ E.IsReady() &&
                    Config.Item("UseEGapcloseW").GetValue<bool>() && (target != null) && W.IsReady() &&
                    Config.Item("UseRGapcloseW").GetValue<bool>() && R.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = Epred.GetPrediction(target);
                        Epred.Cast(pred.CastPosition, usePacket);
                        R.CastOnUnit(ObjectManager.Player);
                    }
                }
                if (Enorm && Wnorm && Qevolved && Player.Distance(target) <= E.Range + W.Range &&
                    Player.Distance(target) > QE.Range && /* Config.Item("UseECombo").GetValue<bool>()  && */
                    E.IsReady() && Config.Item("UseEGapcloseW").GetValue<bool>() && (target != null) && W.IsReady() &&
                    Config.Item("UseRGapcloseW").GetValue<bool>() && R.IsReady())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = Epred.GetPrediction(target);
                        Epred.Cast(pred.CastPosition, usePacket);
                        R.CastOnUnit(ObjectManager.Player);
                    }
                }
                //Mixed Evolveds
                if (Eevolved && Qnorm && Player.Distance(target) <= EE.Range + Q.Range &&
                    Player.Distance(target) > Q.Range && /* Config.Item("UseECombo").GetValue<bool>()  && */ E.IsReady() &&
                    Config.Item("UseEGapclose").GetValue<bool>() && (target != null))
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = EEpred.GetPrediction(target);
                        EEpred.Cast(pred.CastPosition, usePacket);
                    }
                }
                if (Enorm && Qevolved && Player.Distance(target) <= E.Range + QE.Range &&
                    Player.Distance(target) > QE.Range && /* Config.Item("UseECombo").GetValue<bool>()  && */
                    E.IsReady() && Config.Item("UseEGapclose").GetValue<bool>() && (target != null))
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        PredictionOutput pred = Epred.GetPrediction(target);
                        Epred.Cast(pred.CastPosition, usePacket);
                    }
                }


                if (Config.Item("UseItems").GetValue<bool>())
                {
                    if (HDR.IsReady() && Player.Distance(target) <= HDR.Range)
                    {
                        HDR.Cast();
                    }
                    if (TIA.IsReady() && Player.Distance(target) <= TIA.Range)
                    {
                        TIA.Cast();
                    }
                    if (BKR.IsReady() && Player.Distance(target) <= BKR.Range)
                    {
                        BKR.Cast(target);
                    }
                    if (YOU.IsReady() && Player.Distance(target) <= YOU.Range)
                    {
                        YOU.Cast(target);
                    }
                    if (BWC.IsReady() && Player.Distance(target) <= BWC.Range)
                    {
                        BWC.Cast(target);
                    }
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("CircleLag").GetValue<bool>())
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    if (Qnorm)
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White,
                            Config.Item("CircleThickness").GetValue<Slider>().Value,
                            Config.Item("CircleQuality").GetValue<Slider>().Value);
                        if (Qevolved)
                        {
                            Utility.DrawCircle(ObjectManager.Player.Position, QE.Range, Color.White,
                                Config.Item("CircleThickness").GetValue<Slider>().Value,
                                Config.Item("CircleQuality").GetValue<Slider>().Value);
                        }
                    }
                }
                if (Config.Item("DrawW").GetValue<bool>())
                {
                    if (Wnorm)
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Red,
                            Config.Item("CircleThickness").GetValue<Slider>().Value,
                            Config.Item("CircleQuality").GetValue<Slider>().Value);
                    }
                    if (Wevolved)
                    {
                    }
                    Utility.DrawCircle(ObjectManager.Player.Position, WE.Range, Color.Red,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
                }


                if (Config.Item("DrawE").GetValue<bool>())
                {
                    if (Enorm)
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Green,
                            Config.Item("CircleThickness").GetValue<Slider>().Value,
                            Config.Item("CircleQuality").GetValue<Slider>().Value);
                    }
                }
                if (Eevolved)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, EE.Range, Color.Green,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }


            else
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    if (Qnorm)
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White);
                    }
                    if (Qevolved)
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, QE.Range, Color.White);
                    }
                }
                if (Config.Item("DrawW").GetValue<bool>())
                {
                    if (Wnorm)
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Red);
                    }
                    if (Wevolved)
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, WE.Range, Color.Red);
                    }
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    if (Enorm)
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Green);
                    }
                    if (Eevolved)
                    {
                        
                        Drawing.DrawCircle(ObjectManager.Player.Position, EE.Range, Color.Green);
                    }
                }
            }
        }
    }
}

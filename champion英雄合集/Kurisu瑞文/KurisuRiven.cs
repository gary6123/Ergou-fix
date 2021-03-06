using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace KurisuRiven
{
    internal class KurisuRiven
    {
        #region  Main

        public static Menu config;
        private static Obj_AI_Hero enemy;
        private static readonly Obj_AI_Hero me = ObjectManager.Player;
        private static Orbwalking.Orbwalker orbwalker;

        private static int eet;
        private static int qqt;
        public static int cleavecount;

        private static double ritems;
        private static double ua, uq, uw;
        private static double ra, rq, rw, rr, ri;
        private static float truerange;

        private static double now;
        private static double extraqtime;
        private static double extraetime;

        private static bool ultion, useblade, useautows;
        private static bool usecombo, useclear;
        private static int gaptime, wslash, wsneed, bladewhen;

        private static readonly Spell valor = new Spell(SpellSlot.E, 390f);
        private static readonly Spell wings = new Spell(SpellSlot.Q, 250f);
        private static readonly Spell kiburst = new Spell(SpellSlot.W, 250f);
        private static readonly Spell blade = new Spell(SpellSlot.R, 900f);

        private static float ee, ff;
        private static readonly int[] items = { 3144, 3153, 3142, 3112, 3131 };
        private static readonly int[] runicpassive =
        {
            20, 20, 25, 25, 25, 30, 30, 30, 35, 35, 35, 40, 40, 40, 45, 45, 45, 50, 50
        };

        public const string Revision = "1.0.0.1";
        private static readonly string[] jungleminions =
        {     
            "SRU_Razorbeak", "SRU_Krug", "Sru_Crab",
            "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp"
        };

        #endregion

        public KurisuRiven()
        {
            Console.WriteLine("KurisuRiven is loading..");
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }


        #region  OnGameLoad
        private void Game_OnGameLoad(EventArgs args)
        {
            if (me.ChampionName != "Riven")
                return;

            Initialize();

            config = new Menu("Kurisu瑞文", "kriven", true);

            // Target Selector
            var menuTS = new Menu("Riven: 目标选择", "tselect");
            TargetSelector.AddToMenu(menuTS);
            config.AddSubMenu(menuTS);

            // Orbwalker
            var menuOrb = new Menu("Riven: 走砍", "orbwalker");
            orbwalker = new Orbwalking.Orbwalker(menuOrb);
            config.AddSubMenu(menuOrb);

            var menuK = new Menu("Riven: 键位设置", "demkeys");
            menuK.AddItem(new MenuItem("combokey", "连招 键位")).SetValue(new KeyBind(32, KeyBindType.Press));
            menuK.AddItem(new MenuItem("harasskey", "H骚扰 键位")).SetValue(new KeyBind(67, KeyBindType.Press));
            menuK.AddItem(new MenuItem("clearkey", "清线 键位")).SetValue(new KeyBind(86, KeyBindType.Press));
            menuK.AddItem(new MenuItem("jumpkey", "跳墙 键位")).SetValue(new KeyBind(88, KeyBindType.Press));
            menuK.AddItem(new MenuItem("changemode", "更换 模式")).SetValue(new KeyBind(90, KeyBindType.Press));
            config.AddSubMenu(menuK);

            // Draw settings
            var menuD = new Menu("Riven: 范围设置 ", "dsettings");
            menuD.AddItem(new MenuItem("dsep1", "==== 范围设置"));
            menuD.AddItem(new MenuItem("drawrr", "显示 R 范围")).SetValue(false);
            menuD.AddItem(new MenuItem("drawqq", "显示 Q 范围")).SetValue(true);
            menuD.AddItem(new MenuItem("drawp", "显示 被动")).SetValue(true);
            menuD.AddItem(new MenuItem("drawengage", "显示 交战 范围")).SetValue(true);
            menuD.AddItem(new MenuItem("drawjumps", "显示 跳墙 位置")).SetValue(true);
            menuD.AddItem(new MenuItem("drawkill", "显示 击杀 提示")).SetValue(true);
            menuD.AddItem(new MenuItem("drawtarg", "显示 攻击 目标")).SetValue(true);
            config.AddSubMenu(menuD);

            // Combo Settings
            var menuC = new Menu("Riven: 连招设置 ", "csettings");
            menuC.AddItem(new MenuItem("csep1", "==== E 设置"));
            menuC.AddItem(new MenuItem("usevalor", "连招使用 E")).SetValue(true);
            menuC.AddItem(new MenuItem("valorhealth", "使用 E > 超出平A范围|血量低于")).SetValue(new Slider(40));
            menuC.AddItem(new MenuItem("csep2", "==== R 设置"));
            menuC.AddItem(new MenuItem("useblade", "连招使用 R")).SetValue(true);
            menuC.AddItem(new MenuItem("bladewhen", "R使用时机: "))
                .SetValue(new StringList(new[] { "容易击杀", "一般击杀", "困难击杀" }, 2));
            menuC.AddItem(new MenuItem("checkover", "击杀 提示")).SetValue(true);

            menuC.AddItem(new MenuItem("csep3", "==== Q 设置"));
            menuC.AddItem(new MenuItem("cancelassist", "连招使用 Q")).SetValue(true);           
            menuC.AddItem(new MenuItem("nostickyq", "左右 QA （封锁走位）")).SetValue(false);
            menuC.AddItem(new MenuItem("blockanim", "封包 Q (取消后摇)")).SetValue(false);
            menuC.AddItem(new MenuItem("qqdelay", "使用 Q 延迟: ")).SetValue(new Slider(1200, 1, 3000));
            
            config.AddSubMenu(menuC);

            // Extra Settings
            var menuO = new Menu("Riven: 额外设置 ", "osettings");
            menuO.AddItem(new MenuItem("osep2", "==== 额外设置"));
            menuO.AddItem(new MenuItem("useignote", "使用 点燃")).SetValue(true);
            menuO.AddItem(new MenuItem("useautow", "使用 自动 W")).SetValue(true);
            menuO.AddItem(new MenuItem("enableAntiG", "自动W 防止突进")).SetValue(true);
            menuO.AddItem(new MenuItem("autow", "W 眩晕最少目标")).SetValue(new Slider(3, 1, 5));
            menuO.AddItem(new MenuItem("osep1", "==== 风速斩 设置"));
            menuO.AddItem(new MenuItem("useautows", "启用 风速斩")).SetValue(true);
            menuO.AddItem(new MenuItem("wslash", "风速斩: "))
                .SetValue(new StringList(new[] { "可以击杀", "最大伤害" }));
            menuO.AddItem(new MenuItem("autows", "使用风速斩|伤害%")).SetValue(new Slider(65, 1));
            menuO.AddItem(new MenuItem("autows2", "使用风速斩|人数>=")).SetValue(new Slider(3, 2, 5));
            menuO.AddItem(new MenuItem("osep3", "==== 中断法术 设置"));
            menuO.AddItem(new MenuItem("interrupter", "启用 中断法术")).SetValue(true);
            menuO.AddItem(new MenuItem("interruptQ3", "使用 Q3 中断法术")).SetValue(true);
            menuO.AddItem(new MenuItem("interruptW", "使用 W 中断法术")).SetValue(true);
            config.AddSubMenu(menuO);

            // Farm/Clear Settings
            var menuJ = new Menu("Riven: 清线|清野", "jsettings");
            menuJ.AddItem(new MenuItem("jsep1", "==== 清野 设置"));
            menuJ.AddItem(new MenuItem("jungleE", "启用 E ")).SetValue(true);
            menuJ.AddItem(new MenuItem("jungleW", "启用 W ")).SetValue(true);
            menuJ.AddItem(new MenuItem("jungleQ", "启用 Q")).SetValue(true);
            menuJ.AddItem(new MenuItem("jsep2", "==== 清线 设置"));
            menuJ.AddItem(new MenuItem("farmE", "启用 E")).SetValue(true);
            menuJ.AddItem(new MenuItem("farmW", "启用 W")).SetValue(true);
            menuJ.AddItem(new MenuItem("farmQ", "启用 Q")).SetValue(true);
            config.AddSubMenu(menuJ);

            var rivenD = new Menu("瑞文: 调试设置", "therivend");
            rivenD.AddItem(new MenuItem("dsep2", "==== 调试 设置"));
            rivenD.AddItem(new MenuItem("debugdmg", "调试 组合 连招")).SetValue(false);
            rivenD.AddItem(new MenuItem("debugtrue", "调试 准确 范围")).SetValue(false);
            rivenD.AddItem(new MenuItem("exportjump", "调试 输出 位置")).SetValue(new KeyBind(73, KeyBindType.Press));
            config.AddSubMenu(rivenD);

            var donate = new Menu("瑞文: 捐赠信息", "feelo");
            donate.AddItem(new MenuItem("donate", "捐款,因为你爱我 <3欧元?"));
            donate.AddItem(new MenuItem("donate2", "帐号：xrobinsong@gmail.com"));

            config.AddSubMenu(donate);
            config.AddToMainMenu();



            wings.SetSkillshot(0.25f, 200f, 1700, false, SkillshotType.SkillshotCircle);
            blade.SetSkillshot(0.25f, 300f, 120f, false, SkillshotType.SkillshotCone);

            var tcolor = config.Item("wslash").GetValue<StringList>().SelectedIndex == 0;
            var hex = tcolor ? "#7CFC00" : "#FF0080";

            Game.PrintChat("<font color='" + hex + "'>Kurisu鐟炴枃</font> - 璇诲彇鎴愬姛锛侊紒锛亅 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847");

        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            // initialize walljumps
            new KurisuLib();

            // On Game Draw
            Drawing.OnDraw += Game_OnDraw;

            // On Game Update
            Game.OnGameUpdate += Game_OnGameUpdate;

            // On Game Process Packet
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;

            // On Possible Interrupter
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;

            //On Enemy Gapcloser
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            // On Game Process Spell Cast
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            // On Play Animation
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;

        }


        private static void Obj_AI_Base_OnPlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe && args.Animation.Contains("Spell1"))
            {
                
                var id = orbwalker.GetTarget() != null
                    ? orbwalker.GetTarget().NetworkId
                    : me.NetworkId;

                var obj = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(id);

                var movePos = obj.ServerPosition +
                              Vector3.Normalize(me.ServerPosition - obj.ServerPosition) * (me.Distance(obj.ServerPosition) + 63);
                 
                if (config.Item("combokey").GetValue<KeyBind>().Active || 
                    config.Item("clearkey").GetValue<KeyBind>().Active || 
                    config.Item("harasskey").GetValue<KeyBind>().Active)
                {

                    if (config.Item("cancelassist").GetValue<bool>())
                        Orbwalking.Move = false;
                    //Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(movePos.X, movePos.Y)).Send();
                    me.IssueOrder(GameObjectOrder.MoveTo, new Vector3(movePos.X, movePos.Y, movePos.Z));
                    Utility.DelayAction.Add(350, () => Orbwalking.Move = true);
                    Utility.DelayAction.Add(370, delegate
                    {
                        Orbwalking.LastAATick = 0;
                        //Game.PrintChat("reset");
                    });
                    
                }
            }
        }      

        #endregion

        #region  OnGameUpdate

        private static bool color;
        private void Game_OnGameUpdate(EventArgs args)
        {
            enemy = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
            if (config.Item("changemode").GetValue<KeyBind>().Active)
            {
                switch (wslash)
                {
                    case 1:
                        Utility.DelayAction.Add(300,
                            () => config.Item("wslash").SetValue(new StringList(new[] { "Only Kill", "Max Damage" }, 0)));
                        break;
                    case 0:
                        Utility.DelayAction.Add(300,
                            () => config.Item("wslash").SetValue(new StringList(new[] { "Only Kill", "Max Damage" }, 1)));
                        break;
                }
            }

            if (usecombo)
                CastCombo(enemy);

            foreach (var b in me.Buffs.Where(b => b.Name == "RivenTriCleave"))
                cleavecount = b.Count;

            if (!me.HasBuff("RivenTriCleave", true))
                Utility.DelayAction.Add(700, () => cleavecount = 0);

            Clear();
            AutoW();
            Requisites();
            WindSlash();
            
        }

        #endregion

        #region  Requisites
        private static void Requisites()
        {

            color = wslash == 0;
            CheckDamage(enemy);

            now = TimeSpan.FromMilliseconds(Environment.TickCount).TotalSeconds;
            truerange = me.AttackRange + me.Distance(me.BBox.Minimum) + 1;

            ee = (ff - Game.Time > 0) ? (ff - Game.Time) : 0;
            ultion = me.HasBuff("RivenFengShuiEngine", true);

            wsneed = config.Item("autows").GetValue<Slider>().Value;
            gaptime = config.Item("qqdelay").GetValue<Slider>().Value;

            usecombo = config.Item("combokey").GetValue<KeyBind>().Active;
            useclear = config.Item("clearkey").GetValue<KeyBind>().Active;
            useautows = config.Item("useautows").GetValue<bool>();

            bladewhen = config.Item("bladewhen").GetValue<StringList>().SelectedIndex;
            wslash = config.Item("wslash").GetValue<StringList>().SelectedIndex;

            useblade = config.Item("useblade").GetValue<bool>();

            extraqtime = TimeSpan.FromMilliseconds(gaptime).TotalSeconds;
            extraetime = TimeSpan.FromMilliseconds(300).TotalSeconds;
        }

        #endregion

        #region  On Draw
        private static void Game_OnDraw(EventArgs args)
        {
            if (config.Item("drawtarg").GetValue<bool>() && enemy.IsValidTarget(900))
            {
                Utility.DrawCircle(enemy.Position, wings.Range, Color.Red, 5, 1);
            }

            if (config.Item("drawqq").GetValue<bool>() && !me.IsDead)
            {
                Utility.DrawCircle(me.Position, wings.Range + 1, color ? Color.LawnGreen : Color.Fuchsia, 2, 1);
                Utility.DrawCircle(me.Position, wings.Range - 3, Color.White, 2, 1);
            }

            if (config.Item("drawrr").GetValue<bool>() && !me.IsDead)
            {
                Utility.DrawCircle(me.Position, blade.Range + 1, color ? Color.LawnGreen : Color.Fuchsia, 2, 1);
                Utility.DrawCircle(me.Position, blade.Range - 3, Color.White, 2, 1);
            }
            if (config.Item("drawp").GetValue<bool>() && !me.IsDead)
            {
                var wts = Drawing.WorldToScreen(me.Position);

                if (wslash == 0)
                    Drawing.DrawText(wts[0] - 55, wts[1] + 30, Color.LawnGreen, "Mode: Only Kill");
                else
                    Drawing.DrawText(wts[0] - 55, wts[1] + 30, Color.Fuchsia, "Mode: Reckless");
                if (me.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0] - 35, wts[1] + 10, Color.White, "Q: Not Learned!");
                else if (ee <= 0)
                    Drawing.DrawText(wts[0] - 35, wts[1] + 10, Color.White, "Q: Ready");
                else
                    Drawing.DrawText(wts[0] - 35, wts[1] + 10, Color.White, "Q: " + ee.ToString("0.0"));

            }
            if (config.Item("debugtrue").GetValue<bool>())
            {
                if (!me.IsDead)
                {
                    Utility.DrawCircle(me.Position, truerange + 25, Color.Yellow, 2, 1);
                }
            }

            if (config.Item("drawengage").GetValue<bool>() && !me.IsDead)
            {
                Utility.DrawCircle(me.Position, valor.Range + me.AttackRange - 35, color ? Color.LawnGreen : Color.Fuchsia, 2, 1);
                Utility.DrawCircle(me.Position, valor.Range + me.AttackRange - 32, Color.White, 2, 1);
            }

            if (config.Item("drawkill").GetValue<bool>())
            {
                if (enemy != null && !enemy.IsDead && !me.IsDead)
                {
                    var ts = enemy;
                    var wts = Drawing.WorldToScreen(enemy.Position);
                    if ((float)(ra + rq * 2 + rw + ri + ritems) > ts.Health)
                        Drawing.DrawText(wts[0] - 20, wts[1] + 40, Color.OrangeRed, "Kill!");
                    else if ((float)(ra * 2 + rq * 2 + rw + ritems) > ts.Health)
                        Drawing.DrawText(wts[0] - 40, wts[1] + 40, Color.OrangeRed, "Easy Kill!");
                    else if ((float)(ua * 3 + uq * 2 + uw + ri + rr + ritems) > ts.Health)
                        Drawing.DrawText(wts[0] - 40, wts[1] + 40, Color.OrangeRed, "Full Combo Kill!");
                    else if ((float)(ua * 3 + uq * 3 + uw + rr + ri + ritems) > ts.Health)
                        Drawing.DrawText(wts[0] - 40, wts[1] + 40, Color.OrangeRed, "Full Combo Hard Kill!");
                    else if ((float)(ua * 3 + uq * 3 + uw + rr + ri + ritems) < ts.Health)
                        Drawing.DrawText(wts[0] - 40, wts[1] + 40, Color.OrangeRed, "Cant Kill!");

                }
            }

            if (config.Item("debugdmg").GetValue<bool>())
            {
                if (enemy != null && !enemy.IsDead && !me.IsDead)
                {
                    var wts = Drawing.WorldToScreen(enemy.Position);
                    if (!blade.IsReady())
                        Drawing.DrawText(wts[0] - 75, wts[1] + 60, Color.Orange,
                            "Combo Damage: " + (float)(ra * 3 + rq * 3 + rw + rr + ri + ritems));
                    else
                        Drawing.DrawText(wts[0] - 75, wts[1] + 60, Color.Orange,
                            "Combo Damage: " + (float)(ua * 3 + uq * 3 + uw + rr + ri + ritems));
                }
            }

            if (config.Item("drawjumps").GetValue<bool>())
            {
                var jumplist = KurisuLib.jumpList;
                if (jumplist.Any())
                {
                    foreach (var j in jumplist)
                    {
                        if (me.Distance(j.pointA) <= 800 || me.Distance(j.pointB) <= 800)
                        {
                            Utility.DrawCircle(j.pointA, 100, color ? Color.LawnGreen : Color.Fuchsia, 2, 1);
                            Utility.DrawCircle(j.pointB, 100, color ? Color.LawnGreen : Color.Fuchsia, 2, 1);
                        }
                    }
                }
            }
        }

        #endregion

        #region Clear
        private static void Clear()
        {
            if (!useclear)
                return;

            var target = orbwalker.GetTarget();
            if (target.IsValidTarget(kiburst.Range) && jungleminions.Any(name => target.Name.StartsWith(name)))
            {
                if (kiburst.IsReady() && config.Item("jungleW").GetValue<bool>())
                    kiburst.Cast();
            }

            else if (target.IsValidTarget(wings.Range) && target.Name.StartsWith("Minion"))
            {
                if (!valor.IsReady() || !config.Item("farmE").GetValue<bool>()) return;
                if (wings.IsReady() && cleavecount >= 1)
                    valor.Cast(Game.CursorPos);
            }

            if (kiburst.IsReady() && config.Item("farmW").GetValue<bool>())
            {
                var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(kiburst.Range)).ToList();

                if (minions.Count() > 2)
                {
                    if (Items.HasItem(3077) && Items.CanUseItem(3077))
                        Items.UseItem(3077);
                    if (Items.HasItem(3074) && Items.CanUseItem(3074))
                        Items.UseItem(3074);
                    kiburst.Cast();
                }
            }

        }

        #endregion

        #region  AntiGapcloser
        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("enableAntiG").GetValue<bool>())
                return;

            if (gapcloser.Sender.Type == me.Type && gapcloser.Sender.IsValid)
                if (gapcloser.Sender.Distance(me.Position) < kiburst.Range && kiburst.IsReady())
                    kiburst.Cast();
        }
        #endregion

        #region  Interrupter
        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Base sender, InterruptableSpell spell)
        {
            if (!config.Item("interuppter").GetValue<bool>())
                return;

            if (sender.Type == me.Type && sender.IsValid && sender.Distance(me.Position) < wings.Range)
                if (wings.IsReady() && cleavecount == 2 && config.Item("interruptQ3").GetValue<bool>())
                    wings.Cast(sender.Position);

            if (sender.Type == me.Type && sender.IsValid && sender.Distance(me.Position) < kiburst.Range)
                if (kiburst.IsReady() && config.Item("interruptW").GetValue<bool>())
                    kiburst.Cast();
        }
        #endregion

        #region  OnProcessSpellCast

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var target = enemy;

            if (!sender.IsMe)
                return;

            switch (args.SData.Name)
            {
                case "RivenTriCleave":
                    qqt = Environment.TickCount;
                    if (cleavecount < 1)
                        ff = Game.Time + (13 + (13 * me.PercentCooldownMod));
                    if (wslash == 1 && cleavecount == 1)
                        blade.Cast(enemy.Position);

                    break;
                case "RivenMartyr":
                    Orbwalking.LastAATick = 0;
                    if (wings.IsReady() && usecombo)
                        Utility.DelayAction.Add(Game.Ping + 75, () => wings.Cast(target.ServerPosition));

                    if (wings.IsReady() && useclear)
                        Utility.DelayAction.Add(Game.Ping + 75, () => wings.Cast(orbwalker.GetTarget().Position));

                    break;
                case "ItemTiamatCleave":
                    Orbwalking.LastAATick = 0;
                    if (target.IsValidTarget(kiburst.Range) && usecombo)
                        if (wings.IsReady() && !kiburst.IsReady())
                            wings.Cast(enemy.ServerPosition);

                    if (wslash == 1 && blade.IsReady())
                        if (ultion && cleavecount == 2)
                            blade.Cast(enemy.ServerPosition);

                    break;
                case "RivenFeint":
                    eet = Environment.TickCount;
                    Orbwalking.LastAATick = 0;

                    if (usecombo)
                        castitems(target);

                    if (!useblade || !useautows)
                        return;

                    if (blade.IsReady() && usecombo)
                        if (ultion && wslash == 1)
                            blade.Cast(target.ServerPosition);
                    break;
                case "RivenFengShuiEngine":
                    Orbwalking.LastAATick = 0;

                    if (!usecombo)
                        return;

                    castitems(target);
                    if (!target.IsValidTarget(kiburst.Range))
                        return;

                    if (kiburst.IsReady())
                        kiburst.Cast();
                    break;
                case "rivenizunablade":
                    if (wings.IsReady())
                        wings.Cast(enemy.ServerPosition);
                    break;
            }
        }

        #endregion

        #region  OnProcessPacket
        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            var packet = new GamePacket(args.PacketData);

            // Headers: 0x9a
            if (packet.Header == 0x9a)
            {
                packet.Position = 2;
                var sourceId = packet.ReadInteger();
                if (sourceId != me.NetworkId)
                    return;

                if (config.Item("blockanim").GetValue<bool>())
                    args.Process = false;
            }

            // Headers: 0x23
            if (packet.Header == 0x23 && wings.IsReady())
            {
                var trees = Packet.S2C.Damage.Decoded(args.PacketData);

                var targetId = trees.TargetNetworkId;
                var damageType = trees.Type;

                var targ = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(targetId);
                if (targ.NetworkId == me.NetworkId)
                    return;

                if (damageType.ToString() != "54")
                    return;

                switch (targ.Type)
                {
                    case GameObjectType.obj_Barracks:
                    case GameObjectType.obj_AI_Turret:
                        if (!config.Item("clearkey").GetValue<KeyBind>().Active)
                            return;
                        castitems(targ);
                        wings.Cast(targ.ServerPosition, true);
                        break;
                    case GameObjectType.obj_AI_Hero:
                        if (config.Item("combokey").GetValue<KeyBind>().Active)
                            castitems(targ);
                        if (config.Item("combokey").GetValue<KeyBind>().Active)
                            wings.Cast(targ.ServerPosition, true);
                        if (config.Item("harasskey").GetValue<KeyBind>().Active)
                            wings.Cast(targ.ServerPosition, true);
                        break;
                    case GameObjectType.obj_AI_Minion:
                        if (!config.Item("clearkey").GetValue<KeyBind>().Active)
                            return;
                        if (jungleminions.Any(name => targ.Name.StartsWith(name)) && !targ.Name.Contains("Mini") &&
                            wings.IsReady() && config.Item("jungleQ").GetValue<bool>())
                            wings.Cast(targ.ServerPosition, true);
                        if (targ.Name.StartsWith("Minion") && config.Item("farmQ").GetValue<bool>())
                            wings.Cast(targ.ServerPosition, true);
                        if (valor.IsReady() && cleavecount >= 1 && config.Item("jungleE").GetValue<bool>())
                            valor.Cast(targ.ServerPosition, true);
                        break;
                }
            }
        }

        #endregion

        #region  Combo Logic

        private static void CastCombo(Obj_AI_Base target)
        {
            if (!target.IsValidTarget(950))
                return;

            var aHealthPercent = (int)((me.Health/me.MaxHealth)*100);

            if (me.Distance(target.ServerPosition) > truerange + 25 || 
                aHealthPercent <= config.Item("valorhealth").GetValue<Slider>().Value)
            {
                if (valor.IsReady() && config.Item("usevalor").GetValue<bool>())
                    valor.Cast(target.ServerPosition, true);
                if (wings.IsReady() && cleavecount <= 1)
                    CheckR(target);
            }

            if (wings.IsReady() && (valor.IsReady() || kiburst.IsReady())
                && me.Distance(target.Position) <= kiburst.Range - 10)
            {
                if (cleavecount <= 1)
                    CheckR(target);
            }

            if (blade.IsReady() && valor.IsReady() && ultion)
            {
                if (cleavecount == 2)
                    valor.Cast(target.ServerPosition, true);
            }

            if (me.Distance(target.Position) < kiburst.Range)
            {
                if (Items.HasItem(3077) && Items.CanUseItem(3077))
                    Items.UseItem(3077);
                if (Items.HasItem(3074) && Items.CanUseItem(3074))
                    Items.UseItem(3074);
                if (kiburst.IsReady())
                    Utility.DelayAction.Add(200, () => kiburst.Cast());
                    
            }

            if ((!valor.IsReady() || !config.Item("usevalor").GetValue<bool>()) &&
                wings.IsReady()  && me.Distance(target.ServerPosition) > me.AttackRange + 20)
            {
                if (TimeSpan.FromMilliseconds(qqt).TotalSeconds + extraqtime < now &&
                    TimeSpan.FromMilliseconds(eet).TotalSeconds + extraetime < now)
                {
                    wings.Cast(target.ServerPosition, true);
                }
            }

        }

        #endregion

        #region  Windlsash
        private static void WindSlash()
        {
            if (!ultion)
                return;

            foreach (var e in ObjectManager.Get<Obj_AI_Hero>().Where(e => e.IsValidTarget(blade.Range)))
            {
                var hitcount = config.Item("autows2").GetValue<Slider>().Value;

                var prediction = blade.GetPrediction(e, true);
                if (blade.IsReady() && useautows)
                {
                    if (wslash == 1 && prediction.AoeTargetsHitCount >= hitcount)
                        blade.Cast(prediction.CastPosition);

                    if (wslash == 1 && prediction.Hitchance >= HitChance.Medium)
                    {
                        if (rr / e.MaxHealth * 100 >= e.Health / e.MaxHealth * wsneed)
                            blade.Cast(prediction.CastPosition);

                        else if (e.Health < rr + ua * 1 + uq * 1 && enemy.Distance(me.Position) < wings.Range + 50)
                        {
                            blade.Cast(prediction.CastPosition);
                        }
                        else if (enemy.Distance(me.Position) <= blade.Range && e.Health <= rr)
                        {
                            blade.Cast(prediction.CastPosition);
                        }
                    }
                    else if (wslash == 0)
                    {
                        if (prediction.Hitchance >= HitChance.Medium && e.Health <= rr)
                            blade.Cast(prediction.CastPosition);
                    }
                }
            }
        }

        #endregion

        #region  Item Handler
        private static void castitems(Obj_AI_Base target)
        {
            foreach (var i in items.Where(i => Items.CanUseItem(i) && Items.HasItem(i)))
            {
                if (target.IsValidTarget(valor.Range + blade.Range))
                    Items.UseItem(i);
            }
        }
        #endregion

        #region  Damage Handler
        private static void CheckDamage(Obj_AI_Base target)
        {
            if (target == null) return;

            var ignite = me.GetSpellSlot("summonerdot");
            double aaa = me.GetAutoAttackDamage(target);

            double tmt = Items.HasItem(3077) && Items.CanUseItem(3077) ? me.GetItemDamage(target, Damage.DamageItems.Tiamat) : 0;
            double hyd = Items.HasItem(3074) && Items.CanUseItem(3074) ? me.GetItemDamage(target, Damage.DamageItems.Hydra) : 0;
            double bwc = Items.HasItem(3144) && Items.CanUseItem(3144) ? me.GetItemDamage(target, Damage.DamageItems.Bilgewater) : 0;
            double brk = Items.HasItem(3153) && Items.CanUseItem(3153) ? me.GetItemDamage(target, Damage.DamageItems.Botrk) : 0;

            rr = me.GetSpellDamage(target, SpellSlot.R);
            ra = aaa + (aaa * (runicpassive[me.Level] / 100));
            rq = wings.IsReady() ? DamageQ(target) : 0;
            rw = kiburst.IsReady() ? me.GetSpellDamage(target, SpellSlot.W) : 0;
            ri = me.Spellbook.CanUseSpell(ignite) == SpellState.Ready ? me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) : 0;

            ritems = tmt + hyd + bwc + brk;

            ua = blade.IsReady()
                ? ra +
                  me.CalcDamage(target, Damage.DamageType.Physical,
                      me.BaseAttackDamage + me.FlatPhysicalDamageMod * 0.2)
                : ua;

            uq = blade.IsReady()
                ? rq +
                  me.CalcDamage(target, Damage.DamageType.Physical,
                      me.BaseAttackDamage + me.FlatPhysicalDamageMod * 0.2 * 0.7)
                : uq;

            uw = blade.IsReady()
                ? rw +
                  me.CalcDamage(target, Damage.DamageType.Physical,
                      me.BaseAttackDamage + me.FlatPhysicalDamageMod * 0.2 * 1)
                : uw;

            rr = blade.IsReady()
                ? rr +
                  me.CalcDamage(target, Damage.DamageType.Physical,
                      me.BaseAttackDamage + me.FlatPhysicalDamageMod * 0.2)
                : rr;
        }

        public static float DamageQ(Obj_AI_Base target)
        {
            double dmg = 0;
            if (wings.IsReady())
            {
                dmg += me.CalcDamage(target, Damage.DamageType.Physical,
                    -10 + (wings.Level * 20) +
                    (0.35 + (wings.Level * 0.05)) * (me.FlatPhysicalDamageMod + me.BaseAttackDamage));
            }

            return (float)dmg;
        }

        #endregion

        #region  Ultimate Handler
        private static void CheckR(Obj_AI_Base target)
        {
            if (useblade && usecombo)
            {
                switch (bladewhen)
                {
                    case 2:
                        if ((float)(ua * 3 + uq * 3 + uw + rr + ri + ritems) >= target.Health && !ultion)
                        {
                            if (target.Health <= (float)(ra * 2 + rq * 2 + ri + ritems))
                                if (config.Item("checkover").GetValue<bool>())
                                    return;

                            blade.Cast();

                            if (config.Item("useignote").GetValue<bool>() && blade.IsReady())
                                if (cleavecount <= 1 && ultion)
                                    CastIgnite(target);
                        }
                        break;
                    case 1:
                        if ((float)(ra * 3 + rq * 3 + rw + rr + ri + ritems) >= target.Health && !ultion)
                        {
                            if (target.Health <= (float)(ra * 2 + rq * 2 + ri + ritems))
                                if (config.Item("checkover").GetValue<bool>())
                                    return;

                            blade.Cast();
                            if (config.Item("useignote").GetValue<bool>() && blade.IsReady())
                                if (cleavecount <= 1 && ultion)
                                    CastIgnite(target);
                        }
                        break;
                    case 0:
                        if ((float)(ra * 2 + rq * 2 + rw + rr + ri + ritems) >= target.Health && !ultion)
                        {
                            blade.Cast();
                        }
                        break;
                }
            }
        }

        #endregion

        #region  Ignote Handler
        private static void CastIgnite(Obj_AI_Base target)
        {
            if (target.IsValidTarget(600))
            {
                var ignote = me.GetSpellSlot("summonerdot");
                if (me.Spellbook.CanUseSpell(ignote) == SpellState.Ready)
                {
                    me.Spellbook.CastSpell(ignote, target);
                }
            }
        }
        #endregion

        #region  AutoW
        private void AutoW()
        {
            var getenemies = ObjectManager.Get<Obj_AI_Hero>().Where(en => en.IsValidTarget(kiburst.Range));
            if (getenemies.Count() >= config.Item("autow").GetValue<Slider>().Value)
            {
                if (kiburst.IsReady() && config.Item("useautow").GetValue<bool>())
                {
                    kiburst.Cast();
                }
            }

        }

        #endregion

    }
}

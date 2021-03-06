﻿using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterSeries.Common.M_Orbwalker;

namespace MasterSeries.Champions
{
    class Nasus : Program
    {
        private int Sheen = 3057, Iceborn = 3025, Trinity = 3078;

        public Nasus()
        {
            Q = new Spell(SpellSlot.Q, 300);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 20);
            Q.SetTargetted(0.0435f, float.MaxValue);
            E.SetSkillshot(0.5f, 380, float.MaxValue, false, SkillshotType.SkillshotCircle);

            var ChampMenu = new Menu("插件", Name + "Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用 Q");
                    ItemList(ComboMenu, "QMode", "-> 模式", new[] { "总是", "智能" }, 1);
                    ItemBool(ComboMenu, "W", "使用 W");
                    ItemBool(ComboMenu, "E", "使用 E");
                    ItemBool(ComboMenu, "Item", "使用物品");
                    ItemBool(ComboMenu, "Ignite", "如果能击杀自动点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
               var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "Q", "使用 Q");
                    ItemBool(HarassMenu, "W", "使用 W");
                    ItemBool(HarassMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线/清野", "Clear");
                {
                    var SmiteMob = new Menu("如果惩戒能击杀野怪", "SmiteMob");
                    {
                        ItemBool(SmiteMob, "Baron", "大龙");
                        ItemBool(SmiteMob, "Dragon", "小龙");
                        ItemBool(SmiteMob, "Red", "红BUFF");
                        ItemBool(SmiteMob, "Blue", "蓝BUFF");
                        ItemBool(SmiteMob, "Krug", "石头怪");
                        ItemBool(SmiteMob, "Gromp", "大蛤蟆");
                        ItemBool(SmiteMob, "Raptor", "啄木鸟4兄弟");
                        ItemBool(SmiteMob, "Wolf", "幽灵狼3兄弟");
                        ClearMenu.AddSubMenu(SmiteMob);
                    }
                    ItemBool(ClearMenu, "Q", "使用 Q");
                    ItemBool(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var UltiMenu = new Menu("大招", "Ultimate");
                {
                    ItemBool(UltiMenu, "RSurvive", "尝试使用R求生");
                    ItemSlider(UltiMenu, "RUnder", "-> 如果血量低于", 30);
                    ChampMenu.AddSubMenu(UltiMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    ItemBool(MiscMenu, "QLastHit", "使用Q补刀");
                    ItemBool(MiscMenu, "EKillSteal", "使用E抢人头");
                    ItemSlider(MiscMenu, "CustomSkin", "失效-换肤", 5, 0, 5).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    ItemBool(DrawMenu, "W", "W 范围", false);
                    ItemBool(DrawMenu, "E", "E 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                Config.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Base.OnProcessSpellCast += TrySurviveSpellCast;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsChannelingImportantSpell() || Player.IsRecalling()) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo(Orbwalk.CurrentMode.ToString());
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear)
            {
                LaneJungClear();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit) LastHit();
            if (ItemBool("Ultimate", "RSurvive") && R.IsReady()) TrySurvive(R.Slot, ItemSlider("Ultimate", "RUnder"));
            if (ItemBool("Misc", "EKillSteal")) KillSteal();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (ItemBool("Draw", "W") && W.Level > 0) Utility.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "E") && E.Level > 0) Utility.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.SData.IsAutoAttack() && ((Obj_AI_Base)args.Target).IsValidTarget(Orbwalk.GetAutoAttackRange(Player, (Obj_AI_Base)args.Target) + 20) && Q.IsReady())
            {
                var Obj = (Obj_AI_Base)args.Target;
                var DmgAA = Player.GetAutoAttackDamage(Obj) * Math.Floor(Q.Instance.Cooldown / 1 / Player.AttackDelay);
                if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit && ItemBool("Misc", "QLastHit") && CanKill(Obj, Q, GetBonusDmg(Obj)) && args.Target is Obj_AI_Minion)
                {
                    Q.Cast(PacketCast());
                }
                else if (((Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear && ItemBool("Clear", "Q") && (args.Target is Obj_AI_Minion || args.Target is Obj_AI_Turret)) || (Orbwalk.CurrentMode == Orbwalk.Mode.Combo && ItemBool("Combo", "Q") && ItemList("Combo", "QMode") == 1 && args.Target is Obj_AI_Hero)) && (CanKill(Obj, Q, GetBonusDmg(Obj)) || (!(args.Target is Obj_AI_Turret) && !CanKill(Obj, Q, GetBonusDmg(Obj) + DmgAA))))
                {
                    Q.Cast(PacketCast());
                }
                else if ((Orbwalk.CurrentMode == Orbwalk.Mode.Harass || (Orbwalk.CurrentMode == Orbwalk.Mode.Combo && ItemList("Combo", "QMode") == 0)) && ItemBool(Orbwalk.CurrentMode.ToString(), "Q") && args.Target is Obj_AI_Hero) Q.Cast(PacketCast());
            }
        }

        private void NormalCombo(string Mode)
        {
            if (targetObj == null) return;
            if (ItemBool(Mode, "Q") && Q.IsReady() && Player.Distance3D(targetObj) <= Orbwalk.GetAutoAttackRange(Player, targetObj) + 20)
            {
                var DmgAA = Player.GetAutoAttackDamage(targetObj) * Math.Floor(Q.Instance.Cooldown / 1 / Player.AttackDelay);
                if (Mode == "Harass" || (Mode == "Combo" && (ItemList(Mode, "QMode") == 0 || (ItemList(Mode, "QMode") == 1 && (CanKill(targetObj, Q, GetBonusDmg(targetObj)) || !CanKill(targetObj, Q, GetBonusDmg(targetObj) + DmgAA))))))
                {
                    Orbwalk.SetAttack(false);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, targetObj);
                    Orbwalk.SetAttack(true);
                }
            }
            if (ItemBool(Mode, "W") && W.CanCast(targetObj) && (Mode == "Combo" || (Mode == "Harass" && Player.Distance3D(targetObj) <= Orbwalk.GetAutoAttackRange(Player, targetObj) + 100))) W.CastOnUnit(targetObj, PacketCast());
            if (ItemBool(Mode, "E") && E.CanCast(targetObj) && (Mode == "Combo" || (Mode == "Harass" && Player.Distance3D(targetObj) <= Orbwalk.GetAutoAttackRange(Player, targetObj) + 100))) E.Cast(targetObj.Position.Randomize(0, (int)E.Width / 2), PacketCast());
            if (Mode == "Combo" && ItemBool(Mode, "Item") && Items.CanUseItem(Randuin) && Player.CountEnemysInRange(450) >= 1) Items.UseItem(Randuin);
            if (Mode == "Combo" && ItemBool(Mode, "Ignite") && IgniteReady()) CastIgnite(targetObj);
        }

        private void LaneJungClear()
        {
            var towerObj = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(i => i.IsValidTarget(Orbwalk.GetAutoAttackRange(Player, i) + 20) && CanKill(i, Q, GetBonusDmg(i)));
            if (towerObj != null && Q.IsReady())
            {
                Orbwalk.SetAttack(false);
                Player.IssueOrder(GameObjectOrder.AttackUnit, towerObj);
                Orbwalk.SetAttack(true);
            }
            var minionObj = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            foreach (var Obj in minionObj)
            {
                if (SmiteReady() && Obj.Team == GameObjectTeam.Neutral)
                {
                    if ((ItemBool("SmiteMob", "Baron") && Obj.Name.StartsWith("SRU_Baron")) || (ItemBool("SmiteMob", "Dragon") && Obj.Name.StartsWith("SRU_Dragon")) || (!Obj.Name.Contains("Mini") && (
                        (ItemBool("SmiteMob", "Red") && Obj.Name.StartsWith("SRU_Red")) || (ItemBool("SmiteMob", "Blue") && Obj.Name.StartsWith("SRU_Blue")) ||
                        (ItemBool("SmiteMob", "Krug") && Obj.Name.StartsWith("SRU_Krug")) || (ItemBool("SmiteMob", "Gromp") && Obj.Name.StartsWith("SRU_Gromp")) ||
                        (ItemBool("SmiteMob", "Raptor") && Obj.Name.StartsWith("SRU_Razorbeak")) || (ItemBool("SmiteMob", "Wolf") && Obj.Name.StartsWith("SRU_Murkwolf"))))) CastSmite(Obj);
                }
                if (ItemBool("Clear", "Q") && Q.IsReady() && Player.Distance3D(Obj) <= Orbwalk.GetAutoAttackRange(Player, Obj) + 20)
                {
                    var DmgAA = Player.GetAutoAttackDamage(Obj) * Math.Floor(Q.Instance.Cooldown / 1 / Player.AttackDelay);
                    if (CanKill(targetObj, Q, GetBonusDmg(targetObj)) || !CanKill(Obj, Q, GetBonusDmg(Obj) + DmgAA))
                    {
                        Orbwalk.SetAttack(false);
                        Player.IssueOrder(GameObjectOrder.AttackUnit, Obj);
                        Orbwalk.SetAttack(true);
                        break;
                    }
                }
                if (ItemBool("Clear", "E") && E.IsReady() && (minionObj.Count >= 2 || Obj.MaxHealth >= 1200))
                {
                    var posEFarm = E.GetCircularFarmLocation(minionObj);
                    E.Cast(posEFarm.MinionsHit >= 2 ? posEFarm.Position : Obj.Position.Randomize(0, (int)E.Width / 2).To2D(), PacketCast());
                }
            }
        }

        private void LastHit()
        {
            if (!ItemBool("Misc", "QLastHit") || !Q.IsReady()) return;
            foreach (var Obj in MinionManager.GetMinions(Orbwalk.GetAutoAttackRange() + 100, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).Where(i => CanKill(i, Q, GetBonusDmg(i))).OrderByDescending(i => i.Distance3D(Player)))
            {
                Orbwalk.SetAttack(false);
                Player.IssueOrder(GameObjectOrder.AttackUnit, Obj);
                Orbwalk.SetAttack(true);
                break;
            }
        }

        private void KillSteal()
        {
            if (!E.IsReady()) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(E.Range) && CanKill(i, E) && i != targetObj).OrderBy(i => i.Health).OrderByDescending(i => i.Distance3D(Player))) E.Cast(Obj.Position.Randomize(0, (int)E.Width / 2), PacketCast());
        }

        private double GetBonusDmg(Obj_AI_Base Target)
        {
            double DmgItem = 0;
            if (Items.HasItem(Sheen) && ((Items.CanUseItem(Sheen) && Q.IsReady()) || Player.HasBuff("Sheen")) && Player.BaseAttackDamage > DmgItem) DmgItem = Player.BaseAttackDamage;
            if (Items.HasItem(Iceborn) && ((Items.CanUseItem(Iceborn) && Q.IsReady()) || Player.HasBuff("ItemFrozenFist")) && Player.BaseAttackDamage * 1.25 > DmgItem) DmgItem = Player.BaseAttackDamage * 1.25;
            if (Items.HasItem(Trinity) && ((Items.CanUseItem(Trinity) && Q.IsReady()) || Player.HasBuff("Sheen")) && Player.BaseAttackDamage * 2 > DmgItem) DmgItem = Player.BaseAttackDamage * 2;
            return (Q.IsReady() ? Q.GetDamage(Target) : 0) + Player.GetAutoAttackDamage(Target, Q.IsReady() ? false : true) + Player.CalcDamage(Target, Damage.DamageType.Physical, DmgItem);
        }
    }
}
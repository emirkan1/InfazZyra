using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;


namespace InfazZyra
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static AIHeroClient myHero { get { return ObjectManager.Player; } }
        public static Menu Menu, SkillMenu, FarmingMenu, MiscMenu, DrawMenu, ZhonyaMenu;
        public static Item Zhonia;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Zyra")
                return;

            Bootstrap.Init(null);
            // Skille                // Range skili V
            Q = new Spell.Skillshot(SpellSlot.Q, 800, SkillShotType.Circular);
            W = new Spell.Skillshot(SpellSlot.W, 850, SkillShotType.Circular);
            E = new Spell.Skillshot(SpellSlot.E, 1100, SkillShotType.Linear);
            R = new Spell.Skillshot(SpellSlot.R, 700, SkillShotType.Circular);
            // Menu
            Menu = MainMenu.AddMenu("InfazZyra", "InfazZyra");
            Menu.AddGroupLabel("InfazZyra");

            Menu.AddSeparator();

            Menu.AddLabel("Bol şans");
            SkillMenu = Menu.AddSubMenu("Skills", "Skills");
            SkillMenu.AddGroupLabel("Skills");
            SkillMenu.AddLabel("Combo");
            SkillMenu.Add("QCombo", new CheckBox("Use Q on Combo"));
            SkillMenu.Add("WCombo", new CheckBox("Use W on Combo"));
            SkillMenu.Add("ECombo", new CheckBox("Use E on Combo"));
            SkillMenu.Add("RCombo", new CheckBox("Use R on Combo"));
            // SkillMenu.Add("rslider", new Slider("Minimum people for R", 2, 0, 5));

            SkillMenu.AddLabel("Harass");
            SkillMenu.Add("QHarass", new CheckBox("Use Q on Harass"));
            SkillMenu.Add("WHarass", new CheckBox("Use W on Harass"));

            FarmingMenu = Menu.AddSubMenu("Farming", "Farming");
            FarmingMenu.AddGroupLabel("Farming");
            FarmingMenu.AddLabel("LastHit");
            FarmingMenu.Add("Qlasthit", new CheckBox("Use Q on LastHit"));
            FarmingMenu.Add("QlasthitMana", new Slider("Mana % To Use Q", 30, 0, 100));
            FarmingMenu.Add("Wlasthit", new CheckBox("Use W on LastHit"));
            FarmingMenu.Add("WlasthitMana", new Slider("Mana % To Use W", 30, 0, 100));

            FarmingMenu.AddLabel("LaneClear");
            FarmingMenu.Add("QLaneClear", new CheckBox("Use Q on LaneClear"));
            FarmingMenu.Add("QlaneclearMana", new Slider("Mana % To Use Q", 30, 0, 100));
            FarmingMenu.Add("ELaneClear", new CheckBox("Use E on LaneClear"));
            FarmingMenu.Add("ElaneclearMana", new Slider("Mana % To Use E", 30, 0, 100));

            MiscMenu = Menu.AddSubMenu("Misc", "Misc");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddLabel("KillSteal");
            MiscMenu.Add("Qkill", new CheckBox("Use Q KillSteal"));
            MiscMenu.Add("Ekill", new CheckBox("Use E KillSteal"));           
            MiscMenu.AddLabel("Auto Zhonya");
            Zhonia = new Item((int)ItemId.Zhonyas_Hourglass);
            ZhonyaMenu = MainMenu.AddMenu("Zhonya", "zhonyamenu");
            ZhonyaMenu.Add("zhonya", new CheckBox("Use Zhonya"));
            ZhonyaMenu.Add("zhealth", new Slider("Auto Zhonia Health %", 28));

            DrawMenu = Menu.AddSubMenu("Drawings", "Drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddLabel("Drawings");
            DrawMenu.Add("drawQ", new CheckBox("Draw Q"));
            DrawMenu.Add("drawW", new CheckBox("Draw W"));
            DrawMenu.Add("drawE", new CheckBox("Draw E"));
            DrawMenu.Add("drawR", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Chat.Print("Infaz Zyra loaded & Tf ", Color.Green);

        }
        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            KillSteal();
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null) return;
            var useQ = SkillMenu["QCombo"].Cast<CheckBox>().CurrentValue;
            Q.AllowedCollisionCount = int.MaxValue;
            var QPred = Prediction.Position.PredictLinearMissile(target, Q.Range, Q.Radius, Q.CastDelay, Q.Speed, int.MaxValue, myHero.ServerPosition);
            var EPred = Prediction.Position.PredictCircularMissile(target, E.Range, E.Radius, E.CastDelay, E.Speed, myHero.ServerPosition);
            var useW = SkillMenu["WCombo"].Cast<CheckBox>().CurrentValue;
            var useE = SkillMenu["ECombo"].Cast<CheckBox>().CurrentValue;
            var useR = SkillMenu["RCombo"].Cast<CheckBox>().CurrentValue;

            if (target.IsValidTarget(Q.Range))
            
            {
                if (E.IsReady() && useE && EPred.HitChance >= HitChance.High && target.IsValidTarget(E.Range) && !target.IsZombie && !target.IsDead)
                {
                    E.Cast(target);
                }
                if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsZombie)
                {
                    W.Cast(target);
                }
                if (Q.IsReady() && useQ && QPred.HitChance >= HitChance.High && target.IsValidTarget(Q.Range) && !target.IsZombie)
                {
                    Q.Cast(target);
                }
                if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsZombie)
                {
                    W.Cast(target);
                }
                if (R.IsReady() && useR && target.IsValidTarget(R.Range) && !target.IsZombie)
                {
                    R.Cast(target);
                }
            }
        }

        private static void Zhonya()
        {
            var zhoniaon = ZhonyaMenu["zhonya"].Cast<CheckBox>().CurrentValue;
            var zhealth = ZhonyaMenu["zhealth"].Cast<Slider>().CurrentValue;

            if (zhoniaon && Zhonia.IsReady() && Zhonia.IsOwned())
            {
                if (_Player.HealthPercent <= zhealth)
                {
                    Zhonia.Cast();
                }
            }
        }

        private static void Tick(EventArgs args)
        {
            Zhonya();
        }

        private static void KillSteal()
        
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null) return;
            var useQ = MiscMenu["Qkill"].Cast<CheckBox>().CurrentValue;
            var useE = MiscMenu["Ekill"].Cast<CheckBox>().CurrentValue;
            var useR = MiscMenu["Rkill"].Cast<CheckBox>().CurrentValue;

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie && !target.IsInvulnerable && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q))
            {
                Q.Cast(target);
            }
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie && !target.IsInvulnerable && target.Health <= _Player.GetSpellDamage(target, SpellSlot.E))
            {
                W.Cast(target);
            }
            if (useR && R.IsReady() && target.IsValidTarget(R.Range) && !target.IsZombie && !target.IsInvulnerable && target.Health <= _Player.GetSpellDamage(target, SpellSlot.R))
            {
                R.Cast(target);
            }
        }
        private static void Harass()
       
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null) return;
            var useQ = SkillMenu["QHarass"].Cast<CheckBox>().CurrentValue;
            var useW = SkillMenu["WHarass"].Cast<CheckBox>().CurrentValue;

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range) && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsZombie)
            {
                W.Cast(target);
            }
        }
        private static void LaneClear()
        
        {
            var useQ = FarmingMenu["QLaneClear"].Cast<CheckBox>().CurrentValue;
            var Qmana = FarmingMenu["QlaneclearMana"].Cast<Slider>().CurrentValue;
            var useW = FarmingMenu["WLaneClear"].Cast<CheckBox>().CurrentValue;
            var Wmana = FarmingMenu["WlaneclearMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > Qmana && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
                if (useW && W.IsReady() && Player.Instance.ManaPercent > Wmana && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.W))
                {
                    W.Cast(minion);
                }
            }
        }
        private static void LastHit()
        {
            var useQ = FarmingMenu["Qlasthit"].Cast<CheckBox>().CurrentValue;
            var Qmana = FarmingMenu["QlasthitMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Yellow, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Green, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}
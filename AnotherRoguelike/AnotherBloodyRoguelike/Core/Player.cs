﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp.DiceNotation;
using AnotherRoguelike.Systems;

namespace AnotherRoguelike.Core
{
    public class Player : Actor
    {
        private int level = 1;
        public int floor = 1;
        public int Kills { get; set; }
        public Player()
        {
            Attack = 2;
            AttChance = 65;
            Defense = 2;
            DefChance = 40;
            Gold = 0;
            MaxHealth = 25;
            Health = MaxHealth;
            Awareness = 15;
            Xp = 0;
            MaxXp = 30;
            Speed = 4;
            Name = "Delver";
            Color = Colors.Player;
            Symbol = '@';
        }

        public void DrawStats(RLConsole statConsole)
        {
            int hpWidth = Convert.ToInt32(((double)Health / (double)MaxHealth) * 15.0);
            int remainingHpWidth = 15 - hpWidth;

            int xpWidth = Convert.ToInt32(((double)Xp / (double)MaxXp) * 15.0);
            int remainingXpWidth = 15 - xpWidth;

            statConsole.Print(1, 1, $"{Name}", Colors.Text);
            statConsole.Print(15, 1, $"Lv.{level}", RLColor.Green);
            statConsole.Print(1, 3, $"HP: {Health}/{MaxHealth}", Colors.Text);
            statConsole.Print(1, 5, $"XP: {Xp}/{MaxXp}", RLColor.White);
            statConsole.Print(1, 7, $"A: {Attack}/{AttChance}% D: {Defense}/{DefChance}%", Colors.Text);
            //statConsole.Print(1, 9, $"", Colors.Text);
            statConsole.Print(1, 9, $"$:{Gold}", Colors.Gold);
            statConsole.Print(13, 9, $"Flr {floor}",RLColor.White);

            statConsole.SetBackColor(4, 3, hpWidth, 1, RLColor.Red);
            statConsole.SetBackColor(4 + hpWidth, 3, remainingHpWidth, 1, RLColor.Gray);

            statConsole.SetBackColor(4, 5, xpWidth, 1, RLColor.Green);
            statConsole.SetBackColor(4 + xpWidth, 5, remainingXpWidth, 1, RLColor.Gray);
        }

        public void DrawInventory(RLConsole invConsole)
        {
            invConsole.Print(1, 1, "Equipment", RLColor.White);
            invConsole.Print(1, 3, $"Head: {Head.Name}", RLColor.LightGray);
            invConsole.Print(1, 5, $"Body: {Body.Name}", RLColor.LightGray);
            invConsole.Print(1, 7, $"Weapon: {Wep.Name}", RLColor.LightGray);
            invConsole.Print(1, 9, $"Shield: {Shie.Name}", RLColor.LightGray);
            
            invConsole.Print(28, 1, "Abilities", RLColor.White);
            /*invConsole.Print(28, 3, "Q - Charge", RLColor.LightGray);
            invConsole.Print(28, 5, "W - Whirlwind Attack", RLColor.LightGray);
            invConsole.Print(28, 7, "E - Fireball", RLColor.LightGray);
            invConsole.Print(28, 9, "R - Lightning Bolt", RLColor.LightGray);*/
            
            invConsole.Print(55, 1, "Items", RLColor.White);
            /*invConsole.Print(55, 3, "1 - Health Potion", RLColor.LightGray);
            invConsole.Print(55, 5, "2 - Mana Potion", RLColor.LightGray);
            invConsole.Print(55, 7, "3 - Scroll", RLColor.LightGray);
            invConsole.Print(55, 9, "4 - Wand", RLColor.LightGray);*/
        }

        public void CheckStatus()
        {
            //Anything that needs to be checked per-turn
            if (Xp >= MaxXp)
            {
                level++;
                Game.MessageLog.Add($"You leveled up! You're now level {level}!");
                MaxHealth += Dice.Roll("1D8") + level / 2;
                if (level % 2 == 0) Attack += Dice.Roll("1D2") + level / 3;
                if (level % 4 == 0) Defense += Dice.Roll("1D2") + level / 4;
                Xp = Xp - MaxXp;
                MaxXp += Dice.Roll("1D10") + level * 2;
                Health += (MaxHealth / 4);
                if (Health > MaxHealth) Health = MaxHealth;
            }

            if (Health > MaxHealth) Health = MaxHealth;

            //Check for status effects later
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using AnotherRoguelike.Core;
using AnotherRoguelike.Systems;
using RogueSharp.Random;

namespace AnotherRoguelike
{
    // 7-12-17 Nick Hilger
    //Thank you Faron Bracy. https://roguesharp.wordpress.com/
    public class Game
    {
        public static DungeonMap DungeonMap { get; private set; }

        public static Player Player { get; set; }

        public static MessageLog MessageLog { get; private set; }

        public static SchedulingSystem SchedulingSystem { get; private set; }

        public int Floor { get; set; }

        public static int steps = 0;

        // Singleton of IRandom used throughout the game when generating random numbers
        public static IRandom Random { get; private set; }

        private static bool renderReq = true;

        public static CommandSystem CommandSystem { get; private set; }

        //Screen height and width (number of tiles)
        private static readonly int scrnWidth = 100;
        private static readonly int scrnHeight = 70;
        private static RLRootConsole rootConsole;

        //Map console takes up most of the screen, map is drawn here
        private static readonly int mapWidth = 80;
        private static readonly int mapHeight = 48;
        private static RLConsole mapConsole;

        //Message console, below the map console
        private static readonly int msgWidth = 80;
        private static readonly int msgHeight = 11;
        private static RLConsole msgConsole;

        //Stats console, right of the map
        private static readonly int statsWidth = 20;
        private static readonly int statsHeight = 70;
        private static RLConsole statsConsole;

        //Inventory console, above map
        private static readonly int invWidth = 80;
        private static readonly int invHeight = 11;
        private static RLConsole invConsole;

        public static string PlayerName { get; private set; }

        static void Main(string[] args)
        {
            // Establish the seed for the random number generator from the current time
            int seed = (int)DateTime.UtcNow.Ticks;
            Random = new DotNetRandom(seed);

            //This has to be the name of the bitmap font file
            string fontFileName = "terminal8x8.png";
            
            //Title appears in console window, includes seed
            string consoleTitle = $"Another {Adjectives.AdjGen()} Roguelike";
            
            //Tell RLNet to use the bitmap font and that each tile is 8x8 pix
            rootConsole = new RLRootConsole(fontFileName, scrnWidth, scrnHeight, 8, 8, 1f, consoleTitle);

            //Enter name
            rootConsole.Print(30, 35, "What's yer name, traveler?", RLColor.White);
            PlayerName = Console.ReadLine();
            rootConsole.Print(50, 35, PlayerName, RLColor.White);


            //Initialize sub consoles
            mapConsole = new RLConsole(mapWidth, mapHeight);
            msgConsole = new RLConsole(msgWidth, msgHeight);
            statsConsole = new RLConsole(statsWidth, statsHeight);
            invConsole = new RLConsole(invWidth, invHeight);

            SchedulingSystem = new SchedulingSystem();

            //Generate map
            MapGenerator mapGen = new MapGenerator(mapWidth, mapHeight, 24, 14, 5, 1);
            DungeonMap = mapGen.CreateMap();
            DungeonMap.UpdatePlayerFOV();
            CommandSystem = new CommandSystem();

            //Set up handler for RLNet's Update event
            rootConsole.Update += OnRootConsoleUpdate;
            
            //Set up a handler for RLnet's Render event
            rootConsole.Render += OnRootConsoleRender;

            //Set background and text for each console
            mapConsole.SetBackColor(0, 0, mapWidth, mapHeight, Colors.FloorBackground);

            MessageLog = new MessageLog();
            MessageLog.Add("You arrive on Floor 1");
            MessageLog.Add($"Your floor seed: {seed}");

            //invConsole.SetBackColor(0, 0, invWidth, invHeight, Palette.DbWood);
            //invConsole.Print(1, 1, "Inventory", Colors.TextHeading);

            //Begin the game loop
            rootConsole.Run();
        }

        //Event handler for Update event
        private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e)
        {
            bool didPlayerAct = false;
            RLKeyPress keyPress = rootConsole.Keyboard.GetKeyPress();
            if (CommandSystem.IsPlayerTurn)
            {
                if (keyPress != null)
                {
                    if (Player.Health > 0)
                    {
                        if (keyPress.Key == RLKey.Up) didPlayerAct = CommandSystem.MovePlayer(Direction.Up);
                        else if (keyPress.Key == RLKey.Down) didPlayerAct = CommandSystem.MovePlayer(Direction.Down);
                        else if (keyPress.Key == RLKey.Left) didPlayerAct = CommandSystem.MovePlayer(Direction.Left);
                        else if (keyPress.Key == RLKey.Right) didPlayerAct = CommandSystem.MovePlayer(Direction.Right);
                        else if (keyPress.Key == RLKey.Period)
                        {
                            if (DungeonMap.CanMoveToNextFloor())
                            {
                                MapGenerator mapGenerator = new MapGenerator(mapWidth, mapHeight, 24, 14, 5, ++Player.floor);
                                DungeonMap = mapGenerator.CreateMap();
                                MessageLog = new MessageLog();
                                CommandSystem = new CommandSystem();
                                //rootConsole.Title = $"Another...Roguelike";
                                MessageLog.Add($"You have reached Floor {Player.floor}");
                                didPlayerAct = true;
                            }
                        }
                    }
                    if (keyPress.Key == RLKey.Escape) rootConsole.Close();
                }

                if (didPlayerAct)
                {
                    steps++;
                    Player.CheckStatus();
                    //MessageLog.Add($"Step " + steps);
                    renderReq = true;
                    CommandSystem.EndPlayerTurn();
                }
            }
            else
            {
                CommandSystem.ActivateMonsters();
                renderReq = true;
            }
        }

        //Event handler for Render event
        private static void OnRootConsoleRender(object sender, UpdateEventArgs e)
        {
            //Move blits back here if need be
            //Only redraw the console if something has changed
            if (renderReq)
            {
                mapConsole.Clear();
                statsConsole.Clear();
                msgConsole.Clear();

                //Draw the map itself
                DungeonMap.Draw(mapConsole,statsConsole);

                //Draw YOU
                Player.Draw(mapConsole, DungeonMap);
                Player.DrawInventory(invConsole);

                //Draw the message log
                MessageLog.Draw(msgConsole);

                //Draws player stats
                Player.DrawStats(statsConsole);

                //Blit the sub consoles to the root console
                RLConsole.Blit(mapConsole, 0, 0, mapWidth, mapHeight, rootConsole, 0, invHeight);
                RLConsole.Blit(statsConsole, 0, 0, statsWidth, statsHeight, rootConsole, mapWidth, 0);
                RLConsole.Blit(msgConsole, 0, 0, msgWidth, msgHeight, rootConsole, 0, scrnHeight - msgHeight);
                RLConsole.Blit(invConsole, 0, 0, invWidth, invHeight, rootConsole, 0, 0);

                //Tell RLNet to draw the console that we set
                rootConsole.Draw();

                renderReq = false;
            }
        }
    }
}

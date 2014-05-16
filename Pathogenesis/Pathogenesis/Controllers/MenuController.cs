using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Pathogenesis.Controllers
{
    public enum MainMenuState
    {
        NORMAL,
        INFECTING,
        INFECTED
    }

    public class MenuController
    {
        private Dictionary<MenuType, Menu> menus;
        private Dictionary<int, Menu> dialogues;
        public List<Menu> OpenMenus;

        // Returns the current menu
        public String curSelection;
        public Menu CurMenu
        {
            get
            {
                return OpenMenus.Last();
            }
        }

        // Returns the current dialogue id
        public int CurDialogue { get; set; }

        // Main menu data
        public MainMenuState MainMenuState { get; set; }
        public Stopwatch mainmenu_stopwatch { get; set; }
        public Stopwatch mainfade_stopwatch { get; set; }

        public const int MAIN_FADE_TIME = 1000;
        public const int MAIN_INFECTING_TIME = 2300;
        public const int MAIN_INFECTED_TIME = 1000;

        private SoundController sound_controller;
        private GameEngine engine;

        public MenuController(GameEngine engine, ContentFactory factory, SoundController sound_controller)
        {
            menus = factory.getMenus();
            dialogues = factory.getDialogues();
            OpenMenus = new List<Menu>();
            mainmenu_stopwatch = new Stopwatch();
            mainfade_stopwatch = new Stopwatch();

            this.sound_controller = sound_controller;
            this.engine = engine;
        }

        /*
         * Set the current menu to the specified type
         */
        public void LoadMenu(MenuType type)
        {
            if (OpenMenus.Count > 0)
            {
                Menu current = OpenMenus.Last();
                if (!current.Children.Contains(type))
                {
                    OpenMenus.Clear();
                }
            }
            OpenMenus.Add(menus[type]);
            if (type == MenuType.MAIN)
            {
                MainMenuState = MainMenuState.NORMAL;
                CurMenu.SetAnimation("heart");
                CurMenu.MASK_OPACITY = CurMenu.DEFAULT_MASK_OPACITY;
            }
        }

        /*
         * Loads a dialogue
         */
        public void LoadDialogue(int id)
        {
            OpenMenus.Clear();
            OpenMenus.Add(dialogues[id]);
            dialogues[id].AnimatingIn = true;
            dialogues[id].Frame = 0;
            CurDialogue++;
        }

        /*
         * Begins transition from main menu with cool heart animation
         */
        public void StartMain(Menu menu)
        {
            MainMenuState = MainMenuState.INFECTING;
            menu.SetAnimation("infectingheart");
            mainmenu_stopwatch.Start();
            mainfade_stopwatch.Start();
        }

        /*
         * Starts the game
         */
        public void StartGame()
        {
            mainmenu_stopwatch.Stop();
            mainmenu_stopwatch.Reset();
            mainfade_stopwatch.Stop();
            mainfade_stopwatch.Reset();
            switch (curSelection)
            {
                case "Play":
                    engine.StartGame();
                    break;
                case "Tutorial":
                    engine.StartTutorial();
                    break;
            }
        }

        /*
         * Update menus
         */
        public void Update()
        {
            if (OpenMenus.Count == 0) return;
            
            // Handle infected heart animation transition
            if (mainmenu_stopwatch.IsRunning)
            {
                CurMenu.MASK_OPACITY = MathHelper.Lerp(CurMenu.DEFAULT_MASK_OPACITY, 0,
                    (float)mainfade_stopwatch.ElapsedMilliseconds/MAIN_FADE_TIME);
                if (MainMenuState == MainMenuState.INFECTING &&
                    mainmenu_stopwatch.ElapsedMilliseconds >= MAIN_INFECTING_TIME)
                {
                    MainMenuState = MainMenuState.INFECTED;
                    CurMenu.SetAnimation("infectedheart");
                    mainmenu_stopwatch.Restart();
                }
                else if (MainMenuState == MainMenuState.INFECTED &&
                  mainmenu_stopwatch.ElapsedMilliseconds >= MAIN_INFECTED_TIME)
                {
                    StartGame();
                }
            }

            foreach(Menu menu in OpenMenus)
            {
                menu.UpdateAnimation();
            }
            foreach (Menu menu in dialogues.Values)
            {
                if (menu.AnimatingIn)
                {
                    menu.Frame++;
                    if (menu.Frame >= menu.AnimationTime)
                    {
                        menu.AnimatingIn = false;
                    }
                }
                else if (menu.AnimatingOut)
                {
                    menu.Frame--;
                    if (menu.Frame <= 0)
                    {
                        menu.AnimatingOut = false;
                        engine.ChangeGameState(GameState.IN_GAME);
                    }
                }
            }
        }

        /*
         * Handle all menu selections
         */
        public void HandleMenuInput(InputController input_controller)
        {
            if (OpenMenus.Count == 0) return;
            if (CurMenu.Type == MenuType.MAIN && MainMenuState != MainMenuState.NORMAL)
            {
                if(input_controller.Enter)
                {
                    StartGame();
                }
                return;
            }

            Menu menu = OpenMenus.Last();
            MenuOption option = menu.Options[menu.CurSelection];

            // Handle primary option change
            if (input_controller.DownOnce)
            {
                sound_controller.play(SoundType.EFFECT, "menu_move");
                menu.CurSelection = (menu.CurSelection + 1) % menu.Options.Count;
            }
            if (input_controller.UpOnce)
            {
                sound_controller.play(SoundType.EFFECT, "menu_move");
                if(menu.CurSelection == 0) menu.CurSelection = menu.Options.Count-1;
                else menu.CurSelection--;
            }

            // Handle secondary selection (left right)
            bool secondarySelected = false;
            if (input_controller.LeftOnce && option.Options.Count > 0)
            {
                sound_controller.play(SoundType.EFFECT, "menu_move");
                option.CurSelection = (int)MathHelper.Clamp(option.CurSelection - 1, 0, option.Options.Count - 1);
                secondarySelected = true;
            }
            if (input_controller.RightOnce && option.Options.Count > 0)
            {
                sound_controller.play(SoundType.EFFECT, "menu_move");
                option.CurSelection = (int)MathHelper.Clamp(option.CurSelection + 1, 0, option.Options.Count - 1);
                secondarySelected = true;
            }

            if (menu.Type == MenuType.OPTIONS && secondarySelected)
            {
                String selection = option.Options[option.CurSelection].Text;
                switch (option.Text)
                {
                    case "Music":
                        if (selection.Equals("Off"))
                        {
                            sound_controller.MuteSounds(SoundType.MUSIC);
                        }
                        else if (selection.Equals("On"))
                        {
                            sound_controller.UnmuteSounds(SoundType.MUSIC);
                        }
                        break;
                    case "Sound Effects":
                        if (selection.Equals("Off"))
                        {
                            sound_controller.MuteSounds(SoundType.EFFECT);
                        }
                        else if (selection.Equals("On"))
                        {
                            sound_controller.UnmuteSounds(SoundType.EFFECT);
                        }
                        break;
                }
            }

            // Handle back button
            if (input_controller.Back)
            {
                if(menu.Type == MenuType.OPTIONS)
                {
                    OpenMenus.RemoveAt(OpenMenus.Count - 1);
                }
                else if (menu.Type == MenuType.PAUSE)
                {
                    OpenMenus.RemoveAt(OpenMenus.Count - 1);
                    engine.ChangeGameState(GameState.IN_GAME);
                }

            }

            // Handle option selection
            curSelection = option.Text;
            if (input_controller.Enter)
            {
                sound_controller.play(SoundType.EFFECT, "menu_select");
                switch (menu.Type)
                {
                    case MenuType.MAIN:
                        switch (curSelection)
                        {
                            case "Play":
                                StartMain(menu);
                                //engine.StartGame();
                                break;
                            case "Tutorial":
                                StartMain(menu);
                                //engine.StartTutorial();
                                break;
                            case "Load":
                                engine.LoadGame("savetest.xml");
                                break;
                            case "Options":
                                LoadMenu(MenuType.OPTIONS);
                                break;
                            case "Quit":
                                engine.Exit();
                                break;
                        }
                        break;
                    case MenuType.PAUSE:
                        switch (curSelection)
                        {
                            case "Resume":
                                engine.ChangeGameState(GameState.IN_GAME);
                                break;
                            case "Restart":
                                engine.RestartLevel();
                                break;
                            case "Save":
                                engine.SaveGame();
                                break;
                            case "Load":
                                engine.LoadGame("savetest");
                                break;
                            case "Map":
                                break;
                            case "Options":
                                LoadMenu(MenuType.OPTIONS);
                                break;
                            case "Quit to Menu":
                                engine.fadeTo(GameState.MENU);
                                break;
                        }
                        break;
                    case MenuType.OPTIONS:
                        switch (curSelection)
                        {
                            case "Back":
                                OpenMenus.Remove(menu);
                                break;
                        }
                        break;
                    case MenuType.WIN:
                        switch (curSelection)
                        {
                            case "Continue":
                                engine.fadeTo(GameState.LOADING, 50, 80);
                                break;
                        }
                        break;
                    case MenuType.LOSE:
                        switch (curSelection)
                        {
                            case "Start Over":
                                engine.fadeTo(GameState.LOADING);
                                break;
                            case "Quit to Menu":
                                engine.fadeTo(GameState.MENU);
                                break;
                        }
                        break;
                    case MenuType.DIALOGUE:
                        switch(CurDialogue) {
                            case 1:
                                // OMG NO, but in the interest of time
                                HUD.showWASD = true;
                                break;
                            default:
                                break;
                        }
                        menu.AnimatingOut = true;
                        break;
                }
            }
        }

        public void DrawMenu(GameCanvas canvas, Vector2 center)
        {
            foreach (Menu menu in OpenMenus)
            {
                menu.Draw(canvas, center, menu == CurMenu);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Controllers
{
    public class MenuController
    {
        private Dictionary<MenuType, Menu> menus;
        private Dictionary<int, Menu> dialogues;
        public List<Menu> OpenMenus;

        // Returns the current menu
        public Menu CurMenu
        {
            get
            {
                return OpenMenus.Last();
            }
        }

        // Returns the current dialogue id
        public int CurDialogue { get; set; }

        private SoundController sound_controller;
        private GameEngine engine;

        public MenuController(GameEngine engine, ContentFactory factory, SoundController sound_controller)
        {
            menus = factory.getMenus();
            dialogues = factory.getDialogues();
            OpenMenus = new List<Menu>();
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
         * Update menus
         */
        public void Update()
        {
            if (OpenMenus.Count == 0) return;
            foreach(Menu menu in menus.Values)
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
                    }
                }
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
            if (input_controller.LeftOnce)
            {
                sound_controller.play(SoundType.EFFECT, "menu_move");
                option.CurSelection = (int)MathHelper.Clamp(option.CurSelection - 1, 0, option.Options.Count - 1);
                secondarySelected = true;
            }
            if (input_controller.RightOnce)
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
            String curSelection = option.Text;
            if (input_controller.Enter)
            {
                sound_controller.play(SoundType.EFFECT, "menu_select");
                switch (menu.Type)
                {
                    case MenuType.MAIN:
                        switch (curSelection)
                        {
                            case "Play":
                                engine.StartGame();
                                break;
                            case "Tutorial":
                                engine.StartTutorial();
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
                            case 0:
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
                menu.Draw(canvas, center);
            }
        }
    }
}

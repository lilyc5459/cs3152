using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Models
{
    #region Enum
    public enum MenuType
    {
        MAIN,
        PAUSE,
        OPTIONS,
        WIN,
        LOSE,
        DIALOGUE,
    };
    #endregion

    public class Menu
    {
        public static Color fontColor = Color.Wheat;
        public static Color fontHighlightColor = new Color(230, 180, 100);
        public const int ANIMATION_TIME = 20;

        public MenuType Type { get; set; }
        public Dictionary<String, Texture2D> Textures { get; set; }
        public Dictionary<String, Dictionary<String, int>> AnimationData { get; set; }

        public String CurrentAnimation { get; set; }
        public bool AnimatingIn { get; set; }
        public bool AnimatingOut { get; set; }

        public int Frame { get; set; }
        public int Frame_y { get; set; }

        public int FrameTimeCounter { get; set; }
        public int NumFrames { get; set; }
        public Vector2 FrameSize { get; set; }
        public int AnimationTime { get; set; }

        public float MASK_OPACITY { get; set; }
        public float DEFAULT_MASK_OPACITY = 0.5f;

        public List<MenuOption> Options { get; set; }
        public int CurSelection { get; set; }
        public String Text1 { get; set; }
        public String Text2 { get; set; }

        public List<MenuType> Children { get; set; }

        public Menu(MenuType type, List<MenuOption> options, List<MenuType> children,
            Dictionary<String, Texture2D> textures, Dictionary<String, Dictionary<String, int>> animation_data)
        {
            Type = type;
            Options = options;
            Children = children;
            Textures = textures;
            AnimationData = animation_data;

            CurSelection = 0;
            CurrentAnimation = "";
            MASK_OPACITY = DEFAULT_MASK_OPACITY;

            if (type == MenuType.DIALOGUE) AnimationTime = ANIMATION_TIME;
            else if (type == MenuType.MAIN)
            {
                // Default animation is the pumping heart
                SetAnimation("heart");
            }
        }

        // Increments animation frame according to the specified speed
        public void UpdateAnimation()
        {
            if (NumFrames > 0)
            {
                FrameTimeCounter++;
                if (FrameTimeCounter >= AnimationTime)
                {
                    Frame = (Frame + 1) % NumFrames;
                    FrameTimeCounter = 0;
                }
            }
            if (CurrentAnimation.Equals("infectingheart"))
            {
                AnimationTime = (int)MathHelper.Lerp(14, 8, (float)(Frame + 4*Frame_y) / (NumFrames-1));
            }
        }

        // Sets the animation data for the specified animation
        public void SetAnimation(String animation)
        {
            CurrentAnimation = animation;
            Frame = 0;
            Frame_y = 0;
            FrameTimeCounter = 0;
            NumFrames = AnimationData[animation]["NumFrames"];
            AnimationTime = AnimationData[animation]["FrameSpeed"];
            FrameSize = new Vector2(AnimationData[animation]["FrameWidth"], AnimationData[animation]["FrameHeight"]);
        }

        public void Draw(GameCanvas canvas, Vector2 center, bool on_top)
        {
            Color color = Color.Black;
            String title = "";

            switch (Type)
            {
                case MenuType.MAIN:
                    title = "PATHOGENESIS";
                    color = new Color(0, 0, 0, 150);
                    break;
                case MenuType.PAUSE:
                    title = "Paused";
                    color = new Color(20, 0, 0, 150);
                    break;
                case MenuType.OPTIONS:
                    title = "Options";
                    color = new Color(0, 0, 0, 0);
                    break;
                case MenuType.WIN:
                    title = "Victory!";
                    color = new Color(20, 0, 0, 100);
                    break;
                case MenuType.LOSE:
                    title = "You're dead.";
                    color = new Color(20, 0, 0, 150);
                    break;
                case MenuType.DIALOGUE:
                    color = new Color(50, 0, 0, 180);
                    break;
            }

            // Background
            if (Type == MenuType.DIALOGUE)
            {
                canvas.DrawSprite(Textures["solid"], color * ((float)Frame/AnimationTime),
                    new Rectangle((int)center.X - canvas.Width / 2 + 20, (int)center.Y + canvas.Height / 7, canvas.Width - 40, 250),
                    new Rectangle(0, 0, Textures["solid"].Width, Textures["solid"].Height));
            }
            else if(Type == MenuType.MAIN)
            {
                canvas.DrawSprite(Textures["mainmenu"], Color.White,
                    new Rectangle((int)center.X - canvas.Width / 2, (int)center.Y - canvas.Height / 2, canvas.Width, canvas.Height),
                    new Rectangle(0, 0, Textures["mainmenu"].Width, Textures["mainmenu"].Height));

                int framelimit = 0;
                for (int i = 0; i < NumFrames; i++)
                {
                    if (Textures[CurrentAnimation].Width <= i * FrameSize.X * 1.5)
                    {
                        framelimit = i;
                        break;
                    }
                }
                if (Textures[CurrentAnimation].Width <= Frame * FrameSize.X * 1.5)
                {
                    if (!(Textures[CurrentAnimation].Height <= Frame_y * FrameSize.Y * 1.5))
                    {
                        Frame_y++;
                        Frame = 0;
                    }
                    else
                    {
                        Frame = framelimit;
                    }
                }
                canvas.DrawSprite(Textures[CurrentAnimation], Color.White,
                    new Rectangle((int)center.X - canvas.Width / 2 + 240, (int)center.Y - canvas.Height / 2 + 10,
                        AnimationData["heart"]["FrameWidth"], AnimationData["heart"]["FrameHeight"]),
                    new Rectangle(Frame * (int)FrameSize.X, Frame_y * (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y));

                canvas.DrawSprite(Textures["solid"], Color.Black * MASK_OPACITY,
                    new Rectangle((int)center.X - canvas.Width / 2, (int)center.Y - canvas.Height / 2, canvas.Width, canvas.Height),
                    new Rectangle(0, 0, Textures["solid"].Width, Textures["solid"].Height));
            }
            else
            {
                canvas.DrawSprite(Textures["solid"], color,
                    new Rectangle((int)center.X - canvas.Width / 2, (int)center.Y - canvas.Height / 2, canvas.Width, canvas.Height),
                    new Rectangle(0, 0, Textures["solid"].Width, Textures["solid"].Height));
            }

            // Draw dialogue if applicable
            if (Type == MenuType.DIALOGUE && Text1 != null)
            {
                Color font_draw_color = fontColor * ((float)Frame / AnimationTime);
                canvas.DrawText(Text1, font_draw_color,
                    new Vector2((int)center.X, (int)center.Y + canvas.Height / 2 - 200), "font1", true);
                if (Text2 != null)
                {
                    canvas.DrawText(Text2, font_draw_color,
                        new Vector2((int)center.X, (int)center.Y + canvas.Height / 2 - 150), "font1", true);
                }
                canvas.DrawText("Enter >", font_draw_color,
                    new Vector2((int)center.X + 320, (int)center.Y + canvas.Height / 2 - 70), "font1", true);
                return;
            }

            // Options
            if (on_top)
            {
                Color fontDrawColor = fontColor * MathHelper.Lerp(0, 1, MASK_OPACITY / DEFAULT_MASK_OPACITY);
                // Title
                canvas.DrawText(title, fontDrawColor,
                     new Vector2((int)center.X, (int)center.Y - canvas.Height / 2 + 100), "font3", true);

                for (int i = 0; i < Options.Count; i++)
                {
                    MenuOption option = Options[i];
                    Color option_color = fontDrawColor;
                    if (i == CurSelection)
                    {
                        option_color = fontHighlightColor;
                        canvas.DrawSprite(Textures["solid"], option_color,
                            new Rectangle((int)(center.X + option.Offset.X - 150), (int)(center.Y + option.Offset.Y), 15, 15),
                            new Rectangle(0, 0, Textures["solid"].Width, Textures["solid"].Height));
                        option.Draw(canvas, center, option_color, true);
                    }
                    else
                    {
                        option.Draw(canvas, center, option_color, false);
                    }
                }
            }
        }
    }
}

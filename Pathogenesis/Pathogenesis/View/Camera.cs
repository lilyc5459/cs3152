using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Pathogenesis
{
    // Camera code from http://jason-mitchell.com/game-development/xna-camera-for-2d-games/
    public class Camera
    {
        private int screen_width;
        private int screen_height;

        public Camera(int s_width, int s_height)
        {
            screen_width = s_width;
            screen_height = s_height;

            Position = Vector2.Zero;
            Zoom = 1f;
        }

        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public float Zoom { get; set; }

        public Matrix TransformMatrix
        {
            get
            {
                return Matrix.CreateRotationZ(Rotation) * Matrix.CreateScale(Zoom) *
                       Matrix.CreateTranslation(-Position.X + screen_width/2, -Position.Y + screen_height/2, 0);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace WindowsGame3
{
    class SpaceshipTypess
    {
        public Rectangle collisionRectangle;
        public Texture2D objectTexture;
        public float objectScale;
        public float objectSpeed;
        public string Description;

        public enum ObjectpType
        {
            planet, EnemyShip, SpaceStation
        }
        public static string[] ObjectModelFileName = { "figtherclass", "frigateclass", "freighterclass" };
        public static string[] objectDesc = { "Planet", "EnemyShip", "SpaceStation" };
        public static int[] ObjectArmor = { 0, 50, 90 };
        public static float[] ObjectSpeed = { 0.02f, 5.0f, 0.02f };
    }
}

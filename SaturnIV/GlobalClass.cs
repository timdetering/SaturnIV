using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SaturnIV
{
    /// <summary>
    /// A class to hold static global variables
    /// </summary>
    public static class Global
    {
        public static GraphicsDevice Graphics;
        //public static SpriteBatch spriteBatch = new SpriteBatch(Graphics);        
        public static int ScreenWidth { get { return Graphics.Viewport.Width; } }
        public static int ScreenHeight { get { return Graphics.Viewport.Height; } }
    }
}


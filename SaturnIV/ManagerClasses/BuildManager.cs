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

namespace SaturnIV
{
    public class BuildManager
    {
        int cost = 10;
        public enum BuildStates
        {
            notstarted = 0,
            started = 1,
            building = 2,
            done = 3
        }

        public void updateBuilds(double currentTime, newShipStruct buildThisObject)
        {
        }
        public void DrawBuildMenu(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            Vector2 pos = new Vector2(100, 768);
            spriteBatch.Begin();
                spriteBatch.DrawString(spriteFont, "Build Menu", pos, Color.Yellow);
            spriteBatch.End();
        }
    }
}

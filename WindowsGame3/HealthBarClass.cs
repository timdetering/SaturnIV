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

    public class HealthBarClass : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Texture2D mHealthBar;
        int mCurrentHealth = 100;

        public HealthBarClass(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            mHealthBar = Game.Content.Load<Texture2D>("textures//HealthBar") as Texture2D;
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            mCurrentHealth = (int)MathHelper.Clamp(mCurrentHealth, 0, 100);
            base.Update(gameTime);
        }

        public void DrawHbar(GameTime gameTime, SpriteBatch mBatch, Color barColor, int barStartX, int barStartY, 
                             int mHealthBarWidth, int mHealthBarHeight, int mCurrentHealth)
        {
            //TODO: Add your drawing code here
            //mBatch.Begin();
            //Draw the negative space for the health bar
            //barStartX += mHealthBarWidth;
            //mBatch.Draw(mHealthBar, new Rectangle(barStartX / 2 - mHealthBar.Width / 2,
            //barStartY, mHealthBar.Width, 44), new Rectangle(500, 450, mHealthBar.Width, 44), Color.Gray);

            //Draw the current health level based on the current Health
            mBatch.Draw(mHealthBar, new Rectangle(barStartX,
                 barStartY, (int)(mHealthBarWidth * ((double)mCurrentHealth / 100)), mHealthBarHeight),
                 new Rectangle(50, 50, mHealthBarWidth, mHealthBarHeight), barColor);
            base.Draw(gameTime);
        }

    }
}
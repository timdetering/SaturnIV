using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{
    class MessageClass
    {
        StringBuilder messageBuffer = new StringBuilder();
        public static List<String> messageLog = new List<string>();

        public void sendSystemMsg(SpriteFont spriteFont,SpriteBatch spriteBatch,string myMessage, Vector2 systemMessagePos)                         
        {
            if (myMessage != null)
                messageLog.Add(myMessage);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            messageBuffer = new StringBuilder();
            foreach (string msg in messageLog)
                messageBuffer.AppendFormat("\n" + msg);
            spriteBatch.DrawString(spriteFont, messageBuffer.ToString(), systemMessagePos, Color.MediumSlateBlue);
            //systemMessagePos.Y += 10;
            spriteBatch.End();
        }
    }
}

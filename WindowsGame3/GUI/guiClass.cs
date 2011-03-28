using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace WindowsGame3
{
    public class guiClass
    {
        Texture2D dummyTexture;
        Rectangle rectangle1,rectangle2,rectangle3,rectangle4,rectangle5;
        Color opt1Color, opt2Color, opt3Color, opt4Color,opt5Color;
        editOptions currentSelection;

        public enum editOptions
        {
            addremove,
            save,
            load,
            exit
        }

        public struct menuItem
        {
            public string itemText;
            public Rectangle itemRectangle;
            public Vector2 itemPosition;
        }

        
        public void initalize(Game game) 
        {
            rectangle1 = new Rectangle(5, 5, 150, 20);
            rectangle2 = new Rectangle(150, 5, 150, 20);
            rectangle3 = new Rectangle(300, 5, 70, 20);
            rectangle4 = new Rectangle(370, 5, 67, 20);
            rectangle5 = new Rectangle(437, 5, 75, 20);
            opt1Color = Color.Gray;
            opt2Color = Color.Gray;
            opt3Color = Color.Gray;
            opt4Color = Color.Gray;
            opt5Color = Color.Gray;


            dummyTexture = game.Content.Load<Texture2D>("textures//dummy") as Texture2D;

        }

        public void update(int mouseX, int mouseY)
        {
            currentSelection = editOptions.addremove;
            if (rectangle2.Contains(new Point(mouseX, mouseY)))
            {
                currentSelection = editOptions.addremove;
                opt2Color = Color.Black;
            }
            else
                opt2Color = Color.White;
            if (rectangle3.Contains(new Point(mouseX, mouseY)))
            {
                currentSelection = editOptions.addremove;
                opt3Color = Color.Black;
            }
            else
                opt3Color = Color.White;
            if (rectangle4.Contains(new Point(mouseX, mouseY)))
            {
                currentSelection = editOptions.addremove;
                opt4Color = Color.Black;
            }
            else
                opt4Color = Color.Gray;
            if (rectangle5.Contains(new Point(mouseX, mouseY)))
            {
                currentSelection = editOptions.addremove;
                opt5Color = Color.Black;
            }
            else
                opt5Color = Color.Gray;

        }

        public void drawGUI(SpriteBatch mBatch,SpriteFont spriteFont)
        {
 
            mBatch.Draw(dummyTexture, rectangle1, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle2, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle3, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle4, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle5, Color.Gray);

            StringBuilder messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Edit Mode");          
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(10,7), Color.Black);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Add/Remove");
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(150, 7), opt2Color);
        }
    }

}

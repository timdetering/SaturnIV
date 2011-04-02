using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace SaturnIV
{
    public class guiClass
    {
        Texture2D dummyTexture;
        Rectangle rectangle1,rectangle2,rectangle3,rectangle4,rectangle5;
        Color opt1Color, opt2Color, opt3Color, opt4Color,opt5Color;
        editOptions currentSelection;
        int verticalStartY = 25;
        int horizontalStartX = 150;
        int verticalItemSpacing = 20;
        int horizontalItemWidth = 200;
        bool AddRemove = false;
        Vector4 transGray = new Vector4(255, 255, 255, 128);
        
        List<MenuItem> menuItemList = new List<MenuItem>();

        public enum editOptions
        {
            addremove,
            save,
            load,
            exit
        }

        public struct MenuItem
        {
            public string itemText;
            public Rectangle itemRectangle;
        }

        public void buildShipMenu(ref List<shipData> shipList)
        {
            for (int i=0; i < shipList.Count; i++)
            {
                MenuItem tempItem = new MenuItem();
                tempItem.itemText = shipList[i].Type;
                tempItem.itemRectangle = new Rectangle(horizontalStartX, verticalStartY, horizontalItemWidth, verticalItemSpacing);
                verticalStartY += verticalItemSpacing;
                menuItemList.Add(tempItem);
            }
        }

        public void initalize(Game game) 
        {
            currentSelection = editOptions.load;
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

        public void update(MouseState currentMouse,MouseState oldMouse)
        {
            int mouseX = currentMouse.X; int mouseY = currentMouse.Y;

            if (rectangle2.Contains(new Point(mouseX, mouseY)))
            {    
                currentSelection = editOptions.addremove;
                opt2Color = Color.Black;
            }
            else
                opt2Color = Color.White;
            if (rectangle3.Contains(new Point(mouseX, mouseY)))
            {
                currentSelection = editOptions.save;
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

            if (currentMouse.LeftButton == ButtonState.Pressed) //&& oldMouse.LeftButton == ButtonState.Released)
            {
                if (currentSelection == editOptions.addremove)
                    AddRemove = true;
            }
        }

        public void drawGUI(SpriteBatch mBatch,SpriteFont spriteFont)
        {
 
            mBatch.Draw(dummyTexture, rectangle1, Color.TransparentBlack);
            mBatch.Draw(dummyTexture, rectangle2, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle3, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle4, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle5, Color.Gray);

            StringBuilder messageBuffer = new StringBuilder();
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Edit Mode");          
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(10,7), Color.Black);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Add/Remove");
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(150, 7), opt2Color);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Save");
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(300, 7), opt3Color);
            messageBuffer = new StringBuilder();
            if (AddRemove == true)
            {
                foreach (MenuItem mItem in menuItemList)
                {
                    mBatch.Draw(dummyTexture, mItem.itemRectangle, Color.Gray);
                    messageBuffer.AppendFormat(mItem.itemText);
                    mBatch.DrawString(spriteFont, messageBuffer,
                                    new Vector2(mItem.itemRectangle.X, mItem.itemRectangle.Y), Color.White);
                    messageBuffer = new StringBuilder();

                }
            }
        }
    }
}

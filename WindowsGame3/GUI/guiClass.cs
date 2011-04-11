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
        Rectangle rectangle1,rectangle2,rectangle3,rectangle4,rectangle5,rectangle6;
        Color opt1Color, opt2Color, opt3Color, opt4Color,opt5Color,opt6Color;
        editOptions currentSelection;
        int verticalStartY = 25;
        int horizontalStartX = 150;
        bool AddRemove = false;
        public int thisItem;
        Color itemColor;
        Vector4 transGray = new Vector4(255, 255, 255, 128);
        
        List<MenuItem> menuItemList = new List<MenuItem>();

        public enum editOptions
        {
            addremove,
            changeplayership,
            save,
            load,
            exit
        }

        public struct MenuItem
        {
            public string itemText;
            public Rectangle itemRectangle;
            public int itemIndex;
        }

        public void buildShipMenu(List<shipData> shipList)
        {
            for (int i=0; i < shipList.Count; i++)
            {
                MenuItem tempItem = new MenuItem();
                tempItem.itemText = shipList[i].Type;
                tempItem.itemIndex = i;
                tempItem.itemRectangle = new Rectangle(horizontalStartX, verticalStartY, 200,20);
                verticalStartY += 20;
                menuItemList.Add(tempItem);
            }
        }

        public void initalize(Game game, ref List<shipData> shipList) 
        {
            buildShipMenu(shipList);
            currentSelection = editOptions.load;
            rectangle1 = new Rectangle(5, 5, 150, 20);
            rectangle2 = new Rectangle(150, 5, 150, 20);
            rectangle3 = new Rectangle(300, 5, 70, 20);
            rectangle4 = new Rectangle(370, 5, 67, 20);
            rectangle5 = new Rectangle(437, 5, 75, 20);
            rectangle6 = new Rectangle(587, 5, 75, 20);
            opt1Color = Color.Gray;
            opt2Color = Color.Gray;
            opt3Color = Color.Gray;
            opt4Color = Color.Gray;
            opt5Color = Color.Gray;
            opt6Color = Color.Gray;
            itemColor = Color.White;

            dummyTexture = game.Content.Load<Texture2D>("textures//dummy") as Texture2D;
        }

        public void update(MouseState currentMouse,MouseState oldMouse)
        {
            int mouseX = currentMouse.X; int mouseY = currentMouse.Y;
            AddRemove = false;
            if (rectangle2.Contains(new Point(mouseX, mouseY)))
            {    
                currentSelection = editOptions.addremove;
                opt2Color = Color.Black;
            }
            else
                opt2Color = Color.White;
            if (rectangle3.Contains(new Point(mouseX, mouseY)))
            {
                currentSelection = editOptions.changeplayership;
                opt3Color = Color.Black;
            }
            else
                opt3Color = Color.White;
            if (rectangle4.Contains(new Point(mouseX, mouseY)))
            {
                currentSelection = editOptions.save;
                opt4Color = Color.Black;
            }
            else
                opt4Color = Color.White;
            if (rectangle5.Contains(new Point(mouseX, mouseY)))
            {
                currentSelection = editOptions.addremove;
                opt5Color = Color.Black;
            }
            else
                opt5Color = Color.White;

            if (currentMouse.LeftButton == ButtonState.Pressed) //&& oldMouse.LeftButton == ButtonState.Released)
            {
                if (currentSelection == editOptions.addremove)
                    AddRemove = true;
                else
                    AddRemove = false;
            }
            if (AddRemove)
            {
                for (int i = 0; i < menuItemList.Count; i++)
                {
                    if (menuItemList[i].itemRectangle.Contains(new Point(mouseX, mouseY)))
                        thisItem = i;
                }
            }

        }

        public void drawGUI(SpriteBatch mBatch,SpriteFont spriteFont)
        {
            
            mBatch.Draw(dummyTexture, rectangle1, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle2, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle3, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle4, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle5, Color.Gray);
            mBatch.Draw(dummyTexture, rectangle6, Color.Gray);

            StringBuilder messageBuffer = new StringBuilder();
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Edit Mode");          
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(10,7), Color.Black);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Add/Remove");
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(150, 7), opt2Color);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Change Player Ship");
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(300, 7), opt3Color);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Save");
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(450, 7), opt4Color);
            messageBuffer = new StringBuilder();
            if (AddRemove == true)
            {
                for (int i=0;i<menuItemList.Count;i++)
                {
                    if (thisItem == i)
                        itemColor = Color.Black;
                    else
                        itemColor = Color.White;

                    mBatch.Draw(dummyTexture, menuItemList[i].itemRectangle, Color.Gray);
                    messageBuffer.AppendFormat(menuItemList[i].itemText);
                    mBatch.DrawString(spriteFont, messageBuffer,
                                    new Vector2(menuItemList[i].itemRectangle.X, menuItemList[i].itemRectangle.Y),itemColor);
                    messageBuffer = new StringBuilder();

                }
            }
        }
    }
}

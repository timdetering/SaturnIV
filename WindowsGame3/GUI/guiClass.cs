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
        Rectangle rectangle1,rectangle2,rectangle3,rectangle4,rectangle5,rectangle6,rectangle7;
        Color opt1Color, opt2Color, opt3Color, opt4Color, opt5Color, opt6Color, opt7Color;
        public static editOptions currentSelection;
        int verticalStartY = 25;
        int horizontalStartX = 150;
        public static bool AddRemove = false;
        public static bool selectTeam = false;
        public int thisItem;
        public int thisTeam=0;
        Color itemColor;
        Vector4 transGray = new Vector4(255, 255, 255, 128);
        
        List<MenuItem> menuShipList = new List<MenuItem>();
        List<MenuItem> menuTeamList = new List<MenuItem>();

        public enum editOptions
        {
            addremove,
            formsquad,
            team,
            changeplayership,
            save,
            load,
            exit,
            none
        }

        public struct MenuItem
        {
            public string itemText;
            public Rectangle itemRectangle;
            public int itemIndex;
        }

        public void buildShipMenu(List<shipData> shipList)
        {
            horizontalStartX = 150;
            for (int i=0; i < shipList.Count; i++)
            {
                MenuItem tempItem = new MenuItem();
                tempItem.itemText = shipList[i].Type;
                tempItem.itemIndex = i;
                tempItem.itemRectangle = new Rectangle(horizontalStartX, verticalStartY, 300,20);
                verticalStartY += 20;
                menuShipList.Add(tempItem);
            }
           
        }

        public void buildTeamMenu()
        {
            horizontalStartX = 500;
            verticalStartY = 25;
            MenuItem tempItem = new MenuItem();
            tempItem.itemText = "One";
            tempItem.itemIndex = 0;
            tempItem.itemRectangle = new Rectangle(horizontalStartX, verticalStartY, 100, 20);
            verticalStartY += 20;
            menuTeamList.Add(tempItem);
            tempItem = new MenuItem();
            tempItem.itemText = "Two";
            tempItem.itemIndex = 1;
            tempItem.itemRectangle = new Rectangle(horizontalStartX, verticalStartY, 100, 20);
            verticalStartY += 20;
            menuTeamList.Add(tempItem);
        }


        public void initalize(Game game, ref List<shipData> shipList) 
        {
            buildShipMenu(shipList);
            buildTeamMenu();
            currentSelection = editOptions.load;
            rectangle1 = new Rectangle(5, 5, 150, 20);
            rectangle2 = new Rectangle(150, 5, 150, 20);
            rectangle3 = new Rectangle(300, 5, 150, 20);
            rectangle4 = new Rectangle(450, 5, 150, 20);
            rectangle5 = new Rectangle(600, 5, 150, 20);
            rectangle6 = new Rectangle(750, 5, 150, 20);
            rectangle7 = new Rectangle(850, 5, 150, 20);
            opt1Color = Color.Gray;
            opt2Color = Color.Gray;
            opt3Color = Color.Gray;
            opt4Color = Color.Gray;
            opt5Color = Color.Gray;
            opt6Color = Color.Gray;
            opt7Color = Color.Gray;
            itemColor = Color.White;

            dummyTexture = game.Content.Load<Texture2D>("textures//dummy") as Texture2D;
        }

        public void update(MouseState currentMouse,MouseState oldMouse)
        {
            int mouseX = currentMouse.X; int mouseY = currentMouse.Y;
            AddRemove = false;
            selectTeam = false;
            opt2Color = Color.White;
            opt3Color = Color.White;
            opt4Color = Color.White;
            opt5Color = Color.White;
            opt6Color = Color.White;
            opt7Color = Color.White;

            if (rectangle2.Contains(new Point(mouseX, mouseY)))
            {
                currentSelection = editOptions.addremove;
                opt2Color = Color.Black;
            }
            else
                if (rectangle3.Contains(new Point(mouseX, mouseY)))
                {
                    currentSelection = editOptions.formsquad;
                    opt3Color = Color.Black;
                }
                else
                    if (rectangle4.Contains(new Point(mouseX, mouseY)))
                    {
                        currentSelection = editOptions.team;
                        opt4Color = Color.Black;
                    }
                    else
                        if (rectangle5.Contains(new Point(mouseX, mouseY)))
                        {
                            currentSelection = editOptions.load;
                            opt5Color = Color.Black;
                        }

            if (currentMouse.LeftButton == ButtonState.Pressed) //&& oldMouse.LeftButton == ButtonState.Released)
            {
                if (currentSelection == editOptions.addremove)
                {
                    AddRemove = true;
                    for (int i = 0; i < menuShipList.Count; i++)
                    {
                        if (menuShipList[i].itemRectangle.Contains(new Point(mouseX, mouseY)))
                        {
                            thisItem = i;
                            break;
                        }
                    }
                }
                else
                    AddRemove = false;

                // Team Menu
                if (currentSelection == editOptions.team)
                {
                    selectTeam = true;
                    for (int i = 0; i < menuTeamList.Count; i++)
                    {
                        if (menuTeamList[i].itemRectangle.Contains(new Point(mouseX, mouseY)))
                        {
                            thisTeam = i;
                            break;
                        }
                    }
                }
                else
                    selectTeam = false;
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
            mBatch.Draw(dummyTexture, rectangle7, Color.Gray);

            StringBuilder messageBuffer = new StringBuilder();
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Edit Mode");          
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(10,7), Color.Black);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Add/Remove");
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(150, 7), opt2Color);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Form Squad");
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(350, 7), opt3Color);
            messageBuffer = new StringBuilder();
            messageBuffer.AppendFormat("Team");
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(500, 7), opt4Color);
            messageBuffer = new StringBuilder();
            if (AddRemove == true)
            {
                for (int i = 0; i < menuShipList.Count; i++)
                {
                    if (thisItem == i)
                        itemColor = Color.Black;
                    else
                        itemColor = Color.White;

                    mBatch.Draw(dummyTexture, menuShipList[i].itemRectangle, Color.Gray);
                    messageBuffer.AppendFormat(menuShipList[i].itemText);
                    mBatch.DrawString(spriteFont, messageBuffer,
                                    new Vector2(menuShipList[i].itemRectangle.X, menuShipList[i].itemRectangle.Y), itemColor);
                    messageBuffer = new StringBuilder();

                }
            }

                if (selectTeam == true)
                {
                    for (int i = 0; i < menuTeamList.Count; i++)
                    {
                        if (thisTeam == i)
                            itemColor = Color.Black;
                        else
                            itemColor = Color.White;

                        mBatch.Draw(dummyTexture, menuTeamList[i].itemRectangle, Color.Gray);
                        messageBuffer.AppendFormat(menuTeamList[i].itemText);
                        mBatch.DrawString(spriteFont, messageBuffer,
                                        new Vector2(menuTeamList[i].itemRectangle.X, menuTeamList[i].itemRectangle.Y), itemColor);
                        messageBuffer = new StringBuilder();

                    }
               }
        }
    }
}

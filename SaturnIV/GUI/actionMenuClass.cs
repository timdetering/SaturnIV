using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{
    public class actionMenuClass
    {
        Texture2D dummyTexture, medBox;
        public Rectangle medRec;
        Color opt1Color;
        public static editOptions currentSelection;
        public int thisFaction, thisShip;
        public int thisScenario = -1;
        int verticalStartY = 25;
        int horizontalStartX = 150;
        public static bool Show = false;
        public static bool selectTeam = false;
        public static bool LoadScenario = false;
        public static bool inGui;
        public string loadThisScenario = null;
        Color itemColor;
        Vector4 transGray = new Vector4(255, 255, 255, 128);
        //int[] menuStartX = new int[10]{10,150,300,450,600,750};
               
        List<MenuItem> menuShipList = new List<MenuItem>();
        List<MenuItem> menuActionList = new List<MenuItem>();

        public enum editOptions
        {
            none,
            build,
        }

        public struct MenuItem
        {
            public string itemText;
            public Rectangle itemRectangle;
            public int itemIndex;
        }

        public void buildShipMenu(List<shipData> shipList)
        {
            menuShipList.Clear();
            Vector2 pos = new Vector2(50, 775);
            horizontalStartX = (int)pos.X;
            verticalStartY = (int)pos.Y;
            for (int i=0; i < shipList.Count; i++)
            {
                MenuItem tempItem = new MenuItem();
                tempItem.itemText = shipList[i].Type;
                tempItem.itemIndex = i;
                tempItem.itemRectangle = new Rectangle(horizontalStartX, verticalStartY, 225,20);
                verticalStartY += 20;
                menuShipList.Add(tempItem);
            }
        }
 
        public void initalize(Game game, ref List<shipData> shipList) 
        {            
            //currentSelection = editOptions.load;            
            opt1Color = Color.Gray;           
            itemColor = Color.White;
            buildShipMenu(shipList);
            dummyTexture = game.Content.Load<Texture2D>("textures//dummy") as Texture2D;
            medBox = game.Content.Load<Texture2D>("textures//GUI/medbox") as Texture2D;
            medRec = new Rectangle(25, 700, 250, 300);
        }

        public void update(MouseState currentMouse, MouseState oldMouse, ref List<shipData> shipList)
        {
            int mouseX = currentMouse.X; int mouseY = currentMouse.Y;
            Show = false;
            for (int i = 0; i < menuShipList.Count; i++)
            {
                if (menuShipList[i].itemRectangle.Contains(new Point(mouseX, mouseY)))
                {
                    thisShip = i;
                    inGui = true;
                    break;
                }
            }

        }

        public void drawGUI(SpriteBatch mBatch,SpriteFont spriteFont)
        {            
            StringBuilder messageBuffer = new StringBuilder();
            messageBuffer = new StringBuilder();
            mBatch.Draw(medBox, medRec, Color.White);
            messageBuffer.AppendFormat("Foundry" + "\n");
            messageBuffer.AppendFormat("Build Ship");
            mBatch.DrawString(spriteFont, messageBuffer.ToString(), new Vector2(60, 725), Color.Yellow);
            messageBuffer = new StringBuilder();
            for (int i = 0; i < menuShipList.Count; i++)
            {
                if (thisShip == i)
                    itemColor = Color.White;
                else
                    itemColor = Color.Green;
                    //mBatch.Draw(dummyTexture, menuShipList[i].itemRectangle, Color.Gray);
                    messageBuffer.AppendFormat(menuShipList[i].itemText);
                    mBatch.DrawString(spriteFont, messageBuffer,
                                    new Vector2(menuShipList[i].itemRectangle.X, menuShipList[i].itemRectangle.Y), itemColor);
                    messageBuffer = new StringBuilder();
            }
                            
        }
    }
}
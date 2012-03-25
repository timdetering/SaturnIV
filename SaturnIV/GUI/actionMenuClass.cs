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
        /// <summary>
        /// tethanium IS the resource!
        /// </summary>
        /// 
        int tethAvailable;
        Texture2D dummyTexture, medBox, buildMBox, rSideWindow;
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
        public static bool inGui, isSelected;
        public string loadThisScenario = null;
        Vector2 queueListPos = new Vector2(55, 600);        
        Vector2 shipListPos = new Vector2(55, 425);
        Vector2 shipTitlePos = new Vector2(60, 400);
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
            Vector2 pos = shipListPos;
            horizontalStartX = (int)pos.X;
            verticalStartY = (int)pos.Y;
            for (int i=0; i < shipList.Count; i++)
            {
                MenuItem tempItem = new MenuItem();
                tempItem.itemText = shipList[i].Type;
                tempItem.itemIndex = i;
                tempItem.itemRectangle = new Rectangle(horizontalStartX, verticalStartY, 250,20);
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
            buildMBox = game.Content.Load<Texture2D>("textures//GUI/buildmenubg") as Texture2D;
            rSideWindow = game.Content.Load<Texture2D>("textures//GUI/mWindow") as Texture2D;
            medRec = new Rectangle((int)shipListPos.X, (int)shipListPos.Y, 200, 200);
        }

        public void update(MouseState currentMouse, MouseState oldMouse, BuildManager buildManager, bool isLClicked, Vector3 pos)
        {
            isSelected = false;
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
            if (isLClicked)
            {                
                newShipStruct tempShip = new newShipStruct();
                buildManager.addBuild(thisShip, "new ship", pos);
                isSelected = true;
            }
        }

        public void drawGUI(SpriteBatch mBatch,SpriteFont spriteFont, BuildManager buildManager)
        {            
            StringBuilder messageBuffer = new StringBuilder();
            messageBuffer = new StringBuilder();
            //mBatch.Draw(buildMBox, medRec, Color.White);
            mBatch.Draw(rSideWindow, new Rectangle(20, 355, 348, 640), Color.Blue);
            mBatch.DrawString(spriteFont, "Build Menu", shipTitlePos, Color.White);
            /// This is the Available Ship List Area
            /// 
            for (int i = 0; i < menuShipList.Count; i++)
            {
                if (thisShip == i)
                {
                    itemColor = Color.White;
                    mBatch.Draw(dummyTexture, menuShipList[i].itemRectangle, Color.Gray);
                }
                else
                    itemColor = Color.Green;
                    messageBuffer.AppendFormat(menuShipList[i].itemText);
                    mBatch.DrawString(spriteFont, messageBuffer,
                                    new Vector2(menuShipList[i].itemRectangle.X, menuShipList[i].itemRectangle.Y), itemColor);
                    messageBuffer = new StringBuilder();
            }
            /// This is the Current Build Queue List Area
            /// 
            messageBuffer = new StringBuilder();
            for (int i = 0; i < buildManager.buildQueueList.Count; i++)
            {
                messageBuffer.AppendFormat(buildManager.buildQueueList[i].name + "\n");
                messageBuffer.AppendFormat("% {0} \n ", buildManager.buildQueueList[i].percentComplete);                         
            }       
            mBatch.DrawString(spriteFont, messageBuffer, queueListPos, Color.White); 
        }
    }
}
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
        Texture2D dummyTexture, medBox, buildMBox, rSideWindow;
        Texture2D resourceIconTex, bottomMenubarTex, constructMenuTex;
        public Rectangle medRec;
        Color opt1Color;
        public static MenuOptions currentSelection;
        public int thisFaction, thisShip, thisAction, thisMainAction;
        public int thisScenario = -1;
        int verticalStartY = 25;
        int horizontalStartX = 150;
        public static bool Show = false;
        public static bool selectTeam = false;
        public static bool LoadScenario = false;
        public static bool inGui, isSelected, showContructMenu;
        public string loadThisScenario = null;
        public bool isPlacing = false;
        public bool isPlaced = false;
        Vector2 queueListPos = new Vector2(55, 600);        
        Vector2 listPos = new Vector2(40, 415);
        Vector2 listTitlePos = new Vector2(45, 425);      
        Vector2 listRecSize = new Vector2(75, 20);
        Vector2 listRecSize2 = new Vector2(200, 20);
        Vector2 mainMenuPos = new Vector2(20, 900);
        Vector2 orderListPos = new Vector2(90, 850);
        Rectangle rightSideWindowRectange = new Rectangle(5, 840, 700, 40);
        Rectangle rightSideWindowRectange2 = new Rectangle(20,200, 400, 600);
        Rectangle resourceAreaRectange = new Rectangle(900, 20, 192, 62);
        Color itemColor;
        Vector4 transGray = new Vector4(255, 255, 255, 128);
        string[] actionText = new string[4]{"Engage", "Defend", "Patrol", "Hold"};
        string[] mainMenuText = new string[2] { "Tacmap", "Exit"};
        List<MenuItem> menuShipList = new List<MenuItem>();
        List<MenuItem> actionList = new List<MenuItem>();
        List<MenuItem> mainMenuList = new List<MenuItem>();
        disposition newOrder;
        MenuOptions menuOption;

        public enum MenuOptions
        {
            none,
            construct
        }

        public void buildMainMenu()
        {
            Vector2 pos = mainMenuPos;
            horizontalStartX = (int)pos.X;
            verticalStartY = (int)pos.Y;
            for (int i = 0; i < mainMenuText.Count(); i++)
            {
                MenuItem tempItem = new MenuItem();
                tempItem.itemText = mainMenuText[i];
                tempItem.itemIndex = i;
                tempItem.itemRectangle = new Rectangle(horizontalStartX, verticalStartY, 170, 70);
                horizontalStartX += 183;
                mainMenuList.Add(tempItem);
            }
        }

        public void buildActionList()
        {
            actionList.Clear();
            Vector2 pos = orderListPos;
            horizontalStartX = (int)pos.X;
            verticalStartY = (int)pos.Y;
            for (int i = 0; i < actionText.Count(); i++)
            {
                MenuItem tempItem = new MenuItem();
                tempItem.itemText = actionText[i];
                tempItem.itemIndex = i;
                tempItem.itemRectangle = new Rectangle(horizontalStartX, verticalStartY, (int)listRecSize.X, (int)listRecSize.Y);
                horizontalStartX += 125;
                listRecSize.X += 125;
                actionList.Add(tempItem);
            }
        }

        public void buildShipMenu(List<shipData> shipList)
        {
            menuShipList.Clear();
            Vector2 pos = listPos;
            horizontalStartX = (int)pos.X;
            verticalStartY = (int)pos.Y;
            for (int i=0; i < shipList.Count; i++)
            {
                if (shipList[i].TechLevel == 1 && shipList[i].BelongsTo == "Fleet")
                {
                    MenuItem tempItem = new MenuItem();
                    tempItem.itemText = shipList[i].Type;
                    tempItem.itemIndex = i;
                    tempItem.itemRectangle = new Rectangle(horizontalStartX, verticalStartY, (int)listRecSize2.X, (int)listRecSize2.Y);
                    verticalStartY += 20;
                    menuShipList.Add(tempItem);
                }
            }
        }
 
        public void initalize(Game game, ref List<shipData> shipList) 
        {            
            //currentSelection = editOptions.load;            
            opt1Color = Color.Gray;           
            itemColor = Color.White;
            buildShipMenu(shipList);
            buildActionList();
            buildMainMenu();
            dummyTexture = game.Content.Load<Texture2D>("textures//dummy") as Texture2D;
            medBox = game.Content.Load<Texture2D>("textures//GUI/medbox") as Texture2D;
            buildMBox = game.Content.Load<Texture2D>("textures//GUI/buildmenubg") as Texture2D;
            rSideWindow = game.Content.Load<Texture2D>("textures//GUI/mWindow") as Texture2D;
            resourceIconTex = game.Content.Load<Texture2D>("textures//GUI/resourceIcons") as Texture2D;
            bottomMenubarTex = game.Content.Load<Texture2D>("Models/tacmap_items/bottom_menu_bar") as Texture2D;
            constructMenuTex = game.Content.Load<Texture2D>("Models/tacmap_items/construct_menubox") as Texture2D;
            medRec = new Rectangle((int)listPos.X, (int)listPos.Y, 200, 200);
        }

        public void update(MouseState currentMouse, MouseState oldMouse, BuildManager buildManager, 
                           bool isLClicked, Vector3 pos, MenuActions menuAction, ref List<newShipStruct> shipList)
        {
            isSelected = false;
            int mouseX = currentMouse.X; int mouseY = currentMouse.Y;
            Show = false;
            /// Bottom Main Menu Checks ///
            /// 
            if (currentMouse.LeftButton == ButtonState.Pressed)
            {
                for (int i = 0; i < mainMenuList.Count; i++)
                {
                    if (mainMenuList[i].itemRectangle.Contains(new Point(mouseX, mouseY)))
                    {
                        thisMainAction = i;
                        guiClass.inGui = true;
                        break;
                    }
                }
                switch (thisMainAction)
                {
                    case 0:
                        menuOption = MenuOptions.none;
                        break;
                    case 1:
                        Game1.doExit = true;
                        break;
                }
            }
            /// Build Menu Checks ///
            /// 
            if (menuAction == MenuActions.build)
            {
                for (int i = 0; i < menuShipList.Count; i++)
                {
                    if (menuShipList[i].itemRectangle.Contains(new Point(mouseX, mouseY)))
                    {
                        thisShip = menuShipList[i].itemIndex;
                        guiClass.inGui = true;
                        isPlacing = true;
                        break;
                    }
                }
                if (currentMouse.RightButton == ButtonState.Pressed)
                {
                    isPlacing = true;
                    menuAction = MenuActions.none;
                    if (isPlaced)
                    {
                        newShipStruct tempShip = new newShipStruct();
                        buildManager.addBuild(thisShip, "new ship", pos);
                        isSelected = true;
                        isPlaced = false;                        
                    }
                }
            }
            /// Ship Action Menu Update Logic
            /// 
            if (menuAction == MenuActions.action)
            {
                for (int i = 0; i < actionList.Count; i++)
                {
                    if (actionList[i].itemRectangle.Contains(new Point(mouseX, mouseY)))
                    {
                        thisAction = i;
                        guiClass.inGui = true;
                        break;
                    }
                }
                if (isLClicked)
                {
                    switch (thisAction)
                    {
                        case 0:
                            newOrder = disposition.engaging;
                            break;
                        case 1:
                            newOrder = disposition.defensive;
                            break;
                        case 2:
                            newOrder = disposition.patrol;
                            break;
                        case 3:
                            newOrder = disposition.idle;
                            break;
                    }
                    foreach (newShipStruct tShip in shipList)
                        if (tShip.isSelected)
                            tShip.currentDisposition = newOrder;
                    isSelected = true;
                }
            }            
        }

        public void drawMainMenu(SpriteBatch mBatch, SpriteFont spriteFont, int tethAmnt, int amAmnt)
        {
            StringBuilder messageBuffer = new StringBuilder();
            messageBuffer = new StringBuilder();
            mBatch.Draw(bottomMenubarTex, mainMenuPos, Color.White);
            mBatch.Draw(resourceIconTex, resourceAreaRectange, Color.White);
            mBatch.DrawString(spriteFont, tethAmnt.ToString(), new Vector2(resourceAreaRectange.X, resourceAreaRectange.Y), Color.White);
            mBatch.DrawString(spriteFont, amAmnt.ToString(), new Vector2(resourceAreaRectange.X+ 48, resourceAreaRectange.Y), Color.White);            
        }

        public void drawBuildGUI(SpriteBatch mBatch,SpriteFont spriteFont, BuildManager buildManager)
        {            
            StringBuilder messageBuffer = new StringBuilder();
            messageBuffer = new StringBuilder();
            mBatch.Draw(constructMenuTex, rightSideWindowRectange2, Color.White);
            /// This is the Available Ship List Area
            /// 
            for (int i = 0; i < menuShipList.Count; i++)
            {
                if (thisShip == menuShipList[i].itemIndex)
                {
                    itemColor = Color.Yellow;
                }
                else
                    itemColor = Color.White;
                    messageBuffer.AppendFormat(menuShipList[i].itemText);
                    mBatch.DrawString(spriteFont, messageBuffer,
                                    new Vector2(menuShipList[i].itemRectangle.X, menuShipList[i].itemRectangle.Y), itemColor);
                    messageBuffer = new StringBuilder();
            }
        }

        public void drawActionGUI(SpriteBatch mBatch, SpriteFont spriteFont, ref List<newShipStruct> shipList)
        {
            StringBuilder messageBuffer = new StringBuilder();
            messageBuffer = new StringBuilder();
            mBatch.Draw(rSideWindow, rightSideWindowRectange, Color.Blue);
            for (int i = 0; i < mainMenuList.Count; i++)
            {
                mBatch.Draw(dummyTexture, mainMenuList[i].itemRectangle, Color.White);
            }
            /// This is the Available Ship List Area
            /// 
            for (int i = 0; i < actionList.Count; i++)
            {
                if (thisAction == i)
                {
                    itemColor = Color.Black;
                    mBatch.Draw(dummyTexture, actionList[i].itemRectangle, Color.Gray);
                }
                else
                    itemColor = Color.White;
                messageBuffer.AppendFormat(actionList[i].itemText);
                mBatch.DrawString(spriteFont, messageBuffer,
                                new Vector2(actionList[i].itemRectangle.X, actionList[i].itemRectangle.Y), itemColor);
                messageBuffer = new StringBuilder();
            }                   
        }
    }
}
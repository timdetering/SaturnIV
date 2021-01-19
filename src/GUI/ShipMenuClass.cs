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
    public class ShipMenuClass
    {
        Vector2 shipInfoPos = new Vector2(1000, 384);
        Texture2D shipInfoTex, platform_icon, station_icon;
        public Texture2D fighter_icon, constructor_icon;
        List<MenuItem> menuShipList = new List<MenuItem>();
        int sCount;
        int selected;

        public void Init(Game game)
        {
            fighter_icon = game.Content.Load<Texture2D>("textures//icon_fighter") as Texture2D;
            constructor_icon = game.Content.Load<Texture2D>("textures//icon_constructor") as Texture2D;
            platform_icon = game.Content.Load<Texture2D>("textures//icon_platform") as Texture2D;
            station_icon = game.Content.Load<Texture2D>("textures//icon_station") as Texture2D;

            shipInfoTex = game.Content.Load<Texture2D>("Models/tacmap_items/shipinfobox");
        }

        public void Update(ref List<newShipStruct> activeShipList)
        {
            sCount = 0;
            shipInfoPos = new Vector2(1100, 384);
            menuShipList.Clear();
            foreach (newShipStruct tShip in activeShipList)
            {
                if (tShip.isSelected)
                {
                    Rectangle cRectangle = new Rectangle((int)shipInfoPos.X, (int)shipInfoPos.Y, 72, 64);
                    MenuItem newItem = new MenuItem();
                    newItem.itemIndex = sCount;
                    newItem.itemRectangle = cRectangle;
                    newItem.itemText = "";
                    if (newItem.itemRectangle.Intersects(new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 5, 5)))
                    {
                        newItem.itemSelected = true;
                        if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                        {
                            if (tShip.objectClass == ClassesEnum.Station || tShip.objectClass == ClassesEnum.Constructor)
                                Game1.menuAction = MenuActions.build;                            
                            tShip.isBuilding = true;
                        }
                    }
                    menuShipList.Add(newItem);
                    sCount++;
                    if ((sCount % 2) == 0 && sCount > 0)
                    {
                        ///is Even
                        shipInfoPos.Y += 64;
                        shipInfoPos.X = 1100;
                    }
                    else
                    {
                        ///is Odd
                        shipInfoPos.X += 72;
                    }
                }
            }              
        }
        
        public void DrawShipInfoMenu(SpriteBatch spriteBatch, SpriteFont spriteFont, ref List<newShipStruct> activeShipList)
        {
            shipInfoPos = new Vector2(1000, 384);
            sCount = 0;
            Color boxColor = Color.White;
            foreach (newShipStruct tShip in activeShipList)
            {
                boxColor = Color.White;
                if (tShip.isSelected)
                {
                    Vector2 fontPos = new Vector2(tShip.screenCords.X, tShip.screenCords.Y - 45);
                    StringBuilder buffer = new StringBuilder();
                    Rectangle cRectangle = menuShipList[sCount].itemRectangle;                   
                    spriteBatch.Begin();
                    if (menuShipList[sCount].itemSelected) boxColor = Color.Red;
                    spriteBatch.Draw(shipInfoTex, cRectangle, boxColor);
                    Texture2D shipIcon = fighter_icon;
                    if (tShip.objectClass == ClassesEnum.Station) shipIcon = station_icon;
                    if (tShip.objectClass == ClassesEnum.Platform) shipIcon = platform_icon;
                    if (tShip.objectClass == ClassesEnum.Constructor) shipIcon = constructor_icon;
                    spriteBatch.Draw(shipIcon, cRectangle, boxColor);
                    //spriteBatch.DrawString(spriteFont, tShip.objectAlias, fontPos, Color.White);
                    //spriteBatch.DrawString(spriteFont, tShip.objectAlias.ToString() + "\n" + tShip.objectClass.ToString(), new Vector2(shipInfoPos.X + 12, shipInfoPos.Y + 4), Color.White);
                    //spriteBatch.DrawString(spriteFont, "Hull:" + tShip.hullLvl.ToString() + "\n" + tShip.currentDisposition, new Vector2(shipInfoPos.X + 12, shipInfoPos.Y + 44), Color.Yellow);
                    spriteBatch.End();
                    sCount++;
                    if ((sCount % 2) == 0 && sCount > 0)
                    {
                        ///is Even
                        shipInfoPos.Y += 96;
                        shipInfoPos.X = 1000;
                    }
                    else
                    {
                        ///is Odd
                        shipInfoPos.X += 128;
                    }
                }
            }
        }

        /// Update
        /// 
        public void UpdateMe(ref List<newShipStruct> activeShipList)
        {
        
        }

    }
}

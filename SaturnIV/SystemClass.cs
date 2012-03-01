using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{
    public class SystemClass
    {
        public static List<systemStruct> systemList = new List<systemStruct>();

        public void initSystems(string systemListFileName)
        {
            
        }

        public void drawSystemMap(SpriteBatch spritebatch)
        {
        }

        public void createNewSystem(string name, Vector3 pos, planetSaveStruct planet, string mapFile, int id)
        {
            systemStruct newSystem = new systemStruct();
            newSystem.systemID = id;
            newSystem.systemMapPosition = pos;
            newSystem.systemName = name;
            newSystem.systemPlanet = planet;
            systemList.Add(newSystem);
            //return newSystem;       
        }
    }
}

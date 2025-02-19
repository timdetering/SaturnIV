﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{
    public class SystemClass
    {
        public List<systemStruct> systemList = new List<systemStruct>();

        public void loadSystems(Game game, string filename, ref SerializerClass serialClass, ref List<shipData> shipDefList, ref Vector3 cameraPos, double currentTime)
        {            
            systemStruct newSystem = new systemStruct();
            newSystem.pManager = new PlanetManager(game);
            newSystem.pManager.Initialize();
            newSystem.buildManager = new BuildManager();
            newSystem.systemShipList = new List<newShipStruct>();
            newSystem.weaponsManager = new WeaponsManager(game);
            newSystem.weaponsManager.Initialize();
            newSystem.systemScene = serialClass.loadScene(currentTime, filename, ref newSystem.systemShipList, ref shipDefList, ref cameraPos, ref newSystem.pManager);
            newSystem.lastCameraPos = cameraPos;
            systemList.Add(newSystem);
        }

        public void drawSystemMap(SpriteBatch spritebatch)
        {
        }
    }
}

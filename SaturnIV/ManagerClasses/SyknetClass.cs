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
    public class SkynetClass
    {
        int thisTeam = 1;
        newShipStruct useThisShip = new newShipStruct();
        List<newShipStruct> tmpList = new List<newShipStruct>();

        planetStruct useThisPlanet = new planetStruct();

        public void update(systemStruct cSystem, ref List<newShipStruct> shipList, ref List<shipData> shipData)
        {
            useThisShip = findContructor(ref shipList);
            tmpList = shipList.Where(item => item.team != thisTeam).ToList();
            tmpList = tmpList.Where(item => item.objectClass == ClassesEnum.Fighter).ToList();
            foreach (newShipStruct tShip in shipList)
            {
                /// No fighers!  Build some
                if (tmpList.Count() < 5 && useThisShip.buildManager.buildQueueList.Count() < 2)
                {
                    newShipStruct tempShip = new newShipStruct();
                    Vector3 buildPosition = findPlanet(ref cSystem).planetPosition;
                    buildPosition.Y = 0;
                    useThisShip.buildManager.addBuild(3, "new ship", buildPosition + new Vector3(100,0,200), thisTeam);
                }
            }
        }

        private newShipStruct findContructor(ref List<newShipStruct> shipList)
        {
             /// Find a Constrcutor
            foreach (newShipStruct tShip in shipList)
                if (tShip.objectClass == ClassesEnum.Constructor && tShip.team == 1)
                    return tShip;
            return null;
        }

        private planetStruct findPlanet(ref systemStruct cSystem)
        {
            /// Find a Constrcutor
            foreach (planetStruct tPlanet in cSystem.pManager.planetList)
            {
                if (tPlanet.isControlled > 0)
                    return tPlanet;
            }
            return null;
        }
    }
}

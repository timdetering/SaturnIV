using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace SaturnIV
{
    public class BuildManager
    {
        public List<buildItem> buildQueueList = new List<buildItem>();
        int cost = 10;
        double currentTime;
        float buildTime = 2000;

        public enum BuildStates
        {
            notstarted = 0,
            started = 1,
            building = 2,
            done = 3
        }

        public void addBuild(int sType, string sName, Vector3 sPos)
        {
            buildItem buildThis = new buildItem();
            buildThis.pos = sPos; buildThis.name = sName; buildThis.shipType = sType; buildThis.startTime = currentTime;
            MessageClass.messageLog.Add("Adding New Ship to Foundry Build Queue at" + currentTime);
            buildQueueList.Add(buildThis);
        }

        public void updateBuildQueue(ref List<shipData> shipDefList, ref List<newShipStruct> activeShipList, double cTime)
        {
            currentTime = cTime;
            float pComplete = (float)((currentTime - buildQueueList.First().startTime) / buildTime * 100);
            pComplete = pComplete / buildTime * 100;
            buildQueueList.First().percentComplete = pComplete;
            if (buildQueueList.First().percentComplete > 99)
            {
                newShipStruct newShip = EditModeComponent.spawnNPC(buildQueueList.First().pos, ref shipDefList,
                                   buildQueueList.First().name, buildQueueList.First().shipType, 0, false);
                                newShip.wayPointPosition = buildQueueList.First().pos * 1000;
                                newShip.currentDisposition = disposition.moving;
                activeShipList.Add(newShip);
                buildQueueList.Remove(buildQueueList.First());
            }
        }
    }
}

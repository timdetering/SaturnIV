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
        float buildTime = 1000;

        public void addBuild(int sType, string sName, Vector3 sPos, int side)
        {
            buildItem buildThis = new buildItem();
            buildThis.buildState = BuildStates.started;
            buildThis.pos = sPos; buildThis.name = sName; buildThis.shipType = sType; buildThis.side = side;
            MessageClass.messageLog.Add("Adding New Ship to Foundry Build Queue");
            buildQueueList.Add(buildThis);
        }

        public void updateBuildQueue(ref List<shipData> shipDefList, ref List<newShipStruct> activeShipList, double cTime, newShipStruct tConstructor)
        {
            if (buildQueueList.Count > 0)
            {
                if (buildQueueList.First().buildState == BuildStates.building)
                {
                    currentTime = cTime;
                    if (buildQueueList.First().startTime < 1) buildQueueList.First().startTime = currentTime;                    
                    float pComplete = (float)((currentTime - buildQueueList.First().startTime) / buildTime * 100);
                    pComplete = pComplete / buildTime * 100;
                    buildQueueList.First().percentComplete = pComplete;
                    MessageClass.messageLog.Add("Build at" + pComplete);
                    if (buildQueueList.First().percentComplete > 99)
                    {
                        newShipStruct newShip = EditModeComponent.spawnNPC(cTime, buildQueueList.First().pos, ref shipDefList,
                                           buildQueueList.First().name, buildQueueList.First().shipType, buildQueueList.First().side, false);
                        newShip.wayPointPosition = buildQueueList.First().pos * 75;
                        newShip.currentDisposition = disposition.patrol;
                        activeShipList.Add(newShip);
                        buildQueueList.Remove(buildQueueList.First());
                        tConstructor.currentDisposition = disposition.moving;
                    }
                }
            }
        }
    }
}

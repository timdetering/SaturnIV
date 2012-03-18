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

        public class buildItem
        {
            public Vector3 pos;
            public string name;
            public int shipType;
            public double startTime;
        }

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

        public void updateBuildQueue(ref List<newShipStruct> activeShipList, double cTime)
        {
            currentTime = cTime;
        }
    }
}

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
    class ResourceClass
    {
        public List<ResourceStruct> resourceList;
        public void Init()
        {
            resourceList = new List<ResourceStruct>();
            ResourceStruct newResource = new ResourceStruct();
            newResource.collectionTime = 9000;
            newResource.resourceType = ResourceType.Tethanium;            
            resourceList.Add(newResource);
            newResource = new ResourceStruct();
            newResource.collectionTime = 20000;
            newResource.resourceType = ResourceType.AntiMatter;
            resourceList.Add(newResource);
            newResource = new ResourceStruct();
            newResource.collectionTime = 5000;
            newResource.resourceType = ResourceType.Metal;
            resourceList.Add(newResource);

        }
        
        public int updateResourceCollection(GameTime gameTime, List<planetStruct> planetList, newShipStruct tCollector,
            ref int playerTethAmount, ref int playerAMAmount)
        {
            int newAmount = 0;
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            foreach (planetStruct cPlanet in planetList)
            {
                //MessageClass.messageLog.Add("Updating Resource Collection from" +  cPlanet.planetName);
                BoundingSphere extPlanetBS = new BoundingSphere(cPlanet.planetPosition, cPlanet.planetRadius * 1200);
                if (tCollector.modelBoundingSphere.Intersects(extPlanetBS))
                {
                    tCollector.currentDisposition = disposition.mining;
                    if (currentTime - resourceList[(int)cPlanet.aResource].lastCollectTime > resourceList[(int)cPlanet.aResource].collectionTime)
                    {
                        MessageClass.messageLog.Add("(" + tCollector.objectAlias + ") Collecting Am from " + cPlanet.planetName);
                        resourceList[(int)cPlanet.aResource].lastCollectTime = currentTime;
                        //if (tCollector.techLevel == 1)
                            newAmount = 50;
                        switch (cPlanet.aResource)
                        {
                            case ResourceType.AntiMatter:
                                playerAMAmount += newAmount;
                                break;
                            case ResourceType.Tethanium:
                                playerTethAmount += newAmount;
                                break;
                        }

                    }
                }
            }
            return newAmount;
        }
    }
}

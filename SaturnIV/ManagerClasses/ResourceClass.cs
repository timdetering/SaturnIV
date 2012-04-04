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
        
        public void updateResourceCollection(GameTime gameTime, List<planetStruct> planetList, newShipStruct tCollector, int tethAmt,int amAmt, int mtlAmt)
        {
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
                    }
                }

            }            
        }
    }
}

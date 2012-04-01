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
        PlanetManager planetManager;

        public void Init(PlanetManager planetManager1)
        {
            planetManager = planetManager1;
        }
        
        public void updateResourceCollection(List<planetStruct> planetList, newShipStruct tCollector)
        {
            foreach (planetStruct cPlanet in planetManager.planetList)
            {
                //MessageClass.messageLog.Add("Updating Resource Collection from" +  cPlanet.planetName);
                BoundingSphere extPlanetBS = new BoundingSphere(cPlanet.planetPosition, cPlanet.planetRadius * 1000);
                if (tCollector.modelBoundingSphere.Intersects(extPlanetBS))
                {
                    //MessageClass.messageLog.Add("(" + tCollector.objectAlias + ") Collecting Am from " + cPlanet.planetName);   
                }

            }            
        }
    }
}

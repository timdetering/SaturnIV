using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace SaturnIV
{
    public class SaveClass
    {
        public void serializeClass(List<newShipStruct> activeShipList)
        {
            List<saveObject> saveList = new List<saveObject>();
            // Create the data to save
            saveObject saveMe;
            //weaponTypes exportWeaponDefs;
            shipData exportShipDefs = new shipData();
            // exportWeaponDefs = new weaponTypes();
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;

            foreach (newShipStruct ship in activeShipList)
            {
                saveMe = new saveObject();
                saveMe.shipPosition = ship.modelPosition;
                saveMe.shipDirection = ship.targetPosition;
                saveMe.shipName = ship.objectAlias;
                saveMe.shipType = ship.objectType;
                saveList.Add(saveMe);
            }

            using (XmlWriter xmlWriter = XmlWriter.Create("scene.xml", xmlSettings))
            {
                IntermediateSerializer.Serialize(xmlWriter, saveList, null);
            }
        }
    }
    [Serializable]
    public struct saveObject
    {
        public string shipType;
        public string shipName;
        public Vector3 shipPosition;
        public Vector3 shipDirection;
    }

    [Serializable]
    public class RandomNames
    {
        public List<string> capitalShipNames;
    }

}
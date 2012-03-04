using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace SaturnIV
{
        public class SerializerClass
        {
            public void loadMetaData(ref List<shipData> shipDefList, ref List<weaponData> weaponDefList, ref RandomNames rNameList)
            {
                XmlReaderSettings xmlSettings = new XmlReaderSettings();
                XmlReader xmlReader = XmlReader.Create("Content/XML/shipdefs.xml");
                shipDefList = IntermediateSerializer.Deserialize<List<shipData>>(xmlReader, null);
                xmlReader = XmlReader.Create("Content/XML/weapondefs.xml");
                weaponDefList = IntermediateSerializer.Deserialize<List<weaponData>>(xmlReader, null);
                xmlReader = XmlReader.Create("Content/XML/listofnames.xml");
                rNameList = IntermediateSerializer.Deserialize<RandomNames>(xmlReader, null);                
             }

            public static void exportWClass()
            {
                // Generate XML Template for Mech Data
                XmlWriterSettings xmlSettings = new XmlWriterSettings();
                xmlSettings.Indent = true;
                using (XmlWriter xmlWriter = XmlWriter.Create("weaponsdata.xml", xmlSettings))
                {
                }
            }

            public void exportSaveScenario(List<newShipStruct> activeShipList, string saveName)
            {
                List<saveObject> saveList = new List<saveObject>();
                // Create the data to save
                saveObject saveMe;                
                shipData exportShipDefs = new shipData();                
                XmlWriterSettings xmlSettings = new XmlWriterSettings();
                xmlSettings.Indent = true;

                foreach (newShipStruct ship in activeShipList)
                {
                    saveMe = new saveObject();
                    saveMe.shipPosition = ship.modelPosition;
                    saveMe.shipDirection = ship.targetPosition;
                    saveMe.shipName = ship.objectAlias;
                    saveMe.shipIndex = ship.objectIndex;
                    saveMe.side = ship.team;
                    saveList.Add(saveMe);
                }

                using (XmlWriter xmlWriter = XmlWriter.Create("Content/XML/Scenarios/" + saveName + ".xml", xmlSettings))
                {
                    IntermediateSerializer.Serialize(xmlWriter, saveList, null);
                }
            }

            public void loadScenario(string filename, ref List<newShipStruct> ShipList, ref List<shipData> shipDefList)
            {
                if (filename != null)
                {
                    ShipList.Clear();
                    List<saveObject> tempScenario = new List<saveObject>();
                    XmlReaderSettings xmlSettings = new XmlReaderSettings();
                    XmlReader xmlReader = XmlReader.Create("Content/XML/Scenarios/" + filename);
                    tempScenario = IntermediateSerializer.Deserialize<List<saveObject>>(xmlReader, null);
                    foreach (saveObject ship in tempScenario)
                    {
                        newShipStruct shipAdd = new newShipStruct();
                        shipAdd.objectAlias = ship.shipName;
                        ShipList.Add(EditModeComponent.spawnNPC(ship.shipPosition, ref shipDefList, ship.shipName, ship.shipIndex, ship.side));
                    }
                }

            }

            public void saveSystemList(string saveName, List<systemStruct> systemList)
            {
                XmlWriterSettings xmlSettings = new XmlWriterSettings();
                xmlSettings.Indent = true;
                using (XmlWriter xmlWriter = XmlWriter.Create("Content/XML/Systems/" + saveName + ".xml", xmlSettings))
                {
                    IntermediateSerializer.Serialize(xmlWriter, systemList, null);
                }
            }
    }        
}

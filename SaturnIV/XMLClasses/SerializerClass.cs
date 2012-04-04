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

            public void exportSaveScene(List<newShipStruct> activeShipList, List<planetStruct> planetList, Vector3 startPos, string saveName)
            {
                List<saveObject> saveShipList = new List<saveObject>();
                List<planetSaveStruct> savePlanetList = new List<planetSaveStruct>();
                saveName = "main_scene";
                // Create the data to save
                SceneSaveStruct saveMe = new SceneSaveStruct();
                XmlWriterSettings xmlSettings = new XmlWriterSettings();
                xmlSettings.Indent = true;

                saveMe.startingPosition = startPos;

                saveObject tmpObject;
                foreach (newShipStruct ship in activeShipList)
                {
                    tmpObject = new saveObject();
                    tmpObject.shipPosition = ship.modelPosition;
                    tmpObject.shipDirection = ship.targetPosition;
                    tmpObject.shipName = ship.objectAlias;
                    tmpObject.shipIndex = ship.objectIndex;
                    tmpObject.side = ship.team;
                    saveShipList.Add(tmpObject);
                }
                saveMe.initalObjectList = saveShipList;

                planetSaveStruct tmpObject2;
                foreach (planetStruct planet in planetList)
                {
                    tmpObject2 = new planetSaveStruct();
                    tmpObject2.aResource = planet.aResource;
                    tmpObject2.aResourceAmount = planet.aResourceAmount;
                    tmpObject2.isControlled = planet.isControlled;
                    tmpObject2.planetPosition = planet.planetPosition;
                    tmpObject2.planetRadius = planet.planetRadius;
                    tmpObject2.planetTextureFile = 1;
                    tmpObject2.planetName = planet.planetName;
                    savePlanetList.Add(tmpObject2);
                }
                saveMe.planetList = savePlanetList;

                using (XmlWriter xmlWriter = XmlWriter.Create("Content/XML/Scenarios/" + saveName + ".xml", xmlSettings))
                {
                    IntermediateSerializer.Serialize(xmlWriter, saveMe, null);
                }
            }

            public void loadScene(string filename, ref List<newShipStruct> ShipList, ref List<shipData> shipDefList, 
                ref Vector3 cameraStart, PlanetManager pManager)
            {
                filename = "main_scene.xml";
                    ShipList.Clear();
                    SceneSaveStruct newScene = new SceneSaveStruct();
                    List<saveObject> tempScenario = new List<saveObject>();
                    List<planetSaveStruct> tPlanetList = new List<planetSaveStruct>();
                    XmlReaderSettings xmlSettings = new XmlReaderSettings();
                    XmlReader xmlReader = XmlReader.Create("Content/XML/Scenarios/" + filename);
                    newScene = IntermediateSerializer.Deserialize<SceneSaveStruct>(xmlReader, null);
                    foreach (saveObject ship in newScene.initalObjectList)
                    {
                        newShipStruct shipAdd = new newShipStruct();
                        shipAdd.objectAlias = ship.shipName;
                        ShipList.Add(EditModeComponent.spawnNPC(ship.shipPosition, ref shipDefList, ship.shipName, ship.shipIndex, ship.side, false));
                    }

                    foreach (planetSaveStruct planet in newScene.planetList)
                    {
                        pManager.generatSpaceObjects(planet.planetTextureFile, planet.planetPosition, planet.planetRadius, planet.isControlled, planet.planetName,
                            planet.aResource, planet.aResourceAmount);
                    }
                    cameraStart = newScene.startingPosition;
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
                        ShipList.Add(EditModeComponent.spawnNPC(ship.shipPosition, ref shipDefList, ship.shipName, ship.shipIndex, ship.side, false));
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

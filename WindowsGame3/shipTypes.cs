using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{

    public enum disposition
    {
        pursue = 0,
        patrol = 1,
        evade = 2,
        loiter = 3
    }

    public class newShipStruct
    {
        public string objectFileName;
        public string objectAlias;
        public float objectMass;
        public float objectThrust;
        public float objectScale;
        public string objectDesc;
        public float objectAgility;
        public string objectClass;
        public float objectArmorFactor;
        public float objectArmorLvl;
        public float radius;
        public weaponTypes[] weaponArray;
        public Matrix worldMatrix;
        public Vector3 modelPosition;
        public Matrix modelRotation;
        public Vector3 vecToTarget;
        public BoundingSphere modelBoundingSphere;
        public BoundingFrustum modelFrustum;
        public Vector3 screenCords;
        public disposition npcDisposition;
        public Vector3 destination;
        public newShipStruct currentTarget;
        public Model shipModel;
        public float thrustAmount;
        public bool isVisable;
        public bool isSelected;
        public float distanceFromTarget;
        public Vector3 Velocity;
        public Vector3 Direction;
        public Vector3 Up;
        public Vector3 right;
        public Vector3 Right;
    }
    public class weaponStruct : newShipStruct
    {
        public bool isProjectile;
        public bool isHoming;
        public int regenTime;
        public int damageFactor;
        public Vector3 missileOrigin;
        public float distanceFromOrigin;
        public float range;
        public Color objectColor;
        public ParticleEmitter trailEmitter;
        public newShipStruct missileTarget;
    }

    [Serializable]
    public class genericObjectLoadClass
    {
        public string FileName;
        public string Desc;
        public float Mass;
        public float Thrust;
        public float SphereRadius;
        public float Scale;
        public float Agility;
        public string BelongsTo;
    }

    public class shipData : genericObjectLoadClass
    {
        public string shipClass;
        public int ShieldLvl;
        public int ShieldRegenTime;
        public weaponTypes.MissileType[] shipWeapons2;
    }

    public class weaponData : genericObjectLoadClass
    {
        public bool  isProjectile;
        public bool  isHoming;
        public int   regenTime;
        public int   damageFactor;
        public float range;
        
    }

    public struct classData
    {
        public string className;
        public float evadeDistance;
    }

    [Serializable]
    public class shipTypes
    {
        public enum Ships
        {
            orion,
            antares,
            procyon,
            ts140,
            banshee,
            tiger,
            hercules
        }

        public string[] ShipModelFileName = { "Models//eriax_battleship", "Models//eriax_light_crusier", "Models//fleet_heavy_battleship", "Models//fleet_sf-1a", "Models//fleet_sf-1a", "Models//fleet_sf-1a", "Models//fleet_sf-1a" };
        public string[] objectDesc = { "Eriax Battleship", "Eriax Light Crusier", "Eriax Heavy Crusier", "TS-140 Transport Shuttle",
                                              "Fleet SF-4B", "SF-1A Fighter", "Hercules Class Cargo Ship" };
        public shipClasses.ClassesEnum[] objectClass = { shipClasses.ClassesEnum.carrier,shipClasses.ClassesEnum.crusier, 
                                                                shipClasses.ClassesEnum.battleship,shipClasses.ClassesEnum.cargo, 
                                                                shipClasses.ClassesEnum.fighter,shipClasses.ClassesEnum.fighter,
                                                                shipClasses.ClassesEnum.cargo };
        public int[] ShipArmorLvl = { 100, 100, 100, 100, 100, 100 };
        public int[] ShipShieldLvl = { 100, 100, 100, 100, 100, 100 };
        public float[] ShipArmorFactor = { 10, 10, 5, 75, 70, 75, 50 };
        public float[] ShipShieldRegenTime = { 1500, 1600, 1500, 1700, 1700, 2000, 2100 };
        public float[] ShipMaxSpeed = { 30.0f, 30.0f, 30.0f, 30.0f, 80.0f, 80.0f, 30.0f };
        public float[] mass = { 7.0f, 7.0f, 7.0f, 7.0f, 2.0f, 2.5f, 10.0f };
        public float[] thrust = { 250.0f, 250.0f, 250.0f, 350.0f, 400.0f, 650.0f, 150.0f };
        public float[] sphereRadius = { 7.0f, 7.0f, 285.0f, 7.0f, 17.0f, 2.5f, 10.0f };
        public Vector3[] scale = { new Vector3(1.0f,1.0f,1.0f), new Vector3(1.0f,1.0f,1.0f),
                                          new Vector3(1.0f,1.0f,1.0f), new Vector3(1.0f,1.0f,1.0f),
                                          new Vector3(1.0f,1.0f,1.0f), new Vector3(1.0f,1.0f,1.0f),
                                          new Vector3(1.0f,1.0f,1.0f)};
        public float[] agility = { 2.0f, 2.2f, 2.0f, 3.0f, 4.5f, 4.0f, 3.0f };
        public weaponTypes.MissileType[][] shipWeapons = new weaponTypes.MissileType[][] 
            {
                new weaponTypes.MissileType[] {weaponTypes.MissileType.KM200,weaponTypes.MissileType.LargeIonCanon,weaponTypes.MissileType.KM200},
                new weaponTypes.MissileType[] {weaponTypes.MissileType.KM200,weaponTypes.MissileType.LargeIonCanon,weaponTypes.MissileType.KM200},
                new weaponTypes.MissileType[] {weaponTypes.MissileType.LargeIonCanon,weaponTypes.MissileType.KM200},
                new weaponTypes.MissileType[] {weaponTypes.MissileType.AC10,weaponTypes.MissileType.KM100},
                new weaponTypes.MissileType[] {weaponTypes.MissileType.KM100,weaponTypes.MissileType.AC10},
                new weaponTypes.MissileType[] {weaponTypes.MissileType.AC10,weaponTypes.MissileType.KM100},
                new weaponTypes.MissileType[] {weaponTypes.MissileType.AC10}
            };
        public int[] missileCount = { 50, 50, 25, 20, 20, 15, 0 };

        public Vector3[][] turretLocations = new Vector3[][] 
            {
                new Vector3[] {new Vector3(-125,100,150),new Vector3(-125,100,50),new Vector3(-125,100,-150),new Vector3(-125,100,-250),
                               new Vector3(125,100,150),new Vector3(125,100,50),new Vector3(125,100,-150),new Vector3(125,100,-250)},
                new Vector3[] {new Vector3(-125,100,150),new Vector3(-125,100,50),new Vector3(-125,100,-150),new Vector3(-125,100,-250),
                               new Vector3(125,100,150),new Vector3(125,100,50),new Vector3(125,100,-150),new Vector3(125,100,-250)},
                new Vector3[] {new Vector3(-6,6,10)},
                new Vector3[] {new Vector3(-6,1,1),new Vector3(6,1,1),new Vector3(-6,1,1),new Vector3(6,1,1)},
                new Vector3[] {new Vector3(-6,1,1),new Vector3(6,1,1),new Vector3(-6,1,1),new Vector3(6,1,1)},
                new Vector3[] {new Vector3(-6,1,1),new Vector3(6,1,1),new Vector3(-6,1,1),new Vector3(6,1,1)},
                new Vector3[] {new Vector3(-6,6,10)}
            };
    }

    public class shipClasses
    {
        public enum ClassesEnum
        {
            fighter,
            battleship,
            crusier,
            carrier,
            civilian,
            cargo
        }
        public string[] classDesc = {"Fighter","BattleShip","Crusier","Carrier","Civilian Ship","Cargo Ship" };
        public int[][] classEvadeDistance = new int[][] 
            {
                new int[] {100,300,300,300,100,100},
                new int[] {10,200,800,700,500,1000},
                new int[] {10,200,1400,600,500,1000},
                new int[] {10,200,1500,1400,1100,1100},
                new int[] {200,200,800,700,500,1000},
                new int[] {200,200,1400,600,500,1000},
            };
        }

    [Serializable]
    public class weaponTypes
    {
        public enum MissileType
        {
            KM100,
            KM200,
            AC10,
            LargeIonCanon
        }
        public static string[] ModelFileName = { "Models//LaserBolt2", "Models//KM200", "Models//LaserBolt2", "Models//LaserBolt2" };
        public static string[] weaponDesc = { "KM-100", "KM-200", "AC-10 Auto Canon", "Large Ion Canon" };
        public static int[] weaponMaxRange = { 7000, 8000, 5000, 7000 };
        public static float[] mass = { 0.50f, 1.50f, 4.0f, 4.0f };
        public static float[] thrust = {900.0f, 900.0f, 1500.0f, 800.0f };
        public static Vector3[] scale = { new Vector3(5.0f,5.0f,5.0f), new Vector3(50.0f,50.0f,75.0f), 
                                          new Vector3(10f,10f,15f), new Vector3(36.0f,36.0f,66.0f) };
                                           
        public static bool[] isProjectile = { true, true, false, false };
        public static bool[] isGudied = { true, true, false, false };
        public static Color[] laserColor = { Color.Blue,Color.White, Color.WhiteSmoke, Color.Green };
        public static float[] agility = { 6.0f, 3.0f, 10.0f, 15.0f };
        public static int[] regenTime = { 3600, 3600, 1000, 1500 };
        public static int[] damage = { 15,25,5,10 };
    }


}

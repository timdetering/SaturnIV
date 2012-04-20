using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{
    public enum PacketTypes
    {
        LOGIN,
        GETOBJECTS,
        REMOVE,
        UPDATE,
        ADD
    }

    public enum disposition
    {        
        engaging = 0,
        patrol = 1,
        idle = 2,
        moving = 3,
        defensive = 4,
        nofire = 5,
        mining = 6,
        building = 7
    }

    public enum MenuActions
    {
        action = 0,
        build = 1,
        none = 2
    }
    
    public enum SquadDisposition
    {
        tight = 0,
        formup = 1,
        engage = 2,
        breakform = 3
    }

    public enum WeaponClassEnum
    {
        Cannon = 0,
        Missile = 1,
        Torpedo = 2,
        Beam = 3
    }

    public enum WeaponTypeEnum
    {
        PointDefense = 0,
        SRM = 1,
        SRMH = 2,
        LRM = 3,
        LRMH = 4,
        AutoCannon = 5,
        LargeIon = 6,
        MassDriver = 7,        
    }

    public class newShipStruct
    {
        public int objectIndex;
        public string objectFileName;
        public string objectAlias;
        public float objectMass;
        public float objectThrust;
        public float objectScale;
        public string objectType;
        public float objectAgility;
        public ClassesEnum objectClass;
        public float hullFactor;
        public float hullLvl;
        public float shieldFactor;
        public float shieldLvl;
        public float hullValue;
        public float shieldValue;
        public float radius;
        public int techLevel;
        public Matrix worldMatrix;
        public Vector3 modelPosition;
        public Vector3 editModeOffset;
        public Matrix modelRotation;
        public BoundingSphere modelBoundingSphere;
        public BoundingBox modelBB;
        public BoundingFrustum modelFrustum;
        public BoundingFrustum starboardFrustum;
        public BoundingFrustum portFrustum;
        public Vector3 screenCords;
        public disposition npcDisposition;
        public Vector3 targetPosition;
        public Vector3 wayPointPosition;
        public newShipStruct currentTarget;
        public disposition currentDisposition;
        public Model shipModel;
        public float thrustAmount;
        public float distanceFromTarget;
        public Vector3 Velocity;
        public Vector3 Direction;
        public Vector3 Up;
        public Vector3 right;
        public Vector3 Right;
        public Athruster shipThruster;
        public Vector3 ThrusterPosition;
        public Matrix viewMatrix;
        public Matrix projectionMatrix;
        public bool ThrusterEngaged = false;
        public bool isVisable;
        public bool isSelected;
        public bool isGroupSelected;
        public bool isEngaging;
        public bool isEvading;
        public bool isPursuing;
        public bool isAlreadyEngaged;
        public bool canEngageMultipleTargets;
        public bool isBuilding;
        public bool userOverride;
        public float[] EvadeDist;
        public float[] TargetPrefs;
        public bool isBehind;
        public int[] ChasePrefs;
        public double lastWeaponFireTime;
        public WeaponModule[] weaponArray;
        public List<BoundingFrustum> moduleFrustum;
        public WeaponModule currentWeapon;
        public int pylonIndex = 0;
        public double angleOfAttack;
        public float currentTargetLevel;
        public int currentTargetIndex;
        public float runDistance;
        public Vector3 vecToTarget;
        public float modelLen;
        public float modelWidth;
        public float speed;
        public float maxDetectRange;
        public bool isSquad;
        public int squadNo = -1;
        public bool isSquadLeader;
        public int team;
        public newShipStruct threat;
        public Texture2D hudTex;
        public double timer;
        public double[] regenTimer;
        public int currentWeaponMod;
        public bool isStationary;
        public BuildManager buildManager;
    }

    public class weaponStruct : newShipStruct
    {
        public bool isProjectile;
        public bool isHoming;
        public int regenTime;
        public int damageFactor;
        public newShipStruct missileOrigin;
        public float distanceFromOrigin;
        public float range;
        public Color objectColor;
        public ParticleEmitter trailEmitter;
        public Projectile projectile;
        public newShipStruct missileTarget;
        public double timeToLive;
        public WeaponClassEnum objectClass;
        public Quad beamQuad;
        public int modIndex;
    }

    [Serializable]
    public class genericObjectLoadClass
    {
        public string FileName;
        public string Type;
        public int TechLevel;
        public float Mass;
        public float Thrust;
        public float SphereRadius;
        public float Scale;
        public float Agility;
        public string BelongsTo;
    }

    public class shipData : genericObjectLoadClass
    {
        public ClassesEnum ShipClass;
        public float ShieldFactor;
        public float HullFactor;
        public float hullValue;
        public float shieldValue;
        public int ShieldRegenTime;
        public float[] EvadeDist;
        public float[] TargetPrefs;
        public int[] ChasePrefs;
        public float maxDetectRange;
        public bool isStationary;
        public WeaponModule[] AvailableWeapons;
        public Vector3 ThrusterPosition;
    }

    public class weaponData : genericObjectLoadClass
    {
        public WeaponClassEnum wClass;
        public bool  isProjectile;
        public bool  isHoming;
        public int   regenTime;
        public int   damageFactor;
        public float range;
        public int timeToLive;
    }

    public class WeaponModule
    {
        public WeaponTypeEnum weaponType;
        public Vector4[] ModulePositionOnShip;
        public float FiringEnvelopeAngle;
        public float weaponRange;
        public double timer;
    }

    public class squadClass
    {
        public int squadNum;
        public List<newShipStruct> squadmate;
        public newShipStruct leader;
        public SquadDisposition squadOrders;
    }

        public enum ClassesEnum
        {
            Fighter = 0,
            Crusier = 1,
            Capitalship = 2,
            Frigate = 3,
            Bomber = 4,
            SWACS = 5,
            Transport = 6,
            Platform = 7,
            Collector = 8,
            Station = 9,
            Constructor = 10
        }
        
        public enum DirectionEnum
        {
            front = 0, // Bow
            rear = 1, // Stern
            left = 2, // Port
            right = 3 // Starboard
        }

        public enum ResourceType
        {
            Tethanium = 0,
            AntiMatter = 1,
            Metal = 2
        }

        public class planetStruct
        {
            public Vector3 planetPosition;
            public Texture2D planetTexture;
            public int planetRadius;
            public Model planetModel;
            public BoundingSphere planetBS;
            public int isControlled;
            public Vector3 screenCoords;
            public string planetName;
            public bool isSelected;
            public ResourceType aResource;
            public int aResourceAmount;
        }

        public class planetSaveStruct
        {
            public int planetTextureFile;
            public int planetRadius;
            public Vector3 planetPosition;
            public int isControlled;
            public string planetName;
            public ResourceType aResource;
            public int aResourceAmount;
        }

        public class ResourceStruct
        {
            public ResourceType resourceType;
            public int collectionTime;
            public double lastCollectTime;
        }

        public enum TextBoxActions
        {
            SaveScenario,
            LoadScenario,
            SaveMap,
            LoadMap,
            None
        }

        public enum MouseActions
        {
            None,
            AddUnit,
            MoveUnit,
            SelectUnit,
            PaintHex
        }

        public enum GameModes
        {
            None,
            EditMode,
            UnitCommandMode,
            CombatMode,
            MoveMode
        }

        [Serializable]
        public struct saveObject
        {
            public int shipIndex;
            public string shipName;
            public Vector3 shipPosition;
            public Vector3 shipDirection;
            public int side;
        }

        public class SceneStruct
        {
            public int sceneID;
            public string sceneName;
            public List<planetSaveStruct> planetList;
            public List<saveObject> initalObjectList;
            public Vector3 startingPosition;
            public List<newShipStruct> shipList;
        }

        public class SceneSaveStruct
        {
            public int sceneID;            
            public string sceneName;                        
            public List<planetSaveStruct> planetList;
            public List<saveObject> initalObjectList;
            public Vector3 startingPosition;
        }

        public class systemStruct
        {
            public SceneSaveStruct systemScene;
            public List<newShipStruct> systemShipList;
            public PlanetManager pManager;
            public Vector3 lastCameraPos;
            public BuildManager buildManager;
            public WeaponsManager weaponsManager;
        }

        [Serializable]
        public class RandomNames
        {
            public List<string> capitalShipNames;
        }

        public enum BuildStates
        {
            notstarted = 0,
            started = 1,
            building = 2,
            done = 3
        }

        public class buildItem
        {
            public BuildStates buildState;
            public Vector3 pos;
            public string name;
            public int shipType;
            public int side;
            public double startTime;
            public float percentComplete;
        }

        public struct MenuItem
        {
            public string itemText;
            public Rectangle itemRectangle;
            public int itemIndex;
            public bool itemSelected;
        }
}

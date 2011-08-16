using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{
    public enum disposition
    {        
        engaging = 0,
        patrol = 1,
        evade = 2,
        idle = 3,
        moving = 4,
        onstation = 5,
        defensive = 6,
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
        SRM = 0,
        SRMH = 1,
        LRM = 2,
        LRMH = 3,
        AutoCannon = 4,
        LargeIon = 5,
        MassDriver = 6
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
        public Matrix worldMatrix;
        public Vector3 modelPosition;
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
        public disposition preEvadeDisposition;
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
        public bool isEngaging;
        public bool isEvading;
        public bool isChasing;
        public float[] EvadeDist;
        public float[] TargetPrefs;
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
        public bool isSquad;
        public int squadNo = -1;
        public bool isSquadLeader;
        public int team;
        public newShipStruct threat;
        public Texture2D hudTex;
        public double timer;
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
        public double timer;
        public WeaponClassEnum objectClass;
        public Quad beamQuad;
    }

    [Serializable]
    public class genericObjectLoadClass
    {
        public string FileName;
        public string Type;
        public float Mass;
        public float Thrust;
        public float SphereRadius;
        public float Scale;
        public float Agility;
        public string BelongsTo;
    }

    public class shipData : genericObjectLoadClass
    {
        public string ThumbFileName;
        public ClassesEnum ShipClass;
        public float ShieldFactor;
        public float HullFactor;
        public float hullValue;
        public float shieldValue;
        public int ShieldRegenTime;
        public float[] EvadeDist;
        public float[] TargetPrefs;
        public int[] Chase;
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
            Carrier = 3,
            Neutral = 4
        }

        public enum DirectionEnum
        {
            front = 0, // Bow
            rear = 1, // Stern
            left = 2, // Port
            right = 3 // Starboard
        }

        public class planetStruct
        {
            public Vector3 planetPosition;
            public Texture2D planetTexture;
            public int planetRadius;
            public Model planetModel;
            public BoundingSphere planetBS;
        }

}

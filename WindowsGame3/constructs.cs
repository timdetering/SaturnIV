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

    public enum WeaponClassEnum
    {
        Cannon = 0,
        Missile = 0,
        Torpedo = 0,
        Energy = 0
    }

    public enum WeaponTypeEnum
    {
        KM100 = 0,
        KM200G = 1,
        AC10 = 2
    }

    public class newShipStruct
    {
        public string objectFileName;
        public string objectAlias;
        public float objectMass;
        public float objectThrust;
        public float objectScale;
        public string objectType;
        public float objectAgility;
        public ClassesEnum objectClass;
        public float objectArmorFactor;
        public float objectArmorLvl;
        public float objectShieldFactor;
        public float objectShieldLvl;
        public float radius;
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
        public string team;
        public float[] EvadeDist;
        public float[] TargetPrefs;
        public double lastWeaponFireTime;
        public WeaponModule[] weaponArray;
        public WeaponModule currentWeapon;
        public int pylonIndex = 0;
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
        public ClassesEnum ShipClass;
        public int ShieldLvl;
        public int ShieldRegenTime;
        public float[] EvadeDist;
        public float[] TargetPrefs;
        public WeaponModule[] AvailableWeapons;
        public Vector3 ThrusterPosition;
    }

    public class weaponData : genericObjectLoadClass
    {
        public WeaponTypeEnum wType;
        public WeaponClassEnum wClass;
        public bool  isProjectile;
        public bool  isHoming;
        public int   regenTime;
        public int   damageFactor;
        public float range;
    }

    [Serializable]
    public class WeaponModule
    {
        public WeaponTypeEnum weaponType;
        public Vector3[] ModulePositionOnShip;
        public float FiringEnvelopeAngle;
    }

        public enum ClassesEnum
        {
            Fighter = 0,
            Capitalship = 1,
            Carrier = 2,
            SWACS = 3
        }
}

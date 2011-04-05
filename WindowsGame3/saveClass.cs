using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SaturnIV
{
    [Serializable]
    public struct saveObject
    {
        public string shipType;
        public string shipName;
        public Vector3 shipPosition;
        public Vector3 shipDirection;

    }

    [Serializable]
    public class randomNames
    {
        public List<string> capitalShipNames; // = { "Stormhawk","Senlac","Vanguard","Vendetta","Ultima","Relentless","Relentless",
                                         //"Stalker","Skyhook","Reckoning","Malice","Red Gauntlet","Nitsa","Kreiger",
                                         //"Hydra","Leonides","Grey Wolf","Freedom","Intrepid","Tyrant","Tecumseh","Inexorable","Devastator","Vengeance",
                                         //"Thunderflare","Red October","Nova Scotia","Manticore","Bismark","Steadfast","Direption" };
    }

}

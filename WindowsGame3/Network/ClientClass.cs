using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
namespace SaturnIV
{
    class gameClient
    {
        NetClient client;
        NetPeerConfiguration config;
        public void initializeNetwork()
        {
            config = new NetPeerConfiguration("saturniv"); // needs to be same on client and server!
            client = new NetClient(config);
            client.Connect("127.0.0.1", 14242);
        }
    }
}

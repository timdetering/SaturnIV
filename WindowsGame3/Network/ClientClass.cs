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
            client.Start();
            client.Connect("192.168.0.5", 14242);

        }

        public void Update()
        {
            NetOutgoingMessage sendMsg = client.CreateMessage();
            sendMsg.Write("Chat");
            client.SendMessage(sendMsg, NetDeliveryMethod.ReliableOrdered);
        }
    }
}

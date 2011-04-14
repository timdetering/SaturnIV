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

        public void Update()
        {
            //
            // Collect input
            //
            int xinput = 0;
            int yinput = 0;

            if (xinput != 0 || yinput != 0)
            {
                //
                // If there's input; send it to server
                //
                NetOutgoingMessage om = client.CreateMessage();
                om.Write(xinput); // very inefficient to send a full Int32 (4 bytes) but we'll use this for simplicity
                om.Write(yinput);
                client.SendMessage(om, NetDeliveryMethod.Unreliable);
            }

            // read messages
            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        // just connect to first server discovered
                        client.Connect(msg.SenderEndpoint);
                        break;
                    case NetIncomingMessageType.Data:
                        // server sent a position update
                        long who = msg.ReadInt64();
                        int x = msg.ReadInt32();
                        int y = msg.ReadInt32();
                        break;
                }
            }
        }
    }
}

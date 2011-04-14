using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
namespace SaturnIV
{
    class gameClient
    {
        String newChatMsg;
        NetClient client;
        NetPeerConfiguration config;
        public void initializeNetwork()
        {
            config = new NetPeerConfiguration("saturniv"); // needs to be same on client and server!
            client = new NetClient(config);
            client.Start();
            client.Connect("192.168.0.5", 14242);

        }

        public void Update(String send)
        {
        }

        public void SendChat(String send)
        {
            if (send != null)
            {
                if (send.Length > 0)
                    send = send.Substring(0, 1);
                NetOutgoingMessage sendMsg = client.CreateMessage();
                sendMsg.Write(send);
                client.SendMessage(sendMsg, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}

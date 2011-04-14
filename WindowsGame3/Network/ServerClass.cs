using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lidgren.Network;

namespace SaturnIV
{
    public class gameServer
    {
        NetServer server;
        NetPeerConfiguration config;
        public double nextSendUpdates;
        public string fromClient;
        public int clientsConnected;
        NetIncomingMessage incMsg;

        public void initializeServer()
        {
            config = new NetPeerConfiguration("saturniv");
            //config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = 14242;

            // create and start server
            server = new NetServer(config);
            server.Start();
            nextSendUpdates = NetTime.Now;
        }

        public void update()
        {
            NetIncomingMessage msg;
            clientsConnected = server.ConnectionsCount;
            while ((msg = server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(msg.ReadString());
                        break;
                    case NetIncomingMessageType.Data:
                        fromClient += msg.ReadString();
                        break;
                }
                server.Recycle(msg);

            }
        }
        //
        // send position updates 30 times per second
        //
        //double now = NetTime.Now;
        //  if (now > nextSendUpdates)
        //  {

    }
}

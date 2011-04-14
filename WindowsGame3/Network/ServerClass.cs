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
        
        public void initializeServer()
        {
            config = new NetPeerConfiguration("saturniv");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = 14242;

            // create and start server
            server = new NetServer(config);
            server.Start();
            nextSendUpdates = NetTime.Now;
        }

        public void update()
        {
            NetIncomingMessage msg;
            while ((msg = server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryRequest:
                        //
                        // Server received a discovery request from a client; send a discovery response (with no extra data attached)
                        //
                        server.SendDiscoveryResponse(null, msg.SenderEndpoint);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        //
                        // Just print diagnostic messages to console
                        //
                        Console.WriteLine(msg.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        if (status == NetConnectionStatus.Connected)
                        {
                            //
                            // A new player just connected!
                            //
                            Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " connected!");
                            clientsConnected = 1;
                            // randomize his position and store in connection tag
                            msg.SenderConnection.Tag = new int[] {
									NetRandom.Instance.Next(10, 100),
									NetRandom.Instance.Next(10, 100)
								};
                        }

                        break;
                    case NetIncomingMessageType.Data:
                        //
                        // The client sent input to the server
                        //
                        string callsign = msg.ReadString();
                        fromClient = callsign;
                        Console.WriteLine("Sent from Client:" + callsign);
                        break;
                }

                //
                // send position updates 30 times per second
                //
                double now = NetTime.Now;
                if (now > nextSendUpdates)
                {
                    // Yes, it's time to send position updates

                    // for each player...
                    foreach (NetConnection player in server.Connections)
                    {
                        // ... send information about every other player (actually including self)
                        foreach (NetConnection otherPlayer in server.Connections)
                        {
                            // send position update about 'otherPlayer' to 'player'
                            NetOutgoingMessage om = server.CreateMessage();

                            // write who this position is for
                            om.Write(otherPlayer.RemoteUniqueIdentifier);

                            if (otherPlayer.Tag == null)
                                otherPlayer.Tag = new int[2];

                            int[] pos = otherPlayer.Tag as int[];
                            om.Write(pos[0]);
                            om.Write(pos[1]);

                            // send message
                            server.SendMessage(om, player, NetDeliveryMethod.Unreliable);
                        }
                    }

                    // schedule next update
                    nextSendUpdates += (1.0 / 30.0);
                }
            }

            // sleep to allow other processes to run smoothly
            Thread.Sleep(1);
        }
    }
}

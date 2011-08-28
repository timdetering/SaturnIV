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
        bool isFetched;
        List<saveObject> netSendList = new List<saveObject>();
        // Create the data to save
        saveObject sendMe;

        public void initializeServer()
        {
            config = new NetPeerConfiguration("saturniv");
            //config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = 14242;

            // create and start server
            server = new NetServer(config);
            server.Start();
            nextSendUpdates = NetTime.Now;
            MessageClass.messageLog.Add("Listening on 127.0.0.1");           
        }

        public void update(ref List<newShipStruct> shipList)
        {
            NetIncomingMessage msg;
            clientsConnected = server.ConnectionsCount;
            while ((msg = server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        MessageClass.messageLog.Add("step1");
                        break;
                    case NetIncomingMessageType.Data:
                        MessageClass.messageLog.Add("step2");
                        if (msg.ReadByte() == (byte)PacketTypes.GETOBJECTS)
                        {
                            MessageClass.messageLog.Add("Incoming Connection");

                            // Approve clients connection ( Its sort of agreenment. "You can be my client and i will host you" )
                            //msg.SenderConnection.Approve();

                            // Init random
                            Random r = new Random();

                            // Add new character to the game.
                            // It adds new player to the list and stores name, ( that was sent from the client )
                            // Random x, y and stores client IP+Port
                            //GameWorldState.Add(new Character(inc.ReadString(), r.Next(1, 40), r.Next(1, 20), inc.SenderConnection));

                            // Create message, that can be written and sent
                            NetOutgoingMessage outmsg = server.CreateMessage();
                           // if (!isFetched)
                           // {
                                outmsg.Write((byte)PacketTypes.GETOBJECTS);
                            //   isFetched = true;
                            //}


                            // then int
                            outmsg.Write(shipList.Count);

                            foreach (newShipStruct ship in shipList)
                            {
                                sendMe = new saveObject();
                                sendMe.shipPosition = ship.modelPosition;
                                sendMe.shipDirection = ship.targetPosition;
                                sendMe.shipName = ship.objectAlias;                                
                                sendMe.side = ship.team;
                                sendMe.shipIndex = ship.objectIndex;
                                outmsg.Write(sendMe.shipName);
                                outmsg.Write(sendMe.shipIndex);
                                outmsg.Write(sendMe.shipPosition.X);
                                outmsg.Write(sendMe.shipPosition.Y);
                                outmsg.Write(sendMe.shipPosition.Z);
                                MessageClass.messageLog.Add("Sending" + sendMe.shipName);
                            }
                            // Send message/packet to all connections, in reliably order, channel 0
                            // Reliably means, that each packet arrives in same order they were sent. Its slower than unreliable, but easyest to understand
                            server.SendMessage(outmsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            // Debug
                            MessageClass.messageLog.Add("Approved new connection and updated the world status");
                        }

                        break;
                    default:
                        Console.WriteLine(msg.ReadString());
                        break;
                    //case NetIncomingMessageType.Data:
                    //    fromClient += msg.ReadString();
                    //    MessageClass.messageLog.Add("Client:" + fromClient);
                    //    break;
                }
                server.Recycle(msg);
            }
        }
        public void SendChat(String send)
        {
            if (send != null)
            {
                NetOutgoingMessage sendMsg = server.CreateMessage();
                sendMsg.Write(send);
                //server.SendMessage(sendMsg,NetDeliveryMethod.Unreliable);
            }
        }
    }  
}

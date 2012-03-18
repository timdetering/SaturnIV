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
        bool isFirstRun;
        List<saveObject> netFetchList = new List<saveObject>();

        public void initializeNetwork(string ipAddress)
        {
            config = new NetPeerConfiguration("saturniv"); // needs to be same on client and server!
            client = new NetClient(config);
            // Create new outgoing message
            MessageClass.messageLog.Add("Attempting Connection to Server "+ipAddress);
            client.Start();
            client.Connect(ipAddress, 14242);
            //if (client.Status == NetPeerStatus.NotRunning)
            //    MessageClass.messageLog.Add("Failed to connect");
        }

        public void Update(ref List<newShipStruct> shipList, ref NPCManager npcManager)
        {
            MessageClass.messageLog.Add("Update");
             NetIncomingMessage msg;
                if ((msg = client.ReadMessage()) != null)
                {                     
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.StatusChanged:
                            if (!isFirstRun || shipList.Count == 0)
                            {
                                NetOutgoingMessage outmsg = client.CreateMessage();
                                outmsg.Write((byte)PacketTypes.GETOBJECTS);
                                client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                                isFirstRun = true;
                            }
                            break;
                        // All manually sent messages are type of "Data"
                        case NetIncomingMessageType.Data:
                            if (msg.LengthBytes > 0)
                            {
                               // if (msg.ReadByte() == (byte)PacketTypes.REMOVE)
                               // {
                               //     int objectIndex = msg.ReadInt32();
                               //     MessageClass.messageLog.Add("Remove Object Request: " + objectIndex);
                               // }

                                if (msg.ReadByte() == (byte)PacketTypes.GETOBJECTS)
                                {
                                    MessageClass.messageLog.Add("Initial Object Get Request");
                                    int count = 0;
                                    count = msg.ReadInt32();
                                    MessageClass.messageLog.Add("Ship Count: " + count);
                                    for (int i = 0; i < count; i++)
                                    {
                                        // Create new character to hold the data
                                        saveObject ch = new saveObject();
                                        //msg.ReadAllProperties(ch);
                                        ch.shipName = msg.ReadString();
                                        ch.shipIndex = msg.ReadInt32();
                                        ch.side = msg.ReadInt32();
                                        ch.shipPosition.X = msg.ReadFloat();
                                        ch.shipPosition.Y = msg.ReadFloat();
                                        ch.shipPosition.Z = msg.ReadFloat();
                                        MessageClass.messageLog.Add("Data: " + ch.shipName);
                                        //MessageClass.messageLog.Add("Data: " + ch.shipIndex);
                                        // netFetchList.Add(ch);
                                        shipList.Add(EditModeComponent.spawnNPC(ch.shipPosition, ref Game1.shipDefList, 
                                            ch.shipName, ch.shipIndex, ch.side, false));
                                    }
                                }
                                else 
                                    if (msg.ReadByte() == (byte)PacketTypes.REMOVE)
                                    {
                                        int oIndex;
                                        //if (msg.LengthBytes > 0)
                                        //    oIndex = msg.ReadInt32();
                                        MessageClass.messageLog.Add("Remove Request Gotten: ");
                                    }
                               //else
                               //     if (shipList.Count > 0)
                               //     {
                               //         MessageClass.messageLog.Add("Getting Updates from Server..");
                               //         int count = 0;
                               //         foreach (newShipStruct ship in shipList)
                               //         {
                               //             ship.thrustAmount = msg.ReadFloat();
                               //             ship.targetPosition.X = msg.ReadFloat();
                               //             ship.targetPosition.Y = msg.ReadFloat();
                               //             ship.targetPosition.Z = msg.ReadFloat();
                               //             MessageClass.messageLog.Add(" " + ship.objectAlias + " " + ship.Direction.X);
                               //         }
                               //     }
                            }
                            break;

                        default:
                            // Should not happen and if happens, don't care
                            Console.WriteLine(msg.ReadString() + " Strange message");
                            break;
                    }
                }
        }        

        public void SendChat(String send)
        {
            if (send != null)
            {
                NetOutgoingMessage sendMsg = client.CreateMessage();
                sendMsg.Write("theDude: "+send);
                client.SendMessage(sendMsg, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}

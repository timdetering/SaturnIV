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
        // Create the data to save
        saveObject fetchMe;

        public void initializeNetwork()
        {
            config = new NetPeerConfiguration("saturniv"); // needs to be same on client and server!
            client = new NetClient(config);
            // Create new outgoing message
            client.Start();
            client.Connect("127.0.0.1", 14242);
        }

        public void Update(ref List<newShipStruct> shipList, ref NPCManager npcManager)
        {           
             NetIncomingMessage msg;
                if ((msg = client.ReadMessage()) != null)
                {                     
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.StatusChanged:
                            if (!isFirstRun)
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
                                if (msg.ReadByte() == (byte)PacketTypes.GETOBJECTS)
                                {
                                    MessageClass.messageLog.Add("Getting Objects from Server..");
                                    int count = 0;
                                    count = msg.ReadInt32();
                                    MessageClass.messageLog.Add("Ship Count: " + count);
                                    for (int i = 0; i < count; i++)
                                    {
                                        // Create new character to hold the data
                                        saveObject ch = new saveObject();
                                        // Read all properties ( Server writes characters all props, so now we can read em here. Easy )
                                        //msg.ReadAllProperties(ch);
                                        ch.shipName = msg.ReadString();
                                        ch.shipIndex = msg.ReadInt32();
                                        ch.side = msg.ReadInt32();
                                        ch.shipPosition.X = msg.ReadFloat();
                                        ch.shipPosition.Y = msg.ReadFloat();
                                        ch.shipPosition.Z = msg.ReadFloat();
                                        MessageClass.messageLog.Add("Data: " + ch.shipName);
                                        MessageClass.messageLog.Add("Data: " + ch.shipIndex);
                                        netFetchList.Add(ch);
                                        shipList.Add(EditModeComponent.spawnNPC(npcManager, ch.shipPosition, ref Game1.shipDefList, 
                                            ch.shipName, ch.shipIndex, ch.side));
                                    }
                                }
                                else
                                    if (shipList.Count > 0)
                                    {
                                        //MessageClass.messageLog.Add("Getting Updates from Server..");
                                        int count = 0;
                                        count = msg.ReadInt32();
                                        for (int i = 0; i < count; i++)
                                        {
                                            shipList[i].thrustAmount = msg.ReadFloat();
                                            shipList[i].targetPosition.X = msg.ReadFloat();
                                            shipList[i].targetPosition.Y = msg.ReadFloat();
                                            shipList[i].targetPosition.Z = msg.ReadFloat();
                                           // MessageClass.messageLog.Add(" " + shipList[i].objectAlias + " " + shipList[i].Direction.X);
                                        }
                                    }
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

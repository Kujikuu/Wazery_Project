using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project_Terror_v2.AuthClient
{
    public unsafe class ClientObject
    {
        public class ClientInfomation
        {
            public string Account;
            public string Password;
            public uint UID;
            public byte Grade;

            public string Email;
        }
        public ServerSockets.Packet LoginPacket = null;
        public bool InLife = false;
        public ServerSockets.SecuritySocket Socket;
        private Cryptography.AuthCryptography Crypto;
        public ClientObject(ServerSockets.SecuritySocket _socket)
        {
            _socket.QueuePacket = new ServerSockets.ConcurrentPacketQueue(0);

            _socket.Client = this;
            InLife = true;
            Socket = _socket;
            Crypto = new Cryptography.AuthCryptography();
            Socket.SetCrypto(Crypto);
            Packets.PasswordCryptographySeed seed = Packets.PasswordCryptographySeed.Create();
            seed.Seed = Program.GetRandom.Next();
            Send((byte*)&seed);
            _socket.Receive();
        }
        public unsafe void Send(byte* packet)
        {
            Socket.Send(packet, 0);

        }

        public void Disconnect()
        {
            if (InLife)
            {
                InLife = false;
                //add save and others...
            }
        }
        public static unsafe void ProcesHandler(ClientObject client)
        {
            if (client.LoginPacket == null)
                return;
            Packets.LoginPacketInfo* LoginInformations = (Packets.LoginPacketInfo*)client.LoginPacket.Pointer;
            Packets.AuthResponsePacket AuthComfirm = Packets.AuthResponsePacket.Create();

   

            if (LoginInformations->PacketType == 1542)
            {
               /* ClientInfomation DatabaseInformation = new ClientInfomation();
                DatabaseInformation.Account = "2";
                DatabaseInformation.Password = "2";
                DatabaseInformation.UID = 1000003;
                //if (Database.ServerDatabase.LoadAuth(LoginInformations->User, LoginInformations->Password, out DatabaseInformation))
                {
                    int Hash = DatabaseInformation.Account.GetHashCode();
                    AuthComfirm.Identifier = Hash;
                    AuthComfirm.Type = DatabaseInformation.UID;
                    AuthComfirm.Port = Program.ServerConfig.GamePort;
                    AuthComfirm.IPAddress = Program.ServerConfig.IPAddres;
                    // AuthComfirm.Unknow = 733461308;//????
                    Database.Server.AuthQueues.TryAdd(Hash, DatabaseInformation);
                }*/
                ClientInfomation DatabaseInformation;
                if (Database.ServerDatabase.LoadAuth(LoginInformations->User, LoginInformations->Password, out DatabaseInformation))
                {
                    int Hash =DatabaseInformation.Account.GetHashCode();
                    AuthComfirm.Identifier = Hash;
                    AuthComfirm.Type = DatabaseInformation.UID;
                    AuthComfirm.Port = Program.ServerConfig.GamePort;
                    AuthComfirm.IPAddress = Program.ServerConfig.IPAddres;
                   // AuthComfirm.Unknow = 733461308;//????
                    Database.Server.AuthQueues.TryAdd(Hash, DatabaseInformation);
                }
                else
                    AuthComfirm.Type = 1;//invalid acc or password;
                
                client.Send((byte*)&AuthComfirm);
            }
        }
    }
}

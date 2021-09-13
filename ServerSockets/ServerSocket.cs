using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;

namespace LightConquer_Project.ServerSockets
{
    public class ServerSocket
    {
        private bool Alive = false;
        private Socket Connection;

        private string ServerAddresIP = "";
        public uint SPort, MaxClientsConnectionsCount;
        public string ServerName = "";

        private Action<SecuritySocket> ProcessConnection, ProcessDisconnect;
        private Action<SecuritySocket, Packet> ProcessReceive;
        private BruteforceProtection Bruteforce;
        public Extensions.MyList<SecuritySocket> Clients = new Extensions.MyList<SecuritySocket>();
        public ServerSocket(Action<SecuritySocket> _processConnection, Action<SecuritySocket, Packet> _procesreceive, Action<SecuritySocket> _processdisconnect)
        {
            ProcessConnection = _processConnection;
            ProcessReceive = _procesreceive;
            ProcessDisconnect = _processdisconnect;
            Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Initilize(ushort MaxBufferSend, ushort MaxBufferReceive, uint MaxAcceptConnections, uint MaxClientsConnections)
        {
            Connection.ReceiveBufferSize = MaxBufferReceive;
            Connection.SendBufferSize = MaxBufferSend;
            MaxClientsConnectionsCount = MaxClientsConnections;
        }

        public void Connect(string IpAddres, ushort port, string aServerName)
        {
            SPort = port;
            ServerAddresIP = IpAddres;
            ServerName = aServerName;
            TryConnect(aServerName);
        }
        private void TryConnect(string servername)
        {
            //Console.WriteLine("Server Trying connect to InterServer...");
            Connection.BeginConnect(ServerAddresIP, (int)SPort, new AsyncCallback(ConnectCallback), null);
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Connection.EndConnect(ar);

                //Console.WriteLine("Socket connected to {0} successful !",Connection.RemoteEndPoint.ToString());
                Console.WriteLine($"Socket connected to {Connection.RemoteEndPoint.ToString()} successful !");


                Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                // Set option that allows socket to receive out-of-band information in the data stream.
                Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.OutOfBandInline, true);

                Connection.ReceiveBufferSize = 16384;
                Connection.SendBufferSize = 16384;

                Alive = Connection.Connected;
                if (Alive)
                {
                    var client = new SecuritySocket(this, ProcessDisconnect, ProcessReceive);
                    client.Create(Connection);
                    if (ProcessConnection != null)
                    {
                        ProcessConnection(client);

                    }
                }
            }
            catch
            {

                TryConnect(ServerName);
            }
        }

        public void Open(string IpAddres, ushort port, int backlog)
        {
            Bruteforce = new BruteforceProtection();
            Bruteforce.Init(10);

            SPort = port;
            ServerAddresIP = IpAddres;
            Connection.Bind(new IPEndPoint(IPAddress.Any, port));

            Connection.Listen((int)100);
            Connection.Blocking = false;
            Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Connection.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            Console.WriteLine("Listening on port : " + port.ToString());
            Alive = true;


        }




        public void Accept()
        {

            try
            {

                if (Alive && Connection.Poll(0, SelectMode.SelectRead))
                {
                    if (Connection.Poll(1, SelectMode.SelectError))
                    {
                        var close_socket = Connection.Accept();
                        WindowsAPI.ws2_32.shutdown(close_socket.Handle, WindowsAPI.ws2_32.ShutDownFlags.SD_BOTH);
                        WindowsAPI.ws2_32.closesocket(close_socket.Handle);
                        Console.WriteLine("[Sockets] Error on socket Accept().");
                        return;
                    }
                    var socket = Connection.Accept();
                    string RemoteIP = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();

                    if (!Bruteforce.IsBanned(RemoteIP) || Bruteforce.AllowAddress(RemoteIP))
                    {
                        SecuritySocket user = new SecuritySocket(this, ProcessDisconnect, ProcessReceive);
                        user.Create(socket);
                        Clients.Add(user);
                        //Console.WriteLine("IP " + user.RemoteIp + " try to connect");
                        if (ProcessConnection != null)
                        {
                            ProcessConnection.Invoke(user);
                        }
                        user.ConnectFull = true;
                        Bruteforce.AddWatch(user.RemoteIp);
                    }
                    else
                    {
                        WindowsAPI.ws2_32.shutdown(socket.Handle, WindowsAPI.ws2_32.ShutDownFlags.SD_BOTH);
                        WindowsAPI.ws2_32.closesocket(socket.Handle);
                    }
                }
            }
            catch (Exception e) { MyConsole.SaveException(e); }


        }



        public void Close()
        {
            if (Alive)
            {
                Alive = false;
                Connection.Close(1);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.ServerSockets
{
    public class SocketThread
    {
        /*const int	RECV_BUFFER_SIZE	= 2048;			// Ó¦ÓÃ³ÌÐò×Ô¼ºµÄBUF
 const int	SEND_BUFFER_SIZE	= 4048;			// Ó¦ÓÃ³ÌÐò×Ô¼ºµÄBUF*/
        const int SOCKET_PROCESS_INTERVAL = 1;

        private Extensions.ThreadGroup.ThreadItem ThreadItem;
        private ServerSocket[] Sockets;

        public SocketThread(string GroupName, params ServerSocket[] Sockets)
        {
            this.Sockets = Sockets;
            ThreadItem = new Extensions.ThreadGroup.ThreadItem(SOCKET_PROCESS_INTERVAL, GroupName, Process);//asta inseamna totusi minim ping 40 da dar daca se folosesc 10 milisecunde cat trece prin toate... face 40-10 si face sleep(30);


        }
        public void Start()
        {
            ThreadItem.Open();
        }

        public void Process()
        {
            try
            {
                //ok deci aici e thread... am 40 ms in caz ca se folosesc 5... face sleep 35...
                foreach (var _socket in Sockets)//sockets adica am .. 1 socket ptr acc server 1 socket ptr loader.. si 1 socket ptr game ...
                {

                    try
                    {
                        if (_socket == null)//for inter server
                            continue;

                        _socket.Accept();
                    }
                    catch (Exception e)
                    {
                        MyConsole.SaveException(e);
                    }
                    /*   foreach (var user in _socket.Clients.GetValues())
                       {
                           try
                           {

                               while (user.ReceiveBuffer())
                               {
                               }
                               user.HandlerBuffer();
                           }
                           catch
                           {
                           }
                       }*/
                    // var array = _socket.Sockets.ToArray();

                    /* foreach (var user in array)
                     {
                         try
                         {

                             while (user.ReceiveBuffer())
                             {
                                 if (user.HandlerBuffer())
                                 {

                                 }
                             }
                             user.HandlerBuffer();





                             user.TrySend();


                         }
                         catch (Exception e) { Console.WriteLine(e.ToString()); }
                     }*/
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void Close()
        {
            ThreadItem.Close();
        }

    }
}

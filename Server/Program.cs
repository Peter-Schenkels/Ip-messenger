using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;


namespace Server
{
    class Server
    {
        [ThreadStatic]
        public static Socket listener, acc;
        private static int port = 1998;
        private static string LocalIP = "127.0.0.1";
        private static string messageRecieved;
        private static List<Client> Clients = new List<Client>();

        public class Client
        { 
            public Socket Socket { get; set; }
        }
        static void CreateServer()
        {
            while (true)
            {
                acc = listener.Accept();
                Client client = new Client();
                client.Socket = acc;
                Clients.Add(client);
                Console.WriteLine("Connecting new client");
                ParameterizedThreadStart pts = new ParameterizedThreadStart(Listeners);
                Thread thr = new Thread(pts);
                thr.IsBackground = true;
                thr.Start(client); //start client processing thread
                acc = null;
                client = null;
            }

        }

        static bool SocketConnected(Socket s)
        {
            if(s == null)
            {
                return false;
            }
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }


        static void bindSocket()
        {
            Console.WriteLine("Connecting");
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine(LocalIP);
            listener.Bind(new IPEndPoint(IPAddress.Any, port));
            listener.Listen(0);
        }
        static void Main(string[] args)
        {
            bindSocket();
            CreateServer();
        }

        static void Listeners(object Object)
        {
            Client client = (Client)Object;
            while (true)
            {
                if (SocketConnected(client.Socket))
                {
                    Recieve(client.Socket);
                    Send();
                }
                else
                {
                    if (listener != null && client != null)
                    {
                        listener.Close();
                        client.Socket.Close();
                    }
                }
            }

        }

        static void Send()
        {
            if(messageRecieved != null)
            {
                string msg = messageRecieved;
                byte[] msgBuffer = Encoding.Default.GetBytes(msg);
                foreach(Client client in Clients)
                {
                    client.Socket.Send(msgBuffer, 0, msgBuffer.Length, 0);
                }
                
            }
            messageRecieved = null;



        }

        static void Recieve(Socket client)
        {
            try
            {
                byte[] buffer = new byte[255];
                int recieved = client.Receive(buffer, 0, buffer.Length, 0);
                Array.Resize(ref buffer, recieved);
                messageRecieved = Encoding.Default.GetString(buffer);
            }
            catch
            {
                Console.WriteLine("disconnected");
            }

        }
    }


    class Client
    {


    }


}
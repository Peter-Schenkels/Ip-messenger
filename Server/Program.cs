﻿using System;
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
        public static Socket listener;

        private static int port = 1998;
        private static string LocalIP = "127.0.0.1";
        private static byte[] messageRecieved = new byte[1000000];
        private static bool newMessage = false;
        private static List<Client> Clients = new List<Client>();

        public class Client
        {
            public Socket Socket { get; set; }
            public Thread Thread { get; set; }
        }

        static void CreateServer()
        {
            while (true)
            {
                var acc = listener.Accept();
                Client client = new Client();
                client.Socket = acc;
                Clients.Add(client);
                Console.WriteLine("Connecting new client");
                ParameterizedThreadStart pts = new ParameterizedThreadStart(Listeners);
                client.Thread = new Thread(pts);
                client.Thread.IsBackground = true;
                client.Thread.Start(client); //start client processing thread
            }
        }

        static bool SocketConnected(Socket s)
        {
            if (s == null)
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
                        client.Thread.Abort();
                    }
                }
            }
        }

        static void Send()
        {
            if (newMessage)
            {
                foreach (Client client in Clients)
                {
                    client.Socket.Send(messageRecieved, 0, messageRecieved.Length, 0);
                }

            }
            newMessage = false;
        }

        static void Recieve(Socket client)
        {
            try
            {
                byte[] buffer = new byte[1000000];
                int recieved = client.Receive(buffer, 0, buffer.Length, 0);
                Array.Resize(ref buffer, recieved);
                Array.Resize(ref messageRecieved, recieved);
                Array.Copy(buffer, messageRecieved, buffer.Length);

                newMessage = true;
            }
            catch
            {
                Console.WriteLine("disconnected");
            }
        }
    }
}
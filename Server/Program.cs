namespace Server
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Defines the <see cref="Server" />.
    /// </summary>
    internal class Server
    {
        /// <summary>
        /// Defines the listener.
        /// </summary>
        [ThreadStatic]
        public static Socket listener;

        /// <summary>
        /// Defines the port.
        /// </summary>
        private static int port = 1998;

        /// <summary>
        /// Defines the LocalIP.
        /// </summary>
        private static string LocalIP = "127.0.0.1";

        /// <summary>
        /// Defines the messageRecieved.
        /// </summary>
        private static byte[] messageRecieved = new byte[1000000];

        /// <summary>
        /// Defines the newMessage.
        /// </summary>
        private static bool newMessage = false;

        /// <summary>
        /// Defines the Clients.
        /// </summary>
        private static List<Client> Clients = new List<Client>();

        /// <summary>
        /// Defines the <see cref="Client" />.
        /// </summary>
        public class Client
        {
            /// <summary>
            /// Gets or sets the Socket.
            /// </summary>
            public Socket Socket { get; set; }

            /// <summary>
            /// Gets or sets the Thread.
            /// </summary>
            public Thread Thread { get; set; }
        }

        /// <summary>
        /// The CreateServer.
        /// </summary>
        internal static void CreateServer()
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

        /// <summary>
        /// The SocketConnected.
        /// </summary>
        /// <param name="s">The s<see cref="Socket"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal static bool SocketConnected(Socket s)
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

        /// <summary>
        /// The bindSocket.
        /// </summary>
        internal static void bindSocket()
        {
            Console.WriteLine("Connecting");
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine(LocalIP);
            listener.Bind(new IPEndPoint(IPAddress.Any, port));
            listener.Listen(0);
        }

        /// <summary>
        /// The Main.
        /// </summary>
        /// <param name="args">The args<see cref="string[]"/>.</param>
        internal static void Main(string[] args)
        {
            bindSocket();
            CreateServer();
        }

        /// <summary>
        /// The Listeners.
        /// </summary>
        /// <param name="Object">The Object<see cref="object"/>.</param>
        internal static void Listeners(object Object)
        {
            Client client = (Client)Object;
            while (true)
            {
                try
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
                catch
                {
                    Console.WriteLine("client disconnect");
                }
            }
        }

        /// <summary>
        /// The Send.
        /// </summary>
        internal static void Send()
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

        /// <summary>
        /// The Recieve.
        /// </summary>
        /// <param name="client">The client<see cref="Socket"/>.</param>
        internal static void Recieve(Socket client)
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

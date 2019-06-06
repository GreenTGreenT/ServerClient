using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Server_Tutorial
{
    class Program
    {


        //static void Main(string[] args)
        //{
        //    int port = 13000;
        //    string IpAddress = "127.0.0.1";

        //    //var myprotocol = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

        //    Socket ServerListener = new Socket(AddressFamily
        //        .InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IpAddress), port);
        //    ServerListener.Bind(ep);
        //    ServerListener.Listen(100);
        //    Console.WriteLine("Server is Listening...");
        //    Socket ClientSocket = default(Socket);
        //    int counter = 0;
        //    Program p = new Program();
        //    while (true)
        //    {
        //        counter++;
        //        ClientSocket = ServerListener.Accept();
        //        Console.WriteLine(counter + "Client connected");
        //        Thread UserThread = new Thread(new ThreadStart(() =>p.User(ClientSocket)));
        //        UserThread.Start();
        //    }
        //}
        //public void User(Socket client)
        //{   while (true)
        //    {
        //        byte[] msg = new byte[1024];
        //        int size = client.Receive(msg);
        //        client.Send(msg, 0, size, SocketFlags.None);
        //    }
        //}
        public static List<Session> all_session = new List<Session>();
        public class Session
        {
            public string id { get; set; }
            public TcpClient _socket { get; set; }

            public void receive()
            {
                //int id = (int)o;
                //Console.WriteLine(all_session);
                while (true)
                {
                    NetworkStream stream = _socket.GetStream();
                    //Console.WriteLine(stream);
                    byte[] buffer = new byte[1024];
                    int byte_count = stream.Read(buffer, 0, buffer.Length);
                    //Console.WriteLine(byte_count);

                    if (byte_count == 0)
                    {
                        break;
                    }


                    //verify
                    if(byte_count >=6)
                    {
                        if(buffer[0]==0xff)  //check header
                        {
                            int len = buffer[1];

                            byte[] data = new byte[byte_count - 3];

                            Array.Copy(buffer, 3, data, 0, data.Length);

                            string str = Encoding.ASCII.GetString(data, 0, data.Length);

                            Console.WriteLine(str);

                        }
                        else
                        {
                            Console.WriteLine("wrong header");
                        }
                    }
                    else{
                        Console.WriteLine("unknow package");
                    }


                    //test(); 
                    //string data = "1bedroom1";
                    ////string data = Encoding.ASCII.GetString(buffer, 0, byte_count); //It's string
                    ////broadcast(data);
                    //NetworkStream _stream = _socket.GetStream();
                    //byte[] _buffer = Encoding.ASCII.GetBytes(data);

                    ////string d_id = BitConverter.ToString(_buffer, 0);
                    ////string room = BitConverter.ToString(_buffer, 1, 7);
                    ////string status = BitConverter.ToString(_buffer, 8);

                    //_stream.Write(_buffer, 0, _buffer.Length);
                    //Console.WriteLine(_buffer.ToString());

                    //byte[] buffer = Encoding.ASCII.GetBytes(data);
                }

                //lock (_lock) list_clients.Remove(id);
                _socket.Client.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }

        //static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();

        static void Main(string[] args)
        {
            int count = 1;   //จะเอา count มาเป็น id

            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
            ServerSocket.Start();

            while (true)
            {
                
                TcpClient client = ServerSocket.AcceptTcpClient();
               
                Session new_session = new Session();
                new_session._socket = client; 
                new_session.id = count.ToString();
                all_session.Add(new_session);  
                //Console.WriteLine(new_session);
                
                Thread t = new Thread(new_session.receive);

                t.Start();  
                count++;
            }
        }

        //public static void handle_clients(object o)
        //{
        //    int id = (int)o;
        //    TcpClient client;

        //    lock (_lock) client = list_clients[id];

        //    while (true)
        //    {
        //        NetworkStream stream = client.GetStream();
        //        //Console.WriteLine(stream);
        //        byte[] buffer = new byte[1024];
        //        int byte_count = stream.Read(buffer, 0, buffer.Length);
        //        //Console.WriteLine(byte_count);

        //        if (byte_count == 0)
        //        {
        //            break;
        //        }
        //        //test(); 
        //        string data = "101bedroom100";
        //        //string data = Encoding.ASCII.GetString(buffer, 0, byte_count); //It's string
        //        broadcast(data);
               
        //        //byte[] buffer = Encoding.ASCII.GetBytes(data);
        //        Console.WriteLine(Encoding.ASCII.GetBytes(data));
        //    }

        //    lock (_lock) list_clients.Remove(id);
        //    client.Client.Shutdown(SocketShutdown.Both);
        //    client.Close();
        //}

        //public static void test()
        //{
        //    string data = "101bedroom100";
        //    //byte[] buffer = Encoding.ASCII.GetBytes(data);
        //    Console.WriteLine(Encoding.ASCII.GetBytes(data));
        //}

        //public static void broadcast(string data)
        //{
        //    byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

        //    lock (_lock)
        //    {
        //        foreach (TcpClient c in list_clients.Values)
        //        {
        //            NetworkStream stream = c.GetStream();

        //            stream.Write(buffer, 0, buffer.Length);
        //        }
        //    }
        //}

    }
    

   
}

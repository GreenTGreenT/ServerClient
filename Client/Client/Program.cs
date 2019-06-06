using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
   
    class Program
    {
        //static void Main(string[] args)
        //{
        //    int port = 11000;
        //    string IpAddress = "127.0.0.1";
        //    Socket ClientSocket = new Socket(AddressFamily
        //        .InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IpAddress), port);
        //    ClientSocket.Connect(ep);
        //    Console.WriteLine("You are Connecting!");
        //    while(true)
        //    {
        //        string messageFromClient = null;
        //        Console.WriteLine("Enter the Message");
        //        messageFromClient = Console.ReadLine();
        //        ClientSocket.Send(System.Text.Encoding.ASCII
        //            .GetBytes(messageFromClient), 0, messageFromClient.Length, SocketFlags.None);
        //        byte[] MsgFromServer = new byte[1024];
        //        int size = ClientSocket.Receive(MsgFromServer);
        //        Console.WriteLine("Sever " + System.Text.Encoding.ASCII.GetString(MsgFromServer, 0, size));
        //    }
        //}

         
        static void Main(string[] args)
        {
            List<byte> bytelist = new List<byte>();
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 5000;
            TcpClient client = new TcpClient();
            client.Connect(ip, port);
            Console.WriteLine("client connected!!");
            NetworkStream ns = client.GetStream();
            Thread thread = new Thread(o => ReceiveData((TcpClient)o));

            thread.Start(client);

            string s;
            while (!string.IsNullOrEmpty((s = Console.ReadLine())))
            {
                //byte[] buffer = Encoding.ASCII.GetBytes(s);
                //ns.Write(buffer, 0, buffer.Length); 

                int id = int.Parse(s);



                List<byte> sendByte = new List<byte>();
                sendByte.Add(0xff);
                sendByte.Add(0x04);
                sendByte.Add((byte)id);


                string status = "Datetime:2019-06-06 23:59:59|SensorId:1|Status:1";

                byte[] data = Encoding.ASCII.GetBytes(status);

                for(int i=0;i<data.Length;i++)
                {
                    sendByte.Add(data[i]);
                }

                ns.Write(sendByte.ToArray(), 0, sendByte.Count);

            }
            
            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            Console.WriteLine("disconnect from server!!");
            Console.ReadKey();
        }

        static void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            List<byte> bytelist = new List<byte>();
            byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length);
            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                //string data = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
                //Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            }
            string data = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
            byte[] _buffer = Encoding.ASCII.GetBytes(data);
            string d_id = BitConverter.ToString(_buffer, 0);
            Console.WriteLine("printing!!!!" + d_id);
            //string room = BitConverter.ToString(_buffer, 1, 7);
            //string status = BitConverter.ToString(_buffer, 8);

            //bytelist.AddRange(Encoding.ASCII.GetBytes(d_id));
            //bytelist.AddRange(Encoding.ASCII.GetBytes(room));
            //bytelist.AddRange(Encoding.ASCII.GetBytes(status));
            //bytelist.ToArray();

            //Console.WriteLine("printing!!!!" + bytelist);
        }
    }
}

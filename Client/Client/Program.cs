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
            if (!string.IsNullOrEmpty((s = Console.ReadLine())))
            {
                //byte[] buffer = Encoding.ASCII.GetBytes(s);
                //ns.Write(buffer, 0, buffer.Length); 
                //Console.WriteLine("Enter if");
                int id = int.Parse(s);
                //Console.WriteLine("Enter " + id);
                //byte[] receivedBytes = new byte[1024];
                //int byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length);
                
                List<byte> sendByte = new List<byte>();

                sendByte.Add(0x05);
                sendByte.Add(0x05);
                sendByte.Add((byte)id);
                sendByte.Add(0x01);
                sendByte.Add(0x01);
                sendByte.Add(0x01);  // id = 1
                sendByte.Add(0x02);
                sendByte.Add(0x02);
                sendByte.Add(0x01);  // status = 1
                sendByte.Add(0x00);
                sendByte.Add(0x01);


                //string status = "Datetime:2019-06-06 23:59:59|SensorId:1|Status:1";

                //byte[] data = Encoding.ASCII.GetBytes(status);

                //for(int i=0;i<data.Length;i++)
                //{
                //    sendByte.Add(data[i]);
                //}

                ns.Write(sendByte.ToArray(), 0, sendByte.Count);
                Console.WriteLine(BitConverter.ToString(sendByte.ToArray(), 0, sendByte.Count));

            }

            //client.Client.Shutdown(SocketShutdown.Send);
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
            int byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length);


            //verify
            if (byte_count >= 11)
            {
                if (receivedBytes[0] == 0x05 && receivedBytes[1] == 0x05)  //check header
                {
                    if (receivedBytes[9] == 0x00 && receivedBytes[10] == 0x01)
                    {
                        string server = BitConverter.ToString(receivedBytes, 2, 1);
                        Console.WriteLine(server);
                        if (receivedBytes[3] == 0x01 && receivedBytes[4] == 0x01)
                        {
                            string id = BitConverter.ToString(receivedBytes, 5, 1);
                            Console.WriteLine("id = " + id);
                        }
                        if (receivedBytes[6] == 0x02 && receivedBytes[7] == 0x02)
                        {
                            string status = BitConverter.ToString(receivedBytes, 8, 1);
                            Console.WriteLine("status = " + status);
                        }
                        else
                        {
                            Console.WriteLine("Wrong data");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Wrong end");
                    }
                }
                else
                {
                    Console.WriteLine("wrong header");
                }
            }
            else
            {
                Console.WriteLine("unknow package");
            }
            // Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));

        }
    }
}

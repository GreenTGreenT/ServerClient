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
        public static List<Session> all_session = new List<Session>();
        //static readonly Dictionary<string, TcpClient> list_clients = new Dictionary<string, TcpClient>();

        static void Main(string[] args)
        {
            try
            {
                TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
                ServerSocket.Start();

                Session new_session = new Session();

                //byte[] receivedBytes = new byte[1024];

                //Thread t = new Thread(new_session.receive);
                while (true)
                {
                    //Console.WriteLine("Enter while");
                    TcpClient client = ServerSocket.AcceptTcpClient();
                    //Console.WriteLine("After cl");
                    NetworkStream ns = client.GetStream();
                    //Console.WriteLine("After ns");
                    byte[] receivedBytes = new byte[1024];
                    int byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length);
                    //Console.WriteLine("This is byte_count " + byte_count);

                    if (client.ReceiveBufferSize > 0)
                    {
                        if (all_session.Count == 0)
                        {
                            Console.WriteLine("Enter 0");
                            //verify
                            if (byte_count >= 11)
                            {
                                if (receivedBytes[0] == 0x05 && receivedBytes[1] == 0x05)  //check header
                                {
                                    if (receivedBytes[9] == 0x00 && receivedBytes[10] == 0x01)
                                    {
                                        new_session._socket = client;
                                        new_session.id = BitConverter.ToString(receivedBytes, 2, 1);
                                        all_session.Add(new_session);
                                        Console.WriteLine(all_session[0].id);
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
                        }
                        //if (all_session.Count == 1)
                        //{
                        //    Console.WriteLine("Enter 1");
                        //    Thread t = new Thread(() => SendData(0));
                        //    t.Start();
                        //}
                        if (all_session.Count > 0)
                        {
                            Console.WriteLine("Enter >1");
                            string id_client = BitConverter.ToString(receivedBytes, 2, 1);
                            int j = all_session.Count;
                            for (int i = 0; i <= j; i++)
                            {
                                if (all_session[i].id == id_client)
                                {
                                    Console.WriteLine("Enter find id");
                                    Thread t = new Thread(() => SendData(i));
                                    t.Start();
                                }
                                else
                                {
                                    new_session._socket = client;
                                    new_session.id = BitConverter.ToString(receivedBytes, 2, 1);
                                    all_session.Add(new_session);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void SendData(int k)
        {
            Console.WriteLine("Enter SendData");
            //TcpClient client = new TcpClient();
            NetworkStream _stream = all_session[k]._socket.GetStream();
            List<byte> sendByte = new List<byte>();

            //byte[] data1 = Encoding.ASCII.GetBytes("5");
            sendByte.Add(0x05);
            sendByte.Add(0x05);
            sendByte.Add(0xff);
            sendByte.Add(0x01);
            sendByte.Add(0x01);
            sendByte.Add(0x01);  // id = 1
            sendByte.Add(0x02);
            sendByte.Add(0x02);
            sendByte.Add(0x01);  // status = 1
            sendByte.Add(0x00);
            sendByte.Add(0x01);
            _stream.Write(sendByte.ToArray(), 0, sendByte.Count);
        }
        public class Session
        {
            public string id { get; set; }
            public TcpClient _socket { get; set; }

            //_socket.Client.Shutdown(SocketShutdown.Both);
            //_socket.Close();        
        }

    }
}

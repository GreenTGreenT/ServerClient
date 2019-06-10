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
        public static List<byte> sendByte = new List<byte>();
        public static int j = 0;
        static void Main(string[] args)
        {
            try
            {
                TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
                ServerSocket.Start();

                Session new_session = new Session();
                
                while (true)
                {
               
                    TcpClient client = ServerSocket.AcceptTcpClient();
                    
                    NetworkStream ns = client.GetStream();
                    
                    byte[] receivedBytes = new byte[1024];
                    int byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length);
                    //Console.WriteLine("This is byte_count " + byte_count);
                 
                    if (client.ReceiveBufferSize > 0)
                    {
                        //verify
                        if (byte_count >= 11)
                        {
                            if (receivedBytes[0] == 0x05 && receivedBytes[1] == 0x05)  //check header
                            {
                                if (receivedBytes[9] == 0x00 && receivedBytes[10] == 0x01)
                                {
                                    string id_client = BitConverter.ToString(receivedBytes, 2, 1);

                                    Monitor.Enter(all_session);  // wait

                                    try
                                    {

                                        //int j = all_session.Count;
                                        if (j == 0)
                                        {
                                            new_session._socket = client;
                                            new_session.id = BitConverter.ToString(receivedBytes, 2, 1);
                                            all_session.Add(new_session);
                                            j += 1;
                                        }                                        
                                        //j = all_session.Count;
                                        for (int i = 0; i < j; i++)
                                        {
                                            if (all_session[i].id == id_client)
                                            {
                                                ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(SendData), i);
                                                //t.Start();
                                            }
                                            else
                                            {
                                                continue;
                                            }

                                            if (i > 1 && i == j - 1)
                                            {
                                                if (all_session[i].id != id_client)
                                                {
                                                    new_session._socket = client;
                                                    new_session.id = BitConverter.ToString(receivedBytes, 2, 1);
                                                    all_session.Add(new_session);
                                                    j += 1;
                                                    //Thread t = new Thread(() => SendData(i));
                                                    //t.Start();
                                                    ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(SendData), i);
                                                }
                                                else
                                                {
                                                    ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(SendData), i);
                                                    //Thread t = new Thread(() => SendData(i));
                                                    //t.Start();
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    finally
                                    {

                                    }

                                    Monitor.Exit(all_session);  //until exit

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
                        //}

                        //if (all_session.Count == 1)
                        //{
                        //    Thread t = new Thread(() => SendData(0));
                        //    t.Start();
                        //}
                        //Console.WriteLine("TTTTTTTT" + all_session.Count);
                        //if (all_session.Count > 1)
                        //{
                        //    Console.WriteLine("Enter >0");
                        //    string id_client = BitConverter.ToString(receivedBytes, 2, 1);

                        //    Console.WriteLine("To see ID " + id_client);
                        //    int j = all_session.Count;
                        //    Console.WriteLine("To see j  " + j);
                        //    for (int i = 0; i < j; i++)
                        //    {
                        //        //Console.WriteLine("To see i  " + i);
                        //        Console.WriteLine("To see what is out when put i  " + all_session[i].id);
                        //        if (all_session[i].id == id_client)
                        //        {
                        //            Console.WriteLine("To see i  " + i);
                        //            Console.WriteLine("Enter find id");
                        //            Console.WriteLine("Enter exist ID");
                        //            //Thread t = new Thread(() => SendData(i));
                        //            Thread t = new Thread(() => SendData(i));
                        //            t.Start();
                        //        }
                        //        else
                        //        {
                        //            Console.WriteLine("Enter else ");
                        //            new_session._socket = client;
                        //            new_session.id = BitConverter.ToString(receivedBytes, 2, 1);
                        //            all_session.Add(new_session);

                        //            Thread t = new Thread(() => SendData(i));
                        //            t.Start();
                        //        }
                        //    }
                        //    //Console.WriteLine("ALL " + all_session);
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //Console.Read();
        }

        public static void SendData(object b)
        {
            int k = (int)b;
            //k = k - 1;
            Console.WriteLine("Enter SendData");
            Console.WriteLine("To see K " + k);
            //TcpClient client = new TcpClient();
            NetworkStream _stream = all_session[k]._socket.GetStream();
            

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

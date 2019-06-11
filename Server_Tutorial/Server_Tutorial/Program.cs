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

                Array testdata = new Array[20];
                
                while (true)
                {
                    Console.WriteLine("Number of session " + j);
                    TcpClient client = ServerSocket.AcceptTcpClient();
                    
                    NetworkStream ns = client.GetStream();
                    
                    byte[] receivedBytes = new byte[1024];

                    while (ns.Read(receivedBytes, 0, receivedBytes.Length) > 0)
                    { 

                        //Console.WriteLine("This is byte_count " + byte_count);

                        int byte_count = receivedBytes[2]; 

                        //verify
                        if (byte_count >= 11)
                        {
                            if (receivedBytes[0] == 0x05 && receivedBytes[1] == 0x05)  //check header
                            {
                                if (receivedBytes[byte_count-2] == 0x00 && receivedBytes[byte_count-1] == 0x01) // check end
                                {
                                    string id_client = BitConverter.ToString(receivedBytes, 3, 1);

                                    Monitor.Enter(all_session);  // wait

                                    try
                                    {
                                        string Id = BitConverter.ToString(receivedBytes, 3, 1);

                                        bool found = false;
                                        foreach (Session _session in all_session)
                                        {
                                            if (_session.id == Id)
                                            {
                                                found = true;

                                                //_session._socket = client;
                                                Thread t = new Thread(() => SendData(Id));
                                                t.Start();

                                                break;
                                            }
                                        }

                                        if (!found)
                                        {
                                            Session new_session = new Session();
                                            new_session._socket = client;
                                            new_session.id = BitConverter.ToString(receivedBytes, 3, 1);
                                            all_session.Add(new_session);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                    finally
                                    {

                                    }

                                    Monitor.Exit(all_session);  //until exit

                                        
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
            string id = (string)b;
            Session cmdSession = new Session();
            bool found = false;
            foreach(Session _session in all_session)
            {
                if(_session.id == id)
                {
                    found = true;
                    cmdSession = _session;
                    break;
                }
            }

            if (found)
            {               
                Console.WriteLine("Enter SendData");
                Console.WriteLine("To see " + cmdSession.id);
                NetworkStream _stream = cmdSession._socket.GetStream();
                //var ID = Encoding.UTF8.GetBytes(id);

                sendByte.Add(0x05);
                sendByte.Add(0x05);
                sendByte.Add(0x0A);
                //sendByte.Add(ID);
                sendByte.Add(0xff);
                sendByte.Add(0xff);
                sendByte.Add(0xff); 
                sendByte.Add(0xff);
                sendByte.Add(0xff);
                sendByte.Add(0xff);  // status = 1
                sendByte.Add(0x00);
                sendByte.Add(0x01);
                _stream.Write(sendByte.ToArray(), 0, sendByte.Count);

            }
            
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

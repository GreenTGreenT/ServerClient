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
        public static int j = 0;
        public static int loop = 0;
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

                    loop = 0;
                    //bool loop_check = true;                    
                    while (true)
                    {
                        if (loop > 1)
                        {
                            //loop_check = false;
                            break;
                        }
                        if (ns.Read(receivedBytes, 0, receivedBytes.Length) > 0)
                        {
                            loop++;
                            //Console.WriteLine("This is byte_count " + byte_count);
                            int byte_count = receivedBytes[2];

                            //verify
                            if (byte_count >= 11)
                            {
                                if (receivedBytes[0] == 0x05 && receivedBytes[1] == 0x05)  //check header
                                {
                                    if (receivedBytes[byte_count - 2] == 0x00 && receivedBytes[byte_count - 1] == 0x01) // check end
                                    {
                                        //string id_client = BitConverter.ToString(receivedBytes, 3, 1);

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
                                                    _session._socket = client; //refresh session

                                                    //_session._socket = client;
                                                    ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(SendData), Id);
                                                    loop++;
                                                    break;
                                                }
                                            }

                                            if (!found)
                                            {
                                                Session new_session = new Session();
                                                new_session._socket = client;
                                                new_session.id = BitConverter.ToString(receivedBytes, 3, 1);
                                                all_session.Add(new_session);
                                                j++;
                                                
                                                //ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(SendData), Id);
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
                        }
                        

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
            List<byte> sendByte = new List<byte>();
            string id = (string)b;
            Session cmdSession = new Session();
            try
            {
                bool found = false;

                foreach (Session _session in all_session)
                {
                    if (_session.id == id)
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
                    sendByte.Add(0x0B);
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
                    Console.WriteLine(BitConverter.ToString(sendByte.ToArray(), 0, sendByte.Count));

                }                
            }
            catch(Exception e)
            {
                Console.WriteLine("This ID use to connect");          
                Console.WriteLine(e);
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

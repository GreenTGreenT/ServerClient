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
    //This is synchronous(I think)
    class Program
    {
        public static List<Session> all_session = new List<Session>();        
            
        public static int j = 0; //Create j to check that how many session I have now
        public static int loop = 0; //Because I use while(true) that will run forever, But I want to fix that while(true) should run two time
        static void Main(string[] args)
        {
            try
            {
                TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000); 
                ServerSocket.Start(); // Create server at port 5000

                while (true) //Because I want it to check that are there the connection from client
                {
                    Console.WriteLine("Number of session " + j); // print out the number of session I have
                    TcpClient client = ServerSocket.AcceptTcpClient(); // If there have client that try to connect my server, I will accept.
                    
                    NetworkStream ns = client.GetStream(); // Get the data that client pass to server
                    
                    byte[] receivedBytes = new byte[1024]; // Create this to store data that client pass to server

                    loop = 0; // If it exit the loop below. "loop" should revalue
                                      

                    while (true)
                    {
                        if (loop > 1)
                        {
                            
                            break;
                        }

                        // Read the data that sent from client and store the data in receivedBytes
                        if (ns.Read(receivedBytes, 0, receivedBytes.Length) > 0)
                        {
                            loop++;
                            
                            int byte_count = receivedBytes[2];

                            //verify
                            if (byte_count >= 11)
                            {
                                if (receivedBytes[0] == 0x05 && receivedBytes[1] == 0x05)  //check header
                                {
                                    if (receivedBytes[byte_count - 2] == 0x00 && receivedBytes[byte_count - 1] == 0x01) // check end
                                    {
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
                                                   
                                                    ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(SendData), Id);
                                                    loop++;
                                                    break;
                                                }
                                            }

                                            //The first time of client that never connect will come in this condition
                                            //When we save the ID that use to connect, It still in this loop but It will go to the foreach and condition above
                                            //After that, It will go to function SendData that will send the data back to client
                                            //If the client close connection and try to connect again it will not come in this condition(It won't create new session)
                                            //but it will go to foreach and go to function SendData
                                            if (!found)
                                            {
                                                Session new_session = new Session();
                                                new_session._socket = client;
                                                new_session.id = BitConverter.ToString(receivedBytes, 3, 1);
                                                all_session.Add(new_session);
                                                j++;                                               
                                               
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
                    //Console.WriteLine("Enter SendData");
                    //Console.WriteLine("ID is " + cmdSession.id);
                    NetworkStream _stream = cmdSession._socket.GetStream();                   

                    sendByte.Add(0x05);
                    sendByte.Add(0x05);
                    sendByte.Add(0x0B);                  
                    sendByte.Add(0xff);
                    sendByte.Add(0xff);
                    sendByte.Add(0xff);
                    sendByte.Add(0xff);
                    sendByte.Add(0xff);
                    sendByte.Add(0xff);  
                    sendByte.Add(0x00);
                    sendByte.Add(0x01);
                    _stream.Write(sendByte.ToArray(), 0, sendByte.Count);
                    //Console.WriteLine(BitConverter.ToString(sendByte.ToArray(), 0, sendByte.Count));

                }                
            }
            catch(Exception e)
            {                       
                Console.WriteLine(e);
            }            
        }

        public class Session
        {
            public string id { get; set; }
            public TcpClient _socket { get; set; }
        }
        
    }
}

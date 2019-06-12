﻿using System;
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

            byte[] receivedBytes = new byte[1024];

            
            Console.WriteLine("enter id : ");

            int id = 0;
            string s = "";
            //register by id
            if (!string.IsNullOrEmpty((s = Console.ReadLine())))
            {

                id = int.Parse(s);

                List<byte> sendByte = new List<byte>();

                sendByte.Add(0x05);
                sendByte.Add(0x05);
                sendByte.Add(0x0B); // All byte to send
                sendByte.Add((byte)id); // ID
                sendByte.Add(0x00);
                sendByte.Add(0x00);
                sendByte.Add(0x00);
                sendByte.Add(0x00);
                sendByte.Add(0x00);
                sendByte.Add(0x00);
                sendByte.Add(0x01);

                ns.Write(sendByte.ToArray(), 0, sendByte.Count);
                Console.WriteLine(BitConverter.ToString(sendByte.ToArray(), 0, sendByte.Count));

            }

            bool running = true;

            while(running)
            {               
                if (!string.IsNullOrEmpty((s = Console.ReadLine())))
                {
                    if (s == "exit")
                    {
                        running = false;
                    }
                    else
                    {
                        if(s.Length <4)
                        {
                            while(s.Length <4)
                            {
                                s += "0";
                            }
                        }

                        char[] arr = s.ToArray();

                        List<byte> sendByte = new List<byte>();

                        sendByte.Add(0x05);
                        sendByte.Add(0x05);

                        int len = arr.Length + 7;
                        sendByte.Add((byte)len);
                        sendByte.Add((byte)id);
                        sendByte.Add(0x01);  // cmd  1 = normal message    

                        foreach(char c in arr)
                        {
                            sendByte.Add((byte)c);
                        }

                        sendByte.Add(0x00);
                        sendByte.Add(0x01);

                        ns.Write(sendByte.ToArray(), 0, sendByte.Count);
                        Console.WriteLine(BitConverter.ToString(sendByte.ToArray(), 0, sendByte.Count));
                        //Thread thread = new Thread(o => ReceiveData((TcpClient)o));

                        //thread.Start(client);
                    }
                }
                //if (ns.Read(receivedBytes, 0, receivedBytes.Length) > 0)
                //{                    
                //    Thread thread = new Thread(() => ReceiveData(client));
                //    thread.Start();
                //    thread.Join();
                //}
            }
            //thread.Join();
            //}
            //ns.Close();
            //client.Close();
            //Console.WriteLine("disconnect from server!!");
            //Console.ReadKey();
        }

        static void ReceiveData(TcpClient client)
        {
            Console.WriteLine("Enter ReceiveData");

            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int read = ns.Read(receivedBytes, 0, receivedBytes.Length);
            int byte_count = receivedBytes[2];
            Console.WriteLine("All byte "+byte_count);

            //verify
            if (read > 0)
            {
                if (byte_count >= 11)
                {
                    if (receivedBytes[0] == 0x05 && receivedBytes[1] == 0x05)  //check header
                    {
                        if (receivedBytes[byte_count - 2] == 0x00 && receivedBytes[byte_count - 1] == 0x01)
                        {
                            string server = BitConverter.ToString(receivedBytes, 3, 1);
                            Console.WriteLine(server);
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
            // Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));

        }
    }
}

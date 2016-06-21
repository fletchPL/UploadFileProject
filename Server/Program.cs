using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class HandleClient
    {
        Socket socketClient;
        string clNo;
        private int size = 1024;
        public void startClient(Socket inClientSocket, string clineNo)
        {
            this.socketClient = inClientSocket;
            this.clNo = clineNo;
            Thread clientThread = new Thread(doChat);
            clientThread.Start();
        }

        private void doChat()
        {

            BinaryWriter bWrite = null;

            try
            {

                while (true)
                {
                    byte[] clientData = new byte[size];
                    string receivedPath = @"F:\Studia\";

                    int receivedBytesLen = socketClient.Receive(clientData);
                    // var watch = System.Diagnostics.Stopwatch.StartNew();

                    if (receivedBytesLen != 0)
                    {
                        int fileNameLen = BitConverter.ToInt32(clientData, 0);
                        string fileName = Encoding.ASCII.GetString(clientData, 4, fileNameLen);

                        int count = 1;

                        string fileNameOnly = Path.GetFileNameWithoutExtension(fileName);
                        string extension = Path.GetExtension(fileName);

                        while (File.Exists(receivedPath + fileName))
                        {
                            string tempFileName = string.Format("{0}({1}){2}", fileNameOnly, count++, extension);
                            fileName = tempFileName;
                        }

                        Console.WriteLine("Client:{0} connected & File {1} started received.", socketClient.RemoteEndPoint, fileName);

                        bWrite = new BinaryWriter(File.Open(receivedPath + fileName, FileMode.Append));
                        bWrite.Write(clientData, 4 + fileNameLen, receivedBytesLen - 4 - fileNameLen);

                        if (receivedBytesLen == size)
                        {
                            while (receivedBytesLen == size)
                            {
                                if ((receivedBytesLen = socketClient.Receive(clientData)) > 0)
                                {
                                    bWrite.Write(clientData, 0, receivedBytesLen);
                                }


                                if (receivedBytesLen < size)
                                {
                                    Console.WriteLine("File: {0} received & saved at path: {1}", fileName, receivedPath);

                                    bWrite.Close();
                                    // watch.Stop();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("File: {0} received & saved at path: {1}", fileName, receivedPath);

                            bWrite.Close();
                            //watch.Stop();
                        }

                    }
                    //  Console.WriteLine("Sent file in: "+watch.ElapsedMilliseconds+"ms");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Client-" + clNo + e.Message);
                if (bWrite != null)
                {
                    bWrite.Close();
                }

            }

        }
    }


    class Program
    {
        private static Socket socketServer;

        static void Main(string[] args)
        {
            try
            {
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Any, 5656);
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                socketServer.Bind(ipEnd);
                socketServer.Listen(100);

                Socket socketClient;
                int counter = 0;

                while (true)
                {
                    counter += 1;
                    socketClient = socketServer.Accept();

                    Console.WriteLine("Client No:" + Convert.ToString(counter) + " started!");
                    HandleClient client = new HandleClient();
                    client.startClient(socketClient, Convert.ToString(counter));
                }

                socketClient.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Server error:" + e.Message);
            }
        }
    }
}

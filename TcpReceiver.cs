using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace TcpTool
{
    public class TcpReceiver
    {
        protected TcpListener listener_ = null;

        public void startListening(int port, Func<string, bool> messageRecvdFunction)
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse("0.0.0.0");

                // TcpListener server = new TcpListener(port);
                listener_ = new TcpListener(localAddr, port);

                // Start listening for client requests.
                listener_.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                bool done = false;
                    while (!done)
                    {
                    try
                    {
                        Console.Write("Waiting for a connection... ");

                        // Perform a blocking call to accept requests.
                        // You could also use server.AcceptSocket() here.
                        TcpClient client = listener_.AcceptTcpClient();
                        Console.WriteLine("Connected!");

                        data = null;

                        // Get a stream object for reading and writing
                        NetworkStream stream = client.GetStream();

                        int i;

                        // Loop to receive all the data sent by the client.
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            // Console.WriteLine("Received: {0}", data);
                            if (messageRecvdFunction(data))
                            {
                                done = true;
                                break;
                            }

                            //// Process the data sent by the client.
                            //data = data.ToUpper();

                            //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                            // Send back a response.
                            //stream.Write(msg, 0, msg.Length);
                            //Console.WriteLine("Sent: {0}", data);
                        }

                        // Shutdown and end connection
                        client.Close();
                    }
                    catch (System.IO.IOException)
                    {
                        Console.WriteLine("Socket exception, closing.");
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                listener_.Stop();
            }
        }
    }
}

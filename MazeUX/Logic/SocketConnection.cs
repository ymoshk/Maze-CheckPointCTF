using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MazeSolver
{
    public class SocketConnetion
    {
        private readonly string r_IpAddress;
        private readonly int r_Port;
        private byte[] m_Buffer;
        private Socket m_Socket;

        public SocketConnetion(string i_Ip, int i_Port)
        {
            this.r_IpAddress = i_Ip;
            this.r_Port = i_Port;
            this.m_Buffer = new byte[4096];
            this.m_Socket = null;
        }

        public bool Connect()
        {
            bool result = false;

            // Connect to a remote device.  
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(this.r_IpAddress);

                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, this.r_Port);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    this.m_Socket = sender;
                    result = true;
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                result = false;
            }

            return result;
        }

        public void Close()
        {
            // Release the socket.  
            this.m_Socket.Shutdown(SocketShutdown.Both);
            this.m_Socket.Close();
        }

        public bool Send(char i_Char)
        {
            string message = i_Char.ToString();
            return Send(message);
        }

        public bool Send(string i_Message)
        {
            bool result = false;
            Console.WriteLine(i_Message);

            try
            {
                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes(i_Message);

                // Send the data through the socket.  
                int bytesSent = this.m_Socket.Send(msg);
                result = bytesSent > 0;
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return result;
        }
        

        public string Receive()
        {
            string result = "";

            try
            {
                // Receive the response from the remote device.  
                int bytesRec = this.m_Socket.Receive(this.m_Buffer);
                result += Encoding.ASCII.GetString(this.m_Buffer, 0, bytesRec);
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            Console.WriteLine(result);
            return result.Trim();
        }

        public string ReceiveUntil(string i_StringToLookFor)
        {
            string result = "";

            try
            {
                while (!result.Contains(i_StringToLookFor))
                {
                    // Receive the response from the remote device.  
                    int bytesRec = this.m_Socket.Receive(this.m_Buffer);
                    result += Encoding.ASCII.GetString(this.m_Buffer, 0, bytesRec);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            Console.WriteLine(result);
            return result.Trim();
        }
    }
}

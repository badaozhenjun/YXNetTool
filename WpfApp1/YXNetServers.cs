using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YXNetServers
{

    public class YXSoketServer
    {
        public string ip;
        public string port;
        public delegate void ReceiveMessage(string ip, string port, string message, YXSoketServer server);
        public ReceiveMessage didReceiveMessage;
    }
    public class YXTcpServer : YXSoketServer
    {
        public Dictionary<string, Socket> sockets = new Dictionary<string, Socket>();
        
        public delegate void ReceiveSocket(string ip, string port, YXTcpServer socket);
        
        public ReceiveSocket didConnected;
        public Action<string> didConnectInterrupted;
        public Socket udpSocket;


        private YXTcpServer()
        {
        }

        public YXTcpServer(string ip, string port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void startServer()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), Convert.ToInt32(port)));
            socket.Listen(10);
            Thread thread = new Thread(connect);
            thread.IsBackground = true;
            thread.Start(socket);
        }

        void send(string ip, string port, string message)
        {

        }
        void connect(Object o)
        {
            Socket watchSocket = o as Socket;
            while (true)
            {
                Socket acceptSocket = watchSocket.Accept();

                IPAddress clientIP = (acceptSocket.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (acceptSocket.RemoteEndPoint as IPEndPoint).Port;
                this.sockets.Add(clientIP + ":" + clientPort, acceptSocket);
                showMessage("连接成功:" + acceptSocket.LocalEndPoint.ToString());
                if (this.didConnected != null)
                {
                    this.didConnected(clientIP + "", clientPort + "", this);
                }
                Thread thread = new Thread(receive);
                thread.IsBackground = true;
                thread.Start(acceptSocket);
            }
        }

        void receive(Object o)
        {
            Socket acceptSocket = o as Socket;
            while (true)
            {
                byte[] buffer = new byte[1024 * 1024 * 2];
                int count = 0;
                try
                {
                    count = acceptSocket.Receive(buffer);
                    if (count <= 0) break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    this.didConnectInterrupted?.Invoke(e.Message);
                    break;
                }
                
                IPAddress clientIP = (acceptSocket.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (acceptSocket.RemoteEndPoint as IPEndPoint).Port;
                string message = Encoding.UTF8.GetString(buffer, 0, count);
                if (this.didReceiveMessage != null)
                {
                    this.didReceiveMessage(clientIP + "", clientPort + "", message, this);
                }


            }
        }


        void showMessage(string message)
        {
            Console.WriteLine(message);
        }



    }
    public class YXUdpServer : YXSoketServer
    {
        private Socket socket;
        public Action<string> didSendFail;
        public Action<string> didReceiveFail;
        private YXUdpServer()
        {

        }
        public YXUdpServer(string ip, string port)
        {
            this.ip = ip;
            this.port = port;
        }
        public void startServer()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), Convert.ToInt32(port)));
            this.socket = socket;
            Thread thread = new Thread(receive);
            thread.IsBackground = true;
            thread.Start(socket);
        }

        public void closeServer()
        {
            if (this.socket != null)
            {
                this.socket.Close();
                this.socket = null;
            }
        }
        public void send(string message, string ip, string port)
        {
            if (message != null)
            {
                EndPoint remote = new IPEndPoint(IPAddress.Parse(ip), Convert.ToInt32(port));
                byte[] bytes = Encoding.UTF8.GetBytes(message.Trim());
                try
                {
                    this.socket.SendTo(bytes, SocketFlags.None, remote);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    this.didReceiveFail?.Invoke((e.Message));
                    
                }
               
            }
        }
        void receive(Object o)
        {
            Socket acceptSocket = o as Socket;
            while (true)
            {
                byte[] buffer = new byte[1024 * 1024 * 2];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)(sender);
                int count = 0;
                try
                {
                    count = acceptSocket.ReceiveFrom(buffer, ref remote);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    this.didReceiveFail(e.Message);
                    break;
                }
                
                IPAddress clientIP = (remote as IPEndPoint).Address;
                int clientPort = (remote as IPEndPoint).Port;
                string message = Encoding.UTF8.GetString(buffer, 0, count);
                if (this.didReceiveMessage != null)
                {
                    this.didReceiveMessage(clientIP + "", clientPort + "", message, this);
                }


            }
        }
    }


}

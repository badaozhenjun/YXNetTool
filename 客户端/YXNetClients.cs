using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YXNetClients
{
    public class YXSocketClient
    {
        public string ip;
        public string port;
        public delegate void ReceiveMessage(string ip, string port, string message, YXSocketClient client);
        public ReceiveMessage didReceiveMessage;
        protected Socket socket;

      
    }
    public class YXTcpClient : YXSocketClient
    {
        public Action didConnectSuccess;
        public Action didConnectFail;
        public Action<string> didConnectInterrupt;
        private YXTcpClient()
        {

        }
        public YXTcpClient(string ip, string port)
        {
            this.ip = ip;
            this.port = port;
        }
        public void connect()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(new IPEndPoint(IPAddress.Parse(ip), Convert.ToInt32(port)));
                if (socket.Connected)
                {
                    this.didConnectSuccess?.Invoke();
                }
                else
                {
                    this.didConnectFail?.Invoke();
                }
            }
            catch
            {
                this.didConnectFail?.Invoke();
            }

            this.socket = socket;
            Thread thread = new Thread(receive);
            thread.IsBackground = true;
            thread.Start(socket);
        }

        public void send(string message)
        {
            if (this.socket != null && message != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message.Trim());
                this.socket.Send(bytes);
            }
        }
        void receive(Object o)
        {
            Socket clientSocket = o as Socket;
            while (true)
            {
                byte[] buffer = new byte[1024 * 1024 * 2];
                int count=0;
                try
                {
                     count = clientSocket.Receive(buffer);
                    if (count <= 0) break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    this.didConnectInterrupt?.Invoke(e.Message);
                    break;
                }
                IPAddress clientIP = (clientSocket.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (clientSocket.RemoteEndPoint as IPEndPoint).Port;
                string message = Encoding.UTF8.GetString(buffer, 0, count);
                this.didReceiveMessage?.Invoke(clientIP + "", clientPort + "", message, this);

            }
        }



    }
    public class YXUdpClient : YXSocketClient
    {
        private EndPoint remote;
        private Thread recieveThread;
        public Action<string> didReceiveFail;
        private YXUdpClient()
        {

        }
        public YXUdpClient(string ip, string port)
        {
            this.ip = ip;
            this.port = port;
        }
        private Socket getSocket()
        {
            if (base.socket == null)
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                remote = new IPEndPoint(IPAddress.Parse(ip), Convert.ToInt32(port));
                base.socket = socket;
               
            }
            return base.socket;
        }
        public void startReceive()
        {
            if (this.recieveThread == null)
            {
                Thread thread = new Thread(receive);
                thread.IsBackground = true;
                thread.Start(this.getSocket());
                this.recieveThread = thread;
            }
            
        }

        public void send(string message)
        {
            if ( message != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message.Trim());
                this.getSocket().SendTo(bytes,SocketFlags.None, remote);
            }
            this.startReceive();
        }
        void receive(Object o)
        {
            Socket clientSocket = o as Socket;
            while (true)
            {
                byte[] buffer = new byte[1024 * 1024 * 2];
                int count = 0;
                try
                {
                     count = clientSocket.ReceiveFrom(buffer, ref remote);
                    if (count <= 0) break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    this.didReceiveFail?.Invoke(e.Message);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YXNetServers;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private YXTcpServer server;
        private YXUdpServer udpServer;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void startServerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.server == null)
            {
                YXTcpServer server = new YXTcpServer(this.ipText.Text, this.portText.Text);
                server.didConnected = delegate(string ip,string port, YXTcpServer socketServer)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.serverText.AppendText(ip+":"+port+" 连接成功\n");
                    }));
                };
                server.didReceiveMessage = delegate(string ip, string port, string message, YXSoketServer socketServer)
                {
                    this.Dispatcher.Invoke(new Action(() => 
                    {
                        this.serverText.AppendText(message+"\n");
                    }));
                };
                server.startServer();
                this.serverText.AppendText("服务器已开启\n");
                this.startServerBtn.Content = "关闭服务器";
                this.server = server;
            }
            else
            {
                this.server = null;
                this.startServerBtn.Content = "开启服务器";
            }
        }

        private void startServerBtn_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void serverSendBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (this.udpServer == null)
            {
                YXUdpServer server = new YXUdpServer(ipText.Text, portText.Text);
                server.didReceiveMessage = delegate (string ip, string port, string message, YXSoketServer udpServer)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        udpText.AppendText(ip + ":" + port + ":" + message + "\n");
                        server.send("收到消息", ip, port);
                    }));
                };
                 server.didReceiveFail = new Action<string>((string message) =>
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        udpText.AppendText( message + "\n");

                    }));
                });
                server.startServer();
                this.udpServer = server;
            }
            else
            {
                this.udpServer.closeServer();
                this.udpServer = null;
            }

            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
using YXNetClients;

namespace 客户端
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private YXTcpClient client;
        private YXUdpClient udpClient;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.client == null)
            {
                YXTcpClient client = new YXTcpClient(this.ipText.Text,this.portText.Text);
                client.didConnectSuccess = new Action(() => {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.clientText.AppendText("连接成功\n");
                    }));
                });
                client.didConnectFail = new Action(() => {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.clientText.AppendText("连接失败\n");
                    }));
                });
                client.didReceiveMessage = delegate(string ip, string port, string message, YXSocketClient socketClient)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.clientText.AppendText(message+"\n");
                    }));
                };
                client.connect();
                this.client = client;
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            this.client.send(this.sendText.Text);
        }

        private void udpSendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.udpClient == null)
            {
                YXUdpClient server = new YXUdpClient(ipText.Text, portText.Text);
                server.didReceiveMessage = delegate (string ip, string port, string message, YXSocketClient client)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        clientText.AppendText(ip + ":" + port + ":" + message + "\n");
                        
                    }));
                };
                server.didReceiveFail = new Action<string>((string message) =>
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        clientText.AppendText( message + "\n");

                    }));
                });
                this.udpClient = server;
            }
            this.udpClient.send(this.sendText.Text);
        }

         
    }
}

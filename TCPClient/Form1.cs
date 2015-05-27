using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TCPLibrary;
using TCPLibrary.Abstracts;
using TCPLibrary.DefaultImplements;

namespace TCPClient
{
    public partial class Form1 : Form
    {
        BaseClientSocket _client;
        BaseProxySocket _proxy;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _client = new BaseClientSocket();
            _client.Connected += new ConnectedEventHandler(_client_Connected);
            _client.DisConnected += new DisConnectedEventHandler(_client_DisConnected);
            _client.MessageReceived += new MessageReceivedEventHandler(_client_MessageReceived);
            _client.Connect("127.0.0.1",9100);
        }

        void _client_MessageReceived(ZProxySocket proxySocket, ZMessage message)
        {
            this.Invoke((Action)(delegate()
            {
                BaseMessage msg = message as BaseMessage;
                if (msg.MsgType == 1)  //文本
                {
                    textBox1.AppendText(proxySocket.RemoteIP + ":" + proxySocket.RemotePort + " 发送一条文本消息：\r\n");
                    textBox1.AppendText(Encoding.Unicode.GetString(msg.MsgContent) + "\r\n");
                }
                if (msg.MsgType == 2)  //图片
                {
                    textBox1.AppendText(proxySocket.RemoteIP + ":" + proxySocket.RemotePort + " 发送一条图片消息：\r\n");
                    BinaryFormatter bf = new BinaryFormatter();
                    pictureBox1.Image = bf.Deserialize(new MemoryStream(msg.MsgContent)) as Bitmap;  //将内容反序列化为bitmap
                }
            }));
        }

        void _client_DisConnected(ZProxySocket proxySocket)
        {
            this.Invoke((Action)(delegate()
            {
                textBox1.AppendText("与服务器断开\r\n");
            }));
        }

        void _client_Connected(ZProxySocket proxySocket)
        {
            this.Invoke((Action)(delegate()
            {
                if (proxySocket == null)
                {
                    textBox1.AppendText("连接服务器失败！");
                    
                }
                else
                {
                    textBox1.AppendText("连接服务器 " + proxySocket.RemoteIP + ":" + proxySocket.RemotePort + " 成功！");
                    _proxy = proxySocket as BaseProxySocket;
                }
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _proxy.SendMessage(new BaseMessage(1, Encoding.Unicode.GetBytes(textBox2.Text)));  //发送BaseMessage消息
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "图片文件|*.jpg;*.jpeg";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bf.Serialize(ms, Image.FromFile(ofd.FileName));
                        _proxy.SendMessage(new BaseMessage(2, ms.ToArray()));  //发送BaseMessage消息

                    }
                }
            }
        }
    }
}

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

namespace TCPServer
{
    public partial class Form1 : Form
    {
        BaseServerSocket _server;
        public Form1()
        {
            InitializeComponent();
        }
        List<ZProxySocket> list = new List<ZProxySocket>();
        private void Form1_Load(object sender, EventArgs e)
        {
            _server = new BaseServerSocket();
            _server.Connected += new ConnectedEventHandler(_server_Connected);
            _server.DisConnected += new DisConnectedEventHandler(_server_DisConnected);
            _server.MessageReceived += new MessageReceivedEventHandler(_server_MessageReceived);
            _server.StartAccept(9100);
            textBox1.AppendText("服务器启动，监听端口 " + 9000 + "...\r\n");
        }
        /// <summary>
        /// 接受消息
        /// </summary>
        /// <param name="proxySocket"></param>
        /// <param name="message"></param>
        void _server_MessageReceived(ZProxySocket proxySocket, ZMessage message)
        {
            this.Invoke((Action)(delegate()
            {
                BaseMessage msg = message as BaseMessage;
                if (msg.MsgType == 1)  //文本
                {
                    textBox1.AppendText(proxySocket.RemoteIP+":"+proxySocket.RemotePort+"发送一条文本消息：\r\n");
                    textBox1.AppendText(Encoding.Unicode.GetString(msg.MsgContent) + "\r\n");
                }
                if (msg.MsgType == 2)  //图片
                {
                    textBox1.AppendText(proxySocket.RemoteIP + ":" + proxySocket.RemotePort + "发送一条图片消息：\r\n");
                    BinaryFormatter bf = new BinaryFormatter();
                    pictureBox1.Image = bf.Deserialize(new MemoryStream(msg.MsgContent)) as Bitmap;  //将内容反序列化为bitmap
                }
            }));
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="proxySocket"></param>
        void _server_DisConnected(ZProxySocket proxySocket)
        {
            this.Invoke((Action)(delegate()
            {
                list.Remove(proxySocket);
                textBox1.AppendText(proxySocket.RemoteIP + ":" + proxySocket.RemotePort + "断开服务器\r\n");
            }));
        }
        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="proxySocket"></param>
        void _server_Connected(ZProxySocket proxySocket)
        {
            this.Invoke((Action)(delegate()
            {
                list.Add(proxySocket);
                textBox1.AppendText(proxySocket.RemoteIP + ":" + proxySocket.RemotePort + "连接服务器\r\n");
            }));
        }
        /// <summary>
        /// 发送文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            foreach (ZProxySocket proxy in list)
            {
                proxy.SendMessage(new BaseMessage(1,Encoding.Unicode.GetBytes(textBox2.Text)));  //发送BaseMessage消息
            }
        }
        /// <summary>
        /// 发送图片（可序列化对象）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        foreach (ZProxySocket proxy in list)
                        {
                            proxy.SendMessage(new BaseMessage(2, ms.ToArray()));  //发送BaseMessage消息
                        }
                    }
                }
            }
        }
    }
}

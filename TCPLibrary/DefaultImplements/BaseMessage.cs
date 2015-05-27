using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPLibrary.Abstracts;
using System.IO;

namespace TCPLibrary.DefaultImplements
{
    /// <summary>
    /// ZMessage的默认实现
    /// </summary>
    public class BaseMessage:ZMessage
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public int MsgType
        {
            get;
            set;
        }
        /// <summary>
        /// 消息内容
        /// </summary>
        public byte[] MsgContent
        {
            get;
            set;
        }
        public BaseMessage(int msgType, byte[] msgContent)
        {
            MsgType = msgType;
            MsgContent = msgContent;
        }
        /// <summary>
        /// 按照规定协议，重写RawData属性
        /// </summary>
        public override byte[] RawData
        {
            get
            {
                byte[] rawdata = new byte[4 + 4 + MsgContent.Length];  //消息类型 + 消息长度 + 消息内容
                using (MemoryStream ms = new MemoryStream(rawdata))
                {
                    BinaryWriter bw = new BinaryWriter(ms);
                    bw.Write(MsgType);  //先写入MsgType
                    bw.Write(MsgContent.Length);  //再写入MsgContent的长度
                    bw.Write(MsgContent); //最后写入消息内容
                    return rawdata;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ivi.Visa;
using VISAInstrument.Extension;
using VISAInstrument.Port;
using VISAInstrument.Utility;

namespace VISAInstrument
{
    public class USBPort_Work
    {
        public PortOperatorBase USBPort_portOperatorBase;
        public bool USBPort_isWritingError = false;
        /// <summary>
        ///给当前仪器端口负值
        /// </summary>
        ///  <param name="cbo_usb_string">输入端口名称字符串</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public bool NewPortInstance(string cbo_usb_string,out string message)
        {
            bool hasAddress = false;
            bool hasException = false;
            message = string.Empty;
            try
            {
                USBPort_portOperatorBase = new USBPortOperator(cbo_usb_string);
                hasAddress = true;
            }
            catch (Exception e1)
            {
                hasException = true;
                message = e1.ToString();
            }
            if (!hasException && hasAddress) USBPort_portOperatorBase.Timeout = (int)2000;
            return hasAddress;
        }
        /// <summary>
        ///打开或者关闭端口
        /// </summary>
        ///  <param name="open_close">"OPEN"打开，其余关闭</param>
        ///  <param name="cbo_usb_string">端口名称字符串</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public bool USBPort_Open_Close(string open_close,string cbo_usb_string, out string message)
        {
            message = string.Empty;
            if (open_close=="OPEN")
            {
                try
                {
                    if (!NewPortInstance(cbo_usb_string,out  message))
                    {
                        return false;
                    }
                    USBPort_portOperatorBase.Open();                
                }
                catch (Exception ex)
                {
                    message= ex.ToString();
                    return false;
                }
            }
            else
            {
                try
                {
                    USBPort_portOperatorBase.Close();
                }
                catch (Exception ex) { message = ex.ToString(); return false; }
            }
            return true;
        }
        /// <summary>
        ///发送字符串
        /// </summary>
        ///  <param name="USBPort_isAsciiCommand">TRUE:发送字符串，FALSE:发送HEX</param>
        ///  <param name="Write_text">发送数据</param>
        ///  <param name="AppendNewLine">TRUE添加换行符，false不添加换行符</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public bool USBPort_Write(bool USBPort_isAsciiCommand ,string Write_text,bool AppendNewLine, out string message)
        {
            message = string.Empty;
            USBPort_isWritingError = false;
            if (string.IsNullOrEmpty(Write_text))
            {//判定字符串为孔CommandNotEmpty
                message = "命令不能为空！";
                return false;
            }
            string asciiString = string.Empty;
            byte[] byteArray = null;
            if (USBPort_isAsciiCommand)
            {
                asciiString = Write_text;
            }
            else
            {
                if (StringEx.TryParseByteStringToByte(Write_text, out byte[] bytes))
                {
                    byteArray = bytes;
                }
                else
                {
                    USBPort_isWritingError = true;
                    message= "转换字节失败，请按照“XX XX XX”格式输入内容";
                    return false;
                }
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                if (USBPort_isAsciiCommand)
                {
                    if (AppendNewLine)
                    {
                        USBPort_portOperatorBase.WriteLine(asciiString);
                    }
                    else
                    {
                        USBPort_portOperatorBase.Write(asciiString);
                    }
                }
                else
                {
                    if (AppendNewLine)
                    {
                        USBPort_portOperatorBase.WriteLine(byteArray);
                    }
                    else
                    {
                        USBPort_portOperatorBase.Write(byteArray);
                    }
                }
            }
            catch (Exception ex)
            {
                message=$@"写入命令“{Write_text}”失败！\r\n{ex.Message}";
                return false;
            }
            message = $"[Time:{stopwatch.ElapsedMilliseconds}ms] Write: {Write_text}";
            return true;
        }
        /// <summary>
        ///读取字符串
        /// </summary>
        /// <param name="_isAsciiCommand">TRUE:接收字符串，FALSE:接收HEX</param>
        ///  <param name="isUntilNewLine">TRUE接收一行数据，FALSE接收固定长度数据</param>
        ///  <param name="specifiedCount">接收数据长度</param>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public bool USBPort_Read(bool _isAsciiCommand,bool isUntilNewLine, int specifiedCount, out string message,out string message_time)
        {
           // string result;
            Stopwatch stopwatch = Stopwatch.StartNew();//读取时间
            try
            {
                if (_isAsciiCommand)
                {
                    message = isUntilNewLine ? USBPort_portOperatorBase.ReadLine() : USBPort_portOperatorBase.Read(specifiedCount);
                }
                else
                {
                    byte[] bytes = isUntilNewLine ? USBPort_portOperatorBase.ReadToBytes() : USBPort_portOperatorBase.ReadToBytes(specifiedCount);
                    if (ByteEx.TryParseByteToByteString(bytes, out string byteString))
                    {
                        message = byteString;
                    }
                    else
                    {
                        message = "无法转换从接收缓冲区接收回来的数据";
                        message_time = $"[Time:{stopwatch.ElapsedMilliseconds}ms] ";
                        return false;
                    }
                }
            }
            catch (IOTimeoutException)
            {
                message = "读取时间超时";
                message_time = $"[Time:{stopwatch.ElapsedMilliseconds}ms] ";
                return false;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                message_time = $"[Time:{stopwatch.ElapsedMilliseconds}ms] ";
                return false;
            }
            message_time=$"[Time:{stopwatch.ElapsedMilliseconds}ms] ";
            return true;
        }

    }
}

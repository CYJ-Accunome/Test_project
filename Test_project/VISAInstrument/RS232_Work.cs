using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Ivi.Visa;
using VISAInstrument.Extension;
using VISAInstrument.Port;
using VISAInstrument.Utility;

namespace VISAInstrument
{
    public class RS232_Work
    {
        public PortOperatorBase RS232_portOperatorBase;
        public bool RS232_isWritingError = false;
        /// <summary>
        ///给当前仪器端口负值
        /// </summary>
        ///  <param name="cbo_232_string">输入端口名称字符串</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public bool NewPortInstance(string cbo_232_string, out string message)
        {
            bool hasAddress = false;
            bool hasException = false;
            message = string.Empty;
            try
            {
                RS232_portOperatorBase = new RS232PortOperator(cbo_232_string,
                                               (int)115200, SerialParity.None,
                                               SerialStopBitsMode.One, (int)8);

                hasAddress = true;
            }
            catch (Exception e1)
            {
                hasException = true;
                message = e1.ToString();
            }
            if (!hasException && hasAddress) RS232_portOperatorBase.Timeout = (int)2000;
            return hasAddress;
        }
        /// <summary>
        ///打开或者关闭端口
        /// </summary>
        ///  <param name="open_close">"OPEN"打开，其余关闭</param>
        ///  <param name="cbo_232_string">端口名称字符串</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public bool RS232_Open_Close(string open_close, string cbo_232_string, out string message)
        {
            message = string.Empty;
            if (open_close == "OPEN")
            {
                try
                {
                    if (!NewPortInstance(cbo_232_string, out message))
                    {
                        return false;
                    }
                    RS232_portOperatorBase.Open();                                         
                }
                catch (Exception ex)
                {
                    message = ex.ToString();
                    return false;
                }
            }
            else
            {
                try
                {
                    RS232_portOperatorBase.Close();               
                }
                catch (Exception ex) { message = ex.ToString(); return false; }
            }
            return true;
        }
        /// <summary>
        ///发送字符串
        /// </summary>
        ///  <param name="RS232_isAsciiCommand">TRUE:发送字符串，FALSE:发送HEX</param>
        ///  <param name="Write_text">发送数据</param>
        ///  <param name="AppendNewLine">TRUE添加换行符，false不添加换行符</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public bool RS232_Write(bool RS232_isAsciiCommand, string Write_text, bool AppendNewLine, out string message)
        {
            message = string.Empty;
            RS232_isWritingError = false;
            if (string.IsNullOrEmpty(Write_text))
            {//判定字符串为孔CommandNotEmpty
                message = "命令不能为空！";
                return false;
            }
            string asciiString = string.Empty;
            byte[] byteArray = null;
            if (RS232_isAsciiCommand)
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
                    RS232_isWritingError = true;
                    message = "转换字节失败，请按照“XX XX XX”格式输入内容";
                    return false;
                }
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                if (RS232_isAsciiCommand)
                {
                    if (AppendNewLine)
                    {
                        RS232_portOperatorBase.WriteLine(asciiString);
                    }
                    else
                    {
                        RS232_portOperatorBase.Write(asciiString);
                    }
                }
                else
                {
                    if (AppendNewLine)
                    {
                        RS232_portOperatorBase.WriteLine(byteArray);
                    }
                    else
                    {
                        RS232_portOperatorBase.Write(byteArray);
                    }
                }
            }
            catch (Exception ex)
            {
                message = $@"写入命令“{Write_text}”失败！\r\n{ex.Message}";
                return false;
            }
            message = $"[Time:{stopwatch.ElapsedMilliseconds}ms] Write: {Write_text}";
            return true;
        }
        /// <summary>
        ///发送字符串
        /// </summary>
        ///  <param name="Write_byte">发送数据</param>
        ///  <param name="offset">长度</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public bool RS232_WriteByte( byte[] Write_byte, int offset, out string message)
        {
            message = string.Empty;
            RS232_isWritingError = false;
            if (offset<=0)
            {//判定字符串为孔CommandNotEmpty
                message = "命令不能为空！";
                return false;
            }
            string asciiString = string.Empty;
            byte[] byteArray = new byte[offset];
            for(int i = 0;i<offset; i++)
            {
                byteArray[i] = Write_byte[i];
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                RS232_portOperatorBase.Write(byteArray);            
            }
            catch (Exception ex)
            {
                message = $@"写入命令失败！\r\n{ex.Message}";
                return false;
            }
            message = $"[Time:{stopwatch.ElapsedMilliseconds}ms] Write:";
            return true;
        }
    }
}

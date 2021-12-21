using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VISAInstrument.Extension;
using VISAInstrument.Port;
using VISAInstrument;
using VISAInstrument.Utility;
using Ivi.Visa;

namespace Test_project
{
    class P2230G_Instantiation
    {
        static USBPort_Work P2230G = new USBPort_Work();
        /// <summary>
        ///打开或者关闭端口
        /// </summary>
        ///  <param name="open_close">"OPEN"打开，其余关闭</param>
        ///  <param name="cbo_usb_string">端口名称字符串</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public static bool P2230G_Open_Close(string open_close, string cbo_usb_string, out string message)
        {
            return P2230G.USBPort_Open_Close(open_close, cbo_usb_string, out message);
        }
        /// <summary>
        ///P2230G错误查询
        /// </summary>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool P2230G_Config(out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!P2230G.USBPort_Write(true, "*RST", true, out message))
            {//发送错误
                message += " * RST";
                return false;
            }         
            if (!P2230G.USBPort_Write(true, "SYST:ERR?", true, out message))
            {//发送错误
                message += "SYST:ERR?";
                return false;
            }
            if (!P2230G.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取信息错误
                return false;
            }
            return true;
        }
        /// <summary>
        ///P2230G设置通道值
        /// </summary>
        /// <param name="ch">通道值"CH1|CH2|CH3"</param>
        /// <param name="voltage">电压值V</param>
        /// <param name="current">电流值A</param>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool P2230G_SET(string ch,string voltage, string current, out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!P2230G.USBPort_Write(true, ":SOURce:APPLy "+ch+", "+voltage + ", " +current, true, out message))
            {//发送错误
                message += " * RST";
                return false;
            }
            if (!P2230G.USBPort_Write(true, "SYST:ERR?", true, out message))
            {//发送错误
                message += "SYST:ERR?";
                return false;
            }
            if (!P2230G.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取信息错误
                return false;
            }
            return true;
        }
        /// <summary>
        ///P2230控制是能，开始或关闭输出
        /// </summary>
        /// <param name="EN">ON|OFF|0|1 1表示启用，0表示禁用</param>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool P2230G_EN(string EN, out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!P2230G.USBPort_Write(true, ":SOURce:OUTPut  " + EN, true, out message))
            {//发送错误
                message += " * RST";
                return false;
            }
            if (!P2230G.USBPort_Write(true, "SYST:ERR?", true, out message))
            {//发送错误
                message += "SYST:ERR?";
                return false;
            }
            if (!P2230G.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取信息错误
                return false;
            }
            return true;
        }
    }
}

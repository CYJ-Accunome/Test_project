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

    class PM100D_Instantiation
    {
        static USBPort_Work PM100D = new USBPort_Work();
        static string[] PM100D_WAV = new string[8] { "470", "520", "530", "565", "570", "615", "630", "690" };

        /// <summary>
        ///打开或者关闭端口
        /// </summary>
        ///  <param name="open_close">"OPEN"打开，其余关闭</param>
        ///  <param name="cbo_usb_string">端口名称字符串</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public static bool PD100_Open_Close(string open_close, string cbo_usb_string, out string message)
        {
            return PM100D.USBPort_Open_Close(open_close, cbo_usb_string, out message);
        }
        /// <summary>
        ///PD100错误查询
        /// </summary>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool PD100_Err(out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!PM100D.USBPort_Write(true, "SYST:ERR?", true, out message))
            {//发送错误
                return false;
            }
            if (!PM100D.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取信息错误
                return false;
            }
            return true;
        }
        /// <summary>
        ///PD100开始测量并读数
        /// </summary>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool PD100_READ(out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!PM100D.USBPort_Write(true, "READ?", true, out message))
            {//发送错误
                return false;
            }
            if (!PM100D.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取信息错误
                return false;
            }
            return true;
        }
        /// <summary>
        ///PD100查询当前的测量配置
        /// </summary>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool PD100_READConfing(out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!PM100D.USBPort_Write(true, "CONFigure?", true, out message))
            {//发送错误
                return false;
            }
            if (!PM100D.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取信息错误
                return false;
            }
            return true;
        }
        /// <summary>
        ///PD100设置波长
        /// </summary>
        /// <param name="WAW">设置的波长</param>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool PD100_WAV(string WAW,out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!PM100D.USBPort_Write(true, "SENS:CORR:WAV "+WAW, true, out message))
            {//发送错误
                return false;
            }
            if (!PM100D.USBPort_Write(true, "SENS:CORR:WAV?", true, out message))
            {//发送错误
                return false;
            }
            if (!PM100D.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取信息错误
                return false;
            }
            return true;
        }
    }
}






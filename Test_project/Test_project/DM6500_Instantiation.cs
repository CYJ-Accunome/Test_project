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
    class DM6500_Instantiation
    {
        static USBPort_Work DM6500 = new USBPort_Work();
        static string head = "1";
        /// <summary>
        ///打开或者关闭端口
        /// </summary>
        ///  <param name="open_close">"OPEN"打开，其余关闭</param>
        ///  <param name="cbo_usb_string">端口名称字符串</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public static bool DM6500_Open_Close(string open_close, string cbo_usb_string, out string message)
        {
            return DM6500.USBPort_Open_Close(open_close, cbo_usb_string, out message);
        }
        /// <summary>
        ///DM6500错误查询
        /// </summary>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool DM6500_Config(out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!DM6500.USBPort_Write(true, "*RST", true, out message))
            {//发送错误
                message += " * RST";
                return false;
            }
            if (!DM6500.USBPort_Write(true, ":ROUTe:TERMinals?", true, out message))
            {//查询前面板还是后面板
                message += ":ROUTe:TERMinals?";
                return false;
            }
            if (!DM6500.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取信息错误
                return false;
            }
            if(message=="FRON")
            {//如果是前面板
                head = "1";
                if (!DM6500.USBPort_Write(true, ":SENS:FUNC \"VOLT:DC\" ", true, out message)) //,(@1:10)
                {//发送错误
                    message += ":SENS:FUNC \"VOLT: DC\"";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":VOLT:RANG:AUTO ON", true, out message))
                {//发送错误
                    message += " :VOLT:RANG:AUTO ON";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:INP AUTO", true, out message))
                {//发送错误
                    message += ":SENS:VOLT:INP AUTO";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:NPLC 1", true, out message)) //这里10大致在200ms采集一次
                {//发送错误
                    message += " :SENS:VOLT:NPLC 10";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:AZER ON", true, out message))
                {//发送错误
                    message += "";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:AVER:TCON REP", true, out message))
                {//发送错误
                    message += "";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:AVER:COUN 5", true, out message))
                {//发送错误
                    message += "";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:AVER ON", true, out message)) //on启用滤波器 OFF关闭滤波器
                {//发送错误
                    message += "";
                    return false;
                }
            }
            else
            {//如果是后面板
                head = "1";
                if (!DM6500.USBPort_Write(true, ":SENS:FUNC \"VOLT:DC\",(@1:10) ", true, out message)) //,(@1:10)
                {//发送错误
                    message += ":SENS:FUNC \"VOLT: DC\",(@1:10)";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":VOLT:RANG:AUTO ON,(@1:10)", true, out message))
                {//发送错误
                    message += " :VOLT:RANG:AUTO ON,(@1:10)";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:INP AUTO,(@1:10)", true, out message))
                {//发送错误
                    message += ":SENS:VOLT:INP AUTO,(@1:10)";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:NPLC 1,(@1:10)", true, out message)) //这里10大致在200ms采集一次
                {//发送错误
                    message += " :SENS:VOLT:NPLC 10,(@1:10)";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:AZER ON,(@1:10)", true, out message))
                {//发送错误
                    message += "";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:AVER:TCON REP,(@1:10)", true, out message))
                {//发送错误
                    message += "";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:AVER:COUN 5,(@1:10)", true, out message))
                {//发送错误
                    message += "";
                    return false;
                }
                if (!DM6500.USBPort_Write(true, ":SENS:VOLT:AVER ON,(@1:10)", true, out message)) //on启用滤波器 OFF关闭滤波器
                {//发送错误
                    message += "";
                    return false;
                }
                //设置默认扫描通道
                if (!DM6500_Instantiation.DM6500_SHE_CH("5", out message, out message_time))
                {
                    
                    message += "DM6500配置通道失败";
                    return false;
                }
            }
            
            if (!DM6500.USBPort_Write(true, "SYST:ERR?", true, out message))
            {//发送错误
                message += "SYST:ERR?";
                return false;
            }
            if (!DM6500.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取信息错误
                return false;
            }
            return true;
        }
        /// <summary>
        ///DM6500开始测量
        /// </summary>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool DM6500_Start(out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!DM6500.USBPort_Write(true, "TRIG:CONT REST", true, out message))
            {//发送错误
                return false;
            }
            //if (!DM6500.USBPort_Read(true, true, 10, out message, out message_time))
            //{//读取信息错误
            //    return false;
            //}
            return true;
        }
        /// <summary>
        ///DM6500读取数据
        /// </summary>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool DM6500_READ_ALL(out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            string s_head = "1";
            int t_head, t_end;
            if (!DM6500.USBPort_Write(true, ":TRACe:ACTual:END?", true, out message))//读取缓冲区数据
            {//发送错误
                return false;
            }  
            //读取数据结尾
            if (!DM6500.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取信息错误
                return false;
            }
            s_head = System.Convert.ToString((System.Convert.ToInt32(message)+1));
            t_end = System.Convert.ToInt32(message);
            t_head = System.Convert.ToInt32(head);
            if(t_end - t_head>10)
            { 
                t_head = t_end - 10;
                head = System.Convert.ToString(t_end);
            }

            if (!DM6500.USBPort_Write(true, "TRAC:DATA? "+ head+", " + message+",\"defbuffer1\"", true, out message))//读取缓冲区数据
            {//发送错误
                return false;
            }
            if (!DM6500.USBPort_Read(true, true, 10, out message, out message_time))
            {//读取数据
                return false;
            }
            head = s_head;
            return true;
        }
        /// <summary>
        ///删除缓冲区DM6500读取数据
        /// </summary>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool DM6500_DEL(out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!DM6500.USBPort_Write(true, "TRAC:DEL \"defbuffer1\"", true, out message))//读取缓冲区数据
            {//发送错误
                return false;
            }
            return true;
        }
        /// <summary>
        ///清空DM6500读取数据
        /// </summary>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool DM6500_CLE(out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!DM6500.USBPort_Write(true, "TRAC:CLEar \"defbuffer1\"", true, out message))//读取缓冲区数据
            {//发送错误
                return false;
            }
            return true;
        }
        /// <summary>
        ///DM6500通道设置
        /// </summary>
        /// <param name="ch">通道0~10</param>
        ///  <param name="message">输出的信息</param>
        ///  <param name="message_time">当前时间</param>
        /// <returns>成功返回TRUE</returns>
        public static bool DM6500_SHE_CH(string ch,out string message, out string message_time)
        {
            message = string.Empty;
            message_time = string.Empty;
            if (!DM6500.USBPort_Write(true, "ROUTe:CLOSe (@" + ch + ")", true, out message))
            {//关闭所有通道
                return false;
            }

            if (!DM6500.USBPort_Write(true, "DISP:WATC:CHAN (@" + ch + ")", true, out message))
            {//发送错误
                return false;
            }
            return true;
        }
    }
}

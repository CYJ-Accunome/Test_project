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
    class RS232_Instantiation
    {
        public static RS232_Work RS232 = new RS232_Work();
        /// <summary>
        ///打开或者关闭串口
        /// </summary>
        ///  <param name="open_close">"OPEN"打开，其余关闭</param>
        ///  <param name="cbo_usb_string">端口名称字符串</param>
        ///  <param name="message">输出的信息</param>
        /// <returns>成功返回TRUE</returns>
        public static bool RS232_Open_Close(string open_close, string cbo_usb_string, out string message)
        {
            return RS232.RS232_Open_Close(open_close, cbo_usb_string, out message);           
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test_project
{
    class set_win_time
    {
        public struct SYSTEMTIME
        {
            [MarshalAs(UnmanagedType.U2)]
            internal ushort wYear; // 年
            [MarshalAs(UnmanagedType.U2)]
            internal ushort wMonth; // 月
            [MarshalAs(UnmanagedType.U2)]
            internal ushort wDayOfWeek; // 星期
            [MarshalAs(UnmanagedType.U2)]
            internal ushort wDay; // 日
            [MarshalAs(UnmanagedType.U2)]
            internal ushort wHour; // 时
            [MarshalAs(UnmanagedType.U2)]
            internal ushort wMinute; // 分
            [MarshalAs(UnmanagedType.U2)]
            internal ushort wSecond; // 秒
            [MarshalAs(UnmanagedType.U2)]
            internal ushort wMilliseconds; // 毫秒
            /// <summary>
            /// 从System.DateTime转换。
            /// </summary>
            /// <param name="time">System.DateTime类型的时间。</param>
            public void FromDateTime(DateTime time)
            {
                wYear = (ushort)time.Year;
                wMonth = (ushort)time.Month;
                wDayOfWeek = (ushort)time.DayOfWeek;
                wDay = (ushort)time.Day;
                wHour = (ushort)time.Hour;
                wMinute = (ushort)time.Minute;
                wSecond = (ushort)time.Second;
                wMilliseconds = (ushort)time.Millisecond;
            }
            /// <summary>
            /// 转换为System.DateTime类型。
            /// </summary>
            /// <returns></returns>
            public DateTime ToDateTime()
            {
                return new DateTime(wYear, wMonth, wDay, wHour, wMinute, wSecond, wMilliseconds);
            }
            /// <summary>
            /// 静态方法。转换为System.DateTime类型。
            /// </summary>
            /// <param name="time">SYSTEMTIME类型的时间。</param>
            /// <returns></returns>
            public static DateTime ToDateTime(SYSTEMTIME time)
            {
                return time.ToDateTime();
            }
        }
        public class Win32API
        {
            [DllImport("Kernel32.dll")]
            public static extern bool SetLocalTime(ref SYSTEMTIME Time);
            [DllImport("Kernel32.dll")]
            public static extern void GetLocalTime(ref SYSTEMTIME Time);
            //设定，获取系统时间,SetSystemTime()默认设置的为UTC时间，比北京时间少了8个小时因此我们只用LocalTime。
            [DllImport("Kernel32.dll")]
            public static extern void GetSystemTime(ref SYSTEMTIME Time);

            [DllImport("Kernel32.dll")]
            public static extern bool SetSystemTime(ref SYSTEMTIME Time);
        }

        /// <summary>
        ///设置时间
        /// </summary>
        ///<param name="s"/>是否处理数据</param>
        /// </summary>
        /// 
        public static void TIME()
        {
            Random sdatan = new Random();
            //foreach (var sData in r)
            //{//遍历所有数据并显示
            //    sData.FluorescenceData = sData.FluorescenceData + sdatan.Next(1, 200);
            //取得当前系统时间
            DateTime t = DateTime.Now;
            //在当前时间上加上一周
             //t = t.AddDays(3);
            // t = t.AddDays(-3);
            t = t.AddMinutes(3);
            t = t.AddMilliseconds(sdatan.Next(1, 10));
           
            
            //转换System.DateTime到SYSTEMTIME
            SYSTEMTIME st = new SYSTEMTIME();
            st.FromDateTime(t);
            //调用Win32 API设置系统时间
            Win32API.SetLocalTime(ref st);

        }

    }
}

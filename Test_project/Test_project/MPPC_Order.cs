using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_project
{
    /// <summary>
    ///通过主界面串口发送数据
    /// </summary>
    /// <param name="SendByte_T">数据保存的地址</param>
    /// <param name="SendByte_i_T">生成的数据长度</param>
    /// <param name="threadon">true当前主线程，false非主线程，需要托管</param>
    /// <param name="time_out">超时时间，只有在非主线程有效</param>
    public delegate bool DX_com_SEND(byte[] SendByte_T, int SendByte_i_T, bool threadon, int time_out);
    public class MPPC_Order
    {
        //定义一个委托
        public DX_com_SEND COM_SEND;
        /// <summary>
        ///设置扫描头开始控制,打开/关闭高压与LED
        /// </summary>
        /// <param name="t">1打开、0关闭 </param>
        ///<param name="threadon">true当前主线程，false非主线程，需要托管</param>
        public void MPPC_Power_OUT(byte t, bool threadon)
        {
            byte[] SendByte = new byte[2500];
            int SendByte_i = 0;
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0xA4;//设置mpp高压输出
            SendByte[SendByte_i++] = t;


            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            COM_SEND(SendByte, SendByte_i, threadon, 100);//串口数据发送
        }
        /// <summary>
        ///设置扫描头高压输出或关闭
        /// </summary>
        /// <param name="t">1打开、0关闭 </param>
        ///<param name="threadon">true当前主线程，false非主线程，需要托管</param>
        public void MPPC_Vop_OUT(byte t, bool threadon)
        {
            byte[] SendByte = new byte[2500];
            int SendByte_i = 0;
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0x0D;
            SendByte[SendByte_i++] = t;
            SendByte[SendByte_i++] = t;
            SendByte[SendByte_i++] = t;
            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            COM_SEND(SendByte, SendByte_i, threadon, 100);//串口数据发送
        }
        /// <summary>
        ///设置扫描头led输出或关闭
        /// </summary>
        /// <param name="t">1打开、0关闭 </param>
        ///<param name="threadon">true当前主线程，false非主线程，需要托管</param>
        public void MPPC_LED_OUT(byte t, bool threadon)
        {
            byte[] SendByte = new byte[2500];
            int SendByte_i = 0;
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0x08;
            SendByte[SendByte_i++] = t;

            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            COM_SEND(SendByte, SendByte_i, threadon, 100);//串口数据发送
        }
        /// <summary>
        ///读取扫描头温度
        /// </summary>
        ///<param name="threadon">true当前主线程，false非主线程，需要托管</param>
        public void MPPC_Temp_Get( bool threadon)
        {
            byte[] SendByte = new byte[2500];
            int SendByte_i = 0;
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0xA5;

            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            COM_SEND(SendByte, SendByte_i, threadon, 0);//串口数据发送
        }
    }
}

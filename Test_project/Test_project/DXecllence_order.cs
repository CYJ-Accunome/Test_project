using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_project
{
    class DXecllence_order
    {
        /// <summary>
        ///MPPC的LED控制电流EEPROM设置
        /// </summary>
        /// <param name="SendByte">数据保存的地址</param>
        /// <param name="SendByte_i">生成的数据长度</param>
        public static void And_check(ref byte[] SendByte,byte SendByte_i)
        {
            byte i = 0;
            byte add = 0x00;
            for(i = 0;i< SendByte_i-1;i++)
            {
                add += SendByte[i];
            }
            SendByte[i]= add;
        }
        /// <summary>
        ///握手指令
        /// </summary>
        /// <param name="SendByte">数据保存的地址</param>
        /// <param name="SendByte_i">生成的数据长度</param>
        /// <param name="cmdno">帧序列号</param>
        public static void HELLO(ref byte[] SendByte,out byte SendByte_i,byte cmdno)
        {
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5E;//头
            SendByte[SendByte_i++] = 0x01;  //功能

            SendByte[SendByte_i++] = cmdno;// 帧序号
            SendByte[SendByte_i++] = 0x01;// 长度

            SendByte[SendByte_i++] = 0x01;  //校验位 和教研
            SendByte[3] = SendByte_i;//3是数据长度
            And_check(ref SendByte, SendByte_i);           
        }
        /// <summary>
        ///扫描电机复位指令
        /// </summary>
        /// <param name="SendByte">数据保存的地址</param>
        /// <param name="SendByte_i">生成的数据长度</param>
        /// <param name="cmdno">帧序列号</param>
        public static void XSCAN_RST(ref byte[] SendByte, out byte SendByte_i, byte cmdno)
        {
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5E;//头
            SendByte[SendByte_i++] = 0x07;  //功能

            SendByte[SendByte_i++] = cmdno;// 帧序号
            SendByte[SendByte_i++] = 0x01;// 长度

            SendByte[SendByte_i++] = 0x13;

            SendByte[SendByte_i++] = 0x01;  //校验位 和教研
            SendByte[3] = SendByte_i;//3是数据长度
            And_check(ref SendByte, SendByte_i);
        }
        /// <summary>
        ///打开LED指令
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5/ALLON/ALLOFF"</param>
        /// <param name="SendByte">数据保存的地址</param>
        /// <param name="SendByte_i">生成的数据长度</param>
        /// <param name="cmdno">帧序列号</param>
        public static void LED_OPEN_ON(string FluorescenceChannel,ref byte[] SendByte, out byte SendByte_i, byte cmdno)
        {
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5E;//头
            SendByte[SendByte_i++] = 0x08;  //功能

            SendByte[SendByte_i++] = cmdno;// 帧序号
            SendByte[SendByte_i++] = 0x01;// 长度

            SendByte[SendByte_i++] = 0x01;
            switch(FluorescenceChannel)
            {
                case "FAM":
                    SendByte[SendByte_i++] = 0x02;
                    break;
                case "VIC":
                    SendByte[SendByte_i++] = 0x08;
                    break;
                case "ROX":
                    SendByte[SendByte_i++] = 0x01;
                    break;
                case "CY5":
                    SendByte[SendByte_i++] = 0x04;
                    break;
                case "ALLON":
                    SendByte[SendByte_i++] = 0xFF;
                    break;
                case "ALLOFF":
                    SendByte[SendByte_i++] = 0x00;
                    break;
                default:
                    SendByte[SendByte_i++] = 0x00;
                    break;
            }
            

            SendByte[SendByte_i++] = 0x01;  //校验位 和教研
            SendByte[3] = SendByte_i;//3是数据长度
            And_check(ref SendByte, SendByte_i);
        }
        /// <summary>
        ///测试模式的选择
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5/ALLON"</param>
        /// <param name="SendByte">数据保存的地址</param>
        /// <param name="SendByte_i">生成的数据长度</param>
        /// <param name="cmdno">帧序列号</param>
        public static void TEST_MODE(string FluorescenceChannel, ref byte[] SendByte, out byte SendByte_i, byte cmdno)
        {
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5E;//头
            SendByte[SendByte_i++] = 0x24;  //功能

            SendByte[SendByte_i++] = cmdno;// 帧序号
            SendByte[SendByte_i++] = 0x01;// 长度
           
            switch (FluorescenceChannel)
            {
                case "FAM":
                    SendByte[SendByte_i++] = 0x01;// 
                    SendByte[SendByte_i++] = 0x02;
                    break;
                case "VIC":
                    SendByte[SendByte_i++] = 0x01;// 
                    SendByte[SendByte_i++] = 0x08;
                    break;
                case "ROX":
                    SendByte[SendByte_i++] = 0x01;// 
                    SendByte[SendByte_i++] = 0x01;
                    break;
                case "CY5":
                    SendByte[SendByte_i++] = 0x01;// 
                    SendByte[SendByte_i++] = 0x04;
                    break;
                case "ALLON":
                    SendByte[SendByte_i++] = 0x00;// 
                    SendByte[SendByte_i++] = 0xFF;
                    break;
                case "ALLOFF":
                    SendByte[SendByte_i++] = 0x00;// 
                    SendByte[SendByte_i++] = 0x00;
                    break;
                default:
                    SendByte[SendByte_i++] = 0x00;// 
                    SendByte[SendByte_i++] = 0x00;
                    break;
            }


            SendByte[SendByte_i++] = 0x01;  //校验位 和教研
            SendByte[3] = SendByte_i;//3是数据长度
            And_check(ref SendByte, SendByte_i);
        }
        /// <summary>
        ///设置MPPC增益
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5/ALLON/ALLOFF"</param>
        /// <param name="SendByte">数据保存的地址</param>
        /// <param name="SendByte_i">生成的数据长度</param>
        /// <param name="cmdno">帧序列号</param>
        public static void MPPC_GAIN(string FluorescenceChannel, ref byte[] SendByte, out byte SendByte_i, byte cmdno)
        {
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5E;//头
            SendByte[SendByte_i++] = 0x0D;  //功能

            SendByte[SendByte_i++] = cmdno;// 帧序号
            SendByte[SendByte_i++] = 0x01;// 长度

            switch (FluorescenceChannel)
            {
                case "FAM":
                    SendByte[SendByte_i++] = 0x01;// 
                    SendByte[SendByte_i++] = 0x0F;
                    SendByte[SendByte_i++] = 0xA0;
                    break;
                case "VIC":
                    SendByte[SendByte_i++] = 0x03;// 
                    SendByte[SendByte_i++] = 0x0F;
                    SendByte[SendByte_i++] = 0xA0;
                    break;
                case "ROX":
                    SendByte[SendByte_i++] = 0x00;// 
                    SendByte[SendByte_i++] = 0x0F;
                    SendByte[SendByte_i++] = 0xA0;
                    break;
                case "CY5":
                    SendByte[SendByte_i++] = 0x02;// 
                    SendByte[SendByte_i++] = 0x0F;
                    SendByte[SendByte_i++] = 0xA0;
                    break;
                case "ALLON":
                    SendByte[SendByte_i++] = 0x00;// 
                    SendByte[SendByte_i++] = 0x0F;
                    SendByte[SendByte_i++] = 0xA0;
                    break;
                case "ALLOFF":
                    SendByte[SendByte_i++] = 0x03;// 
                    SendByte[SendByte_i++] = 0x00;
                    SendByte[SendByte_i++] = 0x00;
                    break;
                default:
                    SendByte[SendByte_i++] = 0x03;// 
                    SendByte[SendByte_i++] = 0x00;
                    SendByte[SendByte_i++] = 0x00;
                    break;
            }


            SendByte[SendByte_i++] = 0x01;  //校验位 和教研
            SendByte[3] = SendByte_i;//3是数据长度
            And_check(ref SendByte, SendByte_i);
        }
        /// <summary>
        ///xscan电机扫描
        /// </summary>
        /// <param name="step_length">扫描步长</param>
        /// <param name="SendByte">数据保存的地址</param>
        /// <param name="SendByte_i">生成的数据长度</param>
        /// <param name="cmdno">帧序列号</param>
        public static void XSCAN_SCAN(UInt32 step_length, ref byte[] SendByte, out byte SendByte_i, byte cmdno)
        {
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5E;//头
            SendByte[SendByte_i++] = 0x07;  //功能

            SendByte[SendByte_i++] = cmdno;// 帧序号
            SendByte[SendByte_i++] = 0x01;// 长度

           
            SendByte[SendByte_i++] = 0x15;//辅助 
            SendByte[SendByte_i++] = (byte)(step_length>>16);
            SendByte[SendByte_i++] = (byte)(step_length >> 8);
            SendByte[SendByte_i++] = (byte)(step_length);

            SendByte[SendByte_i++] = 0x00;
            SendByte[SendByte_i++] = 0x4e;
            SendByte[SendByte_i++] = 0x20;

            SendByte[SendByte_i++] = 0x01;  //校验位 和教研
            SendByte[3] = SendByte_i;//3是数据长度
            And_check(ref SendByte, SendByte_i);
        }
        /// <summary>
        ///read——data 读取原始数据
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="SendByte">数据保存的地址</param>
        /// <param name="SendByte_i">生成的数据长度</param>
        /// <param name="cmdno">帧序列号</param>
        public static void READ_DATA(string FluorescenceChannel, ref byte[] SendByte, out byte SendByte_i, byte cmdno)
        {
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5E;//头
            SendByte[SendByte_i++] = 0x0C;  //功能

            SendByte[SendByte_i++] = cmdno;// 帧序号
            SendByte[SendByte_i++] = 0x01;// 长度

            switch (FluorescenceChannel)
            {
                case "FAM":
                    SendByte[SendByte_i++] = 0x00;// 
                    break;
                case "VIC":
                    SendByte[SendByte_i++] = 0x02;// 
                    break;
                case "ROX":
                    SendByte[SendByte_i++] = 0x03;// 
                    break;
                case "CY5":
                    SendByte[SendByte_i++] = 0x01;// 
                    break;             
                default:
                    SendByte[SendByte_i++] = 0x00;
                    break;
            }
            SendByte[SendByte_i++] = 0x01;  //校验位 和教研
            SendByte[3] = SendByte_i;//3是数据长度
            And_check(ref SendByte, SendByte_i);
        }
        /// <summary>
        ///发送EEPROM数据
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="Address">eeprom首地址</param>
        /// <param name="DATA_length">读取EEPROM长度</param>
        /// <param name="data">发送的EEPROM数据</param>
        /// <param name="SendByte">数据保存的地址</param>
        /// <param name="SendByte_i">生成的数据长度</param>
        /// <param name="cmdno">帧序列号</param>
        public static void SEND_EEPROM_DATA(byte id, int Address, byte DATA_length,byte[] data, ref byte[] SendByte, out byte SendByte_i, byte cmdno)
        {
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5E;//头
            SendByte[SendByte_i++] = 0x12;  //功能

            SendByte[SendByte_i++] = cmdno;// 帧序号
            SendByte[SendByte_i++] = 0x01;// 长度

            SendByte[SendByte_i++] = id;// 节点ID

            SendByte[SendByte_i++] = (byte)(Address >> 16); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Address >> 8); //偏移地址中8位
            SendByte[SendByte_i++] = (byte)(Address); //偏移地址低8位
            byte[] data2 = new byte[256];

            //for (int i = 0;i<DATA_length;i=i+2)
            //{
            //    data2[i] = data[i+1]; 
            //    data2[i+1] = data[i ]; 
            //}
            //if(DATA_length%2==1)
            //{//如果数据是奇数个  最后一个不需要转换
            //    data2[DATA_length- 1] = data[DATA_length-1];
            //}
            for (int i = 0; i < DATA_length; i++)
            {
                SendByte[SendByte_i++] = data[i]; //偏移地址低8位
            }
            

            SendByte[SendByte_i++] = 0x01;  //校验位 和教研
            SendByte[3] = SendByte_i;//3是数据长度
            And_check(ref SendByte, SendByte_i);
        }
        /// <summary>
        ///读取EEPROM数据
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="Address">eeprom首地址</param>
        /// <param name="DATA_length">读取EEPROM长度</param>
        /// <param name="SendByte">数据保存的地址</param>
        /// <param name="SendByte_i">生成的数据长度</param>
        /// <param name="cmdno">帧序列号</param>
        public static void READ_EEPROM_DATA(byte id,int Address,byte DATA_length, ref byte[] SendByte, out byte SendByte_i, byte cmdno)
        {
            SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5E;//头
            SendByte[SendByte_i++] = 0x11;  //功能

            SendByte[SendByte_i++] = cmdno;// 帧序号
            SendByte[SendByte_i++] = 0x01;// 长度

            SendByte[SendByte_i++] = id;// 节点ID

            SendByte[SendByte_i++] = (byte)(Address >> 16); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Address >> 8); //偏移地址中8位
            SendByte[SendByte_i++] = (byte)(Address); //偏移地址低8位

            SendByte[SendByte_i++] = DATA_length;// 数据长度
            SendByte[SendByte_i++] = 0x01;  //校验位 和教研
            SendByte[3] = SendByte_i;//3是数据长度
            And_check(ref SendByte, SendByte_i);
        }
    }
}

#define PCB_V2_4

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;
using System.Threading;
using VISAInstrument.Extension;
using VISAInstrument.Port;
using VISAInstrument;
using VISAInstrument.Utility;
using Ivi.Visa;
using Test.Shart_Show;
using System.Collections;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;


namespace Test_project
{

    public partial class DXecllence : UserControl
    {
        Task t_Task = null;
        byte[] SendByte = new byte[256];
        byte SendByte_i = 0;
        byte cmdno = 0;
        bool SendByte_ok = true;
        Thread RS232_read;
        public INI_CSV ini_ces_instantiation = new INI_CSV();
    
        public DXecllence()
        {
            InitializeComponent();
            //刷新串口RS232
            GetPort();
            MPPC_SET.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            tab_INI.Controls.Clear();//清空当前窗口的窗口控件
            tab_INI.Controls.Add(ini_ces_instantiation);
            ini_ces_instantiation.Shart_Show_Auto(tab_INI.Size.Width, tab_INI.Size.Height);
            ini_ces_instantiation.RS232_Eeprom_SEND += RS232_SEND;
            text_centre_FAM.Text = System.Configuration.ConfigurationManager.AppSettings["FAM_Threshold"];
            text_centre_VIC.Text = System.Configuration.ConfigurationManager.AppSettings["VIC_Threshold"];
            text_centre_ROX.Text = System.Configuration.ConfigurationManager.AppSettings["ROX_Threshold"];
            text_centre_CY5.Text = System.Configuration.ConfigurationManager.AppSettings["CY5_Threshold"];
        }
        void GetPort()
        {
            int i;
            string[] content1 = PortUltility.FindAddresses(PortType.RS232);
            string[] content2 = PortUltility.FindRS232Type(content1);
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            for (i = 0; i < content2.Length; i++)
            {
                if (content2[i].Contains("LPT")) continue;
                list1.Add(content1[i]);
                list2.Add(content2[i]);
            }
            content1 = list1.ToArray();
            content2 = list2.ToArray();
            cboRS232.ShowAndDisplay(content1, content2);
            string s = System.Configuration.ConfigurationManager.AppSettings["RS232"];
            int num = content2.Length; //获取下拉框数量
            if (num < 1)
            {
                return;
            }
            for (i = 0; i < num; i++)
            {
                if (s == (content2[i].ToString()))
                {
                    break;
                }
            }
            if (i >= num)  //说明串口号已经消失
            {
                cboRS232.SelectedIndex = 0;//获取或设置指定当前选定项的索引。
            }
            else
            {
                cboRS232.SelectedIndex = i;//获取或设置指定当前选定项的索引。
            }
            s = System.Configuration.ConfigurationManager.AppSettings["RS232Enabled"];
            if (s == "关闭串口")
            {
                string message;
                if (!RS232_Instantiation.RS232_Open_Close("OPEN", ((Pair<string, string>)cboRS232.SelectedItem).Value.ToString(), out message))
                {
                    MessageBox.Show("串口打开失败");
                    return;
                }
                Serial_Data.AppendText("打开串口232" + message + "\r\n");
                //if (RS232_Instantiation.RS232.RS232_portOperatorBase is RS232PortOperator portOperator)
                //{
                //    portOperator.DataReceived += PortOperator_DataReceived;
                //}
                RS232_read = new Thread(new ThreadStart(RS232_Read));
                RS232_read.IsBackground = true;
                RS232_read.Start();
                btnOpen.Text = "关闭串口";
                btnOpen.Text = "关闭串口";
                GetAppSetting.GetAppSetting_data("RS232", cboRS232.Text);
                GetAppSetting.GetAppSetting_data("RS232Enabled", btnOpen.Text);
            }
        }
        /// <summary>
        ///设置显示窗口的大小
        /// </summary>
        /// <param name="Width">修改后窗口宽度</param>
        ///  <param name="Height">修改后窗口长度</param>
        /// <returns>成功返回TRUE</returns>
        public void Shart_Show_Auto(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            btnRefresh.Enabled = false; btnOpen.Enabled = false;
            string[] content1 = PortUltility.FindAddresses(PortType.RS232);
            string[] content2 = PortUltility.FindRS232Type(content1);
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            for (int i = 0; i < content2.Length; i++)
            {
                if (content2[i].Contains("LPT")) continue;
                list1.Add(content1[i]);
                list2.Add(content2[i]);
            }
            content1 = list1.ToArray();
            content2 = list2.ToArray();
            cboRS232.ShowAndDisplay(content1, content2);
            btnRefresh.Enabled = true; btnOpen.Enabled = true;
        }
        private void InvokeToForm(Action action)
        {
            try
            {
                this.Invoke(action);
               
            }
            catch { }
        }
        //UInt16[] read_data = new UInt16[6000];
        int[,] read_data = new int[4, 6000];
        string Read_data_Channel = "FAM";
        int read_data_len = 0;
        /// <summary>
        ///解码读取的荧光数据
        /// </summary>
        private void Read_Data_Decode()
        {
            int i, j;
            int len=(USART_RX_BUF[3]-6)/ 2;
            switch (Read_data_Channel)
            {
                case "FAM":
                    i = 0x00;// 
                    break;
                case "VIC":
                    i = 0x01;// 
                    break;
                case "ROX":
                    i  = 0x02;// 
                    break;
                case "CY5":
                    i = 0x03;// 
                    break;
                default:
                    i = 0x00;
                    break;
            }
            //i = 0;
            for(j=0;j<len;j++)
            {
                read_data[i, read_data_len++] = (USART_RX_BUF[4 + 2 * j])+ (USART_RX_BUF[5 + 2 * j] << 8);
               
            }
            InvokeToForm(() =>
            {
                Serial_Data.AppendText(read_data_len.ToString()+"读取数据……………………\r\n");
            });
        }

            byte[] USART_RX_BUF = new byte[1200];//接收缓冲区  USART_RX_BUF[];     //接收缓冲,最大USART_REC_LEN个字节.
        //接收到的有效字节数目
        int USART_RX_STA = 0;       //接收状态标记	  
        bool USART_RX_STA1 = false;
        int USART_step = 1;
        byte USART_numx = 0;//有效数据长度
        //串口回调函数
        private void PortOperator_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            if (e.BytesToRead > 0) //Read(false, e.BytesToRead)
            {
                byte[] recBuffer;//= new byte[e.BytesToRead]
                recBuffer = RS232_Instantiation.RS232.RS232_portOperatorBase.ReadToBytes(e.BytesToRead);
                //string recData;//接收数据转码后缓存
                //recData = System.Text.Encoding.Default.GetString(recBuffer);//转码
                //下面进行16进制接收
                //StringBuilder recBuffer16 = new StringBuilder();//定义16进制接收缓存(可变长度)
                //for (int i = 0; i < recBuffer.Length; i++)
                //{
                //    recBuffer16.AppendFormat("{0:X2}" + " ", recBuffer[i]);
                //    // recBuffer16.AppendFormat("0X"+"{0:X2}" + " ",recBuffer[i]);
                //}
                //recData = recBuffer16.ToString();
                //InvokeToForm(() => { Serial_Data.AppendText(recData); });
                //进行数据解码
                byte Res;
                for (int i = 0; i < recBuffer.Length; i++)
                {
                    Res = recBuffer[i];
                    if (USART_step == 1) //说明还没有接收到数据开始
                    {
                        USART_RX_BUF[USART_RX_STA] = Res;//将数据保存至缓冲区
                        if (USART_RX_BUF[0] == 0x5E) //这里判断是不是0X5E
                        {
                            USART_RX_STA++;                        
                            USART_step = 2;
                            USART_numx = 5;
                        }
                        else
                        {//这里说明未接收到正确的数据
                            USART_RX_STA = 0;
                        }
                    }
                    else
                    {
                        USART_RX_BUF[USART_RX_STA++] = Res; //将数据保存至缓冲区
                        if(USART_RX_STA==4)//说明词字节为长度
                        {
                            USART_numx = USART_RX_BUF[3];
                        }
                        else
                        {
                            if ((USART_RX_STA) >= USART_numx)//说明数据接收完成
                            {
                                //判断和校验是否正确
                                DXecllence_order.And_check(ref USART_RX_BUF, USART_numx);//计算和校验
                                if(USART_RX_BUF[USART_numx-1] == Res)
                                {
                                    USART_RX_STA1 = true;	//接收完成了
                                }
                                else
                                {
                                    USART_step = 1;
                                    USART_RX_STA = 0;//接收数据错误,重新开始接收
                                }
                            }
                        }
                    }
                    if(USART_RX_STA1)
                    {
                        string USART_T = null;
                        switch (USART_RX_BUF[1]&0x7f)
                        {
                            case 0X01:
                                USART_T="握手指令返回\r\n";
                                break;
                            case 0X07:
                                USART_T="电机指令返回\r\n";
                                break;
                            case 0X08:
                                USART_T="led控制返回指令\r\n";
                                break;
                            case 0X0d:
                                USART_T = "mppc增益控制返回指令\r\n";
                                break;
                            case 0X24:
                                USART_T="模式设置返回指令\r\n";
                                break;
                            case 0x0c:                                
                                Read_Data_Decode();
                                break;
                            case 0x11:
                                USART_T = "EEPROM读取完成\r\n";

                                break;
                            case 0x12:
                                USART_T = "EEPROM发送完成\r\n";
                                break;
                            default:

                                break;
                        }
                        if (USART_T != null) {
                            InvokeToForm(() =>
                            {

                                Serial_Data.AppendText(USART_T);
                            });
                        }
                            
                        SendByte_ok = true;
                        USART_step = 1;
                        USART_RX_STA = 0;
                        USART_RX_STA1 = false;
                    }  
                }
            }
        }
        public void RS232_Read()
        {
            byte[] recBuffer;
            string result;
            byte Res;
            while (true)
            {
                try
                {
                    recBuffer = RS232_Instantiation.RS232.RS232_portOperatorBase.ReadToBytes(1);
                    Res = recBuffer[0];
                    if (USART_step == 1) //说明还没有接收到数据开始
                    {
                        USART_RX_BUF[USART_RX_STA] = Res;//将数据保存至缓冲区
                        if (USART_RX_BUF[0] == 0x5E) //这里判断是不是0X5E
                        {
                            USART_RX_STA++;
                            USART_step = 2;
                            USART_numx = 5;
                        }
                        else
                        {//这里说明未接收到正确的数据
                            USART_RX_STA = 0;
                        }
                    }
                    else
                    {
                        USART_RX_BUF[USART_RX_STA++] = Res; //将数据保存至缓冲区
                        if (USART_RX_STA == 4)//说明词字节为长度
                        {
                            USART_numx = USART_RX_BUF[3];
                        }
                        else
                        {
                            if ((USART_RX_STA) >= USART_numx)//说明数据接收完成
                            {
                                //判断和校验是否正确
                                DXecllence_order.And_check(ref USART_RX_BUF, USART_numx);//计算和校验
                                if (USART_RX_BUF[USART_numx - 1] == Res)
                                {
                                    USART_RX_STA1 = true;	//接收完成了
                                }
                                else
                                {
                                    USART_step = 1;
                                    USART_RX_STA = 0;//接收数据错误,重新开始接收
                                }
                            }
                        }
                    }
                    if (USART_RX_STA1)
                    {
                        string USART_T = null;
                        switch (USART_RX_BUF[1] & 0x7f)
                        {
                            case 0X01:
                                USART_T = "握手指令返回\r\n";
                                break;
                            case 0X07:
                                USART_T = "电机指令返回\r\n";
                                break;
                            case 0X08:
                                USART_T = "led控制返回指令\r\n";
                                break;
                            case 0X0d:
                                USART_T = "mppc增益控制返回指令\r\n";
                                break;
                            case 0X24:
                                USART_T = "模式设置返回指令\r\n";
                                break;
                            case 0x0c:
                                Read_Data_Decode();
                                break;
                            case 0x11:
                                USART_T = "读取eeprom完成\r\n";
                                //ini_ces_instantiation.RS232_Eeprom_READ(USART_RX_BUF);
                                ini_ces_instantiation.RS232_eepromREAD(USART_RX_BUF);
                                break;
                            case 0x12:
                                USART_T = "写入eeprom完成\r\n";
                                break;
                            default:

                                break;
                        }
                        if (USART_T != null)
                        {
                            InvokeToForm(() =>
                            {

                                Serial_Data.AppendText(USART_T);
                            });
                        }

                        SendByte_ok = true;
                        USART_step = 1;
                        USART_RX_STA = 0;
                        USART_RX_STA1 = false;
                    }

                }
                catch (IOTimeoutException)
                {
                    result = "读取时间超时";
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }
        }
            private void btnOpen_Click(object sender, EventArgs e)
        {
            string message, message_time;
            if(btnOpen.Text == "打开串口")
            {
                if(!RS232_Instantiation.RS232_Open_Close("OPEN", ((Pair<string, string>)cboRS232.SelectedItem).Value.ToString(), out message))
                {
                    MessageBox.Show("串口打开失败");
                    return;
                }
                Serial_Data.AppendText("打开串口232" + message + "\r\n");
                //if (RS232_Instantiation.RS232.RS232_portOperatorBase is RS232PortOperator portOperator)
                //{
                //    portOperator.DataReceived += PortOperator_DataReceived;
                //}
                RS232_read = new Thread(new ThreadStart(RS232_Read));
                RS232_read.IsBackground = true;
                RS232_read.Start();
                btnOpen.Text = "关闭串口";
                GetAppSetting.GetAppSetting_data("RS232", cboRS232.Text);
                GetAppSetting.GetAppSetting_data("RS232Enabled", btnOpen.Text);
            }
            else
            {
                if (!RS232_Instantiation.RS232_Open_Close("CLOSE", ((Pair<string, string>)cboRS232.SelectedItem).Value.ToString(), out message))
                {
                    MessageBox.Show("串口关闭失败");
                    return;
                }
                RS232_read.Abort();
                Serial_Data.AppendText("关闭rs232" + message + "\r\n");
                
                btnOpen.Text = "打开串口";
                GetAppSetting.GetAppSetting_data("RS232", cboRS232.Text);
                GetAppSetting.GetAppSetting_data("RS232Enabled", btnOpen.Text);
            }
            
        }
        /// <summary>
        ///RS232_SEND数据发送函数
        /// </summary>
        /// <param name="sender">什么按键触发的</param>
        /// <param name="SendByte_T">数据保存的地址</param>
        /// <param name="SendByte_i_T">生成的数据长度</param>
        /// <param name="threadon">true当前主线程，false非主线程，需要托管</param>
        /// <param name="return_on">true需要返回值 false 不需要返回,只有在非主线程有效</param>
        /// <param name="time_out">超时时间，只有在非主线程有效</param>
        public bool RS232_SEND(object sender,byte[] SendByte_T,  byte SendByte_i_T, bool threadon,bool return_on,int time_out)
        {
            
            string message = string.Empty;
            if (threadon)
            {
                RS232_Instantiation.RS232.RS232_WriteByte(SendByte_T, SendByte_i_T, out message);
                //Serial_Data.AppendText("写入" + message + "\r\n");
                return true;
            }
            else
            {
                SendByte_ok = false;
                InvokeToForm(() =>
                {
                    try
                    {
                        RS232_Instantiation.RS232.RS232_WriteByte(SendByte_T, SendByte_i_T, out message);
                        //Serial_Data.AppendText("写入" + message + "\r\n");
                    }
                    catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
                });
                if (return_on)
                {                  
                    while (true)
                    {
                        if (SendByte_ok == true)
                        {
                            return true;
                        }
                        Thread.Sleep(1);
                        time_out--;
                        if (time_out <= 0)
                        {
                            return false;
                        }
                    }
                }
                else
                { return true; }
            }
            
        }
        private void butHELLO_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            //Button buttonxx = (Button)sender;
            DXecllence_order.HELLO(ref SendByte, out SendByte_i, cmdno++);
            RS232_SEND(sender, SendByte, SendByte_i, true, true, 1000);
            this.Enabled = true;      
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            t_Task = Task.Factory.StartNew(() =>
            {
                //打开所有led
                DXecllence_order.LED_OPEN_ON("ALLON", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("打开所有LED\r\n");
                });
                //设置增益
                DXecllence_order.MPPC_GAIN("ALLON", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("打开MPPC高压\r\n");
                });
                //x扫描电机复位
                DXecllence_order.XSCAN_RST(ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 15000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("完成扫描电机复位\r\n");
                });
                Thread.Sleep(5000);
                
                Read_data_Channel = "ALLON";
                read_data_len = 0;
                //模式设置
                DXecllence_order.TEST_MODE(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("设置扫描电机模式为" + Read_data_Channel + "\r\n");
                });
                Stopwatch stopwatch = Stopwatch.StartNew();
                DXecllence_order.XSCAN_SCAN(27500, ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("电机扫描完成" + Read_data_Channel + "\r\n");
                    Serial_Data.AppendText("电机扫描时间:" + $" { stopwatch.ElapsedMilliseconds}ms" + "\r\n");
                });

                Thread.Sleep(1000);
                Read_data_Channel = "FAM";
                read_data_len = 0;
                DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 500);
                Read_data_Channel = "VIC";
                read_data_len = 0;
                DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 500);
                Read_data_Channel = "ROX";
                read_data_len = 0;
                DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 500);
                Read_data_Channel = "CY5";
                read_data_len = 0;
                DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 500);
                //关闭所有led
                DXecllence_order.LED_OPEN_ON("ALLOFF", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("关闭所有LED\r\n");
                });
                //设置增益
                DXecllence_order.MPPC_GAIN("ALLOFF", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("关闭MPPC高压\r\n");
                });
                InvokeToForm(() =>
                {
                    this.Enabled = true;
                });
                
            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
            
        }

        private void butREAD_DATA_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            this.Enabled = false;
            t_Task = Task.Factory.StartNew(() =>
            {
                //打开所有led
                DXecllence_order.LED_OPEN_ON("ALLON", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("打开所有LED\r\n");
                });
                //设置增益
                DXecllence_order.MPPC_GAIN("ALLON", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("打开MPPC高压\r\n");
                });
                //x扫描电机复位
                DXecllence_order.XSCAN_RST(ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 15000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("完成扫描电机复位\r\n");
                });
                
                Thread.Sleep(5000);
                Read_data_Channel = "ROX";
                read_data_len = 0;
                //模式设置
                DXecllence_order.TEST_MODE(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("设置扫描电机模式为" + Read_data_Channel + "\r\n");
                });
                DXecllence_order.XSCAN_SCAN(27500, ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("电机扫描完成" + Read_data_Channel + "\r\n");
                });
                Thread.Sleep(3000);
                for (int i = 0; i < 60; i++)
                {
                    DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 500);

                    //Thread.Sleep(100);
                }

                {
                    //x扫描电机复位
                    DXecllence_order.XSCAN_RST(ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 15000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("完成扫描电机复位\r\n");
                    });

                    Read_data_Channel = "VIC";
                    read_data_len = 0;
                    //模式设置
                    DXecllence_order.TEST_MODE(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("设置扫描电机模式为" + Read_data_Channel + "\r\n");
                    });
                    DXecllence_order.XSCAN_SCAN(27500, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("电机扫描完成" + Read_data_Channel + "\r\n");
                    });
                    Thread.Sleep(3000);
                    for (int i = 0; i < 60; i++)
                    {
                        DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                        RS232_SEND(sender, SendByte, SendByte_i, false, false, 500);
                        Thread.Sleep(100);
                    }
                }
                {
                    //x扫描电机复位
                    DXecllence_order.XSCAN_RST(ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 15000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("完成扫描电机复位\r\n");
                    });
                    Read_data_Channel = "FAM";
                    read_data_len = 0;
                    //模式设置
                    DXecllence_order.TEST_MODE(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("设置扫描电机模式为" + Read_data_Channel + "\r\n");
                    });
                    DXecllence_order.XSCAN_SCAN(27500, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("电机扫描完成" + Read_data_Channel + "\r\n");
                    });
                    Thread.Sleep(3000);
                    for (int i = 0; i < 60; i++)
                    {
                        DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                        RS232_SEND(sender, SendByte, SendByte_i, false, false, 500);

                        Thread.Sleep(100);
                    }
                }
                {
                    //x扫描电机复位
                    DXecllence_order.XSCAN_RST(ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 15000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("完成扫描电机复位\r\n");
                    });
                    Read_data_Channel = "CY5";
                    read_data_len = 0;
                    //模式设置
                    DXecllence_order.TEST_MODE(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("设置扫描电机模式为" + Read_data_Channel + "\r\n");
                    });
                    DXecllence_order.XSCAN_SCAN(27500, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("电机扫描完成" + Read_data_Channel + "\r\n");
                    });
                    Thread.Sleep(3000);
                    for (int i = 0; i < 60; i++)
                    {
                        DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                        RS232_SEND(sender, SendByte, SendByte_i, false, false, 500);

                        Thread.Sleep(100);
                    }
                }
                //关闭所有led
                DXecllence_order.LED_OPEN_ON("ALLOFF", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("关闭所有LED\r\n");
                });
                //设置增益
                DXecllence_order.MPPC_GAIN("ALLOFF", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("关闭MPPC高压\r\n");
                });
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("数据读取完成\r\n");
                    this.Enabled = true;
                });
                



            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
        }

        private void butSave_data_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            this.Enabled = false;
            string data_path1 = System.Environment.CurrentDirectory + "\\测试\\"+ textBox1.Text+"原始数据.csv";
#if PCB_V2_4
            string x = "I,VIC,CY5,FAM,ROX\r\n";//read_data_len
            for (int i = 0;i< read_data_len;i++)
            {
                x += i.ToString() + ","+ read_data[1, i].ToString()+","+ read_data[3, i].ToString() + ","+ read_data[0, i].ToString() + ","+ read_data[2, i].ToString() + ",\r\n";
            }
#else
            string x = "I,ROX,FAM,CY5,VIC\r\n";//read_data_len
            for (int i = 0;i< read_data_len;i++)
            {
                x += i.ToString() + ","+ read_data[2, i].ToString()+","+ read_data[0, i].ToString() + ","+ read_data[3, i].ToString() + ","+ read_data[1, i].ToString() + ",\r\n";
            }
#endif
            
            IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
            using (var stream = File.Open(data_path1, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                csv.Configuration.HasHeaderRecord = false;
                csv.WriteField(x, false);
            }
            MessageBox.Show("数据保存完成");

            this.Enabled = true;
        }
        List<Original_DATA_CSV> results = new List<Original_DATA_CSV>();
        public static string Read_Name_Path()
        {
            string path1 = System.Configuration.ConfigurationManager.AppSettings["Original_path_APP"];  //读取上次保存的路径
            if (Directory.Exists(path1) == false)//判断路径是否存在
            {
                path1 = System.Environment.CurrentDirectory;//获取绝对路径
                path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                 //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
            }
            OpenFileDialog file1 = new OpenFileDialog(); //定义提示用户打开文件
            file1.Filter = "CSV文件;xls文件|*.csv;*.xls";//设置文件后缀的过滤
            if (path1 != null) { file1.InitialDirectory = path1; }//设置打开文件路径
                                                                  //获取文件夹的绝对路径
            if (file1.ShowDialog() == DialogResult.OK) //如果有选择打开文件
            {
                path1 = file1.FileName;
                string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上一层目录
                GetAppSetting.GetAppSetting_data("Original_path_APP", path2);//保存本次打开路径
                return path1;
            }
            return null;
        }
        public Shart_Show show_achieve = new Shart_Show();
        int[,] read_data_t = new int[8, 6000];
        int read_data__len = 0;
        private void button2_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;         
            results.Clear();//清空缓冲数据
            string file_name = Read_Name_Path();//需要读取的文件名
            if (file_name == null) { buttonxx.Enabled = true; return; }
            using (var reader = new StreamReader(file_name, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                results = csv.GetRecords<Original_DATA_CSV>().ToList();
            }

            //var results_s = results.Where(sx => sx.I == int.Parse()
            results.Sort((x, y) =>
            {
                if (System.Convert.ToInt32(x.I) > (System.Convert.ToInt32(y.I)))
                { return 1; }
                else
                { return -1; }
            });//从小到大排序
            read_data__len = results.Count();
            foreach (var Original_DATA in results)
            {//遍历所有数据
#if PCB_V2_4
                read_data_t[0, System.Convert.ToInt32(Original_DATA.I)] = System.Convert.ToInt32(Original_DATA.VIC);
                read_data_t[2, System.Convert.ToInt32(Original_DATA.I)] = System.Convert.ToInt32(Original_DATA.CY5);
                read_data_t[4, System.Convert.ToInt32(Original_DATA.I)] = System.Convert.ToInt32(Original_DATA.FAM);
                read_data_t[6, System.Convert.ToInt32(Original_DATA.I)] = System.Convert.ToInt32(Original_DATA.ROX);
#else
                read_data_t[0, System.Convert.ToInt32(Original_DATA.I)] = System.Convert.ToInt32(Original_DATA.ROX);
                read_data_t[2, System.Convert.ToInt32(Original_DATA.I)] = System.Convert.ToInt32(Original_DATA.FAM);
                read_data_t[4, System.Convert.ToInt32(Original_DATA.I)] = System.Convert.ToInt32(Original_DATA.CY5);
                read_data_t[6, System.Convert.ToInt32(Original_DATA.I)] = System.Convert.ToInt32(Original_DATA.VIC);
#endif
            }
            int[] s = new int[4] { 13500, 1100, 11500, 11400 };
            int[] max = new int[4] { 0, 0, 0, 0 };
            int[] T = new int[4] { 0, 0, 0, 0 };
           // string[] stx = new string[4];
            for(int i =0;i< read_data__len; i++)
            {
                if(i>100)
                {
                    for(int j=0;j<4;j++)
                    {
                        int ji = 2 * j;
                        if (read_data_t[ji, i] > s[j])
                        {
                            if (read_data_t[ji, i] > max[j])
                            {//说明当前值不一定是最大值，保存max
                                max[j] = read_data_t[ji, i];
                                T[j] = i;
                            }
                        }
                        else
                        {
                            if (T[j] != 0)
                            {
                                read_data_t[ji+1, T[j]] = T[j];
                                
                            }
                            max[j] = 0;
                            T[j] = 0;
                        }
                    }
                   
                }
            }

            //保存数据
            string data_path1 = System.Environment.CurrentDirectory + "\\测试\\" + textBox1.Text + "xxx原始数据.csv";
#if PCB_V2_4
                string xt = "I,VIC,VICx,CY5,CY5x,FAM,FAMx,ROX,ROXx\r\n";//read_data_len
#else
             string xt = "I,ROX,ROXx,FAM,FAMx,CY5,CY5x,VIC,VICx\r\n";//read_data_len
#endif

            for (int i = 0; i < read_data__len; i++)
            {
                xt += i.ToString() + "," + read_data_t[0, i].ToString() + "," + read_data_t[1, i].ToString() + "," + read_data_t[2, i].ToString() + "," + read_data_t[3, i].ToString() + "," + read_data_t[4, i].ToString() + "," + read_data_t[5, i].ToString() + "," + read_data_t[6, i].ToString() + "," + read_data_t[7, i].ToString() + ",\r\n";
            }
            IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件          
            using (var stream = File.Open(data_path1, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                csv.Configuration.HasHeaderRecord = false;
                csv.WriteField(xt, false);
            }
            MessageBox.Show("数据保存完成");
            
            if(show_achieve.onSelectionRangeChanged ==null)
            {//订阅消息 Sliding_window
                show_achieve.onSelectionRangeChanged += Sliding_window;
                show_achieve.checkCount.Enabled = true;
            }
            show_achieve.chart1.Series.Clear();
            show_achieve.ADD_series(new System.Windows.Forms.DataVisualization.Charting.Series(), "Series1", "Legend1", "荧光值", Color.Blue);
            show_achieve.chart1.Series[0].Points.Clear();
            show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Position = 0;
            show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Size = double.NaN; // winnum;//视野范围内共有多少个数据点

            show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Position = 0;
            show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Size = double.NaN;
            show_achieve.chart1.ChartAreas[0].AxisX.Title = "";
            show_achieve.chart1.ChartAreas[0].AxisY.Title = "值";
            show_achieve.chart1.Series[0].LegendText = comboBox1.Text;

            int char_y_min = 999999;
            int char_y_max = 0;
            int z = 6;
#if PCB_V2_4
            switch (comboBox1.Text)
            {
                case "FAM":
                    z = 4;
                    break;
                case "VIC":
                    z = 0;
                    break;
                case "ROX":
                    z = 6;
                    break;
                case "CY5":
                    z = 2;
                    break;
                default:
                    z = 0;
                    break;
            }
#else
         switch (comboBox1.Text)
            {
                case "FAM":
                    z = 2;
                    break;
                case "VIC":
                    z = 6;
                    break;
                case "ROX":
                    z = 0;
                    break;
                case "CY5":
                    z = 4;
                    break;
                default:
                    z = 0;
                    break;
            }   
#endif

            for (int i = 0; i < read_data__len; i++)
            {          
                show_achieve.chart1.Series[0].Points.AddXY(i, read_data_t[z, i]);
                if(read_data_t[z, i]> char_y_max)
                {
                    char_y_max = read_data_t[z, i];
                }
                if(read_data_t[z, i]< char_y_min)
                {
                    char_y_min = read_data_t[z, i];
                }
            }
            show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Position = char_y_min-500 ;
            show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Size = char_y_max - char_y_min + 3000;// + 10;
            show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Position = 0;
            show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Size = read_data__len+1;//视野范围内共有多少个数据点
            show_achieve.chart1.ChartAreas[0].AxisX.Interval = 0;  //进行四舍五入 
            show_achieve.chart1.ChartAreas[0].AxisY.Interval = 0;  //进行四舍五入  
            show_achieve.Show();
            SHART.Controls.Clear();//清空当前窗口的窗口控件
            SHART.Controls.Add(show_achieve);
            show_achieve.Shart_Show_Auto(SHART.Size.Width, SHART.Size.Height);
            show_achieve.Shart_Show_Auto2(SHART.Size.Width, SHART.Size.Height);
            //这里增加图形显示
            buttonxx.Enabled = true;
        }

        private void DXecllence_Resize(object sender, EventArgs e)
        {
            show_achieve.Shart_Show_Auto(SHART.Size.Width, SHART.Size.Height);
            ini_ces_instantiation.Shart_Show_Auto(tab_INI.Size.Width, tab_INI.Size.Height);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(read_data__len>0)
            {
                show_achieve.chart1.Series.Clear();
                show_achieve.ADD_series(new System.Windows.Forms.DataVisualization.Charting.Series(), "Series1", "Legend1", "荧光值", Color.Blue);
                show_achieve.chart1.Series[0].Points.Clear();
                show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Position = 0;
                show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Size = double.NaN; // winnum;//视野范围内共有多少个数据点

                show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Position = 0;
                show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Size = double.NaN;
                show_achieve.chart1.ChartAreas[0].AxisX.Title = "";
                show_achieve.chart1.ChartAreas[0].AxisY.Title = "值";
                show_achieve.chart1.Series[0].LegendText = comboBox1.Text;
                int char_y_min = 999999;
                int char_y_max = 0;
                int z;
#if PCB_V2_4
                switch (comboBox1.Text)
                {
                    case "FAM":
                        z = 4;
                        break;
                    case "VIC":
                        z = 0;
                        break;
                    case "ROX":
                        z = 6;
                        break;
                    case "CY5":
                        z = 2;
                        break;
                    default:
                        z = 0;
                        break;
                }
#else
         switch (comboBox1.Text)
            {
                case "FAM":
                    z = 2;
                    break;
                case "VIC":
                    z = 6;
                    break;
                case "ROX":
                    z = 0;
                    break;
                case "CY5":
                    z = 4;
                    break;
                default:
                    z = 0;
                    break;
            }   
#endif
                for (int i = 0; i < read_data__len; i++)
                {
                    show_achieve.chart1.Series[0].Points.AddXY(i, read_data_t[z, i]);
                    if (read_data_t[z, i] > char_y_max)
                    {
                        char_y_max = read_data_t[z, i];
                    }
                    if (read_data_t[z, i] < char_y_min)
                    {
                        char_y_min = read_data_t[z, i];
                    }
                }
                show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Position = char_y_min - 500;
                show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Size = char_y_max - char_y_min + 3000;// + 10;
                show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Position = 0;
                show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Size = read_data__len + 1;//视野范围内共有多少个数据点
                show_achieve.chart1.ChartAreas[0].AxisX.Interval = 0;  //进行四舍五入 
                show_achieve.chart1.ChartAreas[0].AxisY.Interval = 0;  //进行四舍五入  
                show_achieve.Show();
                SHART.Controls.Clear();//清空当前窗口的窗口控件
                SHART.Controls.Add(show_achieve);
                show_achieve.Shart_Show_Auto(SHART.Size.Width, SHART.Size.Height);
                show_achieve.Shart_Show_Auto2(SHART.Size.Width, SHART.Size.Height);
            }
        }

        private void butRedo_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            
            string[] channel = new string[4] { "FAM", "VIC", "ROX", "CY5" };
            List<RawPcrScanData> results = new List<RawPcrScanData>();
            string results_path = System.Environment.CurrentDirectory + "\\测试";
            //生成数据保存的文件夹
            {
                System.DateTime currentTime = new System.DateTime();
                currentTime = System.DateTime.Now;
                
                //创建根目录文件
                if (Directory.Exists(results_path) == false)//根目录不存在责创建
                {
                    Directory.CreateDirectory(results_path);
                }
                results_path += "\\RawData";
                if (Directory.Exists(results_path) == false)//根目录不存在责创建
                {
                    Directory.CreateDirectory(results_path);
                }
                results_path += "\\" + currentTime.ToString("yyyyMMddHHmmss");
                if (Directory.Exists(results_path) == false)//根目录不存在责创建
                {
                    Directory.CreateDirectory(results_path);
                }
            }
            string data_patht = System.Environment.CurrentDirectory + "\\测试\\" + textBox1.Text + "重复数据.csv";
            //data_patht = System.Environment.CurrentDirectory + "\\测试0\\测试1\\测试2\\测试3\\" + textBox1.Text + "重复数据.csv";
            //IO_Operate.File_creation(ref data_patht,true);
            int i_number = System.Convert.ToInt32(textNumber.Text);
            string[] t_string = new string[4] { "FAM,","VIC,","ROX,","CY5,"};
            for(int i = 0;i<48;i++)
            {
                t_string[0] += "通道" + i.ToString() + ",";
                t_string[1] += "通道" + i.ToString() + ",";
                t_string[2] += "通道" + i.ToString() + ",";
                t_string[3] += "通道" + i.ToString() + ",";
            }
            t_string[0] += "\r\n";
            t_string[1] += "\r\n";
            t_string[2] += "\r\n";
            t_string[3] += "\r\n";
            t_Task = Task.Factory.StartNew(() =>
            {
                //设置增益
                DXecllence_order.MPPC_GAIN("ALLON", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("打开MPPC高压\r\n");
                });              
                Thread.Sleep(11000);
                //打开所有led
                DXecllence_order.LED_OPEN_ON("ALLON", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("打开所有LED\r\n");
                });
                Thread.Sleep(4000);
                for (int i = 0; i < i_number; i++)
                {
                    //x扫描电机复位
                    DXecllence_order.XSCAN_RST(ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 15000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("完成扫描电机复位\r\n");
                    });
                    Read_data_Channel = "ALLON";
                    read_data_len = 0;
                    //模式设置
                    DXecllence_order.TEST_MODE(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("设置扫描电机模式为" + Read_data_Channel + "\r\n");
                    });
                    DXecllence_order.XSCAN_SCAN(27500, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("电机扫描完成" + Read_data_Channel + "\r\n");
                    });
                    //关闭所有led
                    DXecllence_order.LED_OPEN_ON("ALLOFF", ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("打开所有LED\r\n");
                    });
                    Thread.Sleep(100);
                    //打开所有led
                    DXecllence_order.LED_OPEN_ON("ALLON", ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    InvokeToForm(() =>
                    {
                        Serial_Data.AppendText("打开所有LED\r\n");
                    });
                    Thread.Sleep(2000);
                    Read_data_Channel = "FAM";
                    read_data_len = 0;
                    DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 500);
                    Read_data_Channel = "VIC";
                    read_data_len = 0;
                    DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 500);
                    Read_data_Channel = "ROX";
                    read_data_len = 0;
                    DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 500);
                    Read_data_Channel = "CY5";
                    read_data_len = 0;
                    DXecllence_order.READ_DATA(Read_data_Channel, ref SendByte, out SendByte_i, cmdno++);
                    RS232_SEND(sender, SendByte, SendByte_i, false, true, 500);
                    t_string[0] += i.ToString() + ",";
                    t_string[1] += i.ToString() + ",";
                    t_string[2] += i.ToString() + ",";
                    t_string[3] += i.ToString() + ",";
                    for (int j = 0; j < 48; j++)
                    {
                        
                        //t_string[1] += read_data[1, j].ToString() + ",";
                        //t_string[2] += read_data[2, j].ToString() + ",";
                        //t_string[3] += read_data[3, j].ToString() + ",";
                        
                        for(int ji = 0;ji<4;ji++)
                        {
                            t_string[ji] += read_data[ji, j].ToString() + ",";
                            //number.ToString().PadLeft(5, '0');
                            //数据加入results中  List<RawPcrScanData> results = new List<RawPcrScanData>();
                            int jt = 47 - j;
                            RawPcrScanData one_data = new RawPcrScanData();
                            one_data.AssayName = "ThermalInspection.scanning@25c";
                            one_data.AssayVersion = "null";
                            one_data.SampleBarcode = "lane"+(jt/4).ToString().PadLeft(2,'0');
                            one_data.TestOrderId = 0;
                            one_data.StageName = "StageCycle";
                            one_data.StepNumber = 2;
                            one_data.IterationNumber = i;
                            one_data.PcrTubeLocation = "TC-" + (jt / 4).ToString() + "-"+(jt % 4).ToString();
                            one_data.FluorescenceData = read_data[ji, j];
                            one_data.FluorescenceChannel = channel[ji];
                            one_data.TargetTemperature = 25;
                            one_data.MeasuredTemperature = 0;
                            results.Add(one_data);
                        }
                    }
                    t_string[0] += "\r\n";
                    t_string[1] += "\r\n";
                    t_string[2] += "\r\n";
                    t_string[3] += "\r\n";
                    Thread.Sleep(1000);
                }
                t_string[0] += "\r\n";
                t_string[1] += "\r\n";
                t_string[2] += "\r\n";
                t_string[3] += "\r\n";
                t_string[0] += "\r\n";
                t_string[1] += "\r\n";
                t_string[2] += "\r\n";
                t_string[3] += "\r\n";
                string data_path1 = System.Environment.CurrentDirectory + "\\测试\\" + textBox1.Text + "重复数据.csv";
                IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
                using (var stream = File.Open(data_path1, FileMode.Append))
                using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                {
                    // Don't write the header again. csv.WriteComment(writer);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteField(t_string[0]+ t_string[1]+ t_string[2]+ t_string[3], false);
                }
                //写入RawData results_path
                {//原始数据进行排序  pathx = pathx.Substring(pathx.LastIndexOf("\\") + 1, (pathx.Length - pathx.LastIndexOf("\\") - 1));
                    results.Sort((x, y) => {
                        int xch = 0, ych = 0;
                        switch (x.FluorescenceChannel)
                        {
                            case "FAM":
                                xch = 0;
                                break;
                            case "VIC":
                                xch = 1;
                                break;
                            case "ROX":
                                xch = 2;
                                break;
                            case "CY5":
                                xch = 3;
                                break;
                            default:
                                xch = 0;
                                break;
                        }
                        switch (y.FluorescenceChannel)
                        {
                            case "FAM":
                                ych = 0;
                                break;
                            case "VIC":
                                ych = 1;
                                break;
                            case "ROX":
                                ych = 2;
                                break;
                            case "CY5":
                                ych = 3;
                                break;
                            default:
                                ych = 0;
                                break;
                        }
                        int x_tuble, y_tuble;
                        string ss = x.PcrTubeLocation.Substring(x.PcrTubeLocation.IndexOf("-") + 1, x.PcrTubeLocation.LastIndexOf("-") - x.PcrTubeLocation.IndexOf("-") - 1);
                        string sss = x.PcrTubeLocation.Substring(x.PcrTubeLocation.LastIndexOf("-") + 1, x.PcrTubeLocation.Length - x.PcrTubeLocation.LastIndexOf("-") - 1);
                        x_tuble = System.Convert.ToInt32(ss) * 4 + System.Convert.ToInt32(sss);

                        ss = y.PcrTubeLocation.Substring(y.PcrTubeLocation.IndexOf("-") + 1, y.PcrTubeLocation.LastIndexOf("-") - y.PcrTubeLocation.IndexOf("-") - 1);
                        sss = y.PcrTubeLocation.Substring(y.PcrTubeLocation.LastIndexOf("-") + 1, y.PcrTubeLocation.Length - y.PcrTubeLocation.LastIndexOf("-") - 1);
                        y_tuble = System.Convert.ToInt32(ss) * 4 + System.Convert.ToInt32(sss);
                        if(xch> ych)
                        {
                            return 1;
                        }
                        else if (xch == ych)
                        {
                            if (x_tuble > y_tuble)
                            {
                                return 1;
                            }
                            else if (x_tuble == y_tuble)
                            {
                                if(x.IterationNumber>y.IterationNumber)
                                {
                                    return 1;
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            return -1;
                        }



                    });

                }
                results_path += "\\RawData_Lane.csv";
                IO_Operate.File_creation(ref results_path, true);//判定文件路径并根据选择判定是否重新构建文件
                var reader_w = new StreamWriter(results_path);
                var csv_w = new CsvWriter(reader_w, CultureInfo.InvariantCulture);
                csv_w.Configuration.RegisterClassMap<RawPcrScanDataMap>();
                csv_w.WriteRecords(results);
                csv_w.Flush();
                csv_w.Dispose();
                //写入RawData完成
                //关闭所有led
                DXecllence_order.LED_OPEN_ON("ALLOFF", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("关闭所有LED\r\n");
                });
                //设置增益
                DXecllence_order.MPPC_GAIN("ALLOFF", ref SendByte, out SendByte_i, cmdno++);
                RS232_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("关闭MPPC高压\r\n");
                });
                InvokeToForm(() =>
                {
                    this.Enabled = true;
                });
                           
                
            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
            
        }

        private void MPPC_SET_SelectedIndexChanged(object sender, EventArgs e)
        {
            show_achieve.Shart_Show_Auto(SHART.Size.Width, SHART.Size.Height);
            ini_ces_instantiation.Shart_Show_Auto(tab_INI.Size.Width, tab_INI.Size.Height);
        }
        public void Sliding_window(object sender, CursorEventArgs e)
        {
            int data_max = 0;
            int data_max_index = 0;
            int data_min = 99999;
            int data_min_index = 0;
            int z;
#if PCB_V2_4
            switch (comboBox1.Text)
            {
                case "FAM":
                    z = 4;
                    break;
                case "VIC":
                    z = 0;
                    break;
                case "ROX":
                    z = 6;
                    break;
                case "CY5":
                    z = 2;
                    break;
                default:
                    z = 0;
                    break;
            }
#else
            switch (comboBox1.Text)
            {
                case "FAM":
                    z = 2;
                    break;
                case "VIC":
                    z = 6;
                    break;
                case "ROX":
                    z = 0;
                    break;
                case "CY5":
                    z = 4;
                    break;
                default:
                    z = 0;
                    break;
            }
#endif

            //计算信号的1/2位置值
            if (show_achieve.x_start>100 && show_achieve.x_end<5000 && show_achieve.chart1.Series.Count <= 1)
            {
                //查找最大值最小值，以及对应位置
                for(int i = (int)show_achieve.x_start;i<(int)show_achieve.x_end;i++)
                {
                    if (read_data_t[z, i] > data_max)
                    {
                        data_max = read_data_t[z, i];
                        data_max_index = i;
                    }
                    if (read_data_t[z, i] < data_min)
                    {
                        data_min = read_data_t[z, i];
                        data_min_index = i;
                    }
                }
                //计算1/2最大最小
                int mid_value = (data_max + data_min) / 2;
                int mid_left = 0;
                int mid_left_index = 0;
                int mid_right = 0;
                int mid_right_index = 0;
                //寻找左边中值对应的x
                for (int i = data_max_index;i> show_achieve.x_start;i--)
                {
                    if (read_data_t[z, i] < mid_value)
                    {//第一个小于中值的值
                        if(mid_value- read_data_t[z, i] > read_data_t[z, i+1]- mid_value)
                        {
                            mid_left = read_data_t[z, i + 1];
                            mid_left_index = i + 1;
                        }
                        else
                        {
                            mid_left = read_data_t[z, i ];
                            mid_left_index = i;
                        }
                        break;
                    }
                }
                //寻找右边中值对应的x
                for (int i = data_max_index; i < show_achieve.x_end; i++)
                {
                    if (read_data_t[z, i] < mid_value)
                    {//第一个小于中值的值
                        if (mid_value - read_data_t[z, i] > read_data_t[z, i - 1] - mid_value)
                        {
                            mid_right = read_data_t[z, i - 1];
                            mid_right_index = i - 1;
                        }
                        else
                        {
                            mid_right = read_data_t[z, i];
                            mid_right_index = i;
                        }
                        break;
                    }
                }
                show_achieve.ADD_series(new System.Windows.Forms.DataVisualization.Charting.Series(), "Series2", "Legend1", "荧光值", Color.Blue);
                
                show_achieve.chart1.Series[1].Points.AddXY(mid_left_index, mid_left);
                show_achieve.chart1.Series[1].Points.AddXY(data_max_index, data_max);
                show_achieve.chart1.Series[1].Points.AddXY(mid_right_index, mid_right);
                int zhongxin = (mid_right_index + mid_left_index) / 2;
                int ave = 0;
                for (int  i = zhongxin-5;i< zhongxin+7;i++)// 7
                {
                    ave += read_data_t[z, i];
                }
                ave = ave / 12;
                InvokeToForm(() =>
                {//$"[Time:{stopwatch.ElapsedMilliseconds}ms] Read:  {result}"
                    Serial_Data.AppendText($"最大值:({data_max_index},{data_max}),中值:{mid_value},最小值:({data_min_index},{data_min}),左1/2:({mid_left_index},{mid_left}),右1/2:({mid_right_index},{mid_right}),1/2宽度:{mid_right_index- mid_left_index},中心位置:{(mid_right_index + mid_left_index)/2};中心6点平均:{ave}\r\n");
                });
                string data_path1 = System.Environment.CurrentDirectory + "\\测试\\中心点位.csv";
                IO_Operate.File_creation(ref data_path1, false);//判定文件路径并根据选择判定是否重新构建文件
                using (var stream = File.Open(data_path1, FileMode.Append))
                using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                {
                    // Don't write the header again. csv.WriteComment(writer);
                    csv.Configuration.HasHeaderRecord = false;
                   csv.WriteField(zhongxin.ToString()+"\r\n", false);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            if (show_achieve.chart1.Series.Count == 2)
            {
                show_achieve.chart1.Series.Remove(show_achieve.chart1.Series[1]);
            }
            show_achieve.ADD_series(new System.Windows.Forms.DataVisualization.Charting.Series(), "Series2", "Legend1", "荧光值", Color.Blue);

            int z,z_t;
            
            int[,] data_Center = new int[4, 200];
            int data_Center_i = 0;
            GetAppSetting.GetAppSetting_data("FAM_Threshold", text_centre_FAM.Text);
            GetAppSetting.GetAppSetting_data("VIC_Threshold", text_centre_VIC.Text);
            GetAppSetting.GetAppSetting_data("ROX_Threshold", text_centre_ROX.Text);
            GetAppSetting.GetAppSetting_data("CY5_Threshold", text_centre_CY5.Text);

#if PCB_V2_4
            int[] data_vpt = new int[4] { 12000, 4500, 4400, 4500 }; //分别是VIC、CY5、FAM、ROX通道的孔位校准阈值
            {
                data_vpt[3] = int.Parse(text_centre_ROX.Text);
                data_vpt[2] = int.Parse(text_centre_FAM.Text);
                data_vpt[1] = int.Parse(text_centre_CY5.Text);
                data_vpt[0] = int.Parse(text_centre_VIC.Text);
            }
            string data_Center_s = "序号,VIC,CY5,FAM,ROX\r\n";
            switch (comboBox1.Text)
            {
                case "FAM":
                    z_t = 4;
                    break;
                case "VIC":
                    z_t = 0;
                    break;
                case "ROX":
                    z_t = 6;
                    break;
                case "CY5":
                    z_t = 2;
                    break;
                default:
                    z_t = 0;
                    break;
            }
#else
int[] data_vpt = new int[4] { 12000, 4500, 4400, 4500 }; //分别是ROX、FAM、CY5、VIC通道的孔位校准阈值
            {
                data_vpt[0] = int.Parse(text_centre_ROX.Text);
                data_vpt[1] = int.Parse(text_centre_FAM.Text);
                data_vpt[2] = int.Parse(text_centre_CY5.Text);
                data_vpt[3] = int.Parse(text_centre_VIC.Text);
            }
         string data_Center_s = "序号,ROX,FAM,CY5,VIC\r\n";
         switch (comboBox1.Text)
            {
                case "FAM":
                    z_t = 2;
                    break;
                case "VIC":
                    z_t = 6;
                    break;
                case "ROX":
                    z_t = 0;
                    break;
                case "CY5":
                    z_t = 4;
                    break;
                default:
                    z_t = 0;
                    break;
            }  
#endif

            for (z = 0;z<7;z=z+2)
            {
                data_Center_i = 0;

                for (int data_i = 250; data_i < 4700; data_i++)
                {
                    //差值局部最大值
                    if (read_data_t[z, data_i] > data_vpt[z / 2])
                    {//出现一次差值区域
                        int data_max = read_data_t[z, data_i - 15];
                        int data_max_index = data_i - 15;
                        int data_min = read_data_t[z, data_i - 15];
                        int data_min_index = data_i - 15;
                        //查找区域最大最小
                        for (int i = data_i - 15; i < data_i + 40; i++)
                        {
                            if (read_data_t[z, i] > data_max)
                            {
                                data_max = read_data_t[z, i];
                                data_max_index = i;
                            }
                            if (read_data_t[z, i] < data_min)
                            {
                                data_min = read_data_t[z, i];
                                data_min_index = i;
                            }
                        }
                        //计算1/2最大最小
                        int mid_value = (data_max + data_min) / 2;
                        int mid_left = 0;
                        int mid_left_index = 0;
                        int mid_right = 0;
                        int mid_right_index = 0;
                        //寻找左边中值对应的x
                        for (int i = data_max_index; i > data_i - 15; i--)
                        {
                            if (read_data_t[z, i] < mid_value)
                            {//第一个小于中值的值
                                if (mid_value - read_data_t[z, i] > read_data_t[z, i + 1] - mid_value)
                                {
                                    mid_left = read_data_t[z, i + 1];
                                    mid_left_index = i + 1;
                                }
                                else
                                {
                                    mid_left = read_data_t[z, i];
                                    mid_left_index = i;
                                }
                                break;
                            }
                        }
                        //寻找右边中值对应的x
                        for (int i = data_max_index; i < data_i + 40; i++)
                        {
                            if (read_data_t[z, i] < mid_value)
                            {//第一个小于中值的值
                                if (mid_value - read_data_t[z, i] > read_data_t[z, i - 1] - mid_value)
                                {
                                    mid_right = read_data_t[z, i - 1];
                                    mid_right_index = i - 1;
                                }
                                else
                                {
                                    mid_right = read_data_t[z, i];
                                    mid_right_index = i;
                                }
                                break;
                            }
                        }
                        int zhongxin = (mid_right_index + mid_left_index) / 2;
                        if (z == z_t)
                        {
                            show_achieve.chart1.Series[1].Points.AddXY(mid_left_index, mid_left);
                            show_achieve.chart1.Series[1].Points.AddXY(data_max_index, data_max);
                            show_achieve.chart1.Series[1].Points.AddXY(zhongxin, 0);
                            show_achieve.chart1.Series[1].Points.AddXY(zhongxin, data_max);
                            show_achieve.chart1.Series[1].Points.AddXY(mid_right_index, mid_right);
                        }
                        data_i = data_i + 40;
                        data_Center[z / 2, data_Center_i++] = zhongxin;
                    }
                }
            }

            //保存中心位置 data_Center_s  $"LED输出电流:{message},LED读取的光强{message1}\r\n"
            for (int i = 0;i<48;i++)
            {
                data_Center_s += $"{i},{data_Center[0, i]},{data_Center[1, i]},{data_Center[2, i]},{data_Center[3, i]}\r\n";
            }
            string data_path1 = System.Environment.CurrentDirectory + "\\测试\\中心点位.csv";
            IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
            using (var stream = File.Open(data_path1, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                csv.Configuration.HasHeaderRecord = false;
                csv.WriteField(data_Center_s, false);
            }
            this.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Serial_Data.Text = "";
        }
    }
}

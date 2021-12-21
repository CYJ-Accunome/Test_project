#define PCB_MPPC

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
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

using Accord;
using Accord.Math;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Math.Optimization.Losses;
//定义
//按键Button buttonxx = (Button)sender; 
//类型转换System.Convert.ToInt32(y.PcrTubeLocation)

namespace Test_project
{
    public partial class Form1 : Form
    {
        USBPort_Work PM100x_ce = new USBPort_Work();
        string[] PM100x_ce_WAV = new string[8] { "470.00", "520.00", "530.00", "565", "570.00", "615.00", "630.00", "690.00" };
        Stack PM100D_St = new Stack();
        Thread PM100D_read;
        // delegate void PM100DUICallback();
        Thread PM100DUI;
        Thread Ageing_Test;//老化测试
        string[] Fluorescent_Head_TEMP = new string[8];
        bool Fluorescent_Head_TEMP_Flag = false;
        public MPPC_Order MPPC_Order1 = new MPPC_Order();
        RS232_Work RS232 = new RS232_Work();
        Shart_Show PM100D_show_achieve = new Shart_Show();
        DXecllence DXecllence_1 = new DXecllence();
        Shart_Show DM6500_show_achieve = new Shart_Show();
        Stack DM6500_St = new Stack();//新建一个队列
        Thread DM6500_read;
        // delegate void PM100DUICallback();
        Thread DM6500UI;
        curve curve1 = new curve();
        string data_path;
        int curver_en = 0;//界面是否正在显示
        int show_i = 0;
        int curve_x = 0;
        int winnum = 500;
        int shuju_w = 0;
        int shuju_r = 0;//这个是当前读取值
        public int i = 0;
        double shuju_max = 0;
        double shuju_min = 4000000;
        float[,] shuju_VH = new float[4, 5000];
        float[,] shuju_Temp = new float[4, 5000];
        float[,] shuju_VH_target = new float[4, 5000];
        float[,] shuju_Temp_0 = new float[8, 5000];
        float[,] shuju_Temp_1 = new float[8, 5000];
        float[,] shuju_Temp_2 = new float[8, 5000];
        Thread tXfers1;
        delegate void UpdateUICallback();//三个参数分别为发送数据，长度以及描述符
        UpdateUICallback updateUI;
        int StatusUpdate_start = 0;  //表示UI进程是否正在运行
        TextBox[] TextBox_CE = new TextBox[8];
        TextBox[,] TextBox_Gather = new TextBox[4, 12];
        TextBox[,] TextBox_LED = new TextBox[4, 7];
        TextBox[] textBoxe_DAC_SET = new TextBox[8];
        List<iniData> results = new List<iniData>();
        List<MppcSetGetData> Mppc_results = new List<MppcSetGetData>();
        static string ini_name = "IDx-xxx.ini";
        string[] ADC_CH = new string[8] { "LED_TEMP_VIC",  //0x00
                                          "LED_TEMP_CY5",
                                          "MPPC_TEMP_VIC",
                                          "MPPC_TEMP_ROX",
                                          "LED_TEMP_ROX",
                                          "LED_TEMP_FAM",
                                          "MPPC_TEMP_FAM",
                                          "MPPC_TEMP_CY5",
                                               };
        int[] ADC_num = new int[8] { 5, 7, 1, 2, 6, 4, 0, 3 };
        int[] ADC_DATA = new int[8];//通道分别是M_F/M_V/M_R/M_C/L_F/L_V/L_R/L_C
        string[] FluorescenceChannel = new string[4] { "FAM",  //0x00
                                          "VIC",
                                          "ROX",
                                          "CY5",

                                               };
        // 定义下拉列表框
        private ComboBox cmb_Temp = new ComboBox();

        public Form1()
        {
            InitializeComponent();
            tabPage1.Parent = null;
            tabPage2.Parent = null;//隐藏选项卡
            int tab = System.Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["tab"]);
            tabControl1.SelectedIndex = tab;
            combobox_show_i.SelectedIndex = 0;
            Temp_show_i.SelectedIndex = 0;
            dataGridView1.ColumnCount = 6;//设置列数6
            dataGridView1.ColumnHeadersVisible = true;//显示列标题
            // 设置DataGridView控件标题列的样式
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
            columnHeaderStyle.BackColor = Color.Beige;//设置标题背景颜色
            columnHeaderStyle.Font = new Font("Verdana", 10, FontStyle.Bold);//设置标题字体大小即样式
            dataGridView1.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
            //设置DataGridView控件的标题列名
            dataGridView1.Columns[0].Name = "参数名称";
            dataGridView1.Columns[1].Name = "地址";
            dataGridView1.Columns[2].Name = "内容";
            dataGridView1.Columns[3].Name = "长度";
            dataGridView1.Columns[4].Name = "数据类型";
            dataGridView1.Columns[5].Name = "预留";


            //X下面为增加的串口
            GetPort();  //获取串口号
            string s = System.Configuration.ConfigurationManager.AppSettings["com"];
            int num = Serial_Port.Items.Count; //获取下拉框数量
            int i;
            for (i = 0; i < num; i++)
            {
                if (s == (Serial_Port.Items[i].ToString()))
                {
                    break;
                }
            }
            if (i >= num)  //说明串口号已经消失
            {
                if (num > 0)
                    Serial_Port.SelectedIndex = 0;//获取或设置指定当前选定项的索引。
            }
            else
            {
                Serial_Port.SelectedIndex = i;//获取或设置指定当前选定项的索引。
            }
            s = System.Configuration.ConfigurationManager.AppSettings["comEnabled"];
            Serial_Open.Text = s;
            if (s == "关闭串口")//关闭串口其实就是需要打开串口
            {
                try
                {
                    serialPort1.Close(); //关闭串口
                    serialPort1.PortName = Convert.ToString(Serial_Port.Text);  //获取或设置 ComboBox 中当前选定的项。
                    serialPort1.BaudRate = 115200;
                    serialPort1.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误：" + ex.Message);
                }
                /*****************************************/
                if (serialPort1.IsOpen)//串口打开成功
                {
                    Serial_Open.Text = "关闭串口";
                    tsStatus.Text = "串口打开";
                    tsStatus.Update();

                    GetAppSetting.GetAppSetting_data("com", Serial_Port.Text);
                }
                else //串口打开失败
                {
                    Serial_Open.Text = "打开串口";
                    tsStatus.Text = "串口打开失败";
                    tsStatus.Update();
                    //string s2 = comboBox1.Text;//获取窗口显示文本。
                    GetAppSetting.GetAppSetting_data("com", Serial_Port.Text);
                }
            }
            updateUI = new UpdateUICallback(StatusUpdate);
            tXfers1 = new Thread(new ThreadStart(TransfersThread1));
            tXfers1.IsBackground = true;
            tXfers1.Priority = ThreadPriority.Highest;
            //Starts the new thread启动线程
            tXfers1.Start();
            //tabControl1.SelectedIndex = 2;//设置显示第几个框
            //tabControl1.TabPages.Remove(tabPage1);//代码删除第一个选项卡 即不显示第一个选项卡
            //tabControl1.TabPages.Remove(tabPage3);//代码删除第一个选项卡 即不显示第一个选项卡
            //MPPC_SET.TabPages.Remove(ROX);
            //MPPC_SET.TabPages.Remove(VIC);
            //MPPC_SET.TabPages.Remove(CY5);
            //初始化显示窗口
            {
                int TextBox_Gather_i = 0;
                int TextBox_Gather_j = 0;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_Vbr;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_Vov;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_Tvop;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_Temp_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_Temp_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_Temp_B;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_DAC_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_DAC_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_DAC_B;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_ADC_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_ADC_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = FAM_ADC_B;
                TextBox_Gather_i++; TextBox_Gather_j = 0;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_Vbr;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_Vov;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_Tvop;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_Temp_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_Temp_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_Temp_B;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_DAC_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_DAC_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_DAC_B;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_ADC_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_ADC_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = VIC_ADC_B;
                TextBox_Gather_i++; TextBox_Gather_j = 0;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_Vbr;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_Vov;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_Tvop;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_Temp_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_Temp_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_Temp_B;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_DAC_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_DAC_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_DAC_B;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_ADC_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_ADC_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = ROX_ADC_B;
                TextBox_Gather_i++; TextBox_Gather_j = 0;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_Vbr;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_Vov;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_Tvop;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_Temp_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_Temp_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_Temp_B;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_DAC_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_DAC_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_DAC_B;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_ADC_K2;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_ADC_K1;
                TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++] = CY5_ADC_B;
                TextBox_Gather_i = 0; TextBox_Gather_j = 0;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = FAM_LED_Vi;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = FAM_LED_K2;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = FAM_LED_K1;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = FAM_LED_B;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = FAM_I_K2;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = FAM_I_K1;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = FAM_I_B;
                TextBox_Gather_i++; TextBox_Gather_j = 0;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = VIC_LED_Vi;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = VIC_LED_K2;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = VIC_LED_K1;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = VIC_LED_B;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = VIC_I_K2;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = VIC_I_K1;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = VIC_I_B;
                TextBox_Gather_i++; TextBox_Gather_j = 0;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = ROX_LED_Vi;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = ROX_LED_K2;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = ROX_LED_K1;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = ROX_LED_B;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = ROX_I_K2;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = ROX_I_K1;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = ROX_I_B;
                TextBox_Gather_i++; TextBox_Gather_j = 0;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = CY5_LED_Vi;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = CY5_LED_K2;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = CY5_LED_K1;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = CY5_LED_B;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = CY5_I_K2;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = CY5_I_K1;
                TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++] = CY5_I_B;
                TextBox_Gather_i = 0;
                textBoxe_DAC_SET[TextBox_Gather_i] = MPPC_DAC_FAM;
                TextBox_Gather_i++;
                textBoxe_DAC_SET[TextBox_Gather_i] = MPPC_DAC_VIC;
                TextBox_Gather_i++;
                textBoxe_DAC_SET[TextBox_Gather_i] = MPPC_DAC_ROX;
                TextBox_Gather_i++;
                textBoxe_DAC_SET[TextBox_Gather_i] = MPPC_DAC_CY5;
                TextBox_Gather_i++;
                textBoxe_DAC_SET[TextBox_Gather_i] = LED_DAC_FAM;
                TextBox_Gather_i++;
                textBoxe_DAC_SET[TextBox_Gather_i] = LED_DAC_VIC;
                TextBox_Gather_i++;
                textBoxe_DAC_SET[TextBox_Gather_i] = LED_DAC_ROX;
                TextBox_Gather_i++;
                textBoxe_DAC_SET[TextBox_Gather_i] = LED_DAC_CY5;
                TextBox_Gather_i++;
                TextBox_Gather_i = 0;
                TextBox_CE[TextBox_Gather_i] = textBox_CE1;
                TextBox_Gather_i++;
                TextBox_CE[TextBox_Gather_i] = textBox_CE2;
                TextBox_Gather_i++;
                TextBox_CE[TextBox_Gather_i] = textBox_CE3;
                TextBox_Gather_i++;
                TextBox_CE[TextBox_Gather_i] = textBox_CE4;
                TextBox_Gather_i++;
                TextBox_CE[TextBox_Gather_i] = textBox_CE5;
                TextBox_Gather_i++;
                TextBox_CE[TextBox_Gather_i] = textBox_CE6;
                TextBox_Gather_i++;
                TextBox_CE[TextBox_Gather_i] = textBox_CE7;
                TextBox_Gather_i++;
                TextBox_CE[TextBox_Gather_i] = textBox_CE8;
                TextBox_Gather_i++;
                {//数据初始化
                    string path1 = System.Environment.CurrentDirectory;//获取绝对路径
                                                                       //path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                                       //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
                    path1 += "\\" + "MPPC_GET_SET_DATA.csv";
                    if (!File.Exists(path1))
                    {//如果指定文件不存在，责重新创建文件
                        File.Create(path1).Close();//创建文件                       
                        for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
                        {
                            MppcSetGetData mppc_data = new MppcSetGetData();
                            mppc_data.Number = int.Parse(Number.Text);
                            mppc_data.LightHeaadNumber = int.Parse(lightHeaadNumber.Text);
                            mppc_data.PCBNumber = int.Parse(PCBNumber.Text);
                            mppc_data.FluorescenceChannel = FluorescenceChannel[TextBox_Gather_i];
                            TextBox_Gather_j = 0;
                            mppc_data.Vbr = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.Vov = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.Tvop = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.Temp_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.Temp_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.Temp_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.DAC_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.DAC_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.DAC_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.ADC_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.ADC_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.ADC_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            TextBox_Gather_j = 0;
                            mppc_data.LED_Vi = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.LED_K2 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.LED_K1 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.LED_B = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.I_K2 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.I_K1 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            mppc_data.I_B = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                            Mppc_results.Add(mppc_data);
                        }
                        using (var stream = File.Open(path1, FileMode.Append))
                        using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                        using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                        {
                            // Don't write the header again. csv.WriteComment(writer);
                            //csv.Configuration.HasHeaderRecord = false;
                            csv.Configuration.RegisterClassMap<MppcSetGetDataMap>();
                            csv.WriteRecords(Mppc_results);
                            csv.Flush();
                            csv.Dispose();
                        }
                    }
                    //else
                    {//配置文件存在则读取最新的一个数据
                        Mppc_results.Clear();
                        using (var reader = new StreamReader(path1, Encoding.GetEncoding("GB2312")))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            Mppc_results = csv.GetRecords<MppcSetGetData>().ToList();
                        }
                        //将数据进行排序找到最大的一个
                        Mppc_results.Sort((x, y) =>
                        {
                            if (System.Convert.ToInt32(x.Number) > (System.Convert.ToInt32(y.Number)))
                            { return 1; }
                            else
                            { return -1; }
                        });//按照顺序排列配置文件  按照写入次数从小到大
                        //var mppc_list = Mppc_results.Select(sx => sx.LightHeaadNumber).Distinct<string>().ToList(); 
                        var mppc_list = Mppc_results.Select(t => t.LightHeaadNumber).Distinct().ToList();
                        mppc_list.Sort((x, y) =>
                        {
                            if (x > y)
                            { return -1; }
                            else
                            { return 1; }
                        });
                        for (TextBox_Gather_i = 0; TextBox_Gather_i < mppc_list.Count && TextBox_Gather_i < 20; TextBox_Gather_i++)
                        {
                            lightHeaadNumber.Items.Add(mppc_list[TextBox_Gather_i].ToString().PadLeft(5, '0'));
                        }
                        //lightHeaadNumber.SelectedIndex = 0;
                        mppc_list = Mppc_results.Select(t => t.PCBNumber).Distinct().ToList();
                        mppc_list.Sort((x, y) =>
                        {
                            if (x > y)
                            { return -1; }
                            else
                            { return 1; }
                        });
                        for (TextBox_Gather_i = 0; TextBox_Gather_i < mppc_list.Count && TextBox_Gather_i < 20; TextBox_Gather_i++)
                        {
                            PCBNumber.Items.Add(mppc_list[TextBox_Gather_i].ToString().PadLeft(5, '0'));
                        }
                        PCBNumber.SelectedIndex = 0;
                        var mppc_s = Mppc_results.Where(sx => sx.Number == Mppc_results[Mppc_results.Count - 1].Number).ToList();
                        Number.Text = mppc_s[0].Number.ToString().PadLeft(5, '0');
                        lightHeaadNumber.Text = mppc_s[0].LightHeaadNumber.ToString().PadLeft(5, '0');
                        PCBNumber.Text = mppc_s[0].PCBNumber.ToString().PadLeft(5, '0');
                        for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
                        {
                            var mppc_sx = mppc_s.Where(sx => sx.FluorescenceChannel == FluorescenceChannel[TextBox_Gather_i]).ToList();
                            TextBox_Gather_j = 0;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Vbr.ToString();//FAM_Vbr;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Vov.ToString();//FAM_Vov;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Tvop.ToString();//FAM_Tvop;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Temp_K2.ToString();//FAM_Temp_K2;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Temp_K1.ToString();//FAM_Temp_K1;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Temp_B.ToString();//FAM_Temp_B;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].DAC_K2.ToString();//FAM_DAC_K2;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].DAC_K1.ToString();//FAM_DAC_K1;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].DAC_B.ToString();//FAM_DAC_B;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].ADC_K2.ToString();//FAM_ADC_K2;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].ADC_K1.ToString();//FAM_ADC_K1;
                            TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].ADC_B.ToString();//FAM_ADC_B;
                            TextBox_Gather_j = 0;
                            TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_Vi.ToString();
                            TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_K2.ToString();
                            TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_K1.ToString();
                            TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_B.ToString();
                            TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].I_K2.ToString();
                            TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].I_K1.ToString();
                            TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].I_B.ToString();
                        }
                    }
                }


                //{
                //    int i_cs = 0;
                //    for (TextBox_Gather_j = 0; TextBox_Gather_j < 4; TextBox_Gather_j++)
                //    {
                //        for (TextBox_Gather_i = 0; TextBox_Gather_i < 12; TextBox_Gather_i++)
                //        {

                //                TextBox_Gather[TextBox_Gather_j, TextBox_Gather_i].Text = i_cs.ToString();
                //            i_cs++;
                //        }
                //    }
                //    for (TextBox_Gather_j = 0; TextBox_Gather_j < 4; TextBox_Gather_j++)
                //    {
                //        for (TextBox_Gather_i = 0; TextBox_Gather_i < 7; TextBox_Gather_i++)
                //        {

                //                TextBox_LED[TextBox_Gather_j, TextBox_Gather_i].Text = i_cs.ToString();
                //            i_cs++;

                //        }
                //    }
                //}
            }
        }
        //把网格 
        //Combobox上修改的数据提交到当前的当用户选择的单元格移动到性别这一列时，我们要显示下拉列表框，添加如下事件
        public void dgv_User_CurrentCellChanged(object sender, EventArgs e)
        {
            //try
            //{

            //    if (this.dataGridView1.CurrentCell.ColumnIndex == 4)
            //    {
            //        Rectangle rect = dataGridView1.GetCellDisplayRectangle(dataGridView1.CurrentCell.ColumnIndex, dataGridView1.CurrentCell.RowIndex, false);
            //        string sexValue = dataGridView1.CurrentCell.Value.ToString();                  
            //        cmb_Temp.Text = sexValue;
            //        cmb_Temp.Left = rect.Left;
            //        cmb_Temp.Top = rect.Top;
            //        cmb_Temp.Width = rect.Width;
            //        cmb_Temp.Height = rect.Height;
            //        cmb_Temp.Visible = true;
            //    }
            //    else
            //    {
            //        cmb_Temp.Visible = false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("错误：" + ex.Message);
            //}
        }

        //当用户选择下拉列表框时改变DataGridView单元格的内容
        public void cmb_Temp_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (((ComboBox)sender).Text == "男")
            //{
            //    dataGridView1.CurrentCell.Value = "男";
            //    dataGridView1.CurrentCell.Tag = "1";
            //}
            //else
            //{
            //    dataGridView1.CurrentCell.Value = "女";
            //    dataGridView1.CurrentCell.Tag = "0";
            //}
            //dataGridView1.CurrentCell.Value =((ComboBox)sender).Text ;
            //dataGridView1.CurrentCell.Tag = ((ComboBox)sender).SelectedValue;
            //  dataGridView1.CurrentCell.
            //switch(((ComboBox)sender).Text)
            //{
            //    case 
            //}
        }

        //当滚动DataGridView或者改变DataGridView列宽时将下拉列表框设为不可见

        public void dgv_User_Scroll(object sender, ScrollEventArgs e)
        {
            this.cmb_Temp.Visible = false;
        }

        private void dgv_User_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            this.cmb_Temp.Visible = false;
        }

        //绑定数据表后将性别列中的每一单元格的Value和Tag属性（Tag为值文本，Value为显示文本）
        public void dgv_User_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            //{
            //    //if (dataGridView1.Rows[i].Cells[2].Value != null && dataGridView1.Rows[i].Cells[2].ColumnIndex == 2)
            //    //{
            //    //    dataGridView1.Rows[i].Cells[2].Tag = dataGridView1.Rows[i].Cells[2].Value.ToString();
            //    //    if (dataGridView1.Rows[i].Cells[2].Value.ToString() == "1")
            //    //    {
            //    //        dataGridView1.Rows[i].Cells[2].Value = "男";
            //    //    }
            //    //    else if (dataGridView1.Rows[i].Cells[2].Value.ToString() == "0")
            //    //    {
            //    //        dataGridView1.Rows[i].Cells[2].Value = "女";
            //    //    }
            //    //}
            //}
        }



        public void StatusUpdate()
        {
            //curve_x ==1
            if (curver_en == 1)
            {
                string s_data = "";
                if (curve_x == 0)
                {
                    while (shuju_w != shuju_r)
                    {

                        curve1.textBox1.Text = shuju_Temp[show_i, shuju_r].ToString();
                        curve1.chart1.Series[0].Points.AddXY(i, shuju_Temp[show_i, shuju_r]);//设置显示数据
                        //curve1.textBox2.Text = shuju_VH[show_i, shuju_r].ToString();
                        //curve1.chart1.Series[1].Points.AddXY(i, shuju_VH[show_i, shuju_r]);//设置显示数据  shuju_VH_target
                        curve1.textBox3.Text = shuju_VH_target[show_i, shuju_r].ToString();
                        curve1.chart1.Series[2].Points.AddXY(i, shuju_VH_target[show_i, shuju_r]);//设置显示数据      
                        curve1.textBox1.Update();
                        curve1.textBox2.Update();
                        curve1.textBox3.Update();
                        curve1.textBox4.Update();
                        if (shuju_max < shuju_Temp[show_i, shuju_r])
                        {
                            shuju_max = shuju_Temp[show_i, shuju_r];
                        }
                        if (shuju_min > shuju_Temp[show_i, shuju_r])
                        {
                            shuju_min = shuju_Temp[show_i, shuju_r] - 0.5;
                            if (shuju_min < 0)
                                shuju_min = 0;
                        }
                        //if (shuju_max < shuju_VH[show_i, shuju_r])
                        //{
                        //    shuju_max = shuju_VH[show_i, shuju_r];
                        //}
                        //if (shuju_min > shuju_VH[show_i, shuju_r])
                        //{
                        //    shuju_min = shuju_VH[show_i, shuju_r] - 10;
                        //    if (shuju_min < 0)
                        //        shuju_min = 0;
                        //}
                        if (shuju_max < shuju_VH_target[show_i, shuju_r])
                        {
                            shuju_max = shuju_VH_target[show_i, shuju_r];
                        }
                        if (shuju_min > shuju_VH_target[show_i, shuju_r])
                        {
                            shuju_min = shuju_VH_target[show_i, shuju_r] - 0.5;
                            if (shuju_min < 0)
                                shuju_min = 0;
                        }
                        for (int j = 0; j < 4; j++)
                        {
                            s_data += shuju_Temp[j, shuju_r].ToString() + "," + shuju_VH_target[j, shuju_r].ToString() + ",";
                        }
                        shuju_r++;
                        if (shuju_r >= 5000)
                        {
                            shuju_r = 0;
                        }
                        i++;
                        if (i > (winnum + 1))
                        {
                            curve1.chart1.ChartAreas[0].AxisX.ScaleView.Position = i - winnum;
                            curve1.chart1.ChartAreas[0].AxisX.ScaleView.Size = winnum;//视野范围内共有多少个数据点

                            curve1.chart1.Series[0].Points.RemoveAt(0);
                            //i11++;
                        }

                        curve1.chart1.ChartAreas[0].AxisY.ScaleView.Position = shuju_min;
                        curve1.chart1.ChartAreas[0].AxisY.ScaleView.Size = shuju_max - shuju_min + 0.5;// + 10;

                        s_data += "\r\n";
                    }
                    using (var stream = File.Open(data_path, FileMode.Append))
                    using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                    using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                    {
                        // Don't write the header again. csv.WriteComment(writer);
                        csv.Configuration.HasHeaderRecord = false;
                        csv.WriteField(s_data, false);
                    }
                }
                else if (curve_x == 1)
                {
                    while (shuju_w != shuju_r)
                    {

                        curve1.textBox1.Text = shuju_Temp_0[show_i, shuju_r].ToString();
                        curve1.chart1.Series[0].Points.AddXY(i, shuju_Temp_0[show_i, shuju_r]);//设置显示数据
                        curve1.textBox2.Text = shuju_Temp_1[show_i, shuju_r].ToString();
                        curve1.chart1.Series[1].Points.AddXY(i, shuju_Temp_1[show_i, shuju_r]);//设置显示数据  shuju_VH_target
                        curve1.textBox3.Text = shuju_Temp_2[show_i, shuju_r].ToString();
                        curve1.chart1.Series[2].Points.AddXY(i, shuju_Temp_2[show_i, shuju_r]);//设置显示数据      
                        curve1.textBox1.Update();
                        curve1.textBox2.Update();
                        curve1.textBox3.Update();
                        curve1.textBox4.Update();
                        if (shuju_max < shuju_Temp_0[show_i, shuju_r])
                        {
                            shuju_max = shuju_Temp_0[show_i, shuju_r];
                        }
                        if (shuju_min > shuju_Temp_0[show_i, shuju_r])
                        {
                            shuju_min = shuju_Temp_0[show_i, shuju_r] - 0.5;
                            if (shuju_min < 0)
                                shuju_min = 0;
                        }
                        for (int j = 0; j < 8; j++)
                        {
                            s_data += shuju_Temp_0[j, shuju_r].ToString() + "," + shuju_Temp_1[j, shuju_r].ToString() + "," + shuju_Temp_2[j, shuju_r].ToString() + ",";
                        }
                        shuju_r++;
                        if (shuju_r >= 5000)
                        {
                            shuju_r = 0;
                        }
                        i++;
                        if (i > (winnum + 1))
                        {
                            curve1.chart1.ChartAreas[0].AxisX.ScaleView.Position = i - winnum;
                            curve1.chart1.ChartAreas[0].AxisX.ScaleView.Size = winnum;//视野范围内共有多少个数据点

                            curve1.chart1.Series[0].Points.RemoveAt(0);
                            //i11++;
                        }

                        curve1.chart1.ChartAreas[0].AxisY.ScaleView.Position = shuju_min;
                        curve1.chart1.ChartAreas[0].AxisY.ScaleView.Size = shuju_max - shuju_min + 0.5;// + 10;

                        s_data += "\r\n";
                    }
                    using (var stream = File.Open(data_path, FileMode.Append))
                    using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                    using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                    {
                        // Don't write the header again. csv.WriteComment(writer);
                        csv.Configuration.HasHeaderRecord = false;
                        csv.WriteField(s_data, false);
                    }
                }
                else if (curve_x == 2)
                {
                    while (shuju_w != shuju_r)
                    {
                        curve1.textBox1.Text = shuju_Temp_2[0, shuju_r].ToString();
                        curve1.chart1.Series[0].Points.AddXY(i, shuju_Temp_2[0, shuju_r]);//设置显示数据
                        curve1.textBox2.Text = shuju_Temp_2[1, shuju_r].ToString();
                        curve1.chart1.Series[1].Points.AddXY(i, shuju_Temp_2[1, shuju_r]);//设置显示数据  shuju_VH_target
                        curve1.textBox3.Text = shuju_Temp_2[2, shuju_r].ToString();
                        curve1.chart1.Series[2].Points.AddXY(i, shuju_Temp_2[2, shuju_r]);//设置显示数据  
                        curve1.textBox4.Text = shuju_Temp_2[3, shuju_r].ToString();
                        curve1.chart1.Series[3].Points.AddXY(i, shuju_Temp_2[3, shuju_r]);//设置显示数据   
                        curve1.textBox1.Update();
                        curve1.textBox2.Update();
                        curve1.textBox3.Update();
                        curve1.textBox4.Update();
                        if (shuju_max < shuju_Temp_2[0, shuju_r])
                        {
                            shuju_max = shuju_Temp_2[0, shuju_r];
                        }
                        if (shuju_min > shuju_Temp_2[0, shuju_r])
                        {
                            shuju_min = shuju_Temp_2[0, shuju_r] - 0.5;
                            if (shuju_min < 0)
                                shuju_min = 0;
                        }
                        for (int j = 0; j < 8; j++)
                        {
                            s_data += shuju_Temp_0[j, shuju_r].ToString() + "," + shuju_Temp_1[j, shuju_r].ToString() + "," + shuju_Temp_2[j, shuju_r].ToString() + ",";
                        }
                        shuju_r++;
                        if (shuju_r >= 5000)
                        {
                            shuju_r = 0;
                        }
                        i++;
                        if (i > (winnum + 1))
                        {
                            curve1.chart1.ChartAreas[0].AxisX.ScaleView.Position = i - winnum;
                            curve1.chart1.ChartAreas[0].AxisX.ScaleView.Size = winnum;//视野范围内共有多少个数据点

                            curve1.chart1.Series[0].Points.RemoveAt(0);
                            //i11++;
                        }
                        curve1.chart1.ChartAreas[0].AxisY.ScaleView.Position = shuju_min;
                        curve1.chart1.ChartAreas[0].AxisY.ScaleView.Size = shuju_max - shuju_min + 0.5;// + 10;
                        s_data += "\r\n";
                    }
                    using (var stream = File.Open(data_path, FileMode.Append))
                    using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                    using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                    {
                        // Don't write the header again. csv.WriteComment(writer);
                        csv.Configuration.HasHeaderRecord = false;
                        csv.WriteField(s_data, false);
                    }
                }
                else
                {
                    while (shuju_w != shuju_r)
                    {
                        curve1.textBox1.Text = shuju_Temp_2[4, shuju_r].ToString();
                        curve1.chart1.Series[0].Points.AddXY(i, shuju_Temp_2[4, shuju_r]);//设置显示数据
                        curve1.textBox2.Text = shuju_Temp_2[5, shuju_r].ToString();
                        curve1.chart1.Series[1].Points.AddXY(i, shuju_Temp_2[5, shuju_r]);//设置显示数据  shuju_VH_target
                        curve1.textBox3.Text = shuju_Temp_2[6, shuju_r].ToString();
                        curve1.chart1.Series[2].Points.AddXY(i, shuju_Temp_2[6, shuju_r]);//设置显示数据  
                        curve1.textBox4.Text = shuju_Temp_2[7, shuju_r].ToString();
                        curve1.chart1.Series[3].Points.AddXY(i, shuju_Temp_2[7, shuju_r]);//设置显示数据   
                        curve1.textBox1.Update();
                        curve1.textBox2.Update();
                        curve1.textBox3.Update();
                        curve1.textBox4.Update();
                        if (shuju_max < shuju_Temp_2[4, shuju_r])
                        {
                            shuju_max = shuju_Temp_2[4, shuju_r];
                        }
                        if (shuju_min > shuju_Temp_2[4, shuju_r])
                        {
                            shuju_min = shuju_Temp_2[4, shuju_r] - 0.5;
                            if (shuju_min < 0)
                                shuju_min = 0;
                        }
                        for (int j = 0; j < 8; j++)
                        {
                            s_data += shuju_Temp_0[j, shuju_r].ToString() + "," + shuju_Temp_1[j, shuju_r].ToString() + "," + shuju_Temp_2[j, shuju_r].ToString() + ",";
                        }
                        shuju_r++;
                        if (shuju_r >= 5000)
                        {
                            shuju_r = 0;
                        }
                        i++;
                        if (i > (winnum + 1))
                        {
                            curve1.chart1.ChartAreas[0].AxisX.ScaleView.Position = i - winnum;
                            curve1.chart1.ChartAreas[0].AxisX.ScaleView.Size = winnum;//视野范围内共有多少个数据点

                            curve1.chart1.Series[0].Points.RemoveAt(0);
                            //i11++;
                        }
                        curve1.chart1.ChartAreas[0].AxisY.ScaleView.Position = shuju_min;
                        curve1.chart1.ChartAreas[0].AxisY.ScaleView.Size = shuju_max - shuju_min + 0.5;// + 10;
                        s_data += "\r\n";
                    }
                    using (var stream = File.Open(data_path, FileMode.Append))
                    using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                    using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                    {
                        // Don't write the header again. csv.WriteComment(writer);
                        csv.Configuration.HasHeaderRecord = false;
                        csv.WriteField(s_data, false);
                    }
                }

            }


            StatusUpdate_start = 0;
        }
        //显示线程
        public void TransfersThread1()
        {
            System.Threading.Thread.Sleep(1000);//延时1s//等待窗口创建完成
            while (true)
            {
                if (StatusUpdate_start == 0 && curver_en == 1)  //说明显示进程未执行，进行ui显示进程 shuju_stop = 1;//1开始显示数据  0不显示数据
                {
                    if (shuju_w != shuju_r)
                    {
                        StatusUpdate_start = 1;
                        this.Invoke(updateUI);//在主线程调用ui显示线程  119行显示

                    }
                }
                else
                {
                    //shuju_r = shuju_w;
                }
            }

        }

        public const int WM_DEVICECHANGE = 0x0219;//WM_DeviceChange
        public enum WM_DEVICECHANGE_WPPARAMS
        {
            DBT_CONFIGCHANGECANCELED = 0x0019,//ConfigChangeCanceled
            DBT_CONFIGCHANGED = 0x0018,//ConfigChanged
            DBT_CUSTOMEVENT = 0x8006,//CustomEvent
            DBT_DEVICEARRIVAL = 0x8000,//DeviceArray//添加设备
            DBT_DEVICEQUERYREMOVE = 0x8001,//DeviceQueryRomove
            DBT_DEVICEQUERYREMOVEFAILED = 0x8002,//DeviceQueryRemoveFailed
            DBT_DEVICEREMOVECOMPLETE = 0x8004,//DeviceMoveComplete//移除设备  
            DBT_DEVICEREMOVEPENDING = 0x8003,//DeviceMovePending
            DBT_DEVICETYPESPECIFIC = 0x8005,//DeviceTypeSpecific
            DBT_DEVNODES_CHANGED = 0x0007,//DevNodes_Changed
            DBT_QUERYCHANGECONFIG = 0x0017,//QueryChangeConfig
            DBT_USERDEFINED = 0xFFFF//Userdefined
        };
        //windows消息处理函数
        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == WM_DEVICECHANGE)
                {
                    switch ((WM_DEVICECHANGE_WPPARAMS)(m.WParam.ToInt32()))
                    {
                        case WM_DEVICECHANGE_WPPARAMS.DBT_DEVICEARRIVAL: //设备的增加
                            //tabControl1.
                            GetPort();//直接调用串口重新获取          
                            break;
                        case WM_DEVICECHANGE_WPPARAMS.DBT_DEVICEREMOVECOMPLETE: //设备的删除
                                                                                // textBox1.Text = " ";
                            int x1 = Serial_Port.SelectedIndex;//获取或设置指定当前选定项的索引。
                            string s1 = Serial_Port.Text;//获取窗口显示文本。 
                            GetPort();//调用串口重新获取
                            int num = Serial_Port.Items.Count; //获取下拉框数量
                            int i;
                            for (i = 0; i < num; i++)
                            {
                                if (s1 == (Serial_Port.Items[i].ToString()))
                                {
                                    break;
                                }
                            }
                            if (i >= num)  //说明串口号已经消失
                            {
                                Serial_Port.SelectedIndex = 0;//获取或设置指定当前选定项的索引。
                            }
                            else
                            {
                                Serial_Port.SelectedIndex = x1;//获取或设置指定当前选定项的索引。
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            base.WndProc(ref m);
        }
        //串口解析代码
        byte[] USART_RX_BUF = new byte[1200];//接收缓冲区  USART_RX_BUF[];     //接收缓冲,最大USART_REC_LEN个字节.
        //接收到的有效字节数目
        int USART_RX_STA = 0;       //接收状态标记	  
        bool USART_RX_STA1 = false;
        int USART_step = 1;
        int USART_numx = 0;//有效数据长度
        //这使异步调用委托给设置/文本框控件上的文本属性。
        delegate void SetTextCallback(string text);
        //此方法显示一个模式进行线程安全调用Windows窗体控件上。
        //如果调用线程不同的线程从TextBox控件创建，此方法创建SetTextCallback和使用调用异步方法调用自身。
        //如果调用线程是一样的TextBox控件创建的线程，直接设置Text属性。 
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.Serial_Data.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                Serial_Data.AppendText(text);
                // this.textBox30.Text = text;
            }
        }

        //这使异步调用委托给设置/文本框控件上的文本属性。
        delegate void SetText0xxCallback(string text, object textBox0xx1);
        //此方法显示一个模式进行线程安全调用Windows窗体控件上。
        //如果调用线程不同的线程从TextBox控件创建，此方法创建SetTextCallback和使用调用异步方法调用自身。
        //如果调用线程是一样的TextBox控件创建的线程，直接设置Text属性。 
        private void SetText0xx(string text, object textBox0xx1)
        {
            TextBox textBox0xx = (TextBox)textBox0xx1;
            if (textBox0xx.InvokeRequired)
            {
                SetText0xxCallback d = new SetText0xxCallback(SetText0xx);
                this.Invoke(d, new object[] { text, textBox0xx });
            }
            else
            {
                textBox0xx.Text = text;
                textBox0xx.Update();
            }
        }
        private void setText_Appendtext(string text, object textBox0xx1)
        {
            TextBox textBox0xx = (TextBox)textBox0xx1;
            InvokeToForm(() =>
            {
                textBox0xx.AppendText(text);
            });
        }

        //解码flash数据
        private void Decode()
        {
            // BitConverter.ToSingle(USART_RX_BUF, 6);
            int i = 7;
            int Offset_Address = (USART_RX_BUF[7] << 8) + USART_RX_BUF[8];//计算偏移地址
            i = i + 2;
            int Data_Length = (USART_RX_BUF[9] << 8) + USART_RX_BUF[10];//计算数据长度
            //BitConverter.ToUInt16();
            i = i + 2;
            if (Offset_Address == 0 && Data_Length == 304)
            {
                int TextBox_Gather_i;
                int TextBox_Gather_j;
                string sdatax;
                string sDATAS = "返回数据为\r\n";

                for (TextBox_Gather_j = 0; TextBox_Gather_j < 4; TextBox_Gather_j++)
                {
                    for (TextBox_Gather_i = 0; TextBox_Gather_i < 12; TextBox_Gather_i++)
                    {
                        sdatax = System.Convert.ToString(BitConverter.ToSingle(USART_RX_BUF, i));
                        i = i + 4;
                        //将数据写入文本框
                        if (TextBox_Gather[TextBox_Gather_j, TextBox_Gather_i].InvokeRequired)
                        {//这里由于进程是不一样的AppendText
                            SetText0xxCallback d = new SetText0xxCallback(SetText0xx);
                            this.Invoke(d, new object[] { sdatax, TextBox_Gather[TextBox_Gather_j, TextBox_Gather_i] });
                        }
                        else
                        {
                            TextBox_Gather[TextBox_Gather_j, TextBox_Gather_i].Text = sdatax;
                            TextBox_Gather[TextBox_Gather_j, TextBox_Gather_i].Update();
                        }
                        sDATAS += TextBox_Gather[TextBox_Gather_j, TextBox_Gather_i].Name + " = " + sdatax + " ;   ";
                    }
                    sDATAS += "\r\n***********************************\r\n";
                }
                for (TextBox_Gather_j = 0; TextBox_Gather_j < 4; TextBox_Gather_j++)
                {
                    for (TextBox_Gather_i = 0; TextBox_Gather_i < 7; TextBox_Gather_i++)
                    {
                        sdatax = System.Convert.ToString(BitConverter.ToSingle(USART_RX_BUF, i));
                        i = i + 4;
                        //将数据写入文本框
                        if (TextBox_LED[TextBox_Gather_j, TextBox_Gather_i].InvokeRequired)
                        {//这里由于进程是不一样的AppendText
                            SetText0xxCallback d = new SetText0xxCallback(SetText0xx);
                            this.Invoke(d, new object[] { sdatax, TextBox_LED[TextBox_Gather_j, TextBox_Gather_i] });
                        }
                        else
                        {
                            TextBox_LED[TextBox_Gather_j, TextBox_Gather_i].Text = sdatax;
                            TextBox_LED[TextBox_Gather_j, TextBox_Gather_i].Update();
                        }
                        sDATAS += TextBox_LED[TextBox_Gather_j, TextBox_Gather_i].Name + " = " + sdatax + " ;   ";
                    }
                    sDATAS += "\r\n***********************************\r\n";
                }
                if (this.Serial_Data.InvokeRequired)
                {//这里由于进程是不一样的AppendText
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { sDATAS });
                }
                else
                {
                    Serial_Data.AppendText(sDATAS);
                }
            }
            else
            {
                string sDATAS = "发生一次存储事件\r\n";
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText(sDATAS);
                });
            }
        }

        private void Temperature_Decode()
        {
            string[] ADC_TEMP = new string[8] { "MPPC_TEMP_FAM",
                                                "MPPC_TEMP_VIC",
                                                "MPPC_TEMP_ROX",
                                                "MPPC_TEMP_CY5",
                                                "LED_TEMP_FAM",
                                                "LED_TEMP_VIC",
                                                "LED_TEMP_ROX",
                                                "LED_TEMP_CY5",
                                               };
            int i = 6;
            string sdatax;
            string sDATAS = "温度为：\r\n";
            for (int t = 0; t < 8; t++)
            {
                sdatax = System.Convert.ToString(BitConverter.ToSingle(USART_RX_BUF, i));
                Fluorescent_Head_TEMP[t] = sdatax;
                i = i + 4;
                sDATAS += ADC_TEMP[t] + " = " + sdatax + "    ;   ";
            }
            Fluorescent_Head_TEMP_Flag = true;
            sDATAS += "\r\n";
            if (this.Serial_Data.InvokeRequired)
            {//这里由于进程是不一样的AppendText
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { sDATAS });
            }
            else
            {
                Serial_Data.AppendText(sDATAS);
            }
        }
        //解码adc读取数据
        private void ADC_Decode()
        {
            string sdatax;
            string sDATAS = "返回数据为\r\n";
            string[] sdatass = new string[8];
            int i = 6;
            int data;
            for (int t = 0; t < 8; t++)
            {
                data = (USART_RX_BUF[i] << 8) + USART_RX_BUF[i + 1];//计算偏移地址
                i = i + 2;
                ADC_DATA[ADC_num[t]] = data;
                sdatass[t] = ADC_CH[t] + " = " + System.Convert.ToString(data) + " ;   ";
            }
            sDATAS += sdatass[6] + sdatass[2] + sdatass[3] + sdatass[7] + " \r\n" + sdatass[5] + sdatass[0] + sdatass[4] + sdatass[1] + " \r\n";
            if (this.Serial_Data.InvokeRequired)
            {//这里由于进程是不一样的AppendText
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { sDATAS });
            }
            else
            {
                Serial_Data.AppendText(sDATAS);
            }
        }

        private void VH_Decde()
        {
            float f = 1.0f;
            float f2 = 1.0f;
            int i = 6;
            for (int j = 0; j < 4; j++)
            {
                shuju_VH_target[j, shuju_w] = BitConverter.ToSingle(USART_RX_BUF, i);
                f2 = shuju_VH_target[j, shuju_w];
                i = i + 4;
                shuju_Temp[j, shuju_w] = BitConverter.ToSingle(USART_RX_BUF, i);
                f = shuju_Temp[j, shuju_w];
                i = i + 4;
            }

            shuju_w++;
            if (shuju_w >= 5000)
            {
                shuju_w = 0;
            }
        }
        private void TEMP_Decde()
        {
            float f = 1.0f;
            float f2 = 1.0f;
            int i = 6;
            for (int j = 0; j < 8; j++)
            {
                shuju_Temp_0[j, shuju_w] = BitConverter.ToSingle(USART_RX_BUF, i);
                i = i + 4;
                shuju_Temp_1[j, shuju_w] = BitConverter.ToSingle(USART_RX_BUF, i);
                i = i + 4;
                shuju_Temp_2[j, shuju_w] = BitConverter.ToSingle(USART_RX_BUF, i);
                i = i + 4;
            }

            shuju_w++;
            if (shuju_w >= 5000)
            {
                shuju_w = 0;
            }
        }
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {//串口1数据读取
            int ReceiveNums = serialPort1.BytesToRead;   //获取接收缓冲区的字节数
            byte[] recBuffer;//接收缓冲区
            recBuffer = new byte[serialPort1.BytesToRead];//接收数据缓存大小
            serialPort1.Read(recBuffer, 0, recBuffer.Length);//读取数据
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
            //if (this.Serial_Data.InvokeRequired)
            //{//这里由于进程是不一样的AppendText
            //    SetTextCallback d = new SetTextCallback(SetText);
            //    this.Invoke(d, new object[] { recData });
            //}
            //else
            //{
            //    Serial_Data.AppendText(recData);
            //}
            //进行数据解码
            byte Res;
            for (int i = 0; i < recBuffer.Length; i++)
            {
                Res = recBuffer[i];
                if (USART_step == 1) //说明还没有接收到数据开始
                {
                    if ((USART_RX_STA) == 0) //这里说明是第一个数据
                    {
                        USART_RX_BUF[USART_RX_STA] = Res;//将数据保存至缓冲区
                        if (USART_RX_BUF[0] == 0x5A) //这里判断是不是0X5A
                        {
                            USART_RX_STA++;
                        }
                    }
                    else
                    {
                        USART_RX_BUF[USART_RX_STA] = Res;//将数据保存至缓冲区
                        if (USART_RX_BUF[1] == 0xA5)
                        {//这里说明接收到了0X5A 0XA5这样的头
                            USART_step = 2;
                            USART_RX_STA++;
                        }
                        else
                        {//这里说明未接收到正确的数据
                            USART_RX_STA = 0;
                        }
                    }
                }
                else//这里说明已经接收到了正确的头
                {
                    USART_RX_BUF[USART_RX_STA] = Res; //将数据保存至缓冲区
                    if ((USART_RX_STA) == 2) //这里说明是第一个数据,实际为数据长度
                    {
                        USART_RX_STA++;
                    }
                    else if ((USART_RX_STA) == 3) //这里说明是第二个数据，实际为数据类型
                    {
                        USART_numx = (USART_RX_BUF[2] << 8) + USART_RX_BUF[3] + 6;
                        USART_RX_STA++;
                    }
                    else
                    {
                        USART_RX_STA++;
                        if ((USART_RX_STA) >= USART_numx)//说明数据接收完成
                        {
                            if ((USART_RX_BUF[USART_numx - 1] == 0xBB) && (USART_RX_BUF[USART_numx - 2] == 0xAA))//接收到正确数据
                            {
                                USART_RX_STA1 = true;	//接收完成了 
                            }
                            else
                            {//数据接收出错
                                USART_step = 1;
                                USART_RX_STA = 0;//接收数据错误,重新开始接收	  
                            }
                        }
                    }
                }
                if (USART_RX_STA1)//说明接收到数据
                {
                    int xi = 8;
                    string sDATAS = "";
                    switch (USART_RX_BUF[5])
                    {
                        case 0X9E:   //温度返回数据
                            TEMP_Decde();
                            break;
                        case 0X9F:   //温度返回数据
                            VH_Decde();
                            break;
                        case 0XA1:   //返回FALSH
                            Decode();

                            break;
                        case 0XA2:   //ADC查询返回数据
                            ADC_Decode();
                            break;
                        case 0XA3:   //温度返回数据
                            sDATAS = "设置DAC输出正常\r\n";
                            if (this.Serial_Data.InvokeRequired)
                            {//这里由于进程是不一样的AppendText
                                SetTextCallback d = new SetTextCallback(SetText);
                                this.Invoke(d, new object[] { sDATAS });
                            }
                            else
                            {
                                Serial_Data.AppendText(sDATAS);
                            }
                            break;
                        case 0XA4:   //温度返回数据
                            {
                                if (USART_RX_BUF[6] == 0x01)
                                {
                                    sDATAS = "启动高压输出\r\n";
                                }
                                else
                                {
                                    sDATAS = "关闭高压输出\r\n";
                                }


                                if (this.Serial_Data.InvokeRequired)
                                {//这里由于进程是不一样的AppendText
                                    SetTextCallback d = new SetTextCallback(SetText);
                                    this.Invoke(d, new object[] { sDATAS });
                                }
                                else
                                {
                                    Serial_Data.AppendText(sDATAS);
                                }
                            }
                            break;
                        case 0xA5:
                            Temperature_Decode();
                            break;
                        case 0X08:   //LED打开关闭
                            {
                                if (USART_RX_BUF[6] >= 0x01)
                                {
                                    sDATAS = "打开LED\r\n";
                                }
                                else
                                {
                                    sDATAS = "关闭LED\r\n";
                                }


                                if (this.Serial_Data.InvokeRequired)
                                {//这里由于进程是不一样的AppendText
                                    SetTextCallback d = new SetTextCallback(SetText);
                                    this.Invoke(d, new object[] { sDATAS });
                                }
                                else
                                {
                                    Serial_Data.AppendText(sDATAS);
                                }
                            }
                            break;
                        case 0X0D:   //LED打开关闭
                            {
                                int data = (USART_RX_BUF[7] << 8) + USART_RX_BUF[8];//计算偏移地址
                                if (data > 0)
                                {
                                    sDATAS = "打开MPPC\r\n";
                                }
                                else
                                {
                                    sDATAS = "关闭MPPC\r\n";
                                }


                                if (this.Serial_Data.InvokeRequired)
                                {//这里由于进程是不一样的AppendText
                                    SetTextCallback d = new SetTextCallback(SetText);
                                    this.Invoke(d, new object[] { sDATAS });
                                }
                                else
                                {
                                    Serial_Data.AppendText(sDATAS);
                                }
                            }
                            break;
                        default:

                            break;
                    }
                    //下面为接收数据处理
                    USART_step = 1;
                    USART_RX_STA = 0;
                    USART_RX_STA1 = false;
                }
            }
        }

        /// <summary>
        ///读取文件路径
        /// </summary>
        /// <returns>文件路径</returns>
        public static string Read_Name_Path()
        {
            string path1 = System.Configuration.ConfigurationManager.AppSettings["INI_PATH"];  //读取上次保存的路径
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
                ini_name = path1.Substring(path1.LastIndexOf("\\") + 1, (path1.Length - path1.LastIndexOf("\\") - 5));  //截取目
                //下面遍历PATH1的所有文件
                //DirectoryInfo TheFolder = new DirectoryInfo(path1);
                //if (!TheFolder.Exists)
                //{
                //    return null;
                //}
                GetAppSetting.GetAppSetting_data("INI_PATH", path2);//保存本次打开路径
                return path1;
            }
            return null;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string file_name = Read_Name_Path();//需要读取的文件名
            using (var reader = new StreamReader(file_name))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var s = csv.GetRecords<RawPcrScanData>().ToList();
            }
            file_name = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string file_name = Read_Name_Path();//需要读取的文件名
            var reader = new StreamReader(file_name);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            while (csv.Read())
            {
                var s = csv.GetRecord<RawPcrScanData>();//ok
                                                        // var s = csv.GetRecord<string>();//ok
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string file_name = Read_Name_Path();//需要读取的文件名
            var reader = new StreamReader(file_name);
            var parser = new CsvParser(reader, CultureInfo.InvariantCulture);
            while (true)
            {
                var row = parser.Read();
                if (row == null)
                {
                    break;
                }
                for (int i = 0; i < row.Length; i++)
                {
                    string s = row[i];
                    string[] arr = s.Split('\t');
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string path1 = System.Configuration.ConfigurationManager.AppSettings["PATH"];  //读取上次保存的路径
            if (Directory.Exists(path1) == false)//判断路径是否存在
            {
                path1 = System.Environment.CurrentDirectory;//获取绝对路径
                path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                 //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
            }
            path1 += "\\ name.csv";
            IO_Operate.File_creation(ref path1, true);//判定文件路径并根据选择判定是否重新构建文件
            using (var stream = File.Open(path1, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                csv.Configuration.HasHeaderRecord = false;
                csv.WriteField("ss\r\nss,SetStyle,\r\n", false);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string path1 = System.Configuration.ConfigurationManager.AppSettings["PATH"];  //读取上次保存的路径
            if (Directory.Exists(path1) == false)//判断路径是否存在
            {
                path1 = System.Environment.CurrentDirectory;//获取绝对路径
                path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                 //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
            }
            path1 += "\\ name.csv";
            IO_Operate.File_creation(ref path1, false);//判定文件路径并根据选择判定是否重新构建文件
            using (var stream = File.Open(path1, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                csv.Configuration.HasHeaderRecord = false;
                csv.WriteField("ss\r\nss,SetStyle,\r\n", false);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {

            string path1 = System.Configuration.ConfigurationManager.AppSettings["PATH"];  //读取上次保存的路径
            if (Directory.Exists(path1) == false)//判断路径是否存在
            {
                path1 = System.Environment.CurrentDirectory;//获取绝对路径
                path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                 //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
            }
            path1 += "\\ name1.csv";
            IO_Operate.File_creation(ref path1, true);//判定文件路径并根据选择判定是否重新构建文件
            string file_name = Read_Name_Path();//需要读取的文件名
            //using (var stream = File.Open(path1, FileMode.Append))
            //using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            //using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            //{
            //    // Don't write the header again. csv.WriteComment(writer);
            //    csv.Configuration.HasHeaderRecord = false;
            //    csv.WriteField("ss\r\nss,SetStyle,\r\n", false);
            //}

            using (var reader = new StreamReader(file_name))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var s = csv.GetRecords<RawPcrScanData>().ToList();
                var reader_w = new StreamWriter(path1);
                var csv_w = new CsvWriter(reader_w, CultureInfo.InvariantCulture);
                csv_w.Configuration.RegisterClassMap<RawPcrScanDataMap>();
                csv_w.WriteRecords(s);
                csv_w.Flush();
                //csv_w.WriteRecord(s);
                csv_w.Dispose();
            }
        }
        /// <summary>
        ///读取文件路径
        /// </summary>
        /// <returns>文件路径</returns>
        public static string INIRead_Name_Path()
        {
            string path1 = System.Configuration.ConfigurationManager.AppSettings["INI_PATH"];  //读取上次保存的路径
            if (Directory.Exists(path1) == false)//判断路径是否存在
            {
                path1 = System.Environment.CurrentDirectory;//获取绝对路径
                path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                 //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
            }
            OpenFileDialog file1 = new OpenFileDialog(); //定义提示用户打开文件
            file1.Filter = "INI文件|*.ini";//设置文件后缀的过滤
            if (path1 != null) { file1.InitialDirectory = path1; }//设置打开文件路径
                                                                  //获取文件夹的绝对路径
            if (file1.ShowDialog() == DialogResult.OK) //如果有选择打开文件
            {
                path1 = file1.FileName;
                string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上一层目录
                ini_name = path1.Substring(path1.LastIndexOf("\\") + 1, (path1.Length - path1.LastIndexOf("\\") - 5));  //截取目录的上一层目录
                //下面遍历PATH1的所有文件
                //DirectoryInfo TheFolder = new DirectoryInfo(path1);
                //if (!TheFolder.Exists)
                //{
                //    return null;
                //}
                GetAppSetting.GetAppSetting_data("INI_PATH", path2);//保存本次打开路径
            }
            return path1;
        }
        /// <summary>
        ///读取文件路径,一次可以读取多个
        /// </summary>
        /// <returns>文件路径</returns>
        public OpenFileDialog INIRead_Name_PathS()
        {
            string path1 = System.Configuration.ConfigurationManager.AppSettings["INI_PATH"];  //读取上次保存的路径
            if (Directory.Exists(path1) == false)//判断路径是否存在
            {
                path1 = System.Environment.CurrentDirectory;//获取绝对路径
                path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                 //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
            }
            OpenFileDialog file1 = new OpenFileDialog(); //定义提示用户打开文件
            file1.Multiselect = true;
            file1.Filter = "INI文件|*.ini";//设置文件后缀的过滤
            if (path1 != null) { file1.InitialDirectory = path1; }//设置打开文件路径
                                                                  //获取文件夹的绝对路径
            if (file1.ShowDialog() == DialogResult.OK) //如果有选择打开文件
            {
                path1 = file1.FileName;
                string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上一层目录
                ini_name = path1.Substring(path1.LastIndexOf("\\") + 1, (path1.Length - path1.LastIndexOf("\\") - 5));  //截取目
                //下面遍历PATH1的所有文件
                //DirectoryInfo TheFolder = new DirectoryInfo(path1);
                //if (!TheFolder.Exists)
                //{
                //    return null;
                //}
                GetAppSetting.GetAppSetting_data("INI_PATH", path2);//保存本次打开路径
                return file1;
            }
            return null;
        }
        private void Read_ini_Click(object sender, EventArgs e)
        {
            results.Clear();//清空缓冲数据
            dataGridView1.Rows.Clear();//清空表格内部数据                     
            OpenFileDialog file1 = INIRead_Name_PathS();
            if (file1 == null) return;
            foreach (var ini_path1 in file1.FileNames)
            {
                int key_value = System.Convert.ToInt32(INIClass.IniReadValue("NodeEEParam", "ParamNum", null, ini_path1));
                for (int i = 1; i <= key_value; i++)
                {
                    iniData sIni = new iniData();
                    sIni.ParamName = INIClass.IniReadValue("Param" + System.Convert.ToString(i), "ParamName", null, ini_path1);
                    sIni.ParamAddress = INIClass.IniReadValue("Param" + System.Convert.ToString(i), "ParamAddress", null, ini_path1);
                    sIni.ParamValue = INIClass.IniReadValue("Param" + System.Convert.ToString(i), "ParamValue", null, ini_path1);
                    sIni.ParamLen = INIClass.IniReadValue("Param" + System.Convert.ToString(i), "ParamLen", null, ini_path1);
                    sIni.ParamType = INIClass.IniReadValue("Param" + System.Convert.ToString(i), "ParamType", null, ini_path1);
                    sIni.ParamProp = INIClass.IniReadValue("Param" + System.Convert.ToString(i), "ParamProp", null, ini_path1);
                    dataGridView1.Rows.Add(sIni.ParamName, sIni.ParamAddress, sIni.ParamValue, sIni.ParamLen, sIni.ParamType, sIni.ParamProp);
                    results.Add(sIni);
                }
            }
            //dataGridView1.DataSource = results;
            //dataGridView1.Refresh();
            //cmb_Temp.DataSource = dataGridView1;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            string path1 = System.Configuration.ConfigurationManager.AppSettings["INI_PATH"];  //读取上次保存的路径
            if (Directory.Exists(path1) == false)//判断路径是否存在
            {
                path1 = System.Environment.CurrentDirectory;//获取绝对路径
                path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                 //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
            }
            path1 += "\\" + ini_name + "_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";//hhmmss
            IO_Operate.File_creation(ref path1, true);//判定文件路径并根据选择判定是否重新构建文件

            results.Clear();
            foreach (DataGridViewRow dr in dataGridView1.Rows)
            {
                if (System.Convert.ToString(dr.Cells[0].Value) != "")
                {
                    iniData sIni = new iniData();
                    int i = 0;
                    sIni.ParamName = System.Convert.ToString(dr.Cells[i++].Value);
                    sIni.ParamAddress = System.Convert.ToString(dr.Cells[i++].Value);
                    sIni.ParamValue = System.Convert.ToString(dr.Cells[i++].Value);
                    sIni.ParamLen = System.Convert.ToString(dr.Cells[i++].Value);
                    sIni.ParamType = System.Convert.ToString(dr.Cells[i++].Value);
                    sIni.ParamProp = System.Convert.ToString(dr.Cells[i++].Value);

                    results.Add(sIni);
                }

            }


            using (var stream = File.Open(path1, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                //csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.RegisterClassMap<iniDataMap>();
                csv.WriteRecords(results);
                csv.Flush();
                csv.Dispose();
            }
            MessageBox.Show("数据转换完成");
            buttonxx.Enabled = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //dataGridView1.c
            results.Clear();//清空缓冲数据
            dataGridView1.Rows.Clear();//清空表格内部数据
            string file_name = Read_Name_Path();//需要读取的文件名
            if (file_name == null) return;
            using (var reader = new StreamReader(file_name, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                results = csv.GetRecords<iniData>().ToList();
            }

            foreach (var sIni in results)
            {//遍历所有数据并显示
                dataGridView1.Rows.Add(sIni.ParamName, sIni.ParamAddress, sIni.ParamValue, sIni.ParamLen, sIni.ParamType, sIni.ParamProp);
            }
            //dataGridView1.DataSource = results;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            string path1 = System.Configuration.ConfigurationManager.AppSettings["INI_PATH"];  //读取上次保存的路径
            if (Directory.Exists(path1) == false)//判断路径是否存在
            {
                path1 = System.Environment.CurrentDirectory;//获取绝对路径
                path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                 //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
            }
            path1 += "\\" + ini_name + "_" + DateTime.Now.ToString("yyyyMMdd") + ".ini";//hhmmss
            IO_Operate.File_creation(ref path1, true);//判定文件路径并根据选择判定是否重新构建文件
            results.Clear();
            foreach (DataGridViewRow dr in dataGridView1.Rows)
            {
                if (System.Convert.ToString(dr.Cells[0].Value) != "")
                {
                    iniData sIni = new iniData();
                    int i = 0;
                    sIni.ParamName = System.Convert.ToString(dr.Cells[i++].Value);
                    sIni.ParamAddress = System.Convert.ToString(dr.Cells[i++].Value);
                    sIni.ParamValue = System.Convert.ToString(dr.Cells[i++].Value);
                    sIni.ParamLen = System.Convert.ToString(dr.Cells[i++].Value);
                    sIni.ParamType = System.Convert.ToString(dr.Cells[i++].Value);
                    sIni.ParamProp = System.Convert.ToString(dr.Cells[i++].Value);

                    results.Add(sIni);
                }

            }

            //IniWriteValue
            INIClass.IniWriteValue("NodeEEParam", "ParamNum", System.Convert.ToString(results.Count()), path1);
            for (int i = 0; i < results.Count(); i++)
            {
                string section = "Param" + System.Convert.ToString(i + 1);
                INIClass.IniWriteValue(section, "ParamName", coded_system.gb2312_utf8(results[i].ParamName), path1);
                string sss = coded_system.gb2312_utf8(results[i].ParamName);
                INIClass.IniWriteValue(section, "ParamAddress", results[i].ParamAddress, path1);
                INIClass.IniWriteValue(section, "ParamValue", results[i].ParamValue, path1);
                INIClass.IniWriteValue(section, "ParamLen", results[i].ParamLen, path1);
                INIClass.IniWriteValue(section, "ParamType", results[i].ParamType, path1);
                INIClass.IniWriteValue(section, "ParamProp", results[i].ParamProp, path1);
            }
            MessageBox.Show("数据转换完成");
            buttonxx.Enabled = true;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //测试继承
            Inheritance.sss();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            if (auto_time.Checked)
            {
                CSV_Read_Write.Read_data_Autotime(Count_Random.Checked, auto_time.Checked);
            }
            else
            {
                CSV_Read_Write.Read_data(Count_Random.Checked, auto_time.Checked);
            }

            MessageBox.Show("数据转换完成");
            buttonxx.Enabled = true;
        }

        private void set_winTIME_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 1; i++)
            {
                set_win_time.TIME();
            }
        }

        //调用这个函数会对几个下拉全部清空，并且重新写入Items(下拉列表)
        void GetPort()
        {
            String[] Str2 = System.IO.Ports.SerialPort.GetPortNames();//第二中方法，直接取得串口值
            //获得当前子健存在的健值
            int i;
            Serial_Port.Items.Clear();  //移除所有项
            for (i = 0; i < Str2.Length; i++)
            {
                Serial_Port.Items.Add(Str2[i]);
            }
        }

        private void Serial_Open_Click(object sender, EventArgs e)
        {
            //打开或者关闭串口操作
            string s1 = "打开串口";
            if (s1 == Serial_Open.Text) //当串口内部为打开串口则是打开串口
            {
                /***************异常处理！****************/
                try
                {
                    serialPort1.Close(); //关闭串口
                    serialPort1.PortName = Convert.ToString(Serial_Port.Text);  //获取或设置 ComboBox 中当前选定的项。
                    serialPort1.BaudRate = 115200;
                    serialPort1.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误：" + ex.Message);
                }
                /*****************************************/
                if (serialPort1.IsOpen)//串口打开成功
                {
                    Serial_Open.Text = "关闭串口";
                    tsStatus.Text = "串口打开";
                    tsStatus.Update();
                    //string s2 = comboBox1.Text;//获取窗口显示文本。
                    GetAppSetting.GetAppSetting_data("com", Serial_Port.Text);
                }
            }
            else
            {//关闭串口
                /***************异常处理！****************/
                try
                {
                    serialPort1.Close(); //关闭串口
                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误：" + ex.Message);
                }
                /*****************************************/
                if (!serialPort1.IsOpen)//串口关闭成功
                {
                    Serial_Open.Text = "打开串口";
                    tsStatus.Text = "串口关闭成功";
                    tsStatus.Update();
                }
            }
        }

        private void Display_DATA_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            byte[] SendByte = new byte[2500];
            i = 0;
            curve1.label1.Text = "温度：";
            curve1.label2.Text = "MPPC高压";
            curve1.label3.Text = "MPPC高压设定值：";
            curve1.label4.Text = "CY5：";
            curve1.chart1.Series[0].LegendText = "温度值x";
            curve1.chart1.Series[1].LegendText = "MPPC高压";
            curve1.chart1.Series[2].LegendText = "MPPC高压设定值";
            curve1.chart1.Series[3].LegendText = "CY5";
            if (buttonxx.Text == "显示温度")
            {

                buttonxx.Text = "隐藏温度";

                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0xA0;
                SendByte[i++] = 0x01;
                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; ; }

                try
                {
                    curve1.Show();

                }
                catch
                {
                    curve1 = new curve();
                    curve1.Show();
                }
                shuju_max = 0;
                shuju_min = 4000000;
                i = 0;
                curve1.chart1.Series[0].Points.Clear();
                curve1.chart1.Series[1].Points.Clear();
                curve1.chart1.Series[2].Points.Clear();
                curve1.chart1.ChartAreas[0].AxisX.ScaleView.Position = 0;
                curve1.chart1.ChartAreas[0].AxisX.ScaleView.Size = double.NaN; // winnum;//视野范围内共有多少个数据点

                curve1.chart1.ChartAreas[0].AxisY.ScaleView.Position = 0;
                curve1.chart1.ChartAreas[0].AxisY.ScaleView.Size = double.NaN;
                curver_en = 1;
                show_i = combobox_show_i.SelectedIndex;
                curve_x = 0;
                //进行数据保存
                data_path = System.Environment.CurrentDirectory;//获取绝对路径
                                                                // data_path = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录

                System.DateTime currentTime = new System.DateTime();
                currentTime = System.DateTime.Now;
                data_path += "\\" + currentTime.ToString("yyMMddHHmmss") + ".csv";
                IO_Operate.File_creation(ref data_path, true);//判定文件路径并根据选择判定是否重新构建文件
                using (var stream = File.Open(data_path, FileMode.Append))
                using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                {
                    // Don't write the header again. csv.WriteComment(writer);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteField("FAM温度,FAM设置高压,VIC温度,VIC设置高压,ROX温度,ROX设置高压,CY5温度,CY5设置高压\r\n", false);
                }
            }
            else
            {
                buttonxx.Text = "显示温度";
                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0xA0;
                SendByte[i++] = 0x00;
                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";

                }
                catch { tsStatus.Text = "写入失败!"; ; }
                try
                {
                    curve1.Hide();
                }
                catch
                {
                    curve1 = new curve();
                    curve1.Hide();
                }
                curver_en = 0;
            }
            buttonxx.Enabled = true;
        }

        private void dataGridView1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
        private void PasteData()
        {
            string clipboardText = Clipboard.GetText(); //获取剪贴板中的内容
            if (string.IsNullOrEmpty(clipboardText))
            {
                return;
            }
            int colnum = 0;
            int rownum = 0;
            for (int i = 0; i < clipboardText.Length; i++)
            {
                if (clipboardText.Substring(i, 1) == "\t")
                {
                    colnum++;
                }
                if (clipboardText.Substring(i, 1) == "\n")
                {
                    rownum++;
                }
            }
            colnum = colnum / rownum + 1;
            int selectedRowIndex, selectedColIndex;
            selectedRowIndex = this.dataGridView1.CurrentRow.Index;
            selectedColIndex = this.dataGridView1.CurrentCell.ColumnIndex;
            if (selectedRowIndex + rownum > dataGridView1.RowCount || selectedColIndex + colnum > dataGridView1.ColumnCount)
            {
                MessageBox.Show("粘贴区域大小不一致");
                return;
            }
            String[][] temp = new String[rownum][];
            for (int i = 0; i < rownum; i++)
            {
                temp[i] = new String[colnum];
            }
            int m = 0, n = 0, len = 0;
            while (len != clipboardText.Length)
            {
                String str = clipboardText.Substring(len, 1);
                if (str == "\t")
                {
                    n++;
                }
                else if (str == "\n")
                {
                    m++;
                    n = 0;
                }
                else
                {
                    temp[m][n] += str;
                }
                len++;
            }
            for (int i = selectedRowIndex; i < selectedRowIndex + rownum; i++)
            {
                for (int j = selectedColIndex; j < selectedColIndex + colnum; j++)
                {
                    this.dataGridView1.Rows[i].Cells[j].Value = temp[i - selectedRowIndex][j - selectedColIndex];
                }
            }
        }

        private void FAM_GET_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            byte[] SendByte = new byte[2500];
            int i = 0;
            SendByte[i++] = 0x5A;
            SendByte[i++] = 0xA5;  //前两个是开始头

            SendByte[i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[i++] = 0x01;// 0x11; //低8位

            SendByte[i++] = 0x01;  //目标模块
            SendByte[i++] = 0xA1;//A1设置or读取MPPC参数
            SendByte[i++] = 0x00;//01设置  orter 读取参数
            int Data_Length = 304;
            int Offset_Address = 0;
            SendByte[i++] = (byte)(Offset_Address >> 8); //偏移地址高8位
            SendByte[i++] = (byte)(Offset_Address); //偏移地址低8位
            SendByte[i++] = (byte)(Data_Length >> 8); //偏移地址高8位
            SendByte[i++] = (byte)(Data_Length); //偏移地址低8位

            SendByte[i++] = 0xAA;  //最后连个是数据尾
            SendByte[i++] = 0xBB;
            SendByte[2] = (byte)((i - 6) >> 8);
            SendByte[3] = (byte)(i - 6);
            //发送数据,返回正常发送多少个字节
            try
            {
                serialPort1.Write(SendByte, 0, i);
                tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
            }
            catch { tsStatus.Text = "写入失败!"; }
            buttonxx.Enabled = true;
        }

        private void FAM_SET_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            byte[] SendByte = new byte[2500];
            int i = 0;
            SendByte[i++] = 0x5A;
            SendByte[i++] = 0xA5;  //前两个是开始头

            SendByte[i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[i++] = 0x01;// 0x11; //低8位

            SendByte[i++] = 0x01;  //目标模块
            SendByte[i++] = 0xA1;//A1设置or读取MPPC参数
            SendByte[i++] = 0x01;//01设置  orter 读取参数
            int Data_Length = 304;
            int Offset_Address = 0;
            SendByte[i++] = (byte)(Offset_Address >> 8); //偏移地址高8位
            SendByte[i++] = (byte)(Offset_Address); //偏移地址低8位
            SendByte[i++] = (byte)(Data_Length >> 8); //偏移地址高8位
            SendByte[i++] = (byte)(Data_Length); //偏移地址低8位
            float float_value;
            int TextBox_Gather_i;
            int TextBox_Gather_j;
            for (TextBox_Gather_j = 0; TextBox_Gather_j < 4; TextBox_Gather_j++)
            {
                for (TextBox_Gather_i = 0; TextBox_Gather_i < 12; TextBox_Gather_i++)
                {
                    float.TryParse(TextBox_Gather[TextBox_Gather_j, TextBox_Gather_i].Text, out float_value);
                    Array.ConstrainedCopy(BitConverter.GetBytes(float_value), 0, SendByte, i, 4);
                    i = i + 4;
                }
            }
            for (TextBox_Gather_j = 0; TextBox_Gather_j < 4; TextBox_Gather_j++)
            {
                for (TextBox_Gather_i = 0; TextBox_Gather_i < 7; TextBox_Gather_i++)
                {
                    float.TryParse(TextBox_LED[TextBox_Gather_j, TextBox_Gather_i].Text, out float_value);
                    Array.ConstrainedCopy(BitConverter.GetBytes(float_value), 0, SendByte, i, 4);
                    i = i + 4;
                }
            }
            SendByte[i++] = 0xAA;  //最后连个是数据尾
            SendByte[i++] = 0xBB;
            SendByte[2] = (byte)((i - 6) >> 8);
            SendByte[3] = (byte)(i - 6);
            //发送数据,返回正常发送多少个字节
            try
            {
                serialPort1.Write(SendByte, 0, i);
                tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
            }
            catch { tsStatus.Text = "写入失败!"; }
            MppcSetGerData_CSV_Write("");//保存数据
            buttonxx.Enabled = true;
        }

        private void Serial_Send_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            byte[] SendByte = new byte[2500];
            int i = 0;
            SendByte[i++] = 0x5A;
            SendByte[i++] = 0xA5;  //前两个是开始头

            SendByte[i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[i++] = 0x01;// 0x11; //低8位

            SendByte[i++] = 0x01;  //目标模块
            SendByte[i++] = 0xA2;//A2查询adc读数
            SendByte[i++] = 0x01;//01:ADC1 03:ADC3  orter 读取参数
            SendByte[i++] = 0xAA;  //最后连个是数据尾
            SendByte[i++] = 0xBB;
            SendByte[2] = (byte)((i - 6) >> 8);
            SendByte[3] = (byte)(i - 6);
            //发送数据,返回正常发送多少个字节
            try
            {
                serialPort1.Write(SendByte, 0, i);
                tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
            }
            catch { tsStatus.Text = "写入失败!"; }
            buttonxx.Enabled = true;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            byte[] SendByte = new byte[2500];
            i = 0;
            if (buttonxx.Text == "开始输出")
            {

                buttonxx.Text = "关闭输出";

                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0xA4;
                SendByte[i++] = 0x01;
                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; ; }

                //t_Task = Task.Factory.StartNew(() =>
                //{
                //    Thread.Sleep(10000);//延时20ms 
                //    InvokeToForm(() =>
                //    {
                Serial_Data.AppendText(DM6500_show_achieve.textBox1.Text + "\r\n");
                buttonxx.Enabled = true;
                //    });

                //}).ContinueWith(x =>
                //{
                //    if (x.IsFaulted)
                //    {
                //        MessageBox.Show(this, x.Exception.InnerException.Message);
                //    }
                //});
            }
            else
            {
                buttonxx.Text = "开始输出";
                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0xA4;
                SendByte[i++] = 0x00;
                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";

                }
                catch { tsStatus.Text = "写入失败!"; ; }
                buttonxx.Enabled = true;
            }
            //buttonxx.Enabled = true;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            Serial_Data.Text = "";       
        }

        private void DAC_SET_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            byte[] SendByte = new byte[2500];
            int i = 0;
            SendByte[i++] = 0x5A;
            SendByte[i++] = 0xA5;  //前两个是开始头

            SendByte[i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[i++] = 0x01;// 0x11; //低8位

            SendByte[i++] = 0x01;  //目标模块
            SendByte[i++] = 0xA3;//A2查询adc读数
            int TextBox_Gather_i;
            UInt16 tempx;
            for (TextBox_Gather_i = 0; TextBox_Gather_i < 8; TextBox_Gather_i++)
            {
                //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                tempx = (System.Convert.ToUInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text));
                SendByte[i++] = (byte)(tempx >> 8);
                SendByte[i++] = (byte)(tempx);
            }


            SendByte[i++] = 0xAA;  //最后连个是数据尾
            SendByte[i++] = 0xBB;
            SendByte[2] = (byte)((i - 6) >> 8);
            SendByte[3] = (byte)(i - 6);
            //发送数据,返回正常发送多少个字节
            try
            {
                serialPort1.Write(SendByte, 0, i);
                tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
            }
            catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
            //将设置电流写入发送电流框
            for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
            {
                //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                double tempxx = (System.Convert.ToDouble(textBoxe_DAC_SET[TextBox_Gather_i + 4].Text));
                tempxx = tempxx / 65536 * 1.25 / 10 * 1000;
                TextBox_LED[TextBox_Gather_i, 0].Text = tempxx.ToString();
            }
            //tempx = (System.Convert.ToUInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text));
            buttonxx.Enabled = true;
        }

        private void Query_temp_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            byte[] SendByte = new byte[2500];
            int i = 0;
            SendByte[i++] = 0x5A;
            SendByte[i++] = 0xA5;  //前两个是开始头

            SendByte[i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[i++] = 0x01;// 0x11; //低8位

            SendByte[i++] = 0x01;  //目标模块
            SendByte[i++] = 0xA5;//A1设置or读取MPPC参数

            SendByte[i++] = 0xAA;  //最后连个是数据尾
            SendByte[i++] = 0xBB;
            SendByte[2] = (byte)((i - 6) >> 8);
            SendByte[3] = (byte)(i - 6);
            //发送数据,返回正常发送多少个字节
            try
            {
                serialPort1.Write(SendByte, 0, i);
                tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
            }
            catch { tsStatus.Text = "写入失败!"; }
            buttonxx.Enabled = true;
        }

        private void button13_Click_1(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            byte[] SendByte = new byte[2500];
            i = 0;
            if (buttonxx.Text == "显示数据")
            {

                buttonxx.Text = "隐藏数据";
                try
                {
                    curve1.Show();

                }
                catch
                {
                    curve1 = new curve();
                    curve1.Show();
                }
                shuju_max = 0;
                shuju_min = 4000000;
                i = 0;
                curve1.chart1.Series[0].Points.Clear();
                curve1.chart1.Series[1].Points.Clear();
                curve1.chart1.Series[2].Points.Clear();
                curve1.chart1.ChartAreas[0].AxisX.ScaleView.Position = 0;
                curve1.chart1.ChartAreas[0].AxisX.ScaleView.Size = double.NaN; // winnum;//视野范围内共有多少个数据点

                curve1.chart1.ChartAreas[0].AxisY.ScaleView.Position = 0;
                curve1.chart1.ChartAreas[0].AxisY.ScaleView.Size = double.NaN;
                curver_en = 1;
                show_i = Temp_show_i.SelectedIndex;
                if (Temp_show_i.Text == "M_ALL")
                {
                    curve1.label1.Text = "M_FAM：";
                    curve1.label2.Text = "M_VIC";
                    curve1.label3.Text = "M_ROX：";
                    curve1.label4.Text = "M_CY5：";
                    curve1.chart1.Series[0].LegendText = "M_FAM";
                    curve1.chart1.Series[1].LegendText = "M_VIC";
                    curve1.chart1.Series[2].LegendText = "M_ROX";
                    curve1.chart1.Series[3].LegendText = "M_CY5";
                    curve_x = 2;
                }
                else if (Temp_show_i.Text == "L_ALL")
                {
                    curve1.label1.Text = "L_FAM：";
                    curve1.label2.Text = "L_VIC";
                    curve1.label3.Text = "L_ROX：";
                    curve1.label4.Text = "L_CY5：";
                    curve1.chart1.Series[0].LegendText = "L_FAM";
                    curve1.chart1.Series[1].LegendText = "L_VIC";
                    curve1.chart1.Series[2].LegendText = "L_ROX";
                    curve1.chart1.Series[3].LegendText = "L_CY5";
                    curve_x = 3;
                }
                else
                {
                    curve1.label1.Text = "原始数据：";
                    curve1.label2.Text = "滤波数据";
                    curve1.label3.Text = "卡尔曼滤波：";
                    curve1.label4.Text = "L_CY5：";
                    curve1.chart1.Series[0].LegendText = "原始数据";
                    curve1.chart1.Series[1].LegendText = "滤波数据";
                    curve1.chart1.Series[2].LegendText = "卡尔曼滤波";
                    curve1.chart1.Series[3].LegendText = "L_CY5";
                    curve_x = 1;
                }

                data_path = System.Environment.CurrentDirectory;//获取绝对路径
                                                                // data_path = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录

                System.DateTime currentTime = new System.DateTime();
                currentTime = System.DateTime.Now;
                data_path += "\\" + currentTime.ToString("yyMMddHHmmss") + ".csv";
                IO_Operate.File_creation(ref data_path, true);//判定文件路径并根据选择判定是否重新构建文件
                using (var stream = File.Open(data_path, FileMode.Append))
                using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                {
                    // Don't write the header again. csv.WriteComment(writer);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteField("FAM原始温度,FAM第一级滤波,FAM卡尔曼滤波,VIC原始温度,VIC第一级滤波,VIC卡尔曼滤波,ROX原始温度,ROX第一级滤波,ROX卡尔曼滤波,CY5原始温度,CY5第一级滤波,CY5卡尔曼滤波,L_FAM原始温度,L_FAM第一级滤波,L_FAM卡尔曼滤波,L_VIC原始温度,L_VIC第一级滤波,L_VIC卡尔曼滤波,L_ROX原始温度,L_ROX第一级滤波,L_ROX卡尔曼滤波,L_CY5原始温度,L_CY5第一级滤波,L_CY5卡尔曼滤波,\r\n", false);
                }
            }
            else
            {
                buttonxx.Text = "显示数据";
                try
                {
                    curve1.Hide();
                }
                catch
                {
                    curve1 = new curve();
                    curve1.Hide();
                }
                curver_en = 0;
            }
            buttonxx.Enabled = true;
        }

        private void lightHeaadNumber_SelectedIndexChanged(object sender, EventArgs e)
        {//光头改变
            var mppc_s = Mppc_results.Where(sx => sx.LightHeaadNumber == int.Parse(lightHeaadNumber.Text)).ToList();
            if (mppc_s.Count >= 1)
            {
                mppc_s.Sort((x, y) =>
                {
                    if (System.Convert.ToInt32(x.Number) > (System.Convert.ToInt32(y.Number)))
                    { return 1; }
                    else
                    { return -1; }
                });//从小到大排序
                   // mppc_s[mppc_s.Count - 1].Number =1;
                var mppc_new = Mppc_results.Where(sx => sx.Number == mppc_s[mppc_s.Count - 1].Number).ToList();
                int TextBox_Gather_i = 0;
                int TextBox_Gather_j = 0;
                for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
                {
                    var mppc_sx = mppc_new.Where(sx => sx.FluorescenceChannel == FluorescenceChannel[TextBox_Gather_i]).ToList();
                    TextBox_Gather_j = 0;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Vbr.ToString();//FAM_Vbr;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Vov.ToString();//FAM_Vov;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Tvop.ToString();//FAM_Tvop;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Temp_K2.ToString();//FAM_Temp_K2;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Temp_K1.ToString();//FAM_Temp_K1;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Temp_B.ToString();//FAM_Temp_B;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].DAC_K2.ToString();//FAM_DAC_K2;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].DAC_K1.ToString();//FAM_DAC_K1;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].DAC_B.ToString();//FAM_DAC_B;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].ADC_K2.ToString();//FAM_ADC_K2;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].ADC_K1.ToString();//FAM_ADC_K1;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].ADC_B.ToString();//FAM_ADC_B;
                    TextBox_Gather_j = 0;
                    TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_Vi.ToString();
                    //TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_K2.ToString();
                    //TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_K1.ToString();
                    //TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_B.ToString();
                    //TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].I_K2.ToString();
                    //TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].I_K1.ToString();
                    //TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].I_B.ToString();
                }
            }

        }

        private void PCBNumber_SelectedIndexChanged(object sender, EventArgs e)
        {//PCB电路板改变
            var mppc_s = Mppc_results.Where(sx => sx.PCBNumber == int.Parse(PCBNumber.Text)).ToList();
            if (mppc_s.Count >= 1)
            {
                mppc_s.Sort((x, y) =>
                {
                    if (System.Convert.ToInt32(x.Number) > (System.Convert.ToInt32(y.Number)))
                    { return 1; }
                    else
                    { return -1; }
                });//从小到大排序
                   // mppc_s[mppc_s.Count - 1].Number =1;
                var mppc_new = Mppc_results.Where(sx => sx.Number == mppc_s[mppc_s.Count - 1].Number).ToList();
                int TextBox_Gather_i = 0;
                int TextBox_Gather_j = 0;
                for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
                {
                    var mppc_sx = mppc_new.Where(sx => sx.FluorescenceChannel == FluorescenceChannel[TextBox_Gather_i]).ToList();
                    TextBox_Gather_j = 3;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Vbr.ToString();//FAM_Vbr;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Vov.ToString();//FAM_Vov;
                    //TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Tvop.ToString();//FAM_Tvop;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Temp_K2.ToString();//FAM_Temp_K2;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Temp_K1.ToString();//FAM_Temp_K1;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].Temp_B.ToString();//FAM_Temp_B;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].DAC_K2.ToString();//FAM_DAC_K2;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].DAC_K1.ToString();//FAM_DAC_K1;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].DAC_B.ToString();//FAM_DAC_B;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].ADC_K2.ToString();//FAM_ADC_K2;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].ADC_K1.ToString();//FAM_ADC_K1;
                    TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].ADC_B.ToString();//FAM_ADC_B;
                    TextBox_Gather_j = 1;
                    //TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_Vi.ToString();
                    TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_K2.ToString();
                    TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_K1.ToString();
                    TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].LED_B.ToString();
                    TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].I_K2.ToString();
                    TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].I_K1.ToString();
                    TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text = mppc_sx[0].I_B.ToString();
                }
            }

        }

        private void Add_pcb_number_Click(object sender, EventArgs e)
        {
            var mppc_list = Mppc_results.Select(t => t.PCBNumber).Distinct().ToList();
            mppc_list.Sort((x, y) =>
            {
                if (x > y)
                { return -1; }
                else
                { return 1; }
            });
            int pcb_number = mppc_list[0] + 1;
            mppc_list = Mppc_results.Select(t => t.Number).Distinct().ToList();
            mppc_list.Sort((x, y) =>
            {
                if (x > y)
                { return -1; }
                else
                { return 1; }
            });
            int number = mppc_list[0] + 1;
            int TextBox_Gather_i = 0;
            int TextBox_Gather_j = 0;
            for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
            {
                MppcSetGetData mppc_data = new MppcSetGetData();
                mppc_data.Number = number;// pcb_numberint.Parse(Number.Text);
                mppc_data.LightHeaadNumber = int.Parse(lightHeaadNumber.Text);
                mppc_data.PCBNumber = pcb_number;// int.Parse(PCBNumber.Text);
                mppc_data.FluorescenceChannel = FluorescenceChannel[TextBox_Gather_i];
                TextBox_Gather_j = 0;
                mppc_data.Vbr = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Vov = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Tvop = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Temp_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Temp_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Temp_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.DAC_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.DAC_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.DAC_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.ADC_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.ADC_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.ADC_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                TextBox_Gather_j = 0;
                mppc_data.LED_Vi = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.LED_K2 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.LED_K1 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.LED_B = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.I_K2 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.I_K1 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.I_B = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                Mppc_results.Add(mppc_data);
            }
            Number.Text = number.ToString().PadLeft(5, '0');
            PCBNumber.Text = pcb_number.ToString().PadLeft(5, '0');
        }

        private void Add_lightHeaad_number_Click(object sender, EventArgs e)
        {
            var mppc_list = Mppc_results.Select(t => t.LightHeaadNumber).Distinct().ToList();
            mppc_list.Sort((x, y) =>
            {
                if (x > y)
                { return -1; }
                else
                { return 1; }
            });
            int LightHeaadNumber = mppc_list[0] + 1;
            mppc_list = Mppc_results.Select(t => t.Number).Distinct().ToList();
            mppc_list.Sort((x, y) =>
            {
                if (x > y)
                { return -1; }
                else
                { return 1; }
            });
            int number = mppc_list[0] + 1;
            int TextBox_Gather_i = 0;
            int TextBox_Gather_j = 0;
            for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
            {
                MppcSetGetData mppc_data = new MppcSetGetData();
                mppc_data.Number = number;// pcb_numberint.Parse(Number.Text);
                mppc_data.LightHeaadNumber = LightHeaadNumber;// int.Parse(lightHeaadNumber.Text);
                mppc_data.PCBNumber = int.Parse(PCBNumber.Text);
                mppc_data.FluorescenceChannel = FluorescenceChannel[TextBox_Gather_i];
                TextBox_Gather_j = 0;
                mppc_data.Vbr = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Vov = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Tvop = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Temp_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Temp_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Temp_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.DAC_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.DAC_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.DAC_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.ADC_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.ADC_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.ADC_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                TextBox_Gather_j = 0;
                mppc_data.LED_Vi = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.LED_K2 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.LED_K1 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.LED_B = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.I_K2 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.I_K1 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.I_B = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                Mppc_results.Add(mppc_data);
            }
            Number.Text = number.ToString().PadLeft(5, '0');
            lightHeaadNumber.Text = LightHeaadNumber.ToString().PadLeft(5, '0');
        }
        public void MppcSetGerData_CSV_Write(string s)
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            var mppc_list = Mppc_results.Select(t => t.Number).Distinct().ToList();
            mppc_list.Sort((x, y) =>
            {
                if (x > y)
                { return -1; }
                else
                { return 1; }
            });
            int number = mppc_list[0] + 1;
            Number.Text = number.ToString().PadLeft(5, '0');
            int TextBox_Gather_i = 0;
            int TextBox_Gather_j = 0;
            for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
            {
                MppcSetGetData mppc_data = new MppcSetGetData();
                mppc_data.Number = int.Parse(Number.Text);
                mppc_data.Time = s + currentTime.ToString("yyMMddHHmmss");
                mppc_data.LightHeaadNumber = int.Parse(lightHeaadNumber.Text);
                mppc_data.PCBNumber = int.Parse(PCBNumber.Text);
                mppc_data.FluorescenceChannel = FluorescenceChannel[TextBox_Gather_i];
                TextBox_Gather_j = 0;
                mppc_data.Vbr = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Vov = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Tvop = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Temp_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Temp_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.Temp_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.DAC_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.DAC_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.DAC_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.ADC_K2 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.ADC_K1 = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.ADC_B = float.Parse(TextBox_Gather[TextBox_Gather_i, TextBox_Gather_j++].Text);
                TextBox_Gather_j = 0;
                mppc_data.LED_Vi = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.LED_K2 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.LED_K1 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.LED_B = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.I_K2 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.I_K1 = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                mppc_data.I_B = float.Parse(TextBox_LED[TextBox_Gather_i, TextBox_Gather_j++].Text);
                Mppc_results.Add(mppc_data);
                
                Mppc_results.Sort((x, y) =>
                {
                    if (x.Number > y.Number)
                    { return 1; }
                    else if (x.Number == y.Number) //
                    {
                        int xt = 0, yt = 0;
                        switch (x.FluorescenceChannel)
                        {
                            case "FAM":
                                xt = 0;
                                break;
                            case "VIC":
                                xt = 1;
                                break;
                            case "ROX":
                                xt = 2;
                                break;
                            case "CY5":
                                xt = 3;
                                break;
                            default:
                                yt = 0;
                                break;
                        }
                        switch (y.FluorescenceChannel)
                        {
                            case "FAM":
                                yt = 0;
                                break;
                            case "VIC":
                                yt = 1;
                                break;
                            case "ROX":
                                yt = 2;
                                break;
                            case "CY5":
                                yt = 3;
                                break;
                            default:
                                yt = 0;
                                break;
                        }
                        if (xt > yt)//string.Compare(x.FluorescenceChannel,y.FluorescenceChannel)
                        {
                            return 1;
                        }
                        else { return -1; }
                    }
                    else
                    { return -1; }
                });
                
                            
            }
            string path = System.Environment.CurrentDirectory;//获取绝对路径
                                                              //path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                              //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
            path += "\\" + "MPPC_GET_SET_DATA.csv";
            IO_Operate.File_creation(ref path, true);//判定文件路径并根据选择判定是否重新构建文件
            using (var stream = File.Open(path, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                //csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.RegisterClassMap<MppcSetGetDataMap>();
                csv.WriteRecords(Mppc_results);
                csv.Flush();
                csv.Dispose();
            }
            MppcSetGerData_CSV_Write_Only(Mppc_results);

        }
        public void MppcSetGerData_CSV_Write_Only(List<MppcSetGetData> Mppc_results_Onlys)
        {
            List<MppcSetGetData> Mppc_results_Only =new List<MppcSetGetData>(Mppc_results_Onlys);
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            List<MppcSetGetData> Mppc_results_Only_one = new List<MppcSetGetData>(); 
            while (Mppc_results_Only.Count > 0)
            {
                var sDatax = Mppc_results_Only.Where(s => s.LightHeaadNumber == Mppc_results_Only[0].LightHeaadNumber && s.PCBNumber == Mppc_results_Only[0].PCBNumber && s.FluorescenceChannel == Mppc_results_Only[0].FluorescenceChannel).ToList();
                sDatax.Sort((x, y) =>
                {
                    if (x.Number > y.Number)
                    { return -1; }//从大到小排序
                    else
                    { return 1; }
                });
                MppcSetGetData sdata_t = sDatax.Find(s => s.Number == sDatax[0].Number);
                Mppc_results_Only_one.Add(sdata_t);
                Mppc_results_Only.RemoveAll(s => s.LightHeaadNumber == sDatax[0].LightHeaadNumber && s.PCBNumber == sDatax[0].PCBNumber && s.FluorescenceChannel == sDatax[0].FluorescenceChannel);
            }


            string path = System.Environment.CurrentDirectory;//获取绝对路径
                                                              //path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
                                                              //string path2 = path1.Substring(0, path1.LastIndexOf("\\"));  //截取目录的上
            path += "\\" + "MPPC_GET_SET_DATA_Only.csv";
            Mppc_results_Only_one.Sort((x, y) =>
            {
                if (x.Number > y.Number)
                { return 1; }
                else if (x.Number == y.Number) //
                {
                    int xt = 0, yt = 0;
                    switch (x.FluorescenceChannel)
                    {
                        case "FAM":
                            xt = 0;
                            break;
                        case "VIC":
                            xt = 1;
                            break;
                        case "ROX":
                            xt = 2;
                            break;
                        case "CY5":
                            xt = 3;
                            break;
                        default:
                            yt = 0;
                            break;
                    }
                    switch (y.FluorescenceChannel)
                    {
                        case "FAM":
                            yt = 0;
                            break;
                        case "VIC":
                            yt = 1;
                            break;
                        case "ROX":
                            yt = 2;
                            break;
                        case "CY5":
                            yt = 3;
                            break;
                        default:
                            yt = 0;
                            break;
                    }
                    if (xt > yt)//string.Compare(x.FluorescenceChannel,y.FluorescenceChannel)
                    {
                        return 1;
                    }
                    else { return -1; }
                }
                else
                { return -1; }
            });
            IO_Operate.File_creation(ref path, true);//判定文件路径并根据选择判定是否重新构建文件
            using (var stream = File.Open(path, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                //csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.RegisterClassMap<MppcSetGetDataMap>();
                csv.WriteRecords(Mppc_results_Only_one);
                csv.Flush();
                csv.Dispose();
            }

        }

        private void MPPC_DAC_FAM_TextChanged(object sender, EventArgs e)
        {
            //int TextBox_Gather_i;
            //for (TextBox_Gather_i = 1; TextBox_Gather_i < 4; TextBox_Gather_i++)
            //{
            //    //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
            //    textBoxe_DAC_SET[TextBox_Gather_i].Text= textBoxe_DAC_SET[0].Text;
            // }
        }

        private void LED_DAC_FAM_TextChanged(object sender, EventArgs e)
        {
            //int TextBox_Gather_i;
            //for (TextBox_Gather_i = 5; TextBox_Gather_i < 8; TextBox_Gather_i++)
            //{
            //    //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
            //    textBoxe_DAC_SET[TextBox_Gather_i].Text = textBoxe_DAC_SET[4].Text;
            //}
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            UInt16[] MPPC_SET = new UInt16[6] { (UInt16)45000, (UInt16)46000, (UInt16)47000, (UInt16)48000, (UInt16)49000, (UInt16)0 };
            UInt16[] LED_SET = new UInt16[13] { (UInt16)0, (UInt16)5000, (UInt16)10000, (UInt16)15000, (UInt16)20000, (UInt16)25000,
                                                (UInt16)30000, (UInt16)35000, (UInt16)40000 , (UInt16)45000, (UInt16)50000, (UInt16)55000,(UInt16)0,};
            for (int MPPC_SHT_i = 0; MPPC_SHT_i < 5; MPPC_SHT_i++)
            {
                for (int LED_SET_j = 0; LED_SET_j < 12; LED_SET_j++)
                {
                    byte[] SendByte = new byte[2500];
                    int i = 0;
                    SendByte[i++] = 0x5A;
                    SendByte[i++] = 0xA5;  //前两个是开始头

                    SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                    SendByte[i++] = 0x01;// 0x11; //低8位

                    SendByte[i++] = 0x01;  //目标模块
                    SendByte[i++] = 0xA3;//A2查询adc读数
                    int TextBox_Gather_i;
                    UInt16 tempx;
                    for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
                    {
                        //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                        tempx = MPPC_SET[MPPC_SHT_i];
                        SendByte[i++] = (byte)(tempx >> 8);
                        SendByte[i++] = (byte)(tempx);
                    }
                    for (TextBox_Gather_i = 4; TextBox_Gather_i < 8; TextBox_Gather_i++)
                    {
                        //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                        tempx = LED_SET[LED_SET_j];
                        SendByte[i++] = (byte)(tempx >> 8);
                        SendByte[i++] = (byte)(tempx);
                    }

                    SendByte[i++] = 0xAA;  //最后连个是数据尾
                    SendByte[i++] = 0xBB;
                    SendByte[2] = (byte)((i - 6) >> 8);
                    SendByte[3] = (byte)(i - 6);
                    //发送数据,返回正常发送多少个字节
                    try
                    {
                        serialPort1.Write(SendByte, 0, i);
                        tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
                    }
                    catch { tsStatus.Text = "写入失败!"; }
                    Thread.Sleep(1000);//延时20ms                  
                }
            }
            {
                byte[] SendByte = new byte[2500];
                int i = 0;
                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0xA3;//A2查询adc读数
                int TextBox_Gather_i;
                UInt16 tempx;
                for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
                {
                    //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                    tempx = 0;
                    SendByte[i++] = (byte)(tempx >> 8);
                    SendByte[i++] = (byte)(tempx);
                }
                for (TextBox_Gather_i = 4; TextBox_Gather_i < 8; TextBox_Gather_i++)
                {
                    //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                    tempx = 0;
                    SendByte[i++] = (byte)(tempx >> 8);
                    SendByte[i++] = (byte)(tempx);
                }

                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; }
            }
            MessageBox.Show("设置完成");
            buttonxx.Enabled = true;
        }

        private void DAC_Calibrate_SET_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            UInt16[] MPPC_SET = new UInt16[6] { (UInt16)42000, (UInt16)44000, (UInt16)46000, (UInt16)48000, (UInt16)49000, (UInt16)0 };
            UInt16[] LED_SET = new UInt16[13] { (UInt16)0, (UInt16)5000, (UInt16)10000, (UInt16)15000, (UInt16)20000, (UInt16)25000,
                                                (UInt16)30000, (UInt16)35000, (UInt16)40000 , (UInt16)45000, (UInt16)50000, (UInt16)55000,(UInt16)0,};
            for (int MPPC_SHT_i = 0; MPPC_SHT_i < 4; MPPC_SHT_i++)
            {
                byte[] SendByte = new byte[2500];
                int i = 0;
                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0xA3;//A2查询adc读数
                int TextBox_Gather_i;
                UInt16 tempx;
                for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
                {
                    //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                    tempx = MPPC_SET[MPPC_SHT_i];
                    SendByte[i++] = (byte)(tempx >> 8);
                    SendByte[i++] = (byte)(tempx);
                }
                for (TextBox_Gather_i = 4; TextBox_Gather_i < 8; TextBox_Gather_i++)
                {
                    //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                    tempx = (System.Convert.ToUInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text));
                    SendByte[i++] = (byte)(tempx >> 8);
                    SendByte[i++] = (byte)(tempx);
                }

                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; }
                Thread.Sleep(1000);//延时20ms                                  
            }
            {
                byte[] SendByte = new byte[2500];
                int i = 0;
                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0xA3;//A2查询adc读数
                int TextBox_Gather_i;
                UInt16 tempx;
                for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
                {
                    //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                    tempx = 0;
                    SendByte[i++] = (byte)(tempx >> 8);
                    SendByte[i++] = (byte)(tempx);
                }
                for (TextBox_Gather_i = 4; TextBox_Gather_i < 8; TextBox_Gather_i++)
                {
                    //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                    tempx = 0;
                    SendByte[i++] = (byte)(tempx >> 8);
                    SendByte[i++] = (byte)(tempx);
                }

                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; }
            }
            MessageBox.Show("校准完成");
            buttonxx.Enabled = true;
        }
        private void InvokeToForm(Action action)
        {
            try
            {
                this.Invoke(action);
            }
            catch { }
        }
        Task t = null;
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            string title = Text;
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
            content1 = PortUltility.FindAddresses(PortType.USB);
            cboUSB.ShowAndDisplay(content1);
            list1 = new List<string>();
            for (int i = 0; i < content1.Length; i++)
            {
                if (content1[i].Contains("0x1313"))
                {
                    list1.Add(content1[i]);
                }
            }
            content1 = list1.ToArray();
            cboPM100.ShowAndDisplay(content1); cboPM100.Update(); ;

            content1 = PortUltility.FindAddresses(PortType.USB);
            list1 = new List<string>();
            for (int i = 0; i < content1.Length; i++)
            {
                if (content1[i].Contains("0x6500"))
                {
                    list1.Add(content1[i]);
                }
            }
            content1 = list1.ToArray();
            cboDM6500.ShowAndDisplay(content1); cboDM6500.Update();

            content1 = PortUltility.FindAddresses(PortType.USB);
            list1 = new List<string>();
            for (int i = 0; i < content1.Length; i++)
            {
                if (content1[i].Contains("0x2230"))
                {
                    list1.Add(content1[i]);
                }
            }
            content1 = list1.ToArray();
            cbo2230G.ShowAndDisplay(content1); cbo2230G.Update();

            content1 = PortUltility.FindAddresses(PortType.GPIB);
            cboGPIB.ShowAndDisplay(content1);
            content1 = PortUltility.FindAddresses(PortType.LAN);
            cboLAN.ShowAndDisplay(content1);
            btnRefresh.Enabled = true; btnOpen.Enabled = true; Text = title; 
        }
        private PortOperatorBase _portOperatorBase;
        private bool _isWritingError = false;
        private void DoSomethingForRadioButton(out string message, params Func<string>[] actionOfRbt)
        {
            message = string.Empty;
            if (actionOfRbt.Length != 4) throw new ArgumentException();
            if (rbtRS232.Checked) message = actionOfRbt[0]();
            if (rbtUSB.Checked) message = actionOfRbt[1]();
            if (rbtGPIB.Checked) message = actionOfRbt[2]();
            if (rbtLAN.Checked) message = actionOfRbt[3]();
        }
        private bool NewPortInstance(out string message)
        {
            //typeof(SerialParity);
            bool hasAddress = false;
            bool hasException = false;
            DoSomethingForRadioButton(out message,
                () =>
                {
                    string message1 = string.Empty;
                    if (cboRS232.SelectedIndex == -1) return "没有串口选中";
                    try
                    {
                        _portOperatorBase = new RS232PortOperator(((Pair<string, string>)cboRS232.SelectedItem).Value.ToString(),
                                               (int)9600, SerialParity.None,
                                               SerialStopBitsMode.One, (int)8);
                        hasAddress = true;
                    }
                    catch (Exception e1)
                    {

                        hasException = true;
                        message1 = e1.ToString();
                    }

                    return message1;
                },
                () =>
                {
                    string message2 = string.Empty;
                    if (cboUSB.SelectedIndex == -1) return "没有USB选中";
                    try
                    {
                        _portOperatorBase = new USBPortOperator(cboUSB.SelectedItem.ToString());
                        hasAddress = true;
                    }
                    catch (Exception e1)
                    {
                        hasException = true;
                        message2 = e1.ToString();
                    }
                    return message2;
                },
                () =>
                {
                    string message3 = string.Empty;
                    if (cboGPIB.SelectedIndex == -1) return "没有GPIB选中";
                    try
                    {
                        _portOperatorBase = new GPIBPortOperator(cboGPIB.SelectedItem.ToString());
                        hasAddress = true;
                    }
                    catch (Exception e1)
                    {
                        hasException = true;
                        message3 = e1.ToString();
                    }
                    return message3;
                },
                () =>
                {
                    string message4 = string.Empty;
                    if (cboLAN.SelectedIndex == -1) return "没有LAN选中";
                    try
                    {
                        _portOperatorBase = new LANPortOperator(cboLAN.SelectedItem.ToString());
                        hasAddress = true;
                    }
                    catch (Exception e1)
                    {
                        hasException = true;
                        message4 = e1.ToString();
                    }
                    return message4;
                });
            if (!hasException && hasAddress) _portOperatorBase.Timeout = (int)2000;
            return hasAddress;
        }
        //private void BindOrRemoveDataReceivedEvent()
        //{
        //    if (_portOperatorBase is RS232PortOperator portOperator)
        //    {
        //            portOperator.DataReceived += PortOperator_DataReceived;

        //    }
        //}
        private void btnOpen_Click(object sender, EventArgs e)
        {//cboUSB.SelectedItem.ToString()
            string message, message_time;

            PM100x_ce.USBPort_Open_Close("OPEN", cboUSB.SelectedItem.ToString(), out message);
            Serial_Data.AppendText(message);
            PM100x_ce.USBPort_Write(true, "*RST", true, out message);
            Serial_Data.AppendText(message);
            //w1.Show();// 显示窗口控件
            //gpbWindows.Controls.Clear();//清空之前加载的窗口控件
            //gpbWindows.Controls.Add(w1);//加载窗口控件1
            PM100D_show_achieve.Show();
            PM100M_TAB.Controls.Clear();//清空当前窗口的窗口控件
            PM100M_TAB.Controls.Add(PM100D_show_achieve);
            PM100D_show_achieve.Shart_Show_Auto(PM100M_TAB.Size.Width, PM100M_TAB.Size.Height);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            PM100D_show_achieve.Shart_Show_Auto(PM100M_TAB.Size.Width, PM100M_TAB.Size.Height);
            DXecllence_1.Shart_Show_Auto(tabPage4.Size.Width, tabPage4.Size.Height);
        }

        private void LED_OPEN_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            byte[] SendByte = new byte[2500];
            i = 0;
            if (buttonxx.Text == "打开LED")
            {

                buttonxx.Text = "关闭LED";

                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0x08;
                SendByte[i++] = 0x01;
                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; ; }

            }
            else
            {
                buttonxx.Text = "打开LED";
                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0x08;
                SendByte[i++] = 0x00;
                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";

                }
                catch { tsStatus.Text = "写入失败!"; ; }
            }
            buttonxx.Enabled = true;
        }

        private void MPPC_OPEN_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            byte[] SendByte = new byte[2500];
            i = 0;
            if (buttonxx.Text == "打开MPPC")
            {

                buttonxx.Text = "关闭MPPC";

                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0x0d;
                SendByte[i++] = 0x01;
                SendByte[i++] = 0x01;
                SendByte[i++] = 0x01;
                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; ; }

            }
            else
            {
                buttonxx.Text = "打开MPPC";
                SendByte[i++] = 0x5A;
                SendByte[i++] = 0xA5;  //前两个是开始头

                SendByte[i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[i++] = 0x01;// 0x11; //低8位

                SendByte[i++] = 0x01;  //目标模块
                SendByte[i++] = 0x0d;
                SendByte[i++] = 0x01;
                SendByte[i++] = 0x00;
                SendByte[i++] = 0x00;
                SendByte[i++] = 0xAA;  //最后连个是数据尾
                SendByte[i++] = 0xBB;
                SendByte[2] = (byte)((i - 6) >> 8);
                SendByte[3] = (byte)(i - 6);
                //发送数据,返回正常发送多少个字节
                try
                {
                    serialPort1.Write(SendByte, 0, i);
                    tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";

                }
                catch { tsStatus.Text = "写入失败!"; ; }
            }
            buttonxx.Enabled = true;
        }
        private void PortOperator_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            if (e.BytesToRead > 0) //Read(false, e.BytesToRead)
            {
                byte[] recBuffer;//= new byte[e.BytesToRead]
                recBuffer = RS232.RS232_portOperatorBase.ReadToBytes(e.BytesToRead);
                string recData;//接收数据转码后缓存
                recData = System.Text.Encoding.Default.GetString(recBuffer);//转码
                //下面进行16进制接收
                StringBuilder recBuffer16 = new StringBuilder();//定义16进制接收缓存(可变长度)
                for (int i = 0; i < recBuffer.Length; i++)
                {
                    recBuffer16.AppendFormat("{0:X2}" + " ", recBuffer[i]);
                    // recBuffer16.AppendFormat("0X"+"{0:X2}" + " ",recBuffer[i]);
                }
                recData = recBuffer16.ToString();
                if (this.Serial_Data.InvokeRequired)
                {//这里由于进程是不一样的AppendText
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { recData });
                }
                else
                {
                    Serial_Data.AppendText(recData);
                }
            }

        }
        private void RS232_open_Click(object sender, EventArgs e)
        {
            string message, message_time;

            RS232.RS232_Open_Close("OPEN", ((Pair<string, string>)cboRS232.SelectedItem).Value.ToString(), out message);
            Serial_Data.AppendText(message);
            if (RS232.RS232_portOperatorBase is RS232PortOperator portOperator)
            {
                portOperator.DataReceived += PortOperator_DataReceived;
            }
            byte[] SendByte = new byte[2500];
            int i = 0;
            SendByte[i++] = 0x5A;
            SendByte[i++] = 0xA5;  //前两个是开始头

            SendByte[i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[i++] = 0x01;// 0x11; //低8位

            SendByte[i++] = 0x01;  //目标模块
            SendByte[i++] = 0xA2;//A2查询adc读数
            SendByte[i++] = 0x01;//01:ADC1 03:ADC3  orter 读取参数
            SendByte[i++] = 0xAA;  //最后连个是数据尾
            SendByte[i++] = 0xBB;
            SendByte[2] = (byte)((i - 6) >> 8);
            SendByte[3] = (byte)(i - 6);
            //发送数据,返回正常发送多少个字节
            try
            {
                serialPort1.Write(SendByte, 0, i);
                tsStatus.Text = System.Convert.ToString(i) + " 写入的字节数.";
            }
            catch { tsStatus.Text = "写入失败!"; }
            //RS232.RS232_Write(true, "*RST", true, out message);
            RS232.RS232_WriteByte(SendByte, i, out message);
            Serial_Data.AppendText(message);
        }

        
        private void Form1_Load(object sender, EventArgs e)
        {
            MPPC_Order1.COM_SEND += COM_SEND;
            //tabControl1.SelectedIndex = 2;//设置显示第几个框 this.tabPage1.Parent=null;//隐藏tabPage1选项卡
            //tabControl1.TabPages.Remove(tabPage1);//代码删除第一个选项卡 即不显示第一个选项卡
            //tabControl1.TabPages.Remove(tabPage2);//代码删除第一个选项卡 即不显示第一个选项卡
            //tabControl1.TabPages.Remove(tabPage4);//代码删除第一个选项卡 即不显示第一个选项卡
            com_Channel.SelectedIndex = 0;
            btnRefresh_Click(btnRefresh, new EventArgs());
            cboPM100_WAV.SelectedIndex = 0;
            com_DM6500_Channel.SelectedIndex = 0;
            com_2300G_ch.SelectedIndex = 0;
            if (cboPM100.Items.Count != 0)
            {
                cboPM100.SelectedIndex = 0;
                btnOpenPm100_Click(btnOpenPm100, new EventArgs());
            }
            if (cboDM6500.Items.Count != 0)
            {
                cboDM6500.SelectedIndex = 0;
                btnOpenDM6500_Click(btnOpenDM6500, new EventArgs());
            }
            if (cbo2230G.SelectedItem != null)
            {
                btnOpen2230G_Click(btnOpen2230G, new EventArgs());
            }
            DXecllence_1.Show();
            tabPage4.Controls.Clear();//清空当前窗口的窗口控件
            tabPage4.Controls.Add(DXecllence_1);
            DXecllence_1.Shart_Show_Auto(tabPage4.Size.Width, tabPage4.Size.Height);
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (cbo2230G.SelectedItem != null && (btnOpen2230G.Text == "关闭2230G"))
            {
                btnOpen2230G_Click(btnOpen2230G, new EventArgs());
            }
        }
        public void PM100D_Update()
        {//托管显示线程
            double CHAR_Y;
            double CHAR_X = 0;
            double char_y_max = 0;
            double char_y_min = 100000;
            while (true)
            {
                if (PM100D_St.Count > 1)
                {

                    InvokeToForm(() => {
                        CHAR_Y = System.Convert.ToDouble(PM100D_St.Pop()) * 1000000;
                        PM100D_show_achieve.textBox1.Text = System.Convert.ToString(CHAR_Y);

                        PM100D_show_achieve.chart1.Series[0].Points.AddXY(CHAR_X++, CHAR_Y);
                        if (char_y_max < CHAR_Y)
                        { char_y_max = CHAR_Y; }
                        if (char_y_min > CHAR_Y)
                        { char_y_min = CHAR_Y; }
                        if (CHAR_X > (winnum + 1))
                        {
                            PM100D_show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Position = CHAR_X - winnum;
                            PM100D_show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Size = winnum;//视野范围内共有多少个数据点
                            PM100D_show_achieve.chart1.Series[0].Points.RemoveAt(0);
                        }
                        PM100D_show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Position = char_y_min - 0.5;
                        PM100D_show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Size = char_y_max - char_y_min + 1;// + 10;
                    });

                }
            }
        }
        public void PM100D_Read()
        {
            string message, message_time;
            while (true)
            {
                if (!PM100D_Instantiation.PD100_READ(out message, out message_time))
                {
                    InvokeToForm(() => { MessageBox.Show("PM100读取失败"); });
                    return;
                }
                InvokeToForm(() => {
                    //Serial_Data.AppendText("读取" + message_time + "  :  " + message + "\r\n");
                    PM100D_St.Push(message);
                });

                Thread.Sleep(500);//延时500ms
            }
        }

        private void btnOpenPm100_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            string message, message_time;
            if (cboPM100.SelectedItem == null)
            {
                MessageBox.Show("请链接PD100M仪器……");
                return;
            }
            if (buttonxx.Text == "打开PM100")
            {
                buttonxx.Text = "关闭PM100";
                if (!PM100D_Instantiation.PD100_Open_Close("OPEN", cboPM100.SelectedItem.ToString(), out message))
                {//打开PM100
                    MessageBox.Show("PM100打开失败");
                    return;
                }
                Serial_Data.AppendText("打开端口" + message + "\r\n");

                //清除错误信息
                if (!PM100D_Instantiation.PD100_Err(out message, out message_time))
                {
                    MessageBox.Show("PM100清除错误信息失败");
                    return;
                }
                Serial_Data.AppendText("清除错误信息" + message_time + "  :  " + message + "\r\n");

                //设置波长
                if (!PM100D_Instantiation.PD100_WAV(PM100x_ce_WAV[cboPM100_WAV.SelectedIndex], out message, out message_time))
                {
                    MessageBox.Show("PM100波长设置失败");
                    return;
                }
                Serial_Data.AppendText("波长设置" + message_time + "  :  " + message + "\r\n");
                //读取
                if (!PM100D_Instantiation.PD100_READ(out message, out message_time))
                {
                    MessageBox.Show("PM100获取值");
                    return;
                }
                Serial_Data.AppendText("读取" + message_time + "  :  " + message + "\r\n");
                //读取当前读取配置信息
                if (!PM100D_Instantiation.PD100_READConfing(out message, out message_time))
                {
                    MessageBox.Show("读取当前读取配置信息");
                    return;
                }
                Serial_Data.AppendText("读取" + message_time + "  :  " + message + "\r\n");
                PM100D_show_achieve.chart1.Series[0].Points.Clear();
                PM100D_show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Position = 0;
                PM100D_show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Size = double.NaN; // winnum;//视野范围内共有多少个数据点

                PM100D_show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Position = 0;
                PM100D_show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Size = double.NaN;
                PM100D_show_achieve.chart1.ChartAreas[0].AxisX.Title = "时间/.5s";
                PM100D_show_achieve.chart1.ChartAreas[0].AxisY.Title = "功率/uW";
                PM100D_show_achieve.chart1.Series[0].LegendText = "PM100D";
                PM100D_show_achieve.label1.Text = "功率：";
                PM100D_show_achieve.label2.Text = "uW";
                //开启PM100的读取进程
                PM100D_read = new Thread(new ThreadStart(PM100D_Read));
                PM100D_read.IsBackground = true;
                PM100D_read.Start();

                PM100DUI = new Thread(new ThreadStart(PM100D_Update));
                PM100DUI.IsBackground = true;
                PM100DUI.Start();
            }
            else
            {
                buttonxx.Text = "打开PM100";
                if (!PM100D_Instantiation.PD100_Open_Close("CLOSE", cboPM100.SelectedItem.ToString(), out message))
                {//打开PM100
                    MessageBox.Show("PM100打开失败");
                    return;
                }
                Serial_Data.AppendText("关闭端口" + message + "\r\n");
                PM100D_read.Abort();
                PM100DUI.Abort();
            }
            PM100D_show_achieve.Show();
            PM100M_TAB.Controls.Clear();//清空当前窗口的窗口控件
            PM100M_TAB.Controls.Add(PM100D_show_achieve);
            PM100D_show_achieve.Shart_Show_Auto(PM100M_TAB.Size.Width, PM100M_TAB.Size.Height);
        }
        public void DM6500_Update()
        {//托管显示线程
            double CHAR_Y;
            double CHAR_X = 0;
            double char_y_max = 0;
            double char_y_min = 100000;
            string DM6500_S = null;
            while (true)
            {
                if (DM6500_St.Count > 1)
                {

                    InvokeToForm(() =>
                    {
                        DM6500_S = System.Convert.ToString(DM6500_St.Pop());

                    });
                    string[] DM6500_S1 = DM6500_S.Split(',');
                    InvokeToForm(() =>
                    {
                        for (int i = 0; i < DM6500_S1.Length; i++)
                        {
                            CHAR_Y = System.Convert.ToDouble(DM6500_S1[i]);
                            DM6500_show_achieve.textBox1.Text = System.Convert.ToString(CHAR_Y);
                            DM6500_show_achieve.chart1.Series[0].Points.AddXY(CHAR_X++, CHAR_Y);
                            if (char_y_max < CHAR_Y)
                            { char_y_max = CHAR_Y; }
                            if (char_y_min > CHAR_Y)
                            { char_y_min = CHAR_Y; }
                            if (CHAR_X > (winnum + 1))
                            {
                                DM6500_show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Position = CHAR_X - winnum;
                                DM6500_show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Size = winnum;//视野范围内共有多少个数据点
                                DM6500_show_achieve.chart1.Series[0].Points.RemoveAt(0);
                            }
                            DM6500_show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Position = char_y_min - 0.5;
                            DM6500_show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Size = char_y_max - char_y_min + 1;// + 10;
                        }

                    });
                }
            }
        }

        int DM6500_set_ch = 0;
        int DM6500_get_ok = 0;
        string DM6500_set_channel = "1";
        public void DM6500_Read()
        {
            string message, message_time;
            while (true)
            {
                if (!DM6500_Instantiation.DM6500_Start(out message, out message_time))
                {
                    InvokeToForm(() => { MessageBox.Show("DM6500开启扫描失败"); });
                    return;
                }
                //InvokeToForm(() =>
                //{
                //    Serial_Data.AppendText("开启扫描" + message_time + "  :  " + message + "\r\n");
                //});
                Thread.Sleep(500);//延时500ms
                if (!DM6500_Instantiation.DM6500_READ_ALL(out message, out message_time))
                {
                    InvokeToForm(() => { MessageBox.Show("DM6500读取失败"); });
                    return;
                }
                InvokeToForm(() => {
                    //Serial_Data.AppendText("读取" + message_time + "  :  " + message + "\r\n");
                    DM6500_St.Push(message);
                    DM6500_get_ok = 1;
                });
                if (DM6500_set_ch == 1)
                {
                    DM6500_get_ok = 0;
                    if (!DM6500_Instantiation.DM6500_SHE_CH(DM6500_set_channel, out message, out message_time))
                    {
                        MessageBox.Show("DM6500配置通道失败");
                        return;
                    }
                    DM6500_set_ch = 0;
                }
            }
        }
        private void btnOpenDM6500_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            string message, message_time;
            if (cboDM6500.SelectedItem == null)
            {
                MessageBox.Show("请链接DM6500仪器");
                return;
            }
            if (buttonxx.Text == "打开DM6500")
            {
                buttonxx.Text = "关闭DM6500";
                if (!DM6500_Instantiation.DM6500_Open_Close("OPEN", cboDM6500.SelectedItem.ToString(), out message))
                {//打开PM100
                    MessageBox.Show("DM6500打开失败");
                    return;
                }
                Serial_Data.AppendText("打开端口" + message + "\r\n");

                //清除错误信息
                if (!DM6500_Instantiation.DM6500_Config(out message, out message_time))
                {
                    MessageBox.Show("DM6500配置失败");
                    return;
                }
                Serial_Data.AppendText("清除错误信息" + message_time + "  :  " + message + "\r\n");
                //清除错误信息
                //if (!DM6500_Instantiation.DM6500_SHE_CH("7",out message, out message_time))
                //{
                //    MessageBox.Show("DM6500配置失败");
                //    return;
                //}



                DM6500_show_achieve.chart1.Series[0].Points.Clear();
                DM6500_show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Position = 0;
                DM6500_show_achieve.chart1.ChartAreas[0].AxisX.ScaleView.Size = double.NaN; // winnum;//视野范围内共有多少个数据点

                DM6500_show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Position = 0;
                DM6500_show_achieve.chart1.ChartAreas[0].AxisY.ScaleView.Size = double.NaN;
                DM6500_show_achieve.chart1.ChartAreas[0].AxisX.Title = "";
                DM6500_show_achieve.chart1.ChartAreas[0].AxisY.Title = "V";
                DM6500_show_achieve.chart1.Series[0].LegendText = "DM6500";
                DM6500_show_achieve.label1.Text = "电压：";
                DM6500_show_achieve.label2.Text = "V";

                //开启PM100的读取进程
                DM6500_read = new Thread(new ThreadStart(DM6500_Read));
                DM6500_read.IsBackground = true;
                DM6500_read.Start();

                DM6500UI = new Thread(new ThreadStart(DM6500_Update));
                DM6500UI.IsBackground = true;
                DM6500UI.Start();
            }
            else
            {
                buttonxx.Text = "打开DM6500";
                if (!DM6500_Instantiation.DM6500_Open_Close("CLOSE", cboDM6500.SelectedItem.ToString(), out message))
                {//打开PM100
                    MessageBox.Show("PM100打开失败");
                    return;
                }
                Serial_Data.AppendText("关闭端口" + message + "\r\n");
                DM6500_read.Abort();
                DM6500_read = null;
                DM6500UI.Abort();
            }
            DM6500_show_achieve.Show();
            DM6500_TAB.Controls.Clear();//清空当前窗口的窗口控件
            DM6500_TAB.Controls.Add(DM6500_show_achieve);
            DM6500_show_achieve.Shart_Show_Auto(DM6500_TAB.Size.Width, DM6500_TAB.Size.Height);
        }

        private void btnOpen2230G_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            string message, message_time;
            if (cbo2230G.SelectedItem == null)
            {
                MessageBox.Show("请链接2230G-60-3仪器");
                return;
            }
            if (buttonxx.Text == "打开2230G")
            {
                buttonxx.Text = "关闭2230G";
                if (!P2230G_Instantiation.P2230G_Open_Close("OPEN", cbo2230G.SelectedItem.ToString(), out message))
                {//打开PM100
                    MessageBox.Show("2230G-60-3打开失败");
                    return;
                }
                Serial_Data.AppendText("打开端口" + message + "\r\n");

                //清除错误信息
                if (!P2230G_Instantiation.P2230G_Config(out message, out message_time))
                {
                    MessageBox.Show("2230G-60-3配置失败");
                    return;
                }
                if (!P2230G_Instantiation.P2230G_SET("CH1", "12.000", "3.000", out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return;
                }
                if (!P2230G_Instantiation.P2230G_SET("CH2", "0.000", "0.000", out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return;
                }
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", "0.000", out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return;
                }
                if (!P2230G_Instantiation.P2230G_EN("ON", out message, out message_time))
                {
                    MessageBox.Show("输出失败");
                    return;
                }
            }
            else
            {
                buttonxx.Text = "打开2230G";
                if (!P2230G_Instantiation.P2230G_EN("OFF", out message, out message_time))
                {
                    MessageBox.Show("输出失败");
                    return;
                }
                if (!P2230G_Instantiation.P2230G_Open_Close("CLOSE", cbo2230G.SelectedItem.ToString(), out message))
                {//打开PM100
                    MessageBox.Show("2230G-60-3打开失败");
                    return;
                }
                Serial_Data.AppendText("关闭端口" + message + "\r\n");
            }

        }

        private void button14_Click_1(object sender, EventArgs e)
        {

        }
        /// <summary>
        ///LED校准测试流程
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="DELAY_TIME">读数延时时间</param>
        /// <param name="messageX">输出测试数据</param>
        public void LEDCAL_Read(string FluorescenceChannel, int DELAY_TIME, out string messageX)
        {
            messageX = string.Empty;
            string message, message1, message_time;
            string[] tx = new string[8] {"45000", "45000", "45000", "45000",
                                         "13000","13500","41000","14900"};
            UInt16 number = 10;
            UInt16 step_length = (UInt16)5000;
            UInt16[] LED_SET = new UInt16[number];
            UInt16 tempx = (UInt16)0;
            int TextBox_Gather_i;
            for (int i = 0; i < number; i++)
            {
                tempx += step_length;
                LED_SET[i] = tempx;
            }
            for (TextBox_Gather_i = 0; TextBox_Gather_i < 4; TextBox_Gather_i++)
            {
                InvokeToForm(() =>
                {
                    textBoxe_DAC_SET[TextBox_Gather_i].Text = tx[i];
                });
            }
            //设置功率计波段
            switch (FluorescenceChannel)
            {
                case "FAM":
                    InvokeToForm(() =>
                    { cboPM100_WAV.SelectedIndex = 0; });
                    break;
                case "VIC":
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 2;
                    });
                    break;
                case "ROX":
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 4;
                    });
                    break;
                case "CY5":
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 6;
                    });
                    break;
                default:
                    break;
            }
            //设置光功率计波段
            string PM100_WAVS = string.Empty;
            InvokeToForm(() =>
            {
                PM100_WAVS = PM100x_ce_WAV[cboPM100_WAV.SelectedIndex];
            });
            if (!PM100D_Instantiation.PD100_WAV(PM100_WAVS, out message1, out message_time))
            {
                InvokeToForm(() =>
                {
                    MessageBox.Show("PM100波长设置失败");
                });
                return;
            }
            InvokeToForm(() =>
            {
                Serial_Data.AppendText("波长设置" + message_time + "  :  " + message1 + "\r\n");
            });
            message = FluorescenceChannel + "通道测量\r\nDAC,PM100d,dm6500\r\n";
            for (int i = 0; i < number; i++)
            {
                //设置DAC的值
                switch (FluorescenceChannel)
                {
                    case "FAM":
                        InvokeToForm(() =>
                        {
                            textBoxe_DAC_SET[4].Text = LED_SET[i].ToString();
                        });
                        break;
                    case "VIC":
                        InvokeToForm(() =>
                        {
                            textBoxe_DAC_SET[5].Text = LED_SET[i].ToString(); ;
                        });
                        break;
                    case "ROX":
                        InvokeToForm(() =>
                        {
                            textBoxe_DAC_SET[6].Text = LED_SET[i].ToString();
                        });
                        break;
                    case "CY5":
                        InvokeToForm(() =>
                        {
                            textBoxe_DAC_SET[7].Text = LED_SET[i].ToString();
                        });
                        break;
                    default:
                        break;
                }
                byte[] SendByte = new byte[2500];
                int SendByte_i = 0;
                SendByte[SendByte_i++] = 0x5A;
                SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

                SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
                SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

                SendByte[SendByte_i++] = 0x01;  //目标模块
                SendByte[SendByte_i++] = 0xA3;//A2查询adc读数
                UInt16 tempxt;
                for (TextBox_Gather_i = 0; TextBox_Gather_i < 8; TextBox_Gather_i++)
                {
                    //System.Convert.ToInt16(textBoxe_DAC_SET[TextBox_Gather_i].Text);
                    tempxt = (System.Convert.ToUInt16(textBoxe_DAC_SET[TextBox_Gather_i]));
                    SendByte[SendByte_i++] = (byte)(tempxt >> 8);
                    SendByte[SendByte_i++] = (byte)(tempxt);
                }


                SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
                SendByte[SendByte_i++] = 0xBB;
                SendByte[2] = (byte)((SendByte_i - 6) >> 8);
                SendByte[3] = (byte)(SendByte_i - 6);
                //发送数据,返回正常发送多少个字节
                InvokeToForm(() =>
                {
                    try
                    {
                        serialPort1.Write(SendByte, 0, SendByte_i);
                        tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                    }
                    catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
                });
                Thread.Sleep(DELAY_TIME);//延时20ms   
                InvokeToForm(() =>
                {
                    message += textBoxe_DAC_SET[4].Text + ",";
                    message += PM100D_show_achieve.textBox1.Text + ",";
                    message += DM6500_show_achieve.textBox1.Text + ",\r\n";
                });
            }

            for (TextBox_Gather_i = 0; TextBox_Gather_i < 8; TextBox_Gather_i++)
            {
                InvokeToForm(() =>
                {
                    textBoxe_DAC_SET[TextBox_Gather_i].Text = "0";
                });
            }
            InvokeToForm(() =>
            {
                DAC_SET_Click(DAC_SET, new EventArgs());
            });
            messageX = message;
        }
        Task t_Task = null;

        private void LEDCAL_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            string data_path1 = System.Environment.CurrentDirectory + "\\LED校准数据.csv";
            string message, message1;
            message1 = null;
            int delaytime = 3500;
            t_Task = Task.Factory.StartNew(() =>
            {
                MessageBox.Show("LED测试，将扫描头连接到FAM通道");
                LEDCAL_Read("FAM", delaytime, out message);
                message1 += message;
                MessageBox.Show("LED测试，将扫描头连接到VIC通道");
                LEDCAL_Read("VIC", delaytime, out message);
                message1 += message;
                MessageBox.Show("LED测试，将扫描头连接到ROX通道");
                LEDCAL_Read("ROX", delaytime, out message);
                message1 += message;
                MessageBox.Show("LED测试，将扫描头连接到CY5通道");
                LEDCAL_Read("CY5", delaytime, out message);
                message1 += message;
                IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
                using (var stream = File.Open(data_path1, FileMode.Append))
                using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                {
                    // Don't write the header again. csv.WriteComment(writer);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteField(message1, false);
                }
                MessageBox.Show("测试完成");
                InvokeToForm(() =>
                {
                    LEDCAL.Enabled = true;
                });
            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
        }

        double[] led_pid_tx = new double[8] {45000, 45000, 45000, 45000,
                                         3250,5060,30000,5600};
        /// <summary>
        ///设置DAC的输出值
        /// </summary>
        /// <param name="DAC_T">8路DAC输出值</param>
        /// <returns>true 成功，false失败</returns>
        public bool COM_DAC_SET(ref double[] DAC_T)
        {
            byte[] SendByte = new byte[2000];

            byte SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0xA3;//A2查询adc读数

            for (int TextBox_Gather_i = 0; TextBox_Gather_i < 8; TextBox_Gather_i++)
            {
                var tempx = (System.Convert.ToUInt16(DAC_T[TextBox_Gather_i]));
                SendByte[SendByte_i++] = (byte)(tempx >> 8);
                SendByte[SendByte_i++] = (byte)(tempx);
            }


            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            //发送数据,返回正常发送多少个字节
            InvokeToForm(() =>
            {
                try
                {
                    serialPort1.Write(SendByte, 0, SendByte_i);
                    tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                }
                catch
                { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败");
                }
            });
            Thread.Sleep(200);//延时200ms   等待下位机设置完成
            return true;
        }
        /// <summary>
        ///读取ADC的值
        /// </summary>
        /// <returns>true 成功，false失败</returns>
        public bool COM_ADC_GET()
        {
            byte[] SendByte = new byte[2000];

            byte SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0xA2;//A2查询adc读数
            SendByte[SendByte_i++] = 0x01;//
            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            //发送数据,返回正常发送多少个字节
            InvokeToForm(() =>
            {
                try
                {
                    serialPort1.Write(SendByte, 0, SendByte_i);
                    tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                }
                catch
                {
                    tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败");
                }
            });
            Thread.Sleep(200);//延时200ms   等待下位机设置完成
            return true;
        }
        /// <summary>
        ///LED校准测试流程
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="DELAY_TIME">读数延时时间</param>
        /// <param name="target">目标值</param>
        /// <param name="step">调节步进</param>
        /// <param name="messageX">输出的dac值</param>
        /// <param name="PM100_get">PM100读取值</param>
        /// <returns>true 成功，false失败</returns>
        public bool LED_PID(string FluorescenceChannel, int DELAY_TIME, double target, double step, out string messageX, out string PM100_get)
        {
            messageX = string.Empty;
            PM100_get = string.Empty;
            string message = null, message1, message_time;
            int TextBox_Gather_i = 0, TextBox_Gather_j = 0;
            double pm100_read = 0;
            int Channel = 0;
            float Vi = 0;//
            //设置功率计波段
            switch (FluorescenceChannel)
            {
                case "FAM":
                    Channel = 4;
                    TextBox_Gather_i = 0;
                    InvokeToForm(() =>
                    { cboPM100_WAV.SelectedIndex = 0; });
                    break;
                case "VIC":
                    Channel = 5;
                    TextBox_Gather_i = 1;
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 2;
                    });
                    break;
                case "ROX":
                    Channel = 6;
                    TextBox_Gather_i = 2;
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 4;
                    });
                    break;
                case "CY5":
                    Channel = 7;
                    TextBox_Gather_i = 3;
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 6;
                    });
                    break;
                default:
                    break;
            }
            //设置光功率计波段
            string PM100_WAVS = string.Empty;
            InvokeToForm(() =>
            {
                PM100_WAVS = PM100x_ce_WAV[cboPM100_WAV.SelectedIndex];
            });
            if (!PM100D_Instantiation.PD100_WAV(PM100_WAVS, out message1, out message_time))
            {
                InvokeToForm(() =>
                {
                    MessageBox.Show("PM100波长设置失败");
                });
                return false;
            }
            InvokeToForm(() =>
            {
                Serial_Data.AppendText("波长设置" + message_time + "  :  " + message1 + "\r\n");
            });
            //发射开始输出高压数据指令
            {
                MPPC_Power_OUT(0x01, false);
            }
            Thread.Sleep(DELAY_TIME);//延时20ms 
            //获取当前设置的LED输出电流
            InvokeToForm(() =>
            {
                float.TryParse(TextBox_LED[TextBox_Gather_i, 0].Text, out Vi);
            });
            double last_t = 999;//上一次读取值与目标值的差值
            float last_led_t = Vi;//上一次DAC设置值
            double last_pm100_read = 0;
            while (true)
            {
                //设置当前通道电流输出
                MPPC_EEPROM_LEDSET(FluorescenceChannel, Vi, false);
                Thread.Sleep(100);//延时
                MPPC_Power_OUT(0x01, false);
                Thread.Sleep(DELAY_TIME);//延时
                InvokeToForm(() =>
                {
                    pm100_read = System.Convert.ToDouble(PM100D_show_achieve.textBox1.Text);
                });
                if (System.Math.Abs(pm100_read - target) < 0.01)
                {//误差小于设定值
                    PM100_get = pm100_read.ToString();
                    break;
                }
                else
                {//没有读到要求的最小值
                    if (last_t < System.Math.Abs(pm100_read - target))
                    {
                        Vi = last_led_t;
                        PM100_get = last_pm100_read.ToString();
                        break;
                    }
                    else
                    {
                        last_led_t = Vi;//上一次DAC设置值
                        last_t = System.Math.Abs(pm100_read - target);
                        last_pm100_read = pm100_read;
                    }
                }
                pm100_read = (target - pm100_read) * step;
                Vi += (float)pm100_read;
                //设置DAC的值显示框
                InvokeToForm(() =>
                {
                    textBoxe_DAC_SET[Channel].Text = Vi.ToString();
                });
                message = Vi.ToString();
                if (Vi > 100 || Vi < 0)
                {
                    message = "失败";
                    messageX = message;
                    //关闭输出              
                    MPPC_Power_OUT(0x00, false);
                    Thread.Sleep(20);//延时
                    return false;
                }
            }
            MPPC_EEPROM_LEDSET(FluorescenceChannel, Vi, false);
            Thread.Sleep(20);//延时
            messageX = Vi.ToString();
            InvokeToForm(() =>
            {
                TextBox_LED[TextBox_Gather_i, 0].Text = Vi.ToString();
            });
            //关闭输出              
            MPPC_Power_OUT(0x00, false);
            Thread.Sleep(20);//延时
            return true;
        }
        /// <summary>
        ///LED输出读取
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="DELAY_TIME">读数延时时间</param>
        /// <param name="messageX">输出的dac值</param>
        /// <param name="PM100_get">PM100读取值</param>
        /// <returns>true 成功，false失败</returns>
        public bool LED_READ(string FluorescenceChannel, int DELAY_TIME, out string messageX, out string PM100_get)
        {
            messageX = string.Empty;
            PM100_get = string.Empty;
            string message = null, message1, message_time;
            int TextBox_Gather_i = 0;
            string pm100_read = string.Empty;
            int Channel = 0;
            float Vi = 0;//
            //设置功率计波段
            switch (FluorescenceChannel)
            {
                case "FAM":
                    Channel = 4;
                    TextBox_Gather_i = 0;
                    InvokeToForm(() =>
                    { cboPM100_WAV.SelectedIndex = 0; });
                    break;
                case "VIC":
                    Channel = 5;
                    TextBox_Gather_i = 1;
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 2;
                    });
                    break;
                case "ROX":
                    Channel = 6;
                    TextBox_Gather_i = 2;
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 4;
                    });
                    break;
                case "CY5":
                    Channel = 7;
                    TextBox_Gather_i = 3;
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 6;
                    });
                    break;
                default:
                    break;
            }
            //设置光功率计波段
            string PM100_WAVS = string.Empty;
            InvokeToForm(() =>
            {
                PM100_WAVS = PM100x_ce_WAV[cboPM100_WAV.SelectedIndex];
            });
            if (!PM100D_Instantiation.PD100_WAV(PM100_WAVS, out message1, out message_time))
            {
                InvokeToForm(() =>
                {
                    MessageBox.Show("PM100波长设置失败");
                });
                return false;
            }
            InvokeToForm(() =>
            {
                Serial_Data.AppendText("波长设置" + message_time + "  :  " + message1 + "\r\n");
            });
            //发射开始输出高压数据指令
            {
                MPPC_Power_OUT(0x01, false);
            }
            Thread.Sleep(DELAY_TIME);//延时20ms 
            //获取当前设置的LED输出电流
            InvokeToForm(() =>
            {
                message1 = TextBox_LED[TextBox_Gather_i, 0].Text;
            });
            InvokeToForm(() =>
            {
                pm100_read = PM100D_show_achieve.textBox1.Text;
            });
            messageX = message1;
            PM100_get = pm100_read;
            //关闭输出              
            MPPC_Power_OUT(0x00, false);
            Thread.Sleep(20);//延时
            return true;
        }
        /// <summary>
        ///获取eeprom数据
        /// </summary>
        /// <returns>true 成功，false失败</returns>
        public bool MPPC_EEPROM_GET()
        {
            byte[] SendByte = new byte[2000];
            byte SendByte_i = 0;
            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位
            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0xA1;//A1设置or读取MPPC参数
            SendByte[SendByte_i++] = 0x00;//01设置  orter 读取参数
            int Data_Length = 304;
            int Offset_Address = 0;
            SendByte[SendByte_i++] = (byte)(Offset_Address >> 8); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Offset_Address); //偏移地址低8位
            SendByte[SendByte_i++] = (byte)(Data_Length >> 8); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Data_Length); //偏移地址低8位

            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            //发送数据,返回正常发送多少个字节
            InvokeToForm(() =>
            {
                try
                {
                    serialPort1.Write(SendByte, 0, SendByte_i);
                    tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                }
                catch
                {
                    tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败");
                }
            });
            Thread.Sleep(200);//延时200ms   等待下位机设置完成
            return true;
        }
        /// <summary>
        ///MPPC的LED控制电流EEPROM设置
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="t">设置电流值,单位ma</param>
        /// <param name="threadon">true当前主线程，false非主线程，需要托管</param>
        public void MPPC_EEPROM_LEDSET(string FluorescenceChannel, float t, bool threadon)
        {
            byte[] SendByte = new byte[2500];
            int SendByte_i = 0;
            SendByte_i = 0;
            int Data_Length = 4;
            int Offset_Address = 0;
            //根据通道设置偏移地址
            switch (FluorescenceChannel)
            {
                case "FAM":
                    Offset_Address = 192;
                    break;
                case "VIC":
                    Offset_Address = 220;
                    break;
                case "ROX":
                    Offset_Address = 248;
                    break;
                case "CY5":
                    Offset_Address = 276;
                    break;
                default:
                    break;
            }

            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0xA1;//读取或者设置MPPC参数
            SendByte[SendByte_i++] = 0x01;
            SendByte[SendByte_i++] = (byte)(Offset_Address >> 8); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Offset_Address); //偏移地址低8位
            SendByte[SendByte_i++] = (byte)(Data_Length >> 8); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Data_Length); //偏移地址低8位
            Array.ConstrainedCopy(BitConverter.GetBytes(t), 0, SendByte, SendByte_i, 4);
            SendByte_i = SendByte_i + 4;

            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            if (threadon)
            {
                try
                {
                    serialPort1.Write(SendByte, 0, SendByte_i);
                    tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
            }
            else
            {
                InvokeToForm(() =>
                {
                    try
                    {
                        serialPort1.Write(SendByte, 0, SendByte_i);
                        tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                    }
                    catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
                });
            }
            //发送数据,返回正常发送多少个字节
            Thread.Sleep(100);//延时200ms   等待下位机设置完成
        }
        /// <summary>
        ///MPPC的温度补偿系数设置
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="t">设置电流值,单位ma</param>
        /// <param name="threadon">true当前主线程，false非主线程，需要托管</param>
        public void MPPC_EEPROM_TVOPSET(string FluorescenceChannel, float t, bool threadon)
        {
            byte[] SendByte = new byte[2500];
            int SendByte_i = 0;
            SendByte_i = 0;
            int Data_Length = 4;
            int Offset_Address = 0;
            //根据通道设置偏移地址
            switch (FluorescenceChannel)
            {
                case "FAM":
                    Offset_Address = 8;
                    break;
                case "VIC":
                    Offset_Address = 8 + 48;
                    break;
                case "ROX":
                    Offset_Address = 8 + 96;
                    break;
                case "CY5":
                    Offset_Address = 8 + 144;
                    break;
                default:
                    break;
            }

            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0xA1;//读取或者设置MPPC参数
            SendByte[SendByte_i++] = 0x01;
            SendByte[SendByte_i++] = (byte)(Offset_Address >> 8); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Offset_Address); //偏移地址低8位
            SendByte[SendByte_i++] = (byte)(Data_Length >> 8); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Data_Length); //偏移地址低8位
            //                                              //double tempxx = led_pid_tx[Channel] / 65536 * 1.25 / 20 * 1000;
            //float tempxx = System.Convert.ToSingle(led_pid_tx[Channel] / 65536 * 1.25 / 10 * 1000);
            Array.ConstrainedCopy(BitConverter.GetBytes(t), 0, SendByte, SendByte_i, 4);
            SendByte_i = SendByte_i + 4;

            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            if (threadon)
            {
                try
                {
                    serialPort1.Write(SendByte, 0, SendByte_i);
                    tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
            }
            else
            {
                InvokeToForm(() =>
                {
                    try
                    {
                        serialPort1.Write(SendByte, 0, SendByte_i);
                        tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                    }
                    catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
                });
            }
            //发送数据,返回正常发送多少个字节
            Thread.Sleep(200);//延时200ms   等待下位机设置完成
        }
        /// <summary>
        ///MPPC的kb参数写入
        /// </summary>
        /// <param name="Offset_Address">eeprom的偏移地址</param>
        /// <param name="Slope">斜率K</param>
        /// <param name="Intercept">截距B</param>
        /// <param name="threadon">true当前主线程，false非主线程，需要托管</param>
        public void MPPC_EEPROM_KBET(int Offset_Address, double Slope, double Intercept, bool threadon)
        {
            byte[] SendByte = new byte[2500];
            int SendByte_i = 0;
            SendByte_i = 0;
            int Data_Length = 12;
            float K2 = 0f;
            float K = (float)Slope;
            float B = (float)(Intercept);
            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0xA1;//读取或者设置MPPC参数
            SendByte[SendByte_i++] = 0x01;
            SendByte[SendByte_i++] = (byte)(Offset_Address >> 8); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Offset_Address); //偏移地址低8位
            SendByte[SendByte_i++] = (byte)(Data_Length >> 8); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Data_Length); //偏移地址低8位

            Array.ConstrainedCopy(BitConverter.GetBytes(K2), 0, SendByte, SendByte_i, 4);
            SendByte_i = SendByte_i + 4;
            Array.ConstrainedCopy(BitConverter.GetBytes(K), 0, SendByte, SendByte_i, 4);
            SendByte_i = SendByte_i + 4;
            Array.ConstrainedCopy(BitConverter.GetBytes(B), 0, SendByte, SendByte_i, 4);
            SendByte_i = SendByte_i + 4;
            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            if (threadon)
            {
                try
                {
                    serialPort1.Write(SendByte, 0, SendByte_i);
                    tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
            }
            else
            {
                InvokeToForm(() =>
                {
                    try
                    {
                        serialPort1.Write(SendByte, 0, SendByte_i);
                        tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                    }
                    catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
                });
            }
            //发送数据,返回正常发送多少个字节
            Thread.Sleep(200);//延时200ms   等待下位机设置完成
        }
        /// <summary>
        ///MPPC的mppc控制电源VOV的EEPROM设置
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="Vov">设置电压值单位V</param>
        /// <param name="threadon">true当前主线程，false非主线程，需要托管</param>
        public void MPPC_EEPROM_MPPCVOVSET(string FluorescenceChannel, float Vov, bool threadon)
        {
            byte[] SendByte = new byte[2500];
            int SendByte_i = 0;
            SendByte_i = 0;
            int Data_Length = 4;
            int Offset_Address = 0;
            //根据通道设置偏移地址
            switch (FluorescenceChannel)
            {
                case "FAM":
                    Offset_Address = 4;
                    break;
                case "VIC":
                    Offset_Address = 4 + 48;
                    break;
                case "ROX":
                    Offset_Address = 4 + 96;
                    break;
                case "CY5":
                    Offset_Address = 4 + 144;
                    break;
                default:
                    break;
            }

            SendByte[SendByte_i++] = 0x5A;
            SendByte[SendByte_i++] = 0xA5;  //前两个是开始头

            SendByte[SendByte_i++] = 0x00;// 0x11; //数据长度高8位
            SendByte[SendByte_i++] = 0x01;// 0x11; //低8位

            SendByte[SendByte_i++] = 0x01;  //目标模块
            SendByte[SendByte_i++] = 0xA1;//读取或者设置MPPC参数
            SendByte[SendByte_i++] = 0x01;
            SendByte[SendByte_i++] = (byte)(Offset_Address >> 8); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Offset_Address); //偏移地址低8位
            SendByte[SendByte_i++] = (byte)(Data_Length >> 8); //偏移地址高8位
            SendByte[SendByte_i++] = (byte)(Data_Length); //偏移地址低8位
            //                                              //double tempxx = led_pid_tx[Channel] / 65536 * 1.25 / 20 * 1000;
            //float tempxx = System.Convert.ToSingle(led_pid_tx[Channel] / 65536 * 1.25 / 20 * 1000);
            Array.ConstrainedCopy(BitConverter.GetBytes(Vov), 0, SendByte, SendByte_i, 4);
            SendByte_i = SendByte_i + 4;

            SendByte[SendByte_i++] = 0xAA;  //最后连个是数据尾
            SendByte[SendByte_i++] = 0xBB;
            SendByte[2] = (byte)((SendByte_i - 6) >> 8);
            SendByte[3] = (byte)(SendByte_i - 6);
            if (threadon)
            {
                try
                {
                    serialPort1.Write(SendByte, 0, SendByte_i);
                    tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
            }
            else
            {
                InvokeToForm(() =>
                {
                    try
                    {
                        serialPort1.Write(SendByte, 0, SendByte_i);
                        tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                    }
                    catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
                });
            }
            //发送数据,返回正常发送多少个字节
            Thread.Sleep(300);//延时200ms   等待下位机设置完成
        }
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
            if (threadon)
            {
                try
                {
                    serialPort1.Write(SendByte, 0, SendByte_i);
                    tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
            }
            else
            {
                InvokeToForm(() =>
                {
                    try
                    {
                        serialPort1.Write(SendByte, 0, SendByte_i);
                        tsStatus.Text = System.Convert.ToString(SendByte_i) + " 写入的字节数.";
                    }
                    catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
                });
            }
            //发送数据,返回正常发送多少个字节
            Thread.Sleep(100);//延时200ms   等待下位机设置完成
        }
        /// <summary>
        ///COM_SEND数据发送函数
        /// </summary>
        /// <param name="SendByte_T">数据保存的地址</param>
        /// <param name="SendByte_i_T">生成的数据长度</param>
        /// <param name="threadon">true当前主线程，false非主线程，需要托管</param>
        /// <param name="time_out">发送完成等待长时间</param>
        public bool COM_SEND(byte[] SendByte_T, int SendByte_i_T, bool threadon, int time_out)
        {
            if (threadon)
            {
                try
                {
                    serialPort1.Write(SendByte_T, 0, SendByte_i_T);
                    tsStatus.Text = System.Convert.ToString(SendByte_i_T) + " 写入的字节数.";
                }
                catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
            }
            else
            {
                InvokeToForm(() =>
                {
                    try
                    {
                        serialPort1.Write(SendByte_T, 0, SendByte_i_T);
                        tsStatus.Text = System.Convert.ToString(SendByte_i_T) + " 写入的字节数.";
                    }
                    catch { tsStatus.Text = "写入失败!"; MessageBox.Show("串口写入失败"); }
                });
            }
            Thread.Sleep(time_out);//延时
            return true;
        }
            /// <summary>
            ///MPPC高压调节，此代码为调试测试代码，目的为修改EEPROM中的Vov是输出高压达到目标值
            /// </summary>
            /// <param name="CH">通道值"1-10"</param>
            /// <param name="message">返回信息</param>
            /// <returns>true 成功，false失败</returns>
            public bool SET_DM6500_Channel(string CH, out string message)
        {
            message = string.Empty;
            try
            {
                if (DM6500_read != null)
                {

                    {
                        DM6500_set_channel = CH;
                        DM6500_set_ch = 1;
                        int time_out = 300;
                        //等待设置完成
                        while (true)
                        {
                            if (DM6500_set_ch == 0)
                            {
                                break;
                            }
                            Thread.Sleep(1);
                            time_out--;
                            if (time_out <= 0)
                            {
                                message = "通道设置失败";
                                return false;
                            }
                        }
                    }
                }


            }
            catch (Exception ex) { tsStatus.Text = "DM6500" + ex.Message; ; }
            return true;

        }
        /// <summary>
        ///MPPC高压调节，此代码为调试测试代码，目的为修改EEPROM中的Vov是输出高压达到目标值
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="DELAY_TIME">读数延时时间</param>
        /// <param name="target">目标值</param>
        /// <param name="messageX">输出测试数据</param>
        /// <param name="MPPC_get">实际读取到的MPPC读数</param>
        /// <returns>true 成功，false失败</returns>
        public bool MPPC_Power_PID(string FluorescenceChannel, int DELAY_TIME, float target, out string messageX, out string MPPC_get)
        {
            messageX = string.Empty;
            MPPC_get = string.Empty;
            string message = null, message1, message_time;
            int TextBox_Gather_i = 0, TextBox_Gather_j = 0;
            float pm100_read = 3;
            float Vov = 3;
            int Channel = 0;
            //byte[] SendByte = new byte[2500];
            //int SendByte_i = 0;
            UInt16 tempx;
            //int Data_Length = 4;
            //int Offset_Address = 0;
            //设置基本变量
            switch (FluorescenceChannel)
            {
                case "FAM":
                    TextBox_Gather_i = 0;
                    break;
                case "VIC":
                    TextBox_Gather_i = 1;
                    break;
                case "ROX":
                    TextBox_Gather_i = 2;
                    break;
                case "CY5":
                    TextBox_Gather_i = 3;
                    break;
                default:
                    break;
            }
            //发射开始输出高压数据指令
            {
                MPPC_Power_OUT(0x01, false);
            }
            Thread.Sleep(DELAY_TIME);//延时20ms 
            while (true)
            {
                {//设置当前通道的VOV
                    MPPC_EEPROM_MPPCVOVSET(FluorescenceChannel, Vov, false);
                }
                Thread.Sleep(DELAY_TIME);//延时20ms 
                InvokeToForm(() =>
                {
                    pm100_read = System.Convert.ToSingle(DM6500_show_achieve.textBox1.Text);
                });
                if (System.Math.Abs(pm100_read - target) < 0.005)  //20211104将此处改成了0.005
                {//误差小于设定值
                    break;
                }
                Vov = Vov + (target - pm100_read);
                //设置V0V的值显示框
                InvokeToForm(() =>
                {
                    TextBox_Gather[TextBox_Gather_i, 1].Text = Vov.ToString();
                });
                if (Vov > 4 || Vov < 1)
                {
                    messageX = "失败";
                    //发射开始输出高压关闭数据指令
                    {
                        MPPC_Power_OUT(0x00, false);
                    }
                    return false;
                }
            }
            //发射开始输出高压关闭数据指令
            {
                MPPC_Power_OUT(0x00, false);
                Thread.Sleep(20);//延时20ms 
            }
            MPPC_EEPROM_MPPCVOVSET(FluorescenceChannel, Vov, false);
            //设置V0V的值显示框
            InvokeToForm(() =>
            {
                TextBox_Gather[TextBox_Gather_i, 1].Text = Vov.ToString();
            });
            Thread.Sleep(20);//延时20ms 
            messageX = Vov.ToString();
            MPPC_get = pm100_read.ToString();
            return true;
        }
        /// <summary>
        ///MPPC高压调节，目的为修改EEPROM中的Vov是输出电流达到目标值
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="DELAY_TIME">读数延时时间</param>
        /// <param name="target">目标值</param>
        /// <param name="st">设置工装的输出电流</param>
        /// <param name="messageX">输出测试数据</param>
        /// <param name="MPPC_OUTI">实际读取到的MPPC读数</param>
        /// <returns>true 成功，false失败</returns>
        public bool MPPC_OUTI_Power_PID(string FluorescenceChannel, int DELAY_TIME, float target, string st, out string messageX, out string MPPC_OUTI)
        {
            messageX = string.Empty;
            MPPC_OUTI = string.Empty;
            string message = null, message1, message_time;
            int TextBox_Gather_i = 0, TextBox_Gather_j = 0;
            float pm100_read = 3;
            float Vov = 3;
            //设置基本变量
            switch (FluorescenceChannel)
            {
                case "FAM":
                    TextBox_Gather_i = 0;
                    break;
                case "VIC":
                    TextBox_Gather_i = 1;
                    break;
                case "ROX":
                    TextBox_Gather_i = 2;
                    break;
                case "CY5":
                    TextBox_Gather_i = 3;
                    break;
                default:
                    break;
            }
            //设置LED驱动电流
            InvokeToForm(() =>
            {
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", st, out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return;
                }
            });
            //发射开始输出高压数据指令
            {
                MPPC_Power_OUT(0x01, false);
            }
            Thread.Sleep(DELAY_TIME);//延时20ms 
            //获取当前设置的VOV
            InvokeToForm(() =>
            {
                float.TryParse(TextBox_Gather[TextBox_Gather_i, 1].Text, out Vov);
            });
            while (true)
            {
                {//设置当前通道的VOV
                    MPPC_EEPROM_MPPCVOVSET(FluorescenceChannel, Vov, false);
                }
                Thread.Sleep(DELAY_TIME);//延时20ms 
                InvokeToForm(() =>
                {
                    pm100_read = System.Convert.ToSingle(DM6500_show_achieve.textBox1.Text);
                });
                if (System.Math.Abs(pm100_read - target) < 0.0003)  //误差设置为1mv
                {//误差小于设定值
                    break;
                }
                Vov = Vov - (target - pm100_read) * 8;
                //设置V0V的值显示框
                InvokeToForm(() =>
                {
                    TextBox_Gather[TextBox_Gather_i, 1].Text = Vov.ToString();
                });
                if (Vov > 4 || Vov < 1)
                {
                    messageX = "失败";
                    //发射开始输出高压关闭数据指令
                    {
                        MPPC_Power_OUT(0x00, false);
                    }
                    return false;
                }
            }
            //发射开始输出高压关闭数据指令
            {
                MPPC_Power_OUT(0x00, false);
            }
            messageX = Vov.ToString();
            MPPC_OUTI = pm100_read.ToString();
            return true;
        }
        /// <summary>
        ///工装LED校准测试流程
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="DELAY_TIME">读数延时时间</param>
        /// <param name="target">目标值</param>
        /// <param name="step">调节步进</param>
        /// <param name="messageX">输出测试数据</param>
        /// <param name="PM100_get">PM100读取值</param>
        /// <returns>true 成功，false失败</returns>
        public bool Frock_LED_PID(string FluorescenceChannel, int DELAY_TIME, double target, double step, out string messageX, out string PM100_get)
        {
            messageX = string.Empty;
            PM100_get = string.Empty;
            string message = null, message1 = null, message_time = null;
            int TextBox_Gather_i;
            double pm100_read = 0;
            int Channel = 0;
            byte[] SendByte = new byte[2500];
            int SendByte_i = 0;
            double set_i = 0;
            UInt16 tempx;
            //设置功率计波段
            switch (FluorescenceChannel)
            {
                case "FAM":
                    set_i = 0.130;
                    InvokeToForm(() =>
                    { cboPM100_WAV.SelectedIndex = 1; });
                    break;
                case "VIC":
                    set_i = 0.116;
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 3;
                    });
                    break;
                case "ROX":
                    set_i = 0.110;
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 5;
                    });
                    break;
                case "CY5":
                    set_i = 0.470;
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 7;
                    });
                    break;
                default:
                    break;
            }
            //设置光功率计波段
            string PM100_WAVS = string.Empty;
            InvokeToForm(() =>
            {
                PM100_WAVS = PM100x_ce_WAV[cboPM100_WAV.SelectedIndex];
            });
            if (!PM100D_Instantiation.PD100_WAV(PM100_WAVS, out message1, out message_time))
            {
                InvokeToForm(() =>
                {
                    MessageBox.Show("PM100波长设置失败");
                });
                return false;
            }

            InvokeToForm(() =>
            {
                Serial_Data.AppendText("波长设置" + message_time + "  :  " + message1 + "\r\n");
            });
            double last_t = 999;//上一次读取值与目标值的差值
            double last_led_t = set_i;//上一次DAC设置值
            double last_pm100_read = 0;
            while (true)
            {
                InvokeToForm(() =>
                {
                    if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", set_i.ToString("f3"), out message, out message_time))
                    {
                        MessageBox.Show("设置失败");
                        return;
                    }
                });
                Thread.Sleep(DELAY_TIME);//延时20ms 
                InvokeToForm(() =>
                {
                    pm100_read = System.Convert.ToDouble(PM100D_show_achieve.textBox1.Text);
                });
                if (System.Math.Abs(pm100_read - target) < 0.01)
                {//误差小于设定值
                    PM100_get = pm100_read.ToString();
                    break;
                }
                else
                {//没有读到要求的最小值
                    if (last_t < System.Math.Abs(pm100_read - target))
                    {
                        set_i = last_led_t;
                        PM100_get = last_pm100_read.ToString();
                        break;
                    }
                    else
                    {
                        last_led_t = set_i;//上一次DAC设置值
                        last_t = System.Math.Abs(pm100_read - target);
                        last_pm100_read = pm100_read;
                    }
                }
                pm100_read = (target - pm100_read) * step;
                //设置DAC的值显示框
                set_i += pm100_read;
                if (set_i > 1 || led_pid_tx[Channel] < 0)
                {
                    message = "失败";
                    if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", "0.0", out message, out message_time))
                    {
                        MessageBox.Show("设置失败");
                        return false;
                    }
                    return false;
                }
            }
            messageX = set_i.ToString("f3");
            //关闭光功率计
            if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", "0.0", out message, out message_time))
            {
                MessageBox.Show("设置失败");
                return false;
            }
            return true;
        }
        /// <summary>
        ///MPPC输出值的读取（可以根据万用表通道不同选择读取高压输出或者电流输出）
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="DELAY_TIME">读数延时时间</param>
        /// <param name="st">MPPC工装LED计设置值</param>
        /// <param name="messageX">输出测试数据</param>
        public void MPPC_OUT_READ(string FluorescenceChannel, int DELAY_TIME, string st, out string messageX)
        {
            messageX = string.Empty;
            string message = string.Empty, message_time = string.Empty;
            string pm100_read = "3";
            byte[] SendByte = new byte[2500];
            int SendByte_i = 0;
            //设置LED驱动电流
            InvokeToForm(() =>
            {
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", st, out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return;
                }
            });
            //发射开始输出高压数据指令
            MPPC_Power_OUT(0x01, false);
            Thread.Sleep(DELAY_TIME);//延时20ms 
            InvokeToForm(() =>
            {
                pm100_read = DM6500_show_achieve.textBox1.Text;
            });
            //关闭MPPC高压输出
            MPPC_Power_OUT(0x00, false);
            messageX = pm100_read;
        }
        private void button15_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            string data_path1 = System.Environment.CurrentDirectory + "//H" + lightHeaadNumber.Text + "P" + PCBNumber.Text + "自动标定.csv";
            string message, message1;
            message1 = null;
            string[] log = new string[4] { "FAM,", "VIC,", "ROX,", "CY5," };
            string[,] log_t = new string[4, 10];
            int delaytime = 2000;
            string Channel = "FAM";
            int log_i = 0;
            float target = 0;
            MessageBox.Show("设置通道LED光强为F50,V30,R80,C30\r\n设置通道MPPC驱动电压为标准VBR+3;读取检流电路输出");
            t_Task = Task.Factory.StartNew(() =>
            {
                if (true)
                {
                    MessageBox.Show("LED测试，将扫描头连接到FAM通道");
                    log_i = 1;
                    if (!LED_PID("FAM", delaytime, 70, 0.04, out message, out message1))//100->50  126->63
                    {
                        MessageBox.Show("FAM LED测试失败");
                        InvokeToForm(() =>
                        {
                            buttonxx.Enabled = true;
                        });
                        return;
                    }
                    log[0] += message + ",";
                    log_t[log_i, 0] = message;

                    MessageBox.Show("LED测试，将扫描头连接到VIC通道");
                    log_i = 1;
                    if (!LED_PID("VIC", delaytime, 30, 0.05, out message, out message1))//40->30  269->135
                    {
                        MessageBox.Show("VIC LED测试失败");
                        InvokeToForm(() =>
                        {
                            buttonxx.Enabled = true;
                        });
                        return;
                    }
                    log[1] += message + ",";
                    log_t[log_i, 0] = message;
                    MessageBox.Show("LED测试，将扫描头连接到ROX通道");
                    log_i = 1;
                    if (!LED_PID("ROX", delaytime, 100, 0.3, out message, out message1))  //40->80  750->375
                    {
                        MessageBox.Show("ROX LED测试失败");
                        InvokeToForm(() =>
                        {
                            buttonxx.Enabled = true;
                        });
                        return;
                    }
                    log[2] += message + ",";
                    log_t[log_i, 0] = message;
                    MessageBox.Show("LED测试，将扫描头连接到CY5通道");
                    log_i = 1;
                    if (!LED_PID("CY5", delaytime, 30, 0.05, out message, out message1))//40->30 230->115
                    {
                        MessageBox.Show("CY5 LED测试失败");
                        InvokeToForm(() =>
                        {
                            buttonxx.Enabled = true;
                        });
                        return;
                    }
                    log[3] += message + ",";
                    log_t[log_i, 0] = message;
                }//led校准完成

                Channel = "FAM";
                MessageBox.Show(Channel + "通道高压测试,万用表连接到相应高压输出，并更换滤光片、光功率及位置\r\n并关闭暗箱");
                SET_DM6500_Channel("5", out message);//设置万用表通道
                MPPC_EEPROM_TVOPSET(Channel, 0f, false);//关闭温度补偿
                Thread.Sleep(100);//延时100ms 
                log_i = 0;
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_Gather[log_i, 0].Text, out target);
                });
                if (!MPPC_Power_PID(Channel, delaytime, target + 3, out message, out message1))
                {
                    MessageBox.Show(Channel + "高压设置失败");
                    InvokeToForm(() =>
                    {
                        buttonxx.Enabled = true;
                    });
                    return;
                }
                log[log_i] += message + ",";
                log_t[log_i, 1] = message;
                if (!Frock_LED_PID(Channel, delaytime, 10, 1.3 / 130.0, out message, out message1))
                {
                    MessageBox.Show(Channel + "MPPC_LED调节失败");
                    InvokeToForm(() =>
                    {
                        buttonxx.Enabled = true;
                    });
                    return;
                }
                log[log_i] += message + ",";
                log_t[log_i, 2] = message;
                //MessageBox.Show("更换万用表表笔到"+Channel+"通道");
                SET_DM6500_Channel("1", out message);//设置万用表通道
                MPPC_EEPROM_TVOPSET(Channel, 54f, false);//设置温度补偿
                Thread.Sleep(100);//延时100ms 
                MPPC_OUT_READ(Channel, 20000, "0.000", out message);
                log[log_i] += message + ",";
                log_t[log_i, 3] = message;
                MPPC_OUT_READ(Channel, 20000, log_t[log_i, 2], out message);
                log[log_i] += message + ",";
                log_t[log_i, 4] = message;

                Channel = "VIC";
                MessageBox.Show(Channel + "通道高压测试,万用表连接到相应高压输出，并更换滤光片、光功率及位置\r\n并关闭暗箱");
                SET_DM6500_Channel("6", out message);//设置万用表通道
                MPPC_EEPROM_TVOPSET(Channel, 0f, false);//关闭温度补偿
                Thread.Sleep(100);//延时100ms 
                log_i = 1;
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_Gather[log_i, 0].Text, out target);
                });
                if (!MPPC_Power_PID(Channel, delaytime, target + 3, out message, out message1))
                {
                    MessageBox.Show(Channel + "高压设置失败");
                    InvokeToForm(() =>
                    {
                        buttonxx.Enabled = true;
                    });
                    return;
                }
                log[log_i] += message + ",";
                log_t[log_i, 1] = message;
                if (!Frock_LED_PID(Channel, delaytime, 10, 1.3 / 130.0, out message, out message1))
                {
                    MessageBox.Show(Channel + "MPPC_LED调节失败");
                    InvokeToForm(() =>
                    {
                        buttonxx.Enabled = true;
                    });
                    return;
                }
                log[log_i] += message + ",";
                log_t[log_i, 2] = message;
                //MessageBox.Show("更换万用表表笔到" + Channel + "通道");
                SET_DM6500_Channel("2", out message);//设置万用表通道
                MPPC_EEPROM_TVOPSET(Channel, 54f, false);//设置温度补偿
                Thread.Sleep(100);//延时100ms 

                MPPC_OUT_READ(Channel, 20000, "0.000", out message);
                log[log_i] += message + ",";
                log_t[log_i, 3] = message;
                MPPC_OUT_READ(Channel, 20000, log_t[log_i, 2], out message);
                log[log_i] += message + ",";
                log_t[log_i, 4] = message;

                Channel = "ROX";
                MessageBox.Show(Channel + "通道高压测试,万用表连接到相应高压输出，并更换滤光片、光功率及位置\r\n并关闭暗箱");
                SET_DM6500_Channel("7", out message);//设置万用表通道
                MPPC_EEPROM_TVOPSET(Channel, 0f, false);//关闭温度补偿
                Thread.Sleep(100);//延时100ms 
                log_i = 2;
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_Gather[log_i, 0].Text, out target);
                });
                if (!MPPC_Power_PID(Channel, delaytime, target + 3, out message, out message1))
                {
                    MessageBox.Show(Channel + "高压设置失败");
                    InvokeToForm(() =>
                    {
                        buttonxx.Enabled = true;
                    });
                    return;
                }
                log[log_i] += message + ",";
                log_t[log_i, 1] = message;
                if (!Frock_LED_PID(Channel, delaytime, 10, 1.3 / 130.0, out message, out message1))
                {
                    MessageBox.Show(Channel + "MPPC_LED调节失败");
                    InvokeToForm(() =>
                    {
                        buttonxx.Enabled = true;
                    });
                    return;
                }
                log[log_i] += message + ",";
                log_t[log_i, 2] = message;
                //MessageBox.Show("更换万用表表笔到" + Channel + "通道");
                SET_DM6500_Channel("3", out message);//设置万用表通道
                MPPC_EEPROM_TVOPSET(Channel, 54f, false);////设置温度补偿
                Thread.Sleep(100);//延时100ms 
                MPPC_OUT_READ(Channel, 20000, "0.000", out message);
                log[log_i] += message + ",";
                log_t[log_i, 3] = message;
                MPPC_OUT_READ(Channel, 20000, log_t[log_i, 2], out message);
                log[log_i] += message + ",";
                log_t[log_i, 4] = message;

                Channel = "CY5";
                MessageBox.Show(Channel + "通道高压测试,万用表连接到相应高压输出，并更换滤光片、光功率及位置\r\n并关闭暗箱");
                SET_DM6500_Channel("8", out message);//设置万用表通道
                MPPC_EEPROM_TVOPSET(Channel, 0f, false);//关闭温度补偿
                Thread.Sleep(100);//延时100ms 
                log_i = 3;
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_Gather[log_i, 0].Text, out target);
                });
                if (!MPPC_Power_PID(Channel, delaytime, target + 3, out message, out message1))
                {
                    MessageBox.Show(Channel + "高压设置失败");
                    InvokeToForm(() =>
                    {
                        buttonxx.Enabled = true;
                    });
                    return;
                }
                log[log_i] += message + ",";
                log_t[log_i, 1] = message;
                if (!Frock_LED_PID(Channel, delaytime, 10, 4.7 / 130.0, out message, out message1))
                {
                    MessageBox.Show(Channel + "MPPC_LED调节失败");
                    InvokeToForm(() =>
                    {
                        buttonxx.Enabled = true;
                    });
                    return;
                }
                log[log_i] += message + ",";
                log_t[log_i, 2] = message;
                //MessageBox.Show("更换万用表表笔到" + Channel + "通道");
                SET_DM6500_Channel("4", out message);//设置万用表通道
                MPPC_EEPROM_TVOPSET(Channel, 54f, false);////设置温度补偿
                Thread.Sleep(100);//延时100ms 
                MPPC_OUT_READ(Channel, 20000, "0.000", out message);
                log[log_i] += message + ",";
                log_t[log_i, 3] = message;
                MPPC_OUT_READ(Channel, 20000, log_t[log_i, 2], out message);
                log[log_i] += message + ",";
                log_t[log_i, 4] = message;

                MessageBox.Show("完成测试\r\n" + log[0] + "\r\n" + log[1] + "\r\n" + log[2] + "\r\n" + log[3] + "\r\n");
                IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
                message1 = "通道,led电流,MPPC高压偏移,MPPCled电流,MPPC输出,MPPC输出2" +
                "" +
                "\r\n" + log[0] + "\r\n" + log[1] + "\r\n" + log[2] + "\r\n" + log[3] + "\r\n";
                using (var stream = File.Open(data_path1, FileMode.Append))
                using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                {
                    // Don't write the header again. csv.WriteComment(writer);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteField(message1, false);
                }
                InvokeToForm(() =>
                {
                    buttonxx.Enabled = true;
                });

            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetAppSetting.GetAppSetting_data("tab", System.Convert.ToString(tabControl1.SelectedIndex));
            DXecllence_1.Shart_Show_Auto(tabPage4.Size.Width, tabPage4.Size.Height);

        }
        /// <summary>
        ///电路板的性能测试     
        /// </summary>
        /// <param name="FluorescenceChannel">通道值"FAM/VIC/ROX/CY5"</param>
        /// <param name="LED_T">工装LED输出光强</param>
        /// <returns>true 成功，false失败</returns>
        public bool PCB_TEST_Perf(string FluorescenceChannel, string LED_T)
        {
            string message = null, message1 = null, message_time = null;
            float target = 0;
            int log_i = 0;
            string mppc_v = "MPPC_V,";
            string mppc_i = "MPPC_OUT,";
            switch (FluorescenceChannel)
            {
                case "FAM":
                    log_i = 0;
                    InvokeToForm(() =>
                    { cboPM100_WAV.SelectedIndex = 1; });
                    break;
                case "VIC":
                    log_i = 1;
                    InvokeToForm(() =>
                    { cboPM100_WAV.SelectedIndex = 3; });
                    break;
                case "ROX":
                    log_i = 2;
                    InvokeToForm(() =>
                    { cboPM100_WAV.SelectedIndex = 5; });
                    break;
                case "CY5":
                    log_i = 3;
                    InvokeToForm(() =>
                    { cboPM100_WAV.SelectedIndex = 7; });
                    break;
                default:
                    log_i = 0;
                    break;
            }
            //设置光功率计波段
            string PM100_WAVS = string.Empty;
            InvokeToForm(() =>
            {
                PM100_WAVS = PM100x_ce_WAV[cboPM100_WAV.SelectedIndex];
            });
            if (!PM100D_Instantiation.PD100_WAV(PM100_WAVS, out message1, out message_time))
            {
                InvokeToForm(() =>
                {
                    MessageBox.Show("PM100波长设置失败");
                });
                return false;
            }
            InvokeToForm(() =>
            {
                Serial_Data.AppendText("波长设置" + message_time + "  :  " + message1 + "\r\n");
            });
            //关闭PCB LED测试 关闭工装LED 0
            {//关闭PCB LED测试 关闭工装LED 调节高压
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("关闭PCB LED测试 关闭工装LED 调节高压\r\n");
                });
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道高压、高压、高压检测、以及滤光片更换");
                MPPC_EEPROM_LEDSET("FAM", 0, false);
                MPPC_EEPROM_LEDSET("VIC", 0, false);
                MPPC_EEPROM_LEDSET("ROX", 0, false);
                MPPC_EEPROM_LEDSET("CY5", 0, false);
                //关闭光源的输出
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", "0.000", out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return false;
                }
                //调节目标通道的高压
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_Gather[log_i, 0].Text, out target);
                });
                if (!MPPC_Power_PID(FluorescenceChannel, 5000, target + 3, out message, out message1))
                {
                    MessageBox.Show(FluorescenceChannel + "高压设置失败");
                    return false;
                }
                mppc_v += message1 + ",";
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道电流检测");
                MPPC_OUT_READ(FluorescenceChannel, 10000, "0.000", out message); //此时关闭工装输出检测
                mppc_i += message + ",";
            }
            //打开PCB LED测试 关闭工装LED 不调节高压
            {
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("打开PCB LED测试 关闭工装LED  不调节高压\r\n");
                });
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道高压、高压、高压检测");
                //打开PCB LED测试 关闭工装LED 0
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[0, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("FAM", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[1, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("VIC", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[2, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("ROX", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[3, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("CY5", target, false);
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", "0.000", out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return false;
                }
                //设置MPPC高压开始输出
                MPPC_Power_OUT(0x01, false);
                Thread.Sleep(100);//延时20ms
                //读取万用表的高压值
                Thread.Sleep(5000);//延时20ms 
                InvokeToForm(() =>
                {
                    message = DM6500_show_achieve.textBox1.Text;
                });
                mppc_v += message + ",";
                MPPC_Power_OUT(0x00, false);
                Thread.Sleep(100);//延时20ms
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道电流检测");
                MPPC_OUT_READ(FluorescenceChannel, 10000, "0.000", out message); //此时关闭工装输出检测
                mppc_i += message + ",";
            }
            //关闭PCB LED测试 打开工装LED1 不调节高压
            {
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("关闭PCB LED测试 打开工装LED1 不调节高压\r\n");
                });
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道高压、高压、高压检测");
                //关闭PCB LED测试 打开工装LED1
                MPPC_EEPROM_LEDSET("FAM", 0, false);
                MPPC_EEPROM_LEDSET("VIC", 0, false);
                MPPC_EEPROM_LEDSET("ROX", 0, false);
                MPPC_EEPROM_LEDSET("CY5", 0, false);
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", LED_T, out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return false;
                }
                //设置MPPC高压开始输出
                MPPC_Power_OUT(0x01, false);
                Thread.Sleep(100);//延时20ms
                //读取万用表的高压值
                Thread.Sleep(5000);//延时20ms 
                InvokeToForm(() =>
                {
                    message = DM6500_show_achieve.textBox1.Text;
                });
                mppc_v += message + ",";
                MPPC_Power_OUT(0x00, false);
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道电流检测");
                MPPC_OUT_READ(FluorescenceChannel, 10000, LED_T, out message); //此时关闭工装输出检测
                mppc_i += message + ",";
            }
            //打开PCB LED测试 打开工装LED1 不调节高压
            {
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("打开PCB LED测试 打开工装LED1 不调节高压\r\n");
                });
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道高压、高压、高压检测");
                //打开PCB LED测试 打开工装LED1
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[0, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("FAM", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[1, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("VIC", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[2, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("ROX", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[3, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("CY5", target, false);
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", LED_T, out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return false;
                }
                //设置MPPC高压开始输出
                MPPC_Power_OUT(0x01, false);
                Thread.Sleep(100);//延时20ms
                //读取万用表的高压值
                Thread.Sleep(5000);//延时20ms 
                InvokeToForm(() =>
                {
                    message = DM6500_show_achieve.textBox1.Text;
                });
                mppc_v += message + ",";
                MPPC_Power_OUT(0x00, false);
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道电流检测");
                MPPC_OUT_READ(FluorescenceChannel, 10000, LED_T, out message); //此时关闭工装输出检测
                mppc_i += message + ",";
            }
            //打开PCB LED测试 关闭工装LED 调节高压
            {
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("打开PCB LED测试 关闭工装LED 调节高压\r\n");
                });
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道高压、高压、高压检测");
                //打开PCB LED测试 关闭工装LED 调节
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[0, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("FAM", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[1, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("VIC", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[2, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("ROX", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[3, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("CY5", target, false);
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", "0.000", out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return false;
                }
                //调节目标通道的高压
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_Gather[log_i, 0].Text, out target);
                });
                if (!MPPC_Power_PID(FluorescenceChannel, 5000, target + 3, out message, out message1))
                {
                    MessageBox.Show(FluorescenceChannel + "高压设置失败");
                    return false;
                }
                mppc_v += message1 + ",";
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道电流检测");
                MPPC_OUT_READ(FluorescenceChannel, 10000, "0.000", out message); //此时关闭工装输出检测
                mppc_i += message + ",";
            }
            //关闭PCB LED测试 打开工装LED1 调节高压
            {
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("关闭PCB LED测试 打开工装LED1 调节高压\r\n");
                });
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道高压、高压、高压检测");
                //关闭PCB LED测试 打开工装LED1
                MPPC_EEPROM_LEDSET("FAM", 0, false);
                MPPC_EEPROM_LEDSET("VIC", 0, false);
                MPPC_EEPROM_LEDSET("ROX", 0, false);
                MPPC_EEPROM_LEDSET("CY5", 0, false);
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", LED_T, out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return false;
                }
                //调节目标通道的高压
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_Gather[log_i, 0].Text, out target);
                });
                if (!MPPC_Power_PID(FluorescenceChannel, 5000, target + 3, out message, out message1))
                {
                    MessageBox.Show(FluorescenceChannel + "高压设置失败");
                    return false;
                }
                mppc_v += message1 + ",";
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道电流检测");
                MPPC_OUT_READ(FluorescenceChannel, 10000, LED_T, out message); //此时关闭工装输出检测
                mppc_i += message + ",";
            }
            //打开PCB LED测试 打开工装LED1 调节高压
            {
                InvokeToForm(() =>
                {
                    Serial_Data.AppendText("打开PCB LED测试 打开工装LED 调节高压\r\n");
                });
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道高压、高压、高压检测");
                //打开PCB LED测试 打开工装LED1
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[0, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("FAM", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[1, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("VIC", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[2, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("ROX", target, false);
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_LED[3, 0].Text, out target);
                });
                MPPC_EEPROM_LEDSET("CY5", target, false);
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", LED_T, out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return false;
                }
                //调节目标通道的高压
                InvokeToForm(() =>
                {
                    float.TryParse(TextBox_Gather[log_i, 0].Text, out target);
                });
                if (!MPPC_Power_PID(FluorescenceChannel, 5000, target + 3, out message, out message1))
                {
                    MessageBox.Show(FluorescenceChannel + "高压设置失败");
                    return false;
                }
                mppc_v += message1 + ",";
                MessageBox.Show("更换万用表表笔至" + FluorescenceChannel + "通道电流检测");
                MPPC_OUT_READ(FluorescenceChannel, 10000, LED_T, out message); //此时关闭工装输出检测
                mppc_i += message + ",";
            }
            string data_path1 = System.Environment.CurrentDirectory + "//电路板测试.csv";
            IO_Operate.File_creation(ref data_path1, false);//判定文件路径并根据选择判定是否重新构建文件
            message1 = FluorescenceChannel + "\r\n工装LED,关闭,关闭,打开,打开,关闭,打开,打开\r\nPCBled,关闭,打开,关闭,打开,打开,关闭,打开\r\nMPPC调节,调节,不调节,不调节,不调节,调节,调节,调节\r\n" + mppc_v + "\r\n" + mppc_i + "\r\n";
            using (var stream = File.Open(data_path1, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                csv.Configuration.HasHeaderRecord = false;
                csv.WriteField(message1, false);
            }
            return true;
        }
        /// <summary>
        ///老化测试代码
        /// </summary>
        public  void Ageing_Test_example()
        {
#if PCB_MPPC
           string data_path1 = string.Empty;
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            InvokeToForm(() => { data_path1 = System.Environment.CurrentDirectory + "\\老化测试数据" + "\\H" + lightHeaadNumber.Text + "P" + PCBNumber.Text + "_PCB标定"+ currentTime.ToString("yyyyMMddHHmmss")+".csv"; });
            int channel_i = 0;
            string Channel = string.Empty;
            string message, message1;
            int delaytime = 2000;
            InvokeToForm(() => { channel_i = com_Channel.SelectedIndex + 1; Channel = com_Channel.Text; });
            //设置功率计波段/LED输出光强
            switch (Channel)
            {
                case  "FAM":  
                    InvokeToForm(() =>
                    { cboPM100_WAV.SelectedIndex = 1; });
                    break;
                case "VIC":
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 3;
                    });
                    break;
                case "ROX":
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 5;
                    });
                    break;
                case "CY5":
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 7;
                    });
                    break;
                default:
                    break;
            }
            setText_Appendtext("*********步骤：设置工装LED光强到目标值*********\r\n", Serial_Data);
            if (!Frock_LED_PID(Channel, delaytime, 20, Frock_PID_K[channel_i-1], out message, out message1)) //工装LEDpid校准
            {
                MessageBox.Show(Channel + "工装_LED调节失败");
                return;
            }         
            string write_s=string.Empty;
            InvokeToForm(() => { write_s = "H" + lightHeaadNumber.Text + ",P" + PCBNumber.Text + ","; });
            write_s += $"{Channel},工装电流：,{message},光功率计读数：,{message1},\r\n";
            //打开工装LED
            InvokeToForm(() =>
            {
                if (!P2230G_Instantiation.P2230G_SET("CH3", "5.000", message, out message, out message1))
                {
                    MessageBox.Show("设置失败");
                    return;
                }
            });
            write_s += $"时间,高压读取值,MPPC输出值,光功率读数,MPPC_FAM_TEMP,MPPC_VIC_TEMP,MPPC_ROX_TEMP,MPPC_CY5_TEMP,LED_FAM_TEMP,LED_VIC_TEMP,LED_ROX_TEMP,LED_CY5_TEMP\r\n";
            IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
            CSV_Read_Write.CSV_Write(data_path1, write_s);//写入CSV文件
            //打开MPPC输出高压
            MPPC_Order1.MPPC_Vop_OUT(1, false);
            Thread.Sleep(10000);//延时
            channel_i = 1;
            while (true)
            {
                
                //读取MPPC高压、温度
                SET_DM6500_Channel((channel_i+4).ToString(), out message);//设置万用表通道
                Thread.Sleep(1000);//延时
                string DM6500_read = "3";
                string pm100_read = "3";
                MPPC_Order1.MPPC_Temp_Get(false);  //读取温度数据
                InvokeToForm(() =>
                {
                    DM6500_read = DM6500_show_achieve.textBox1.Text;
                });
                currentTime = System.DateTime.Now;//获取当前系统时间
                write_s = currentTime.ToString("yyyyMMddHHmmss") + "," + DM6500_read + ",";
                //读取电流输出
                SET_DM6500_Channel((channel_i).ToString(), out message);//设置万用表通道
                Thread.Sleep(1000);//延时
                InvokeToForm(() =>
                {
                    DM6500_read = DM6500_show_achieve.textBox1.Text;
                });
                InvokeToForm(() =>
                {
                    pm100_read = PM100D_show_achieve.textBox1.Text;
                });
                write_s +=  DM6500_read + ","+ pm100_read +",";
                for(int i = 0;i<8;i++)  
                {
                    write_s += Fluorescent_Head_TEMP[i] + ",";
                }
                write_s += "\r\n";
                CSV_Read_Write.CSV_Write(data_path1, write_s);//写入CSV文件
                Thread.Sleep(7100);//延时
                InvokeToForm(() =>
                {
                    Serial_Data.Text = "";
                });
            }    
#else
            string data_path1 = string.Empty;
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            InvokeToForm(() => { data_path1 = System.Environment.CurrentDirectory + "\\LED老化测试数据" + "\\H" + lightHeaadNumber.Text + "P" + PCBNumber.Text + "_PCB标定" + currentTime.ToString("yyyyMMddHHmmss") + ".csv"; });
            int channel_i = 0;
            string Channel = string.Empty;
            string message, message1;
            int delaytime = 2000;
            InvokeToForm(() => { channel_i = com_Channel.SelectedIndex + 1; Channel = com_Channel.Text; });
            
            //设置功率计波段/LED输出光强
            switch (Channel)
            {
                case "FAM":
                    InvokeToForm(() =>
                    { cboPM100_WAV.SelectedIndex = 0; });
                    break;
                case "VIC":
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 2;
                    });
                    break;
                case "ROX":
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 4;
                    });
                    break;
                case "CY5":
                    InvokeToForm(() =>
                    {
                        cboPM100_WAV.SelectedIndex = 6;
                    });
                    break;
                default:
                    break;
            }
            string PM100_WAVS = string.Empty;
            InvokeToForm(() =>
            {
                PM100_WAVS = PM100x_ce_WAV[cboPM100_WAV.SelectedIndex];
            });
            if (!PM100D_Instantiation.PD100_WAV(PM100_WAVS, out message1, out message))
            {
                InvokeToForm(() =>
                {
                    MessageBox.Show("PM100波长设置失败");
                });
                return;
            }

            InvokeToForm(() =>
            {
                Serial_Data.AppendText("波长设置" + message + "  :  " + message1 + "\r\n");
            });
            string write_s = string.Empty;
            InvokeToForm(() => { write_s = "H" + lightHeaadNumber.Text + ",P" + PCBNumber.Text + ","; });
            write_s += $"{Channel},\r\n";
            
            write_s += $"时间,光功率读数,MPPC_FAM_TEMP,MPPC_VIC_TEMP,MPPC_ROX_TEMP,MPPC_CY5_TEMP,LED_FAM_TEMP,LED_VIC_TEMP,LED_ROX_TEMP,LED_CY5_TEMP\r\n";
            IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
            CSV_Read_Write.CSV_Write(data_path1, write_s);//写入CSV文件
            //打开LED输出
            MPPC_Order1.MPPC_LED_OUT(1, false);
            Thread.Sleep(10000);//延时
            while (true)
            {
                //读取MPPC高压、温度
                string pm100_read = "3";
                MPPC_Order1.MPPC_Temp_Get(false);  //读取温度数据
                currentTime = System.DateTime.Now;//获取当前系统时间
                write_s = currentTime.ToString("yyyyMMddHHmmss") + "," ;               
                InvokeToForm(() =>
                {
                    pm100_read = PM100D_show_achieve.textBox1.Text;
                });
                write_s +=  pm100_read + ",";
                Thread.Sleep(150);//延时
                for (int i = 0; i < 8; i++)
                {
                    write_s += Fluorescent_Head_TEMP[i] + ",";
                }
                write_s += "\r\n";
                CSV_Read_Write.CSV_Write(data_path1, write_s);//写入CSV文件
                Thread.Sleep(9700);//延时
                InvokeToForm(() =>
                {
                    Serial_Data.Text = "";
                });
            }
#endif

        }
        private void PCB_TEST_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            
            if (buttonxx.Text == "开始老化测试")
            {
                buttonxx.Text = "关闭老化测试";

                //开启PM100的读取进程
                Ageing_Test = new Thread(new ThreadStart(Ageing_Test_example));
                Ageing_Test.IsBackground = true;
                Ageing_Test.Start();
            }
            else
            {
                buttonxx.Text = "开始老化测试";
                Ageing_Test.Abort();
            }
            buttonxx.Enabled = true;
        }

        double[] LED_PID_K = new double[4] { 0.04, 0.04, 0.3, 0.05 };
        double[] Frock_PID_K = new double[4] { 0.01, 0.01, 0.01, 0.036 };
        double[] LED_PID_Target = new double[4] { 45, 22, 76, 28 };
        double[] MPPC_PID_Target = new double[4] { -0.200, -0.190, -0.230, -0.235 };
        string[] channels = new string[4] { "FAM", "VIC", "ROX", "CY5" };
        string[] dm6500_channels = new string[4] { "1", "2", "3", "4" };
        private void button17_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            string string_p = "\\H" + lightHeaadNumber.Text + "P" + PCBNumber.Text + "_PCB标定";
            string data_path1 = System.Environment.CurrentDirectory + "\\校准数据";
            string message, message1;
            message1 = null;
            string[] log = new string[4] { "FAM,", "VIC,", "ROX,", "CY5," };
            string[,] log_t = new string[4, 20];
            int delaytime = 2000;
            string Channel = "FAM";
            int log_i = 0;
            int log_j = 0;
            float target = 0;
            MessageBox.Show($"设置通道LED光强为F{LED_PID_Target[0]},V{LED_PID_Target[1]},R{LED_PID_Target[2]},C{LED_PID_Target[3]}\r\n设置通道MPPC输出F{MPPC_PID_Target[0]},V{MPPC_PID_Target[1]},R{MPPC_PID_Target[2]},C{MPPC_PID_Target[3]}");
            setText_Appendtext($"设置通道LED光强为F{LED_PID_Target[0]},V{LED_PID_Target[1]},R{LED_PID_Target[2]},C{LED_PID_Target[3]}\r\n设置通道MPPC输出F{MPPC_PID_Target[0]},V{MPPC_PID_Target[1]},R{MPPC_PID_Target[2]},C{MPPC_PID_Target[3]}\r\n", Serial_Data);
            t_Task = Task.Factory.StartNew(() =>
            {
                //led定标
                for (log_i = 0; log_i < 4; log_i++)
                {
                    Channel = channels[log_i];
                    MessageBox.Show($"LED测试，将扫描头连接到{Channel}通道");
                    if (!LED_PID(Channel, delaytime, LED_PID_Target[log_i], LED_PID_K[log_i], out message, out message1))//100->50  126->63
                    {
                        MessageBox.Show($"{Channel} LED测试失败");
                        InvokeToForm(() =>
                        {
                            buttonxx.Enabled = true;
                        });
                        return;
                    }
                    log[log_i] += message + "," + message1 + ",";
                    log_t[log_i, 0] = message;
                    log_t[log_i, 1] = message1;
                    setText_Appendtext($"LED输出电流:{message},LED读取的光强{message1}\r\n", Serial_Data);
                }
                //高压定标
                for (log_i = 0; log_i < 4; log_i++)
                {
                    log_j = 2;
                    Channel = channels[log_i];
                    int DM6500_CH = log_i + 5;
                    MessageBox.Show($"{Channel}通道标定;高压探针接到{Channel}通道;并更换滤光片、光功率及位置\r\n并关闭暗箱");
                    //将MPPC输出VOV调节到标准3V
                    {
                        setText_Appendtext("*********步骤：将MPPC输出VOV调节到标准3V*********\r\n", Serial_Data);                       
                        SET_DM6500_Channel(DM6500_CH.ToString(), out message);//设置万用表通道
                        MPPC_EEPROM_TVOPSET(Channel, 0f, false);//关闭温度补偿
                        Thread.Sleep(100);//延时100ms 
                        InvokeToForm(() =>
                        {
                            float.TryParse(TextBox_Gather[log_i, 0].Text, out target);
                        });
                        if (!MPPC_Power_PID(Channel, delaytime, target + 3, out message, out message1))
                        {
                            MessageBox.Show(Channel + "高压设置失败");
                            InvokeToForm(() =>
                            {
                                buttonxx.Enabled = true;
                            });
                            return;
                        }
                        log[log_i] += target.ToString() + "," + message + "," + message1 + ",";
                        log_t[log_i, log_j++] = target.ToString(); //2:MPPC  VBR
                        log_t[log_i, log_j++] = message;            //3:设置的VOV
                        log_t[log_i, log_j++] = message1;           //4:读取电压
                        setText_Appendtext($"VBR:{target.ToString()}设置的VOV:{message},读取电压{message1}\r\n", Serial_Data);
                    }
                    //设置工装LED光强到目标值
                    {
                        setText_Appendtext("*********步骤：设置工装LED光强到目标值*********\r\n", Serial_Data);
                        if (!Frock_LED_PID(Channel, delaytime, 10, Frock_PID_K[log_i], out message, out message1)) //工装LEDpid校准
                        {
                            MessageBox.Show(Channel + "工装_LED调节失败");
                            InvokeToForm(() =>
                            {
                                buttonxx.Enabled = true;
                            });
                            return;
                        }
                        log[log_i] += message + "," + message1 + ",";
                        log_t[log_i, log_j++] = message;            //5:LED工装输出电流
                        log_t[log_i, log_j++] = message1;           //6:LED工装读取值
                        setText_Appendtext($"LED工装输出电流:{message},LED工装读取值{message1}\r\n", Serial_Data);
                    }
                    //读取当前通道关闭工装电流的时候的MPPC输出电流值(即背景值)
                    {
                        setText_Appendtext("*********步骤：读取当前通道关闭工装电流的时候的MPPC输出电流值(即背景值)*********\r\n", Serial_Data);
                        SET_DM6500_Channel(dm6500_channels[log_i], out message);//设置万用表通道
                        MPPC_EEPROM_TVOPSET(Channel, 54f, false);//设置温度补偿
                        Thread.Sleep(100);//延时100ms 
                        MPPC_OUT_READ(Channel, 15000, "0.000", out message);
                        log[log_i] += message + ",";
                        log_t[log_i, log_j++] = message;//7：背景MPPC输出值
                        setText_Appendtext($"背景MPPC输出值:{message}\r\n", Serial_Data);
                    }
                    //在打开工装电流的时候调节VOV使MPPC输出电流达到目标值(固定值+背景值)
                    {
                        setText_Appendtext("*********步骤：在打开工装电流的时候调节VOV使MPPC输出电流达到目标值(固定值+背景值)*********\r\n", Serial_Data);
                        target = (float)MPPC_PID_Target[log_i] + System.Convert.ToSingle(log_t[log_i, 7]);
                        if (!MPPC_OUTI_Power_PID(Channel, 15000, target, log_t[log_i, 5], out message, out message1))
                        {
                            MessageBox.Show(Channel + "VOV调节失败");
                            InvokeToForm(() =>
                            {
                                buttonxx.Enabled = true;
                            });
                            return;
                        }
                        log[log_i] += message + "," + message1 + ",";
                        log_t[log_i, log_j++] = message;            //8:调节MPPC输出VOV设置值
                        log_t[log_i, log_j++] = message1;           //9:万用表读电流取值
                        setText_Appendtext($"调节MPPC输出VOV设置值:{message},万用表读电流取值{message1}\r\n", Serial_Data);
                    }
                    //读取关闭温度补偿的时候电压的值
                    {
                        setText_Appendtext("*********步骤：读取关闭温度补偿的时候电压的值*********\r\n", Serial_Data);
                        SET_DM6500_Channel(DM6500_CH.ToString(), out message);//设置万用表通道
                        MPPC_EEPROM_TVOPSET(Channel, 0f, false);//设置温度补偿
                        Thread.Sleep(100);//延时100ms
                        MPPC_OUT_READ(Channel, 20000, "0.000", out message);
                        log[log_i] += message + ",";
                        log_t[log_i, log_j++] = message;//10：MPPC输出高压值
                        //计算实际调节VOV
                        message1 = (System.Convert.ToSingle(message) - System.Convert.ToSingle(log_t[log_i, 2])).ToString();
                        log[log_i] += message1 + ",";
                        log_t[log_i, log_j++] = message1;//11：调节的VOV
                        setText_Appendtext($"MPPC输出高压值:{message},调节的VOV{message1}\r\n", Serial_Data);
                        Thread.Sleep(100);//延时100ms
                        MPPC_EEPROM_TVOPSET(Channel, 54f, false);//设置温度补偿
                        Thread.Sleep(100);//延时100ms 
                    }
                }
                MessageBox.Show("完成测试\r\n" + log[0] + "\r\n" + log[1] + "\r\n" + log[2] + "\r\n" + log[3] + "\r\n");

                data_path1 += string_p + ".csv";
                IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
                //保存一次EEPROM数据
                //保存一次EEPROM
                MPPC_EEPROM_GET();//读取下位EEPROM
                InvokeToForm(() =>
                {
                    MppcSetGerData_CSV_Write("光头标定");//保存数据
                });
                message1 = "通道,led电流,LED读取光强,Vbr,标准VOV,标准高压,工装LED电流,工装读取光强,背景OutI,MPPC调节VOV,标定OUT_I,调节后MPPC高压,实际VOV" +
                "" +
                "\r\n" + log[0] + "\r\n" + log[1] + "\r\n" + log[2] + "\r\n" + log[3] + "\r\n";
                setText_Appendtext("message1\r\n", Serial_Data);
                using (var stream = File.Open(data_path1, FileMode.Append))
                using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                {
                    // Don't write the header again. csv.WriteComment(writer);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteField(message1, false);
                }
                InvokeToForm(() =>
                {
                    buttonxx.Enabled = true;
                });

            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
        }

        private void button19_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            string string_p = "\\H" + lightHeaadNumber.Text + "P" + PCBNumber.Text;
            string message = string.Empty;
            string text_concentration1 = text_concentration.Text;
            string dm6500_text = string.Empty;
            string data_path1 = System.Environment.CurrentDirectory + "\\数据";
            setText_Appendtext($"*********测试{text_concentration1}浓度荧光值*********\r\n", Serial_Data);
            string s = text_concentration1+",H" + lightHeaadNumber.Text + "P" + PCBNumber.Text + ",";
            int log_i = 0;
            string Channel;
            t_Task = Task.Factory.StartNew(() =>
            {
                //MPPC_EEPROM_TVOPSET("FAM", 54f, false);//设置温度补偿
                //Thread.Sleep(100);//延时100ms
                //MPPC_EEPROM_TVOPSET("VIC", 54f, false);//设置温度补偿
                //Thread.Sleep(100);//延时100ms
                //MPPC_EEPROM_TVOPSET("ROX", 54f, false);//设置温度补偿
                //Thread.Sleep(100);//延时100ms
                //MPPC_EEPROM_TVOPSET("CY5", 54f, false);//设置温度补偿
                //Thread.Sleep(100);//延时100ms
                //发射开始输出高压数据指令
                MPPC_Power_OUT(0x01, false);
                Thread.Sleep(14000);//延时20ms 
                
                for (log_i = 0; log_i < 4; log_i++)
                {
                    int s_ch = log_i + 1;
                    Channel = channels[log_i];
                    SET_DM6500_Channel(s_ch.ToString(), out message);//设置万用表通道
                    Thread.Sleep(1000);//延时1s 
                    InvokeToForm(() =>
                    {
                        dm6500_text = DM6500_show_achieve.textBox1.Text;
                    });
                    s += dm6500_text + ",";
                    setText_Appendtext($"*********读取{Channel}通道值为{dm6500_text}*********\r\n", Serial_Data);
                }               

                //关闭MPPC高压输出
                MPPC_Power_OUT(0x00, false);
                //数据保存
                data_path1 += "\\荧光测试数据.csv";
                string st = string.Empty;
                IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
                st += s+"\r\n";
                using (var stream = File.Open(data_path1, FileMode.Append))
                using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                {
                    // Don't write the header again. csv.WriteComment(writer);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteField(st, false);
                }
                InvokeToForm(() =>
                {
                    buttonxx.Enabled = true;
                });

            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });

        }

        private void com_DM6500_Channel_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Enabled = false;
            try
            {
                if(DM6500_read!=null)
                {
                    
                    {
                        DM6500_set_channel = com_DM6500_Channel.Text;
                        DM6500_set_ch = 1;
                        int time_out = 300;
                        t_Task = Task.Factory.StartNew(() =>
                        {
                            //等待设置完成
                            while (true)
                            {
                                if (DM6500_set_ch == 0)
                                {
                                    break;
                                }
                                Thread.Sleep(1);
                                time_out--;
                                if (time_out <= 0)
                                {
                                    MessageBox.Show("通道设置失败");
                                    break;
                                }
                            }
                        }).ContinueWith(x =>
                        {
                            if (x.IsFaulted)
                            {
                                MessageBox.Show(this, x.Exception.InnerException.Message);
                            }
                        });
                    }
                }
                

            }
            catch (Exception ex) { tsStatus.Text = "DM6500" +ex.Message;; }
           
           
            this.Enabled = true;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            string data_path1 = System.Environment.CurrentDirectory + "//H" + lightHeaadNumber.Text + "P" + PCBNumber.Text + "_PCB标定复测.csv";
            string message, message1;
            message1 = null;
            string[] log = new string[4] { "FAM,", "VIC,", "ROX,", "CY5," };
            string[,] log_t = new string[4, 20];
            int delaytime = 2000;
            string Channel = "FAM";
            int log_i = 0;
            int log_j = 0;
            float target = 0;
            MessageBox.Show($"PCB标定复测");
            setText_Appendtext($"PCB标定复测", Serial_Data);
            t_Task = Task.Factory.StartNew(() =>
            {
                //led定标
                for (log_i = 0; log_i < 4; log_i++)
                {
                    Channel = channels[log_i];
                    MessageBox.Show($"LED复测，将扫描头连接到{Channel}通道");
                    if (!LED_READ(Channel, delaytime, out message, out message1))//100->50  126->63
                    {
                        MessageBox.Show($"{Channel} LED测试失败");
                        InvokeToForm(() =>
                        {
                            buttonxx.Enabled = true;
                        });
                        return;
                    }
                    log[log_i] += message + "," + message1 + ",";
                    log_t[log_i, 0] = message;  //0:LED输出电流
                    log_t[log_i, 1] = message1;//1:LED读取的光强
                    setText_Appendtext($"LED输出电流:{message},LED读取的光强{message1}\r\n", Serial_Data);
                }
                //高压复测
                for (log_i = 0; log_i < 4; log_i++)
                {
                    log_j = 2;
                    int DM6500_CH = log_i + 5;
                    Channel = channels[log_i];
                    MessageBox.Show($"{Channel}通道标定;高压探针接到{Channel}通道;并更换滤光片、光功率及位置\r\n并关闭暗箱");
                    //设置工装LED光强到目标值
                    {
                        setText_Appendtext("*********步骤：设置工装LED光强到目标值*********\r\n", Serial_Data);
                        if (!Frock_LED_PID(Channel, delaytime, 10, Frock_PID_K[log_i], out message, out message1)) //工装LEDpid校准
                        {
                            MessageBox.Show(Channel + "工装_LED调节失败");
                            InvokeToForm(() =>
                            {
                                buttonxx.Enabled = true;
                            });
                            return;
                        }
                        log[log_i] += message + "," + message1 + ",";
                        log_t[log_i, log_j++] = message;            //2:LED工装输出电流
                        log_t[log_i, log_j++] = message1;           //3:LED工装读取值
                        setText_Appendtext($"LED工装输出电流:{message},LED工装读取值{message1}\r\n", Serial_Data);
                    }
                    //读取当前通道打开工装电流的时候的MPPC输出电流值
                    {
                        setText_Appendtext("*********步骤：读取当前通道打开工装电流的时候的MPPC输出电流值*********\r\n", Serial_Data);
                        SET_DM6500_Channel(dm6500_channels[log_i], out message);//设置万用表通道
                        MPPC_EEPROM_TVOPSET(Channel, 54f, false);//设置温度补偿
                        Thread.Sleep(100);//延时100ms 
                        MPPC_OUT_READ(Channel, 15000, log_t[log_i, 2], out message);
                        log[log_i] += message + ",";
                        log_t[log_i, log_j++] = message;//4：MPPC输出值
                        setText_Appendtext($"MPPC输出值:{message}\r\n", Serial_Data);
                        MPPC_OUT_READ(Channel, 15000, "0", out message);
                        log[log_i] += message + ",";
                        log_t[log_i, log_j++] = message;//5：MPPC输出值
                        setText_Appendtext($"MPPC输出背景值:{message}\r\n", Serial_Data);
                    }
                    //读取关闭温度补偿的时候电压的值
                    {
                        setText_Appendtext("*********步骤：读取关闭温度补偿的时候电压的值*********\r\n", Serial_Data);
                        SET_DM6500_Channel(DM6500_CH.ToString(), out message);//设置万用表通道
                        MPPC_EEPROM_TVOPSET(Channel, 0f, false);//设置温度补偿
                        Thread.Sleep(100);//延时100ms
                        MPPC_OUT_READ(Channel, 20000, "0.000", out message);
                        log[log_i] += message + ",";
                        log_t[log_i, log_j++] = message;//6：MPPC输出高压值
                        //计算实际调节VOV
                        InvokeToForm(() =>
                        {
                            float.TryParse(TextBox_Gather[log_i, 0].Text, out target); //读取VBR
                        });
                        message1 = (System.Convert.ToSingle(message) - target).ToString();
                        log[log_i] += target.ToString()+","+message1 + ",";
                        log_t[log_i, log_j++] = target.ToString();//7：调节的实际VBR
                        log_t[log_i, log_j++] = message1;//8：调节的VOV
                        setText_Appendtext($"MPPC输出高压值:{message},调节的VOV{message1}\r\n", Serial_Data);
                        Thread.Sleep(100);//延时100ms
                        MPPC_EEPROM_TVOPSET(Channel, 54f, false);//设置温度补偿
                        Thread.Sleep(100);//延时100ms 
                    }

                }
                MessageBox.Show("完成测试\r\n" + log[0] + "\r\n" + log[1] + "\r\n" + log[2] + "\r\n" + log[3] + "\r\n");
                IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
                message1 = "通道,led电流,LED读取光强,工装LED电流,工装读取光强,标定OUT_I,背景OUT_I,调节后MPPC高压,实际VBR,实际VOV" +
                "" +
                "\r\n" + log[0] + "\r\n" + log[1] + "\r\n" + log[2] + "\r\n" + log[3] + "\r\n";
                setText_Appendtext("message1\r\n", Serial_Data);
                using (var stream = File.Open(data_path1, FileMode.Append))
                using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                {
                    // Don't write the header again. csv.WriteComment(writer);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteField(message1, false);
                }
                InvokeToForm(() =>
                {
                    buttonxx.Enabled = true;
                });


            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
        }

        private void button21_Click(object sender, EventArgs e)
        {
            float VO = float.Parse(text_2230G_VO.Text);
            float VI = float.Parse(text_2230G_Vi.Text);
            string CH = com_2300G_ch.Text;
            switch(CH)
            {
                case "CH3":
                    if(VO>12)
                    {
                        VO = 5.0f;
                        text_2230G_VO.Text = "5.000";
                    }
                    break;
                default:
                    if (VO > 60)
                    {
                        VO = 60.0f;
                        text_2230G_VO.Text = "60.000";
                    }
                    break;
            }
            if (VI > 3)
            {
                VI = 3.0f;
                text_2230G_Vi.Text = "3.000";
            }
            string message, message_time;
            if (btnOpen2230G.Text == "关闭2230G")
            {//说明已经打开了2230g
                if (!P2230G_Instantiation.P2230G_SET(CH, VO.ToString(), VI.ToString(), out message, out message_time))
                {
                    MessageBox.Show("设置失败");
                    return;
                }
            }
                
        }

        private void but_adc_adj_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            string string_p = "\\P" + PCBNumber.Text+"adc校准";
            string message = string.Empty;
            string message_time = string.Empty;          
            string data_path1 = System.Environment.CurrentDirectory + "\\校准数据";
            setText_Appendtext($"*********ADC放大电路定标*********\r\n", Serial_Data);
            string s = "H" + lightHeaadNumber.Text + "P" + PCBNumber.Text + ",";
            int log_i = 0;
            string Channel;
            string[] channel_ADC = new string[8] { "M_FAM", "M_VIC", "M_ROX", "M_CY5", "L_FAM", "L_VIC", "L_ROX", "L_CY5" };
            int[] Offset_Address = new int[8] { 0+12,48 + 12, 96 + 12, 144 + 12, 192 + 4, 220 + 4, 248 + 4, 276 + 4 };
            double[] datax = new double[] {0.8,0.9,1,1.1 };
            double[,] datax1 = new double[8,4] ;
            double[] datay = new double[] { 0.8, 0.9, 1, 1.1 };
            string data_y_s = "y,";
            string data_x_s=null;
            for (int log_j = 0; log_j < datay.Length; log_j++)
            {
                data_y_s += datay[log_j].ToString() + ",";
            }
            data_y_s += "\"SLOPE($B$2:$F$2,B3: F3)\",\"INTERCEPT($B$2:$F$2,B3:F3)\",\"RSQ($B$2:$F$2, B3: F3)\"\r\n";
            t_Task = Task.Factory.StartNew(() =>
            {
                for (int log_j = 0; log_j < datay.Length; log_j++)
                {
                    //设置电源输出电压
                    InvokeToForm(() =>
                    {
                        if (!P2230G_Instantiation.P2230G_SET("CH2", datay[log_j].ToString(), "0.05", out message, out message_time))
                        {
                            MessageBox.Show("设置失败");
                            return;
                        }
                        setText_Appendtext($"电压设置为{datay[log_j].ToString()}\r\n", Serial_Data);
                    });
                    Thread.Sleep(2000);//延时20ms 
                    for (int i = 0; i < 15; i++)
                    {
                        Thread.Sleep(500);//延时500ms 
                        InvokeToForm(() =>
                        {
                            setText_Appendtext($"{i.ToString()}", Serial_Data);
                        });
                        //读取一次ADC值
                        COM_ADC_GET();
                    }
                    Thread.Sleep(100);//延时500ms 
                    for (log_i = 0; log_i < 8; log_i++)
                    {
                        datax1[log_i,log_j] = ADC_DATA[log_i];
                    }
                }
                for (log_i = 0; log_i < 8; log_i++)
                {
                    Channel = channel_ADC[log_i];
                    data_x_s += Channel+",";
                    for (int log_j = 0; log_j < datay.Length; log_j++)
                    {
                        datax[log_j] = datax1[log_i, log_j];
                        data_x_s += datax1[log_i,log_j].ToString() + ",";
                    }
                    //完成adc数据读取
                    // 使用普通最小二乘学习回归
                    OrdinaryLeastSquares ols = new OrdinaryLeastSquares();
                    // 使用OLS学习简单的线性回归
                    SimpleLinearRegression regression = ols.Learn(datax, datay);
                    data_x_s += regression.Slope.ToString() + "," + regression.Intercept.ToString() + ",";
                    double r2 = regression.CoefficientOfDetermination(datax, datay);
                    data_x_s += r2.ToString() + "\r\n";
                    MPPC_EEPROM_KBET(Offset_Address[log_i], regression.Slope,regression.Intercept,false);
                }
                //保存一次EEPROM
                MPPC_EEPROM_GET();//读取下位EEPROM
                InvokeToForm(() =>
                {
                    MppcSetGerData_CSV_Write("pcb定标");//保存数据
                });
                
                {
                    //for (log_i = 0; log_i <8; log_i++)
                    //{

                    //    int s_ch = log_i + 1;
                    //    Channel = channel_ADC[log_i];
                    //    data_x_s += Channel+",";
                    //    MessageBox.Show($"ADC校准测试，将排针链接到{channel_ADC[log_i]}通道");
                    //    for(int log_j = 0;log_j<datay.Length;log_j++)
                    //    {
                    //        //设置电源输出电压
                    //        InvokeToForm(() =>
                    //        {
                    //            if (!P2230G_Instantiation.P2230G_SET("CH2", datay[log_j].ToString(), "0.05", out message, out message_time))
                    //            {
                    //                MessageBox.Show("设置失败");
                    //                return;
                    //            }
                    //            setText_Appendtext($"电压设置为{datay[log_j].ToString()}\r\n", Serial_Data);
                    //        });                      
                    //        Thread.Sleep(2000);//延时20ms 
                    //        for(int i=0;i<15;i++)
                    //        {
                    //            Thread.Sleep(500);//延时500ms 
                    //            InvokeToForm(() =>
                    //            {
                    //                setText_Appendtext($"{i.ToString()}", Serial_Data);
                    //            });                           
                    //            //读取一次ADC值
                    //            COM_ADC_GET();
                    //        }
                    //        Thread.Sleep(100);//延时500ms 
                    //        datax[log_j] = ADC_DATA[log_i];
                    //        data_x_s += datax[log_j].ToString() + ",";
                    //    }
                    //    //完成adc数据读取
                    //    // 使用普通最小二乘学习回归
                    //    OrdinaryLeastSquares ols = new OrdinaryLeastSquares();
                    //    // 使用OLS学习简单的线性回归
                    //    SimpleLinearRegression regression = ols.Learn(datax, datay);
                    //    data_x_s += regression.Slope.ToString() + ","+ regression.Intercept.ToString() + ",";
                    //    double r2 = regression.CoefficientOfDetermination(datax, datay);
                    //    data_x_s += r2.ToString() + "\r\n";
                    //}
                }


                //数据保存
                {
                    //创建根目录文件
                    data_path1 += string_p+".csv";
                    string st = string.Empty;
                    IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
                    st = data_y_s + data_x_s;
                    using (var stream = File.Open(data_path1, FileMode.Append))
                    using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                    using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                    {
                        // Don't write the header again. csv.WriteComment(writer);
                        csv.Configuration.HasHeaderRecord = false;
                        csv.WriteField(st, false);
                    }
                }
                MessageBox.Show("测试完成");
                InvokeToForm(() =>
                {
                    buttonxx.Enabled = true;
                });

            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
        }

        private void but_MPPC_vh_adj_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            string string_p = "\\P" + PCBNumber.Text + "dac校准";
            string message = string.Empty;
            string message_time = string.Empty;
            string data_path1 = System.Environment.CurrentDirectory + "\\校准数据";
            setText_Appendtext($"*********ADC放大电路定标*********\r\n", Serial_Data);
            string s = "H" + lightHeaadNumber.Text + "P" + PCBNumber.Text + ",";
            int log_i = 0;
            string Channel;
            string[] channel_DAC = new string[4] { "FAM", "VIC", "ROX", "CY5" };
            int[] Offset_Address = new int[4] { 0 + 24, 48 + 24, 96 + 24, 144 + 24 };
            double[] datax = new double[6] ;
            double[,] datax1 = new double[4, 6];
            double[] datay = new double[6] { 45000, 46000, 47000, 48000, 49000, 50000 };
            double[] DAC_SET = new double[8] {45000, 45000, 45000, 45000,
                                         4700,5300,41600,4950};
            string data_y_s = "y,";
            string data_x_s = null;
            for (int log_j = 0; log_j < datay.Length; log_j++)
            {
                data_y_s += datay[log_j].ToString() + ",";
            }
            data_y_s += "\"SLOPE($B$2:$F$2,B3: F3)\",\"INTERCEPT($B$2:$F$2,B3:F3)\",\"RSQ($B$2:$F$2, B3: F3)\"\r\n";
            t_Task = Task.Factory.StartNew(() =>
            {
                for (int log_j = 0; log_j < datay.Length; log_j++)
                {
                    //设置DAC输出
                    for (log_i = 0; log_i < 4; log_i++)
                    {
                        DAC_SET[log_i] = datay[log_j];
                    }
                    COM_DAC_SET(ref DAC_SET); //设置DAC输出并等待200ms
                    Thread.Sleep(5000);//延时1s 
                    //读取MPPC值
                    for (log_i = 0; log_i < 4; log_i++)
                    {
                        int s_ch = log_i + 5;
                        string dm6500_text = string.Empty;
                        SET_DM6500_Channel(s_ch.ToString(), out message);//设置万用表通道
                        Thread.Sleep(1000);//延时1s 
                        InvokeToForm(() =>
                        {
                            dm6500_text = DM6500_show_achieve.textBox1.Text;
                        });
                        datax1[log_i, log_j] = System.Convert.ToDouble(dm6500_text);
                    }
                }
                {//关闭DAC输出
                    for (log_i = 0; log_i < 8; log_i++)
                    {
                        DAC_SET[log_i] = 0;
                    }
                    COM_DAC_SET(ref DAC_SET); //设置DAC输出并等待200ms
                }
                for (log_i = 0; log_i < 4; log_i++)
                {
                    Channel = channel_DAC[log_i];
                    data_x_s += Channel + ",";
                    for (int log_j = 0; log_j < datay.Length; log_j++)
                    {
                        datax[log_j] = datax1[log_i, log_j];
                        data_x_s += datax1[log_i, log_j].ToString() + ",";
                    }
                    //完成adc数据读取
                    // 使用普通最小二乘学习回归
                    OrdinaryLeastSquares ols = new OrdinaryLeastSquares();
                    // 使用OLS学习简单的线性回归
                    SimpleLinearRegression regression = ols.Learn(datax, datay);
                    data_x_s += regression.Slope.ToString() + "," + regression.Intercept.ToString() + ",";
                    double r2 = regression.CoefficientOfDetermination(datax, datay);
                    data_x_s += r2.ToString() + "\r\n";
                    MPPC_EEPROM_KBET(Offset_Address[log_i], regression.Slope, regression.Intercept, false);
                }
                //保存一次EEPROM
                MPPC_EEPROM_GET();//读取下位EEPROM
                InvokeToForm(() =>
                {
                    MppcSetGerData_CSV_Write("pcbDAC定标");//保存数据
                });
             


                //数据保存
                {
                    //创建根目录文件
                    data_path1 += string_p + ".csv";
                    string st = string.Empty;
                    IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
                    st = data_y_s + data_x_s;
                    using (var stream = File.Open(data_path1, FileMode.Append))
                    using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                    using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                    {
                        // Don't write the header again. csv.WriteComment(writer);
                        csv.Configuration.HasHeaderRecord = false;
                        csv.WriteField(st, false);
                    }
                }
                MessageBox.Show("测试完成");
                InvokeToForm(() =>
                {
                    buttonxx.Enabled = true;
                });

            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
        }

        private void button16_Click(object sender, EventArgs e)
        {
            Button buttonxx = (Button)sender;
            buttonxx.Enabled = false;
            string string_p = "\\H" + lightHeaadNumber.Text + "P" + PCBNumber.Text + "_PCB测试数据";
            string data_path1 = System.Environment.CurrentDirectory + "\\测试";
            string message, message1;
            message1 = null;
            string[] log = new string[4] { "FAM,", "VIC,", "ROX,", "CY5," };
            string[,] log_t = new string[4, 20];
            int delaytime = 2000;
            string Channel = "FAM";
            int log_i = 0;
            int log_j = 0;
            float target = 0;
            MessageBox.Show($"设置通道LED光强为F{LED_PID_Target[0]},V{LED_PID_Target[1]},R{LED_PID_Target[2]},C{LED_PID_Target[3]}\r\n设置通道MPPC输出F{MPPC_PID_Target[0]},V{MPPC_PID_Target[1]},R{MPPC_PID_Target[2]},C{MPPC_PID_Target[3]}");
            setText_Appendtext($"设置通道LED光强为F{LED_PID_Target[0]},V{LED_PID_Target[1]},R{LED_PID_Target[2]},C{LED_PID_Target[3]}\r\n设置通道MPPC输出F{MPPC_PID_Target[0]},V{MPPC_PID_Target[1]},R{MPPC_PID_Target[2]},C{MPPC_PID_Target[3]}\r\n", Serial_Data);
            t_Task = Task.Factory.StartNew(() =>
            {
                //led定标
                for (log_i = 0; log_i < 4; log_i++)
                {
                    Channel = channels[log_i];
                    MessageBox.Show($"LED测试，将扫描头连接到{Channel}通道");
                    if (!LED_PID(Channel, delaytime, LED_PID_Target[log_i], LED_PID_K[log_i], out message, out message1))//100->50  126->63
                    {
                        MessageBox.Show($"{Channel} LED测试失败");
                        InvokeToForm(() =>
                        {
                            buttonxx.Enabled = true;
                        });
                        return;
                    }
                    log[log_i] += message + "," + message1 + ",";
                    log_t[log_i, 0] = message;
                    log_t[log_i, 1] = message1;
                    setText_Appendtext($"LED输出电流:{message},LED读取的光强{message1}\r\n", Serial_Data);
                }
                //高压定标
                for (log_i = 0; log_i < 4; log_i++)
                {
                    log_j = 2;
                    Channel = channels[log_i];
                    int DM6500_CH = log_i + 5;
                    MessageBox.Show($"{Channel}通道标定;高压探针接到{Channel}通道;并更换滤光片、光功率及位置\r\n并关闭暗箱");
                    //将MPPC输出VOV调节到标准3V
                    {
                        setText_Appendtext("*********步骤：将MPPC输出VOV调节到标准3V*********\r\n", Serial_Data);
                        SET_DM6500_Channel(DM6500_CH.ToString(), out message);//设置万用表通道
                        MPPC_EEPROM_TVOPSET(Channel, 0f, false);//关闭温度补偿
                        Thread.Sleep(100);//延时100ms 
                        InvokeToForm(() =>
                        {
                            float.TryParse(TextBox_Gather[log_i, 0].Text, out target);
                        });
                        if (!MPPC_Power_PID(Channel, delaytime, target + 3, out message, out message1))
                        {
                            MessageBox.Show(Channel + "高压设置失败");
                            InvokeToForm(() =>
                            {
                                buttonxx.Enabled = true;
                            });
                            return;
                        }
                        log[log_i] += target.ToString() + "," + message + "," + message1 + ",";
                        log_t[log_i, log_j++] = target.ToString(); //2:MPPC  VBR
                        log_t[log_i, log_j++] = message;            //3:设置的VOV
                        log_t[log_i, log_j++] = message1;           //4:读取电压
                        setText_Appendtext($"VBR:{target.ToString()}设置的VOV:{message},读取电压{message1}\r\n", Serial_Data);
                    }
                    //设置工装LED光强到目标值
                    {
                        setText_Appendtext("*********步骤：设置工装LED光强到目标值*********\r\n", Serial_Data);
                        if (!Frock_LED_PID(Channel, delaytime, 10, Frock_PID_K[log_i], out message, out message1)) //工装LEDpid校准
                        {
                            MessageBox.Show(Channel + "工装_LED调节失败");
                            InvokeToForm(() =>
                            {
                                buttonxx.Enabled = true;
                            });
                            return;
                        }
                        log[log_i] += message + "," + message1 + ",";
                        log_t[log_i, log_j++] = message;            //5:LED工装输出电流
                        log_t[log_i, log_j++] = message1;           //6:LED工装读取值
                        setText_Appendtext($"LED工装输出电流:{message},LED工装读取值{message1}\r\n", Serial_Data);
                    }
                    //读取当前通道  打开自身光源  关闭工装电流的时候的MPPC输出电流值(即背景值)
                    {
                        setText_Appendtext("*********步骤：读取当前通道关闭工装电流的时候的MPPC输出电流值(即背景值)*********\r\n", Serial_Data);
                        SET_DM6500_Channel(dm6500_channels[log_i], out message);//设置万用表通道
                        MPPC_EEPROM_TVOPSET(Channel, 54f, false);//设置温度补偿
                        Thread.Sleep(100);//延时100ms 
                        MPPC_OUT_READ(Channel, 15000, "0.000", out message);
                        log[log_i] += message + ",";
                        log_t[log_i, log_j++] = message;//7：背景MPPC输出值
                        setText_Appendtext($"背景MPPC输出值:{message}\r\n", Serial_Data);
                    }
                    //读取当前通道  关闭自身光源  关闭工装电流的时候的MPPC输出电流值(即背景值)
                    {
                        setText_Appendtext("*********步骤：读取当前通道  关闭自身光源  关闭工装电流的时候的MPPC输出电流值(即背景值)*********\r\n", Serial_Data);
                        MPPC_EEPROM_LEDSET("FAM", 0, false); //设置LED 输出电流
                        MPPC_EEPROM_LEDSET("VIC", 0, false);
                        MPPC_EEPROM_LEDSET("ROX", 0, false);
                        MPPC_EEPROM_LEDSET("CY5", 0, false);
                        SET_DM6500_Channel(dm6500_channels[log_i], out message);//设置万用表通道
                        MPPC_EEPROM_TVOPSET(Channel, 54f, false);//设置温度补偿
                        Thread.Sleep(100);//延时100ms 
                        MPPC_OUT_READ(Channel, 15000, "0.000", out message);
                        log[log_i] += message + ",";
                        log_t[log_i, log_j++] = message;//8：关闭LED背景MPPC输出值
                        setText_Appendtext($"读取当前通道  关闭自身光源  关闭工装电流:{message}\r\n", Serial_Data);
                    }
                    //在打开工装电流的时候关闭自身光源读取输出值
                    {
                        setText_Appendtext("*********步骤：在打开工装电流的时候关闭自身光源读取输出值*********\r\n", Serial_Data);
                        target = (float)MPPC_PID_Target[log_i] + System.Convert.ToSingle(log_t[log_i, 7]);

                        MPPC_OUT_READ(Channel, 15000, log_t[log_i, 5], out message);
                        log[log_i] += message + ",";
                        log_t[log_i, log_j++] = message;//9：读取到的输出值
                        setText_Appendtext($"在打开工装电流的时候关闭自身光源读取输出值:{message}\r\n", Serial_Data);
                    }
                    {
                        MPPC_EEPROM_LEDSET("FAM", System.Convert.ToSingle(log_t[0, 0]), false); //设置LED 输出电流
                        MPPC_EEPROM_LEDSET("VIC", System.Convert.ToSingle(log_t[1, 0]), false);
                        MPPC_EEPROM_LEDSET("ROX", System.Convert.ToSingle(log_t[2, 0]), false);
                        MPPC_EEPROM_LEDSET("CY5", System.Convert.ToSingle(log_t[3, 0]), false);
                    }
                }
                MessageBox.Show("完成测试\r\n" + log[0] + "\r\n" + log[1] + "\r\n" + log[2] + "\r\n" + log[3] + "\r\n");

                //创建根目录文件
                data_path1 += string_p + ".csv";
                IO_Operate.File_creation(ref data_path1, true);//判定文件路径并根据选择判定是否重新构建文件
                //保存一次EEPROM数据
                //保存一次EEPROM
                MPPC_EEPROM_GET();//读取下位EEPROM
                InvokeToForm(() =>
                {
                    MppcSetGerData_CSV_Write("光头标定");//保存数据
                });
                message1 = "通道,led电流,LED读取光强,Vbr,标准VOV,标准高压,工装LED电流,工装读取光强,背景OutI,全关Out I,工装打开OUT_I" +
                "" +
                "\r\n" + log[0] + "\r\n" + log[1] + "\r\n" + log[2] + "\r\n" + log[3] + "\r\n";
                setText_Appendtext("message1\r\n", Serial_Data);
                using (var stream = File.Open(data_path1, FileMode.Append))
                using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
                using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
                {
                    // Don't write the header again. csv.WriteComment(writer);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteField(message1, false);
                }
                InvokeToForm(() =>
                {
                    buttonxx.Enabled = true;
                });

            }).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
        }
    }
  
}

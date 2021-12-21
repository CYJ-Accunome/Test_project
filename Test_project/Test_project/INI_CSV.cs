using CsvHelper;
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

namespace Test_project
{//public  bool RS232_SEND(object sender,byte[] SendByte_T,  byte SendByte_i_T, bool threadon,bool return_on,int time_out)
    /// <summary>
    ///通过主界面串口发送数据
    /// </summary>
    /// <param name="sender">什么按键触发的</param>
    /// <param name="SendByte_T">数据保存的地址</param>
    /// <param name="SendByte_i_T">生成的数据长度</param>
    /// <param name="threadon">true当前主线程，false非主线程，需要托管</param>
    /// <param name="return_on">true需要返回值 false 不需要返回,只有在非主线程有效</param>
    /// <param name="time_out">超时时间，只有在非主线程有效</param>
    public delegate bool DX_RS232_SEND(object sender, byte[] SendByte_T, byte SendByte_i_T, bool threadon, bool return_on, int time_out);

    /// <summary>
    ///接收EEPROM数据并处理显示
    /// </summary>
    /// <param name="SendByte_T">数据保存的地址</param>
    public delegate bool DX_RS232_READ(byte[] SendByte_T);

    public partial class INI_CSV : UserControl
    {
        //定义一个委托
        public DX_RS232_SEND RS232_Eeprom_SEND;
        public DX_RS232_READ RS232_Eeprom_READ;
        byte[] SendByte = new byte[256];
        byte SendByte_i = 0;
        byte cmdno = 0;

        private ComboBox comboBox = null;
        static string ini_name = "IDx-xxx.ini";
        private BindingList<iniData> P_iniData = new BindingList<iniData>();
        List<iniData> rd_iniData = null;
        byte ID = (byte)1;
        int address = 0;
        byte data_length = 0;
        public INI_CSV()
        {
            InitializeComponent();
           dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
            RS232_Eeprom_READ += RS232_eepromREAD;
        }
        private void InitComboBox()
        {
            comboBox = new ComboBox();
            this.comboBox.Items.Add("Int");
            this.comboBox.Items.Add("Float");
            this.comboBox.Items.Add("Double");
            this.comboBox.Items.Add("Short");
            this.comboBox.Items.Add("Char");
            this.comboBox.Items.Add("Unsigned int");
            this.comboBox.Items.Add("Unsigned short");
            this.comboBox.Items.Add("Unsigned Byte");
            this.comboBox.Items.Add("Byte");
            this.comboBox.Leave += new EventHandler(ComboBox_Leave);//焦点离开时候发生
            this.comboBox.SelectedIndexChanged += new EventHandler(ComboBox_TextChanged);//属性更改时候发生
            this.comboBox.Visible = false;
            this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            this.dataGridView1.Controls.Add(this.comboBox);
        }
        private void ComboBox_TextChanged(object sender, EventArgs e)
        {
            this.dataGridView1.CurrentCell.Value = ((ComboBox)sender).Text;
            int row = dataGridView1.CurrentRow.Index;
            switch (((ComboBox)sender).Text)
            {
                case "Int":
                    dataGridView1.Rows[row].Cells[4].Value = "4";
                    break;
                case "Float":
                    dataGridView1.Rows[row].Cells[4].Value = "4";
                    break;
                case "Double":
                    dataGridView1.Rows[row].Cells[4].Value = "8";
                    break;
                case "Short":
                    dataGridView1.Rows[row].Cells[4].Value = "2";
                    break;
                case "Char":
                    dataGridView1.Rows[row].Cells[4].Value = "1";
                    break;
                case "Unsigned int":
                    dataGridView1.Rows[row].Cells[4].Value = "4";
                    break;
                case "Unsigned short":
                    dataGridView1.Rows[row].Cells[4].Value = "2";
                    break;
                case "Unsigned Byte":
                    dataGridView1.Rows[row].Cells[4].Value = "1";
                    break;
                case "Byte":
                    dataGridView1.Rows[row].Cells[4].Value = "1";
                    break;
                default:
                    break;

            }
            this.comboBox.Visible = false;
        }
        public bool RS232_eepromREAD(byte[] SendByte_T)
        {//进行RS232 EEPROM数据解码
            data_length = (byte)(SendByte_T[3] -0x07);  //address         
            bool sign = false;
            int data_s = 5;
            if(rd_iniData!=null)
            {
                foreach (var stu in rd_iniData)
                {
                    if(System.Convert.ToInt32(stu.ParamAddress, 16)== address)
                    {//说明找到第一个数据
                        sign = true;
                    }
                    if (data_length>0 && sign==true)
                    {
                        int data_len = System.Convert.ToInt32(stu.ParamLen);
                        switch (stu.ParamType)
                        {
                            case "Int":
                                stu.ParamValue = System.Convert.ToString(BitConverter.ToInt32(SendByte_T, data_s));
                                break;
                            case "Float":
                                stu.ParamValue = System.Convert.ToString(BitConverter.ToSingle(SendByte_T, data_s));
                                break;
                            case "Double":
                                stu.ParamValue = System.Convert.ToString(BitConverter.ToDouble(SendByte_T, data_s));
                                break;
                            case "Short":
                                stu.ParamValue = System.Convert.ToString(BitConverter.ToInt16(SendByte_T, data_s));
                                break;
                            case "Char":
                                byte[] bs = new byte[data_len];
                                int bs_i = data_s;
                                for (int char_i = 0; char_i < data_len; char_i++)
                                {
                                    bs[char_i] = SendByte_T[bs_i++];
                                }//System.Text.Encoding.ASCII.GetString(bs);　　
                                stu.ParamValue = System.Text.Encoding.ASCII.GetString(bs);
                                break;
                            case "Unsigned int":
                                stu.ParamValue = System.Convert.ToString(BitConverter.ToUInt32(SendByte_T, data_s));
                                break;
                            case "Unsigned short":
                                stu.ParamValue = System.Convert.ToString(BitConverter.ToUInt16(SendByte_T, data_s));
                                break;
                            case "Unsigned Byte":
                                stu.ParamValue = System.Convert.ToString(SendByte_T[ data_s]);
                                break;
                            case "Byte":                             
                                stu.ParamValue = System.Convert.ToString(SendByte_T[data_s]);
                                break;
                            default:
                                MessageBox.Show("数据类型错误");
                                InvokeToForm(() =>
                                {
                                    this.Enabled = true;
                                });
                                return false;
                                break;
                        }
                        data_length = (byte)(data_length - data_len);
                        data_s += data_len;
                    }

                }
            }
            if(data_length>0)
            {
                InvokeToForm(() =>
                {
                    MessageBox.Show("数据解码失败");
                });
                return false;
            }
            else
            {
                
                return true; 
            }
            
        }
        private void ComboBox_Leave(object sender, EventArgs e)
        {
            this.comboBox.Visible = false;
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
        /// <summary>
        ///读取文件路径,一次可以读取多个文件
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
                ini_name = path1.Substring(path1.LastIndexOf("\\") + 1, (path1.Length - path1.LastIndexOf("\\") - 5));  //截取目  9-
                string idx = ini_name.Substring(ini_name.LastIndexOf("ID") + 2, ini_name.LastIndexOf("-") - ini_name.LastIndexOf("ID")-2);
                ID = System.Convert.ToByte(idx);
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
        /// <summary>
        ///读取文件路径
        /// </summary>
        /// <returns>文件路径</returns>
        public  string Read_Name_Path()
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
                string idx = ini_name.Substring(ini_name.LastIndexOf("ID") + 2, ini_name.LastIndexOf("-") - ini_name.LastIndexOf("ID") - 2);
                ID = System.Convert.ToByte(idx);
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
        private void Read_ini_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;//设置绑定数据为空
            //清空表格内部数据  
            dataGridView1.Rows.Clear();//清空表格内部数据 
            OpenFileDialog file1 = INIRead_Name_PathS();
            if (file1 == null) return;
            P_iniData.Clear();
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
                    sIni.ft = false;
                    //dataGridView1.Rows.Add(sIni.ParamName, sIni.ParamAddress, sIni.ParamValue, sIni.ParamLen, sIni.ParamType, sIni.ParamProp);
                    P_iniData.Add(sIni);
                }
            }
            dataGridView1.DataSource = P_iniData;
        }

        private void INI_CSV_Load(object sender, EventArgs e)
        {
            //dataGridView1.ColumnCount = 6;//设置列数6
            dataGridView1.ColumnHeadersVisible = true;//显示列标题
            // 设置DataGridView控件标题列的样式
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
            columnHeaderStyle.BackColor = Color.Beige;//设置标题背景颜色
            columnHeaderStyle.Font = new Font("Verdana", 10, FontStyle.Bold);//设置标题字体大小即样式
            dataGridView1.ColumnHeadersDefaultCellStyle = columnHeaderStyle;

            DataGridViewCheckBoxColumn dgvc = new DataGridViewCheckBoxColumn();         //创建列对象
            dgvc.HeaderText = "状态";                                                   //设置列标题
            dataGridView1.Columns.Add(dgvc);                                            //添加列
            //P_iniData = 
            dataGridView1.DataSource = P_iniData;                                         //绑定数据集合
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            InitComboBox();

        }

        private void button8_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;//设置绑定数据为空
            dataGridView1.Rows.Clear();//清空表格内部数据
            string file_name = Read_Name_Path();//需要读取的文件名
            List<iniData> P_iniData1 = new List<iniData>();
            if (file_name == null) return;
            using (var reader = new StreamReader(file_name, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                P_iniData1 = csv.GetRecords<iniData>().ToList();
            }
            P_iniData = new BindingList<iniData>(P_iniData1);  //List<T> modelList=new List<T>((BindingList<T>)this.DataGridView.DataSource);
            //foreach (var sIni in results)
            //{//遍历所有数据并显示
            //    dataGridView1.Rows.Add(sIni.ParamName, sIni.ParamAddress, sIni.ParamValue, sIni.ParamLen, sIni.ParamType, sIni.ParamProp);
            //}
            dataGridView1.DataSource = P_iniData; //绑定数据
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

            //IniWriteValue
            INIClass.IniWriteValue("NodeEEParam", "ParamNum", System.Convert.ToString(P_iniData.Count()), path1);
            for (int i = 0; i < P_iniData.Count(); i++)
            {
                string section = "Param" + System.Convert.ToString(i + 1);
                INIClass.IniWriteValue(section, "ParamName", coded_system.gb2312_utf8(P_iniData[i].ParamName), path1);
                string sss = coded_system.gb2312_utf8(P_iniData[i].ParamName);
                INIClass.IniWriteValue(section, "ParamAddress", P_iniData[i].ParamAddress, path1);
                INIClass.IniWriteValue(section, "ParamValue", P_iniData[i].ParamValue, path1);
                INIClass.IniWriteValue(section, "ParamLen", P_iniData[i].ParamLen, path1);
                INIClass.IniWriteValue(section, "ParamType", P_iniData[i].ParamType, path1);
                INIClass.IniWriteValue(section, "ParamProp", P_iniData[i].ParamProp, path1);
            }
            MessageBox.Show("数据转换完成");
            buttonxx.Enabled = true;
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

            using (var stream = File.Open(path1, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                //csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.RegisterClassMap<iniDataMap>();
                csv.WriteRecords(P_iniData);
                csv.Flush();
                csv.Dispose();
            }
            MessageBox.Show("数据转换完成");
            buttonxx.Enabled = true;
        }

        private void but_add_data_Click(object sender, EventArgs e)
        {
            iniData sIni = new iniData();
            sIni.ParamName = "2";
            sIni.ParamAddress = "";
            sIni.ParamValue = "";
            sIni.ParamLen = "";
            sIni.ParamType = "";
            sIni.ParamProp = "";
            //dataGridView1.Rows.Add(sIni.ParamName, sIni.ParamAddress, sIni.ParamValue, sIni.ParamLen, sIni.ParamType, sIni.ParamProp);
            P_iniData.Add(sIni);
            dataGridView1.DataSource = null;//设置绑定数据为空
            dataGridView1.Rows.Clear();//清空表格内部数据
            dataGridView1.DataSource = P_iniData; //绑定数据
        }

        private void dataGridView1_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null || P_iniData.Count == 0)
                {
                    return;
                }
                if (this.dataGridView1.CurrentCell.ColumnIndex == 5)
                {
                    Rectangle rectangle = dataGridView1.GetCellDisplayRectangle(dataGridView1.CurrentCell.ColumnIndex, dataGridView1.CurrentCell.RowIndex, false);
                    if (dataGridView1.CurrentCell.Value == null)
                    {
                        this.comboBox.Visible = false;
                        return;
                    }
                    string value = dataGridView1.CurrentCell.Value.ToString();
                    int row = dataGridView1.CurrentRow.Index;
                    //this.comboBox.Text = value;
                    switch (value)
                    {
                        case "Int":
                            comboBox.SelectedIndex = 0;
                            dataGridView1.Rows[row].Cells[4].Value = "4";
                            break;
                        case "Float":
                            comboBox.SelectedIndex = 1;
                            dataGridView1.Rows[row].Cells[4].Value = "4";
                            break;
                        case "Double":
                            comboBox.SelectedIndex = 2;
                            dataGridView1.Rows[row].Cells[4].Value = "8";
                            break;
                        case "Short":
                            comboBox.SelectedIndex = 3;
                            dataGridView1.Rows[row].Cells[4].Value = "2";
                            break;
                        case "Char":
                            comboBox.SelectedIndex = 4;
                            dataGridView1.Rows[row].Cells[4].Value = "1";
                            break;
                        case "Unsigned int":
                            comboBox.SelectedIndex = 5;
                            dataGridView1.Rows[row].Cells[4].Value = "4";
                            break;
                        case "Unsigned short":
                            comboBox.SelectedIndex = 6;
                            dataGridView1.Rows[row].Cells[4].Value = "2";
                            break;
                        case "Unsigned Byte":
                            comboBox.SelectedIndex = 7;
                            dataGridView1.Rows[row].Cells[4].Value = "1";
                            break;
                        case "Byte":
                            comboBox.SelectedIndex = 8;
                            dataGridView1.Rows[row].Cells[4].Value = "1";
                            break;
                        default:
                            comboBox.SelectedIndex = 8;
                            dataGridView1.Rows[row].Cells[4].Value = "1";
                            break;

                    }
                    this.comboBox.Left = rectangle.Left;
                    this.comboBox.Top = rectangle.Top;
                    this.comboBox.Width = rectangle.Width;
                    this.comboBox.Height = rectangle.Height;
                    this.comboBox.Visible = true;
                }
                else
                {
                    this.comboBox.Visible = false;
                }
            }
            catch (Exception ex)
            {
                return;
            }


        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                PasteData((DataGridView)sender);
            }
          }
        /// <summary>
        /// 实现粘贴功能，将剪贴板中的内容粘贴到DataGridView中
        /// </summary>
        /// <param name="dgv_Test"></param> 
        private void PasteData(DataGridView dgv_Test)
        {
            try
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
                //粘贴板上的数据来源于EXCEL时，每行末尾都有\n，来源于DataGridView是，最后一行末尾没有\n
                if (clipboardText.Substring(clipboardText.Length - 1, 1) == "\n")
                {
                    rownum--;
                }
                colnum = colnum / (rownum + 1);
                object[,] data; //定义object类型的二维数组
                data = new object[rownum + 1, colnum + 1];  //根据剪贴板的行列数实例化数组
                string rowStr = "";
                //对数组各元素赋值
                for (int i = 0; i <= rownum; i++)
                {
                    for (int j = 0; j <= colnum; j++)
                    {
                        //一行中的其它列
                        if (j != colnum)
                        {
                            rowStr = clipboardText.Substring(0, clipboardText.IndexOf("\t"));
                            clipboardText = clipboardText.Substring(clipboardText.IndexOf("\t") + 1);
                        }
                        //一行中的最后一列
                        if (j == colnum && clipboardText.IndexOf("\r") != -1)
                        {
                            rowStr = clipboardText.Substring(0, clipboardText.IndexOf("\r"));
                        }
                        //最后一行的最后一列
                        if (j == colnum && clipboardText.IndexOf("\r") == -1)
                        {
                            rowStr = clipboardText.Substring(0);
                        }
                        data[i, j] = rowStr;
                    }
                    //截取下一行及以后的数据
                    clipboardText = clipboardText.Substring(clipboardText.IndexOf("\n") + 1);
                }
                //获取当前选中单元格的列序号
                int colIndex = dgv_Test.CurrentRow.Cells.IndexOf(dgv_Test.CurrentCell);
                //获取当前选中单元格的行序号
                int rowIndex = dgv_Test.CurrentRow.Index;

                //获取选择单元格 以第一个单元格为0点向第四象限扩展 SelectedCells是以用户选择顺序或者拖动方向决定的 是不规律的
                //为了保证和理性,这里所取的第一个单元格是左上角的单元格 ROW 行  column列
                int startRow = dgv_Test.Rows.Count, startColumn = dgv_Test.ColumnCount; //起始的单元格坐标系
                foreach (DataGridViewCell cell in dgv_Test.SelectedCells)               //迭代比较,获取2轴数字均最小的坐标
                {
                    startRow = cell.RowIndex < startRow ? cell.RowIndex : startRow;
                    startColumn = cell.ColumnIndex < startColumn ? cell.ColumnIndex : startColumn;
                }
                int arr_x = 0, arr_y = 0;
                for (int i = startRow; i <= (rownum+ startRow) && i< dgv_Test.Rows.Count-1; i++, arr_x++)
                {
                    arr_y = 0;
                    for (int j = startColumn; j <= (colnum+ startColumn) &&j< dgv_Test.ColumnCount; j++, arr_y++)
                    {
                        dgv_Test.Rows[i].Cells[j].Value = data[arr_x, arr_y];
                    }
                }
            }
            catch
            {
                MessageBox.Show("粘贴区域大小不一致");
                return;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //for (int i = 0; i < this.dGV_Data.Rows.Count; i++)
            //{
            //    this.dGV_Data.Rows[i].Cells["选择"].Value = 0;
            //}dataGridView1
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                if ( dataGridView1.Rows[i].Cells[1].Value != null &&
                   dataGridView1.Rows[i].Cells[2].Value != null) //判断值是否为空
                {
                    this.dataGridView1.Rows[i].Cells[0].Value = true;
                }
                    
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                this.dataGridView1.Rows[i].Cells[0].Value = false;
            }
        }
        private void but_send_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            List<iniData> pt_iniData = new List<iniData>((BindingList<iniData>)this.dataGridView1.DataSource);
            //遍历行集合
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[0].Value != null && dataGridView1.Rows[i].Cells[1].Value != null &&
                    dataGridView1.Rows[i].Cells[2].Value != null) //判断值是否为空
                {
                    if (Convert.ToBoolean(dataGridView1.Rows[i].Cells[0].Value.ToString()))//判断是否选中项
                    {
                        pt_iniData[i].ft = true;//数据库数据标记
                    }
                    else
                    {
                        pt_iniData[i].ft = false;//数据库数据标记
                    }
                }
            }
            //删除集合中的指定项
            pt_iniData.RemoveAll((pp) =>
            {
                return !pp.ft;
            });


            address = 0;
            int next_address = 0;
            data_length = 0;
            byte[] data = new byte[250];
            t_Task = Task.Factory.StartNew(() =>
            {
                foreach (var stu in pt_iniData)
                {

                    if(data_length==0)
                    {
                        address = System.Convert.ToInt32(stu.ParamAddress, 16);
                        next_address = address;
                    }
                    else 
                    {
                        if(next_address != System.Convert.ToInt32(stu.ParamAddress, 16))
                        {//当前地址不等于目标加载地址则先发送之前数据
                            DXecllence_order.SEND_EEPROM_DATA(ID, address, data_length, data, ref SendByte, out SendByte_i, cmdno++);
                            RS232_Eeprom_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                            address = System.Convert.ToInt32(stu.ParamAddress, 16);
                            next_address = address;
                            data_length = 0;
                        }
                    }
                    int data_len = System.Convert.ToInt32(stu.ParamLen);
                    
                    switch (stu.ParamType)
                    {
                        case "Int":
                            int value_int;
                            int.TryParse(stu.ParamValue, out value_int);
                            Array.ConstrainedCopy(BitConverter.GetBytes(value_int), 0, data, data_length, data_len);                           
                            break;
                        case "Float":
                            float value_float;                          
                            float.TryParse(stu.ParamValue, out value_float);
                            Array.ConstrainedCopy(BitConverter.GetBytes(value_float), 0, data, data_length, data_len);
                            break;
                        case "Double":
                            //dataGridView1.Rows[row].Cells[4].Value = "8";
                            double value_double;
                            double.TryParse(stu.ParamValue, out value_double);
                            Array.ConstrainedCopy(BitConverter.GetBytes(value_double), 0, data, data_length, data_len);
                            break;
                        case "Short":
                            //dataGridView1.Rows[row].Cells[4].Value = "2";
                            short value_;
                            short.TryParse(stu.ParamValue, out value_);
                            Array.ConstrainedCopy(BitConverter.GetBytes(value_), 0, data, data_length, data_len);
                            break;
                        case "Char":                          
                            var STT = System.Text.Encoding.ASCII.GetBytes(stu.ParamValue);
                            for(int char_i = 0;char_i< data_len; char_i++)
                            {
                                if(char_i<STT.Length)
                                {
                                    data[char_i + data_length] = STT[char_i];
                                }
                                else 
                                {
                                    data[char_i + data_length] =0x20;
                                }
                            }
                            break;
                        case "Unsigned int":
                            //dataGridView1.Rows[row].Cells[4].Value = "2";
                            uint value_uint;
                            uint.TryParse(stu.ParamValue, out value_uint);
                            Array.ConstrainedCopy(BitConverter.GetBytes(value_uint), 0, data, data_length, data_len);
                            break;
                        case "Unsigned short":
                            //dataGridView1.Rows[row].Cells[4].Value = "2";
                            ushort value_ushort;
                            ushort.TryParse(stu.ParamValue, out value_ushort);
                            Array.ConstrainedCopy(BitConverter.GetBytes(value_ushort), 0, data, data_length, data_len);
                            break;
                        case "Unsigned Byte":
                            //dataGridView1.Rows[row].Cells[4].Value = "1";
                            byte value_byte;
                            byte.TryParse(stu.ParamValue, out value_byte);
                            Array.ConstrainedCopy(BitConverter.GetBytes(value_byte), 0, data, data_length, data_len);
                            break;
                        case "Byte":
                            //dataGridView1.Rows[row].Cells[4].Value = "1";
                            byte value_byte1;
                            byte.TryParse(stu.ParamValue, out value_byte1);
                            Array.ConstrainedCopy(BitConverter.GetBytes(value_byte1), 0, data, data_length, data_len);
                            break;
                        default:
                            MessageBox.Show("数据类型错误");
                            InvokeToForm(() =>
                            {
                                this.Enabled = true;
                            });
                            return;
                            break;

                    }
                    data_length += (byte)data_len;
                    next_address += (byte)data_len;
                    if (data_length > 120)
                    {//发送数据
                            DXecllence_order.SEND_EEPROM_DATA(ID, address, data_length, data, ref SendByte, out SendByte_i, cmdno++);
                            RS232_Eeprom_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                            address = 0;
                            next_address = address;
                            data_length = 0;
                    }

                }
                if (data_length > 0)
                {//发送数据
                    DXecllence_order.SEND_EEPROM_DATA(ID, address, data_length, data, ref SendByte, out SendByte_i, cmdno++);
                    RS232_Eeprom_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    address = 0;
                    next_address = address;
                    data_length = 0;
                }
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
        Task t_Task = null;
        private void InvokeToForm(Action action)
        {
            try
            {
                this.Invoke(action);

            }
            catch { }
        }
        private void but_read_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            rd_iniData = new List<iniData>((BindingList<iniData>)this.dataGridView1.DataSource);
            List<iniData> pt_iniData = new List<iniData>((BindingList<iniData>)this.dataGridView1.DataSource);
            //遍历行集合
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[0].Value != null && dataGridView1.Rows[i].Cells[1].Value != null &&
                    dataGridView1.Rows[i].Cells[2].Value != null) //判断值是否为空
                {
                    if (Convert.ToBoolean(dataGridView1.Rows[i].Cells[0].Value.ToString()))//判断是否选中项
                    {
                        pt_iniData[i].ft = true;//数据库数据标记
                    }
                    else
                    {
                        pt_iniData[i].ft = false;//数据库数据标记
                    }
                }
            }
            //删除集合中的指定项
            pt_iniData.RemoveAll((pp) =>
            {
                return !pp.ft;
            });


            address = 0;
            int next_address = 0;
            data_length = 0;
            byte[] data = new byte[250];
            t_Task = Task.Factory.StartNew(() =>
            {
                foreach (var stu in pt_iniData)
                {

                    if (data_length == 0)
                    {
                        address = System.Convert.ToInt32(stu.ParamAddress, 16);
                        next_address = address;
                    }
                    else
                    {
                        if (next_address != System.Convert.ToInt32(stu.ParamAddress, 16))
                        {//当前地址不等于目标加载地址则先发送之前数据
                            DXecllence_order.READ_EEPROM_DATA(ID, address, data_length, ref SendByte, out SendByte_i, cmdno++);
                            RS232_Eeprom_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                            address = System.Convert.ToInt32(stu.ParamAddress, 16);
                            next_address = address;
                            data_length = 0;
                        }
                    }
                    int data_len = System.Convert.ToInt32(stu.ParamLen);
                    data_length += (byte)data_len;
                    next_address += (byte)data_len;
                    if (data_length > 120)
                    {//发送数据
                        DXecllence_order.READ_EEPROM_DATA(ID, address, data_length, ref SendByte, out SendByte_i, cmdno++);
                        RS232_Eeprom_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                        address = 0;
                        next_address = address;
                        data_length = 0;
                    }
                }
                if (data_length > 0)
                {//发送数据
                    DXecllence_order.READ_EEPROM_DATA(ID, address, data_length, ref SendByte, out SendByte_i, cmdno++);
                    RS232_Eeprom_SEND(sender, SendByte, SendByte_i, false, true, 1000);
                    address = 0;
                    next_address = address;
                    data_length = 0;
                }
                InvokeToForm(() =>
                {
                    dataGridView1.DataSource = null; //绑定为空
                    P_iniData = new BindingList<iniData>(rd_iniData);
                    dataGridView1.DataSource = P_iniData;  //绑定到数据集合
                    //标记第一行数据
                    for(int i =0; i<rd_iniData.Count;i++)
                    {
                        if(rd_iniData[i].ft==true)
                        { dataGridView1.Rows[i].Cells[0].Value = true; }
                    }
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

        private void but_del_data_Click(object sender, EventArgs e)
        {
            //P_iniData = new BindingList<iniData>(P_iniData1);  //List<T> modelList=new List<T>((BindingList<T>)this.DataGridView.DataSource);
            List<iniData> pt_iniData = new List<iniData>((BindingList<iniData>)this.dataGridView1.DataSource);
            //遍历行集合
            for (int i = 0; i < dataGridView1.Rows.Count; i++)                                       
            {
                if (dataGridView1.Rows[i].Cells[0].Value != null && dataGridView1.Rows[i].Cells[1].Value != null &&
                    dataGridView1.Rows[i].Cells[2].Value != null) //判断值是否为空
                {
                    if (Convert.ToBoolean(dataGridView1.Rows[i].Cells[0].Value.ToString()))//判断是否选中项
                    {
                        pt_iniData.RemoveAll( //标记集合中的指定项
                            (pp) =>
                            {
                                if (pp.ParamAddress == dataGridView1.Rows[i].Cells[2].Value.ToString())
                                    pp.ft = true;                                                    //开始标记
                        return false;                                                        //不删除项
                    });
                    }
                }
            }
            //删除集合中的指定项
            pt_iniData.RemoveAll((pp) =>
                               {
                                   return pp.ft;
                               });
            dataGridView1.DataSource = null; //绑定为空
            P_iniData = new BindingList<iniData>(pt_iniData);
            dataGridView1.DataSource = P_iniData;  //绑定到数据集合
        }

        
    }

}

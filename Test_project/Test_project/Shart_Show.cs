using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Test.Shart_Show
{
    public delegate void SelectionRangeChanged(object sender, CursorEventArgs e);//声明一个委托类型，选择范围改变

    public partial class Shart_Show : UserControl
    {
        public SelectionRangeChanged onSelectionRangeChanged;
        double x1;
        double x2;
        double x3;
        double y1;
        double y2;
        double y3;
        public int xianshi = 0;

        public double x_start = 0;
        public double x_end = 0;
        int t = 0;
        public Shart_Show()
        {
            InitializeComponent();
            x1 = chart1.ChartAreas[0].AxisX.ScaleView.Position;
            //X轴视图长度
            x2 = chart1.ChartAreas[0].AxisX.ScaleView.Size;
            x3 = chart1.ChartAreas[0].AxisX.Interval;  //进行四舍五入
            y1 = chart1.ChartAreas[0].AxisY.ScaleView.Position;
            //X轴视图长度
            y2 = chart1.ChartAreas[0].AxisY.ScaleView.Size;
            y3 = chart1.ChartAreas[0].AxisY.Interval;  //进行四舍五入
            //启用X游标，以支持局部区域选择放大
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].CursorX.LineColor = Color.Empty;//Color.Pink; //
            chart1.ChartAreas[0].CursorX.IntervalType = DateTimeIntervalType.Auto;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
            // chart1.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.None;//.All;//启用X轴滚动条按钮

            //////启用y游标，以支持局部区域选择放大
            chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].CursorY.LineColor = Color.Empty;//Color.Pink;
            chart1.ChartAreas[0].CursorY.IntervalType = DateTimeIntervalType.Auto;
            chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = false;
        }
        /// <summary>
        ///添加一跳曲线到窗口
        /// </summary>
        /// <param name="series_name">曲线的名称</param>
        ///  <param name="name">曲线显示名称</param>
        ///  <param name="Legend_name">曲线图例名称</param>
        ///  <param name="LegendText_name">曲线图例显示名称</param>
        ///  <param name="series_color">曲线显示颜色</param>
        public void ADD_series(System.Windows.Forms.DataVisualization.Charting.Series series_name, string name, string Legend_name, string LegendText_name,Color series_color)
        {
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();

            series_name = new System.Windows.Forms.DataVisualization.Charting.Series();
            series_name.ChartArea = "ChartArea1";
            series_name.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series_name.Color = series_color;
            series_name.Legend = Legend_name;
            series_name.LegendText = LegendText_name;
            series_name.MarkerBorderColor = series_color;
            series_name.MarkerColor = series_color;
            series_name.Name = name;
            series_name.ToolTip = "#LEGENDTEXT:#VALX,#VAL";
            series_name.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series_name.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            //series_name.c
            this.chart1.Series.Add(series_name);
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
        public void Shart_Show_Auto2(int Width, int Height)
        {
            x1 = chart1.ChartAreas[0].AxisX.ScaleView.Position;
            //X轴视图长度
            x2 = chart1.ChartAreas[0].AxisX.ScaleView.Size;
            x3 = chart1.ChartAreas[0].AxisX.Interval;  //进行四舍五入
            y1 = chart1.ChartAreas[0].AxisY.ScaleView.Position;
            //X轴视图长度
            y2 = chart1.ChartAreas[0].AxisY.ScaleView.Size;
            y3 = chart1.ChartAreas[0].AxisY.Interval;  //进行四舍五入
        }


        public void button1_Click(object sender, EventArgs e)
        {
            //X轴视图起点
            chart1.ChartAreas[0].AxisX.ScaleView.Position = 0; //x1;
            //X轴视图长度
            chart1.ChartAreas[0].AxisX.ScaleView.Size = x2;
            chart1.ChartAreas[0].AxisX.Interval = x3;  //进行四舍五入           

            chart1.ChartAreas[0].AxisY.ScaleView.Position = y1 ;
            //X轴视图长度
            chart1.ChartAreas[0].AxisY.ScaleView.Size = y2 ;
            chart1.ChartAreas[0].AxisY.Interval = y3;
        }

        private void chart1_SelectionRangeChanged(object sender, CursorEventArgs e)
        {
            //无数据时返回
            if (chart1.Series[0].Points.Count == 0)
                return;
            // string s = System.Convert.ToString(textboxx.Name);
            string s = System.Convert.ToString(e.Axis.AxisName);
            double start_position = 0.0;
            double end_position = 0.0;
            double myInterval = 0.0;

            start_position = e.NewSelectionStart;//起始位置
            //e.
            end_position = e.NewSelectionEnd;//结束位置
            myInterval = Math.Abs(start_position - end_position);
            double s1 = Math.Round(myInterval / 6, 3);  //进行四舍五入myInterval/6;
            if (myInterval == 0.0)
            { return; }
            if (s == "X")
            {
                x_start = start_position;
                x_end = end_position;
                //xianshi = 1;
                ////X轴视图起点
                //chart1.ChartAreas[0].AxisX.ScaleView.Position = Math.Min(start_position, end_position);
                ////X轴视图长度
                //chart1.ChartAreas[0].AxisX.ScaleView.Size = myInterval;
                //chart1.ChartAreas[0].AxisX.Interval = s1;  //进行四舍五入myInterval/6;
                // chart1.ChartAreas[0].AxisX.Interval = 0;
            }
            if (s == "Y")
            {
                if (x_start < x_end)
                {
                    xianshi = 1;
                    ////X轴视图起点
                    chart1.ChartAreas[0].AxisX.ScaleView.Position = Math.Min(x_start, x_end);
                    ////X轴视图长度
                    chart1.ChartAreas[0].AxisX.ScaleView.Size = Math.Abs(x_start - x_end);
                    chart1.ChartAreas[0].AxisX.Interval = Math.Round(Math.Abs(x_start - x_end) / 6, 3);  //进行四舍五入myInterval/6;

                    //Y轴视图起点
                    chart1.ChartAreas[0].AxisY.ScaleView.Position = Math.Min(start_position, end_position);
                    //Y轴视图长度
                    chart1.ChartAreas[0].AxisY.ScaleView.Size = myInterval;
                    chart1.ChartAreas[0].AxisY.Interval = s1;  //进行四舍五入  
                    if (onSelectionRangeChanged != null && checkCount.Checked == true)//不等于空，说明它已经订阅了具体的方法（即它已经引用了具体的方法）
                    {
                        onSelectionRangeChanged(sender, e);
                    }
                }
                else
                {
                    chart1.ChartAreas[0].AxisX.ScaleView.Position = x1;
                    //X轴视图长度
                    chart1.ChartAreas[0].AxisX.ScaleView.Size = x2;
                    chart1.ChartAreas[0].AxisX.Interval = x3;  //进行四舍五入           

                    chart1.ChartAreas[0].AxisY.ScaleView.Position = y1;
                    //X轴视图长度
                    chart1.ChartAreas[0].AxisY.ScaleView.Size = y2;
                    chart1.ChartAreas[0].AxisY.Interval = x3;  //进行四舍五入
                    xianshi = 0;
                }
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(chart1.Series.Count == 2)
            {
                chart1.Series.Remove(chart1.Series[1]);
            }
        }
    }
}

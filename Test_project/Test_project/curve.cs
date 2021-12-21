using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Test_project
{
    public partial class curve : Form
    {
        double x1;
        double x2;
        double x3;
        double y1;
        double y2;
        double y3;
        public int xianshi = 0;

        double x_start = 0;
        double x_end = 0;
        public curve()
        {
            InitializeComponent();
            //  x1 = 0 / 0;
            x1 = chart1.ChartAreas[0].AxisX.ScaleView.Position;
            //X轴视图长度
            x2 = chart1.ChartAreas[0].AxisX.ScaleView.Size;
            x3 = chart1.ChartAreas[0].AxisX.Interval;  //进行四舍五入
            y1 = chart1.ChartAreas[0].AxisY.ScaleView.Position;
            //X轴视图长度
            y2 = chart1.ChartAreas[0].AxisY.ScaleView.Size;
            y3 = chart1.ChartAreas[0].AxisX.Interval;  //进行四舍五入
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
        protected override void WndProc(ref   Message m)
        {
           const int WM_SYSCOMMAND = 0x0112;
           const int SC_CLOSE = 0xF060;
           if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == SC_CLOSE)
           {
               return;
            }
            base.WndProc(ref m);
       }
        //图表区域局部放大
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
                }
                else
                {
                    chart1.ChartAreas[0].AxisX.ScaleView.Position = x1;
                    //X轴视图长度
                    chart1.ChartAreas[0].AxisX.ScaleView.Size = x2;
                    chart1.ChartAreas[0].AxisX.Interval = x3;  //进行四舍五入           

                    chart1.ChartAreas[0].AxisY.ScaleView.Position = y1 ;
                    //X轴视图长度
                    chart1.ChartAreas[0].AxisY.ScaleView.Size = y2 ;
                    chart1.ChartAreas[0].AxisY.Interval = x3;  //进行四舍五入
                    xianshi = 0;
                }

            }

        }

        private void curve_Load(object sender, EventArgs e)
        {

        }
    }
}

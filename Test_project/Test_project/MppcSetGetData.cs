using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using System.Windows.Forms;
namespace Test_project
{
    public class MppcSetGetData
    {
        /// <summary>写入编号</summary>
        public int Number { get; set; }
        /// <summary>写入时间</summary>
        public string Time { get; set; }
        /// <summary>荧光光头编号</summary>
        public int LightHeaadNumber { get; set; }
        /// <summary>PCB编号</summary>
        public int PCBNumber { get; set; }
        /// <summary>荧光通道</summary>
        public string FluorescenceChannel { get; set; }
        ///<summary>  FAM通道信息</summary>
        public float Vbr { get; set; }
        public float Vov { get; set; }
        public float Tvop { get; set; }
        public float Temp_K2 { get; set; }
        public float Temp_K1 { get; set; }
        public float Temp_B { get; set; }
        public float DAC_K2 { get; set; }
        public float DAC_K1 { get; set; }
        public float DAC_B { get; set; }
        public float ADC_K2 { get; set; }
        public float ADC_K1 { get; set; }
        public float ADC_B { get; set; }
        public float LED_Vi { get; set; }
        public float LED_K2 { get; set; }
        public float LED_K1 { get; set; }
        public float LED_B { get; set; }
        public float I_K2 { get; set; }
        public float I_K1 { get; set; }
        public float I_B { get; set; }

    }
    public sealed class MppcSetGetDataMap : ClassMap<MppcSetGetData>
    {
        public MppcSetGetDataMap()
        {
            Map(m => m.Number).Index(0);
            Map(m => m.Time).Index(1);
            Map(m => m.LightHeaadNumber).Index(2);
            Map(m => m.PCBNumber).Index(3);
            Map(m => m.FluorescenceChannel).Index(4);
            Map(m => m.Vbr).Index(5);
            Map(m => m.Vov).Index(6);
            Map(m => m.Tvop).Index(7);
            Map(m => m.Temp_K2).Index(8);
            Map(m => m.Temp_K1).Index(9);
            Map(m => m.Temp_B).Index(10);
            Map(m => m.DAC_K2).Index(11);
            Map(m => m.DAC_K1).Index(12);
            Map(m => m.DAC_B).Index(13);
            Map(m => m.ADC_K2).Index(14);
            Map(m => m.ADC_K1).Index(15);
            Map(m => m.ADC_B).Index(16);
            Map(m => m.LED_Vi).Index(17);
            Map(m => m.LED_K2).Index(18);
            Map(m => m.LED_K1).Index(19);
            Map(m => m.LED_B).Index(20);
            Map(m => m.I_K2).Index(21);
            Map(m => m.I_K1).Index(22);
            Map(m => m.I_B).Index(23);


        }
    }
    class Original_DATA_CSV
    {
        /// <summary>写入编号</summary>
        public int I { get; set; }
        /// <summary>写入编号</summary>
        public int ROX { get; set; }
        /// <summary>写入编号</summary>
        public int FAM { get; set; }
        /// <summary>写入编号</summary>
        public int CY5 { get; set; }
        /// <summary>写入编号</summary>
        public int VIC { get; set; }

    }
}

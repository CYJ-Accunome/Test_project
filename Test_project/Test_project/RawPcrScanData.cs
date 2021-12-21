using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace Test_project
{
   public  class RawPcrScanData
    {
        /// <summary>PCR试剂ID</summary>
        public string PcrReagentId { get; set; }
        /// <summary>荧光通道</summary>
        public string FluorescenceChannel { get; set; }
        /// <summary>荧光数据</summary>
        public double FluorescenceData { get; set; }
        /// <summary>测试次数</summary>
        public int IterationNumber { get; set; }
        /// <summary>PCR管位置</summary>
        public string PcrTubeLocation { get; set; }
        /// <summary>步数(实验数据)</summary>
        public int StepNumber { get; set; }
        /// <summary>阶段名称</summary>
        public string StageName { get; set; }
        /// <summary>温度测量值</summary>
        public float MeasuredTemperature { get; set; }
        /// <summary>测试订单ID</summary>
        public long TestOrderId { get; set; }
        /// <summary>样本条形码</summary>
        public string SampleBarcode { get; set; }
        /// <summary>分析版本</summary>
        public string AssayVersion { get; set; }
        /// <summary>实验名称</summary>
        public string AssayName { get; set; }
        /// <summary>目标温度</summary>
        public float TargetTemperature { get; set; }
    }

    public class DataConfigration
    {
        /// <summary>PCR试剂ID</summary>
        public string Test_Group { get; set; }
        /// <summary>荧光通道</summary>
        public string FluorescenceChannel { get; set; }
        /// <summary>浓度</summary>
        public string IonContent { get; set; }
        /// <summary>PCR管位置</summary>
        public string PcrTubeLocation { get; set; }
        /// <summary>说明</summary>
        public string Explain { get; set; }
    }
    public sealed class DataConfigrationMap : ClassMap<DataConfigration>
    {
        public DataConfigrationMap()
        {
            Map(m => m.Test_Group).Index(0);
            Map(m => m.FluorescenceChannel).Index(1);
            Map(m => m.IonContent).Index(2);
            Map(m => m.PcrTubeLocation).Index(3);
            Map(m => m.Explain).Index(4);
        }
    }
    public sealed class RawPcrScanDataMap : ClassMap<RawPcrScanData>
    {
        public RawPcrScanDataMap()
        {
            Map(m => m.AssayName).Index(0);
            Map(m => m.AssayVersion).Index(1);
            Map(m => m.SampleBarcode).Index(2);
            Map(m => m.TestOrderId).Index(3);
            Map(m => m.PcrReagentId).Index(4);
            Map(m => m.StageName).Index(5);
            Map(m => m.StepNumber).Index(6);
            Map(m => m.PcrTubeLocation).Index(7);
            Map(m => m.IterationNumber).Index(8);
            Map(m => m.FluorescenceData).Index(9);
            Map(m => m.FluorescenceChannel).Index(10);             
            Map(m => m.TargetTemperature).Index(11);
            Map(m => m.MeasuredTemperature).Index(12);
        }
    }
    public class TESTConfigration
    {
        /// <summary>分组</summary>
        public string Group { get; set; }
        /// <summary>步骤</summary>
        public string Number { get; set; }
        /// <summary>荧光通道</summary>
        public string FluorescenceChannel { get; set; }
        /// <summary>测试方法</summary>
        public string flow_path { get; set; }
        /// <summary>测试类型</summary>
        public string Type { get; set; }
        /// <summary>延时</summary>
        public string Delay_Time { get; set; }
        /// <summary>参数1</summary>
        public string string1 { get; set; }
        /// <summary>参数2</summary>
        public string string2 { get; set; }
        /// <summary>参数3</summary>
        public string string3 { get; set; }
        /// <summary>参数4</summary>
        public string string4 { get; set; }
        /// <summary>参数5</summary>
        public string string5{ get; set; }
        /// <summary>参数6</summary>
        public string string6{ get; set; }
        /// <summary>参数7</summary>
        public string string7{ get; set; }
        /// <summary>参数8</summary>
        public string string8{ get; set; }
        /// <summary>参数9</summary>
        public string string9{ get; set; }

}
}

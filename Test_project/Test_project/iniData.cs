using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
namespace Test_project
{
    public class iniData
    {      
        /// <summary>参数名称</summary>
        public string ParamName { get; set; }
        /// <summary>地址</summary>
        public string ParamAddress { get; set; }
        /// <summary>内容</summary>
        public string ParamValue { get; set; }
        /// <summary>长度</summary>
        public string ParamLen { get; set; }
        /// <summary>数据类型</summary>
        public string ParamType { get; set; }
        /// <summary>预留</summary>
        public string ParamProp { get; set; }
        public bool ft;
    }
    public sealed class iniDataMap : ClassMap<iniData>
    {
        public iniDataMap()
        {
            Map(m => m.ParamName).Index(0);
            Map(m => m.ParamAddress).Index(1);
            Map(m => m.ParamValue).Index(2);
            Map(m => m.ParamLen).Index(3);
            Map(m => m.ParamType).Index(4);
            Map(m => m.ParamProp).Index(5);  
        }
    }
}

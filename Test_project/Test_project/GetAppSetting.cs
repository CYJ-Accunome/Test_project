using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace Test_project
{
    class GetAppSetting
    {
        //string path1 = System.Configuration.ConfigurationManager.AppSettings["PATH"];  //读取上次保存的路径
        /// <summary>
        /// 将字符串n，写入配置文件键值为connectionName里面
        /// </summary>
        /// <param name="connectionName">键值</param>
        /// <param name="n">写入的值</param>
        /// <returns>null</returns>
        public static string GetAppSetting_data(string connectionName, string n)
        {//需要添加using System.Configuration;引用，并且在解决方案内部也要添加引用
            //获取Configuration对象
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.Settings[connectionName].Value;
            //写入<add>元素的Value
            config.AppSettings.Settings[connectionName].Value = n;
            //增加<add>元素
            // config.AppSettings.Settings.Add("url", "http://www.xieyc.com");
            //删除<add>元素
            //  config.AppSettings.Settings.Remove("textBox1");
            //一定要记得保存，写不带参数的config.Save()也可以
            config.Save(ConfigurationSaveMode.Modified);
            //刷新，否则程序读取的还是之前的值（可能已装入内存）
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            string s = "";
            return s;
        }
    }
}

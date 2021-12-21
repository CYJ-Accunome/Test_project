using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Test_project
{
    class INIClass
    {
        /// <summary>  
        /// 写操作 
        /// </summary>  
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>  
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        /// <summary>  
        /// 读操作 
        /// </summary>  
        /// <param name="section">节</param>
        /// <param name="key">键</param>
        /// <param name="def">未读到的默认值</param>
        /// <param name="retVal">读取到的值</param>
        /// <param name="size">大小</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>  
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>  
        /// 写INI文件 
        /// </summary>  
        /// <param name="Section">节</param>  
        /// <param name="Key">键</param>  
        /// <param name="Value">值</param>  
        ///  <param name="filepath">文件路径</param>
        /// <returns></returns>  
        public static void IniWriteValue(string Section, string Key, string Value,string filepath)
        {
            try
            {
                WritePrivateProfileString(Section, Key, Value, filepath);
            }
            catch (Exception) 
            {
                StackFrame callstack = new StackFrame(0, true);
                throw new Exception($"INI文件写入错误\r\nFile:{callstack.GetFileName()}, Line:{callstack.GetFileLineNumber()}");
            }
        }
        /// <summary>  
        /// 读取INI文件 
        /// </summary>  
        /// <param name="Section">节</param>  
        /// <param name="Key">键</param>  
        /// <param name="defValue">未读取到值时的默认值</param>
        /// <param name="filepath">文件路径</param>
        /// <returns>读到的值</returns>
        public static string IniReadValue(string Section, string Key,string defValue,string filepath)
        {
            try
            {
                StringBuilder temp = new StringBuilder(1024);
                int i = GetPrivateProfileString(Section, Key, defValue, temp, 1024, filepath);
                return temp.ToString();
            }
            catch (Exception) 
            {
                StackFrame callstack = new StackFrame(0, true);
                throw new Exception($"INI文件读取错误\r\nFile:{callstack.GetFileName()}, Line:{callstack.GetFileLineNumber()}");
            }
        }
        /// <summary>  
        /// 删除节 
        /// </summary>  
        /// <param name="Section">节</param>  
        /// <param name="filepath">文件路径</param>
        /// <returns></returns> 
        public static long DeleteSection(string Section, string filepath)
        {
            try
            {
                return WritePrivateProfileString(Section, null, null, filepath);
            }
            catch (Exception)
            {
                StackFrame callstack = new StackFrame(0, true);
                throw new Exception($"INI文件删除错误\r\nFile:{callstack.GetFileName()}, Line:{callstack.GetFileLineNumber()}");
            }
        }
        /// <summary>  
        /// 删除键
        /// </summary>  
        /// <param name="Section">节</param> 
        /// <param name="Key">键</param>
        /// <param name="filepath">文件路径</param>
        /// <returns></returns> 
        public static long DeleteKey(string Section, string Key, string filepath)
        {
            try
            {
                return WritePrivateProfileString(Section, Key, null, filepath);
            }
            catch (Exception)
            {
                StackFrame callstack = new StackFrame(0, true);
                throw new Exception($"INI文件删除错误\r\nFile:{callstack.GetFileName()}, Line:{callstack.GetFileLineNumber()}");
            }
        }
    }
}

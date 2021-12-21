using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_project
{
    class IO_Operate
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);
        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 0x40;
        public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);
        /// <summary>
        ///文件路径的判定，当不存在则增加路径
        /// </summary>
        /// <param name="PATH">带有文件路径的文件名</param>
        /// <returns>带有文件路径的文件名</returns>
        public static void PATH_creation(string PATH)
        {
            //创建根目录文件
            if (Directory.Exists(PATH) == false)//根目录不存在责创建
            {
                string PATHt = PATH.Substring(0, PATH.LastIndexOf("\\"));  //截取目录的上一层目录
                PATH_creation(PATHt);
                Directory.CreateDirectory(PATH);
            }
        }
        /// <summary>
        ///文件创建，如果已存在则删除在创建，删除失败则增加时间创建
        /// </summary>
        /// <param name="PATH">带有文件路径的文件名</param>
        /// <param name="Delete_t">为true则删除文件，否则不删除</param>
        /// <returns>带有文件路径的文件名</returns>
        public static void File_creation(ref string PATH,bool Delete_t)
        {
            string PATHt = PATH.Substring(0, PATH.LastIndexOf("\\"));  //截取目录的上一层目录
            PATH_creation(PATHt);
            if (!File.Exists(PATH))
            {//如果指定文件不存在，责重新创建文件
                File.Create(PATH).Close();//创建文件
            }
            else
            {
                if(Delete_t)
                {
                    try
                    {
                        File.Delete(PATH);
                    }
                    catch
                    {
                        //识别文件名
                        string PATH1 = PATH.Substring(0, PATH.LastIndexOf("."));
                        string PATH2 = PATH.Substring(PATH.LastIndexOf("."), PATH.Length - PATH.LastIndexOf("."));
                        System.DateTime currentTime = new System.DateTime();
                        currentTime = System.DateTime.Now;
                        PATH = PATH1 + currentTime.ToString("yyyyMMddHHmmss") + PATH2;
                    }
                    File.Create(PATH).Close();//创建文件
                    //IntPtr vHandle = _lopen(PATH, OF_READWRITE | OF_SHARE_DENY_NONE);
                    //if (vHandle == HFILE_ERROR)
                    //{
                    //    //识别文件名
                    //    string PATH1 = PATH.Substring(0, PATH.LastIndexOf("."));
                    //    string PATH2 = PATH.Substring(PATH.LastIndexOf("."), PATH.Length - PATH.LastIndexOf("."));
                    //    System.DateTime currentTime = new System.DateTime();
                    //    currentTime = System.DateTime.Now;
                    //    PATH = PATH1 + currentTime.ToString("yyyyMMddHHmmss") + PATH2;
                    //}
                    //else
                    //{
                    //    File.Delete(PATH);
                    //}
                }

            }

        }
    }
}

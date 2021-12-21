using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test_project
{
    public class FileComparer : IComparer<string>
    {
        [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
        public int Compare(string psz1, string psz2)
        {
            return StrCmpLogicalW(psz1, psz2);//升序
            //return StrCmpLogicalW(psz2, psz1);//降序
        }
    }
    class CSV_Read_Write
    {
        /// <summary>
        ///读取表格数据0
        /// </summary>
        ///<param name="Count_Randoms">是否处理数据</param>
        ///<param name="auto_time"/>是否自动变化时间</param>
        /// </summary>
        /// 
        public static void Read_data(bool Count_Randoms,bool auto_time)
        {
            string path1 = System.Configuration.ConfigurationManager.AppSettings["CSV_PATH"];  //读取上次保存的路径
            if (Directory.Exists(path1) == false)//判断路径是否存在
            {
                path1 = System.Environment.CurrentDirectory;//获取绝对路径
                path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
            }
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = path1;
            dialog.Description = "请选择根目录";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MessageBox.Show("文件夹路径不能为空", "提示");
                    StackFrame callstack = new StackFrame(0, true);
                    throw new Exception($"文件夹路径不能为空\r\nFile:{callstack.GetFileName()}, Line:{callstack.GetFileLineNumber()}");
                }
                path1 = dialog.SelectedPath;//保存当前读取的文件路径
                GetAppSetting.GetAppSetting_data("CSV_PATH", path1);//保存本次打开路径          
                if(Directory.Exists(path1+"1")==false)//根目录不存在责创建
                {
                    Directory.CreateDirectory(path1 + "1");
                }
                //下面开始遍历文件夹
                DirectoryInfo TheFolder1 = new DirectoryInfo(path1);
                if (!TheFolder1.Exists)
                {
                    StackFrame callstack = new StackFrame(0, true);
                    throw new Exception($"目录不存在\r\nFile:{callstack.GetFileName()}, Line:{callstack.GetFileLineNumber()}");
                }
                DirectoryInfo root = new DirectoryInfo(path1);
                DirectoryInfo[] dics = root.GetDirectories();
                dics = dics.OrderBy(x => x.Name, new FileComparer()).ToArray();
                var dateTime = DateTime.Now;
                foreach (var d in dics)
                {

                    //创建根目录文件
                    if (Directory.Exists(path1 + "1\\"+d.Name) == false)//根目录不存在责创建
                    {
                        Directory.CreateDirectory(path1 + "1\\" + d.Name);
                    }
                    var l = Task<List<RawPcrScanData>>.Run(() => {
                        var names = d.GetFiles().Select(t => t.FullName);
                        var lists = new List<RawPcrScanData>();
                        //ConcurrentQueue<List<RawPcrScanData>> q = new ConcurrentQueue<List<RawPcrScanData>>();
                        Parallel.ForEach(names, fileName =>
                        {
                            //lock (_theFileLock)
                            {
                                var r = LoadPcrRawData.Instance.OpenPcrResultFile(fileName);
                                string s = fileName.Substring(0,fileName.LastIndexOf("\\"));
                                int st = s.LastIndexOf("\\");
                                s = fileName.Substring(0, st)+"1" + fileName.Substring(st, fileName.Length - st);
                                //s = fileName.Substring(0, st)+"1" + fileName.Substring(st, fileName.Length - st);
                                if (!File.Exists(s))
                                {//如果指定文件不存在，责重新创建文件
                                    File.Create(s).Close();//创建文件
                                }
                                else
                                {//删除文件并重新创建
                                    File.Delete(s);
                                    File.Create(s).Close();//创建文件
                                }
                                if(r.Count>0)
                                {
                                    //进行数据修改
                                    if(Count_Randoms)
                                    {
                                        Random sdatan = new Random();
                                        foreach (var sData in r)
                                        {//遍历所有数据并显示
                                            sData.FluorescenceData = sData.FluorescenceData + sdatan.Next(1,200);
                                        }
                                    }                                                   
                                    //进行数据修改完毕
                                    var reader_w = new StreamWriter(s);
                                    var csv_w = new CsvWriter(reader_w, CultureInfo.InvariantCulture);
                                    csv_w.Configuration.RegisterClassMap<RawPcrScanDataMap>();
                                    csv_w.WriteRecords(r);
                                    csv_w.Flush();
                                    //csv_w.WriteRecord(s);
                                    csv_w.Dispose();
                                }
                            }
                        });                   
                        return lists;
                    });
                }
               
                DateTime dateTime1 = DateTime.Now;
            }
        }
        /// <summary>
        ///读取表格数据0
        /// </summary>
        ///<param name="Count_Randoms">是否处理数据</param>
        ///<param name="auto_time"/>是否自动变化时间</param>
        /// </summary>
        /// 
        public static void Read_data_Autotime(bool Count_Randoms, bool auto_time)
        {
            string path1 = System.Configuration.ConfigurationManager.AppSettings["CSV_PATH"];  //读取上次保存的路径
            if (Directory.Exists(path1) == false)//判断路径是否存在
            {
                path1 = System.Environment.CurrentDirectory;//获取绝对路径
                path1 = Path.GetFullPath("..");  //https://www.cnblogs.com/maanshancss/p/4074529.html  //直接获取上层目录
            }
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = path1;
            dialog.Description = "请选择根目录";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MessageBox.Show("文件夹路径不能为空", "提示");
                    StackFrame callstack = new StackFrame(0, true);
                    throw new Exception($"文件夹路径不能为空\r\nFile:{callstack.GetFileName()}, Line:{callstack.GetFileLineNumber()}");
                }
                path1 = dialog.SelectedPath;//保存当前读取的文件路径
                GetAppSetting.GetAppSetting_data("CSV_PATH", path1);//保存本次打开路径          
                if (Directory.Exists(path1 + "1") == false)//根目录不存在责创建
                {
                    Directory.CreateDirectory(path1 + "1");
                }
                //下面开始遍历文件夹
                DirectoryInfo TheFolder1 = new DirectoryInfo(path1);
                if (!TheFolder1.Exists)
                {
                    StackFrame callstack = new StackFrame(0, true);
                    throw new Exception($"目录不存在\r\nFile:{callstack.GetFileName()}, Line:{callstack.GetFileLineNumber()}");
                }
                DirectoryInfo root = new DirectoryInfo(path1);
                DirectoryInfo[] dics = root.GetDirectories();
                dics = dics.OrderBy(x => x.Name, new FileComparer()).ToArray();
                var dateTime = DateTime.Now;
                foreach (var d in dics)
                {

                    //创建根目录文件
                    if (Directory.Exists(path1 + "1\\" + d.Name) == false)//根目录不存在责创建
                    {
                        Directory.CreateDirectory(path1 + "1\\" + d.Name);
                    }
                    ///读取文件并另存
                    var names = d.GetFiles().Select(t => t.FullName);
                    var lists = new List<RawPcrScanData>();
                    foreach(var fileName in names)
                    {
                        var r = LoadPcrRawData.Instance.OpenPcrResultFile(fileName);
                        string s = fileName.Substring(0, fileName.LastIndexOf("\\"));
                        int st = s.LastIndexOf("\\");
                        s = fileName.Substring(0, st) + "1" + fileName.Substring(st, fileName.Length - st);
                        if (!File.Exists(s))
                        {//如果指定文件不存在，责重新创建文件
                            File.Create(s).Close();//创建文件
                        }
                        else
                        {//删除文件并重新创建
                            File.Delete(s);
                            File.Create(s).Close();//创建文件
                        }
                        if (r.Count > 0)
                        {
                            //进行数据修改
                            if (Count_Randoms)
                            {
                                Random sdatan = new Random();
                                foreach (var sData in r)
                                {//遍历所有数据并显示
                                    sData.FluorescenceData = sData.FluorescenceData + sdatan.Next(1, 200);
                                }
                            }
                            //进行数据修改完毕
                            var reader_w = new StreamWriter(s);
                            var csv_w = new CsvWriter(reader_w, CultureInfo.InvariantCulture);
                            csv_w.Configuration.RegisterClassMap<RawPcrScanDataMap>();
                            csv_w.WriteRecords(r);
                            csv_w.Flush();
                            //csv_w.WriteRecord(s);
                            csv_w.Dispose();
                        }
                    }
                    if (auto_time)
                    {
                        set_win_time.TIME();//自动修改系统时间
                    }
                }

                DateTime dateTime1 = DateTime.Now;
            }
        }
        /// <summary>
        ///写入CSV文件
        /// </summary>
        ///<param name="PATH">路径</param>
        ///<param name="data"/>数据</param>
        /// </summary>
        /// 
        public static void CSV_Write( string PATH, string data)
        {
            using (var stream = File.Open(PATH, FileMode.Append))
            using (var writer_data = new StreamWriter(stream, Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvWriter(writer_data, CultureInfo.InvariantCulture))
            {
                // Don't write the header again. csv.WriteComment(writer);
                csv.Configuration.HasHeaderRecord = false;
                csv.WriteField(data, false);
            }
        }
    }
}

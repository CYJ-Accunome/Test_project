using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
namespace Test_project
{
    public class LoadPcrRawData
    {
        private static readonly Lazy<LoadPcrRawData> lazy = new Lazy<LoadPcrRawData>(() => new LoadPcrRawData());
        public static LoadPcrRawData Instance { get { return lazy.Value; } }
        private LoadPcrRawData() { }

        public List<RawPcrScanData> OpenPcrResultFile(string resultFilePath)
        {
            var ResultFilePath = Path.Combine(Directory.GetCurrentDirectory(), resultFilePath);
            if (File.Exists(ResultFilePath))//判读文件路径是否存在
            {
                try
                {
                    using (var reader = new StreamReader(ResultFilePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        return csv.GetRecords<RawPcrScanData>().ToList();
                    }
                }
                catch
                {
                    return new List<RawPcrScanData>();
                }
            }
            else
            {
                return new List<RawPcrScanData>();
            }
        }
        public List<DataConfigration> OpenDataConfigrationFile(string resultFilePath)
        {
            var ResultFilePath = Path.Combine(Directory.GetCurrentDirectory(), resultFilePath);
            if (File.Exists(ResultFilePath))//判读文件路径是否存在
            {
                try
                {
                    using (var reader = new StreamReader(ResultFilePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        return csv.GetRecords<DataConfigration>().ToList();
                    }
                }
                catch
                {
                    return new List<DataConfigration>();
                }
            }
            else
            {
                return new List<DataConfigration>();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using MyExcel = Microsoft.Office.Interop.Excel;

namespace ConsoleApp1
{
    public static class EditTextClass
    {
        #region 批量修改文字
        public static void EditText()
        {
            string originFile = @"D:\测试数据\水厂\【01】粗格栅间及进水泵房（给水）\参数文本.txt";
            string targetFile = @"D:\测试数据\水厂\【01】粗格栅间及进水泵房（给水）\转换结果.txt";
            StreamReader sr = new StreamReader(originFile, Encoding.UTF8);
            StreamWriter sw = new StreamWriter(targetFile);
            string thisLine;
            bool isFirst = true;
            while ((thisLine = sr.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(thisLine) || string.IsNullOrWhiteSpace(thisLine))
                {
                    sw.WriteLine();
                    continue;
                }
                if (thisLine[0] == '#')
                {
                    if (!isFirst)
                        sw.WriteLine("#endregion");
                    sw.WriteLine("\n" + thisLine);
                    isFirst = false;
                    continue;
                }
                string UpperHead = thisLine;
                string LowerHead = thisLine[0].ToString().ToLower() + thisLine.Substring(1);
                Console.WriteLine("Dealing " + UpperHead);
                sw.WriteLine(string.Format("private double _{0}=> G.ConvertToDecimalFeet({1});", LowerHead, UpperHead));
                sw.WriteLine(string.Format("private int {0};", LowerHead));
                sw.WriteLine(string.Format("///<summary>"));
                sw.WriteLine(string.Format("///"));//
                sw.WriteLine(string.Format("///</summary>"));
                sw.WriteLine(string.Format("public int {0}{{", UpperHead));
                sw.WriteLine(string.Format("get => {0};", LowerHead));
                sw.WriteLine(string.Format("set{{"));
                sw.WriteLine(string.Format("{0} = value;", LowerHead));
                sw.WriteLine(string.Format("RaisePropertyChanged(\"{0}\");}}", UpperHead));
                sw.WriteLine(string.Format("}}"));
            }
            if (!isFirst) sw.WriteLine("#endregion");

            sr.Close();
            sw.Close();
            Console.WriteLine("Done");
        }

        #endregion
    }
}

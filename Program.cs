using SHP_KML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using AsposeGisTrans;

namespace ConsoleApp1
{
    public static class Program
    {
        static void Main(string[] args)
        {
            //批量删除文件
            //string fileUri = @"C:\ProgramData\RevitTempFile\test\";
            //DeleteDirClass.DeleteDir(fileUri);

            //批量修改文字
            //EditTextClass.EditText();

            //操作Excel
            //GetExcelBooksClass.GetExcelBooks();

            //SHP转换KML
            //SHP_KML.SHP_KML.Trans();

            //Aspose.Gis
            AsposeTrans.Main();

            Console.ReadKey();
        }
    }
}

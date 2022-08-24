using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using MyExcel = Microsoft.Office.Interop.Excel;

namespace ConsoleApp1
{
    public static class GetExcelBooksClass
    {
        #region 操作Excel
        public static void GetExcelBooks()
        {
            //EXCEL所存储的路径
            string fileName = @"D:\1公司用\水厂\0迁移\平流沉淀池\平流沉淀池_AdvectionSedimentationTank.xlsx";
            var relName = Path.GetFileNameWithoutExtension(fileName).Split('_');
            if (relName.Length != 2)
            {
                MessageBox.Show("文件名称有问题。");
                return;
            }
            string nameC = relName[0];
            string nameE = relName[1];
            //string nameC = "粗格栅间及进水泵房（排水）";
            //string nameE = "IntakePumpDrain";

            string targetFile = string.Format(@"D:\1公司用\水厂\0迁移\平流沉淀池");
            string targetFile1 = string.Format(@"{0}\{1}Window.xaml", targetFile, nameE);
            string targetFile2 = string.Format(@"{0}\{1}Window.xaml.cs", targetFile, nameE);
            string targetFile3 = string.Format(@"{0}\{1}Model.cs", targetFile, nameE);
            string targetFile4 = string.Format(@"{0}\Create{1}Cmd.cs", targetFile, nameE);
            //新建一个应用程序EXC1
            MyExcel.Application EXC1 = new MyExcel.Application();
            EXC1.Visible = false;//设置EXC1打开后可见
            MyExcel.Workbooks wbs = EXC1.Workbooks;
            MyExcel._Workbook wb = wbs.Add(fileName);//打开并显示EXCEL文件
            //Tab-(次Tab-次Tab简称)-(名称-简称-默认值-单位-绑定值-备注)
            var parameterDic = new Dictionary<string, Dictionary<Tuple<string, string>, List<Tuple<string, string, string, string, string, string>>>>();
            var bindDic = new Dictionary<string, List<string>>();

            try
            {
                string curSheetName = "", curSecTab = "", curSecTabTag = "";
                var tipName = "使用说明（必看）";
                var sheetCount = wb.Sheets.Count;
                MyExcel._Worksheet curSheet = wb.Sheets[1];
                //无内容Check
                if (!(sheetCount > 1) || curSheet == null || curSheet.Name == tipName) return;
                for (int i = 1; i < sheetCount; i++)
                {
                    if (curSheet.Name == tipName) continue;
                    curSheet = wb.Sheets[i];
                    curSheet.Activate();//激活工作表
                    curSheetName = curSheet.Name;
                    if (curSheet.Name == tipName) continue;
                    if (!parameterDic.ContainsKey(curSheet.Name))
                        parameterDic.Add(curSheetName, new Dictionary<Tuple<string, string>, List<Tuple<string, string, string, string, string, string>>>());

                    int j = 1;
                    while (!string.IsNullOrEmpty(((MyExcel.Range)curSheet.Cells[j, 1]).Text))
                    {
                        string firstCell = ((MyExcel.Range)curSheet.Cells[j, 1]).Text;
                        if (firstCell.Length > 3 && firstCell.Substring(0, 3).ToLower() == "tab")
                        {
                            curSecTab = ((MyExcel.Range)curSheet.Cells[j, 2]).Text;
                            curSecTabTag = ((MyExcel.Range)curSheet.Cells[j, 3]).Text;
                            if (!parameterDic[curSheetName].ContainsKey(new Tuple<string, string>(curSecTab, curSecTabTag)))
                                parameterDic[curSheetName].Add(new Tuple<string, string>(curSecTab, curSecTabTag), new List<Tuple<string, string, string, string, string, string>>());
                            j++;
                            continue;
                        }
                        if (string.IsNullOrEmpty(curSecTab) || string.IsNullOrEmpty(curSecTabTag))
                        {
                            MessageBox.Show(curSheetName + " 格式有误");
                            return;
                        }
                        var a = ((MyExcel.Range)curSheet.Cells[j, 1]).Text;
                        var b = ((MyExcel.Range)curSheet.Cells[j, 2]).Text;
                        var c = ((MyExcel.Range)curSheet.Cells[j, 3]).Text;
                        var d = ((MyExcel.Range)curSheet.Cells[j, 4]).Text;
                        var e = ((MyExcel.Range)curSheet.Cells[j, 5]).Text;
                        var f = ((MyExcel.Range)curSheet.Cells[j, 6]).Text;

                        var secTab = new Tuple<string, string>(curSecTab, curSecTabTag);
                        parameterDic[curSheetName][secTab].Add(new Tuple<string, string, string, string, string, string>(a, b, c, d, e, f));
                        j++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                wb?.Close();//关闭文档
                wbs?.Close();//关闭工作簿
                EXC1?.Quit();//关闭EXCEL应用程序
                System.Runtime.InteropServices.Marshal.ReleaseComObject(EXC1);//释放EXCEL应用程序的进程
            }
            //Tab-(次Tab-次Tab简称)-(名称-简称-默认值-单位-绑定值-备注)
            foreach (var tab in parameterDic)
            {
                foreach (var secTab in tab.Value)
                {
                    foreach (var item in secTab.Value)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(item.Item5))
                            {
                                string[] bindItem = item.Item5.Split('/');
                                string thisItem = string.Format("{0}_{1}", secTab.Key.Item2, item.Item2);
                                if (bindItem.Length != 3) throw new Exception();
                                var a = parameterDic[bindItem[0]].FirstOrDefault(t => t.Key.Item1 == bindItem[1]);
                                var b = a.Value.FirstOrDefault(t => t.Item1 == bindItem[2]);
                                string bindingItem = string.Format("{0}_{1}", a.Key.Item2, b.Item2);
                                if (!bindDic.ContainsKey(bindingItem))
                                {
                                    bindDic.Add(bindingItem, new List<string>());
                                    bindDic[bindingItem].Add(thisItem);
                                }
                                else
                                    bindDic[bindingItem].Add(thisItem);
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(string.Format("{0}/{1}/{2}的绑定值有问题。", tab.Key, secTab.Key.Item1, item.Item1));
                        }

                    }
                }
            }

            StreamWriter sw1 = new StreamWriter(targetFile1);
            StreamWriter sw2 = new StreamWriter(targetFile2);
            StreamWriter sw3 = new StreamWriter(targetFile3);

            try
            {
                WriteXaml(sw1, nameE, nameC, parameterDic);
                WriteXamlCs(sw2, nameE, nameC, parameterDic);
                WriteModel(sw3, nameE, nameC, parameterDic, bindDic);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                sw1.Close();
                sw2.Close();
                sw3.Close();
                Console.WriteLine("Done");
            }

        }

        #region Xaml
        private static void WriteXaml(StreamWriter sw, string NameE, string NameC, Dictionary<string, Dictionary<Tuple<string, string>, List<Tuple<string, string, string, string, string, string>>>> parameterDic)
        {
            Console.WriteLine("Deal Xaml.");
            WriteXamlFirstHalf(sw, NameE, NameC);
            WriteXamlMiddle(sw, parameterDic);
            WriteXamlSecondHalf(sw);
            Console.WriteLine("Xaml Done");
        }
        private static void WriteXamlFirstHalf(StreamWriter sw, string NameE, string NameC)
        {
            sw.WriteLine(string.Format("<Window x:Class=\"Revit.WaterPlant.Builder.{0}Window\"", NameE));
            sw.WriteLine(string.Format("        xmlns = \"{0}\"", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"));
            sw.WriteLine(string.Format("        xmlns:x = \"{0}\"", "http://schemas.microsoft.com/winfx/2006/xaml"));
            sw.WriteLine(string.Format("        xmlns:d = \"{0}\"", "http://schemas.microsoft.com/expression/blend/2008"));
            sw.WriteLine(string.Format("        xmlns:mc = \"{0}\"", "http://schemas.openxmlformats.org/markup-compatibility/2006"));
            sw.WriteLine(string.Format("        xmlns:converter = \"clr-namespace:Revit.WaterPlant.Builder.Converters\""));
            sw.WriteLine(string.Format("        xmlns:behavior = \"clr-namespace:Revit.WaterPlant.Builder.Behaviors\""));
            sw.WriteLine(string.Format("        xmlns:mpp = \"clr-namespace:Patagames.Pdf.Net.Controls.Wpf;assembly=Patagames.Pdf.Wpf\""));
            sw.WriteLine(string.Format("        mc:Ignorable = \"d\""));
            sw.WriteLine(string.Format("        Title = \"{0}\" Height = \"560\" Width = \"800\" MinHeight = \"480\" MinWidth = \"680\" >", NameC));
            sw.WriteLine(string.Format("<Window.Resources>"));
            sw.WriteLine(string.Format("        <converter:StringToImageSourceConverter x:Key=\"ImageConverter\"/>"));
            sw.WriteLine(string.Format("        <converter:IntToStringConverter x:Key=\"IntConverter\"/>"));
            sw.WriteLine(string.Format("        <converter:UintToStringConverter x:Key=\"UintConverter\"/>"));
            sw.WriteLine(string.Format("        <converter:DoubleToStringConverter x:Key=\"DoubleConverter\"/>"));
            sw.WriteLine(string.Format("        <Style x:Key=\"Btn_RemoveMouseOverSty\"  TargetType=\"Button\">"));
            sw.WriteLine(string.Format("            <Setter Property=\"Background\" Value=\"Transparent\" />"));
            sw.WriteLine(string.Format("            <Setter Property=\"Template\">"));
            sw.WriteLine(string.Format("                <Setter.Value>"));
            sw.WriteLine(string.Format("                    <ControlTemplate TargetType=\"Button\">"));
            sw.WriteLine(string.Format("                        <Grid Background=\"{{TemplateBinding Background}}\">"));
            sw.WriteLine(string.Format("                            <ContentPresenter />"));
            sw.WriteLine(string.Format("                        </Grid>"));
            sw.WriteLine(string.Format("                    </ControlTemplate>"));
            sw.WriteLine(string.Format("                </Setter.Value>"));
            sw.WriteLine(string.Format("            </Setter>"));
            sw.WriteLine(string.Format("        </Style>"));
            sw.WriteLine(string.Format("        <Style TargetType=\"TextBlock\">"));
            sw.WriteLine(string.Format("            <Setter Property=\"FontSize\" Value=\"11\"/>"));
            sw.WriteLine(string.Format("        </Style>"));
            sw.WriteLine(string.Format("    </Window.Resources>"));
            sw.WriteLine(string.Format("<Grid Margin=\"10\">"));
            sw.WriteLine(string.Format("        <Grid.RowDefinitions>"));
            sw.WriteLine(string.Format("            <RowDefinition Height=\"*\"/>"));
            sw.WriteLine(string.Format("            <RowDefinition Height=\"30\"/>"));
            sw.WriteLine(string.Format("        </Grid.RowDefinitions>"));
            sw.WriteLine(string.Format("        <TabControl x:Name=\"tabControl\" Margin=\"0\" SelectedIndex=\"{{Binding SelectedTabIndex}}\" SelectionChanged=\"Tag_SelectionChanged\">"));
        }
        private static void WriteXamlMiddle(StreamWriter sw, Dictionary<string, Dictionary<Tuple<string, string>, List<Tuple<string, string, string, string, string, string>>>> parameterDic)
        {
            int time = 0;
            //Tab-(次Tab-次Tab简称)-(名称-简称-默认值-单位-绑定值-备注)
            foreach (var tab in parameterDic)
            {
                sw.WriteLine(string.Format("<TabItem Header=\"{0}\">", tab.Key));
                sw.WriteLine("<Grid>");
                sw.WriteLine(string.Format("    <TabControl Name=\"{0}\" SelectedIndex=\"{{Binding SecondSelectedTabIndex}}\" SelectionChanged=\"SecondTag_SelectionChanged\">", "SecondTab_" + time++.ToString()));
                foreach (var secTab in tab.Value)
                {
                    sw.WriteLine(string.Format("        <TabItem Header=\"{0}\" Tag=\"{1}\">", secTab.Key.Item1, secTab.Key.Item2));
                    sw.WriteLine(string.Format("            <Grid>"));
                    sw.WriteLine(string.Format("                <Grid.ColumnDefinitions>"));
                    sw.WriteLine(string.Format("                    <ColumnDefinition  Width=\"200\"/>"));
                    sw.WriteLine(string.Format("                    <ColumnDefinition  Width=\"*\"/>"));
                    sw.WriteLine(string.Format("                </Grid.ColumnDefinitions>"));
                    sw.WriteLine(string.Format("                <ScrollViewer Grid.Column=\"0\" VerticalScrollBarVisibility=\"Auto\">"));
                    sw.WriteLine(string.Format("                    <GroupBox Grid.Row=\"0\" Header=\"{0}参数\">", secTab.Key.Item1));
                    sw.WriteLine(string.Format("                        <Grid Margin=\"0,5,5,5\">"));
                    sw.WriteLine(string.Format("                            <Grid.RowDefinitions>"));
                    var valueList = secTab.Value;
                    for (int i = 0; i < valueList.Count; i++)
                    {
                        int rows = Encoding.Default.GetByteCount(valueList[i].Item1) / 14 + 1;
                        int rowH = rows > 2 ? rows * 15 : 30;
                        sw.WriteLine(string.Format("                           <RowDefinition Height=\"{0}\"/>", rowH));
                        //自行检查高度合不合适吧。。。
                        //sw.WriteLine(string.Format("                                <RowDefinition Height=\"30\"/>"));
                    }
                    sw.WriteLine(string.Format("                                <RowDefinition Height=\"*\"/>"));
                    sw.WriteLine(string.Format("                            </Grid.RowDefinitions>"));
                    for (int j = 0; j < valueList.Count; j++)
                    {
                        sw.WriteLine(string.Format("                            <Grid Grid.Row=\"{0}\">", j));
                        sw.WriteLine(string.Format("                                <Grid.ColumnDefinitions>"));
                        sw.WriteLine(string.Format("                                    <ColumnDefinition Width=\"6*\"/>"));
                        sw.WriteLine(string.Format("                                    <ColumnDefinition Width=\"3*\"/>"));
                        sw.WriteLine(string.Format("                                    <ColumnDefinition Width=\"25\"/>"));
                        sw.WriteLine(string.Format("                                </Grid.ColumnDefinitions>"));
                        sw.WriteLine(string.Format("                                <TextBlock Grid.Column=\"0\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" TextWrapping=\"Wrap\"  Margin=\"5,0,5,0\" Text=\"{0} {1}:\"></TextBlock> ", valueList[j].Item1, valueList[j].Item2));
                        sw.WriteLine(string.Format("                                <TextBox x:Name=\"Tbx{0}_{1}\" Tag=\"{0}\" Grid.Column=\"1\" Height=\"25\" VerticalContentAlignment=\"Center\" ", secTab.Key.Item2, valueList[j].Item2));
                        sw.WriteLine(string.Format("                                                        Text=\"{{Binding {0}_{1}, Converter={{StaticResource {2}Converter}}}}\"", secTab.Key.Item2, valueList[j].Item2, valueList[j].Item4.ToLower() == "m" ? "Double" : "Int"));
                        sw.WriteLine(string.Format("                                                        PreviewMouseLeftButtonDown=\"GetFocusBtn_OnClick\" PreviewKeyDown=\"On{0}PreviewKeyDown\" InputMethod.IsInputMethodEnabled=\"False\"", valueList[j].Item2.ToLower() == "lv" && valueList[j].Item4.ToLower() == "m" ? "Lv" : "Sub"));
                        sw.WriteLine(string.Format("                                                        GotFocus=\"Tbx_GotFocus\" LostFocus=\"Tbx_LostFocus\"{0}/>", !(string.IsNullOrEmpty(valueList[j].Item5)) ? " IsEnabled=\"False\"" : ""));
                        sw.WriteLine(string.Format("                                <TextBlock HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Grid.Column=\"2\" Text=\"{0}\" ></TextBlock> ", valueList[j].Item4));
                        sw.WriteLine(string.Format("                            </Grid>"));
                    }
                    sw.WriteLine(string.Format("                        </Grid>"));
                    sw.WriteLine(string.Format("                    </GroupBox>"));
                    sw.WriteLine(string.Format("                </ScrollViewer>"));
                    sw.WriteLine(string.Format("                <Grid Grid.Column=\"1\" Margin=\"5\" >"));
                    sw.WriteLine(string.Format("                    <Grid.RowDefinitions>"));
                    sw.WriteLine(string.Format("                        <RowDefinition Height=\"*\"/>"));
                    sw.WriteLine(string.Format("                        <RowDefinition Height=\"*\"/>"));
                    sw.WriteLine(string.Format("                        <RowDefinition Height=\"*\"/>"));
                    sw.WriteLine(string.Format("                        <RowDefinition Height=\"*\"/>"));
                    sw.WriteLine(string.Format("                        <RowDefinition Height=\"*\"/>"));
                    sw.WriteLine(string.Format("                    </Grid.RowDefinitions>"));
                    sw.WriteLine(string.Format("                    <Grid.ColumnDefinitions>"));
                    sw.WriteLine(string.Format("                        <ColumnDefinition Width=\"*\"/>"));
                    sw.WriteLine(string.Format("                        <ColumnDefinition Width=\"*\"/>"));
                    sw.WriteLine(string.Format("                        <ColumnDefinition Width=\"*\"/>"));
                    sw.WriteLine(string.Format("                        <ColumnDefinition Width=\"*\"/>"));
                    sw.WriteLine(string.Format("                    </Grid.ColumnDefinitions>"));
                    sw.WriteLine(string.Format("                    <Border Grid.Row=\"0\" Grid.Column=\"0\" Grid.RowSpan=\"5\" Grid.ColumnSpan=\"4\" x:Name=\"{0}_Border\" Padding=\"5\"  BorderBrush=\"LightGray\" BorderThickness=\"1\">", secTab.Key.Item2));
                    sw.WriteLine(string.Format("                        <ScrollViewer CanContentScroll=\"True\" HorizontalScrollBarVisibility=\"Visible\" VerticalContentAlignment=\"Center\">"));
                    sw.WriteLine(string.Format("                            <mpp:PdfViewer x:Name=\"{0}_PdfViewer\"  PageBackColor=\"Transparent\" Tag=\"{{Binding ImageFileNames.Item1}}\" ", secTab.Key.Item2));
                    sw.WriteLine(string.Format("                                                           Zoom =\" 0.43\" SizeMode=\"Zoom\" MouseWheel=\"pdfViewer_MouseWheel\"/>"));
                    sw.WriteLine(string.Format("                        </ScrollViewer>"));
                    sw.WriteLine(string.Format("                    </Border>"));
                    sw.WriteLine(string.Format("                    <Border x:Name=\"{0}_SketchMap_Img\"  Grid.Row=\"0\" Grid.Column=\"0\" Grid.RowSpan=\"5\" Grid.ColumnSpan=\"4\" BorderBrush=\"LightGray\" BorderThickness=\"1\">", secTab.Key.Item2));
                    sw.WriteLine(string.Format("                        <ScrollViewer CanContentScroll=\"True\" HorizontalScrollBarVisibility=\"Visible\" VerticalContentAlignment=\"Center\">"));
                    sw.WriteLine(string.Format("                            <mpp:PdfViewer x:Name=\"{0}_SketchMap_PdfViewer\" MouseLeftButtonDown=\"Sketch_MouseLeftButtonDown\" PageBackColor=\"Transparent\" Tag=\"{{Binding ImageFileNames.Item2}}\"", secTab.Key.Item2));
                    sw.WriteLine(string.Format("                                                           Zoom =\" 0.43\" SizeMode=\"Zoom\" MouseWheel=\"pdfViewer_MouseWheel\"/>"));
                    sw.WriteLine(string.Format("                        </ScrollViewer>"));
                    sw.WriteLine(string.Format("                    </Border>"));
                    sw.WriteLine(string.Format("                    <Button x:Name=\"{0}_sketchbtn\"", secTab.Key.Item2));
                    sw.WriteLine(string.Format("                            Grid.Column=\"3\" Grid.Row=\"0\" VerticalContentAlignment=\"Top\" HorizontalAlignment=\"Right\""));
                    sw.WriteLine(string.Format("                                BorderBrush=\"Transparent\" BorderThickness=\"0\""));
                    sw.WriteLine(string.Format("                                Style=\"{{StaticResource Btn_RemoveMouseOverSty}}\""));
                    sw.WriteLine(string.Format("                                Cursor=\"Hand\" Click=\"SketchButton_Click\">"));
                    sw.WriteLine(string.Format("                        <Button.Content>"));
                    sw.WriteLine(string.Format("                            <Grid >"));
                    sw.WriteLine(string.Format("                                <Polygon  Points=\"0,0 30,0, 30,30\" VerticalAlignment=\"Top\" Stroke=\"Black\" Fill=\"LightGray\" StrokeThickness=\"0\">"));
                    sw.WriteLine(string.Format("                                </Polygon>"));
                    sw.WriteLine(string.Format("                                <TextBlock x:Name=\"{0}_SketchTxt_up\" Tag=\"up\" Text=\"&#129149;\"  HorizontalAlignment=\"Right\"></TextBlock> ", secTab.Key.Item2));
                    sw.WriteLine(string.Format("                                <TextBlock x:Name=\"{0}_SketchTxt_down\" Tag=\"down\" Text=\"&#129151;\"  HorizontalAlignment=\"Right\"></TextBlock> ", secTab.Key.Item2));
                    sw.WriteLine(string.Format("                            </Grid>"));
                    sw.WriteLine(string.Format("                        </Button.Content>"));
                    sw.WriteLine(string.Format("                    </Button>"));
                    sw.WriteLine(string.Format("                </Grid>"));
                    sw.WriteLine(string.Format("            </Grid>"));
                    sw.WriteLine(string.Format("        </TabItem>"));
                }
                sw.WriteLine(string.Format("        </TabControl>"));
                sw.WriteLine("</Grid>");
                sw.WriteLine(string.Format("    </TabItem>"));
            }
        }
        private static void WriteXamlSecondHalf(StreamWriter sw)
        {
            sw.WriteLine(string.Format("        <TabItem Header=\"图纸查看\"  >"));
            sw.WriteLine(string.Format("            <Grid>"));
            sw.WriteLine(string.Format("                <TabControl Grid.ColumnSpan=\"3\" x:Name=\"drawingTab\" SelectedIndex=\"{{Binding SecondSelectedTabIndex}}\" SelectionChanged=\"Tag_SelectionChanged\">"));
            sw.WriteLine(string.Format("                    <TabItem Header=\"上层平面图\" Tag=\"{{Binding GraphicDesignImagePath}}\"  >"));
            sw.WriteLine(string.Format("                        <Grid>"));
            sw.WriteLine(string.Format("                            <ScrollViewer CanContentScroll=\"True\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Auto\">"));
            sw.WriteLine(string.Format("                                <mpp:PdfViewer x:Name=\"PdfViewer0\" Zoom=\"0.5\" SizeMode=\"Zoom\" MouseWheel=\"pdfViewer_MouseWheel\"  Tag=\"{{Binding GraphicDesignImagePath}}\" CurrentPageHighlightColor=\"Transparent\" PageBorderColor=\"Transparent\"/>"));
            sw.WriteLine(string.Format("                            </ScrollViewer>"));
            sw.WriteLine(string.Format("                        </Grid>"));
            sw.WriteLine(string.Format("                    </TabItem>"));
            sw.WriteLine(string.Format("                    <TabItem Header=\"下层平面图\" Tag=\"{{Binding GraphicDesignImagePath}}\" >"));
            sw.WriteLine(string.Format("                        <Grid>"));
            sw.WriteLine(string.Format("                            <ScrollViewer CanContentScroll=\"True\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Auto\">"));
            sw.WriteLine(string.Format("                                <mpp:PdfViewer x:Name=\"PdfViewer1\" Zoom=\"0.5\" SizeMode=\"Zoom\" MouseWheel=\"pdfViewer_MouseWheel\"  Tag=\"{{Binding GraphicDesignImagePath}}\" CurrentPageHighlightColor=\"Transparent\" PageBorderColor=\"Transparent\"/>"));
            sw.WriteLine(string.Format("                            </ScrollViewer>"));
            sw.WriteLine(string.Format("                        </Grid>"));
            sw.WriteLine(string.Format("                    </TabItem>"));
            sw.WriteLine(string.Format("                    <TabItem Header=\"1-1剖面图\" Tag=\"{{Binding GraphicDesignImagePath}}\" >"));
            sw.WriteLine(string.Format("                        <Grid>"));
            sw.WriteLine(string.Format("                            <ScrollViewer CanContentScroll=\"True\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Auto\">"));
            sw.WriteLine(string.Format("                                <mpp:PdfViewer x:Name=\"PdfViewer2\" Zoom=\"0.5\" SizeMode=\"Zoom\" MouseWheel=\"pdfViewer_MouseWheel\"  Tag=\"{{Binding GraphicDesignImagePath}}\" CurrentPageHighlightColor=\"Transparent\" PageBorderColor=\"Transparent\"/>"));
            sw.WriteLine(string.Format("                            </ScrollViewer>"));
            sw.WriteLine(string.Format("                        </Grid>"));
            sw.WriteLine(string.Format("                    </TabItem>"));
            sw.WriteLine(string.Format("                    <TabItem Header=\"2-2剖面图\" Tag=\"{{Binding GraphicDesignImagePath}}\" >"));
            sw.WriteLine(string.Format("                        <Grid>"));
            sw.WriteLine(string.Format("                            <ScrollViewer CanContentScroll=\"True\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Auto\">"));
            sw.WriteLine(string.Format("                                <mpp:PdfViewer x:Name=\"PdfViewer3\" Zoom=\"0.5\" SizeMode=\"Zoom\" MouseWheel=\"pdfViewer_MouseWheel\"  Tag=\"{{Binding GraphicDesignImagePath}}\" CurrentPageHighlightColor=\"Transparent\" PageBorderColor=\"Transparent\"/>"));
            sw.WriteLine(string.Format("                            </ScrollViewer>"));
            sw.WriteLine(string.Format("                        </Grid>"));
            sw.WriteLine(string.Format("                    </TabItem>"));
            sw.WriteLine(string.Format("                </TabControl>"));
            sw.WriteLine(string.Format("            </Grid>"));
            sw.WriteLine(string.Format("        </TabItem>"));
            sw.WriteLine(string.Format("    </TabControl>"));
            sw.WriteLine(string.Format("    <StackPanel Margin=\"0,5,0,0\" Grid.Row=\"1\" Orientation=\"Horizontal\" HorizontalAlignment=\"Right\">"));
            sw.WriteLine(string.Format("        <Button Margin=\"5,0\" Width=\"75\" VerticalAlignment=\"Center\" Command=\"{{Binding InitParamCommand}}\" Content=\"重置参数\"/>"));
            sw.WriteLine(string.Format("        <Button Margin=\"5,0\" Width=\"75\" VerticalAlignment=\"Center\" Command=\"{{Binding BuildCommand}}\" Content=\"建模\"/>"));
            sw.WriteLine(string.Format("        <Button Margin=\"5,0\" Width=\"75\" VerticalAlignment=\"Center\" Click=\"CloseButton_Click\" Content=\"取消\"/>"));
            sw.WriteLine(string.Format("    </StackPanel>"));
            sw.WriteLine(string.Format("    </Grid>"));
            sw.WriteLine(string.Format("</Window>"));
        }
        #endregion

        #region cs
        private static void WriteXamlCs(StreamWriter sw, string NameE, string NameC, Dictionary<string, Dictionary<Tuple<string, string>, List<Tuple<string, string, string, string, string, string>>>> parameterDic)
        {
            Console.WriteLine("Deal XamlCs.");
            sw.WriteLine(string.Format("using Patagames.Pdf.Net;                                                                                              "));
            sw.WriteLine(string.Format("using Patagames.Pdf.Net.Controls.Wpf;                                                                                 "));
            sw.WriteLine(string.Format("using Revit.WaterPlant.Builder.Models;                                                                                "));
            sw.WriteLine(string.Format("using System;                                                                                                         "));
            sw.WriteLine(string.Format("using System.Collections.Generic;                                                                                     "));
            sw.WriteLine(string.Format("using System.IO;                                                                                                      "));
            sw.WriteLine(string.Format("using System.Linq;                                                                                                    "));
            sw.WriteLine(string.Format("using System.Net;                                                                                                     "));
            sw.WriteLine(string.Format("using System.Text;                                                                                                    "));
            sw.WriteLine(string.Format("using System.Text.RegularExpressions;                                                                                 "));
            sw.WriteLine(string.Format("using System.Threading.Tasks;                                                                                         "));
            sw.WriteLine(string.Format("using System.Web;                                                                                                     "));
            sw.WriteLine(string.Format("using System.Windows;                                                                                                 "));
            sw.WriteLine(string.Format("using System.Windows.Controls;                                                                                        "));
            sw.WriteLine(string.Format("using System.Windows.Data;                                                                                            "));
            sw.WriteLine(string.Format("using System.Windows.Documents;                                                                                       "));
            sw.WriteLine(string.Format("using System.Windows.Input;                                                                                           "));
            sw.WriteLine(string.Format("using System.Windows.Media;                                                                                           "));
            sw.WriteLine(string.Format("using System.Windows.Media.Imaging;                                                                                   "));
            sw.WriteLine(string.Format("using System.Windows.Navigation;                                                                                      "));
            sw.WriteLine(string.Format("using System.Windows.Shapes;                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("namespace Revit.WaterPlant.Builder                                                                                    "));
            sw.WriteLine(string.Format("{{                                                                                                                     "));
            sw.WriteLine(string.Format("    /// <summary>                                                                                                     "));
            sw.WriteLine(string.Format("    /// {0}Window.xaml 的交互逻辑                                                                          ", NameE));
            sw.WriteLine(string.Format("    /// </summary>                                                                                                    "));
            sw.WriteLine(string.Format("    public partial class {0}Window : Window                                                                ", NameE));
            sw.WriteLine(string.Format("    {{                                                                                                                 "));
            sw.WriteLine(string.Format("        private Dictionary<string, string> imageFileNameDic = new Dictionary<string, string>();                       "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        public {0}Window({0}Model model)                                                        ", NameE));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            //PdfCommon.Initialize();                                                                                 "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("            this.DataContext = model;                                                                                 "));
            sw.WriteLine(string.Format("            InitializeComponent();                                                                                    "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("            //InitSketchMapVisibity();                                                                                "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("        private void GetFocusBtn_OnClick(object sender, RoutedEventArgs e)                                            "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            try                                                                                                       "));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                var tbx = e.Source as TextBox;                                                                        "));
            sw.WriteLine(string.Format("                (DataContext as {0}Model).FocusControlName = tbx.Name;                                     ", NameE));
            sw.WriteLine(string.Format("                LoadPdfImage(tbx.Tag as string);                                                                      "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("            catch (Exception ex)                                                                                      "));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                var a = ex.Message;                                                                                   "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("        private void GetDrawn_OnClick(object sender, RoutedEventArgs e)                                               "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            //LoadDesignPath();                                                                                       "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        private void Tbx_GotFocus(object sender, RoutedEventArgs e)                                                   "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        private void Tbx_LostFocus(object sender, RoutedEventArgs e)                                                  "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        private void CloseButton_Click(object sender, RoutedEventArgs e)                                              "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            this.Close();                                                                                             "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        private void SketchButton_Click(object sender, RoutedEventArgs e)                                             "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            var btn = sender as Button;                                                                               "));
            sw.WriteLine(string.Format("            if (btn == null) return;                                                                                  "));
            sw.WriteLine(string.Format("            var tab = GetParentObject<TabControl>(btn, \"\");                                                           "));
            sw.WriteLine(string.Format("            if (tab == null) return;                                                                                  "));
            sw.WriteLine(string.Format("            var tag = (tab.SelectedItem as TabItem)?.Tag as string;                                                   "));
            sw.WriteLine(string.Format("            if (string.IsNullOrEmpty(tag)) return;                                                                    "));
            sw.WriteLine(string.Format("            UpdateSketchMapStatus(tag);                                                                               "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("        private void InitSketchMapVisibity()                                                                          "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            var SecTabNameEList = new List<string>();
            //Tab-(次Tab-次Tab简称)-(名称-简称-默认值-单位-绑定值-备注)
            foreach (var tab in parameterDic)
                foreach (var secTab in tab.Value)
                {
                    if (SecTabNameEList.Contains(secTab.Key.Item2)) continue;
                    SecTabNameEList.Add(secTab.Key.Item2);
                }
            foreach (var name in SecTabNameEList) sw.WriteLine(string.Format("UpdateSketchMapStatus(\"{0}\", false);", name));
            sw.WriteLine(string.Format("   "));
            foreach (var name in SecTabNameEList) sw.WriteLine(string.Format("LoadPdfImage(\"{0}\");", name));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("        private void LoadPdfImage(string tag)                                                                         "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            try                                                                                                       "));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                PdfViewer pdfViewer = FindName(tag + \"_PdfViewer\") as PdfViewer;                                      "));
            sw.WriteLine(string.Format("                PdfViewer SketchMapPdfViewer = FindName(tag + \"_SketchMap_PdfViewer\") as PdfViewer;                   "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("                var path = pdfViewer?.Tag as string;                                                                  "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("                if (!string.IsNullOrEmpty(path) && File.Exists(path))                                                 "));
            sw.WriteLine(string.Format("                {{                                                                                                     "));
            sw.WriteLine(string.Format("                    pdfViewer.LoadDocument(path);                                                                     "));
            sw.WriteLine(string.Format("                }}                                                                                                     "));
            sw.WriteLine(string.Format("                var sketchmap_path = SketchMapPdfViewer?.Tag as string;                                               "));
            sw.WriteLine(string.Format("                if (!string.IsNullOrEmpty(sketchmap_path) && File.Exists(path))                                       "));
            sw.WriteLine(string.Format("                {{                                                                                                     "));
            sw.WriteLine(string.Format("                    SketchMapPdfViewer.LoadDocument(sketchmap_path);                                                  "));
            sw.WriteLine(string.Format("                }}                                                                                                     "));
            sw.WriteLine(string.Format("                //if (!string.IsNullOrEmpty((DataContext as FilterModel).FocusControlName)) {{                         "));
            sw.WriteLine(string.Format("                //    MainSize_sketchbtn.Visibility = Visibility.Visible;                                             "));
            sw.WriteLine(string.Format("                //}}                                                                                                   "));
            sw.WriteLine(string.Format("                var Sketchbtn = FindName(tag + \"_sketchbtn\") as Button;                                               "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("                Sketchbtn.Visibility = Visibility.Visible;                                                            "));
            sw.WriteLine(string.Format("                UpdateSketchMapStatus(tag, false);                                                                    "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("            catch (Exception ex)                                                                                      "));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                var a = 0;                                                                                            "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("        private void LoadSketchPdfInMainPdf(string tag)                                                               "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            try                                                                                                       "));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                PdfViewer SketchMapPdfViewer = FindName(tag + \"_SketchMap_PdfViewer\") as PdfViewer;                   "));
            sw.WriteLine(string.Format("                PdfViewer pdfViewer = FindName(tag + \"_PdfViewer\") as PdfViewer;                                      "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("                var sketchmap_path = SketchMapPdfViewer?.Tag as string;                                               "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("                if (!string.IsNullOrEmpty(sketchmap_path))                                                            "));
            sw.WriteLine(string.Format("                {{                                                                                                     "));
            sw.WriteLine(string.Format("                    pdfViewer.LoadDocument(sketchmap_path);                                                           "));
            sw.WriteLine(string.Format("                }}                                                                                                     "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("            catch (Exception ex)                                                                                      "));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                var a = 0;                                                                                            "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("        private void Sketch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)                                "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            var pdfviewer = sender as PdfViewer;                                                                      "));
            sw.WriteLine(string.Format("            if (pdfviewer == null) return;                                                                            "));
            sw.WriteLine(string.Format("            var tab = GetParentObject<TabControl>(pdfviewer, \"\");                                                     "));
            sw.WriteLine(string.Format("            if (tab == null) return;                                                                                  "));
            sw.WriteLine(string.Format("            var tag = (tab.SelectedItem as TabItem)?.Tag as string;                                                   "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("            //LoadSketchPdfInMainPdf(tag);                                                                            "));
            sw.WriteLine(string.Format("            //BackDefaultView(tag);                                                                                   "));
            sw.WriteLine(string.Format("            var txtUp = FindName(tag + \"_SketchTxt_up\") as TextBlock;                                                 "));
            sw.WriteLine(string.Format("            var txtDown = FindName(tag + \"_SketchTxt_down\") as TextBlock;                                             "));
            sw.WriteLine(string.Format("            var img = FindName(tag + \"_SketchMap_Img\") as Border;                                                     "));
            sw.WriteLine(string.Format("            txtUp.Visibility = Visibility.Collapsed;                                                                  "));
            sw.WriteLine(string.Format("            txtDown.Visibility = Visibility.Visible;                                                                  "));
            sw.WriteLine(string.Format("            img.Visibility = Visibility.Collapsed;                                                                    "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("        private void BackDefaultView(string tag)                                                                      "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            var SketchTxt_up = FindName(tag + \"_SketchTxt_up\") as TextBlock;                                          "));
            sw.WriteLine(string.Format("            var SketchTxt_down = FindName(tag + \"_SketchTxt_down\") as TextBlock;                                      "));
            sw.WriteLine(string.Format("            var Sketchbtn = FindName(tag + \"_sketchbtn\") as Button;                                                   "));
            sw.WriteLine(string.Format("            var SketchMap_Img = FindName(tag + \"_SketchMap_Img\") as Border;                                           "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("            SketchTxt_up.Visibility = Visibility.Collapsed;                                                           "));
            sw.WriteLine(string.Format("            SketchTxt_down.Visibility = Visibility.Collapsed;                                                         "));
            sw.WriteLine(string.Format("            Sketchbtn.Visibility = Visibility.Collapsed;                                                              "));
            sw.WriteLine(string.Format("            SketchMap_Img.Visibility = Visibility.Collapsed;                                                          "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("        private void UpdateSketchMapStatus(string tag, bool? forceVisible = null)                                     "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            var txtUp = FindName(tag + \"_SketchTxt_up\") as TextBlock;                                                 "));
            sw.WriteLine(string.Format("            var txtDown = FindName(tag + \"_SketchTxt_down\") as TextBlock;                                             "));
            sw.WriteLine(string.Format("            var img = FindName(tag + \"_SketchMap_Img\") as Border;                                                     "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("            if (!forceVisible.HasValue)                                                                               "));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                if (txtUp.Visibility == Visibility.Visible)                                                           "));
            sw.WriteLine(string.Format("                {{                                                                                                     "));
            sw.WriteLine(string.Format("                    txtUp.Visibility = Visibility.Collapsed;                                                          "));
            sw.WriteLine(string.Format("                    txtDown.Visibility = Visibility.Visible;                                                          "));
            sw.WriteLine(string.Format("                    img.Visibility = Visibility.Collapsed;                                                            "));
            sw.WriteLine(string.Format("                }}                                                                                                     "));
            sw.WriteLine(string.Format("                else                                                                                                  "));
            sw.WriteLine(string.Format("                {{                                                                                                     "));
            sw.WriteLine(string.Format("                    txtUp.Visibility = Visibility.Visible;                                                            "));
            sw.WriteLine(string.Format("                    txtDown.Visibility = Visibility.Collapsed;                                                        "));
            sw.WriteLine(string.Format("                    img.Visibility = Visibility.Visible;                                                              "));
            sw.WriteLine(string.Format("                }}                                                                                                     "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("            else                                                                                                      "));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                if (forceVisible.Value)                                                                               "));
            sw.WriteLine(string.Format("                {{                                                                                                     "));
            sw.WriteLine(string.Format("                    txtUp.Visibility = Visibility.Visible;                                                            "));
            sw.WriteLine(string.Format("                    txtDown.Visibility = Visibility.Collapsed;                                                        "));
            sw.WriteLine(string.Format("                    img.Visibility = Visibility.Visible;                                                              "));
            sw.WriteLine(string.Format("                }}                                                                                                     "));
            sw.WriteLine(string.Format("                else                                                                                                  "));
            sw.WriteLine(string.Format("                {{                                                                                                     "));
            sw.WriteLine(string.Format("                    txtUp.Visibility = Visibility.Collapsed;                                                          "));
            sw.WriteLine(string.Format("                    txtDown.Visibility = Visibility.Visible;                                                          "));
            sw.WriteLine(string.Format("                    img.Visibility = Visibility.Collapsed;                                                            "));
            sw.WriteLine(string.Format("                }}                                                                                                     "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        public void Tag_SelectionChanged(object sender, SelectionChangedEventArgs e)                                  "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            var tag = (sender as TabControl);                                                                         "));
            sw.WriteLine(string.Format("            if (tag == null) return;                                                                                  "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("            var selTabItem = tag.SelectedItem as TabItem;                                                             "));
            sw.WriteLine(string.Format("            if (selTabItem == null) return;                                                                           "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("            if (tag.SelectedIndex == (DataContext as {0}Model).DrawingTabIndex)                            ", NameE));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                LoadDesignPath(this.drawingTab.SelectedIndex);                                                        "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("            else                                                                                                      "));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                var tbc = GetControls(((Grid)(selTabItem.Content)).Children);                                         "));
            sw.WriteLine(string.Format("                TabChanged(tbc?.Tag as string);                                                                       "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("                                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("        public void SecondTag_SelectionChanged(object sender, SelectionChangedEventArgs e)                            "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            var selTab = (sender as TabControl);                                                                      "));
            sw.WriteLine(string.Format("            if (selTab == null) return;                                                                               "));
            sw.WriteLine(string.Format("            TagChanged(selTab);                                                                                       "));
            sw.WriteLine(string.Format("            var firstTab = GetParentObject<TabControl>(selTab, \"\");                                                   "));
            sw.WriteLine(string.Format("            if (firstTab == null) return;                                                                             "));
            sw.WriteLine(string.Format("            if (firstTab.SelectedIndex == (DataContext as {0}Model).DrawingTabIndex)                       ", NameE));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                LoadDesignPath(selTab.SelectedIndex);                                                                 "));
            sw.WriteLine(string.Format("            }}                                                                                                         "));
            sw.WriteLine(string.Format("        }}                                                                                                             "));
            sw.WriteLine(string.Format("        private TabItem GetControls(UIElementCollection uiControls)                                                   "));
            sw.WriteLine(string.Format("        {{                                                                                                             "));
            sw.WriteLine(string.Format("            TabItem tabCon = null;                                                                                    "));
            sw.WriteLine(string.Format("            foreach (var element in uiControls)                                                                       "));
            sw.WriteLine(string.Format("            {{                                                                                                         "));
            sw.WriteLine(string.Format("                if (element is TabControl)                                                                            "));
            sw.WriteLine(string.Format("                {{                                                                                                     "));
            sw.WriteLine(string.Format("                    var run = (TabItem)((TabControl)element).SelectedItem;                                            "));
            sw.WriteLine(string.Format("                    (DataContext as {0}Model).SecondSelectedTabIndex = ((TabControl)element).SelectedIndex;", NameE));
            sw.WriteLine(string.Format("                    return run;                                                                                    "));
            sw.WriteLine(string.Format("                }}                                                                                                  "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            return tabCon;                                                                                         "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("        private void TabChanged(string tab)                                                                        "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            if (string.IsNullOrEmpty(tab)) return;                                                                 "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("            BackDefaultView(tab);                                                                                  "));
            sw.WriteLine(string.Format("            LoadPdfImage(tab);                                                                                     "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("        private void LoadDesignPath(int index)                                                                     "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            PdfViewer pdfViewer = FindName(\"PdfViewer\" + index.ToString()) as PdfViewer;                           "));
            sw.WriteLine(string.Format("            var path = pdfViewer.Tag as string;                                                                    "));
            sw.WriteLine(string.Format("            if (string.IsNullOrEmpty(path)) return;                                                                "));
            sw.WriteLine(string.Format("            if (pdfViewer == null) return;                                                                         "));
            sw.WriteLine(string.Format("            pdfViewer.LoadDocument(path);                                                                          "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("        private void TagChanged(TabControl tab)                                                                    "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            var selTabItem = tab?.SelectedItem as TabItem;                                                         "));
            sw.WriteLine(string.Format("            var tag = selTabItem?.Tag as string;                                                                   "));
            sw.WriteLine(string.Format("            if (string.IsNullOrEmpty(tag)) return;                                                                 "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("            BackDefaultView(tag);                                                                                  "));
            sw.WriteLine(string.Format("            LoadPdfImage(tag);                                                                                     "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        #region PdfViewerZoom                                                                                      "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        private void pdfViewer_MouseWheel(object sender, MouseWheelEventArgs e)                                    "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            System.Windows.Point p = e.GetPosition(null);                                                          "));
            sw.WriteLine(string.Format("            var pdfviewer = sender as PdfViewer;                                                                   "));
            sw.WriteLine(string.Format("            if (e.Delta > 0)                                                                                       "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                calculZoomPlus(pdfviewer);                                                                         "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            else if (e.Delta < 0)                                                                                  "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                calculZoomMoins(pdfviewer);                                                                        "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        private void calculZoomPlus(PdfViewer pdfViewer)                                                           "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            if (pdfViewer.Zoom < 0.125f)                                                                           "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                pdfViewer.Zoom = 0.125f;                                                                           "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            else if (pdfViewer.Zoom >= 0.125f && pdfViewer.Zoom < 4.00f)                                           "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                pdfViewer.Zoom += 0.125f;                                                                          "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            else if (pdfViewer.Zoom == 4.00f)                                                                      "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                pdfViewer.Zoom = 4.00f;                                                                            "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        private void calculZoomMoins(PdfViewer pdfViewer)                                                          "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            if (pdfViewer.Zoom == 0.25f)                                                                           "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                pdfViewer.Zoom = 0.125f;                                                                           "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            else if (pdfViewer.Zoom <= 0.125f)                                                                     "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            else if (pdfViewer.Zoom > 0.125f && pdfViewer.Zoom <= 4.00f)                                           "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                pdfViewer.Zoom -= 0.125f;                                                                          "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        #endregion                                                                                                 "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        public List<T> GetChildObjects<T>(DependencyObject obj, string name) where T : FrameworkElement            "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            try                                                                                                    "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                DependencyObject child = null;                                                                     "));
            sw.WriteLine(string.Format("                List<T> childList = new List<T>();                                                                 "));
            sw.WriteLine(string.Format("                for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)                              "));
            sw.WriteLine(string.Format("                {{                                                                                                  "));
            sw.WriteLine(string.Format("                    child = VisualTreeHelper.GetChild(obj, i);                                                     "));
            sw.WriteLine(string.Format("                    if (child is T && (((T)child).Name == name || string.IsNullOrEmpty(name)))                     "));
            sw.WriteLine(string.Format("                    {{                                                                                              "));
            sw.WriteLine(string.Format("                        childList.Add((T)child);                                                                   "));
            sw.WriteLine(string.Format("                    }}                                                                                              "));
            sw.WriteLine(string.Format("                    childList.AddRange(GetChildObjects<T>(child, \"\"));                                             "));
            sw.WriteLine(string.Format("                }}                                                                                                  "));
            sw.WriteLine(string.Format("                return childList;                                                                                  "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            catch (Exception ex)                                                                                   "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                var a = ex.Message;                                                                                "));
            sw.WriteLine(string.Format("                return null;                                                                                       "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("        public T GetParentObject<T>(DependencyObject obj, string name) where T : FrameworkElement                  "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            DependencyObject parent = VisualTreeHelper.GetParent(obj);                                             "));
            sw.WriteLine(string.Format("            while (parent != null)                                                                                 "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                if (parent is T && (((T)parent).Name == name || string.IsNullOrEmpty(name)))                       "));
            sw.WriteLine(string.Format("                {{                                                                                                  "));
            sw.WriteLine(string.Format("                    return (T)parent;                                                                              "));
            sw.WriteLine(string.Format("                }}                                                                                                  "));
            sw.WriteLine(string.Format("                parent = VisualTreeHelper.GetParent(parent);                                                       "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            return null;                                                                                           "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)                                       "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            var dataGrid = sender as DataGrid;                                                                     "));
            sw.WriteLine(string.Format("            var model = DataContext as {0}Model;                                                        ", NameE));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("            if (dataGrid == null || model == null) return;                                                         "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("            var tag = dataGrid.Tag as string;                                                                      "));
            sw.WriteLine(string.Format("            var focusControlName = (DataContext as {0}Model).FocusControlName;                          ", NameE));
            sw.WriteLine(string.Format("            SetDataCellFocusName(dataGrid);                                                                        "));
            sw.WriteLine(string.Format("            LoadPdfImage(tag);                                                                                     "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        private void SetDataCellFocusName(DataGrid dataGrid)                                                       "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            var tag = dataGrid.Tag as string;                                                                      "));
            sw.WriteLine(string.Format("            var header = string.Empty;                                                                             "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("            var cell = dataGrid.CurrentCell;                                                                       "));
            sw.WriteLine(string.Format("            if (cell != null && cell.Column != null)                                                               "));
            sw.WriteLine(string.Format("                header = (string)cell.Column?.Header;                                                              "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("            var focusControlName = $\"{{tag}}_{{header}}\";                                                              "));
            sw.WriteLine(string.Format("            if (string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(header))                                         "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("					//为了判断cell离开事件，是点击其他textbox还是点击非textbox控件                                    "));
            sw.WriteLine(string.Format("                if ((DataContext as {0}Model).FocusControlName != \"TbxMaintenancePlatform_A1\")          ", NameE));
            sw.WriteLine(string.Format("                {{                                                                                                  "));
            sw.WriteLine(string.Format("                    (DataContext as {0}Model).FocusControlName = string.Empty;                          ", NameE));
            sw.WriteLine(string.Format("                }}                                                                                                  "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            else                                                                                                   "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                (DataContext as {0}Model).FocusControlName = focusControlName;                          ", NameE));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        /// <summary>                                                                                              "));
            sw.WriteLine(string.Format("        ///  文本输入判断（只能输入数字不带负号）                                                                  "));
            sw.WriteLine(string.Format("        /// </summary>                                                                                             "));
            sw.WriteLine(string.Format("        /// <param name=\"sender\"></param>                                                                          "));
            sw.WriteLine(string.Format("        /// <param name=\"e\"></param>                                                                               "));
            sw.WriteLine(string.Format("        private void OnSubPreviewKeyDown(object sender, KeyEventArgs e)                                            "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            var tbx = sender as TextBox;                                                                           "));
            sw.WriteLine(string.Format("            var str = tbx.Text;                                                                                    "));
            sw.WriteLine(string.Format("            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||                                                  "));
            sw.WriteLine(string.Format("               (e.Key >= Key.D0 && e.Key <= Key.D9) ||                                                             "));
            sw.WriteLine(string.Format("               e.Key == Key.Back ||                                                                                "));
            sw.WriteLine(string.Format("               e.Key == Key.Left || e.Key == Key.Right)                                                            "));
            sw.WriteLine(string.Format("            //|| ((e.Key == Key.OemPeriod || e.Key == Key.Decimal) && tbx.SelectionStart > 0 && !str.Contains(\".\"))"));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                if (e.KeyboardDevice.Modifiers != ModifierKeys.None)                                               "));
            sw.WriteLine(string.Format("                {{                                                                                                  "));
            sw.WriteLine(string.Format("                    e.Handled = true;                                                                              "));
            sw.WriteLine(string.Format("                }}                                                                                                  "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            else                                                                                                   "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                e.Handled = true;                                                                                  "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        private void OnLvPreviewKeyDown(object sender, KeyEventArgs e)                                               "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            var tbx = sender as TextBox;                                                                           "));
            sw.WriteLine(string.Format("            var str = tbx.Text;                                                                                    "));
            sw.WriteLine(string.Format("            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||                                                  "));
            sw.WriteLine(string.Format("                (e.Key >= Key.D0 && e.Key <= Key.D9) ||                                                            "));
            sw.WriteLine(string.Format("                e.Key == Key.Back || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Subtract ||           "));
            sw.WriteLine(string.Format("                (e.Key == Key.OemMinus && tbx.SelectionStart == 0 && !str.Contains(\"-\"))                          "));
            sw.WriteLine(string.Format("            || ((e.Key == Key.OemPeriod || e.Key == Key.Decimal) && tbx.SelectionStart > 0 && !str.Contains(\".\")))"));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                if (e.KeyboardDevice.Modifiers != ModifierKeys.None)                                               "));
            sw.WriteLine(string.Format("                {{                                                                                                  "));
            sw.WriteLine(string.Format("                    e.Handled = true;                                                                              "));
            sw.WriteLine(string.Format("                }}                                                                                                  "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("            else                                                                                                   "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                e.Handled = true;                                                                                  "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        private void Tbx_PreviewTextInput(object sender, TextCompositionEventArgs e)                               "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            if (!Regex.IsMatch(e.Text, @\"^\\d+$\"))                                                                  "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                e.Handled = true;                                                                                  "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                   "));
            sw.WriteLine(string.Format("        private void Tbx_PreviewKeyDown(object sender, KeyEventArgs e)                                             "));
            sw.WriteLine(string.Format("        {{                                                                                                          "));
            sw.WriteLine(string.Format("            if (e.Key == Key.Space)                                                                                "));
            sw.WriteLine(string.Format("            {{                                                                                                      "));
            sw.WriteLine(string.Format("                e.Handled = true;                                                                                  "));
            sw.WriteLine(string.Format("            }}                                                                                                      "));
            sw.WriteLine(string.Format("        }}                                                                                                          "));
            sw.WriteLine(string.Format("    }}                                                                                                              "));
            sw.WriteLine(string.Format("}}                                                                                                                  "));
            Console.WriteLine("XamlCs Done");
        }
        #endregion

        #region Model
        private static void WriteModel(StreamWriter sw, string NameE, string NameC, Dictionary<string, Dictionary<Tuple<string, string>, List<Tuple<string, string, string, string, string, string>>>> parameterDic, Dictionary<string, List<string>> bindDic)
        {
            Console.WriteLine("Deal Model.");
            WriteModelFirstHalf(sw, NameE, NameC, parameterDic);
            WriteModelParameter(sw, parameterDic, bindDic);
            WriteModelPdf(sw, NameE, parameterDic);
            WriteModelMethod(sw, NameE, NameC, parameterDic);
            sw.WriteLine("}\n}");
            Console.WriteLine("Model Done.");
        }
        /// <summary>
        /// Properties + Command + Constructor + Implements
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="NameE"></param>
        /// <param name="NameC"></param>
        /// <param name="parameterDic"></param>
        private static void WriteModelFirstHalf(StreamWriter sw, string NameE, string NameC, Dictionary<string, Dictionary<Tuple<string, string>, List<Tuple<string, string, string, string, string, string>>>> parameterDic)
        {
            sw.WriteLine(string.Format("using Autodesk.Revit.DB;                                                                      "));
            sw.WriteLine(string.Format("using Autodesk.Revit.UI;                                                                      "));
            sw.WriteLine(string.Format("using Autodesk.Revit.UI.Selection;                                                            "));
            sw.WriteLine(string.Format("using Encrypt;                                                                                "));
            sw.WriteLine(string.Format("using GalaSoft.MvvmLight;                                                                     "));
            sw.WriteLine(string.Format("using GalaSoft.MvvmLight.CommandWpf;                                                          "));
            sw.WriteLine(string.Format("using Revit.WaterPlant.Builder.Helpers;                                                       "));
            sw.WriteLine(string.Format("using System;                                                                                 "));
            sw.WriteLine(string.Format("using System.Collections.Generic;                                                             "));
            sw.WriteLine(string.Format("using System.Collections.ObjectModel;                                                         "));
            sw.WriteLine(string.Format("using System.IO;                                                                              "));
            sw.WriteLine(string.Format("using System.Linq;                                                                            "));
            sw.WriteLine(string.Format("using System.Runtime.CompilerServices;                                                        "));
            sw.WriteLine(string.Format("using System.Runtime.Serialization;                                                           "));
            sw.WriteLine(string.Format("using System.Text;                                                                            "));
            sw.WriteLine(string.Format("using System.Threading.Tasks;                                                                 "));
            sw.WriteLine(string.Format("using System.Windows;                                                                         "));
            sw.WriteLine(string.Format("using System.Windows.Input;                                                                   "));
            sw.WriteLine(string.Format("using System.Xml.Serialization;                                                               "));
            sw.WriteLine(string.Format("                                                                                              "));
            sw.WriteLine(string.Format("namespace Revit.WaterPlant.Builder.Models                                                     "));
            sw.WriteLine(string.Format("{{           "));
            sw.WriteLine(string.Format("    /// <summary> "));
            sw.WriteLine(string.Format("    /// {0} ", NameC));
            sw.WriteLine(string.Format("    /// </summary>"));
            sw.WriteLine(string.Format("    [Serializable]"));
            sw.WriteLine(string.Format("    public class {0}Model : ViewModelBase, IExternalEventHandler, IXmlSerialization", NameE));
            sw.WriteLine(string.Format("    {{       "));
            sw.WriteLine(string.Format("        #region Properties"));
            sw.WriteLine(string.Format("        private bool isSerialising; "));
            sw.WriteLine(string.Format("        ExternalEvent eventHandler = null;"));
            sw.WriteLine(string.Format("        [XmlIgnore] "));
            sw.WriteLine(string.Format("        public EventHandler CloseWindowEventHandler = null; "));
            sw.WriteLine(string.Format("        public UIApplication app; "));
            sw.WriteLine(string.Format("        public int DrawingTabIndex = {0}; ", parameterDic.Keys.Count));
            sw.WriteLine(string.Format("        #endregion"));
            sw.WriteLine(string.Format("		     "));
            sw.WriteLine(string.Format("        #region Command "));
            sw.WriteLine(string.Format("            "));
            sw.WriteLine(string.Format("        public ICommand BuildCommand"));
            sw.WriteLine(string.Format("        {{   "));
            sw.WriteLine(string.Format("            get => new RelayCommand(DoBuild); "));
            sw.WriteLine(string.Format("        }}   "));
            sw.WriteLine(string.Format("        private void DoBuild()"));
            sw.WriteLine(string.Format("        {{   "));
            sw.WriteLine(string.Format("            eventHandler.Raise(); "));
            sw.WriteLine(string.Format("        }}   "));
            sw.WriteLine(string.Format("        public ICommand InitParamCommand"));
            sw.WriteLine(string.Format("        {{   "));
            sw.WriteLine(string.Format("            get => new RelayCommand(DoInitParam); "));
            sw.WriteLine(string.Format("        }}   "));
            sw.WriteLine(string.Format("        private void DoInitParam()"));
            sw.WriteLine(string.Format("        {{   "));
            sw.WriteLine(string.Format("            var r = MessageBox.Show(\"确认重置参数\", \"提示\", MessageBoxButton.OKCancel); "));
            sw.WriteLine(string.Format("            if (r == MessageBoxResult.Cancel) return; "));
            sw.WriteLine(string.Format("            InitParam();"));
            sw.WriteLine(string.Format("        }}   "));
            sw.WriteLine(string.Format("        #endregion"));
            sw.WriteLine(string.Format("            "));
            sw.WriteLine(string.Format("        #region Constructor "));
            sw.WriteLine(string.Format("        public {0}Model()", NameE));
            sw.WriteLine(string.Format("        {{   "));
            sw.WriteLine(string.Format("            isSerialising = false;"));
            sw.WriteLine(string.Format("            eventHandler = ExternalEvent.Create(this);"));
            sw.WriteLine(string.Format("            "));
            sw.WriteLine(string.Format("            InitParam();"));
            sw.WriteLine(string.Format("        }}   "));
            sw.WriteLine(string.Format("        public {0}Model(bool serialising) : this() ", NameE));
            sw.WriteLine(string.Format("        {{   "));
            sw.WriteLine(string.Format("            isSerialising = serialising;"));
            sw.WriteLine(string.Format("            InitParam();"));
            sw.WriteLine(string.Format("        }}   "));
            sw.WriteLine(string.Format("        #endregion"));
            sw.WriteLine(string.Format("            "));
            sw.WriteLine(string.Format("        #region Implements"));
            sw.WriteLine(string.Format("            "));
            sw.WriteLine(string.Format("        public void Execute(UIApplication app)"));
            sw.WriteLine(string.Format("        {{   "));
            sw.WriteLine(string.Format("            this.app = app; "));
            sw.WriteLine(string.Format("            Build();"));
            sw.WriteLine(string.Format("        }}   "));
            sw.WriteLine(string.Format("            "));
            sw.WriteLine(string.Format("        public string GetName() "));
            sw.WriteLine(string.Format("        {{   "));
            sw.WriteLine(string.Format("            return \"Build{0}\"; ", NameE));
            sw.WriteLine(string.Format("        }}   "));
            sw.WriteLine(string.Format("            "));
            sw.WriteLine(string.Format("            "));
            sw.WriteLine(string.Format("        public bool IsSerializaing()"));
            sw.WriteLine(string.Format("        {{   "));
            sw.WriteLine(string.Format("            return isSerialising; "));
            sw.WriteLine(string.Format("        }}   "));
            sw.WriteLine(string.Format("            "));
            sw.WriteLine(string.Format("        public void FinishSerialization() "));
            sw.WriteLine(string.Format("        {{   "));
            sw.WriteLine(string.Format("            isSerialising = false;"));
            sw.WriteLine(string.Format("        }}   "));
            sw.WriteLine(string.Format("            "));
            sw.WriteLine(string.Format("        #endregion"));
        }
        private static void WriteModelParameter(StreamWriter sw, Dictionary<string, Dictionary<Tuple<string, string>, List<Tuple<string, string, string, string, string, string>>>> parameterDic, Dictionary<string, List<string>> bindDic)
        {
            sw.WriteLine();
            sw.WriteLine("#region Parameter");
            //Tab-(次Tab-次Tab简称)-(名称-简称-默认值-单位-绑定值-备注)
            foreach (var tab in parameterDic)
            {
                sw.WriteLine("#region {0}", tab.Key);
                sw.WriteLine();
                foreach (var secTab in tab.Value)
                {
                    sw.WriteLine("#region {0}", secTab.Key.Item1);
                    string upperName = secTab.Key.Item2;
                    string lowerName = secTab.Key.Item2[0].ToString().ToLower() + secTab.Key.Item2.Substring(1);
                    Console.WriteLine("Dealing With " + upperName);
                    foreach (var item in secTab.Value)
                    {
                        string UpperHead = string.Format("{0}_{1}", upperName, item.Item2);
                        string LowerHead = string.Format("{0}_{1}", lowerName, item.Item2);
                        Console.WriteLine("Dealing With " + upperName + "/" + UpperHead);
                        sw.WriteLine(string.Format("private double _{0}=> G.ConvertToDecimalFeet({1});", LowerHead, UpperHead));
                        //sw.WriteLine(string.Format("private {0} {1};", item.Item2.ToLower() == "lv" && item.Item4.ToLower() == "m" ? "double" : "int", LowerHead));
                        sw.WriteLine(string.Format("private {0} {1};", item.Item4.ToLower() == "m" ? "double" : "int", LowerHead));
                        sw.WriteLine(string.Format("///<summary>"));
                        sw.WriteLine(string.Format("///{0} {1} {2} {3}", item.Item1, item.Item2, string.IsNullOrEmpty(item.Item6) ? null : ": " + item.Item6, string.IsNullOrEmpty(item.Item5) ? null : "(与" + item.Item5 + "绑定)"));
                        sw.WriteLine(string.Format("///</summary>"));
                        //sw.WriteLine(string.Format("public {0} {1}{{", item.Item2.ToLower() == "lv" && item.Item4.ToLower() == "m" ? "double" : "int", UpperHead));
                        sw.WriteLine(string.Format("public {0} {1}{{", item.Item4.ToLower() == "m" ? "double" : "int", UpperHead));
                        try
                        {
                            if (!string.IsNullOrEmpty(item.Item5))
                            {
                                string[] bindItem = item.Item5.Split('/');
                                if (bindItem.Length != 3) throw new Exception();
                                var a = parameterDic[bindItem[0]].FirstOrDefault(t => t.Key.Item1 == bindItem[1]);
                                var b = a.Value.FirstOrDefault(t => t.Item1 == bindItem[2]);
                                sw.WriteLine(string.Format("get => {0}_{1};", a.Key.Item2, b.Item2));
                            }
                            else
                            {
                                sw.WriteLine(string.Format("get => {0};", LowerHead));
                            }
                            sw.WriteLine(string.Format("set{{"));
                            sw.WriteLine(string.Format("{0} = value;", LowerHead));
                            sw.WriteLine(string.Format("RaisePropertyChanged(\"{0}\");", UpperHead));
                            if (bindDic.ContainsKey(UpperHead))
                                foreach (var itm in bindDic[UpperHead])
                                    sw.WriteLine(string.Format("RaisePropertyChanged(\"{0}\");", itm));

                            sw.WriteLine(string.Format("}}"));
                            sw.WriteLine(string.Format("}}"));

                        }
                        catch (Exception)
                        {
                            MessageBox.Show(String.Format("{0}/{1}/{2}的绑定值有问题。", tab.Key, secTab.Key.Item1, item.Item1));
                            sw.WriteLine(string.Format("get => {0};", LowerHead));
                            sw.WriteLine(string.Format("set{{"));
                            sw.WriteLine(string.Format("{0} = value;", LowerHead));
                            sw.WriteLine(string.Format("RaisePropertyChanged(\"{0}\");}}", UpperHead));
                            sw.WriteLine(string.Format("}}"));
                            sw.WriteLine(string.Format("//这个值有问题↑"));
                        }
                        finally
                        {
                            sw.WriteLine();
                        }
                    }
                    sw.WriteLine("#endregion");
                }
                sw.WriteLine();
                sw.WriteLine("#endregion");
            }
            sw.WriteLine("#endregion");
        }
        private static void WriteModelPdf(StreamWriter sw, string nameE, Dictionary<string, Dictionary<Tuple<string, string>, List<Tuple<string, string, string, string, string, string>>>> parameterDic)
        {
            sw.WriteLine();
            sw.WriteLine(string.Format("		#region Pdf                                                                                                             "));
            sw.WriteLine(string.Format("        [XmlIgnore]                                                                                                          "));
            sw.WriteLine(string.Format("        public Dictionary<string, string> imageNameDic = new Dictionary<string, string> {{                                    "));
            foreach (var tab in parameterDic)
            {
                foreach (var secTab in tab.Value)
                {
                    int i = 1;
                    foreach (var item in secTab.Value)
                    {
                        string UpperHead = string.Format("{0}_{1}", secTab.Key.Item2, item.Item2);
                        sw.WriteLine(string.Format("                {{ \"Tbx{0}\", \"{1}\"}},                                                                                 ", UpperHead, i++));
                        //todo
                    }
                }
            }
            sw.WriteLine(string.Format("		}};                                                                                                                       "));
            sw.WriteLine(string.Format("		private void UpdateImageFileName()                                                                                      "));
            sw.WriteLine(string.Format("        {{                                                                                                                    "));
            sw.WriteLine(string.Format("            var filename = (string.IsNullOrEmpty(FocusControlName) || !imageNameDic.ContainsKey(FocusControlName)) ?         "));
            sw.WriteLine(string.Format("                SelectedTabIndex + \"_\" + SecondSelectedTabIndex :                                                            "));
            sw.WriteLine(string.Format("                SelectedTabIndex + \"_\" + SecondSelectedTabIndex + \"_\" + imageNameDic[FocusControlName];                      "));
            sw.WriteLine(string.Format("            //局部图                                                                                                         "));
            sw.WriteLine(string.Format("            string partialfilename = filename + \".pdf\";                                                                      "));
            sw.WriteLine(string.Format("            //缩略图                                                                                                         "));
            sw.WriteLine(string.Format("            string sketchfilename = filename + \"_s.pdf\";                                                                     "));
            sw.WriteLine(string.Format("            if (string.IsNullOrEmpty(FocusControlName) || !imageNameDic.ContainsKey(FocusControlName))                       "));
            sw.WriteLine(string.Format("            {{                                                                                                                "));
            sw.WriteLine(string.Format("                sketchfilename = partialfilename;                                                                            "));
            sw.WriteLine(string.Format("            }}                                                                                                                "));
            sw.WriteLine(string.Format("            var partialPath = Path.Combine(ImageFolder, partialfilename);                                                    "));
            sw.WriteLine(string.Format("            var sketchPath = Path.Combine(ImageFolder, sketchfilename);                                                      "));
            sw.WriteLine(string.Format("            ImageFileNames = new Tuple<string, string>(partialPath, sketchPath);                                             "));
            sw.WriteLine(string.Format("        }}                                                                                                                    "));
            sw.WriteLine(string.Format("        private void UpdateDesginPath()                                                                                      "));
            sw.WriteLine(string.Format("        {{                                                                                                                    "));
            sw.WriteLine(string.Format("            if (SelectedTabIndex == DrawingTabIndex)                                                                         "));
            sw.WriteLine(string.Format("                GraphicDesignImagePath = Path.Combine(ImageFolder, SelectedTabIndex + \"_\" + SecondSelectedTabIndex + \".pdf\");"));
            sw.WriteLine(string.Format("        }}                                                                                                                    "));
            sw.WriteLine(string.Format("        private string mirrorPath = string.Empty;                                                                            "));
            sw.WriteLine(string.Format("        public string MirrorPath                                                                                             "));
            sw.WriteLine(string.Format("        {{                                                                                                                    "));
            sw.WriteLine(string.Format("            get {{ return mirrorPath; }}                                                                                       "));
            sw.WriteLine(string.Format("            set                                                                                                              "));
            sw.WriteLine(string.Format("            {{                                                                                                                "));
            sw.WriteLine(string.Format("                mirrorPath = value;                                                                                          "));
            sw.WriteLine(string.Format("                RaisePropertyChanged(\"MirrorPath\");                                                                          "));
            sw.WriteLine(string.Format("                                                                                                                             "));
            sw.WriteLine(string.Format("            }}                                                                                                                "));
            sw.WriteLine(string.Format("        }}                                                                                                                    "));
            sw.WriteLine(string.Format("        [XmlIgnore]                                                                                                          "));
            sw.WriteLine(string.Format("        private static string ImageFolder = Path.Combine(G.BasicFolder, @\"Images\\{0}\");                           ", nameE));
            sw.WriteLine(string.Format("                                                                                                                             "));
            sw.WriteLine(string.Format("                                                                                                                             "));
            sw.WriteLine(string.Format("        private int selectedTabIndex;                                                                                        "));
            sw.WriteLine(string.Format("        public int SelectedTabIndex                                                                                          "));
            sw.WriteLine(string.Format("        {{                                                                                                                    "));
            sw.WriteLine(string.Format("            get {{ return selectedTabIndex; }}                                                                                 "));
            sw.WriteLine(string.Format("            set                                                                                                              "));
            sw.WriteLine(string.Format("            {{                                                                                                                "));
            sw.WriteLine(string.Format("                if (selectedTabIndex == value) return;                                                                       "));
            sw.WriteLine(string.Format("                focusControlName = string.Empty;                                                                             "));
            sw.WriteLine(string.Format("                selectedTabIndex = value;                                                                                    "));
            sw.WriteLine(string.Format("                FocusControlName = string.Empty;                                                                             "));
            sw.WriteLine(string.Format("                //SecondSelectedTabIndex = 0;                                                                                "));
            sw.WriteLine(string.Format("                UpdateDesginPath();                                                                                          "));
            sw.WriteLine(string.Format("            }}                                                                                                                "));
            sw.WriteLine(string.Format("        }}                                                                                                                    "));
            sw.WriteLine(string.Format("        private int secondSelectedTabIndex;                                                                                  "));
            sw.WriteLine(string.Format("        public int SecondSelectedTabIndex                                                                                    "));
            sw.WriteLine(string.Format("        {{                                                                                                                    "));
            sw.WriteLine(string.Format("            get {{ return secondSelectedTabIndex; }}                                                                           "));
            sw.WriteLine(string.Format("            set                                                                                                              "));
            sw.WriteLine(string.Format("            {{                                                                                                                "));
            sw.WriteLine(string.Format("                if (secondSelectedTabIndex == value) return;                                                                 "));
            sw.WriteLine(string.Format("                focusControlName = string.Empty;                                                                             "));
            sw.WriteLine(string.Format("                secondSelectedTabIndex = value;                                                                              "));
            sw.WriteLine(string.Format("                FocusControlName = string.Empty;                                                                             "));
            sw.WriteLine(string.Format("                UpdateDesginPath();                                                                                          "));
            sw.WriteLine(string.Format("            }}                                                                                                                "));
            sw.WriteLine(string.Format("        }}                                                                                                                    "));
            sw.WriteLine(string.Format("        private string focusControlName;                                                                                     "));
            sw.WriteLine(string.Format("        public string FocusControlName                                                                                       "));
            sw.WriteLine(string.Format("        {{                                                                                                                    "));
            sw.WriteLine(string.Format("            get {{ return focusControlName; }}                                                                                 "));
            sw.WriteLine(string.Format("            set                                                                                                              "));
            sw.WriteLine(string.Format("            {{                                                                                                                "));
            sw.WriteLine(string.Format("                focusControlName = value;                                                                                    "));
            sw.WriteLine(string.Format("                if (SelectedTabIndex != DrawingTabIndex)                                                                     "));
            sw.WriteLine(string.Format("                {{                                                                                                            "));
            sw.WriteLine(string.Format("                    UpdateImageFileName();                                                                                   "));
            sw.WriteLine(string.Format("                                                                                                                             "));
            sw.WriteLine(string.Format("                }}                                                                                                            "));
            sw.WriteLine(string.Format("            }}                                                                                                                "));
            sw.WriteLine(string.Format("        }}                                                                                                                    "));
            sw.WriteLine(string.Format("        private string graphicDesignImagePath;                                                                               "));
            sw.WriteLine(string.Format("        public string GraphicDesignImagePath                                                                                 "));
            sw.WriteLine(string.Format("        {{                                                                                                                    "));
            sw.WriteLine(string.Format("            get {{ return graphicDesignImagePath; }}                                                                           "));
            sw.WriteLine(string.Format("            set                                                                                                              "));
            sw.WriteLine(string.Format("            {{                                                                                                                "));
            sw.WriteLine(string.Format("                graphicDesignImagePath = value;                                                                              "));
            sw.WriteLine(string.Format("                RaisePropertyChanged(\"GraphicDesignImagePath\");                                                              "));
            sw.WriteLine(string.Format("                                                                                                                             "));
            sw.WriteLine(string.Format("            }}                                                                                                                "));
            sw.WriteLine(string.Format("        }}                                                                                                                    "));
            sw.WriteLine(string.Format("        private Tuple<string, string> imageFileNames;                                                                        "));
            sw.WriteLine(string.Format("        [XmlIgnore]                                                                                                          "));
            sw.WriteLine(string.Format("        public Tuple<string, string> ImageFileNames                                                                          "));
            sw.WriteLine(string.Format("        {{                                                                                                                    "));
            sw.WriteLine(string.Format("            get => imageFileNames;                                                                                           "));
            sw.WriteLine(string.Format("            set                                                                                                              "));
            sw.WriteLine(string.Format("            {{                                                                                                                "));
            sw.WriteLine(string.Format("                                                                                                                             "));
            sw.WriteLine(string.Format("                imageFileNames = value;                                                                                      "));
            sw.WriteLine(string.Format("                RaisePropertyChanged(\"ImageFileNames\");                                                                      "));
            sw.WriteLine(string.Format("            }}                                                                                                                "));
            sw.WriteLine(string.Format("        }}                                                                                                                    "));
            sw.WriteLine(string.Format("        #endregion                                                                                                           "));
        }
        private static void WriteModelMethod(StreamWriter sw, string nameE, string nameC, Dictionary<string, Dictionary<Tuple<string, string>, List<Tuple<string, string, string, string, string, string>>>> parameterDic)
        {
            sw.WriteLine();
            sw.WriteLine(string.Format("        #region Method"));
            sw.WriteLine(string.Format("        "));
            sw.WriteLine(string.Format("        private void InitParam()"));
            sw.WriteLine(string.Format("        {{"));
            //Tab-(次Tab-次Tab简称)-(名称-简称-默认值-单位-绑定值-备注)
            foreach (var tab in parameterDic)
            {
                sw.WriteLine(string.Format("#region {0}", tab.Key));
                foreach (var secTab in tab.Value)
                {
                    sw.WriteLine(string.Format("#region {0}", secTab.Key.Item1));
                    foreach (var item in secTab.Value)
                    {
                        string UpperHead = string.Format("{0}_{1}", secTab.Key.Item2, item.Item2);
                        sw.WriteLine(string.Format("{0}{1} = {2};", string.IsNullOrEmpty(item.Item5) ? "" : "//", UpperHead, item.Item3));
                        //todo
                    }
                    sw.WriteLine(string.Format("#endregion"));
                }
                sw.WriteLine(string.Format("#endregion"));
            }
            sw.WriteLine(string.Format("		}} "));
            sw.WriteLine(string.Format("		private void Build() "));
            sw.WriteLine(string.Format("		{{ "));
            sw.WriteLine(string.Format("			//if (!RegClass.AllowRun()) return; "));
            sw.WriteLine(string.Format("             "));
            sw.WriteLine(string.Format("            var app = this.app;"));
            sw.WriteLine(string.Format("            Document document = app.ActiveUIDocument.Document; "));
            sw.WriteLine(string.Format("            Autodesk.Revit.Creation.Application aCreate = app.Application.Create;"));
            sw.WriteLine(string.Format("            var m_version = app.Application.VersionNumber; "));
            sw.WriteLine(string.Format("            if (CloseWindowEventHandler != null) CloseWindowEventHandler(null, null);"));
            sw.WriteLine(string.Format("             "));
            sw.WriteLine(string.Format("            if (!CheckParams())"));
            sw.WriteLine(string.Format("            {{ "));
            sw.WriteLine(string.Format("            return;"));
            sw.WriteLine(string.Format("            }} "));
            sw.WriteLine(string.Format("            var Prg = new ProgressViewModel(GetTCount(), \"{0}——\");", nameC));
            sw.WriteLine(string.Format("            var prgWindow = new ProgressWindow(Prg); "));
            sw.WriteLine(string.Format("            prgWindow.Show();"));
            sw.WriteLine(string.Format("             "));
            sw.WriteLine(string.Format("            try"));
            sw.WriteLine(string.Format("            {{"));
            sw.WriteLine(string.Format("                var view3d = G.ThreeDView(document); "));
            sw.WriteLine(string.Format("                if (view3d == null) {{ app.ActiveUIDocument.ActiveView = view3d; }}"));
            sw.WriteLine(string.Format("                G.preLoadCirclerfa(document, m_version); "));
            sw.WriteLine(string.Format("                 "));
            sw.WriteLine(string.Format("                TransactionGroup tsg = new TransactionGroup(document, \"Build{0}\");", nameE));
            sw.WriteLine(string.Format("                tsg.Start(); "));
            sw.WriteLine(string.Format("                 "));
            sw.WriteLine(string.Format("                #region 参数 "));
            sw.WriteLine(string.Format("				var levelCollector = new FilteredElementCollector(document).OfClass(typeof(Level)); "));
            sw.WriteLine(string.Format("                var creation = document.Create;"));
            sw.WriteLine(string.Format("                var floorTypeCollector = new FilteredElementCollector(document).OfClass(typeof(FloorType));"));
            sw.WriteLine(string.Format("                FloorType ftBase = floorTypeCollector.FirstOrDefault<Element>(e => e.Name.Contains(\"Generic\") || e.Name.Contains(\"常规\")) as FloorType;"));
            sw.WriteLine(string.Format("                if (ftBase == null) return;"));
            sw.WriteLine(string.Format("                var wcollector = new FilteredElementCollector(document).OfClass(typeof(WallType)); "));
            sw.WriteLine(string.Format("                WallType wtBase = wcollector.FirstOrDefault<Element>(e => e.Name.Contains(\"Generic\") || e.Name.Contains(\"常规\")) as WallType;"));
            sw.WriteLine(string.Format("                if (wtBase == null) return;"));
            sw.WriteLine(string.Format("                #region 起点坐标 "));
            sw.WriteLine(string.Format("				double x0_tab1 = 0; "));
            sw.WriteLine(string.Format("                double y0_tab1 = 0;"));
            sw.WriteLine(string.Format("				double z0_tab1 = 0; "));
            sw.WriteLine(string.Format("				#endregion"));
            sw.WriteLine(string.Format("				"));
            sw.WriteLine(string.Format("                #region 标高 "));
            sw.WriteLine(string.Format("				"));
            sw.WriteLine(string.Format("                string name = \"水平面\";"));
            sw.WriteLine(string.Format("				"));
            sw.WriteLine(string.Format("                Level level; "));
            sw.WriteLine(string.Format("				Level level0; Floor floor0; "));
            sw.WriteLine(string.Format("				"));
            sw.WriteLine(string.Format("				#endregion"));
            sw.WriteLine(string.Format("				"));
            sw.WriteLine(string.Format("				#endregion"));
            sw.WriteLine(string.Format("				"));
            sw.WriteLine(string.Format("				using (Transaction ts1 = new Transaction(document)) "));
            sw.WriteLine(string.Format("                {{ "));
            sw.WriteLine(string.Format("                #region 底板 "));
            sw.WriteLine(string.Format("                ts1.Start(\"底板_Tab1\");"));
            sw.WriteLine(string.Format("                Prg.Show(ts1); "));
            sw.WriteLine(string.Format("                level = G.CreateLevel(document, levelCollector, name, 0);"));
            sw.WriteLine(string.Format("					#endregion"));
            sw.WriteLine(string.Format("				}}"));
            sw.WriteLine(string.Format("				"));
            sw.WriteLine(string.Format("				"));
            sw.WriteLine(string.Format("				tsg.Assimilate(); "));
            sw.WriteLine(string.Format("                tsg.Dispose(); "));
            sw.WriteLine(string.Format("                Prg.Close(); "));
            sw.WriteLine(string.Format("            }} "));
            sw.WriteLine(string.Format("            catch (Exception ex) "));
            sw.WriteLine(string.Format("            {{ "));
            sw.WriteLine(string.Format("            MessageBox.Show(ex.Message, \"发生错误导致建模终止\"); "));
            sw.WriteLine(string.Format("            }} "));
            sw.WriteLine(string.Format("            finally"));
            sw.WriteLine(string.Format("            {{ "));
            sw.WriteLine(string.Format("            Prg.Close(); "));
            sw.WriteLine(string.Format("            }} "));
            sw.WriteLine(string.Format("		}}		 "));
            sw.WriteLine(string.Format("		private bool CheckParams() "));
            sw.WriteLine(string.Format("        {{"));
            sw.WriteLine(string.Format("        return true;"));
            sw.WriteLine(string.Format("        }}"));
            sw.WriteLine(string.Format("        private int GetTCount() "));
            sw.WriteLine(string.Format("        {{"));
            sw.WriteLine(string.Format("        //todo"));
            sw.WriteLine(string.Format("        int n = 80; "));
            sw.WriteLine(string.Format("        return n; "));
            sw.WriteLine(string.Format("        }}"));
            sw.WriteLine(string.Format("		#endregion "));
        }
        #endregion

        #endregion
    }
}

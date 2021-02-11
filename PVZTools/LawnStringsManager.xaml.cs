using ITrainerExtension;
using Microsoft.Win32;
using PVZClass;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PVZTools
{
    /// <summary>
    /// LawnStringsManager.xaml 的交互逻辑
    /// </summary>
    public partial class LawnStringsManager : UserControl
    {
        public LawnStringsManager()
        {
            InitializeComponent();
            LawnStringsDictionary = new Dictionary<string, string>();
            openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "LawnStrings.txt";
            openFileDialog.Filter = "LawnStrings.txt|LawnStrings.txt";
        }

        private Dictionary<string, string> LawnStringsDictionary;
        private OpenFileDialog openFileDialog;
        private string filePath;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            filePath = GetLawnStrings();
            if(filePath != null)
                LoadLawnStrings();
        }

        private string GetLawnStrings()
        {
            if(PVZ.Game != null)
            {
                string path = Path.GetDirectoryName(PVZ.Game.MainModule.FileName) + "\\properties\\LawnStrings.txt";
                if(File.Exists(path))
                    return path;
            }
            return filePath;
        }

        private void LoadLawnStrings()
        {
            LawnStringsDictionary.Clear();
            LBMain.Items.Clear();
            string[] texts = File.ReadAllLines(filePath, Encoding.Default);
            string key = null;
            StringBuilder content = new StringBuilder();
            foreach(string line in texts)
            {
                if(line.StartsWith("[") && line.EndsWith("]"))
                {
                    if(key != null)
                    {
                        if(!LawnStringsDictionary.ContainsKey(key))
                        {
                            LawnStringsDictionary.Add(key, content.ToString());
                            LBMain.Items.Add(key);
                        }
                        content.Clear();
                    }
                    key = line.Substring(1, line.Length - 2);
                }
                else
                {
                    content.AppendLine(line);
                }
            }
            LawnStringsDictionary.Add(key, content.ToString());
            LBMain.Items.Add(key);
        }

        private void LBMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(IsLoaded)
            {
                if(LBMain.SelectedIndex >= 0)
                {
                    TBContent.Text = LawnStringsDictionary[LBMain.SelectedItem.ToString()];
                }
            }
        }

        private void ButtonRead_Click(object sender, RoutedEventArgs e)
        {
            filePath = GetLawnStrings();
            if(filePath != null)
                LoadLawnStrings();
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    Lang.IsChinese ? "没有找到LawnStrings.txt,想想要手动选择吗?" :
                    "Didn't find LawnStrings.txt, select it manually?",
                    Lang.IsChinese ? "找不到文件" : "File not found",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if(result == MessageBoxResult.Yes)
                {
                    if(openFileDialog.ShowDialog() == true)
                    {
                        filePath = openFileDialog.FileName;
                        LoadLawnStrings();
                    }
                }
            }
        }

        private void ButtonSaveItem_Click(object sender, RoutedEventArgs e)
        {
            if(LBMain.SelectedIndex >= 0)
                LawnStringsDictionary[LBMain.SelectedItem.ToString()] = TBContent.Text;
        }

        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            if(File.Exists(filePath))
            {
                using(StreamWriter file = new StreamWriter(new FileStream(filePath, FileMode.Truncate), Encoding.Default))
                {
                    foreach(var key in LawnStringsDictionary.Keys)
                    {
                        file.WriteLine($"[{key}]");
                        file.Write(LawnStringsDictionary[key]);
                    }
                }
            }
        }

        private void ButtonFlushGame_Click(object sender, RoutedEventArgs e)
        {
            if(PVZ.Game != null)
            {
                PVZ.Memory.CreateThread(0x00519390);
            }
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            ButtonSaveItem_Click(null, null);
            ButtonSaveFile_Click(null, null);
        }

        private void MenuItemCut_Click(object sender, RoutedEventArgs e)
        {
            TBContent.Cut();
        }

        private void MenuItemCopy_Click(object sender, RoutedEventArgs e)
        {
            TBContent.Copy();
        }

        private void MenuItemPaste_Click(object sender, RoutedEventArgs e)
        {
            TBContent.Paste();
        }

        private void TBContent_KeyDown(object sender, KeyEventArgs e)
        {
            if(Keyboard.Modifiers == ModifierKeys.Control)
            {
                if(e.Key == Key.S)
                    MenuItemSave_Click(null, null);
                else if(e.Key == Key.D)
                    ButtonFlushGame_Click(null, null);
            }
        }

        private static string SearchContent;

        private void FindMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SearchContent = DarkStyle.InputDialog.ShowInputDialog(
                Lang.IsChinese ? "查找" : "Search",
                Lang.IsChinese ? "请输入你要查找的内容": "Enter what you are looking for",
                null, !Lang.IsChinese);
            if(SearchContent != null)
                FindItem();
        }

        private void FindItem()
        {
            if(MIFindValue.IsChecked)
            {
                if(MIFullMatch.IsChecked == true)
                {
                    int index = LawnStringsDictionary.Values.ToList().FindIndex(LBMain.SelectedIndex + 1, s => s.Trim() == SearchContent);
                    if(index >= 0)
                    {
                        LBMain.SelectedItem = LBMain.Items[index];
                        LBMain.ScrollIntoView(LBMain.SelectedItem);
                    }
                    else
                        MessageBox.Show(
                            Lang.IsChinese ? $"没有找到内容为{SearchContent}的项目" : $"No items found is {SearchContent}",
                            Lang.IsChinese ? "信息" : "Information",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                }
                else if(MIContains.IsChecked == true)
                {
                    int index = LawnStringsDictionary.Values.ToList().FindIndex(LBMain.SelectedIndex + 1, s => s.Contains(SearchContent));
                    if(index >= 0)
                    {
                        LBMain.SelectedItem = LBMain.Items[index];
                        LBMain.ScrollIntoView(LBMain.SelectedItem);
                    }
                    else
                        MessageBox.Show(
                            Lang.IsChinese ? $"没有找到内容包含{SearchContent}的项目" : $"No items found include {SearchContent}",
                            Lang.IsChinese ? "信息" : "Information",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                }
                else if(MIUseRegex.IsChecked == true)
                {
                    int index = LawnStringsDictionary.Values.ToList().FindIndex(LBMain.SelectedIndex + 1, s => Regex.IsMatch(s, SearchContent));
                    if(index >= 0)
                    {
                        LBMain.SelectedItem = LBMain.Items[index];
                        LBMain.ScrollIntoView(LBMain.SelectedItem);
                    }
                    else
                        MessageBox.Show(
                            Lang.IsChinese ? $"没有找到内容匹配{SearchContent}的项目" : $"No items found match {SearchContent}",
                            Lang.IsChinese ? "信息" : "Information",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                }
            }
            else
            {
                if(MIFullMatch.IsChecked == true)
                {
                    if(LBMain.Items.Contains(SearchContent))
                    {
                        LBMain.SelectedItem = SearchContent;
                        LBMain.ScrollIntoView(LBMain.SelectedItem);
                    }
                    else
                        MessageBox.Show(
                            Lang.IsChinese ? $"没有找到内容为{SearchContent}的项目" : $"No items found is {SearchContent}",
                            Lang.IsChinese ? "信息" : "Information",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                }
                else if(MIContains.IsChecked == true)
                {
                    int index = LBMain.Items.OfType<string>().ToList().FindIndex(LBMain.SelectedIndex + 1, s => s.Contains(SearchContent));
                    if(index >= 0)
                    {
                        LBMain.SelectedItem = LBMain.Items[index];
                        LBMain.ScrollIntoView(LBMain.SelectedItem);
                    }
                    else
                        MessageBox.Show(
                            Lang.IsChinese ? $"没有找到内容包含{SearchContent}的项目" : $"No items found include {SearchContent}",
                            Lang.IsChinese ? "信息" : "Information",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                }
                else if(MIUseRegex.IsChecked == true)
                {
                    int index = LBMain.Items.OfType<string>().ToList().FindIndex(LBMain.SelectedIndex + 1, s => Regex.IsMatch(s, SearchContent));
                    if(index >= 0)
                    {
                        LBMain.SelectedItem = LBMain.Items[index];
                        LBMain.ScrollIntoView(LBMain.SelectedItem);
                    }
                    else
                        MessageBox.Show(
                            Lang.IsChinese ? $"没有找到内容匹配{SearchContent}的项目" : $"No items found match {SearchContent}",
                            Lang.IsChinese ? "信息" : "Information",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                }
            }
        }

        private void FindNextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FindItem();
        }

        private void LBMain_KeyDown(object sender, KeyEventArgs e)
        {
            if(Keyboard.Modifiers == ModifierKeys.Control)
            {
                if(e.Key == Key.F)
                    FindMenuItem_Click(null, null);
                else if(e.Key == Key.N)
                    FindNextMenuItem_Click(null, null);
                e.Handled = true;
            }
        }
    }
}

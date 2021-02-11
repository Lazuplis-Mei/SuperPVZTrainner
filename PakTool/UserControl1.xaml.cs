using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using ITrainerExtension;

namespace TrainnerExpend
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        private FolderBrowserDialog _folderBrowserDialog;
        private OpenFileDialog _fileDialog;

        public UserControl1()
        {
            InitializeComponent();
            _folderBrowserDialog = new FolderBrowserDialog
            {
                AddToRecent = false,
                ChangeDirectory = true,
                CreatePrompt = true,
                ShowHidden = true
            };
            _fileDialog = new OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = "pak",
                Filter = "pak|*.pak"
            };
        }
        private bool BytesEqual(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length) return false;
            if (b1 == null || b2 == null) return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i]) return false;
            return true;
        }
        static private void XorBytes(ref byte[] bytes)
        {
            for(int i =0;i<bytes.Length;i++)
            {
                bytes[i] ^= 0xF7;
            }
        }
        private void BtnDir_Click(object sender, RoutedEventArgs e)
        {
            if(Lang.IsChinese)
            {
                _folderBrowserDialog.DirectoryNameText = "目录名";
                _folderBrowserDialog.Title = "选择包含pak文件的文件夹";
                _folderBrowserDialog.SelectButtonText = "选择文件夹";
            }
            else
            {
                _folderBrowserDialog.DirectoryNameText = "DirectoryName";
                _folderBrowserDialog.Title = "Select the folder containing the pak files";
                _folderBrowserDialog.SelectButtonText = "Select folder";
            }
            
            if (_folderBrowserDialog.ShowDialog() == true)
            {
                TBDir.Text = _folderBrowserDialog.DirectoryPath;
            }
        }
        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            if (RBPack.IsChecked == true)
            {
                _fileDialog.CheckFileExists = false;
            }
            if (_fileDialog.ShowDialog() == true)
            {
                TBFile.Text = _fileDialog.FileName;
            }
        }
        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            byte[] curfilehead = new byte[] { 0x37, 0xBD, 0x37, 0x4D, 0xF7, 0xF7, 0xF7, 0xF7, 0xF7 };
            if (RBPack.IsChecked == true)
            {
                var files = GetAllFiles(TBDir.Text);
                if(files == null)
                {
                    return;
                }
                FileStream writer = new FileStream(TBFile.Text, FileMode.Create, FileAccess.Write);
                writer.Write(curfilehead, 0, 9);
                foreach (var file in files)
                {
                    FileInfo info = new FileInfo(file);
                    var innerfile = file.Substring(TBDir.Text.Length + 1);
                    var chars = Encoding.Default.GetBytes(innerfile);
                    writer.WriteByte((byte)(chars.Length ^ 0xF7));
                    XorBytes(ref chars);
                    writer.Write(chars, 0, chars.Length);
                    byte[] size = BitConverter.GetBytes((int)info.Length);
                    XorBytes(ref size);
                    writer.Write(size, 0, 4);
                    byte[] ft = BitConverter.GetBytes(info.LastWriteTime.ToFileTime());
                    XorBytes(ref ft);
                    writer.Write(ft, 0, 8);
                    writer.WriteByte(0xF7);
                }
                writer.Seek(-1, SeekOrigin.Current);
                writer.WriteByte(0x80 ^ 0xF7);
                foreach (var file in files)
                {
                    byte[] content = File.ReadAllBytes(file);
                    XorBytes(ref content);
                    writer.Write(content, 0, content.Length);
                }
                writer.Close();
                if(Lang.IsChinese)
                    MessageBox.Show("打包完成", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Unpack finished", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if(RBUnPack.IsChecked == true)
            {
                if (File.Exists(TBFile.Text))
                {
                    FileStream file = new FileStream(TBFile.Text, FileMode.Open, FileAccess.ReadWrite);
                    if (file.Length > 9)
                    {
                        byte[] filehead = new byte[9];
                        file.Read(filehead, 0, 9);
                        if (BytesEqual(filehead, curfilehead))
                        {
                            UnPackPAK(file, TBDir.Text);
                        }
                        else
                        {
                            string msg = "该pak文件的文件头可能已经损坏\n或这不是一个有效的pvzpak文件\n你依然想要尝试修复它并继续吗?";
                            if(!Lang.IsChinese)
                            {
                                msg = "The header of the pak file may be corrupted.\n or this is not a valid pvzpak file.\nDo you still want to try to repair it and continue?";
                            }
                            if(MessageBox.Show(msg, Lang.IsChinese ? "继续" : "Continue", MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                            {
                                file.Seek(0, SeekOrigin.Begin);
                                file.Write(curfilehead, 0, 9);
                                UnPackPAK(file, TBDir.Text);
                            }
                        }
                    }
                    else
                    {
                        if(Lang.IsChinese)
                            MessageBox.Show("不是有效的pak文件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                            MessageBox.Show("Not a valid pak file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    if(Lang.IsChinese)
                        MessageBox.Show("没有找到文件" + TBFile.Text, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show($"File {TBFile.Text} not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        class Pakfile
        {
            public string path;
            private int size;
            private long lasttime;
            public  Pakfile(string path, int size, long lasttime)
            {
                this.path = path;
                this.size = size;
                this.lasttime = lasttime;
            }
            public void Save(FileStream file)
            {
                byte[] buffer = new byte[size];
                file.Read(buffer, 0, size);
                XorBytes(ref buffer);
                int i = path.LastIndexOf('\\');
                Directory.CreateDirectory(path.Substring(0, i));
                File.WriteAllBytes(path, buffer);
                FileInfo info = new FileInfo(path);
                info.LastWriteTime = DateTime.FromFileTime(lasttime);
            }
        }
        private void UnPackPAK(FileStream file, string outputfile)
        {
            var filelist = new List<Pakfile>();
            do
            {
                int fnl = file.ReadByte() ^ 0xF7;
                byte[] fnb = new byte[fnl];
                file.Read(fnb, 0, fnl);
                XorBytes(ref fnb);
                string fn = outputfile + "\\" + Encoding.Default.GetString(fnb);
                byte[] sizeb = new byte[4];
                file.Read(sizeb, 0, 4);
                XorBytes(ref sizeb);
                int size = BitConverter.ToInt32(sizeb, 0);
                byte[] ftb = new byte[8];
                file.Read(ftb, 0, 8);
                XorBytes(ref ftb);
                long ft = BitConverter.ToInt64(ftb, 0);
                filelist.Add(new Pakfile(fn, size, ft));
            } while ((file.ReadByte() ^ 0xF7) == 0);
            foreach (var f in filelist)
            {
                f.Save(file);
            }
            file.Close();
            if(Lang.IsChinese)
                MessageBox.Show("解包完成", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Pack finished", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            
        }
        //获得目录中的所有文件
        private string[] GetAllFiles(string path)
        {
            var filelist = new List<string>();
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    filelist.Add(file);
                }
            }
            else
            {
                if(Lang.IsChinese)
                    MessageBox.Show($"路径{path}不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show($"Directory {path} not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return null;
            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                foreach (var file in GetAllFiles(dir))
                {
                    filelist.Add(file);
                }
            }
            return filelist.ToArray();
        }
    }
}

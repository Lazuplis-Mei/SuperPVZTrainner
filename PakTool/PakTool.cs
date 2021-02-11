using System;
using ITrainerExtension;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace TrainnerExpend
{

    public class PakTool : ITrainerExtensionUserControl
    {
        [STAThread]
        public static void Main()
        {
            Window window = new Window();
            window.Background = DarkStyle.Globals.Background;
            window.ResizeMode = ResizeMode.CanMinimize;
            window.Title = "Pak解包打包";
            Canvas canvas = new Canvas();
            window.Content = canvas;
            var usercon = new UserControl1();
            Canvas.SetLeft(usercon, 7);
            Canvas.SetTop(usercon, 10);
            window.Width = 580;
            window.Height = 150;
            canvas.Children.Add(usercon);
            window.ShowDialog();
        }
        public string Text => "Pak解包打包";
        public string ToolTip => "Pak解包打包,作者冥谷川恋";

        public string[] TextLang => new[] { "Pak解包打包", "Pak Unpack&Pack" };

        public string[] ToolTipLang => new[] { "Pak解包打包,作者冥谷川恋", "Pak Unpack&Pack by Lazuplis" };

        public void Layout(Window owner, Canvas canvas)
        {
            //设置控件位置
            var usercon = new UserControl1();
            Canvas.SetLeft(usercon, 10);
            Canvas.SetTop(usercon, 60);
            //修改窗口大小
            owner.Width = 580;
            owner.Height = 190;
            canvas.Children.Add(usercon);
        }
    }
}

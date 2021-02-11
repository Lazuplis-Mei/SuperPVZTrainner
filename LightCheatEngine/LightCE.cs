using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LightCheatEngine
{
    public class LightCETable : ITrainerExtension.ITrainerExtensionUserControl
    {
        public string[] TextLang => new[] { "CE表单(Light)", "CE Table(Light)" };
        public string Text =>  "CE表单(Light)";

        public string[] ToolTipLang => new[] { "提供简单的CE地址修改功能", "Provide simple CE address modification function" };
        public string ToolTip => "提供简单的CE地址修改功能";

        public void Layout(Window owner, Canvas canvas)
        {
            //设置控件位置
            var usercon = new LightCETableControl();
            usercon.Tag = owner;
            Canvas.SetLeft(usercon, 20);
            Canvas.SetTop(usercon, 60);
            //修改窗口大小
            owner.Width = 540;
            owner.Height = 495;
            canvas.Children.Add(usercon);
        }
    }

    public class LightCEDisasm : ITrainerExtension.ITrainerExtensionUserControl
    {
        public string[] TextLang => new[] { "反汇编(Light)", "Disassembly(Light)" };
        public string Text => "反汇编(Light)";

        public string[] ToolTipLang => new[] { "提供简单的反汇编功能", "Provides simple disassembly functions" };
        public string ToolTip => "提供简单的反汇编功能";

        public void Layout(Window owner, Canvas canvas)
        {
            //设置控件位置
            var usercon = new LightCEDisasmControl();
            usercon.Tag = owner;
            Canvas.SetLeft(usercon, 20);
            Canvas.SetTop(usercon, 60);
            //修改窗口大小
            owner.Width = 740;
            owner.Height = 695;
            canvas.Children.Add(usercon);
        }
    }
}

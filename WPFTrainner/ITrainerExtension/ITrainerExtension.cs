using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ITrainerExtension
{
    public static class Lang
    {
        public static int Id = 0;
        public static int Count = 2;
        public static bool IsChinese => Id == 0;
        public static void ChangeLanguage(object obj)
        {
            if(obj == null)
                return;
            if(obj is Decorator decorator)
            {
                ChangeLanguage(decorator.Child);
                return;
            }
            if(obj is GridView gridView)
            {
                foreach(var column in gridView.Columns)
                {
                    ChangeLanguage(column.Header);
                }
                return;
            }
            if(obj is FrameworkElement frameworkEle)
            {
                if(frameworkEle.Resources != null && frameworkEle.Resources.Count > 0 && frameworkEle.Resources.Contains("Lang"))
                {
                    try
                    {
                        string text = Regex.Unescape((frameworkEle.Resources["Lang"] as string[])[Id]);
                        if(frameworkEle is TextBlock textBlock)
                            textBlock.Text = text;
                        else if(frameworkEle is ContentControl contentControl)
                        {
                            if(contentControl is HeaderedContentControl headeredContentControl)
                                headeredContentControl.Header = text;
                            else
                                contentControl.Content = text;
                        }
                        else if(frameworkEle is HeaderedItemsControl headeredItemsControl)
                            headeredItemsControl.Header = text;
                    }
                    catch
                    {

                    }
                }
                if(frameworkEle.ContextMenu != null)
                {
                    foreach(var item in frameworkEle.ContextMenu.Items)
                        ChangeLanguage(item);
                }
                if(frameworkEle is Panel panel)
                {
                    foreach(var child in panel.Children)
                        ChangeLanguage(child);
                }
                if(frameworkEle is ItemsControl itemsControl)
                {
                    foreach(var item in itemsControl.Items)
                        ChangeLanguage(item);
                }
                if(frameworkEle is ContentControl contentControl1)
                    ChangeLanguage(contentControl1.Content);
                if(frameworkEle is HeaderedContentControl headeredContentControl1)
                    ChangeLanguage(headeredContentControl1.Header);
                if(frameworkEle is ListView listView)
                    ChangeLanguage(listView.View);
                ChangeLanguage(frameworkEle.ToolTip);
            }
        }
    }
    public interface ITrainerExtensionItem
    {
        string Text { get; }
        string[] TextLang { get; }
        string ToolTip { get; }
        string[] ToolTipLang { get; }
    }
    public interface ITrainerExtensionButton : ITrainerExtensionItem
    {
        void ButtonOnClick();
    }
    public interface IITrainerExtensionCheckBox : ITrainerExtensionItem
    {
        void CheckBoxOnClick(bool ischecked);
    }

    public interface ITrainerExtensionTextBox : ITrainerExtensionItem
    {
        void FunctionOnCall(string text);
    }
    public interface ITrainerExtensionUserControl : ITrainerExtensionItem
    {
        void Layout(Window owner, Canvas canvas);
    }

}

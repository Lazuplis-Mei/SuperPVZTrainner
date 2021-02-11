using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DarkStyle
{
    public static class Globals
    {
        public static readonly Brush Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x25, 0x26));
        public static readonly Brush Foreground = Brushes.White;
        public static bool IsModal(this Window window)
        {
            var filedInfo = typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic);
            return filedInfo != null && (bool)filedInfo.GetValue(window);
         }
    }

    public class DarkButton : Button
    {
        public DarkButton()
        {
            Width = 120;
            Foreground = Globals.Foreground;
            Background = Globals.Background;
            Style = FindResource("DarkButtonStyleBlue") as Style;
            ToolTipService.SetShowDuration(this, 20000);
        }
    }

    public class DarkToggleButton : ToggleButton
    {
        public DarkToggleButton()
        {
            Width = 120;
            Foreground = Globals.Foreground;
            Background = Globals.Background;
            Style = FindResource("ToggleButtonStyle1") as Style;
            ToolTipService.SetShowDuration(this, 20000);
        }
    }

    public class MinimizeButton : DarkButton
    {
        public MinimizeButton(): base()
        {
            BorderThickness = new Thickness(0);
            FontWeight = FontWeights.Bold;
            FontSize = 14;
            Content = "—";
            Width = 30;
            Click += MinimizeButton_Click;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }
    }

    public class CloseButton : DarkButton
    {
        public CloseButton() : base()
        {
            BorderThickness = new Thickness(0);
            FontWeight = FontWeights.Bold;
            FontSize = 14;
            Content = "X";
            Width = 30;
            Style = FindResource("DarkButtonStyleRed") as Style;
            Click += CloseButton_Click;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window.IsModal())
                window.DialogResult = false;
            window.Close();
        }
    }

    public class DarkTextBox : TextBox
    {
        public DarkTextBox()
        {
            VerticalAlignment = VerticalAlignment.Center;
            VerticalContentAlignment = VerticalAlignment.Center;
            MaxLength = 10;
            MinWidth = 80;
            Foreground = Brushes.White;
            Background = Globals.Background;
            BorderThickness = new Thickness(0);
            InputMethod.SetIsInputMethodEnabled(this, false);
            AllowDrop = false;
            ContextMenu = null;
            PreviewTextInput += DarkTextBox_PreviewTextInput;
        }

        private void DarkTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !double.TryParse(Text.Insert(SelectionStart, e.Text), out double _);
        }
    }

    public class DarkCheckBox : CheckBox
    {
        public DarkCheckBox()
        {
            Background = Brushes.Transparent;
            Foreground = Globals.Foreground;
            Style = FindResource("CheckBoxStyle1") as Style;
            VerticalContentAlignment = VerticalAlignment.Center;
        }
    }

    public class DarkComboBox : ComboBox
    {
        public DarkComboBox()
        {
            SelectedIndex = 0;
            Foreground = Globals.Foreground;
            Style = FindResource("ComboBoxStyle1") as Style;
        }
    }

    public class DarkComboBoxItem : ComboBoxItem
    {
        public DarkComboBoxItem()
        {
            Foreground = Globals.Foreground;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            BorderThickness = new Thickness(0);
            Style = FindResource("ComboBoxItemStyle1") as Style;
        }
    }
}

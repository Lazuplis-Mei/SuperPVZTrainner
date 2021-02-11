using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DarkStyle
{
    /// <summary>
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : Window
    {

        public static string ShowInputDialog(string title, string desc, string def = null, bool eng = false)
        {
            InputDialog input = new InputDialog();
            input.TBTitle.Text = title;
            input.TBDesc.Text = desc;
            input.InputValue = def;
            input.TBContent.Text = input.InputValue;
            input.TBContent.Focus();
            input.TBContent.SelectAll();
            if(eng)
            {
                input.BtnOK.Content = "Ok";
                input.BtnCancel.Content = "Cancel";
            }
            if (input.ShowDialog() == true)
                return input.InputValue;
            else
                return def;
        }

        private InputDialog()
        {
            InitializeComponent();
        }

        private string InputValue { get; set; }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            InputValue = TBContent.Text;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void TBContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnOK_Click(null, null);
            }
        }
    }
}

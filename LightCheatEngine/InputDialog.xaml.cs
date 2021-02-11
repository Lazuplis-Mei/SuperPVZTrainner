using ITrainerExtension;
using PVZClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LightCheatEngine
{
    /// <summary>
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(CETableItem ceTableItem)
        {
            InitializeComponent();
            this.cETableItem = ceTableItem;
            TBTitle.Text = (Lang.IsChinese ? "修改地址" : "Change value at...") + ceTableItem.Address.GetAddress();
            TBContent.Text = ceTableItem.DataValue.ToString();
            TBContent.Focus();
            TBContent.SelectAll();
        }
        CETableItem cETableItem;

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            int address = cETableItem.Address.GetAddress();
            try
            {
                switch (cETableItem.DataType)
                {
                    case DataType.字节:
                        PVZ.Memory.WriteByte(address, Convert.ToByte(TBContent.Text));
                        break;
                    case DataType.短整数:
                        PVZ.Memory.WriteShort(address, Convert.ToInt16(TBContent.Text));
                        break;
                    case DataType.整数:
                        PVZ.Memory.WriteInteger(address, Convert.ToInt32(TBContent.Text));
                        break;
                    case DataType.长整数:
                        PVZ.Memory.WriteLong(address, Convert.ToInt64(TBContent.Text));
                        break;
                    case DataType.浮点数:
                        PVZ.Memory.WriteFloat(address, Convert.ToSingle(TBContent.Text));
                        break;
                    case DataType.双精度浮点数:
                        PVZ.Memory.WriteDouble(address, Convert.ToDouble(TBContent.Text));
                        break;
                    case DataType.字符串:
                        PVZ.Memory.WriteString(address, TBContent.Text);
                        break;
                    default:
                        break;
                }
                DialogResult = true;
                Close();
            }
            catch
            {
                if(Lang.IsChinese)
                    MessageBox.Show("数值无效", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show("Invalid value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

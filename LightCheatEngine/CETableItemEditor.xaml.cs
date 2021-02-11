using ITrainerExtension;
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
using System.Windows.Threading;

namespace LightCheatEngine
{
    /// <summary>
    /// CETableItemEditor.xaml 的交互逻辑
    /// </summary>
    public partial class CETableItemEditor : Window
    {
        public CETableItemEditor()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            TBAddress.Focus();
        }

        public CETableItemEditor(CETableItem cETableItem):this()
        {
            if (cETableItem.Address.Offsets.Count > 0)
            {
                CBPointer.IsChecked = true;
                RectBorder.Height = 255;
                Height = 255;
                GridOffset.Visibility = Visibility.Visible;
            }
            TBAddress.Text = cETableItem.Address.BaseAddress.ToString("X8");
            for (int i = 0; i < cETableItem.Address.Offsets.Count; i++)
            {
                int item = cETableItem.Address.Offsets[i];
                if (i == 0)
                    GridOffset.Children.OfType<TextBox>().Last().Text = item.ToString("X8");
                else
                    AddOffset().Text = item.ToString("X8");
            }
            TBDescription.Text = cETableItem.Description;
            CBType.SelectedIndex = (int)cETableItem.DataType;
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            int address;
            try
            {
                address = ExpressionEval.Parse(TBAddress.Text);
            }
            catch
            {
                CETableItem = null;
                TBValue.Text = "0";
                return;
            }
            if (CBPointer.IsChecked == false)
            {
                OffsetAddress offsetAddress = new OffsetAddress(address);
                CETableItem = new CETableItem(offsetAddress, (DataType)CBType.SelectedIndex);
                CETableItem.Description = TBDescription.Text;
                TBValue.Text = CETableItem.DataValue.ToString();
            }
            else
            {
                try
                {
                    OffsetAddress offsetAddress = new OffsetAddress(address, GridOffset.Children.OfType<TextBox>().Select(tb => ExpressionEval.Parse(tb.Text)).ToArray());
                    CETableItem = new CETableItem(offsetAddress, (DataType)CBType.SelectedIndex);
                    CETableItem.Description = TBDescription.Text;
                    TBValue.Text = CETableItem.DataValue.ToString();
                }
                catch
                {
                    CETableItem = null;
                    TBValue.Text = "0";
                    return;
                }
            }
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        public CETableItem CETableItem { get; set; }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Timer_Tick(null, null);
            if (CETableItem == null)
            {
                if(Lang.IsChinese)
                    MessageBox.Show("项目无效", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show("Item is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnAddOffset_Click(object sender, RoutedEventArgs e)
        {
            AddOffset();
        }

        private TextBox AddOffset()
        {
            RectBorder.Height += 24;
            Height += 24;
            GridOffset.Height += 24;
            GridOffset.RowDefinitions.Add(new RowDefinition());
            int row = GridOffset.RowDefinitions.Count;
            Grid.SetRow(BtnAddOffset, row - 1);
            Grid.SetRow(BtnRemoveOffset, row - 1);

            TextBlock textBlock = new TextBlock();
            textBlock.Foreground = Brushes.White;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Text = "+";
            Grid.SetRow(textBlock, row - 2);
            GridOffset.Children.Add(textBlock);

            TextBox textBox = new TextBox();
            textBox.VerticalContentAlignment = VerticalAlignment.Center;
            textBox.Foreground = Brushes.White;
            textBox.Background = Background;
            textBox.Height = 18;
            Grid.SetColumn(textBox, 1);
            Grid.SetColumnSpan(textBox, 2);
            Grid.SetRow(textBox, row - 2);
            GridOffset.Children.Add(textBox);
            return textBox;
        }

        private void BtnRemoveOffset_Click(object sender, RoutedEventArgs e)
        {
            RemoveOffset();
        }

        private void RemoveOffset()
        {
            int row = GridOffset.RowDefinitions.Count;
            if (row == 2)
            {
                RectBorder.Height = 195;
                Height = 195;
                GridOffset.Visibility = Visibility.Hidden;
                CBPointer.IsChecked = false;
            }
            else
            {
                RectBorder.Height -= 24;
                Height -= 24;
                GridOffset.Height -= 24;
                GridOffset.RowDefinitions.RemoveAt(row - 1);
                Grid.SetRow(BtnAddOffset, row - 1);
                Grid.SetRow(BtnRemoveOffset, row - 1);
                GridOffset.Children.Remove(GridOffset.Children.OfType<TextBlock>().Last());
                GridOffset.Children.Remove(GridOffset.Children.OfType<TextBox>().Last());
            }
        }

        private void CBPointer_Click(object sender, RoutedEventArgs e)
        {
            if (CBPointer.IsChecked == true)
            {
                while (GridOffset.RowDefinitions.Count > 2)
                    RemoveOffset();
                RectBorder.Height = 255;
                Height = 255;
                GridOffset.Visibility = Visibility.Visible;
            }
            else
            {
                CETableItem?.Address.Offsets.Clear();
                RectBorder.Height = 195;
                Height = 195;
                GridOffset.Visibility = Visibility.Hidden;
            }
        }

        private void TBAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key== Key.Enter)
                BtnOK_Click(null, null);
        }
    }
}

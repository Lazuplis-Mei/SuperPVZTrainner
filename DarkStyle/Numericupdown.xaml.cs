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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DarkStyle
{
    /// <summary>
    /// Numericupdown.xaml 的交互逻辑
    /// </summary>
    public partial class Numericupdown : UserControl
    {
        private double maxValue = 10;
        private double minValue;

        public Numericupdown()
        {
            InitializeComponent();
        }

        public bool IgnoreAssign { get; set; }

        public double Value
        {
            get
            {
                if (double.TryParse(ValueText.Text, out double result))
                    return result;
                return MinValue;
            }
            set
            {
                value = Math.Max(MinValue, value);
                value = Math.Min(MaxValue, value);
                ValueText.Text = value.ToString();
                if (!IgnoreAssign)
                    ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ValueChanged;

        public double Increment { get; set; } = 1;

        public double MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
                Value = Math.Min(maxValue, Value);
            }
        }

        public double MinValue
        { 
            get => minValue;
            set
            { 
                minValue = value;
                Value = Math.Max(minValue, Value);
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            IncreaseBy(Increment);
        }

        private void IncreaseBy(double value)
        {
            double newValue = Value + value;
            if (newValue > MaxValue)
                Value = MaxValue;
            else
                Value = newValue;
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            DecreaseBy(Increment);
        }

        private void DecreaseBy(double value)
        {
            double newValue = Value - value;
            if (newValue < MinValue)
                Value = MinValue;
            else
                Value = newValue;
        }

        private void ValueText_LostFocus(object sender, RoutedEventArgs e)
        {
            Value = Math.Max(MinValue, Value);
            Value = Math.Min(MaxValue, Value);
        }

        private void ValueText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageUp)
                IncreaseBy(Increment * 5);
            else if(e.Key== Key.PageDown)
                DecreaseBy(Increment * 5);
        }

        private void ValueText_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                IncreaseBy(Increment);
            else if (e.Delta < 0)
                DecreaseBy(Increment);
        }
    }
}

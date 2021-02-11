using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using System.Xml.Serialization;
using PVZClass;
using Microsoft.Win32;
using ITrainerExtension;

namespace LightCheatEngine
{

    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class LightCETableControl : UserControl
    {
        private static readonly ObservableCollection<CETableItem> List = new ObservableCollection<CETableItem>();

        SaveFileDialog saveFileDialog;
        OpenFileDialog openFileDialog;

        public LightCETableControl()
        {
            InitializeComponent();
            LVMain.ItemsSource = List;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            timer.Start();
            saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = "xml";

            openFileDialog = new OpenFileDialog();
            openFileDialog.AddExtension = true;
            openFileDialog.DefaultExt = "xml";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach (var item in List)
            {
                item.UpDate();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CETableItemEditor cETableItemEditor = new CETableItemEditor();
            ITrainerExtension.Lang.ChangeLanguage(cETableItemEditor.Content);
            if (cETableItemEditor.ShowDialog() == true)
                List.Add(cETableItemEditor.CETableItem);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            List.Remove(LVMain.SelectedItem as CETableItem);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            List.Clear();
        }

        private void LVMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = LVMain.SelectedIndex;
            if (index >= 0)
            {
                if (e.GetPosition(LVMain).X < GVCAddr.Width + GVCDesc.Width + GVCType.Width)
                {
                    var item = LVMain.SelectedItem as CETableItem;
                    CETableItemEditor cETableItemEditor = new CETableItemEditor(item);
                    if (cETableItemEditor.ShowDialog() == true)
                    {
                        item.Address = cETableItemEditor.CETableItem.Address;
                        item.DataType = cETableItemEditor.CETableItem.DataType;
                        item.Description = cETableItemEditor.CETableItem.Description;
                        item.UpDate();
                    }
                }
                else
                {
                    InputDialog inputDialog = new InputDialog(LVMain.SelectedItem as CETableItem);
                    inputDialog.ShowDialog();
                }
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            saveFileDialog.Filter = Lang.IsChinese ? "项目文件(*.xml)|*.xml" : "Items(*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(List.GetType());
                xmlSerializer.Serialize(new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate, FileAccess.Write), List);
            }
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = Lang.IsChinese ? "项目文件(*.xml)|*.xml" : "Items(*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                List.Clear();
                XmlSerializer xmlSerializer = new XmlSerializer(List.GetType());
                ObservableCollection<CETableItem> cETableItems = xmlSerializer.Deserialize(new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read)) as ObservableCollection<CETableItem>;
                foreach (var item in cETableItems)
                {
                    item.UpDate();
                    List.Add(item);
                }
            }
        }

        private void LVMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key== Key.S)
                {
                    MenuItem_Click_3(null, null);
                }
            }
            if (e.Key == Key.Delete)
            {
                MenuItem_Click_1(null, null);
            }
        }
    }

    public enum DataType
    {
        字节,
        短整数,
        整数,
        长整数,
        浮点数,
        双精度浮点数,
        字符串
    }

    [Serializable]
    public class CETableItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private OffsetAddress address;
        private string desc;
        private DataType dataType;
        private byte[] buffer;

        public OffsetAddress Address
        {
            get => address;
            set
            {
                address = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Address"));
            }
        }
        public string Description
        {
            get => desc;
            set
            {
                desc = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Description"));
            }
        }
        public DataType DataType
        {
            get => dataType;
            set
            {
                dataType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DataType"));
            }
        }
        public dynamic DataValue
        {
            get 
            {
                switch (dataType)
                {
                    case DataType.字节:
                        return buffer[0];
                    case DataType.短整数:
                        return BitConverter.ToInt16(buffer, 0);
                    case DataType.整数:
                        return BitConverter.ToInt32(buffer, 0);
                    case DataType.长整数:
                        return BitConverter.ToInt64(buffer, 0);
                    case DataType.浮点数:
                        return BitConverter.ToSingle(buffer, 0);
                    case DataType.双精度浮点数:
                        return BitConverter.ToDouble(buffer, 0);
                    case DataType.字符串:
                        return Encoding.Default.GetString(buffer);
                    default:
                        return null;
                }
            }
        }

        public CETableItem(OffsetAddress address, DataType dataType)
        {
            this.address = address;
            this.desc = "<Empty>";
            this.dataType = dataType;
            UpDate();
        }

        public CETableItem()
        {

        }

        public void UpDate()
        {

            switch (dataType)
            {
                case DataType.字节:
                    buffer = PVZ.Memory.ReadBytes(address.GetAddress(), 1);
                    break;
                case DataType.短整数:
                    buffer = PVZ.Memory.ReadBytes(address.GetAddress(), 2);
                    break;
                case DataType.整数:
                    buffer = PVZ.Memory.ReadBytes(address.GetAddress(), 4);
                    break;
                case DataType.长整数:
                    buffer = PVZ.Memory.ReadBytes(address.GetAddress(), 8);
                    break;
                case DataType.浮点数:
                    buffer = PVZ.Memory.ReadBytes(address.GetAddress(), 4);
                    break;
                case DataType.双精度浮点数:
                    buffer = PVZ.Memory.ReadBytes(address.GetAddress(), 8);
                    break;
                case DataType.字符串:
                    buffer = PVZ.Memory.ReadToNull(address.GetAddress());
                    break;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DataValue"));
        }
    }

    public class OffsetAddress
    {
        public int BaseAddress { get; set; }
        public List<int> Offsets { get; set; }
        public int GetAddress()
        {
            int addr = BaseAddress;
            foreach (var offset in Offsets)
            {
                addr = PVZ.Memory.ReadInteger(addr) + offset;
            }
            return addr;
        }
        public OffsetAddress(int address, params int[] offsets)
        {
            BaseAddress = address;
            Offsets = new List<int>(offsets);
        }
        public OffsetAddress()
        {

        }
        public override string ToString()
        {
            if (Offsets.Count == 0)
                return GetAddress().ToString("X8");
            else
                return $"P->{GetAddress().ToString("X8")}";
        }
    }
}

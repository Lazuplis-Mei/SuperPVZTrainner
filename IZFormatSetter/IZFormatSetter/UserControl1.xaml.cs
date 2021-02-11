using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using PVZClass;


namespace IZFormatSetter
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : System.Windows.Controls.UserControl
    {

        static UserControl1()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
        }

        private string[] dialogFilter = new[] { "iz场地文件(*.izf)|*.izf", "iz formation(*.izf)|*.izf" };
        private string[] saveText = new[] { "保存当前场景", "Save current" };
        private string[] loadText = new[] { "读取场景文件", "Load from file" };
        //构造函数，即使用new时调用的函数
        public UserControl1()
        {
            InitializeComponent();
            PlantImages = Resources["PlantImages"] as BitmapImage[];//获取植物图片资源，资源在UserControl.xaml中定义
            //创建保存文件的对话框
            saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.DefaultExt = "izf";
            saveFileDialog.AddExtension = true;
            //创建打开文件的对话框
            openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.DefaultExt = "izf";
            openFileDialog.AddExtension = true;
        }

        int SelectedCard = -1;//当前选中的卡片

        BitmapImage[] PlantImages;//植物图片资源数组

        Microsoft.Win32.SaveFileDialog saveFileDialog;//保存文件的对话框
        Microsoft.Win32.OpenFileDialog openFileDialog;//打开文件的对话框
        private Type mainForm;
        private Form wkForm;

        //CardSelector是一个Image对象，在UserControl.xaml的141行定义
        //当CardSelector被鼠标按下时的事件处理器，事件由UserControl.xaml的141行138列挂载
        private void CardSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(CardSelector);//获取鼠标相对于CardSelector的位置
            double gridWidth = CardSelector.Width / 8;//得到一格的宽度
            double gridHeight = CardSelector.Height / 6;//得到一格的高度

            int row = (int)(pos.Y / gridHeight);//得到鼠标点击的行
            int column = (int)(pos.X / gridWidth);//得到鼠标点击的列

            //RectCard是一个Rectangle对象，在UserControl.xaml的142行定义
            //设置RectCard的位置，以正好套住选中的卡片
            Canvas.SetLeft(RectCard, 552 + (int)gridWidth * column);
            Canvas.SetTop(RectCard, 12 + (int)gridHeight * row);
            //并将其设置为可见
            RectCard.Visibility = Visibility.Visible;

            //同时得到你选择的卡片序号
            SelectedCard = row * 8 + column;
        }

        //ImageBackground是一个Image对象，在UserControl.xaml的119行定义
        //当ImageBackground被鼠标按下时的事件处理器，事件由UserControl.xaml的119行145列挂载
        private void ImageBackground_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //LawnScene是一个Grid对象，在UserControl.xaml的120行定义，有6行9列
            Point pos = e.GetPosition(LawnScene);//获取鼠标相对于LawnScene的位置
            double gridWidth = LawnScene.Width / 9;//得到一格的宽度
            double gridHeight = LawnScene.Height / 6;//得到一格的高度

            int row = Math.Max(0, Math.Min((int)(pos.Y / gridHeight), 5));//得到鼠标点击的行，并限制其范围在0到5之间
            int column = Math.Max(0, Math.Min((int)(pos.X / gridWidth), 8));//得到鼠标点击的列，并限制其范围在0到8之间

            //如果鼠标左键是按下状态而鼠标右键是松开状态
            if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
            {
                //BtnShovel是一个ToggleButton对象，在UserControl.xaml的147行定义
                //如果按钮铲子(BtnShovel)已经按下
                if (BtnShovel.IsChecked == true)
                {
                    Image rmi = null;//临时变量，因为在foreach中无法操作被遍历集合

                    foreach (Image img in LawnScene.Children)//对于LawnScene中的每一个Image对象
                    {
                        if (Grid.GetRow(img) == row && Grid.GetColumn(img) == column)//如果存在一个img的行和列等于鼠标点击的行和列
                            rmi = img;//获取这个img对象
                    }
                    //遍历结束后将其从LawnScene中移除，因为在foreach中无法移除
                    LawnScene.Children.Remove(rmi);
                    //结束返回
                    return;
                }

                //************以下是按钮铲子没有被按下的情况***************



                if (SelectedCard == -1) return;//如果没有选中任何卡片即返回

                foreach (Image img in LawnScene.Children)//对于LawnScene中的每一个Image对象
                {
                    //Tag是一个用于快捷存放属性的变量，每个ui控件都有，想用来存放什么就存放什么
                    PVZ.PlantType pType = (PVZ.PlantType)img.Tag;//获得这个Image对象对应的植物类型(Tag)
                    if (pType == PVZ.PlantType.Tallnut)//如果这个对象是高建国
                    {
                        if (Grid.GetRow(img) == row - 1 && Grid.GetColumn(img) == column)//并且行列等于鼠标的行列（由于高建国竖占2行，所以判断时减去一行）
                        {
                            //并且选择的卡片不是南瓜头，睡莲，花盆
                            if (SelectedCard != (int)PVZ.PlantType.Pumpkin && SelectedCard != (int)PVZ.PlantType.LilyPad && SelectedCard != (int)PVZ.PlantType.Pot && SelectedCard != (int)PVZ.PlantType.CoffeeBean)
                            {
                                //则判断该位置被占用，什么也不做直接返回
                                return;
                            }
                        }
                    }
                    else
                    {
                        //不是高建国则普通的判断行列是否等于鼠标的行列
                        if (Grid.GetRow(img) == row  && Grid.GetColumn(img) == column)
                        {

                            //如果这个对象等于被选中的卡片（否则就能无限叠花盆），或选择的卡片不是南瓜头，睡莲，花盆
                            if ((int)img.Tag == SelectedCard || (SelectedCard != (int)PVZ.PlantType.Pumpkin &&
                                SelectedCard != (int)PVZ.PlantType.LilyPad &&
                                SelectedCard != (int)PVZ.PlantType.Pot &&
                                SelectedCard != (int)PVZ.PlantType.CoffeeBean &&
                                (int)img.Tag != (int)PVZ.PlantType.Pumpkin &&
                                (int)img.Tag != (int)PVZ.PlantType.LilyPad &&
                                (int)img.Tag != (int)PVZ.PlantType.Pot &&
                                (int)img.Tag != (int)PVZ.PlantType.CoffeeBean))
                            {
                                //则判断该位置被占用，什么也不做直接返回
                                return;
                            }
                        }
                    }
                }
                //*************以下是位置没有被占用的情况**************


                Image image = new Image();//创建一个Image对象
                image.Source = PlantImages[SelectedCard];//图片设置为当前选中的卡片的图片
                image.Tag = SelectedCard;//Tag设置为当前选中的卡片
                image.IsHitTestVisible = false;//设置图片不响应鼠标的点击
                //将image放到Grid的指定行列
                Grid.SetRow(image, row);
                Grid.SetColumn(image, column);

                //如果是玉米加农炮，设置横占两列（否则玉米加农炮的图片会缩在一格里）
                if (SelectedCard == (int)PVZ.PlantType.CobCannon)
                    Grid.SetColumnSpan(image, 2);
                //如果是高建国，设置竖占两行，并行数坐标-1（否则高建国的图片会缩在一格里）
                if (SelectedCard == (int)PVZ.PlantType.Tallnut)
                {
                    Grid.SetRow(image, Math.Max(0, row - 1));
                    Grid.SetRowSpan(image, 2);
                }
                //将image加入到LawnScene中
                LawnScene.Children.Add(image);
            }
            //如果按下的是右键而左键松开
            else if (e.RightButton == MouseButtonState.Pressed && e.LeftButton == MouseButtonState.Released)
            {
                Image rmi = null;
                foreach (Image img in LawnScene.Children)//对于LawnScene中的每一个Image对象
                {
                    //如果这个对象的行列等于鼠标的行列
                    if ((int)img.Tag == (int)PVZ.PlantType.Tallnut)
                    {
                        if (Grid.GetRow(img) == row - 1 && Grid.GetColumn(img) == column)
                            rmi = img;
                    }
                    else
                    {
                        if (Grid.GetRow(img) == row && Grid.GetColumn(img) == column)
                            rmi = img;
                    }
                }
                //遍历结束后将其从LawnScene中移除
                LawnScene.Children.Remove(rmi);
            }

        }

        //按钮[铲子]被点击的事件处理器，事件由UserControl.xaml的147行153列挂载
        private void BtnShovel_Click(object sender, RoutedEventArgs e)
        {
            if (BtnShovel.IsChecked == true)
            {
                RectCard.Visibility = Visibility.Hidden;
                CardSelector.IsEnabled = false;
                SelectedCard = -1;
            }
            else
            {
                CardSelector.IsEnabled = true;
            }
        }

        //按钮[清空场地]被点击的事件处理器，事件由UserControl.xaml的153行124列挂载
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            //清空LawnScene中所有的对象
            LawnScene.Children.Clear();
        }

        //按钮[布阵]被点击的事件处理器，事件由UserControl.xaml的162行152列挂载
        private void SetFormat_Click(object sender, RoutedEventArgs e)
        {
            //先将游戏场上的所有植物清除
            foreach (var plant in PVZ.AllPlants)
            {
                plant.Exist = false;
            }
            //再将LawnScene中所有的植物种在指定的行列，Fix则是使植物不要动
            foreach (Image item in LawnScene.Children)
            {
                if ((int)item.Tag == (int)PVZ.PlantType.Tallnut)
                    PVZ.CreatePlant((PVZ.PlantType)item.Tag, Grid.GetRow(item), (byte)Grid.GetColumn(item)).Fix();
                else
                    PVZ.CreatePlant((PVZ.PlantType)item.Tag, Grid.GetRow(item) - 1, (byte)Grid.GetColumn(item)).Fix();
            }
        }

        //按钮[读取现状]被点击的事件处理器，事件由UserControl.xaml的161行155列挂载
        private void ReadInfo_Click(object sender, RoutedEventArgs e)
        {
            //先清空LawnScene中所有的对象
            LawnScene.Children.Clear();
            foreach (var plant in PVZ.AllPlants)//对于游戏场上的所有植物
            {
                //如果存在，并且没有被压扁，并且植物类型在卡槽范围中
                if (plant.Exist && !plant.Squash && (int)plant.Type >= 0 && (int)plant.Type <= 48)
                {
                    //创建一个Image对象，并加入到LawnScene中
                    Image image = new Image();
                    image.Source = PlantImages[(int)plant.Type];
                    image.Tag = (int)plant.Type;
                    image.IsHitTestVisible = false;
                    Grid.SetRow(image, plant.Row + 1);
                    Grid.SetColumn(image, plant.Column);
                    if (plant.Type == PVZ.PlantType.CobCannon)
                        Grid.SetColumnSpan(image, 2);
                    if (plant.Type == PVZ.PlantType.Tallnut)
                    {
                        Grid.SetRow(image, plant.Row);
                        Grid.SetRowSpan(image, 2);
                    }
                    LawnScene.Children.Add(image);
                }
            }
        }

        //UserControl被加载到窗口上时的事件处理器，事件由UserControl.xaml的6行57列挂载
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            mainForm = Assembly.Load(Properties.Resources.IZ自制关卡快捷布阵器__V1_1_2_).GetType("IZ自制关卡快捷布阵器_V1._1._2.Main");
            wkForm = (System.Windows.Forms.Form)Activator.CreateInstance(mainForm);
            wkForm.FormClosing += WkForm_FormClosing;
            ReadInfo_Click(null, null);
        }

        private void WkForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            (Tag as Window).Show();
            e.Cancel = true;
            wkForm.Hide();
        }

        //鼠标在ImageBackground上移动的事件，事件由UserControl.xaml的119行145列挂载
        private void ImageBackground_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)//如果功能键Shift被按下
            {
                //调用被点击的事件处理器
                ImageBackground_MouseDown(null, e);//相当于按住Shift之后，鼠标一直在持续点击，达到Shift连续操作的效果
            }
        }

        //按钮[保存场景] 被点击的事件处理器
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            saveFileDialog.Filter = dialogFilter[ITrainerExtension.Lang.Id];
            saveFileDialog.Title = saveText[ITrainerExtension.Lang.Id];
            if (saveFileDialog.ShowDialog() == true)//如果保存文件对话框选到了文件
            {
                //using可以自动管理释放资源流
                using (FileStream file = File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))//创建文件流进行写入
                {
                    byte[] buffer = Encoding.Default.GetBytes(".izf");//文件头数据，用以读取时识别文件
                    file.Write(buffer, 0, buffer.Length);//写入文件头数据

                    file.WriteByte((byte)LawnScene.Children.Count);//写入LawnScene中的Image数量
                    foreach (Image img in LawnScene.Children)//对于LawnScene中的每一个Image
                    {
                        file.WriteByte((byte)Grid.GetRow(img));//写入所在行
                        file.WriteByte((byte)Grid.GetColumn(img));//写入所在列
                        file.WriteByte((byte)(int)img.Tag);//写入对应的卡片
                    }
                }
            }
        }

        //按钮[读取场景] 被点击的事件处理器
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = dialogFilter[ITrainerExtension.Lang.Id];
            openFileDialog.Title = loadText[ITrainerExtension.Lang.Id];
            if (openFileDialog.ShowDialog() == true)//如果打开文件对话框选到了文件
            {
                //先清空LawnScene中的对象
                LawnScene.Children.Clear();
                using (FileStream file = File.OpenRead(openFileDialog.FileName))//创建文件流进行读取
                {
                    byte[] buffer = new byte[4];//缓冲区定义
                    file.Read(buffer, 0, buffer.Length);//读取文件头数据
                    if (Encoding.Default.GetString(buffer) == ".izf")//如果文件头符合
                    {
                        int count = file.ReadByte();//读取对象数量
                        for (int i = 0; i < count; i++)
                        {
                            //并将所表示的Image创建，加入到LawnScene中
                            Image image = new Image();
                            image.IsHitTestVisible = false;
                            int row = file.ReadByte();
                            int column = file.ReadByte();
                            int type = file.ReadByte();
                            image.Source = PlantImages[type];
                            Grid.SetRow(image, row);
                            Grid.SetColumn(image, column);
                            if (type == (int)PVZ.PlantType.CobCannon)
                                Grid.SetColumnSpan(image, 2);
                            if (type == (int)PVZ.PlantType.Tallnut)
                                Grid.SetRowSpan(image, 2);
                            image.Tag = type;
                            LawnScene.Children.Add(image);
                        }
                    }
                }
            }
        }

        private void FindGame_Click(object sender, RoutedEventArgs e)
        {
            PVZ.CloseGame();
            if (PVZ.RunGame())
                (Tag as Window).Title = "已找到游戏";
            else
                (Tag as Window).Title = "未找到游戏";
        }

        private void BtnWK_Click(object sender, RoutedEventArgs e)
        {
            (Tag as Window).Hide();
            wkForm.Show();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            wkForm.Dispose();
        }
    }

    //插件类
    public class IZFormatSetterPlugIn : ITrainerExtension.ITrainerExtensionUserControl
    {
        [STAThread]
        public static void Main()//主函数只有测试的时候执行，实际由修改器接管插件，直接执行PutUp函数
        {
            Window window = new Window();//创建一个新的窗口
            window.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x25, 0x25, 0x26));//设置背景色
            window.ResizeMode = ResizeMode.CanMinimize;//设置窗口大小固定
            Canvas canvas = new Canvas();//创建一个布局控件
            window.Content = canvas;//窗口的采用这个布局控件
            UserControl1 userControl1 = new UserControl1();//创建UserControl1
            userControl1.FindGame.Visibility = Visibility.Visible;
            userControl1.Tag = window;
            window.Height = 500;
            window.Width = 835;
            Canvas.SetLeft(userControl1, 10);
            Canvas.SetTop(userControl1, 50);
            canvas.Children.Add(userControl1);//并把UserControl1加到canvas中
            window.ShowDialog();//显示窗口
        }

        public string Text => "IZ布阵器";
        public string[] TextLang => new[] { "IZ布阵器", "IZ Format Setter" };

        public string ToolTip => "IZ布阵器V1.1,作者冥谷川恋,内含Winkle雪线布阵器V1.1.2";
        public string[] ToolTipLang => new[] { "IZ布阵器V1.1,作者冥谷川恋,内含Winkle雪线布阵器V1.1.2" , "IZ Format SetterV1.1 by Lazuplis,include IZ Format SetterV1.1.2 form Winkle雪线" };

        public void Layout(Window owner, Canvas canvas)//接口函数，由修改器调用
        {
            UserControl1 userControl1 = new UserControl1();//创建UserControl1
            userControl1.Tag = owner;
            owner.Height = 500;
            owner.Width = 835;
            Canvas.SetLeft(userControl1, 10);
            Canvas.SetTop(userControl1, 50);
            canvas.Children.Add(userControl1);//并把UserControl1加到canvas中
        }
    }

}

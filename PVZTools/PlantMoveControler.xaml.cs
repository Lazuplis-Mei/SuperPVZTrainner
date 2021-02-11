using PVZClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PVZTools
{
    /// <summary>
    /// PlantMoveControler.xaml 的交互逻辑
    /// </summary>
    public partial class PlantMoveControler : System.Windows.Controls.UserControl
    {
        public PlantMoveControler()
        {
            InitializeComponent();
        }
        GlobalKeyboardHook keyboardHook = new GlobalKeyboardHook();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            keyboardHook.KeyDownEvent += KeyboardHook_KeyDownEvent;
            keyboardHook.Install();
        }

        private void KeyboardHook_KeyDownEvent(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            
            var plant1 = new PVZ.Plant((int)NudPlantIndex1.Value);
            var plant2 = new PVZ.Plant((int)NudPlantIndex2.Value);
            switch(e.KeyValue)
            {
                case (int)Keys.Up:
                    plant1.Row--;
                    break;
                case (int)Keys.Down:
                    plant1.Row++;
                    break;
                case (int)Keys.Left:
                    plant1.Column--;
                    break;
                case (int)Keys.Right:
                    plant1.Column++;
                    break;
                case (int)Keys.W:
                    plant2.Row--;
                    break;
                case (int)Keys.S:
                    plant2.Row++;
                    break;
                case (int)Keys.A:
                    plant2.Column--;
                    break;
                case (int)Keys.D:
                    plant2.Column++;
                    break;
            }
            var pos = new System.Drawing.Point(plant1.Row, plant1.Column);
            PVZ.RCToXY(ref pos);
            plant1.X = pos.X;
            plant1.Y = pos.Y;
            pos = new System.Drawing.Point(plant2.Row, plant2.Column);
            PVZ.RCToXY(ref pos);
            plant2.X = pos.X;
            plant2.Y = pos.Y;
            Refresh();
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            keyboardHook.UnInstall();
        }

        private void NudPlantIndex1_ValueChanged(object sender, EventArgs e)
        {
            if (IsLoaded) Refresh();
        }

        private void Refresh()
        {
            var plant = new PVZ.Plant((int)NudPlantIndex1.Value);
            TBSelPlant1.Text = plant.Type.GetDescription();
            TBSelPlantRow1.Text = $"{plant.Row}行";
            TBSelPlantColumn1.Text = $"{plant.Column}列";
            plant = new PVZ.Plant((int)NudPlantIndex2.Value);
            TBSelPlant2.Text = plant.Type.GetDescription();
            TBSelPlantRow2.Text = $"{plant.Row}行";
            TBSelPlantColumn2.Text = $"{plant.Column}列";
        }

        private void NudPlantIndex2_ValueChanged(object sender, EventArgs e)
        {
            if (IsLoaded) Refresh();
        }
    }
}

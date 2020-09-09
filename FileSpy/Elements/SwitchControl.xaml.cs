using System;
using System.Collections.Generic;
using System.Linq;
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

namespace FileSpy.Elements
{
    /// <summary>
    /// Логика взаимодействия для SwitchControl.xaml
    /// </summary>
    public partial class SwitchControl : UserControl
    {
        public bool Mode { get; private set; }
        bool Busy;

        public SwitchControl()
        {
            InitializeComponent();
            Mode = true;
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Mode)
                Task.Run(TurnOff);
            else
                Task.Run(TurnOn);
        }

        private void TurnOff()
        {
            if (!Busy)
            {
                Busy = true;
                Mode = false;
                int delay = 15;
                for (int i = 0; i < 10; i++)
                {
                    Dispatcher.Invoke(() => RedBell.Opacity += 0.05);
                    Dispatcher.Invoke(() => WhiteLine.X2 += 0.4);
                    Dispatcher.Invoke(() => RedLine.X2 += 0.4);
                    Dispatcher.Invoke(() => WhiteLine.Y2 += 0.7);
                    Dispatcher.Invoke(() => RedLine.Y2 += 0.7);

                    Dispatcher.Invoke(() => RedSwitch.Opacity += 0.05);
                    Dispatcher.Invoke(() => RedSwitch.Margin = new Thickness(RedSwitch.Margin.Left, RedSwitch.Margin.Top, RedSwitch.Margin.Right - 1.75, RedSwitch.Margin.Bottom));
                    Dispatcher.Invoke(() => SwitchCircle.Margin = new Thickness(SwitchCircle.Margin.Left + 1, SwitchCircle.Margin.Top, SwitchCircle.Margin.Right, SwitchCircle.Margin.Bottom));
                    delay--;
                    Thread.Sleep(delay);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dispatcher.Invoke(() => RedBell.Opacity += 0.05);
                    Dispatcher.Invoke(() => WhiteLine.X2 += 0.4);
                    Dispatcher.Invoke(() => RedLine.X2 += 0.4);
                    Dispatcher.Invoke(() => WhiteLine.Y2 += 0.7);
                    Dispatcher.Invoke(() => RedLine.Y2 += 0.7);

                    Dispatcher.Invoke(() => RedSwitch.Opacity += 0.05);
                    Dispatcher.Invoke(() => RedSwitch.Margin = new Thickness(RedSwitch.Margin.Left, RedSwitch.Margin.Top, RedSwitch.Margin.Right - 1.75, RedSwitch.Margin.Bottom));
                    Dispatcher.Invoke(() => SwitchCircle.Margin = new Thickness(SwitchCircle.Margin.Left + 1, SwitchCircle.Margin.Top, SwitchCircle.Margin.Right, SwitchCircle.Margin.Bottom));
                    delay++;
                    Thread.Sleep(delay);
                }
                Busy = false;
            }
        }

        private void TurnOn()
        {
            if (!Busy)
            {
                Busy = true;
                Mode = true;
                int delay = 15;
                for (int i = 0; i < 10; i++)
                {
                    Dispatcher.Invoke(() => RedBell.Opacity -= 0.05);
                    Dispatcher.Invoke(() => WhiteLine.X2 -= 0.4);
                    Dispatcher.Invoke(() => RedLine.X2 -= 0.4);
                    Dispatcher.Invoke(() => WhiteLine.Y2 -= 0.7);
                    Dispatcher.Invoke(() => RedLine.Y2 -= 0.7);

                    Dispatcher.Invoke(() => RedSwitch.Opacity -= 0.05);
                    Dispatcher.Invoke(() => RedSwitch.Margin = new Thickness(RedSwitch.Margin.Left, RedSwitch.Margin.Top, RedSwitch.Margin.Right + 1.75, RedSwitch.Margin.Bottom));
                    Dispatcher.Invoke(() => SwitchCircle.Margin = new Thickness(SwitchCircle.Margin.Left - 1, SwitchCircle.Margin.Top, SwitchCircle.Margin.Right, SwitchCircle.Margin.Bottom));
                    delay--;
                    Thread.Sleep(delay);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dispatcher.Invoke(() => RedBell.Opacity -= 0.05);
                    Dispatcher.Invoke(() => WhiteLine.X2 -= 0.4);
                    Dispatcher.Invoke(() => RedLine.X2 -= 0.4);
                    Dispatcher.Invoke(() => WhiteLine.Y2 -= 0.7);
                    Dispatcher.Invoke(() => RedLine.Y2 -= 0.7);

                    Dispatcher.Invoke(() => RedSwitch.Opacity -= 0.05);
                    Dispatcher.Invoke(() => RedSwitch.Margin = new Thickness(RedSwitch.Margin.Left, RedSwitch.Margin.Top, RedSwitch.Margin.Right + 1.75, RedSwitch.Margin.Bottom));
                    Dispatcher.Invoke(() => SwitchCircle.Margin = new Thickness(SwitchCircle.Margin.Left - 1, SwitchCircle.Margin.Top, SwitchCircle.Margin.Right, SwitchCircle.Margin.Bottom));
                    delay++;
                    Thread.Sleep(delay);
                }

                Busy = false;
            }
        }
    }
}

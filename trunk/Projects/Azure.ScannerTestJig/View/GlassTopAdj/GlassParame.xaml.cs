﻿using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.GlassTopAdj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Azure.ScannerTestJig.View.GlassTopAdj
{
    /// <summary>
    /// GlassParame.xaml 的交互逻辑
    /// </summary>
    public partial class GlassParame : UserControl
    {
        public GlassParame()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }
        void IVContorl_Loaded(object sender, EventArgs e)
        {

            GlassTopAdjustViewModel vm = Workspace.This.GlassAdjustWindVM;
            if (vm != null)
            {
                this.DataContext = vm;
                vm.InitIVControls();
            }
        }
        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox tBox = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null)
                {
                    binding.UpdateSource();
                }

                float laserPower = Convert.ToSingle(tBox.Text);
                GlassTopAdjustViewModel IvVm = DataContext as GlassTopAdjustViewModel;
                if (IvVm != null)
                {
                    if (tBox.Equals(_LaserBPowerTextBox))
                    {
                        IvVm.LaserCPower = laserPower;
                        if (laserPower == 0)
                        {
                            IvVm.IsLaserR2Selected = false;
                        }
                        else
                        {
                            IvVm.IsLaserR2Selected = true;
                        }
                    }
                   
                }
            }
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            rec.Width = rec.Width - 2;
            rec.Height = rec.Height - 2;
        }
    }
}

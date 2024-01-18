﻿using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.FocusAdjust;
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

namespace Azure.ScannerTestJig.View
{
    /// <summary>
    /// FocusAdjustSubWind.xaml 的交互逻辑
    /// </summary>
    public partial class FocusAdjustSubWind : Window
    {
        public FocusAdjustSubWind()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Workspace.This.Owner.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.This.PramaSetteVM.IsStopning)
            {
                string caption = "Scanning Mode";
                string message = "Scanning mode is busy.\nWould you like to terminate the current operation?";
                System.Windows.MessageBoxResult dlgResult = System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo);
                if (dlgResult == System.Windows.MessageBoxResult.No)
                {
                    e.Cancel = true; // don't allow the application to close
                    return;
                }

                PramaSetteViewModule viewModel = Workspace.This.PramaSetteVM;
                viewModel.ExecuteStopScanCommand(null);
            }
        
        }
    }
}

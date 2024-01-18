﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;   // Regex
using Azure.ScannerEUI.ViewModel;
using Azure.ImagingSystem;
using Azure.Configuration.Settings;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// Interaction logic for MotorControl.xaml
    /// </summary>
    public partial class MotorControl : UserControl
    {
        public MotorControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MotorControl_Loaded);
        }

        void MotorControl_Loaded(object sender, RoutedEventArgs e)
        {
            //MotorViewModel vm = Workspace.This.MotorVM;
            //if (vm != null)
            //{
            //    vm.InitMotorControls();
            //}
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
            }
        }

    }
}

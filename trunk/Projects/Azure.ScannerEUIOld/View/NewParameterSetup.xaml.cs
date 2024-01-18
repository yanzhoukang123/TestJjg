using System;
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
using System.Windows.Shapes;
using System.Text.RegularExpressions;   // Regex
using Azure.ScannerEUI.ViewModel;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// NewParameterSetup.xaml 
    /// </summary>
    public partial class NewParameterSetup : Window
    {
        public NewParameterSetup()
        {
            InitializeComponent();
            DataContext = Workspace.This.NewParameterVM;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NewParameterSetupViewModel viewModel = DataContext as NewParameterSetupViewModel;
            if (viewModel != null)
            {
                viewModel.Initialize();
            }

            if (Workspace.This.NewParameterVM != null)
                Workspace.This.NewParameterVM.ExecuteParametersReadCommand(null);      // automatically read parameters before opening the window
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

        private void Window_Closed(object sender, EventArgs e)
        {
           
        }
    }
}

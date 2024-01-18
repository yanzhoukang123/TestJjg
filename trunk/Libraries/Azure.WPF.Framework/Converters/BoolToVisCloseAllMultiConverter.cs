﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Azure.WPF.Framework
{
    public class BoolToVisCloseAllMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var retval = Visibility.Hidden;

            // Expected input format: IsActiveDocument, IsCropping
            if (value[0] is Boolean && value[1] is Boolean)
            {
                // IsActiveDocument && !IsCropping
                if ((bool)value[0] && !(bool)value[1])
                {
                    retval = Visibility.Visible;
                }
                else
                {
                    retval = Visibility.Collapsed;
                }
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Azure.WPF.Framework
{
    public class BooleanToVisibleMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var retval = Visibility.Collapsed;
            if (value[0] is Boolean && value[1] is Boolean)
            {                
                if ((bool)value[0] && (bool)value[1])
                {
                    retval = Visibility.Visible;
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

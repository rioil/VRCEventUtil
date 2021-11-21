using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace VRCEventUtil.Converters
{
    public class EnumDisplayNameConverter : EnumConverter
    {
        public EnumDisplayNameConverter(Type type) : base(type) { }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is Enum eVal)
            {

                if (destinationType == typeof(string))
                {
                    var fieldInfo = eVal.GetType().GetField(eVal.ToString());
                    if (fieldInfo is null)
                    {
                        return eVal.ToString();
                    }

                    if (fieldInfo.GetCustomAttribute<EnumDisplayNameAttribute>() is EnumDisplayNameAttribute attr)
                    {
                        return attr.DisplayName;
                    }
                    else
                    {
                        return eVal.ToString();
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }

    //public class EnumDisplayNameConverter<T> : TypeConverter where T : Enum
    //{
    //    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    //    {
    //        if (value is T eVal && destinationType == typeof(string))
    //        {
    //            var fieldInfo = typeof(T).GetField(eVal.ToString());
    //            if (fieldInfo.GetCustomAttribute<EnumDisplayNameAttribute>() is EnumDisplayNameAttribute attr)
    //            {
    //                return attr.DisplayName;
    //            }
    //            else
    //            {
    //                return eVal.ToString();
    //            }
    //        }
    //        else
    //        {
    //            throw new NotSupportedException();
    //        }
    //    }
    //}

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : Attribute
    {
        public EnumDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }
    }
}

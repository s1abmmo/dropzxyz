using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace main
{
    public class Setting
    {
        public ProxyType Proxytype { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool HidePopup { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string UserAgent { get; set; }
        public bool Visible { get; set; }
        public int DelayClosePopup { get; set; }
    }
    public enum ProxyType
    {
        none,
        socks5,
        https
    }
    public enum ScriptType
    {
        claimtoolcommand,
        csscript
    }
    public class AutoScript
    {
        public ScriptType ScriptType{ get; set; }
        public string Script { get; set; }
        public string ScriptName { get; set; }
    }
    public class StringSettingConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = JsonConverting.JsonConverting.EncodeJsonBeauty(value);
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Setting setting= JsonConverting.JsonConverting.DecodeJsonSetting(value as string);
            return setting;
        }
    }
    public class ScriptParameters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = JsonConverting.JsonConverting.EncodeJsonBeauty(value);
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object obj = JsonConverting.JsonConverting.DecodeJson(value as string);
            return obj;
        }
    }
    public class ScriptParametersConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = JsonConverting.JsonConverting.EncodeJsonBeauty(value);
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object obj = JsonConverting.JsonConverting.DecodeJson(value as string);
            return obj;
        }
    }
}

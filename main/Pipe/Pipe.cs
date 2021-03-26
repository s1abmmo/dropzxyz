using System;
using System.Text;
using System.IO.Pipes;
using System.Text.RegularExpressions;

namespace main.Pipe
{
    public class Pipe
    {
        public static bool PipeIsBusy;
        public string SendAndReceive(NamedPipeServerStream sender, string content)
        {
            string result = null;
            if (sender.IsConnected)
            {
                try
                {
                    var data = GetBytes(content);
                    sender.Write(data, 0, data.Length);
                    result = Read(sender);
                }
                catch { }
            }
            return result;
        }
        public void Disconnect(NamedPipeServerStream sender)
        {
            try
            {
                sender.Disconnect();
                sender.Close();
                sender.Dispose();
            }
            catch { }
        }
        public static byte[] GetBytes(string str)
        {
            str = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public string Read(NamedPipeServerStream sender)
        {
            var buffer = new byte[1000];
            if (sender.IsConnected)
                sender.Read(buffer, 0, 1000);
            return GetString(buffer);
        }
        public string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            string base64 = Regex.Replace(new string(chars), @"[^a-zA-Z0-9\+\/=]+", string.Empty);
            string result = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            return result;
        }
    }
}

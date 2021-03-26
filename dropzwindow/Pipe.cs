using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO.Pipes;

namespace dropzwindow
{
    public class Pipe
    {
        public string Read(NamedPipeClientStream client)
        {
            var buffer = new byte[1000];
            client.Read(buffer, 0, 1000);
            return GetString(buffer);
        }
        public void Response(NamedPipeClientStream sender, string content)
        {
            var data = GetBytes(content);
            sender.Write(data, 0, data.Length);
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
        private byte[] GetBytes(string str)
        {
            str = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
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

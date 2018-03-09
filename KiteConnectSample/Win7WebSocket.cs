using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KiteConnect;
using WebSocketSharp;

namespace KiteConnectSample
{
    class Win7WebSocket : IWebSocket
    {
        public event OnConnectHandler OnConnect;
        public event OnCloseHandler OnClose;
        public event OnDataHandler OnData;
        public event OnErrorHandler OnError;

        WebSocket _ws;

        public void Close(bool Abort = false)
        {
            if (Abort)
            {
                _ws.CloseAsync(CloseStatusCode.Abnormal);
            }
            else
            {
                _ws.Close(CloseStatusCode.Normal);
            }            
        }

        public void Connect(String url, Dictionary<string, string> headers = null)
        {
            // Console.WriteLine("Connect " + url + Utils.JsonSerialize(headers));
            _ws = new WebSocket(url);
            _ws.CustomHeaders = headers;
            _ws.ConnectAsync();
            _ws.OnMessage += (sender, e) =>
            {
                // Console.WriteLine("OnMessage");
                if (e.IsText)
                {
                    OnData?.Invoke(e.RawData, e.RawData.Length, "Text");
                }
                else if (e.IsBinary)
                {
                    OnData?.Invoke(e.RawData, e.RawData.Length, "Binary");
                }
            };
            _ws.OnClose += (sender, e) => 
            {
                // Console.WriteLine("OnClose");
                OnClose?.Invoke();
            };
            _ws.OnError += (sender, e) =>
            {
                // Console.WriteLine("OnError");
                OnError?.Invoke(e.Message);
            };
            _ws.OnOpen += (sender, e) =>
            {
                // Console.WriteLine("OnOpen");
                OnConnect?.Invoke();
            };
        }

        public bool IsConnected()
        {
            if (_ws != null)
            {
                return _ws.IsAlive;
            }
            return false;
        }

        public void Send(string Message)
        {
            _ws.Send(Message);
        }
    }
}

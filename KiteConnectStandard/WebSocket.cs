using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Net;

namespace KiteConnect
{
    // Delegates for events
    public delegate void OnConnectHandler();
    public delegate void OnCloseHandler();
    public delegate void OnErrorHandler(string Message);
    public delegate void OnDataHandler(byte[] Data, int Count, string MessageType);

    /// <summary>
    /// A wrapper for .Net's ClientWebSocket with callbacks
    /// </summary>
    internal class WebSocket : IWebSocket
    {
        // Instance of built in ClientWebSocket
        ClientWebSocket _ws;
        string _url;
        int _bufferLength; // Length of buffer to keep binary chunk

        // Events that can be subscribed
        public event OnConnectHandler OnConnect;
        public event OnCloseHandler OnClose;
        public event OnDataHandler OnData;
        public event OnErrorHandler OnError;

        /// <summary>
        /// Initialize WebSocket class
        /// </summary>
        /// <param name="BufferLength">Size of buffer to keep byte stream chunk.</param>
        public WebSocket(int BufferLength = 2000000)
        {            
            _bufferLength = BufferLength;
        }

        /// <summary>
        /// Check if WebSocket is connected or not
        /// </summary>
        /// <returns>True if connection is live</returns>
        public bool IsConnected()
        {
            if(_ws is null)
                return false;
            
            return _ws.State == WebSocketState.Open;
        }

        /// <summary>
        /// Connect to WebSocket
        /// </summary>
        public void Connect(string Url, Dictionary<string, string> headers = null)
        {
            _url = Url;
            try
            {
                // Initialize ClientWebSocket instance and connect with Url
                _ws = new ClientWebSocket();
                if(headers != null)
                {
                    foreach(string key in headers.Keys)
                    {
                        _ws.Options.SetRequestHeader(key, headers[key]);
                    }
                }
                _ws.ConnectAsync(new Uri(_url), CancellationToken.None).Wait();
            }
            catch (AggregateException e)
            {
                foreach (string ie in e.InnerException.Messages())
                {
                    OnError?.Invoke("Error while connecting. Message: " + ie);
                    if(ie.Contains("Forbidden") && ie.Contains("403"))
                    {
                        OnClose?.Invoke();
                    }
                }
                return;
            }
            catch (Exception e)
            {
                OnError?.Invoke("Error while connecting. Message:  " + e.Message);
                return;
            }
            OnConnect?.Invoke();

            byte[] buffer = new byte[_bufferLength];
            Action<Task<WebSocketReceiveResult>> callback = null;

            try
            {
               //Callback for receiving data
               callback = t =>
                {
                    try
                    {
                        byte[] tempBuff = new byte[_bufferLength];
                        int offset = t.Result.Count;
                        bool endOfMessage = t.Result.EndOfMessage;
                        // if chunk has even more data yet to recieve do that synchronously
                        while (!endOfMessage)
                        {
                            WebSocketReceiveResult result = _ws.ReceiveAsync(new ArraySegment<byte>(tempBuff), CancellationToken.None).Result;
                            Array.Copy(tempBuff, 0, buffer, offset, result.Count);
                            offset += result.Count;
                            endOfMessage = result.EndOfMessage;
                        }
                        // send data to process
                        OnData?.Invoke(buffer, offset, t.Result.MessageType.ToString());
                        // Again try to receive data
                        _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ContinueWith(callback);
                    }catch(Exception e)
                    {
                        if(IsConnected())
                            OnError?.Invoke("Error while recieving data. Message:  " + e.Message);
                        else
                            OnError?.Invoke("Lost ticker connection.");
                    }
                };

                // To start the receive loop in the beginning
                _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ContinueWith(callback);
            }
            catch (Exception e)
            {
                OnError?.Invoke("Error while recieving data. Message:  " + e.Message);
            }
        }

        /// <summary>
        /// Send message to socket connection
        /// </summary>
        /// <param name="Message">Message to send</param>
        public void Send(string Message)
        {
            if (_ws.State == WebSocketState.Open)
                try
                {
                    _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(Message)), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                }
                catch (Exception e)
                {
                    OnError?.Invoke("Error while sending data. Message:  " + e.Message);
                }
        }

        /// <summary>
        /// Close the WebSocket connection
        /// </summary>
        /// <param name="Abort">If true WebSocket will not send 'Close' signal to server. Used when connection is disconnected due to netork issues.</param>
        public void Close(bool Abort = false)
        {
            if(_ws.State == WebSocketState.Open)
            {
                try
                {
                    if (Abort)
                        _ws.Abort();
                    else
                    {
                        _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait();
                        OnClose?.Invoke();
                    }
                }
                catch (Exception e)
                {
                    OnError?.Invoke("Error while closing connection. Message: " + e.Message);
                }
            }
        }
    }
}

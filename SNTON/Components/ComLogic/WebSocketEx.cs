using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SNTON.Com
{
    public class WebSocketEx : WebSocketBehavior
    {
        public string msg;

        //public static event Action<string> MessageEvent;
        //信息往来事件
        protected override void OnMessage(MessageEventArgs e)
        {
            
        }

        //关闭服务事件
        protected override void OnClose(CloseEventArgs e)
        {
            //MessageBox.Show(e.Reason);
        }
    }
}

using SNTON.Com;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VI.MFC;
using VI.MFC.COM;
using VI.MFC.Logic;
using VI.MFC.Utils;
using VI.MFC.Utils.ConfigBinder;
using VI.MFC.Utils.ProcessQueue;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using System.IO;

namespace SNTON.Components.ComLogic
{
    public class PropelWebSocketLogic : VILogic
    {
        private VIThreadEx thread_Socket;
        WebSocketServer AGVstatus_socket;
        WebSocketServer Storage_socket1;
        WebSocketServer Storage_socket2;
        WebSocketServer Storage_socket3;
        WebSocketServer Storage_socket4;
        protected PropelWebSocketLogic(ProcessQueueHandler<WorkItem> handler) : base(handler)
        {

        }
        public PropelWebSocketLogic() : base(null)
        {
            thread_Socket = new VIThreadEx(Send, null, "send Client Socket message", 5000);
        }
        /// <summary>
        /// The Business Logic Component responsible for giving us the data we need.
        /// </summary>
        [ConfigBoundProperty("BusinessLogicId")]
#pragma warning disable 649
        private string businessLogicId;
#pragma warning restore 649
        private BusinessLogic.BusinessLogic businessLogic;

        public BusinessLogic.BusinessLogic BusinessLogic
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref businessLogic, businessLogicId);
                return businessLogic;
            }
        }
        [ConfigBoundProperty("AGVStatusUrl")]
        public string AGVStatusUrl { get; set; }
        [ConfigBoundProperty("StorageInfoUrl1")]
        public string StorageInfoUrl1 { get; set; }
        [ConfigBoundProperty("StorageInfoUrl2")]
        public string StorageInfoUrl2 { get; set; }
        [ConfigBoundProperty("StorageInfoUrl3")]
        public string StorageInfoUrl3 { get; set; }
        [ConfigBoundProperty("StorageInfoUrl4")]
        public string StorageInfoUrl4 { get; set; }
        public new static PropelWebSocketLogic Create(XmlNode node)
        {
            PropelWebSocketLogic a = new PropelWebSocketLogic();
            a.Init(node);
            a.AGVstatus_socket = new WebSocketServer(a.AGVStatusUrl);
            a.AGVstatus_socket.AddWebSocketService<WebSocketEx>("/WebSocketEx");
            a.Storage_socket1 = new WebSocketServer(a.StorageInfoUrl1);
            a.Storage_socket1.AddWebSocketService<WebSocketEx>("/WebSocketEx1");
            a.Storage_socket2 = new WebSocketServer(a.StorageInfoUrl2);
            a.Storage_socket2.AddWebSocketService<WebSocketEx>("/WebSocketEx2");
            a.Storage_socket3 = new WebSocketServer(a.StorageInfoUrl3);
            a.Storage_socket3.AddWebSocketService<WebSocketEx>("/WebSocketEx3");
            a.Storage_socket4 = new WebSocketServer(a.StorageInfoUrl4);
            a.Storage_socket4.AddWebSocketService<WebSocketEx>("/WebSocketEx4");
            //a.AGVstatus_socket.cl
            a.AGVstatus_socket.Start();
            a.Storage_socket1.Start();
            a.Storage_socket2.Start();
            a.Storage_socket3.Start();
            a.Storage_socket4.Start();

            return a;
        }
        protected override bool ProcessingCallHandler(WorkItem item)
        {
            return base.ProcessingCallHandler(item);
        }
        protected override void StartInternal()
        {
            thread_Socket.Start();
            base.StartInternal();
        }
        void Send()
        {//广播暂存库信息 ,AGV坐标状态和AGV状态
            string agvroute = JsonConvert.SerializeObject(this.BusinessLogic.RealTimeAGVRoute());
            string midinfo1 = JsonConvert.SerializeObject(this.BusinessLogic.GetMidStorageInfo(1));
            string midinfo2 = JsonConvert.SerializeObject(this.BusinessLogic.GetMidStorageInfo(2));
            string midinfo3 = JsonConvert.SerializeObject(this.BusinessLogic.GetMidStorageInfo(3));
            string midinfo4 = JsonConvert.SerializeObject(this.BusinessLogic.GetMidStorageInfo(4));
            Storage_socket1.WebSocketServices["/WebSocketEx1"].Sessions.Broadcast(midinfo1);
            Storage_socket2.WebSocketServices["/WebSocketEx2"].Sessions.Broadcast(midinfo2);
            Storage_socket3.WebSocketServices["/WebSocketEx3"].Sessions.Broadcast(midinfo3);
            Storage_socket4.WebSocketServices["/WebSocketEx4"].Sessions.Broadcast(midinfo4);
            //json = null;// File.ReadAllText("‪D:\\新建文本文档.txt", Encoding.UTF8);
            //string json = "{\"data\":[{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":1,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":2,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":3,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":4,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":5,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":6,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":7,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":8,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":9,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":10,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":11,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":12,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":13,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":14,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":15,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":16,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":17,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":18,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":19,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":20,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":21,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":22,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":23,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":24,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":25,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":26,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":27,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":28,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":29,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0},{\"x\":0.0,\"y\":0.0,\"TaskType\":0,\"agvid\":30,\"id\":0,\"CreateTime\":\"2018 - 12 - 05T14: 32:44.2637079 + 08:00\",\"Status\":0,\"agv_id\":0,\"fac_x\":0.0,\"fac_y\":0.0}],\"Error\":null}";
            AGVstatus_socket.WebSocketServices["/WebSocketEx"].Sessions.Broadcast(agvroute);
            //AGVstatus_socket.WebSocketServices["/WebSocketEx"].Sessions.Broadcast("会议内容：科捷方否验收的问题进行了友好协商。新智远方就可视化管理系统设备能否验收的问题进行了友好协商。与会议内容：科捷方与新智远方就可视化管理系统设备能否验收的问题进行了友好协商。会议内容：科捷方与新智远方就可视化管理系统设备能否验收的问题进行了友好协商。会议内容：科捷方与新智远方就可视化管理系统设备能否验收的问题进行了友好协商。会议内容：科捷方与新智远方就可视化管理系统设备能否验收的问题进行了友好协商。会议内容：科捷方与新智远方就可视化管理系统设备能否验收的问题进行了友好协商。会议内容：科捷方与新智远方就可视化管理系统设备能否验收的问题进行了友好协商。会议内容：科捷方与新智远方就可视化管理系统设备能否验收的问题进行了友好协商。会议内容：科捷方与新智远方就可视化管理系统设备能否验收的问题进行了友好协商。会议内容：科捷方与新智远方就可视化管理系统设备能");
        }
    }
}

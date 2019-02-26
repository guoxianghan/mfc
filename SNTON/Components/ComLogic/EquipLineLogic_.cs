using SNTON.Constants; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VI.MFC.COM;
using VI.MFC.Utils;
using VI.MFC.Utils.ConfigBinder;
using static SNTON.Constants.SNTONConstants;
using VI.MFC.Logic;
using VI.MFC.Logging;
using Newtonsoft.Json; 
using System.Xml;
using System.Collections; 
using System.Threading;
using System.Diagnostics;

namespace SNTON.Components.ComLogic
{
    /// <summary>
    /// 分区用的Logic
    /// 车间设备线体:0无任务,1需要送轮子,2
    /// </summary>
    public class EquipLineLogic_ : ComLogic
    {
        //private VIThreadEx threadequiptask;
        private VIThreadEx thread_ReadEquipLineStatus;
        private VIThreadEx thread_SendCreateAGV;
        private VIThreadEx thread_heartbeat;

        public EquipLineLogic_()
        {;
            thread_ReadEquipLineStatus = new VIThreadEx(ReadEquipLine, null, "Check EquipLine Status", 3000);
            thread_SendCreateAGV = new VIThreadEx(SendCreateAGV, null, "thread for SendCreateAGV", 3000);
            thread_heartbeat = new VIThreadEx(heartbeat, null, "heartbeat", 1000);
            this.SubscribeEvent("Heart", (x,y)=> {
                if (x)
                {

                }
                else
                {

                }
            });
        }
        protected override void StartInternal()
        {
            //thread_plctest.Start();
            //thread_ReadEquipLineStatus.Start();
            //thread_SendCreateAGV.Start();
            thread_heartbeat.Start();
            base.StartInternal();
        }
        public new static EquipLineLogic_ Create(XmlNode node)
        {
            EquipLineLogic_ a = new EquipLineLogic_();
            a.Init(node);
            return a;
        } 
        List<string> machcodes = new List<string>();
        List<string> _warningMessageList = new List<string>();

        void SendCreateAGV()
        {
            Stopwatch watch = Stopwatch.StartNew();//创建一个监听器
            watch.Start();
            logger.InfoMethod($"开始写光电 {this.PLCNo}");
            
            logger.InfoMethod($"结束写光电 {this.PLCNo} ,{ watch.ElapsedMilliseconds } 毫秒");
        }
        /// <summary>


        [ConfigBoundProperty("PLCNo")]
        public int PLCNo = 0;

        public override void OnConnect()
        {
            base.OnConnect();
        }
        protected override bool ProcessingCallHandler(WorkItem item)
        {
            bool processed = true;
            try
            {
                if (item.data.RawPacket != null)
                {
                    string theMessage = System.Text.UTF8Encoding.UTF8.GetString(item.data.RawPacket);
                    processed = ProcessMessage(item.data);
                }
                else
                {
                    logger.ErrorMethod("Neutrino to process did not contain any RawPacket Data.");
                }
                DateTime noDb = DateTime.UtcNow;

                item.data.TimeCallingTheDatabase = noDb;
                //int ret = 1;// Execute(item);
                item.data.TimeBackFromTheDatabase = noDb;
                item.data.TimeFinishedProcessing = DateTime.UtcNow;
                //logger.InfoMethod(string.Format("Processing took {0}", ret)); 
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Error", e);
                return true;
            }
            return true;
        }
        int BACKUP3 = 0;
        void heartbeat()
        {
            
            BACKUP3 = BACKUP3 == 0 ? 1 : 0;
            //this.
            var result =this.ReadData("Heart");
            
        }
        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        /// <param name="neutrino"></param>
        /// <returns></returns>
        protected override bool ProcessMessage(Neutrino neutrino)
        {
            return base.ProcessMessage(neutrino);
        }


        /// <summary>
        /// 定时读取设备线体的状态,判断送接轮子
        /// </summary>
        void ReadEquipLineStatus()
        {
           


        }

        void ReadEquipLine()
        {
            Stopwatch watch = Stopwatch.StartNew();//创建一个监听器
            watch.Start();
            
            logger.InfoMethod($"结束读取地面滚筒请求 {this.PLCNo} ,{ watch.ElapsedMilliseconds } 毫秒");
            watch.Restart();
        }
         

        /// <summary>
        /// 读写地面滚筒请求
        /// </summary>
        void RunEquipLine()
        {
            Stopwatch watch = Stopwatch.StartNew();//创建一个监听器
            watch.Start();
            try
            {
                logger.InfoMethod($"开始读取地面滚筒请求 {this.PLCNo}");
                //ReadEquipLineStatus();
                ReadEquipLine();
                logger.InfoMethod($"结束读取地面滚筒请求 {this.PLCNo} ,{ watch.ElapsedMilliseconds } 毫秒");
                watch.Restart();
            }
            catch (Exception ex)
            {
                logger.InfoMethod($"结束读取地面滚筒请求 {this.PLCNo} ,{ watch.ElapsedMilliseconds } 毫秒");
                watch.Restart();
                logger.ErrorMethod("读取地面滚筒请求失败", ex);
            }
            try
            {
                logger.InfoMethod($"开始写光电 {this.PLCNo}");
                SendCreateAGV();
                logger.InfoMethod($"结束写光电 {this.PLCNo} ,{ watch.ElapsedMilliseconds } 毫秒");
            }
            catch (Exception ex)
            {
                logger.InfoMethod($"结束写光电 {this.PLCNo} ,{ watch.ElapsedMilliseconds } 毫秒");
                logger.ErrorMethod("写地面滚筒光电失败", ex);
            }
        }
        IEnumerable<IGrouping<string, string>> MachineSplit(List<string> machnames)
        {
            var group = machcodes.GroupBy(x => x.Substring(0, 4));
            return group;
        }
        public override void Exit()
        {
            this.thread_heartbeat.Stop(1000);
            this.thread_ReadEquipLineStatus.Stop(1000);
            this.thread_SendCreateAGV.Stop(1000);
            base.Exit();    
        }
    }


}

﻿using log4net;
using SNTON.Com;
using SNTON.Components.Parser;
using SNTON.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VI.MFC;
using VI.MFC.COM;
using VI.MFC.Components.Parser;
using VI.MFC.Logging;
using VI.MFC.Logic;
using VI.MFC.Utils.ConfigBinder;
using VI.MFC.Utils.ProcessQueue;

namespace SNTON.Components.ComLogic
{
    public class ComLogic : VILogic
    {
        protected static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public IParser mocParser;
        protected ComLogic(ProcessQueueHandler<WorkItem> handler) : base(handler)
        {

        }
        private IMXCommModule com { get; set; }
        public ComLogic() : base(null)
        {

        }
        [ConfigBoundProperty("Sequencer")]
#pragma warning disable 649
        private string sequencerproviderid;
#pragma warning restore 649
        private VI.MFC.Components.Sequencer.PersistentSequencer.FileBasedSequencer Sequencerid;
        public VI.MFC.Components.Sequencer.PersistentSequencer.FileBasedSequencer Sequencer
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref Sequencerid, sequencerproviderid);
                return Sequencerid;
            }
        }

        /// <summary>
        /// 暂存库号，1号；2号；3号暂存库
        /// </summary>
        [ConfigBoundProperty("StorageArea")]
        public int StorageArea = 0;
        /// <summary>
        /// 车间号，3号车间，4号车间
        /// </summary>
        [ConfigBoundProperty("PlantNo")]
        public byte PlantNo = 0;
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
        [ConfigBoundProperty("MXParser")]
#pragma warning disable 649
        private string mxparser;
#pragma warning restore 649
        private IMXParser mxParse;
        public IMXParser MXParser
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref mxParse, mxparser);
                return mxParse;
            }
        }
        protected virtual bool ProcessMessage(Neutrino neutrino)
        {
            return true;
        }
        /// <summary>
        /// Main functionality. All DestRequests, SortReports will end up here...
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool ProcessingCallHandler(WorkItem item)
        {

            return true;
        }
        public virtual void ReadDervice()
        { }
        public override void OnConnect()
        {
            base.OnConnect();
            if (isInitiated)
            {
                //Send(CreateNeutrino(SNTONAGVCommunicationProtocol.AGVStatusReq, null));
            }
        }
        public override void OnDisconnect()
        {
            base.OnDisconnect();
            OnConnectExecution();
        }
        public override bool Send(Neutrino sendData)
        {
            return base.Send(sendData);
        }
        public bool SendData(Neutrino data)
        {
            bool r = MXParser.SendData(data);
            return r;
        }
        public virtual void OnConnectExecution()
        {
        }
        protected override void ValidateParameters()
        {
            base.ValidateParameters();

            if (string.IsNullOrWhiteSpace(businessLogicId))
            {
                throw new ArgumentException("Please specify a valid 'BusinessLogicId'.");
            }
        }
        public override void Exit()
        {
            if (Sequencer != null)
                Sequencer.Exit();
            base.Exit();
        }
        public override bool CanHandleNeutrino(Neutrino theNeutrino)
        {
            return base.CanHandleNeutrino(theNeutrino);
        }

        /// <summary>
        /// Easy solution to write fields to be sent back to the PLC
        /// </summary>
        /// <param name="theField">Key to be written</param>
        /// <param name="theValue">value to be used</param>
        /// <param name="dataOut">Neutrino to be worked on</param>
        private void WriteField(string theField, string theValue, ref Neutrino dataOut)
        {
            int byteCount = Encoding.GetEncoding(1252).GetByteCount(theValue);
            byte[] data = new byte[byteCount];
            Array.Copy(Encoding.GetEncoding(1252).GetBytes(theValue), data, byteCount);
            dataOut.AddField(theField, data);
        }
        protected override void StartInternal()
        {
            if (Sequencer != null)
                Sequencer.Start();
            base.StartInternal();
        }
        /// <summary>
        /// 将Neutrino转换为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ne"></param>
        /// <returns></returns>
        public T ConvertToObj<T>(Neutrino ne) where T : new()
        {
            T obj = new T();
            Type t = typeof(T);
            var properties = t.GetProperties();
            foreach (var item in properties)
            {
                int i = 0;
                short s = 0;
                try
                {
                    if (item.PropertyType.Name == "String")
                        item.SetValue(obj, ne.GetField(item.Name));
                    else if (item.PropertyType.Name == "Int")
                    {
                        int.TryParse(ne.GetField(item.Name), out i);
                        item.SetValue(obj, i);
                    }
                    else if (item.PropertyType.Name == "Shortk")
                    {
                        short.TryParse(ne.GetField(item.Name), out s);
                        item.SetValue(obj, s);
                    }
                    else
                        item.SetValue(obj, ne.GetField(item.Name));

                }
                catch (Exception ex)
                {
                    logger.ErrorMethod(ex);
                }
            }

            return obj;
        }
        /// <summary>
        /// 将对象转换为Neutrino
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Neutrino ConvertToNeu<T>(T obj)
        {
            Neutrino ne = new Neutrino();
            Type t = typeof(T);
            var properties = t.GetProperties();
            foreach (var item in properties)
            {
                try
                {
                    ne.AddField(item.Name, item.GetValue(obj)?.ToString());
                }
                catch (Exception ex)
                {
                    logger.ErrorMethod(ex);
                }
            }
            return ne;
        }
    }
}

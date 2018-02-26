using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VI.MFC.Components.Parser;
using System.Xml;
using VI.MFC.COM;
using VI.MFC;
using SNTON.Com;
using VI.MFC.Utils;
using log4net;
using System.Reflection;
using VI.MFC.Utils.ConfigBinder;
using SNTON.Components.FieldsDescription;
using System.Threading;
using VI.MFC.Logging;
using System.Text.RegularExpressions;

namespace SNTON.Components.Parser
{
    public class MXParser : VIParser, IMXParser
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public new static MXParser Create(XmlNode configNode)
        {
            MXParser mxParser = new MXParser();
            mxParser.Init(configNode);

            return mxParser;
        }
        public override void Init(XmlNode configNode)
        {
            base.Init(configNode);
            //定时刷新数据检测变化
            this.sendTelegramThread = new VIThreadEx((this.SendThreadExecute), null, null, -1, 1);
            //this.processQueueHandler = new ProcessQueueHandler<Neutrino>(-1);
        }

        [ConfigBoundProperty("TelegramDescriptions")]
        private string fieldsDescriptionId = "";
        private IMXFieldsDescription mxFieldsDescription;
        protected IMXFieldsDescription FieldsDescription
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref mxFieldsDescription, fieldsDescriptionId, this);
                return mxFieldsDescription;
            }
        }

        private IMXCommModule mxCommModule;
        protected new IMXCommModule CommModule
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref mxCommModule, commModuelGlueId, this);
                return mxCommModule;
            }
        }
        protected override void AdjustTempMemory(int length)
        {
            if (length > this.maxFieldLength)
            {
                this.fieldTempContents = new byte[length];
            }
        }
        #region Begin of Interface implementation
        /// <summary>
        /// 在Parser中将相应的数据发送到设备中
        /// </summary>
        /// <param name="neu2Send"></param>
        /// <returns></returns>

        public override bool SendTelegram(Neutrino theNeutrino)
        {
            return SendData(theNeutrino);
        }
        public virtual bool SendData(Neutrino neu2Send)
        {
            bool flag = false;
            try
            {
                neu2Send.TimePutIntoSendQueue = DateTime.UtcNow;
                //neu2Send.WriteField("SEQUENCE", "");
                this.telegramsToSend.Enqueue(neu2Send);
                flag = true;
                this.sendTelegramThread.Release();
            }
            catch (Exception exception)
            {
                logger.ErrorMethod("SendDeviceData exception", exception, "SendDeviceData", 0x1b0);
            }
            return flag;

        }
        #endregion End of Interface implementation
        #region Begin of Override method
        protected override void SendThreadExecute()
        {
            try
            {
                Neutrino neutrino;
                if (this.telegramsToSend.TryDequeue(out neutrino))
                {
                    List<MXDataBlock> list = new List<MXDataBlock>();
                    foreach (var item in neutrino.GetAllKeys())
                    {
                        MXField mx = null;

                        mx = this.FieldsDescription.GetMXField(item);
                        switch (mx.Type)
                        {
                            case "Int":
                                if (string.IsNullOrEmpty(neutrino.GetField(item)))
                                {
                                    logger.ErrorMethod(string.Format("Field {0} is empty value", mx.Name));
                                }
                                short mxValue = Convert.ToInt16(neutrino.GetIntOrDefault(item));
                                //long v= mxvalue >> 3;
                                //对mxvalue进行位移操作
                                if (!string.IsNullOrEmpty(mx.Split))
                                {
                                    MatchCollection mcoll = Regex.Matches(mx.Name, mx.Split);
                                    for (int i = 0; i < mcoll.Count; i++)
                                    {
                                        list.Add(new MXDataBlock() { DBName = mcoll[i].Value, DBLength = SplitDBLength(mxValue, i), Type = mx.Type, Name = mx.Name });
                                    }
                                }
                                else
                                {
                                    short[] db = { mxValue };
                                    list.Add(new MXDataBlock() { DBName = mx.Value, DBLength = mx.Length, DBDataIn2Bytes = db, Type = mx.Type, Name = mx.Name });
                                }

                                break;
                            case "String":
                                string str = neutrino.GetField(item);
                                list.Add(new MXDataBlock() { DBName = mx.Value, DBLength = mx.Length, DBDataValue = str, Type = mx.Type, Name = mx.Name });
                                break;
                            default:
                                break;
                        }

                    }
                    if (neutrino != null)
                    {
                        bool issuccess = CommModule.Try2SendData(list);
                        var tmp = list.FindAll(x => x.Result != 0);//失败的请求
                        if (tmp.Count != 0)
                            logger.ErrorMethod($"有{tmp.Count}个请求失败,{Newtonsoft.Json.JsonConvert.SerializeObject(tmp)}");
                    }
                }
                int count = this.telegramsToSend.Count;
                if (count > 0)
                {
                    logger.InfoMethod($"We currently have {count} Telegrams waiting for being sent out.", null, "SendThreadExecute", 0x399);
                    this.sendTelegramThread.Release();
                }
            }
            catch (Exception exception)
            {
                logger.ErrorMethod("SendThreadExecute exception", exception, "SendThreadExecute", 0x39f);
                Thread.Sleep(100);
                this.sendTelegramThread.Release();
            }


        }
        #endregion End of override method
        #region Begin of private method
        /// <summary>
        /// 对mxvalue进行位移操作
        /// </summary>
        /// <param name="mxvalue"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private int SplitDBLength(long mxvalue, int index)
        {
            throw new NotImplementedException("该方法尚未实现");
        }
        private List<MXDataBlock> Neutrino2DataBlock(Neutrino neutrino)
        {
            List<MXDataBlock> list = new List<MXDataBlock>();
            foreach (var item in neutrino.GetAllKeys())
            {
                #region MyRegion
                MXField mx = null;
                try
                {
                    mx = this.FieldsDescription.GetMXField(item);
                    if (mx == null)
                    {
                        logger.ErrorMethod("当前" + this.telegramDescriptionsId + "没有找到" + item + "项");
                        throw new ArgumentNullException("当前" + this.telegramDescriptionsId + "没有找到" + item + "项");
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                switch (mx.Type)
                {
                    case "Int":
                        int mxvalue = neutrino.GetInt(item);
                        //long v=       mxvalue >> 3;
                        //对mxvalue进行位移操作
                        if (!string.IsNullOrEmpty(mx.Split))
                        {
                            MatchCollection mcoll = Regex.Matches(mx.Name, mx.Split);
                            for (int i = 0; i < mcoll.Count; i++)
                            {
                                list.Add(new MXDataBlock() { DBName = mcoll[i].Value, DBLength = SplitDBLength(mxvalue, i), Name = mx.Name });
                            }
                        }
                        else
                        {
                            int[] db = { mxvalue };
                            list.Add(new MXDataBlock() { DBName = mx.Value, DBLength = mx.Length, DBDataInIn4Bytes = db, Type = mx.Type, Name = mx.Name });
                        }

                        break;
                    case "String":
                        string str = neutrino.GetField(item);
                        list.Add(new MXDataBlock() { DBName = mx.Value, DBLength = mx.Length, DBDataValue = str, Type = mx.Type, Name = mx.Name });
                        break;
                    default:
                        break;
                }
                #endregion
            }
            return list;
        }
        #endregion End of private method
        #region Begin of IMXParser interface
        public Neutrino ReadData(Neutrino neutrino)
        {
            List<MXDataBlock> list = new List<MXDataBlock>();
            #region Comment part
            //foreach (var item in neutrino.GetAllKeys())
            //{
            //    #region MyRegion
            //    var mx = this.FieldsDescription.GetMXField(item);
            //    switch (mx.Type)
            //    {
            //        case "Int":
            //            int mxvalue = neutrino.GetInt(item);
            //            //long v=       mxvalue >> 3;
            //            //对mxvalue进行位移操作
            //            if (!string.IsNullOrEmpty(mx.Split))
            //            {
            //                MatchCollection mcoll = Regex.Matches(mx.Name, mx.Split);
            //                for (int i = 0; i < mcoll.Count; i++)
            //                {
            //                    list.Add(new MXDataBlock() { DBName = mcoll[i].Value, DBLength = SplitDBLength(mxvalue, i), Name = mx.Name });
            //                }
            //            }
            //            else
            //            {
            //                int[] db = { mxvalue };
            //                list.Add(new MXDataBlock() { DBName = mx.Value, DBLength = mx.Length, DBDataInIn4Bytes = db, Type = mx.Type, Name = mx.Name });
            //            }

            //            break;
            //        case "String":
            //            string str = neutrino.GetField(item);
            //            list.Add(new MXDataBlock() { DBName = mx.Value, DBLength = mx.Length, DBDataValue = str, Type = mx.Type, Name = mx.Name });
            //            break;
            //        default:
            //            break;
            //    }
            //    #endregion
            //}
            #endregion
            var neu = CommModule.Try2ReadData(list);

            neu.TheName = neutrino.TheName;
            return neu;
        }
        public bool SendData(Neutrino neu2Send, short maxSendCount = 1)
        {
            var dataBlockList = Neutrino2DataBlock(neu2Send);
            var n = CommModule.Try2SendData(dataBlockList, maxSendCount);
            return n;
        }
        public Tuple<bool, Neutrino> ReadData(Neutrino neu2Read, bool withReadResultSign = true, short maxReadCount = 1)
        {
            var datablock = Neutrino2DataBlock(neu2Read);
            if (withReadResultSign)
            {
                return CommModule.Try2ReadDataWithSign(datablock, maxReadCount);
            }
            else
            {
                return new Tuple<bool, Neutrino>(true, CommModule.Try2ReadData(datablock, maxReadCount));
            }
        }
        #endregion End of IMXParser interface
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Constants
{
    public class SNTONAGVCommunicationProtocol
    {
        /// <summary>
        /// 发送心跳电文监听通讯状态
        /// </summary>
        public const string AliveReq = "AliveReq";
        /// <summary>
        /// AGV 系统回复上位机调度系统的心跳电文请求
        /// </summary>
        public const string AliveAck = "AliveAck";
        /// <summary>
        /// 上位机调度系统请求所有 AGV 小车当前的状态
        /// </summary>
        public const string AGVStatusReq = "AGVStatusReq";
        /// <summary>
        /// AGV 系统上传所有 AGV 小车的状态至上位机调度系统
        /// </summary>
        public const string AGVStatus = "AGVStatus";
        /// <summary>
        /// AGV 调度系统上传单个 AGV 小车的运行信息至上位机调度系统
        /// </summary>
        public const string AGVInfo = "AGVInfo";
        /// <summary>
        /// 上位机调度系统根据现场设备的指令调度 AGV 执行相应的运料，送料和拉空轮等指令
        /// </summary>
        public const string HLCallCmd = "HLCallCmd";
        /// <summary>
        /// AGV 系统对来自上位机调度系统的调度指令进行分析，给出分析结果
        /// </summary>
        public const string HLCallCmdAck = "HLCallCmdAck";
        /// <summary>
        /// AGV 系统通知上位机调度系统关于调度指令的运行情况
        /// </summary>
        public const string AGVCmdExe = "AGVCmdExe";
        /// <summary>
        /// 调度软件对 AGV 系统指令执行通知进行反馈
        /// </summary>
        public const string AGVCmdExeAck = "AGVCmdExeAck";
        /// <summary>
        /// AGV 系统将上位机调度系统下发的指令执行最终报告上传给上位机调度系统
        /// </summary>
        public const string AGVCmdExeReport = "AGVCmdExeReport";
        /// <summary>
        /// 上位机调度系统对 AGV 发送过来的指令执行最终报告进行反馈
        /// </summary>
        public const string AGVCmdExeReportAck = "AGVCmdExeReportAck";
        /// <summary>
        /// AGV 系统将小车的运动路线数据上传至上位机调度系统
        /// </summary>
        public const string AGVRoute = "AGVRoute";

        public const string TELEGRAMID = "TELEGRAMID";
        public const string SEQUENCE = "SEQUENCE";
        internal const string MsgID = "MsgID";
        internal const string DeviceName = "DeviceName";
        internal const string MsgHeaderLength = "MsgHeaderLength";
        internal const int MsgHeaderLengthValue = 54;
    }

    public class SNTONMXPLCCommunicationProtocol
    {
        /// <summary>
        /// 入库
        /// </summary>
        public const string IN_STORE = "IN_STORE";
        /// <summary>
        /// 出库
        /// </summary>
        public const string OUT_STORE = "OUT_STORE";
        /// <summary>
        /// 异常口出库
        /// </summary>
        public const string EXCEPTION_OUT_STORE = "EXCEPTION_OUT_STORE";
        /// <summary>
        /// 读取标志位,和状态位
        /// </summary>
        public const string READ_FLAG_BIT = "READ_FLAG_BIT";
        /// <summary>
        /// 读取报警信息
        /// </summary>
        public const string READ_WARNING = "READ_WARNING"; 
    }
    //MX PLC communication
    //By Song@2018.01.15
    public class MXPLCComm
    {
        //
        public const int MXPlcOK = 0;
        public const int MXPlcCAvailable = 0;
        public const int MXPlcOpened = 0;
        public const int MXPlcClosed = 0;
        public const int ReadDataSuccess = 0;
        public const int WriteDataSuccess = 0;
        public const int MaxReadSendCount = 3;
        public const int MaxSendCountUntilSuccess = -1;
    }
}

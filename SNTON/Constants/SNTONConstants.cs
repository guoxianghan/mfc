using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Constants
{
    /// <summary>
    /// static class containing SNTON related values and enums
    /// </summary>
    public static class SNTONConstants
    {
        /// <summary>
        /// 读取缓存的时间间隔
        /// </summary>
        public const int ReadingCacheInternal = 3000;
        public class Splitors
        {
            /// <summary>
            /// Id list splitors
            /// </summary>
            public const char IdsListSplitor = ',';
        }
        /// <summary>
        /// 返回按指定字符拼接的字符串
        /// </summary>
        /// <param name="a"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ToString(this Array a, char c)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in a)
            {
                sb.Append(item.ToString() + c.ToString());
            }
            return sb.ToString().Trim(c);
        }

        public static IEnumerable<IEnumerable<T>> SplitObjectList<T>(List<T> objects, int count = 0)
        {
            var obj = new List<IEnumerable<T>>();
            if (objects != null)
            {
                while (objects.Count != 0)
                {
                    obj.Add(objects.Take(count));
                    objects.RemoveRange(0, count);
                }
            }
            return obj;
        }
        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static long ToSum(this long[] a)
        {
            long sum = 0;
            foreach (var item in a)
            {
                sum += item;
            }
            return sum;
        }
        /// <summary>
        /// 求平均数
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static long ToAverage(this long[] a)
        {
            if (a == null || a.Length == 0)
                return 0;
            long sum = 0;
            foreach (var item in a)
            {
                sum += item;
            }
            long su = sum / a.Length;
            return su;
        }
        /// <summary>
        /// Deleted tag value defination
        /// </summary>
        public class DeletedTag
        {
            /// <summary>
            /// Not to be deleted
            /// </summary>
            public const int NotDeleted = 0;

            /// <summary>
            /// Should to be deleted
            /// </summary>
            public const int Deleted = 1;
        }
        public class OccupiedTag
        {
            /// <summary>
            /// 已占用
            /// </summary>
            public const int Occupied = 1;
            /// <summary>
            /// 未被占用
            /// </summary>
            public const int NotOccupied = 0;
        }

        public class IsVisible
        {
            public const int Visible = 1;
            public const int NotVisible = 0;
        }
        public class IsEnable
        {
            public const int Enable = 1;
            public const int NotEnable = 0;
        }
        public class IsOccupied
        {
            /// <summary>
            /// 被占用 有轮子
            /// </summary>
            public const int Occupied = 1;
            /// <summary>
            /// 无轮子
            /// </summary>
            public const int NotOccupied = 0;
        }
        /// <summary>
        /// Textkey languages 
        /// </summary>
        public class TextKeyLanguage
        {

            /// <summary>
            /// English language
            /// </summary>
            public const string En = "en";

            /// <summary>
            /// Simplified chinese
            /// </summary>
            public const string Cn = "cn";

        }
        /// <summary>
        /// 中间暂存库线体编号
        /// </summary>
        public class MidStoreLine
        {
            /// <summary>
            /// 异常口编号
            /// </summary>
            public const int ExceptionLine = 3;
            /// <summary>
            /// 正常出库线编号
            /// </summary>
            public const int OutStoreLine = 1;
            /// <summary>
            /// 直通线编号
            /// </summary>
            public const int InStoreLine = 2;
        }
        /// <summary>
        /// Message type
        /// </summary>
        public class MessageType
        {
            /// <summary>
            /// Debug message
            /// </summary>
            public const short Debug = 1;

            /// <summary>
            /// Trace message
            /// </summary>
            public const short Trace = 2;

            /// <summary>
            /// Information message
            /// </summary>
            public const short Info = 3;

            /// <summary>
            /// Warning message
            /// </summary>
            public const short Warning = 4;

            /// <summary>
            /// Error message
            /// </summary>
            public const short Error = 5;

            /// <summary>
            /// Serious error message
            /// </summary>
            public const short SeriousError = 6;
        }

        public class PlantNo
        {
            /// <summary>
            /// 3# plant
            /// </summary>
            public const short PlantNo3 = 3;

            /// <summary>
            /// 4# plant
            /// </summary>
            public const short PlantNo4 = 4;

            /// <summary>
            /// 5# plant
            /// </summary>
            public const short PlantNo5 = 5;
        }
        public enum AGVTaskLevel
        {
            /// <summary>
            /// 默认级别
            /// </summary>
            None = 0,
            /// <summary>
            /// 正常出库口
            /// </summary>
            Normal = 2,
            /// <summary>
            /// 异常出库
            /// </summary>
            Mid = 4,
            /// <summary>
            /// 直通线级别
            /// </summary>
            High = 6,
        }

        public enum SpoolsStatus
        {
            None = 0,
            /// <summary>
            /// 正在抓取
            /// </summary>
            Grab = 1,
            /// <summary>
            /// 抓完
            /// </summary>
            Finished = 2
        }
        [Flags]
        public enum AGVTaskStatus
        {
            /// <summary>
            /// 创建
            /// </summary>
            Created = 0,
            /// <summary>
            /// 正在执行抓轮子
            /// </summary>
            PullExecution = 1,
            /// <summary>
            /// 通知龙门准备工字轮,龙门准备好后发送通知,将状态更新为Ready
            /// </summary>
            Ready = 2,//Ready的时候生成AGVTasks的TaskNo
            /// <summary>
            /// 小车从线体接收完轮子后将状态改完Release
            /// </summary>
            Release = 3,
            /// <summary>
            /// 已发送
            /// </summary>
            Sent = 4,
            /// <summary>
            /// 收到回复
            /// </summary>
            Received = 8,
            /// <summary>
            /// 该任务正在运行，但尚未完成。
            /// </summary>
            Running = 16,
            /// <summary>
            /// 对取消进行了确认
            /// </summary>
            Canceled = 32,
            /// <summary>
            /// 由于未处理异常的原因而完成的任务
            /// </summary>
            Faulted = 64,
            /// <summary>
            /// 已成功完成执行的任务
            /// </summary>
            Finished = 128,
        }
        [Flags]
        public enum EquipTaskStatus
        {
            /// <summary>
            /// 创建
            /// </summary>
            Created = 0,
            /// <summary>
            /// 已创建AGVTask
            /// </summary>
            Ready = 2,//Ready的时候生成AGVTasks的TaskNo
            /// <summary>
            /// 由于未处理异常的原因而完成的任务
            /// </summary>
            Faulted = 4,
            /// <summary>
            /// 已成功完成执行的任务
            /// </summary>
            Finished = 8,
        }
        public const string FileTmpPath = "../MidStoreageCache";
        public class TaskConfig
        {
            /// <summary>
            /// 每个按钮控制设备的设备的数量
            /// </summary>
            public const int ControlSpools = 2;
            /// <summary>
            /// 满一车所需要的数量
            /// </summary>
            public const int AGVTaskSpoolsNum = 12;

            /// <summary>
            /// 根据产品类型获取满一车所需要的轮子数量
            /// </summary>
            /// <returns></returns>
            public static int GetAGVSpoolsCount(string producttype)
            {
                int i = 0;
                #region 根据产品类型设置一车的数量
                switch (producttype)
                {
                    case "WS18":
                    case "WS34":
                        i = 12;
                        break;
                    case "WS44":
                        i = 8;
                        break;
                    default:
                        break;
                }
                #endregion
                return i;
            }
            /// <summary>
            /// 返回满一车需要的单丝数量;
            /// 满一车需要的请料设备数量;
            /// 龙门抓取类型1大,2小,3
            /// </summary>
            /// <param name="producttype"></param>
            /// <returns></returns>
            public static Tuple<int, int, int, int, int> GetEnoughAGVEquipCount(string producttype)
            {
                int i = 0, j = 0, x = 0, l, r;
                #region 根据产品类型设置一车的数量
                switch (producttype)
                {
                    case "WS18":
                        i = 12;
                        j = 2;
                        x = 1;
                        l = 6; r = 6;
                        break;
                    case "WS34":
                        i = 12;
                        j = 2;
                        x = 2;
                        l = 6; r = 6;
                        break;
                    case "WS44":
                        i = 8; j = 2;
                        x = 3;
                        l = 4; r = 4;
                        break;
                    default:
                        i = 8; j = 2; x = 1;
                        l = 4; r = 4;
                        break;
                }
                #endregion
                return new Tuple<int, int, int, int, int>(i, j, x, l, r);
            }
            /// <summary>
            /// 根据暂存库号和线体编号获取AGV停靠站点
            /// </summary>
            /// <param name="storageno">暂存库编号</param>
            /// <param name="lineno">线体编号 正常出库线编号1,直通线2</param>
            /// <returns></returns>
            public static string AGVStation(int storageno, int lineno)
            {
                if (storageno == 1)
                {
                    if (lineno == 1)
                    {
                        return "750;751";
                    }
                    else if (lineno == 2)
                    {
                        return "740;741";
                    }
                    else
                    {
                        throw new FormatException("错误的线体编号");
                    }
                }
                else if (storageno == 2)
                {
                    if (lineno == 1)
                    {
                        return "752;753";
                    }
                    else if (lineno == 2)
                    {
                        return "742;743";
                    }
                    else
                    {
                        throw new FormatException("错误的线体编号");
                    }
                }
                else if (storageno == 3)
                {
                    if (lineno == 1)
                    {
                        return "754;755";
                    }
                    else if (lineno == 2)
                    {
                        return "744;745";
                    }
                    else
                    {
                        throw new FormatException("错误的线体编号");
                    }
                }
                else
                    throw new FormatException("错误的暂存库编号");

            }
            /// <summary>
            /// 获取轮子型号对应的线体编码 1(8个);2(12个)
            /// </summary>
            public static int GetLineCode(string producttype)
            {
                int i = 1;
                switch (producttype)
                {
                    case "WS18":
                    case "WS34":
                        i = 2;
                        break;
                    case "WS44":
                        i = 1;
                        break;
                    default:
                        break;
                }
                return i;
            }
        }
    }
}

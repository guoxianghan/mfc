using SNTON.Entities.DBTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.AGV
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class agv_three_configEntity : EntityBase
    {

        /// <summary>
        /// agv_id
        /// </summary>
        [DataMember]
        public virtual int agv_id { get; set; }

        /// <summary>
        /// sys_x
        /// </summary>
        [DataMember]
        public virtual int sys_x { get; set; }

        /// <summary>
        /// sys_y
        /// </summary>
        [DataMember]
        public virtual int sys_y { get; set; }

        /// <summary>
        /// fac_x
        /// </summary>
        [DataMember]
        public virtual float fac_x { get; set; }

        /// <summary>        
        /// 前端磁钉点X坐标,单位毫米
        /// </summary>
        public virtual double fac_X
        {
            get { return fac_x * 1000; }
        }
        /// <summary>
        /// 实时坐标与该磁钉点的X距离
        /// </summary>
        public virtual double Dev_x { get; set; }
        /// <summary>
        /// 实时坐标与该磁钉点的Y距离
        /// </summary>
        public virtual double Dev_y { get; set; }
         
        /// <summary>
        /// fac_y
        /// </summary>
        [DataMember]
        public virtual float fac_y { get; set; }

        /// <summary>
        /// 前端磁钉点Y坐标,单位毫米
        /// </summary>
        public virtual double fac_Y
        {
            get { return fac_y * 1000; }
        }
        /// <summary>
        /// x_min
        /// </summary>
        [DataMember]
        public virtual double x_min { get; set; }

        /// <summary>
        /// y_min
        /// </summary>
        [DataMember]
        public virtual double y_min { get; set; }

        /// <summary>
        /// x_max
        /// </summary>
        [DataMember]
        public virtual double x_max { get; set; }

        /// <summary>
        /// y_max
        /// </summary>
        [DataMember]
        public virtual double y_max { get; set; }


        /// <summary>
        /// RouteNo
        /// </summary>
        [DataMember]
        public virtual string RouteNo { get; set; }
        /// <summary>
        /// 计算标准偏差 
        /// </summary>
        /// <param name="arrData"></param>
        /// <returns></returns>
        public virtual double StDev
        {
            get
            {
                double[] arrData = { Dev_x, Dev_y };
                double xSum = 0F;
                double xAvg = 0F;
                double sSum = 0F;
                double tmpStDev = 0F;
                int arrNum = arrData.Length;
                for (int i = 0; i < arrNum; i++)
                {
                    xSum += arrData[i];
                }
                xAvg = xSum / arrNum;
                for (int j = 0; j < arrNum; j++)
                {
                    sSum += ((arrData[j] - xAvg) * (arrData[j] - xAvg));
                }
                tmpStDev = Convert.ToSingle(Math.Sqrt((sSum / (arrNum - 1))).ToString());
                return tmpStDev;
            }
        }
    }
}
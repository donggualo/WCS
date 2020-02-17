using ModuleManager.WCS;
using PubResourceManager;
using System.Collections.Generic;

using ADS = WcsManager.Administartor;

namespace WcsManager
{
    /// <summary>
    /// 区域特殊点位&距离
    /// </summary>
    public class AreaDistance
    {
        /// <summary>
        /// 区域
        /// </summary>
        public string area;

        /// <summary>
        /// 行车间安全距离
        /// </summary>
        public int awcsafeDis;

        /// <summary>
        /// 行车取货运输车后安全高度
        /// </summary>
        public int awctakergvDis;

        /// <summary>
        /// 行车放货运输车后安全高度
        /// </summary>
        public int awcgivergvDis;

        /// <summary>
        /// 运输车间对接距离
        /// </summary>
        public int rgvbuttDis;

        /// <summary>
        /// 运输车间安全距离
        /// </summary>
        public int rgvsafeDis;

        /// <summary>
        /// 运输车轨道中间点
        /// </summary>
        public int rgvcenterP;

        /// <summary>
        /// 运输车对接摆渡车坐标
        /// </summary>
        public int rgvbuttArfP;

        /// <summary>
        /// 摆渡车间安全距离
        /// </summary>
        public int arfsafeDis;

        /// <summary>
        /// 摆渡车待命点1
        /// </summary>
        public int arfstandbyP1;

        /// <summary>
        /// 摆渡车对接运输车坐标
        /// </summary>
        public int arfbuttRgvP;

        /// <summary>
        /// 摆渡车待命点2
        /// </summary>
        public int arfstandbyP2;
    }

    /// <summary>
    /// 区域资讯
    /// </summary>
    public class MasterDistance
    {
        /// <summary>
        /// 区域数据集
        /// </summary>
        public List<AreaDistance> distances;

        public MasterDistance()
        {
            distances = new List<AreaDistance>();
            AddAllArea();
        }

        /// <summary>
        /// 获取行车间安全距离
        /// </summary>
        public int GetAwcSafeDis(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.awcsafeDis ?? 0;
        }

        /// <summary>
        /// 行车取货运输车后安全高度
        /// </summary>
        public int GetAwcTakeRgvDis(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.awctakergvDis ?? 0;
        }

        /// <summary>
        /// 行车放货运输车后安全高度
        /// </summary>
        public int GetAwcGiveRgvDis(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.awcgivergvDis ?? 0;
        }

        /// <summary>
        /// 获取运输车间对接距离
        /// </summary>
        public int GetRgvButtDis(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.rgvbuttDis ?? 0;
        }

        /// <summary>
        /// 获取运输车间安全距离
        /// </summary>
        public int GetRgvSafeDis(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.rgvsafeDis ?? 0;
        }

        /// <summary>
        /// 获取运输车轨道中间点
        /// </summary>
        public int GetRgvCenterP(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.rgvcenterP ?? 0;
        }

        /// <summary>
        /// 获取运输车对接摆渡车坐标
        /// </summary>
        public int GetRgvButtP(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.rgvbuttArfP ?? 0;
        }

        /// <summary>
        /// 获取摆渡车间安全距离
        /// </summary>
        public int GetArfSafeDis(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.arfsafeDis ?? 0;
        }

        /// <summary>
        /// 获取摆渡车待命点1(入库车坐标)
        /// </summary>
        public int GetArfStandByP1(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.arfstandbyP1 ?? 0;
        }

        /// <summary>
        /// 获取摆渡车对接运输车坐标
        /// </summary>
        public int GetArfButtP(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.arfbuttRgvP ?? 0;
        }

        /// <summary>
        /// 获取摆渡车待命点2(出库车坐标)
        /// </summary>
        public int GetArfStandByP2(string area)
        {
            return distances.Find(c => c.area.Equals(area))?.arfstandbyP2 ?? 0;
        }

        /// <summary>
        /// 添加所有区域信息
        /// </summary>
        public void AddAllArea()
        {
            List<WCS_CONFIG_AREA> list = CommonSQL.GetAreaInfo();
            if (list == null) return;

            foreach (WCS_CONFIG_AREA a in list)
            {
                AddAreaDis(new AreaDistance()
                {
                    area = a.AREA,
                    awcsafeDis = a.AWC_DIS_SAFE,
                    awctakergvDis = a.AWC_DIS_TAKE,
                    awcgivergvDis = a.AWC_DIS_GIVE,
                    rgvbuttDis = a.RGV_DIS_BUTT,
                    rgvsafeDis = a.RGV_DIS_SAFE,
                    rgvcenterP = a.RGV_P_CENTER,
                    rgvbuttArfP = a.RGV_P_ARF,
                    arfsafeDis = a.ARF_DIS_SAFE,
                    arfbuttRgvP = a.ARF_P_RGV,
                    arfstandbyP1 = a.ARF_P_STAND1,
                    arfstandbyP2 = a.ARF_P_STAND2
                });
            }
        }

        /// <summary>
        /// 添加区域位置信息
        /// </summary>
        public void AddAreaDis(AreaDistance area)
        {
            if (!distances.Exists(c => c.area == area.area))
            {
                distances.Add(area);
            }
        }
    }
}

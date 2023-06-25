using MyService.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using WDKisStockToZKH.DTO;
using WDKisStockToZKH.Util;

namespace MyService
{
    partial class 库存同步服务 : ServiceBase
    {
        System.Timers.Timer _Timer;  //计时器
        private static object _LockSMS_Send = new object();
        DBHelper dBHelper;
        public 库存同步服务()
        {
            dBHelper = new DBHelper();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: 在此处添加代码以启动服务。
            int minute = 1;
            this._Timer = new System.Timers.Timer();
            this._Timer.Interval = minute *1000*20;  //设置计时器事件间隔执行时间
            this._Timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            this._Timer.Enabled = true;

        }

        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            this._Timer.Enabled = false;
        }
        public void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            
            //同步不成功的物料内码
             
           
            //ReportStockModle reportStockModle = new ReportStockModle()
            //{
            //    qty = 123,
            //    supplierWarehouseName = "上海仓",
            //    zkhSku = "123123"
            //};
            ////string Itemid = stockModel["Itemid"].ToString();
            //try
            //{
            //    Response result = APIHelp.PostReportStock(reportStockModle);
            //    if (result.code == 200)
            //    {
            //        //itemIdList.Add(Itemid);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Error(ex.Message, ex);
            //    throw;
            //}

            //DataTable dataTable = dBHelper.GetFillData(SQL);
            DataTable zKHStockModles = GetZKHMaterStockQty();
            foreach (DataRow stockModel in zKHStockModles.Rows)
            {
                List<string> itemIdList = new List<string>();
                LogHelper.Info("库存同步开始");
                List<ReportStockModle> reportStockModles = new List<ReportStockModle>();
                ReportStockModle reportStockModle = new ReportStockModle()
                {
                    qty = decimal.Parse(stockModel["qty"].ToString()),
                    supplierWarehouseName = "天津仓",
                    zkhSku = stockModel["zkhSku"].ToString()
                };
                reportStockModles.Add(reportStockModle);
                string FSTOCKID = stockModel["FSTOCKID"].ToString();
                string Itemid = stockModel["Itemid"].ToString();
                try
                {
                    Response result = APIHelp.PostReportStock(reportStockModles);
                    if (result.code == 200)
                    {
                        itemIdList.Add($" FSTOCKID ='{FSTOCKID}' and Fitemid='{Itemid}' ");
                        UpdateZKHNumberState(itemIdList);
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message, ex);
                    throw;
                }

            }
           
        }
        /// <summary>
        /// 获取KIS需要同步额度的即时库存
        /// </summary>
        DataTable GetZKHMaterStockQty()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(" SELECT distinct T1.FQTY QTY,t1.FITEMID  as Itemid,FMapNumber zkhSku,t1.FSTOCKID FROM MIDDLEICInventory t1  ");
            stringBuilder.Append("join ICItemMapping t2 on t1.fitemid=t2.fitemid  WHERE FSTOCKID=37593 AND  SynState=1 and T1.FQTY>0  and t2.fcompanyid  ");
            stringBuilder.Append(" in (SELECT fiTEMID FROM t_Organization WHERE FNAME in ('SKU编码(OEM)')) ");
            stringBuilder.Append(" UNION ");
            stringBuilder.Append(" SELECT distinct T1.FQTY QTY,t1.FITEMID  as Itemid,FMapNumber zkhSku,t1.FSTOCKID FROM MIDDLEICInventory t1  ");
            stringBuilder.Append(" join ICItemMapping t2 on t1.fitemid=t2.fitemid  WHERE FSTOCKID=38744 AND  SynState=1  and T1.FQTY>0  and t2.fcompanyid  ");
            stringBuilder.Append(" in (SELECT fiTEMID FROM t_Organization WHERE FNAME in ('SKU编码（直营）','SKU编码(佣金）')) ");
            //string SQL = "SELECT T1.FQTY QTY,t1.FITEMID as Itemid,FMapNumber zkhSku FROM MIDDLEICInventory t1  join ICItemMapping t2 on t1.fitemid=t2.fitemid  ";

            return dBHelper.GetFillData(stringBuilder.ToString(), null);
        }
        /// <summary>
        /// 震坤行即时库存状态修改
        /// </summary>
        void UpdateZKHNumberState(List<string> itemList)
        {
            foreach (var item in itemList)//
            {
                string Upt = $"Update MIDDLEICInventory SET SynState=0 WHERE {item}";
                string errormessage = "";
                dBHelper.GetExcuteNonQuery(Upt, out errormessage);
                if (errormessage != "OK")
                {
                    LogHelper.Error(errormessage, new Exception("修改即时库存同步状态失败"));
                }
            }
           
        }

    }
}

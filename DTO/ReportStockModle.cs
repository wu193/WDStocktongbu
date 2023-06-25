using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyService.DTO
{

    [Serializable]
    public class ReportStockModle
    {
        public decimal qty { get; set; }
        /// <summary>
        /// 上海仓
        /// </summary>
        public string supplierWarehouseName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string zkhSku { get; set; }
    }
}

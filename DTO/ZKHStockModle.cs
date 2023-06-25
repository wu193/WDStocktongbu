using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WDKisStockToZKH.DTO
{

    [Serializable]
    public class ZKHStockModle
    {
        public decimal qty { get; set; }
        /// <summary>
        /// 上海仓
        /// </summary>
        public string Itemid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string zkhSku { get; set; }
    }
}

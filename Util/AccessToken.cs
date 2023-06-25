using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WDKisStockToZKH.Util
{

    [Serializable]
    public class Response
    {
        /// <summary>
        /// 
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ResponseData data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string isSuccess { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string msg { get; set; }
    }

    [Serializable]
    public class ResponseData
    {
        /// <summary>
        /// 
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string refreshToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int tokenTtl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int refreshTokenTtl { get; set; }
    }
}

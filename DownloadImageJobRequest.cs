using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadImages
{

    public class DownloadImageJobRequest
    {
        public int Count { get; set; }
        public int ParalelJobCount { get; set; }
        public string SavePath { get; set; }
    }
}

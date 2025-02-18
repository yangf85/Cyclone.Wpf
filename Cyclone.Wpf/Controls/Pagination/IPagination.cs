using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Controls;

public interface IPagination
{
    int PageIndex { get; set; }// 当前页索引
    int PerPageCount { get; set; } // 每页数量
    int Total { get; set; } // 总数量

}

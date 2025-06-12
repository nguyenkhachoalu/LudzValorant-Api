using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.Pageds
{
    public interface IPagedList<T>
    {
        List<T> Items { get; }
        int PageNumber { get; }
        int PageSize { get; }
        int TotalItems { get; }
        int TotalPages { get; }
    }
}

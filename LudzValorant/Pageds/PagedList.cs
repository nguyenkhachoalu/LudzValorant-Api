using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.Pageds
{
    public class PagedList<T> : IPagedList<T>
    {
        public List<T> Items { get; private set; }
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int TotalItems { get; private set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

        private PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            TotalItems = count;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var totalItems = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, totalItems, pageNumber, pageSize);
        }
    }

}

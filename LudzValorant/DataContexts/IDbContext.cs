﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.DataContexts
{
    public interface IDbContext : IDisposable
    {
        DbSet<TEntity> SetEntity<TEntity>() where TEntity : class;
        Task<int> CommitChangesAsync();
    }
}

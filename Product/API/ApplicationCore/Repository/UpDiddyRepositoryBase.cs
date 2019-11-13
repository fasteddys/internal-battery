using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class UpDiddyRepositoryBase<TEntity> : IUpDiddyRepositoryBase<TEntity> where TEntity : class
    {
        private readonly UpDiddyDbContext _dbContext;

        public UpDiddyRepositoryBase(UpDiddyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<TEntity> GetAll()
        {
            return this._dbContext
                        .Set<TEntity>()
                        .AsNoTracking();
        }

        public IQueryable<TEntity> GetAllWithTracking()
        {
            return this._dbContext
                        .Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetByConditionWithTrackingAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await this._dbContext
                             .Set<TEntity>()
                             .Where(expression)
                             .ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await this._dbContext
                             .Set<TEntity>()
                             .AsNoTracking()
                             .Where(expression)
                             .ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllWithSorting(int limit, int offset, string sort, string order)
        {
            System.Reflection.PropertyInfo prop = typeof(TEntity).GetProperty(sort, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            List<TEntity> entity = new List<TEntity>();
            switch (order)
            {
                case "descending":
                    entity = await _dbContext.Set<TEntity>().OrderByDescending(x => prop.GetValue(x, null)).Take(limit).Skip(offset).ToListAsync();
                    break;
                default:
                    entity = await _dbContext.Set<TEntity>().OrderBy(x => prop.GetValue(x, null)).Take(limit).Skip(offset).ToListAsync();
                    break;
            }
            return entity;
        }


        public async Task<TEntity> GetById(int id)
        {
            return await this._dbContext
            .Set<TEntity>()
            .FindAsync(id);
        }

        public async Task ExecuteSQL(string sql)
        {
            await _dbContext.Database.ExecuteSqlCommandAsync(sql);
        }

        public async Task ExecuteSQL(string sql, object[] parameters)
        {
            await _dbContext.Database.ExecuteSqlCommandAsync(sql, parameters);
        }

        public async Task Create(TEntity entity)
        {
            await this._dbContext.Set<TEntity>().AddAsync(entity);
        }

        public async Task CreateRange(TEntity[] entity)
        {
            await this._dbContext.Set<TEntity>().AddRangeAsync(entity);
        }

        public void Update(TEntity entity)
        {
            this._dbContext.Set<TEntity>().Update(entity);
        }

        public void UpdateRange(TEntity[] entity)
        {
            this._dbContext.Set<TEntity>().UpdateRange(entity);
        }

        public void Delete(TEntity entity)
        {
            this._dbContext.Set<TEntity>().Remove(entity);
        }

        public async Task SaveAsync()
        {
            await this._dbContext.SaveChangesAsync();
        }

        public EntityEntry GetEntry(TEntity entity)
        {
            return this._dbContext.Entry(entity);
        }

        public bool HasUnsavedChanges()
        {
            return this._dbContext.ChangeTracker.Entries().Any(e => e.State == EntityState.Added
                                                      || e.State == EntityState.Modified
                                                      || e.State == EntityState.Deleted);
        }
    }
}

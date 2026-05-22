using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace chatApplication.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)  // ← ApplicationDbContext (specific)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    // ==========================================
    // OKUMA OPERASYONLARI
    // ==========================================

    /// <summary>
    /// ID'ye göre entity'yi asynchronous olarak getirir
    /// </summary>
    public async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new Exception($"GetByIdAsync hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tüm entity'leri asynchronous olarak getirir
    /// </summary>
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"GetAllAsync hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Predicate'e uygun entity'leri asynchronous olarak bulur
    /// Örnek: FindAsync(m => m.SenderId == 1)
    /// </summary>
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"FindAsync hatası: {ex.Message}", ex);
        }
    }

    // ==========================================
    // YAZMA OPERASYONLARI (ASYNC)
    // ==========================================

    /// <summary>
    /// ✅ Entity'yi database'e ekler (asynchronous)
    /// SaveChangesAsync() manuel çağrılması gerekebilir
    /// </summary>
    public async Task AddAsync(T entity)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();  // ✅ Otomatik kaydet
        }
        catch (Exception ex)
        {
            throw new Exception($"AddAsync hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// ✅ EKLENDI: Entity'yi database'de günceller (asynchronous)
    /// SaveChangesAsync() otomatik çağrılıyor
    /// </summary>
    public async Task UpdateAsync(T entity)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Entity'nin state'ini "Modified" olarak işaretle
            _context.Entry(entity).State = EntityState.Modified;
            await SaveChangesAsync();  // ✅ Otomatik kaydet
        }
        catch (Exception ex)
        {
            throw new Exception($"UpdateAsync hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// ✅ EKLENDI: Entity'yi database'den siler (asynchronous)
    /// SaveChangesAsync() otomatik çağrılıyor
    /// </summary>
    public async Task DeleteAsync(T entity)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            await SaveChangesAsync();  // ✅ Otomatik kaydet
        }
        catch (Exception ex)
        {
            throw new Exception($"DeleteAsync hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tüm değişiklikleri database'e kaydeder (asynchronous)
    /// AddAsync, UpdateAsync, DeleteAsync tarafından otomatik çağrılır
    /// </summary>
    public async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception($"Database update hatası: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"SaveChangesAsync hatası: {ex.Message}", ex);
        }
    }

    // ==========================================
    // YAZMA OPERASYONLARI (SENKRON - LEGACY)
    // ==========================================

    /// <summary>
    /// Entity'yi günceller (senkron)
    /// Tercihan UpdateAsync() kullanını
    /// </summary>
    public void Update(T entity)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception($"Update hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Entity'yi siler (senkron)
    /// Tercihan DeleteAsync() kullan
    /// </summary>
    public void Delete(T entity)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception($"Delete hatası: {ex.Message}", ex);
        }
    }
}
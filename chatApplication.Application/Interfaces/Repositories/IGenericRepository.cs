using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace chatApplication.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
    // ==========================================
    // OKUMA OPERASYONLARI
    // ==========================================

    /// <summary>
    /// ID'ye göre entity'yi getirir
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Tüm entity'leri getirir
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Predicate'e göre entity'leri bulur
    /// Örnek: FindAsync(m => m.SenderId == 1)
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    // ==========================================
    // YAZMA OPERASYONLARI (ASYNC)
    // ==========================================

    /// <summary>
    /// ✅ EKLENDI: Entity'yi database'e ekler
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// ✅ EKLENDI: Entity'yi asynchronous olarak günceller
    /// SaveChangesAsync() otomatik olarak çağrılıyor
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// ✅ EKLENDI: Entity'yi asynchronous olarak siler
    /// SaveChangesAsync() otomatik olarak çağrılıyor
    /// </summary>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Tüm değişiklikleri database'e kaydeder
    /// AddAsync, UpdateAsync, DeleteAsync otomatik çağrıyabilir
    /// veya manuel olarak çağrılabilir
    /// </summary>
    Task SaveChangesAsync();

    // ==========================================
    // YAZMA OPERASYONLARI (SENKRON - OPSİYONEL)
    // ==========================================

    /// <summary>
    /// Entity'yi günceller (senkron - legacy support)
    /// Tercihan UpdateAsync() kullan
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Entity'yi siler (senkron - legacy support)
    /// Tercihan DeleteAsync() kullan
    /// </summary>
    void Delete(T entity);
}
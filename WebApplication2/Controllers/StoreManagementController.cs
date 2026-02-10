using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Linq.Expressions;
using WebApplication2.Database;
using WebApplication2.Models;
using WebApplication2.RequestModels;
using WebApplication2.ResponseModels;

namespace WebApplication2.Controllers
{
    /*[ApiController]
    [Route("[controller]")]*/
    public class StoreManagementController : ControllerBase
    {
        private CoilDbContext _context;
        //public StoreManagementController() { }
        public StoreManagementController(CoilDbContext context)
        {
            _context = context;
        }

        public async Task<List<Coil>> Get()
        {
            return await _context.Coils
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<ActionResult<Coil>> Add(double length, double weight)
        {
            if (length <= 0)
                return BadRequest("Ошибка запроса не коректная длинна");
            if (weight <= 0)
                return BadRequest("Ошибка запроса не коректный вес");
            try
            {
                Coil coil = new Coil(length, weight, DateTime.UtcNow);
                await _context.Coils.AddAsync(coil);
                await _context.SaveChangesAsync();
                return coil;
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public async Task<ActionResult<Coil>> Delete(double id)
        {
            if (id <=0)
                return BadRequest("Ошибка запроса не коректный id");
            try
            {
                Coil? coil = await _context.Coils.FirstOrDefaultAsync(r => r.Id == id);
                if (coil == null)
                    return NotFound("id не найден");
                else
                {
                    if (coil.DateDelete != null)
                        return BadRequest($"этот рулон был удален {coil.DateDelete}");
                    else
                    {
                        coil.DateDelete = DateTime.UtcNow;
                        _context.SaveChanges();
                        return coil;
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<ActionResult<Coil>> GetCoils([FromQuery] CoilFilterRequest filter)
        {
            IQueryable<Coil> query = _context.Coils;

            // Применяем фильтры по одному диапазону
            if (filter.MinId.HasValue || filter.MaxId.HasValue)
            {
                query = ApplyRangeFilter(query, r => r.Id, filter.MinId, filter.MaxId);
            }
            if (filter.MinLength.HasValue || filter.MaxLength.HasValue)
            {
                query = ApplyRangeFilter(query, r => r.Length, filter.MinLength, filter.MaxLength);
            }
            if (filter.MinWeight.HasValue || filter.MaxWeight.HasValue)
            {
                query = ApplyRangeFilter(query, r => r.Weight, filter.MinWeight, filter.MaxWeight);
            }
            if (filter.MinDateAdd.HasValue || filter.MaxDateAdd.HasValue)
            {
                // Приводим даты к UTC для сравнения
                var minDate = filter.MinDateAdd?.ToUniversalTime();
                var maxDate = filter.MaxDateAdd?.ToUniversalTime();
                query = ApplyRangeFilter(query, r => r.DateAdd, minDate, maxDate);
            }
            if (filter.MinDateDelete.HasValue || filter.MaxDateDelete.HasValue)
            {
                var minDate = filter.MinDateDelete?.ToUniversalTime();
                var maxDate = filter.MaxDateDelete?.ToUniversalTime();
                query = ApplyRangeFilter(query, r => r.DateDelete, minDate, maxDate);
            }

            try
            {
                // Получаем общее количество для пагинации
                int totalCount = await query.CountAsync();

                // Применяем пагинацию
                var items = await query
                    .OrderByDescending(r => r.DateAdd) // Сортировка по умолчанию
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();
                return Ok(items);
            }
            catch(NpgsqlException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch(InvalidOperationException ex)
            {
                return StatusCode(520, "Ошибка");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private IQueryable<Coil> ApplyRangeFilter<T>(
            IQueryable<Coil> query,
            Expression<Func<Coil, T?>> selector,
            T? minValue,
            T? maxValue) where T : struct
        {
            if (minValue.HasValue)
            {
                var parameter = selector.Parameters[0];
                var comparison = Expression.GreaterThanOrEqual(
                    selector.Body,
                    Expression.Constant(minValue.Value, typeof(T?))
                );
                var lambda = Expression.Lambda<Func<Coil, bool>>(comparison, parameter);
                query = query.Where(lambda);
            }

            if (maxValue.HasValue)
            {
                var parameter = selector.Parameters[0];
                var comparison = Expression.LessThanOrEqual(
                    selector.Body,
                    Expression.Constant(maxValue.Value, typeof(T?))
                );
                var lambda = Expression.Lambda<Func<Coil, bool>>(comparison, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        // Перегрузка для nullable типов (например, DateDelete)
        private IQueryable<Coil> ApplyRangeFilter<T>(
            IQueryable<Coil> query,
            Expression<Func<Coil, T>> selector,
            T? minValue,
            T? maxValue) where T : class
        {
            if (minValue != null)
            {
                var parameter = selector.Parameters[0];
                var comparison = Expression.GreaterThanOrEqual(
                    selector.Body,
                    Expression.Constant(minValue, typeof(T))
                );
                var lambda = Expression.Lambda<Func<Coil, bool>>(comparison, parameter);
                query = query.Where(lambda);
            }

            if (maxValue != null)
            {
                var parameter = selector.Parameters[0];
                var comparison = Expression.LessThanOrEqual(
                    selector.Body,
                    Expression.Constant(maxValue, typeof(T))
                );
                var lambda = Expression.Lambda<Func<Coil, bool>>(comparison, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        public async Task<ActionResult> GetStatistics(DateTime? minDate, DateTime? maxDate)
        {
            if (!minDate.HasValue)
                return BadRequest("дата начала обязательна");
            if (!maxDate.HasValue)
                return BadRequest("дата конца обязательна");
            if (minDate > maxDate)
                return BadRequest("дата начала не может быть больше даты конца");

            try
            {
                DateTime min = minDate.Value.ToUniversalTime();
                DateTime max = maxDate.Value.ToUniversalTime();
                var stats = await CalculateStatistics(min, max);
                return Ok(stats);
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Ошибка расчета статистики" });
            }

        }

        private async Task<CoilStatsResponse> CalculateStatistics(
        DateTime startDate, DateTime endDate)
        {
            var response = new CoilStatsResponse
            {
                
                PeriodStart = startDate,
                PeriodEnd = endDate,
                GeneratedAt = DateTime.UtcNow
            };

            // Основная статистика за период
            var coilsInPeriod = await _context.Coils
                .Where(r => r.DateAdd >= startDate && r.DateAdd <= endDate)
                .ToListAsync();

            // Рулоны, которые были удалены в период
            var removedInPeriod = coilsInPeriod
                .Where(r => r.DateDelete.HasValue &&
                           r.DateDelete.Value >= startDate &&
                           r.DateDelete.Value <= endDate)
                .ToList();

            // Рулоны, которые находились на складе в любой момент периода
            var coilsPresentInPeriod = await _context.Coils
                .Where(r =>
                    (r.DateAdd <= endDate) && // Добавлены до конца периода
                    (!r.DateDelete.HasValue || r.DateDelete >= startDate) // Не удалены или удалены после начала
                )
                .ToListAsync();

            // 1. Количество добавленных рулонов
            response.CountAdd = coilsInPeriod.Count;

            // 2. Количество удалённых рулонов
            response.CountDelete = removedInPeriod.Count;

            if (coilsPresentInPeriod.Any())
            {
                // 3. Средние показатели
                response.AvLength = coilsPresentInPeriod.Average(r => r.Length);
                response.AvWeight = coilsPresentInPeriod.Average(r => r.Weight);

                // 4. Максимальные и минимальные значения
                response.MaxLength = coilsPresentInPeriod.Max(r => r.Length);
                response.MinLength = coilsPresentInPeriod.Min(r => r.Length);
                response.MaxWeight = coilsPresentInPeriod.Max(r => r.Weight);
                response.MinWeight = coilsPresentInPeriod.Min(r => r.Weight);

                // 5. Суммарный вес
                response.SumWeight = coilsPresentInPeriod.Sum(r => r.Weight);

                // 6. Промежутки между добавлением и удалением
                var removedCoils = coilsPresentInPeriod
                    .Where(r => r.DateDelete.HasValue)
                    .ToList();

                if (removedCoils.Any())
                {
                    var durations = removedCoils
                        .Select(r => (r.DateDelete.Value - r.DateAdd.Value))
                        .ToList();

                    response.MaxTimeBeforeDelete = durations.Max();
                    response.MinTimeBeforeDelete = durations.Min();
                }
            }

            // 7. Дополнительная статистика по дням
            response.DailyCoilsStats = await CalculateDailyCoilsStats(startDate, endDate);
            response.DailyWeightStats = await CalculateDailyWeightStats(startDate, endDate);

            return response;
        }

        private async Task<CoilStatsResponse.DailyStats> CalculateDailyCoilsStats(
            DateTime startDate, DateTime endDate)
        {
            // Получаем все изменения за период
            var dateRange = Enumerable.Range(0, (endDate.Date - startDate.Date).Days + 1)
                .Select(d => startDate.Date.AddDays(d))
                .ToList();

            // 1. Начальный баланс: рулоны, добавленные ДО начала периода и ещё не удалённые
            // или удалённые ПОСЛЕ начала периода
            var initialCoilsCount = await _context.Coils
                .CountAsync(r => r.DateAdd < startDate &&
                                (!r.DateDelete.HasValue || r.DateDelete >= startDate));

            // 2. Группируем добавления по дням в периоде
            var dailyAdditions = await _context.Coils
                .Where(r => r.DateAdd >= startDate && r.DateAdd <= endDate)
                .GroupBy(r => r.DateAdd.Value.Date)
                .Select(g => new { Date = g.Key, Added = g.Count() })
                .ToDictionaryAsync(a => a.Date, a => a.Added);

            // 3. Группируем удаления по дням в периоде
            var dailyRemovals = await _context.Coils
                .Where(r => r.DateDelete.HasValue &&
                            r.DateDelete >= startDate &&
                            r.DateDelete <= endDate)
                .GroupBy(r => r.DateDelete.Value.Date)
                .Select(g => new { Date = g.Key, Removed = g.Count() })
                .ToDictionaryAsync(r => r.Date, r => r.Removed);

            // 4. Рассчитываем баланс на каждый день
            var dailyBalances = new List<DailyBalance>();
            int currentBalance = initialCoilsCount;

            foreach (var day in dateRange)
            {
                // Добавляем рулоны, которые добавили в этот день
                if (dailyAdditions.TryGetValue(day, out int added))
                    currentBalance += added;

                // Убираем рулоны, которые удалили в этот день
                // Удаление происходит в конце дня, поэтому влияет на следующий день
                // Но если нужна точность на конец дня, оставляем как есть
                if (dailyRemovals.TryGetValue(day, out int removed))
                    currentBalance -= removed;

                dailyBalances.Add(new DailyBalance
                {
                    Date = day,
                    Balance = currentBalance
                });
            }

            if (!dailyBalances.Any())
                return new CoilStatsResponse.DailyStats();

            var maxDay = dailyBalances.OrderByDescending(d => d.Balance).First();
            var minDay = dailyBalances.OrderBy(d => d.Balance).First();

            return new CoilStatsResponse.DailyStats
            {
                Date = maxDay.Date,
                MaxValue = maxDay.Balance,
                DateMin = minDay.Date,
                MinValue = minDay.Balance
            };
        }
        private class DailyBalance
        {
            public DateTime Date { get; set; }
            public int Balance { get; set; }
        }

        private async Task<CoilStatsResponse.DailyStats> CalculateDailyWeightStats(
            DateTime startDate, DateTime endDate)
        {
            // Получаем все рулоны, которые были добавлены до конца периода
            var allCoils = await _context.Coils
                .Where(r => r.DateAdd <= endDate)
                .ToListAsync();

            // Создаем диапазон дат
            var dateRange = Enumerable.Range(0, (endDate.Date - startDate.Date).Days + 1)
                .Select(d => startDate.Date.AddDays(d))
                .ToList();

            // Для каждой даты считаем суммарный вес рулонов на складе
            var dailyWeights = new List<DailyWeight>();

            foreach (var day in dateRange)
            {
                // Рулон считается на складе в этот день, если:
                // 1. Он был добавлен до или в этот день
                // 2. Он либо не удалён, либо удалён после этого дня
                var CoilsOnDay = allCoils.Where(r =>
                    r.DateAdd.Value.Date <= day.Date &&
                    (!r.DateDelete.HasValue || r.DateDelete.Value.Date > day.Date)
                ).ToList();

                double totalWeight = CoilsOnDay.Sum(r => r.Weight);

                dailyWeights.Add(new DailyWeight
                {
                    Date = day,
                    TotalWeight = totalWeight
                });
            }

            if (!dailyWeights.Any())
                return new CoilStatsResponse.DailyStats();

            // Находим дни с максимальным и минимальным весом
            var maxWeightDay = dailyWeights.OrderByDescending(d => d.TotalWeight).First();
            var minWeightDay = dailyWeights.OrderBy(d => d.TotalWeight).First();

            return new CoilStatsResponse.DailyStats
            {
                Date = maxWeightDay.Date,
                MaxValue = maxWeightDay.TotalWeight,
                DateMin = minWeightDay.Date,
                MinValue = minWeightDay.TotalWeight
            };
        }

        private class DailyWeight
        {
            public DateTime Date { get; set; }
            public double TotalWeight { get; set; }
        }

        // Вспомогательный класс для SQL запроса
        private class DailyCount
        {
            public DateTime Date { get; set; }
            public int MaxCount { get; set; }
            public int MinCount { get; set; }
        }

    }
    
}

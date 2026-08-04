using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Ticketero.Application.DTOs;

namespace Ticketero.Application.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PagedRequest request)
    {
        if (request.Page < 1) request.Page = 1;
        if (request.PageSize < 1) request.PageSize = 20;
        if (request.PageSize > 100) request.PageSize = 100;

        return query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, PagedRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SortBy))
            return query;

        var param = Expression.Parameter(typeof(T), "x");
        var property = Expression.PropertyOrField(param, request.SortBy);
        var lambda = Expression.Lambda(property, param);

        var methodName = request.SortDirection?.ToLower() == "desc"
            ? "OrderByDescending"
            : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), property.Type],
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query, PagedRequest request, CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);
        var sorted = query.ApplySorting(request);
        var items = await sorted.ApplyPaging(request).ToListAsync(ct);

        return new PagedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

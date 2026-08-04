using Shouldly;
using Ticketero.Application.DTOs;
using Ticketero.Application.Extensions;

namespace Ticketero.Tests;

public class QueryableExtensionsTests
{
    private static readonly List<TestItem> Data =
    [
        new() { Id = 3, Name = "Charlie" },
        new() { Id = 1, Name = "Alpha" },
        new() { Id = 2, Name = "Bravo" },
        new() { Id = 5, Name = "Echo" },
        new() { Id = 4, Name = "Delta" }
    ];

    private IQueryable<TestItem> Query => Data.AsQueryable();

    [Fact]
    public void ApplyPaging_ShouldReturnCorrectPage()
    {
        var request = new PagedRequest { Page = 2, PageSize = 2 };
        var result = Query.ApplyPaging(request).ToList();
        result.Count.ShouldBe(2);
        result[0].Id.ShouldBe(2);
        result[1].Id.ShouldBe(5);
    }

    [Fact]
    public void ApplyPaging_ShouldClampPageToMinimumOne()
    {
        var request = new PagedRequest { Page = 0, PageSize = 10 };
        var result = Query.ApplyPaging(request).ToList();
        result.Count.ShouldBe(5);
    }

    [Fact]
    public void ApplyPaging_ShouldClampPageSizeToMinimumOne()
    {
        var request = new PagedRequest { Page = 1, PageSize = 0 };
        var result = Query.ApplyPaging(request).ToList();
        result.Count.ShouldBe(5);
    }

    [Fact]
    public void ApplyPaging_ShouldClampPageSizeToMaximumOneHundred()
    {
        var request = new PagedRequest { Page = 1, PageSize = 200 };
        var result = Query.ApplyPaging(request).ToList();
        result.Count.ShouldBe(5);
    }

    [Fact]
    public void ApplyPaging_WithValidPageSizeShouldTakeCorrectNumberOfItems()
    {
        var request = new PagedRequest { Page = 1, PageSize = 3 };
        var result = Query.ApplyPaging(request).ToList();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void ApplySorting_ShouldOrderByAscDefault()
    {
        var request = new PagedRequest { SortBy = "Id" };
        var result = Query.ApplySorting(request).ToList();
        result.Select(x => x.Id).ShouldBe([1, 2, 3, 4, 5]);
    }

    [Fact]
    public void ApplySorting_ShouldOrderByDesc()
    {
        var request = new PagedRequest { SortBy = "Id", SortDirection = "desc" };
        var result = Query.ApplySorting(request).ToList();
        result.Select(x => x.Id).ShouldBe([5, 4, 3, 2, 1]);
    }

    [Fact]
    public void ApplySorting_ShouldOrderByStringProperty()
    {
        var request = new PagedRequest { SortBy = "Name" };
        var result = Query.ApplySorting(request).ToList();
        result.Select(x => x.Name).ShouldBe(["Alpha", "Bravo", "Charlie", "Delta", "Echo"]);
    }

    [Fact]
    public void ApplySorting_ShouldReturnSameQuery_WhenSortByIsEmpty()
    {
        var request = new PagedRequest();
        var result = Query.ApplySorting(request).ToList();
        result.Count.ShouldBe(5);
    }

    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
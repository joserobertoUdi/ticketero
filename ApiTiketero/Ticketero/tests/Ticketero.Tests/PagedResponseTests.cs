using Shouldly;
using Ticketero.Application.DTOs;

namespace Ticketero.Tests;

public class PagedResponseTests
{
    [Fact]
    public void TotalPages_ShouldBeCeilingOfTotalCountDividedByPageSize()
    {
        var response = new PagedResponse<string> { TotalCount = 25, PageSize = 10 };
        response.TotalPages.ShouldBe(3);
    }

    [Fact]
    public void TotalPages_ShouldBeZero_WhenTotalCountIsZero()
    {
        var response = new PagedResponse<string> { TotalCount = 0, PageSize = 10 };
        response.TotalPages.ShouldBe(0);
    }

    [Fact]
    public void TotalPages_ShouldBeOne_WhenTotalCountEqualsPageSize()
    {
        var response = new PagedResponse<string> { TotalCount = 10, PageSize = 10 };
        response.TotalPages.ShouldBe(1);
    }

    [Fact]
    public void HasPreviousPage_ShouldBeTrue_WhenPageGreaterThanOne()
    {
        var response = new PagedResponse<string> { Page = 2, TotalCount = 30, PageSize = 10 };
        response.HasPreviousPage.ShouldBeTrue();
    }

    [Fact]
    public void HasPreviousPage_ShouldBeFalse_WhenPageIsOne()
    {
        var response = new PagedResponse<string> { Page = 1, TotalCount = 30, PageSize = 10 };
        response.HasPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public void HasNextPage_ShouldBeTrue_WhenPageIsNotLast()
    {
        var response = new PagedResponse<string> { Page = 1, TotalCount = 30, PageSize = 10 };
        response.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public void HasNextPage_ShouldBeFalse_WhenPageIsLast()
    {
        var response = new PagedResponse<string> { Page = 3, TotalCount = 30, PageSize = 10 };
        response.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public void Items_ShouldBeEmptyByDefault()
    {
        var response = new PagedResponse<int>();
        response.Items.ShouldBeEmpty();
    }

    [Fact]
    public void PagedRequest_ShouldHaveDefaultPageOfOne()
    {
        var request = new PagedRequest();
        request.Page.ShouldBe(1);
    }

    [Fact]
    public void PagedRequest_ShouldHaveDefaultPageSizeOfTwenty()
    {
        var request = new PagedRequest();
        request.PageSize.ShouldBe(0);
    }

    [Fact]
    public void PagedRequest_ShouldHaveDefaultSortDirectionAsc()
    {
        var request = new PagedRequest();
        request.SortDirection.ShouldBe("asc");
    }
}
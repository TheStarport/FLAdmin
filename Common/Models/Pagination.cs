namespace Common.Models;

public class Pagination<T>
{
	public required IReadOnlyList<T> Data { get; init; }
	public required int PageCount { get; init; }
	public required int CurrentPage { get; init; }
}

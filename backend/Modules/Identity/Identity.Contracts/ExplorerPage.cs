namespace Identity.Contracts;

public sealed record ExplorerPage(List<ExplorerSummary> Items, int TotalCount);

namespace Social.Application.BlogAuthoring;

public sealed record EditBlogDto(string Title, string Description, List<string>? Images);

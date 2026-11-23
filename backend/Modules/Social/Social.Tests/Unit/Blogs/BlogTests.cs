using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Blogs;
using Xunit;

namespace Social.Tests.Unit.Blogs;

public class BlogTests
{
    private static Blog CreateBlog() =>
        new(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", ["https://example.com/tvrdjava.jpg"]);

    private static Blog CreatePublishedBlog()
    {
        var blog = CreateBlog();
        blog.Publish();
        return blog;
    }

    private static Blog CreateArchivedBlog()
    {
        var blog = CreatePublishedBlog();
        blog.Archive();
        return blog;
    }

    [Fact]
    public void New_blog_starts_as_a_draft()
    {
        var blog = CreateBlog();

        blog.Status.Should().Be(BlogStatus.Draft);
        blog.AuthorId.Should().Be(WellKnownUsers.Explorer);
        blog.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        blog.Comments.Should().BeEmpty();
        blog.LastModifiedAt.Should().BeNull();
    }

    [Fact]
    public void New_blog_cannot_be_created_without_an_image()
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", []);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_blog_requires_a_title(string title)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, title, "Opis obilaska tvrđave.", ["https://example.com/tvrdjava.jpg"]);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_blog_requires_a_description(string description)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", description, ["https://example.com/tvrdjava.jpg"]);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_blog_keeps_a_markdown_description_exactly_as_written()
    {
        var description = "# Utisci sa tvrđave\n\n- **Kula** sa pogledom\n- *Kapija* iz 18. veka\n\n";

        var blog = new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", description, ["https://example.com/tvrdjava.jpg"]);

        blog.Description.Should().Be(description);
    }

    [Fact]
    public void Edited_blog_keeps_a_markdown_description_exactly_as_written()
    {
        var blog = CreateBlog();
        var description = "## Dopunjen opis\n\n1. **Kula** sa pogledom\n2. [Mapa](https://example.com/mapa)\n\n";

        blog.Edit("Novi utisci sa tvrđave", description, ["https://example.com/kula.jpg"]);

        blog.Description.Should().Be(description);
    }

    [Fact]
    public void Edited_blog_records_when_it_was_last_modified_and_keeps_its_creation_date()
    {
        var blog = CreatePublishedBlog();
        var createdAt = blog.CreatedAt;

        blog.Edit("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", ["https://example.com/kula.jpg"]);

        blog.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        blog.CreatedAt.Should().Be(createdAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blog_cannot_be_edited_without_a_title(string title)
    {
        var blog = CreateBlog();

        var editing = () => blog.Edit(title, "Dopunjen opis obilaska.", ["https://example.com/kula.jpg"]);

        editing.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blog_cannot_be_edited_without_a_description(string description)
    {
        var blog = CreateBlog();

        var editing = () => blog.Edit("Novi utisci sa tvrđave", description, ["https://example.com/kula.jpg"]);

        editing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Blog_cannot_be_edited_without_an_image()
    {
        var blog = CreateBlog();

        var editing = () => blog.Edit("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", []);

        editing.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blog_cannot_be_edited_with_a_blank_image_address(string imageAddress)
    {
        var blog = CreateBlog();

        var editing = () => blog.Edit("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", ["https://example.com/kula.jpg", imageAddress]);

        editing.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_blog_stores_image_addresses_without_surrounding_spaces()
    {
        var blog = new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", ["  https://example.com/tvrdjava.jpg  "]);

        blog.Images.Should().Equal("https://example.com/tvrdjava.jpg");
    }

    [Fact]
    public void Edited_blog_keeps_its_images_when_the_given_list_changes_later()
    {
        var blog = CreateBlog();
        List<string> images = ["https://example.com/kula.jpg"];
        blog.Edit("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", images);

        images.Clear();

        blog.Images.Should().Equal("https://example.com/kula.jpg");
    }

    [Fact]
    public void Draft_blog_is_published()
    {
        var blog = CreateBlog();

        blog.Publish();

        blog.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public void Published_blog_cannot_be_published_again()
    {
        var blog = CreatePublishedBlog();

        var publishing = () => blog.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_comments_on_a_published_blog()
    {
        var blog = CreatePublishedBlog();

        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        blog.Comments.Should().ContainSingle(comment =>
            comment.UserId == WellKnownUsers.Administrator && comment.Text == "Odlična tura!");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Comment_requires_text(string text)
    {
        var blog = CreatePublishedBlog();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, text);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_edits_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.");

        blog.Comments.Single().Text.Should().Be("Ipak prosečna tura.");
    }

    [Fact]
    public void User_cannot_edit_another_users_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Explorer, "Izmena tuđeg komentara.");

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_deletes_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.DeleteComment(comment.Id, WellKnownUsers.Administrator);

        blog.Comments.Should().BeEmpty();
    }

    [Fact]
    public void Draft_blog_cannot_be_archived()
    {
        var blog = CreateBlog();

        var archiving = () => blog.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_edited()
    {
        var blog = CreateArchivedBlog();

        var editing = () => blog.Edit("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", ["https://example.com/kula.jpg"]);

        editing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_published_again()
    {
        var blog = CreateArchivedBlog();

        var publishing = () => blog.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_commented_on()
    {
        var blog = CreateArchivedBlog();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        addition.Should().Throw<DomainException>();
    }
}

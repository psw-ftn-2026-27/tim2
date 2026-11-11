using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static Club CreateClub() =>
        new(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);

    [Fact]
    public void New_club_cannot_be_founded_without_an_image()
    {
        var founding = () => new Club(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", []);

        founding.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_club_requires_a_name(string name)
    {
        var founding = () => new Club(WellKnownUsers.Explorer, name, "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);

        founding.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_club_requires_a_description(string description)
    {
        var founding = () => new Club(WellKnownUsers.Explorer, "Planinari Fruške gore", description, ["https://example.com/fruska-gora.jpg"]);

        founding.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_club_name_cannot_be_longer_than_the_limit()
    {
        var founding = () => new Club(WellKnownUsers.Explorer, new string('a', Club.MaxNameLength + 1), "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);

        founding.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_club_cannot_have_a_blank_image_address()
    {
        var founding = () => new Club(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg", "   "]);

        founding.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_club_keeps_its_name_description_and_images_without_surrounding_spaces()
    {
        var club = new Club(WellKnownUsers.Explorer, "  Planinari Fruške gore  ", "  Vikend ture po stazama Fruške gore.  ", ["  https://example.com/fruska-gora.jpg  "]);

        club.Name.Should().Be("Planinari Fruške gore");
        club.Description.Should().Be("Vikend ture po stazama Fruške gore.");
        club.Images.Should().Equal("https://example.com/fruska-gora.jpg");
    }

    [Fact]
    public void Club_name_cannot_be_changed_to_one_longer_than_the_limit()
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.Explorer, new string('a', Club.MaxNameLength + 1), "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Club_cannot_be_left_without_an_image()
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", []);

        update.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Club_cannot_be_left_without_a_name(string name)
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.Explorer, name, "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);

        update.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Club_cannot_be_left_without_a_description(string description)
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.Explorer, "Planinari Fruške gore", description, ["https://example.com/fruska-gora.jpg"]);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_change_the_club()
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.SecondExplorer, "Tuđi klub", "Izmena tuđeg kluba.", ["https://example.com/tudji.jpg"]);

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Ownership_check_rejects_a_user_who_is_not_the_owner()
    {
        var club = CreateClub();

        var check = () => club.EnsureOwnedBy(WellKnownUsers.SecondExplorer);

        check.Should().Throw<NotFoundException>();
    }
}

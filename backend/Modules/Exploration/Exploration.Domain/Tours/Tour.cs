using Exploration.Domain.Locations;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class Tour : AggregateRoot
{
    private const int MinimumDescriptionLengthForPublishing = 100;
    private const int MinimumKeyPointsForPath = 2;

    private readonly List<TransportTime> _transportTimes = [];
    private readonly List<KeyPoint> _keyPoints = [];

    public Guid AuthorId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public TourDifficulty Difficulty { get; private set; }
    public List<string> Tags { get; private set; }
    public TourStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public RecommendedGroupSize? RecommendedGroupSize { get; private set; }
    public int Price { get; private set; }
    public double? LengthKm { get; private set; }
    public IReadOnlyList<TransportTime> TransportTimes => _transportTimes;
    public IReadOnlyList<KeyPoint> KeyPoints => _keyPoints;

    private Tour()
    {
        Name = null!;
        Description = null!;
        Tags = null!;
    }

    public Tour(Guid authorId, string name, string description, TourDifficulty difficulty, List<string> tags)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A tour requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A tour requires a description.");
        }
        if (tags is null || tags.Count == 0)
        {
            throw new DomainException("A tour requires at least one tag.");
        }

        AuthorId = authorId;
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Tags = tags;
        Status = TourStatus.Draft;
        LengthKm = 0;
    }

    public void AddTransportTime(TransportMode transport, int minutes)
    {
        if (_transportTimes.Any(time => time.Transport == transport))
        {
            throw new DomainException($"The tour already has a time for {transport}.");
        }

        _transportTimes.Add(new TransportTime(transport, minutes));
    }

    public void ChangeRecommendedGroupSize(int min, int max)
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("The recommended group size can be changed only while the tour is a draft.");
        }

        RecommendedGroupSize = new RecommendedGroupSize(min, max);
    }

    public void ChangePrice(int newPrice)
    {
        if (newPrice < 0)
        {
            throw new DomainException("A tour price cannot be negative.");
        }
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("The price can be changed only while the tour is a draft.");
        }

        Price = newPrice;
    }

    public void AddKeyPoint(string name, string description, string imageUrl, Location location,
        string? secret, bool isVisibleBeforePurchase)
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Key points can be added only while the tour is a draft.");
        }

        _keyPoints.Add(new KeyPoint(_keyPoints.Count + 1, name, description, imageUrl, location, secret, isVisibleBeforePurchase));
        ResetLength();
    }

    public void ChangeKeyPoint(Guid keyPointId, string name, string description, string imageUrl, Location location,
        string? secret, bool isVisibleBeforePurchase)
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Key points can be changed only while the tour is a draft.");
        }

        var keyPoint = GetKeyPoint(keyPointId);
        var locationChanged = keyPoint.Location != location;
        keyPoint.Change(name, description, imageUrl, location, secret, isVisibleBeforePurchase);
        if (locationChanged)
        {
            ResetLength();
        }
    }

    public void RemoveKeyPoint(Guid keyPointId)
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Key points can be removed only while the tour is a draft.");
        }

        var keyPoint = GetKeyPoint(keyPointId);
        _keyPoints.Remove(keyPoint);
        foreach (var following in _keyPoints.Where(point => point.Order > keyPoint.Order))
        {
            following.ChangeOrder(following.Order - 1);
        }
        ResetLength();
    }

    public void MoveKeyPoint(Guid keyPointId, KeyPointMoveDirection direction)
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Key points can be reordered only while the tour is a draft.");
        }

        var keyPoint = GetKeyPoint(keyPointId);
        var neighbourOrder = direction == KeyPointMoveDirection.Up ? keyPoint.Order - 1 : keyPoint.Order + 1;
        var neighbour = _keyPoints.FirstOrDefault(point => point.Order == neighbourOrder);
        if (neighbour is null)
        {
            throw new DomainException("A key point cannot be moved outside the list.");
        }

        neighbour.ChangeOrder(keyPoint.Order);
        keyPoint.ChangeOrder(neighbourOrder);
        ResetLength();
    }

    public void SetCalculatedLength(double lengthKm)
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("The length can be set only while the tour is a draft.");
        }
        if (_keyPoints.Count < MinimumKeyPointsForPath)
        {
            throw new DomainException($"The length can be set only for a tour with at least {MinimumKeyPointsForPath} key points.");
        }
        if (!double.IsFinite(lengthKm) || lengthKm < 0)
        {
            throw new DomainException("A tour length must be a finite number that is not negative.");
        }

        LengthKm = lengthKm;
    }

    public void Publish()
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Only a draft tour can be published.");
        }
        if (Description.Length < MinimumDescriptionLengthForPublishing)
        {
            throw new DomainException($"A tour can be published only with a description of at least {MinimumDescriptionLengthForPublishing} characters.");
        }
        if (_transportTimes.Count == 0)
        {
            throw new DomainException("A tour can be published only with at least one transport time.");
        }
        if (RecommendedGroupSize is null)
        {
            throw new DomainException("A tour can be published only with a recommended group size.");
        }
        if (_keyPoints.Count < MinimumKeyPointsForPath)
        {
            throw new DomainException($"A tour can be published only with at least {MinimumKeyPointsForPath} key points.");
        }
        if (LengthKm is null)
        {
            throw new DomainException("A tour can be published only with a calculated length.");
        }

        Status = TourStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status != TourStatus.Published)
        {
            throw new DomainException("Only a published tour can be archived.");
        }

        Status = TourStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (Status != TourStatus.Archived)
        {
            throw new DomainException("Only an archived tour can be reactivated.");
        }

        Status = TourStatus.Published;
        ArchivedAt = null;
    }

    // A changed list has no path yet, so its length waits for a new calculation; fewer than two points have no path at all.
    private void ResetLength()
    {
        LengthKm = _keyPoints.Count < MinimumKeyPointsForPath ? 0 : null;
    }

    private KeyPoint GetKeyPoint(Guid keyPointId)
    {
        var keyPoint = _keyPoints.FirstOrDefault(point => point.Id == keyPointId);
        if (keyPoint is null)
        {
            throw new NotFoundException($"Key point {keyPointId} does not exist.");
        }
        return keyPoint;
    }
}

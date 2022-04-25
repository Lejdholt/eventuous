using Eventuous.MongoDB.Tools;
using NodaTime;
using static Eventuous.Sut.Domain.BookingEvents;
using static Eventuous.Tests.MongoDB.Fixtures.IntegrationFixture;

namespace Eventuous.Tests.MongoDB.Fixtures;

public static class DomainFixture {
    static DomainFixture() => TypeMap.RegisterKnownEventTypes();

    public static BookingImported CreateImportBooking() {
        var from = Instance.Auto.Create<DateTime>();

        return new BookingImported(
            Instance.Auto.Create<string>(),
            Instance.Auto.Create<string>(),
            LocalDate.FromDateTime(from),
            LocalDate.FromDateTime(from.AddDays(Instance.Auto.Create<int>()))
        );
    }
}

record BookingDocument(string Id) : ProjectedDocument(Id) {
    public string    GuestId      { get; init; } = null!;
    public string    RoomId       { get; init; } = null!;
    public LocalDate CheckInDate  { get; init; }
    public LocalDate CheckOutDate { get; init; }
    public float     BookingPrice { get; init; }
    public float     PaidAmount   { get; init; }
    public float     Outstanding  { get; init; }
    public bool      Paid         { get; init; }
}
global using NodaTime;
using Eventuous.Tests.Fixtures;
using Eventuous.Sut.App;
using Eventuous.Sut.Domain;
using static Eventuous.Sut.Domain.BookingEvents;

namespace Eventuous.Tests;

public class StoringEventsWithCustomStream : NaiveFixture {
    public StoringEventsWithCustomStream() {
        var streamNameMap = new StreamNameMap();
        streamNameMap.Register<Booking, BookingState, BookingId>(GetStreamName);
        Service = new BookingService(AggregateStore, streamNameMap);
        TypeMap.RegisterKnownEventTypes();
    }

    BookingService Service { get; }

    [Fact]
    public async Task TestOnNew() {
        var cmd = new Commands.BookRoom(
            Auto.Create<string>(),
            Auto.Create<string>(),
            LocalDate.FromDateTime(DateTime.Today),
            LocalDate.FromDateTime(DateTime.Today.AddDays(2)),
            Auto.Create<decimal>()
        );

        var expected = new Change[] {
            new(new RoomBooked(cmd.BookingId, cmd.RoomId, cmd.CheckIn, cmd.CheckOut, cmd.Price), "RoomBooked")
        };

        var result = await Service.Handle(cmd, new Metadata(), default);

        result.Success.Should().BeTrue();
        result.Changes.Should().BeEquivalentTo(expected);

        var evt = await EventStore.ReadEvents(
            GetStreamName(new BookingId(cmd.BookingId)),
            StreamReadPosition.Start,
            1,
            CancellationToken.None
        );

        evt[0].Payload.Should().BeEquivalentTo(result.Changes!.First().Event);
    }

    [Fact]
    public async Task TestOnExisting() {
        var cmd = new Commands.BookRoom(
            Auto.Create<string>(),
            Auto.Create<string>(),
            LocalDate.FromDateTime(DateTime.Today),
            LocalDate.FromDateTime(DateTime.Today.AddDays(2)),
            Auto.Create<decimal>()
        );

        await Service.Handle(cmd, new Metadata(), default);

        var secondCmd = new Commands.RecordPayment(cmd.BookingId, Auto.Create<string>(), cmd.Price);

        var expected = new Change[] {
            new(
                new BookingPaymentRegistered(
                    secondCmd.BookingId,
                    secondCmd.PaymentId,
                    secondCmd.Amount
                ),
                "PaymentRegistered"
            ),
            new(new BookingFullyPaid(secondCmd.BookingId), "BookingFullyPaid")
        };

        var result = await Service.Handle(secondCmd, new Metadata(), default);

        result.Success.Should().BeTrue();
        result.Changes.Should().BeEquivalentTo(expected);

        var evt = await EventStore.ReadEvents(
            GetStreamName(new BookingId(cmd.BookingId)),
            StreamReadPosition.Start,
            100,
            CancellationToken.None
        );

        evt.Skip(1)
            .Select(x => x.Payload)
            .Should()
            .BeEquivalentTo(expected.Select(x => x.Event));
    }

    static StreamName GetStreamName(BookingId bookingId) => new($"hotel-booking-{bookingId}");
}

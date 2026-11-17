using CinemaGo.Application.Common;
using CinemaGo.Domain.Entities;
namespace CinemaGo.Tests;

public class EmailTemplatesTests
{
    [Fact]
    public void BookingConfirmed_ListsEveryTicketCode_TheTotal_AndEscapesHtml()
    {
        var session = TestData.NewSession();
        session.Movie.Title = "<script>alert(1)</script>";
        var bookings = new List<Booking>
        {
            new() { Session = session, Seat = TestData.NewSeat(1, vip: true), Ticket = new Ticket { TicketCode = "CG-AAA", FinalPrice = 25m } },
            new() { Session = session, Seat = TestData.NewSeat(2), Ticket = new Ticket { TicketCode = "CG-BBB", FinalPrice = 10m } }
        };

        var (_, body) = EmailTemplates.BookingConfirmed("Anna", bookings);

        Assert.Contains("CG-AAA", body);
        Assert.Contains("CG-BBB", body);
        Assert.Contains("35.00 GEL", body);
        Assert.DoesNotContain("<script>", body);
        Assert.Contains("&lt;script&gt;", body);
    }
}
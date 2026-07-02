using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Services;

public class PdfService : IPdfService
{
    public byte[] GenerateBookingTicketPdf(Booking booking)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(header => ComposeHeader(header, booking));
                page.Content().Element(content => ComposeContent(content, booking));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, Booking booking)
    {
        var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
        var appNameStyle = TextStyle.Default.FontSize(24).ExtraBold().FontColor(Colors.Blue.Darken4);

        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Tourmate").Style(appNameStyle);
                column.Item().PaddingTop(5).Text("CONFIRMED BOOKING TICKET").Style(titleStyle);
                column.Item().Text($"Reference No: {booking.BookingReference}").FontSize(14).SemiBold();
                column.Item().Text($"Booking Date: {booking.BookedAt:MMM dd, yyyy}");
            });
        });
    }

    private void ComposeContent(IContainer container, Booking booking)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(20);

            // Greetings
            column.Item().Text($"Hello {booking.User?.FullName ?? "Traveler"},").FontSize(16).SemiBold();
            column.Item().Text("Thank you for choosing Tourmate! We are thrilled to confirm your upcoming trip. Below you will find all the details regarding your travel and accommodation.").FontSize(12);

            // Package & Travel Details
            column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Travel Details").SemiBold().FontSize(14);
            column.Item().Text($"Package: {booking.Package.Title}");
            column.Item().Text($"Destination: {booking.Package.City}, {booking.Package.Country}");
            column.Item().Text($"Departure Date: {booking.TravelDate:MMM dd, yyyy}");
            column.Item().Text($"Return Date: {booking.ReturnDate:MMM dd, yyyy}");

            // Packager Details
            column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Packager Information").SemiBold().FontSize(14);
            column.Item().Text($"Agency: {booking.Package.Packager.CompanyName}");
            column.Item().Text($"Email: {booking.Package.Packager.ContactEmail ?? "N/A"}");
            column.Item().Text($"Phone: {booking.Package.Packager.ContactPhone ?? "N/A"}");

            // Traveler Details
            column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Travelers").SemiBold().FontSize(14);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Full Name").SemiBold();
                    header.Cell().Text("Gender").SemiBold();
                    header.Cell().Text("Date of Birth").SemiBold();
                });

                foreach (var traveler in booking.BookingTravelers)
                {
                    table.Cell().Text(traveler.FullName ?? "Unknown");
                    table.Cell().Text(traveler.Gender ?? "N/A");
                    table.Cell().Text(traveler.DateOfBirth?.ToString("MMM dd, yyyy") ?? "N/A");
                }
            });

            // Payment Summary
            column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Payment Summary").SemiBold().FontSize(14);
            column.Item().Text($"Total Amount Paid: ₹{booking.TotalAmount:F2}").SemiBold().FontSize(12);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column => 
        {
            column.Item().AlignCenter().Text("Have a safe and wonderful journey!").FontSize(14).Italic().FontColor(Colors.Grey.Darken2);
            column.Item().PaddingTop(10).AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        });
    }
}

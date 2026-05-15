using BusinessLayer.Interfaces;

namespace PresentationLayer.Menus;

public class FineMenu
{
    private readonly IFineService _fineService;

    public FineMenu(IFineService fineService)
    {
        _fineService = fineService;
    }

    public async Task ShowAsync()
    {
        bool back = false;
        while (!back)
        {
            ConsoleHelper.PrintHeader("FINE MANAGEMENT");
            Console.WriteLine("  1. View Pending Fine");
            Console.WriteLine("  2. Pay Fine");
            Console.WriteLine("  3. View Fine History");
            Console.WriteLine("  0. Back");
            Console.Write("\n  Choice: ");

            switch (Console.ReadLine())
            {
                case "1": await ViewPendingFineAsync(); break;
                case "2": await PayFineAsync();         break;
                case "3": await ViewFineHistoryAsync(); break;
                case "0": back = true;                  break;
                default:
                    ConsoleHelper.PrintError("Invalid choice. Please enter a number between 0-3.");
                    ConsoleHelper.Pause();
                    break;
            }
        }
    }

    private async Task ViewPendingFineAsync()
    {
        ConsoleHelper.PrintHeader("PENDING FINE");
        try
        {
            var memberId = ConsoleHelper.ReadInt("Member ID");
            var summary  = await _fineService.GetFineSummaryAsync(memberId);

            ConsoleHelper.PrintInfo($"Member     : {summary.MemberName}");
            ConsoleHelper.PrintInfo($"Total Fine : ₹{summary.TotalFine}");
            ConsoleHelper.PrintInfo($"Paid       : ₹{summary.TotalPaid}");
            ConsoleHelper.PrintInfo($"Unpaid     : ₹{summary.UnpaidFine}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task PayFineAsync()
    {
        ConsoleHelper.PrintHeader("PAY FINE");
        try
        {
            var memberId = ConsoleHelper.ReadInt("Member ID");
            var unpaid   = await _fineService.GetUnpaidFineAsync(memberId);

            ConsoleHelper.PrintInfo($"Unpaid Fine: ₹{unpaid}");

            if (unpaid <= 0)
            {
                ConsoleHelper.PrintInfo("No pending fines for this member.");
                ConsoleHelper.Pause();
                return;
            }

            var amount         = ConsoleHelper.ReadDecimal($"Amount to Pay (max ₹{unpaid})");
            var (success, msg) = await _fineService.PayFineAsync(memberId, amount);

            if (success) ConsoleHelper.PrintSuccess(msg);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task ViewFineHistoryAsync()
    {
        ConsoleHelper.PrintHeader("FINE PAYMENT HISTORY");
        try
        {
            var memberId = ConsoleHelper.ReadInt("Member ID");
            var payments = await _fineService.GetFineHistoryAsync(memberId);

            if (!payments.Any())
            {
                ConsoleHelper.PrintInfo("No payment history found for this member.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine($"\n  {"ID",-5} {"Book Title",-30} {"Amount Paid",-15} {"Payment Date",-20}");
            Console.WriteLine($"  {"─",-5} {"─",-30} {"─",-15} {"─",-20}");

            foreach (var p in payments)
                Console.WriteLine($"  {p.Id,-5} {p.BookTitle,-30} ₹{p.AmountPaid,-14} {p.PaymentDate,-20}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }
}
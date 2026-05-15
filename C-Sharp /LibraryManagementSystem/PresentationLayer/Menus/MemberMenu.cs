using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using DataAccessLayer.Enums;

namespace PresentationLayer.Menus;

public class MemberMenu
{
    private readonly IMemberService _memberService;

    public MemberMenu(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public async Task ShowAsync()
    {
        bool back = false;
        while (!back)
        {
            ConsoleHelper.PrintHeader("MEMBER MANAGEMENT");
            Console.WriteLine("  1. Add New Member");
            Console.WriteLine("  2. View All Members");
            Console.WriteLine("  3. Search by Email");
            Console.WriteLine("  4. Search by Phone");
            Console.WriteLine("  5. Update Member Status");
            Console.WriteLine("  6. Deactivate Member");
            Console.WriteLine("  0. Back");
            Console.Write("\n  Choice: ");

            switch (Console.ReadLine())
            {
                case "1": await AddMemberAsync();        break;
                case "2": await ViewAllMembersAsync();   break;
                case "3": await SearchByEmailAsync();    break;
                case "4": await SearchByPhoneAsync();    break;
                case "5": await UpdateStatusAsync();     break;
                case "6": await DeactivateMemberAsync(); break;
                case "0": back = true;                   break;
                default:
                    ConsoleHelper.PrintError("Invalid choice. Please enter a number between 0-6.");
                    ConsoleHelper.Pause();
                    break;
            }
        }
    }

    private async Task AddMemberAsync()
    {
        ConsoleHelper.PrintHeader("ADD NEW MEMBER");
        try
        {
            var name  = ConsoleHelper.ReadInput("Name");
            var phone = ConsoleHelper.ReadInput("Phone");
            var email = ConsoleHelper.ReadInput("Email");

            Console.WriteLine("\n  Membership Type:");
            Console.WriteLine("  1. Basic   (2 books, 7 days)");
            Console.WriteLine("  2. Student (3 books, 10 days)");
            Console.WriteLine("  3. Premium (5 books, 15 days)");
            var typeChoice = ConsoleHelper.ReadInput("Select type (1/2/3)");

            var membershipType = typeChoice switch
            {
                "1" => MembershipTypeEnum.Basic,
                "2" => MembershipTypeEnum.Student,
                "3" => MembershipTypeEnum.Premium,
                _   => throw new InvalidInputException("Membership Type", "Please enter 1, 2, or 3.")
            };

            var (success, message) = await _memberService.AddMemberAsync(name, phone, email, membershipType);
            if (success) ConsoleHelper.PrintSuccess(message);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task ViewAllMembersAsync()
    {
        ConsoleHelper.PrintHeader("ALL MEMBERS");
        try
        {
            var members = await _memberService.GetAllMembersAsync();

            if (!members.Any())
            {
                ConsoleHelper.PrintInfo("No members found.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine($"\n  {"ID",-5} {"Name",-20} {"Phone",-15} {"Email",-25} {"Type",-10} {"Status",-10}");
            Console.WriteLine($"  {"─",-5} {"─",-20} {"─",-15} {"─",-25} {"─",-10} {"─",-10}");

            foreach (var m in members)
                Console.WriteLine($"  {m.Id,-5} {m.Name,-20} {m.Phone,-15} {m.Email,-25} {m.MembershipType,-10} {m.Status,-10}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task SearchByEmailAsync()
    {
        ConsoleHelper.PrintHeader("SEARCH BY EMAIL");
        try
        {
            var email  = ConsoleHelper.ReadInput("Email");
            var member = await _memberService.GetMemberByEmailAsync(email);

            if (member is null) ConsoleHelper.PrintError("No member found with this email.");
            else                PrintMemberDetail(member);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task SearchByPhoneAsync()
    {
        ConsoleHelper.PrintHeader("SEARCH BY PHONE");
        try
        {
            var phone  = ConsoleHelper.ReadInput("Phone");
            var member = await _memberService.GetMemberByPhoneAsync(phone);

            if (member is null) ConsoleHelper.PrintError("No member found with this phone number.");
            else                PrintMemberDetail(member);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task UpdateStatusAsync()
    {
        ConsoleHelper.PrintHeader("UPDATE MEMBER STATUS");
        try
        {
            var id = ConsoleHelper.ReadInt("Member ID");

            Console.WriteLine("  1. Active");
            Console.WriteLine("  2. Inactive");
            var choice = ConsoleHelper.ReadInput("Select status (1/2)");

            if (choice != "1" && choice != "2")
                throw new InvalidInputException("Status", "Please enter 1 for Active or 2 for Inactive.");

            var status             = choice == "2" ? MemberStatus.Inactive : MemberStatus.Active;
            var (success, message) = await _memberService.UpdateMemberStatusAsync(id, status);

            if (success) ConsoleHelper.PrintSuccess(message);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private async Task DeactivateMemberAsync()
    {
        ConsoleHelper.PrintHeader("DEACTIVATE MEMBER");
        try
        {
            var id                 = ConsoleHelper.ReadInt("Member ID");
            var (success, message) = await _memberService.DeactivateMemberAsync(id);

            if (success) ConsoleHelper.PrintSuccess(message);
        }
        catch (Exception ex)
        {
            ConsoleHelper.HandleException(ex);
        }
        ConsoleHelper.Pause();
    }

    private static void PrintMemberDetail(BusinessLayer.DTOs.MemberDto m)
    {
        ConsoleHelper.PrintInfo($"ID            : {m.Id}");
        ConsoleHelper.PrintInfo($"Name          : {m.Name}");
        ConsoleHelper.PrintInfo($"Phone         : {m.Phone}");
        ConsoleHelper.PrintInfo($"Email         : {m.Email}");
        ConsoleHelper.PrintInfo($"Membership    : {m.MembershipType}");
        ConsoleHelper.PrintInfo($"Status        : {m.Status}");
        ConsoleHelper.PrintInfo($"Max Borrowings: {m.MaxBorrowings}");
        ConsoleHelper.PrintInfo($"Max Days      : {m.MaxBorrowDays}");
        ConsoleHelper.PrintInfo($"Joined Date   : {m.JoinedDate}");
    }
}
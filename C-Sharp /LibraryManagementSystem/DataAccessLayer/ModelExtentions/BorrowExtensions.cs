using DataAccessLayer.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models;

public partial class Borrow
{
    [NotMapped]
    public BorrowStatus BorrowStatus
    {
        get => (BorrowStatus)Status;
        set => Status = (int)value;
    }
}
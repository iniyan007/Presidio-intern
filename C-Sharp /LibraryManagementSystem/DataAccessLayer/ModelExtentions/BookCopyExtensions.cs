using DataAccessLayer.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models;

public partial class BookCopy
{
    [NotMapped]
    public CopyStatus CopyStatus
    {
        get => (CopyStatus)Status;
        set => Status = (int)value;
    }
}
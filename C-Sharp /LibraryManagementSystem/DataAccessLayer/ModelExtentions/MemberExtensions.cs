using DataAccessLayer.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models;

public partial class Member
{
    [NotMapped]
    public MemberStatus MemberStatus
    {
        get => (MemberStatus)Status;
        set => Status = (int)value;
    }
}
using DataAccessLayer.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models;

public partial class MembershipType
{
    [NotMapped]
    public MembershipTypeEnum MembershipTypeEnum
    {
        get => (MembershipTypeEnum)Type;
        set => Type = (int)value;
    }
}
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Repository;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Service;
using LibraryManagement.Models.DTOs;

namespace LibraryManagement.Service
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;
        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }
        [HttpGet]
        public ActionResult<List<Member>> GetAllMembers()
        {
            var members = _memberService.GetAllMembers();
            return Ok(members);
        }
        [HttpGet("{id}")]
        public ActionResult<Member> GetMemberById(int id)
        {
            var member = _memberService.GetMemberById(id);
            if (member == null)
                return NotFound("No member with the given id - " + id);
            return Ok(member);
        }
        [HttpPost]
        public ActionResult AddMember(CreateMemberRequest request)
        {
            try
            {
                var member = new Member
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    MembershipDate = request.MembershipDate
                };
                _memberService.AddMember(member);
                return Created("", new
                {
                    message = "Member created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
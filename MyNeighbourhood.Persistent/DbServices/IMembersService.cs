using System.Collections.Generic;
using System.Threading.Tasks;
using MyNeighbourhood.Persistent.DbObjects.Members;

namespace MyNeighbourhood.Persistent.DbServices
{
    public interface IMembersService
    {
        Task<List<Member>> GetMembers();

        Task<Member> GetMember(int memberId);

        Task<Member> GetMember(string firstname, string lastname);

        Task<Member> CreateMember(Member member);

        Task<Member> UpdateMember(int memberId, Member member);

        Task<Member> DeleteMember(int memberId);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyNeighbourhood.Persistent.DbContexts;
using MyNeighbourhood.Persistent.DbObjects.Members;

namespace MyNeighbourhood.Persistent.DbServices
{
    public class MembersService : IMembersService
    {
        public Task<List<Member>> GetMembers()
        {
            throw new NotImplementedException();
        }

        public Task<Member> GetMember(int memberId)
        {
            throw new NotImplementedException();
        }

        public Task<Member> GetMember(string firstname, string lastname)
        {
            throw new NotImplementedException();
        }

        public Task<Member> CreateMember(Member member)
        {
            throw new NotImplementedException();
        }

        public Task<Member> UpdateMember(int memberId, Member member)
        {
            throw new NotImplementedException();
        }

        public Task<Member> DeleteMember(int memberId)
        {
            throw new NotImplementedException();
        }
    }
}
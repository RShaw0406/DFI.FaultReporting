using DFI.FaultReporting.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Users
{
    public interface IPublicUserService
    {
        Task<List<PublicUser>> GetPublicUsers();

        Task<PublicUser> GetPublicUser(int ID);

        Task<PublicUser> CreatePublicUser(PublicUser contractor);

        Task<PublicUser> UpdatePublicUser(PublicUser contractor);

        Task<PublicUser> DeletePublicUser(int ID);
    }
}

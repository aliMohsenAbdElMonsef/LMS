using Microsoft.AspNetCore.Identity;

namespace LMS.Models.DataModels
{
    public class ApplicationRole: IdentityRole<string>
    {
        public ApplicationRole() : base()
        {
            Id = Guid.NewGuid().ToString();
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
            Id = Guid.NewGuid().ToString(); 
        }
    }
}

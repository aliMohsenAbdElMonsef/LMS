using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MainEntities
{
    public class ApplicationRole:IdentityRole<string>
    {
        ApplicationRole():base() 
        { 
            Id = Guid.NewGuid().ToString();
        }
        ApplicationRole(string roleName) : base(roleName)
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}

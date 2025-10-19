using Domain.Entities.MainEntities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Seedings
{
    public static class IdentitySeeding
    {
        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new ApplicationRole("Admin"));
            }

            var admin = await userManager.FindByEmailAsync("admin@lms.com");
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin@lms.com",
                    Email = "admin@lms.com",
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    Status = Domain.Enums.ApplicationStatus.Approved,
                    ApplyAs = Domain.Enums.UserType.Admin
                };

                var result = await userManager.CreateAsync(admin, "3lemny_");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}

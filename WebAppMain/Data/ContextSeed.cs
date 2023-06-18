using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using WebAppMain.Models;

namespace WebAppMain.Data
{
    public static class ContextSeed
    {
        public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await CreateRoleIfNotExists(roleManager, Enums.Roles.Администратор.ToString());
            await CreateRoleIfNotExists(roleManager, Enums.Roles.Учитель.ToString());
            await CreateRoleIfNotExists(roleManager, Enums.Roles.Ученик.ToString());
        }

        private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@gmail.com",
                Name = "Илья",
                Surname = "Плеханов",
                Patronymic = "Эдуардович",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,  
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "123Pa$$word");

                    await userManager.AddToRoleAsync(defaultUser, Enums.Roles.Администратор.ToString());
                }
            }
        }
    }
}

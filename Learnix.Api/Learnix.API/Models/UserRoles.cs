namespace Learnix.API.Models
{
    public static class UserRoles
    {
        public const string User = "User";
        public const string Admin = "Admin";

        public static string? Normalize(string role)
        {
            if (role.Equals(Admin, StringComparison.OrdinalIgnoreCase))
            {
                return Admin;
            }

            if (role.Equals(User, StringComparison.OrdinalIgnoreCase))
            {
                return User;
            }

            return null;
        }
    }
}

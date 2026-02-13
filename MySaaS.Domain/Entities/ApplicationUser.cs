using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySaaS.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        // THE GOLDEN LINK: This ties the User to a specific Tenant (Company)
        public Guid TenantId { get; set; }

        // Password Reset Token fields
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Navigation Property (allows us to write user.Tenant.Name)
        public Tenant? Tenant { get; set; }
    }
}

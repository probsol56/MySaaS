using MySaaS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySaaS.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? SubscriptionExpiresAt { get; set; }
    }
}

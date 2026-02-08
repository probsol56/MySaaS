using System;
using System.Collections.Generic;
using System.Text;

namespace MySaaS.Application.Common.Interfaces
{
    public interface ICurrentTenantService
    {
        Guid? TenantId { get; }
        bool IsAuthenticated { get; }
        Guid? UserId { get; }
    }
}

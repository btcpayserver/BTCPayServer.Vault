using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;

namespace BTCPayServer.Vault.Services
{
    public class PermissionsService
    {
        ConcurrentDictionary<OriginReason, GrantedPermission> _permissions = new ConcurrentDictionary<OriginReason, GrantedPermission>();
        public Task Grant(OriginReason originReason)
        {
            _permissions.TryAdd(originReason, new GrantedPermission(originReason));
            return Task.CompletedTask;
        }
        public Task UpdateAccessed(OriginReason originReason)
        {
            if (_permissions.TryGetValue(originReason, out var permission))
                permission.LastAccessed = DateTimeOffset.UtcNow;
            return Task.CompletedTask;
        }

        public Task<ICollection<GrantedPermission>> GetPermissions()
        {
            return Task.FromResult(_permissions.Values);
        }

        public Task Revoke(OriginReason originReason)
        {
            _permissions.TryRemove(originReason, out _);
            return Task.CompletedTask;
        }

        public Task<bool> IsGranted(OriginReason originReason)
        {
            return Task.FromResult(_permissions.TryGetValue(originReason, out _));
        }
    }

    public class GrantedPermission
    {
        public GrantedPermission(OriginReason originReason)
        {
            OriginReason = originReason;
            Created = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? LastAccessed { get; set; }
        public OriginReason OriginReason { get; set; }
    }
}

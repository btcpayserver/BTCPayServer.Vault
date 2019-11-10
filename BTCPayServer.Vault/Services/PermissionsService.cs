using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BTCPayServer.Vault.Services
{
    public class PermissionsService
    {
        ConcurrentDictionary<string, GrantedPermission> _permissions = new ConcurrentDictionary<string, GrantedPermission>();
        public Task Grant(string origin)
        {
            _permissions.TryAdd(origin, new GrantedPermission(origin));
            return Task.CompletedTask;
        }
        public Task UpdateAccessed(string origin)
        {
            if (_permissions.TryGetValue(origin, out var permission))
                permission.LastAccessed = DateTimeOffset.UtcNow;
            return Task.CompletedTask;
        }

        public Task<ICollection<GrantedPermission>> GetPermissions()
        {
            return Task.FromResult(_permissions.Values);
        }

        public Task Revoke(string origin)
        {
            _permissions.TryRemove(origin, out _);
            return Task.CompletedTask;
        }
    }

    public class GrantedPermission
    {
        public GrantedPermission(string origin)
        {
            Origin = origin;
            Created = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? LastAccessed { get; set; }
        public string Origin { get; set; }
    }
}

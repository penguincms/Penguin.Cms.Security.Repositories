using Penguin.Cms.Security.Objects;
using Penguin.Entities;
using Penguin.Messaging.Core;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Penguin.Security.Abstractions;
using Penguin.Security.Abstractions.Extensions;
using Penguin.Security.Abstractions.Interfaces;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// A repository for accessing entity permissions
    /// </summary>
    public class EntityPermissionsRepository : EntityRepository<EntityPermissions>
    {
        /// <summary>
        /// Constructs this repository using the given values
        /// </summary>
        /// <param name="context">The underlying context to use for persistence</param>
        /// <param name="messageBus">An optional message bus for event notification</param>
        public EntityPermissionsRepository(IPersistenceContext<EntityPermissions> context, MessageBus messageBus = null) : base(context, messageBus)
        {
        }

        /// <summary>
        /// Retrieves the permissions for a single entity
        /// </summary>
        /// <param name="target">The target entity to retrieve permissions for</param>
        /// <returns>The permissions applicable to the entity</returns>
        public EntityPermissions GetForEntity(Entity target)
        {
            return this.Where(p => p.EntityGuid == target.Guid).SingleOrDefault();
        }

        /// <summary>
        /// Adds the specified permissions to the Entity
        /// </summary>
        /// <param name="target">The entity that the permissions are applied to</param>
        /// <param name="securityGroup">The security group being granted the permissions</param>
        /// <param name="permissionTypes">The permission types to add</param>
        public void AddPermission(Entity target, SecurityGroup securityGroup, PermissionTypes permissionTypes)
        {
            EntityPermissions existing = this.GetForEntity(target);

            if(existing is null)
            {
                existing = new EntityPermissions();
                existing.AddPermission(securityGroup, permissionTypes);
                this.Add(existing);
            } else
            {
                existing.AddPermission(securityGroup, permissionTypes);
                this.AddOrUpdate(existing);
            }
        }

        /// <summary>
        /// Checks if the given entity allows a given kind of access for the specified user
        /// </summary>
        /// <param name="target">The entity the user is trying to access</param>
        /// <param name="user">The user doing the accessing</param>
        /// <param name="permissionTypes">The permission types needed to perform the action</param>
        /// <returns>True if the user is allowed to continue</returns>
        public bool AllowsAccessType(Entity target, IUser user, PermissionTypes permissionTypes)
        {
            EntityPermissions existing = this.GetForEntity(target);

            if(existing != null)
            {
                return existing.AllowsAccessType(user, permissionTypes);
            } else
            {
                return false;
            }
        }
    }
}

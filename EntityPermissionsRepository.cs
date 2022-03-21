using Penguin.Cms.Entities;
using Penguin.Cms.Repositories;
using Penguin.Cms.Repositories.Interfaces;
using Penguin.Messaging.Core;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Security.Abstractions;
using Penguin.Security.Abstractions.Extensions;
using Penguin.Security.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// A repository for accessing entity permissions
    /// </summary>
    public class EntityPermissionsRepository : EntityRepository<EntityPermissions>
    {
        private readonly Dictionary<Guid, EntityPermissions> PermissionsCache = new Dictionary<Guid, EntityPermissions>();
        protected IEntityRepository<SecurityGroup> SecurityGroupRepository { get; set; }

        /// <summary>
        /// Constructs this repository using the given values
        /// </summary>
        /// <param name="context">The underlying context to use for persistence</param>
        /// <param name="messageBus">An optional message bus for event notification</param>
        public EntityPermissionsRepository(IPersistenceContext<EntityPermissions> context, IEntityRepository<SecurityGroup> securityGroupRepository, MessageBus messageBus = null) : base(context, messageBus)
        {
            this.SecurityGroupRepository = securityGroupRepository;
        }

        /// <summary>
        /// Adds the whole set of permissions to a new or existing instance of permissions
        /// </summary>
        /// <param name="o">The new entity permissions</param>
        public override void Add(EntityPermissions o)
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            if (o.EntityGuid == Guid.Empty)
            {
                throw new Exception("Can not add permissions with empty guid");
            }

            EntityPermissions existing = this.GetForEntity(o.EntityGuid);

            if (existing is null)
            {
                base.Add(this.ShallowClone(o));
            }
            else
            {
                foreach (SecurityGroupPermission sg in o.Permissions)
                {
                    this.AddPermission(o.EntityGuid, sg.SecurityGroup, sg.Type);
                }
            }
        }

        /// <summary>
        /// Updates any existing entity permissions or adds if they're new
        /// </summary>
        /// <param name="o">The updated entity permissions</param>
        public override void AddOrUpdate(EntityPermissions o) => this.Update(o);

        /// <summary>
        /// Updates any existing entity permissions or adds if they're new
        /// </summary>
        /// <param name="o">The updated entity permissions</param>
        public override void AddOrUpdateRange(IEnumerable<EntityPermissions> o) => this.UpdateRange(o);

        /// <summary>
        /// Adds the specified permissions to the Entity
        /// </summary>
        /// <param name="target">The entity that the permissions are applied to</param>
        /// <param name="securityGroup">The security group being granted the permissions</param>
        /// <param name="permissionTypes">The permission types to add</param>
        public void AddPermission(Entity target, SecurityGroup securityGroup, PermissionTypes permissionTypes)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            this.AddPermission(target.Guid, securityGroup, permissionTypes);
        }

        /// <summary>
        /// Adds the specified permissions to the Entity
        /// </summary>
        /// <param name="target">The entity that the permissions are applied to</param>
        /// <param name="securityGroup">The security group being granted the permissions</param>
        /// <param name="permissionTypes">The permission types to add</param>
        public void AddPermission(Guid target, SecurityGroup securityGroup, PermissionTypes permissionTypes)
        {
            if (securityGroup is null)
            {
                throw new ArgumentNullException(nameof(securityGroup));
            }

            securityGroup = this.SecurityGroupRepository.Find(securityGroup.Guid);

            EntityPermissions existing = this.GetForEntity(target);
            bool foundPermissions;
            if (existing is null)
            {
                foundPermissions = this.PermissionsCache.TryGetValue(target, out existing);
            }
            else
            {
                foundPermissions = true;
            }

            if (!foundPermissions)
            {
                existing = new EntityPermissions()
                {
                    EntityGuid = target
                };

                existing.AddPermission(securityGroup, permissionTypes);
                this.PermissionsCache.Add(target, existing);
                base.Add(existing);
            }
            else
            {
                existing.AddPermission(securityGroup, permissionTypes);
                base.AddOrUpdate(existing);
            }
        }

        /// <summary>
        /// Adds the whole set of permissions to a new or existing instance of permissions
        /// </summary>
        /// <param name="o">The new entity permissions</param>
        public override void AddRange(IEnumerable<EntityPermissions> o)
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            foreach (EntityPermissions e in o)
            {
                this.Add(e);
            }
        }

        /// <summary>
        /// Checks if the given entity allows a given kind of access for the specified user
        /// </summary>
        /// <param name="target">The Guid of the entity that is being accessed</param>
        /// <param name="user">The user doing the accessing</param>
        /// <param name="permissionTypes">The permission types needed to perform the action</param>
        /// <returns>True if the user is allowed to continue</returns>
        public bool AllowsAccessType(Guid target, IUser user, PermissionTypes permissionTypes)
        {
            EntityPermissions existing = this.GetForEntity(target);

            if (existing != null)
            {
                return existing.AllowsAccessType(user, permissionTypes);
            }
            else
            {
                return false;
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
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return this.AllowsAccessType(target.Guid, user, permissionTypes);
        }

        /// <summary>
        /// Retrieves the permissions for a single entity
        /// </summary>
        /// <param name="target">The target entity to retrieve permissions for</param>
        /// <returns>The permissions applicable to the entity</returns>
        public EntityPermissions GetForEntity(Entity target)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return this.GetForEntity(target.Guid);
        }

        /// <summary>
        /// Retrieves the permissions for a single entity
        /// </summary>
        /// <param name="target">The target entities guid to retrieve permissions for</param>
        /// <returns>The permissions applicable to the entity</returns>
        public EntityPermissions GetForEntity(Guid target)
        {
            List<EntityPermissions> existing = this.Where(p => p.EntityGuid == target).ToList();

            if (!existing.Any())
            {
                return null;
            }

            EntityPermissions toReturn = existing.FirstOrDefault();

            //This is really fucking stupid but there's no time to fix it now
            if (existing.Count > 1)
            {
                using (IWriteContext context = this.WriteContext())
                {
                    foreach (EntityPermissions EntityPermissions in existing.Skip(1))
                    {
                        EntityPermissions entityPermissions = this.Find(EntityPermissions._Id);

                        List<SecurityGroupPermission> toMove = entityPermissions.Permissions.ToList();

                        entityPermissions.Permissions.Clear();

                        foreach (SecurityGroupPermission sg in toMove)
                        {
                            toReturn.AddPermission(sg.SecurityGroup, sg.Type);
                        }

                        this.Delete(entityPermissions);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Updates any existing entity permissions or adds if they're new
        /// </summary>
        /// <param name="o">The updated entity permissions</param>
        public override void Update(EntityPermissions o)
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            if (o.EntityGuid == Guid.Empty)
            {
                throw new Exception("Can not update permissions with empty guid");
            }

            EntityPermissions existing = this.GetForEntity(o.EntityGuid);

            if (existing is null)
            {
                base.Add(this.ShallowClone(o));
            }
            else
            {
                existing.Permissions = o.Permissions;
            }
        }

        /// <summary>
        /// Updates any existing entity permissions or adds if they're new
        /// </summary>
        /// <param name="o">The updated entity permissions</param>
        public override void UpdateRange(IEnumerable<EntityPermissions> o)
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            foreach (EntityPermissions e in o)
            {
                this.Update(e);
            }
        }
    }
}
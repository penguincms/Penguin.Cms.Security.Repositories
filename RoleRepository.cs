using Penguin.Messaging.Core;
using Penguin.Persistence.Abstractions.Interfaces;
using System.Linq;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// An IRepository implementation for Roles
    /// </summary>
    public class RoleRepository : GroupRoleRepository<Role>
    {
        /// <summary>
        /// Constructs a new instance of this repository
        /// </summary>
        /// <param name="dbContext">An IPersistence context for accessing roles</param>
        /// <param name="messageBus">An optional message bus for sending persistence messages</param>
        public RoleRepository(IPersistenceContext<Role> dbContext, MessageBus messageBus = null) : base(dbContext, messageBus)
        {
        }

        /// <summary>
        /// If a role with a matching name exists, it is returned. If not, it is created and then returned
        /// </summary>
        /// <param name="roleName">The name of the role to get/set</param>
        /// <param name="roleDescription">A description to give the role if it does not exist</param>
        /// <param name="source">The data source of the role being created</param>
        /// <returns>The role if it exists, or a new role if it doesn't</returns>
        public Role CreateIfNotExists(string roleName, string roleDescription, SecurityGroup.SecurityGroupSource source = SecurityGroup.SecurityGroupSource.System)
        {
            Role existingRole = this.GetByName(roleName);
            if (existingRole == null)
            {
                Role thisRole = new Role()
                {
                    Name = roleName,
                    Description = roleDescription,
                    Source = source
                };

                this.Add(thisRole);

                return thisRole;
            }

            return existingRole;
        }

        /// <summary>
        /// Checks to see if the role exists in the persistence context
        /// </summary>
        /// <param name="name">The name of the role to check for</param>
        /// <returns>If the role exists</returns>
        public bool Exists(string name) => this.Where(r => r.ExternalId == name).Any();
    }
}
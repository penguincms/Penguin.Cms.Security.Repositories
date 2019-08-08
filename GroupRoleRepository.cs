using Penguin.Cms.Security;
using Penguin.Messaging.Core;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Persistence.Repositories.Interfaces;
using Penguin.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// A base implementation of a repository for both groups and roles
    /// </summary>
    /// <typeparam name="T">Any grouprole type</typeparam>
    public class GroupRoleRepository<T> : SecurityGroupRepository<T> where T : GroupRole
    {
        /// <summary>
        /// Constructs a new instance of this repository
        /// </summary>
        /// <param name="dbContext">A persistence context for either a group, or a role (or any other derived type)</param>
        /// <param name="messageBus">An optional message bus for persistence messages</param>
        public GroupRoleRepository(IPersistenceContext<T> dbContext, MessageBus messageBus = null) : base(dbContext, messageBus)
        {
        }

        /// <summary>
        /// Gets a group or role by name
        /// </summary>
        /// <param name="Name">The name to check for</param>
        /// <returns>The group/role or null</returns>
        public T GetByName(string Name) => this.Where(t => t.ExternalId == Name).SingleOrDefault();

        /// <summary>
        /// Gets any groups/roles that are set to be assigned to all new users
        /// </summary>
        /// <returns>Any groups/roles that are set to be assigned to all new users</returns>
        public List<T> GetDefaults() => this.Where(gr => gr.IsDefault).ToList();
    }
}
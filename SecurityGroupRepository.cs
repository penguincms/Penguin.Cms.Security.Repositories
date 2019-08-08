using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Penguin.Messaging.Core;
using Penguin.Reflection;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Cms.Security;
using Penguin.Cms.Repositories;
using Penguin.Persistence.Repositories.Interfaces;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// An agnostic implementation of a repository for all security group types
    /// </summary>
	public class SecurityGroupRepository : SecurityGroupRepository<SecurityGroup>
    {
        #region Constructors
        /// <summary>
        /// Constructs a new instance of this repository type
        /// </summary>
        /// <param name="dbContext">The Persistence context designed to handle all security groups</param>
        /// <param name="messageBus">An optional message bus for sending persistence messages</param>
        public SecurityGroupRepository(IPersistenceContext<SecurityGroup> dbContext, MessageBus messageBus = null) : base(dbContext, messageBus)
        {
        }

        #endregion Constructors
    }

    /// <summary>
    /// Constructs a new instance of this repository type
    /// </summary>
    /// <typeparam name="T">Any type inheriting from security group</typeparam>
    public class SecurityGroupRepository<T> : UserAuditableEntityRepository<T> where T : SecurityGroup
    {
        #region Properties

        private static AsyncLocal<Dictionary<Guid, SecurityGroup>> CachedGroupsHolder { get; set; } = new AsyncLocal<Dictionary<Guid, SecurityGroup>>()
        {
            Value = new Dictionary<Guid, SecurityGroup>()
        };

        private Dictionary<Type, IEntityRepository> _securityRepositories { get; set; } = new Dictionary<Type, IEntityRepository>();

        private List<Type> _securityTypes { get; set; } = new List<Type>();

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructs a new instance of this repository type
        /// </summary>
        /// <param name="dbContext">The Persistence context for this particular security group kind</param>
        /// <param name="messageBus">An optional message bus for sending persistence messages</param>
        public SecurityGroupRepository(IPersistenceContext<T> dbContext, MessageBus messageBus = null) : base(dbContext, messageBus)
        {

        }

        #endregion Constructors

    }
}
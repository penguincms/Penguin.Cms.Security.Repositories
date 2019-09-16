using Penguin.Cms.Repositories;
using Penguin.Messaging.Core;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Persistence.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// An agnostic implementation of a repository for all security group types
    /// </summary>
	public class SecurityGroupRepository : SecurityGroupRepository<SecurityGroup>
    {
        /// <summary>
        /// Constructs a new instance of this repository type
        /// </summary>
        /// <param name="dbContext">The Persistence context designed to handle all security groups</param>
        /// <param name="messageBus">An optional message bus for sending persistence messages</param>
        public SecurityGroupRepository(IPersistenceContext<SecurityGroup> dbContext, MessageBus messageBus = null) : base(dbContext, messageBus)
        {
        }
    }

    /// <summary>
    /// Constructs a new instance of this repository type
    /// </summary>
    /// <typeparam name="T">Any type inheriting from security group</typeparam>
    public class SecurityGroupRepository<T> : UserAuditableEntityRepository<T> where T : SecurityGroup
    {

        /// <summary>
        /// Constructs a new instance of this repository type
        /// </summary>
        /// <param name="dbContext">The Persistence context for this particular security group kind</param>
        /// <param name="messageBus">An optional message bus for sending persistence messages</param>
        public SecurityGroupRepository(IPersistenceContext<T> dbContext, MessageBus messageBus = null) : base(dbContext, messageBus)
        {
        }
    }
}
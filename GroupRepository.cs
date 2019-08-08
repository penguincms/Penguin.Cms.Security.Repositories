using Penguin.Cms.Security;
using Penguin.Messaging.Core;
using Penguin.Persistence.Abstractions.Interfaces;
using System;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// An IRepository implementation for basic group actions
    /// </summary>
    public class GroupRepository : GroupRoleRepository<Group>
    {
        /// <summary>
        /// Constructs a new instance of this repository
        /// </summary>
        /// <param name="dbContext">An IPersistence context implementation for accessing groups</param>
        /// <param name="messageBus">An optional Message bus for persistence messages</param>
        public GroupRepository(IPersistenceContext<Group> dbContext, MessageBus messageBus = null) : base(dbContext, messageBus)
        {
        }

        /// <summary>
        /// If a group with a matching name does not exist, it creates it. If it does, it returns the existing instance
        /// </summary>
        /// <param name="groupName">The name of the group to get/set</param>
        /// <param name="groupDescription">The description to set if the group does not exist</param>
        /// <returns>If a group with a matching name does not exist, it creates it. If it does, it returns the existing instance</returns>
        public Group CreateIfNotExists(string groupName, string groupDescription)
        {
            Group existingGroup = this.GetByName(groupName);
            if (existingGroup == null)
            {
                Group thisGroup = new Group()
                {
                    Name = groupName,
                    Description = groupDescription
                };

                this.Add(thisGroup);

                return thisGroup;
            }

            return existingGroup;
        }
    }
}
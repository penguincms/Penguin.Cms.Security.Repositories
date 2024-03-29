﻿using Penguin.Cms.Repositories;
using Penguin.Cms.Security.Constants;
using Penguin.Cms.Security.Extensions;
using Penguin.Extensions.String.Security;
using Penguin.Messaging.Abstractions.Interfaces;
using Penguin.Messaging.Core;
using Penguin.Messaging.Persistence.Messages;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Security.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// A default implementation of an IRepository for users containing helpful methods
    /// </summary>
    public class UserRepository : EntityRepository<User>, IMessageHandler<Creating<User>>
    {
        /// <summary>
        /// Group repository for assigning default groups
        /// </summary>
        protected IRepository<Group> GroupRepository { get; set; }

        /// <summary>
        /// Role repository for assigning default roles
        /// </summary>
        protected IRepository<Role> RoleRepository { get; set; }

        /// <summary>
        /// Constructs a new instance of this repository
        /// </summary>
        /// <param name="context">An IPersistence context implementation for Users</param>
        /// <param name="roleRepository">A role repository for getting default roles</param>
        /// <param name="groupRepository">A group repository for getting default groups</param>
        /// <param name="messageBus">An optional message bus for persistence messages</param>
        public UserRepository(IPersistenceContext<User> context, IRepository<Role> roleRepository, IRepository<Group> groupRepository, MessageBus messageBus = null) : base(context, messageBus)
        {
            RoleRepository = roleRepository;
            GroupRepository = groupRepository;
        }

        /// <summary>
        /// Message handler for creating a user, user to ensure that all defaults are properly assigned
        /// </summary>
        /// <param name="message">Persistence message containing the user being created</param>
        public void AcceptMessage(Creating<User> message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            foreach (Role thisRole in RoleRepository.GetDefaults())
            {
                message.Target.Roles.Add(thisRole);
            }

            foreach (Group thisGroup in GroupRepository.GetDefaults())
            {
                message.Target.Groups.Add(thisGroup);
            }

            message.Target.Enabled = true;
        }

        /// <summary>
        /// Adds a new user and assigns default groups/roles
        /// </summary>
        /// <param name="o">Users to add</param>
        public override void Add(User o)
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            AddDefaults(o);
            base.Add(o);
        }

        /// <summary>
        /// Adds or updates a new user and assigns default groups/roles
        /// </summary>
        /// <param name="o">Users to add</param>
        public override void AddOrUpdate(User o)
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            AddDefaults(o);
            base.AddOrUpdate(o);
        }

        /// <summary>
        /// Adds or updates a new user and assigns default groups/roles
        /// </summary>
        /// <param name="o">Users to add</param>
        public override void AddOrUpdateRange(IEnumerable<User> o)
        {
            if (o is null)
            {
                throw new System.ArgumentNullException(nameof(o));
            }

            AddDefaults(o);
            base.AddOrUpdateRange(o);
        }

        /// <summary>
        /// Adds a new user and assigns default groups/roles
        /// </summary>
        /// <param name="o">Users to add</param>
        public override void AddRange(IEnumerable<User> o)
        {
            if (o is null)
            {
                throw new System.ArgumentNullException(nameof(o));
            }

            AddDefaults(o);
            base.AddRange(o);
        }

        /// <summary>
        /// Gets a user by ID, or returns a Guest user instance if the Id is 0 or doesn't exist
        /// </summary>
        /// <param name="Id">The user to get</param>
        /// <returns> a user by ID, or returns a Guest user instance if the Id is 0 or doesn't exist</returns>
        public override User Find(int Id)
        {
            if (Id == 0 || !this.Where(u => u._Id == Id).Any())
            {
                //This is dumb. For guest users we have to make sure the Guid matches the DB role guid, so we have to pull it
                User toReturn = Users.Guest;

                foreach (Role thisRole in toReturn.Roles.ToList())
                {
                    Role DBRole = RoleRepository.GetByName(thisRole.Name);

                    if (DBRole != null)
                    {
                        _ = toReturn.Roles.Remove(thisRole);
                        toReturn.Roles.Add(DBRole);
                    }
                }

                foreach (Group thisGroup in toReturn.Groups.ToList())
                {
                    Group DBGroup = GroupRepository.GetByName(thisGroup.Name);

                    if (DBGroup != null)
                    {
                        _ = toReturn.Groups.Remove(thisGroup);
                        toReturn.Groups.Add(DBGroup);
                    }
                }

                return toReturn;
            }
            else
            {
                return this.Where(u => u._Id == Id).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a user with a matching email
        /// </summary>
        /// <param name="email">The email to check for</param>
        /// <returns>A user with a matching email, or null if none</returns>
        public User GetByEmail(string email)
        {
            return this.FirstOrDefault(u => u.Email == email);
        }

        /// <summary>
        /// Gets a user with a matching login
        /// </summary>
        /// <param name="login">The login to get</param>
        /// <returns>A user with a  matching login, or null if none</returns>
        public User GetByLogin(string login)
        {
            return this.FirstOrDefault(u => u.ExternalId == login);
        }

        /// <summary>
        /// Gets a user matching the supplied username and password
        /// </summary>
        /// <param name="login">The login to check for</param>
        /// <param name="password">The password the user must have</param>
        /// <returns>A user if the login information is correct, or null if not</returns>
        public User GetByLoginPassword(string login, string password)
        {
            string hPass = password.ComputeSha512Hash();
            return this.FirstOrDefault(u => u.ExternalId == login && u.HashedPassword == hPass);
        }

        /// <summary>
        /// Updates an existing user and adds default roles
        /// </summary>
        /// <param name="o">User to update</param>
        public override void Update(User o)
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            AddDefaults(o);
            base.Update(o);
        }

        /// <summary>
        /// Updates an existing user and adds default roles
        /// </summary>
        /// <param name="o">User to update</param>
        public override void UpdateRange(IEnumerable<User> o)
        {
            if (o is null)
            {
                throw new System.ArgumentNullException(nameof(o));
            }

            AddDefaults(o);
            base.UpdateRange(o);
        }

        private void AddDefaults(IEnumerable<User> o)
        {
            foreach (User entity in o)
            {
                AddDefaults(entity);
            }
        }

        private void AddDefaults(User entity)
        {
            if (entity.ExternalId != Users.Guest.ExternalId && !entity.HasRole(Roles.LoggedIn))
            {
                Role LoggedIn = RoleRepository.GetByName(Roles.LoggedIn.Name);

                if (LoggedIn is null)
                {
                    throw new NullReferenceException($"A role with the name \"{Roles.LoggedIn.Name}\" was not found and can not be added to the requested user {entity.ExternalId}");
                }
                else
                {
                    entity.AddRole(LoggedIn);
                }
            }
        }
    }
}
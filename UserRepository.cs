using Penguin.Cms.Security.Extensions;
using Penguin.Messaging.Core;
using Penguin.Messaging.Persistence.Messages;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Security.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// A default implementation of an IRepository for users containing helpful methods
    /// </summary>
    public class UserRepository : SecurityGroupRepository<User>
    {
        /// <summary>
        /// Group repository for assigning default groups
        /// </summary>
        protected GroupRepository GroupRepository { get; set; }

        /// <summary>
        /// Role repository for assigning default roles
        /// </summary>
        protected RoleRepository RoleRepository { get; set; }

        /// <summary>
        /// Constructs a new instance of this repository
        /// </summary>
        /// <param name="context">An IPersistence context implemementation for Users</param>
        /// <param name="roleRepository">A role repository for getting default roles</param>
        /// <param name="groupRepository">A group repository for getting default groups</param>
        /// <param name="messageBus">An optional message bus for persistence messages</param>
        public UserRepository(IPersistenceContext<User> context, RoleRepository roleRepository, GroupRepository groupRepository, MessageBus messageBus = null) : base(context, messageBus)
        {
            this.RoleRepository = roleRepository;
            this.GroupRepository = groupRepository;
        }

        /// <summary>
        /// Message handler for creating a user, user to ensure that all defaults are properly assigned
        /// </summary>
        /// <param name="createMessage">Persistence message containing the user being created</param>
        public override void AcceptMessage(Creating<User> createMessage)
        {
            Contract.Requires(createMessage != null);

            foreach (Role thisRole in this.RoleRepository.GetDefaults())
            {
                createMessage.Target.Roles.Add(thisRole);
            }

            foreach (Group thisGroup in this.GroupRepository.GetDefaults())
            {
                createMessage.Target.Groups.Add(thisGroup);
            }

            createMessage.Target.Enabled = true;

            base.AcceptMessage(createMessage);
        }

        /// <summary>
        /// Adds a new user and assigns default groups/roles
        /// </summary>
        /// <param name="o">Users to add</param>
        public override void Add(User o)
        {
            this.AddDefaults(o);
            base.Add(o);
        }

        /// <summary>
        /// Adds or updates a new user and assigns default groups/roles
        /// </summary>
        /// <param name="o">Users to add</param>
        public override void AddOrUpdate(User o)
        {
            this.AddDefaults(o);
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

            this.AddDefaults(o);
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

            this.AddDefaults(o);
            base.AddRange(o);
        }

        /// <summary>
        /// Gets a user by ID, or returns a Guest user instance if the Id is 0 or doesn't exist
        /// </summary>
        /// <param name="userId">The user to get</param>
        /// <returns> a user by ID, or returns a Guest user instance if the Id is 0 or doesn't exist</returns>
        public override User Find(int userId)
        {
            if (userId == 0 || !this.Where(u => u._Id == userId).Any())
            {
                //This is dumb. For guest users we have to make sure the Guid matches the DB role guid, so we have to pull it
                User toReturn = Users.Guest;

                foreach (Role thisRole in toReturn.Roles.ToList())
                {
                    Role DBRole = this.RoleRepository.GetByName(thisRole.Name);

                    if (DBRole != null)
                    {
                        toReturn.Roles.Remove(thisRole);
                        toReturn.Roles.Add(DBRole);
                    }
                }

                foreach (Group thisGroup in toReturn.Groups.ToList())
                {
                    Group DBGroup = this.GroupRepository.GetByName(thisGroup.Name);

                    if (DBGroup != null)
                    {
                        toReturn.Groups.Remove(thisGroup);
                        toReturn.Groups.Add(DBGroup);
                    }
                }

                return toReturn;
            }
            else
            {
                return this.Where(u => u._Id == userId).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a user with a matching email
        /// </summary>
        /// <param name="email">The email to check for</param>
        /// <returns>A user with a matching email, or null if none</returns>
        public User GetByEmail(string email) => this.FirstOrDefault(u => u.Email == email);

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
            return this.FirstOrDefault(u => u.ExternalId == login && u.Password == password);
        }

        /// <summary>
        /// Updates an existing user and adds default roles
        /// </summary>
        /// <param name="o">User to update</param>
        public override void Update(User o)
        {
            this.AddDefaults(o);
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

            this.AddDefaults(o);
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
                Role LoggedIn = this.RoleRepository.GetByName(Roles.LoggedIn.Name);

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
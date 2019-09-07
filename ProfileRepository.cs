using Penguin.Cms.Repositories;
using Penguin.Messaging.Core;
using Penguin.Persistence.Abstractions.Interfaces;
using System.Linq;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// An IRepository implementation for accessing user profiles
    /// </summary>
    public class ProfileRepository : UserAuditableEntityRepository<Profile>
    {
        /// <summary>
        /// Constructs a new instance of this repository
        /// </summary>
        /// <param name="dbContext">An IPersistenceContext used to access Profiles</param>
        /// <param name="messageBus">An optional message bus for sending persistence messages</param>
        public ProfileRepository(IPersistenceContext<Profile> dbContext, MessageBus messageBus = null) : base(dbContext, messageBus)
        {
        }

        /// <summary>
        /// Returns the user profile for a user with the requested login
        /// </summary>
        /// <param name="login">The login of the user that owns the profile</param>
        /// <returns>The users profile</returns>
        public Profile GetByLogin(string login) => this.Where(u => u.User.ExternalId == login).FirstOrDefault();
    }
}
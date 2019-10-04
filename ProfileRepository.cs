using Penguin.Messaging.Core;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Persistence.Repositories;
using System.Linq;

namespace Penguin.Cms.Security.Repositories
{
    /// <summary>
    /// An IRepository implementation for accessing user profiles
    /// </summary>
    public class ProfileRepository : EntityRepository<UserProfile>
    {
        /// <summary>
        /// Constructs a new instance of this repository
        /// </summary>
        /// <param name="dbContext">An IPersistenceContext used to access Profiles</param>
        /// <param name="messageBus">An optional message bus for sending persistence messages</param>
        public ProfileRepository(IPersistenceContext<UserProfile> dbContext, MessageBus messageBus = null) : base(dbContext, messageBus)
        {
        }

        /// <summary>
        /// Returns the user profile for a user with the requested login
        /// </summary>
        /// <param name="login">The login of the user that owns the profile</param>
        /// <returns>The users profile</returns>
        public UserProfile GetByLogin(string login) => this.Where(u => u.User.ExternalId == login).FirstOrDefault();
    }
}
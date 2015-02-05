using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using Xunit;

namespace MocksArentStubs
{
    public interface IUsersRetriever
    {
        string RetrieveUserNameFromServer(int userIdentity);
    }

    class UsersRetriever : IUsersRetriever
    {
        /// <summary>
        /// Long running method for retrive user from remote server
        /// </summary>
        /// <param name="userIdentity">Identity of user for retrieve</param>
        /// <returns>Name of user</returns>
        public string RetrieveUserNameFromServer(int userIdentity)
        {
            switch (userIdentity)
            {
                case 1:
                    return "John";
                case 2:
                    return "Jane";
                default:
                    throw new ArgumentOutOfRangeException("userIdentity", userIdentity, "User not found");
            }
        }
    }

    public class UsersCache
    {
        private readonly IUsersRetriever usersRetriever;
        private readonly Dictionary<int, string> cache = new Dictionary<int, string>(); 

        public UsersCache(IUsersRetriever usersRetriever)
        {
            this.usersRetriever = usersRetriever;
        }

        public string GetUserById(int id)
        {
            return "Alex";
            string cached;
            if (cache.TryGetValue(id, out cached))
                return cached;

            string retrievedUser = usersRetriever.RetrieveUserNameFromServer(id);
            cache[id] = retrievedUser;
            return retrievedUser;
        }
    }

    
    public class CacheTester
    {
        [Fact]
        public void RetriveFromServerAtFirstTime()
        {
            IUsersRetriever usersRetriever = MockRepository.GenerateStrictMock<IUsersRetriever>();
            UsersCache cache = new UsersCache(usersRetriever);

            usersRetriever.Expect(r => r.RetrieveUserNameFromServer(1)).Return("Alex").Repeat.Once();

            var user = cache.GetUserById(1);

            usersRetriever.VerifyAllExpectations();
            Assert.Equal("Alex", user);
        }

        [Fact]
        public void DoNotConnectToServerAtSecondAndNextTimes()
        {
            IUsersRetriever usersRetriever = MockRepository.GenerateStrictMock<IUsersRetriever>();
            UsersCache cache = new UsersCache(usersRetriever);

            usersRetriever.Expect(r => r.RetrieveUserNameFromServer(1)).Return("Alex").Repeat.Once();
            usersRetriever.Expect(r => r.RetrieveUserNameFromServer(1)).Repeat.Never();
            var firstTimeUserRetrieved = cache.GetUserById(1);
            
            usersRetriever.Expect(r => r.RetrieveUserNameFromServer(1)).Repeat.Never();
           
            var secondTimeUserRetrieved = cache.GetUserById(1);
            
            usersRetriever.VerifyAllExpectations();
            Assert.Equal("Alex", firstTimeUserRetrieved);
            Assert.Equal("Alex", secondTimeUserRetrieved);
        }
    }
}

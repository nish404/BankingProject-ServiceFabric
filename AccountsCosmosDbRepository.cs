using Bank;
using Microsoft.Azure.Cosmos;
using Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess
{
    /// <summary>
    /// Defines the interface for interacting with the account data store.
    /// </summary>
    /// 

    public class AccountsCosmosDbRepository : IAccountRepository
    {
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;

        // The name of the database and container we will create
        private const string _databaseId = "Accounts";
        private const string _containerId = "accounts";

        public AccountsCosmosDbRepository()
        {
            string endpointUri = "https://bankingdb.documents.azure.com:443/";
            string primaryKey = "xfSTXbXQHEUe6D7rTtLdpZ8QXMz8qxuSsJfwiAtWapSPRM7olhT7aAvcgSkDVyAIsO1wVhe5Uxhf0GYVvbW58g==";

            _cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = "BankProject" });
            _database = _cosmosClient.GetDatabase(_databaseId);
            _container = _database.GetContainer(_containerId);
        }

        /// <summary>
        /// Creates a new account data entity
        /// </summary>
        /// <param name="account">The account to be created</param>
        public Result<Account> CreateAccount(Account account)
        {
            // Create and item. Partition key value and id must be provided in order to create
            ItemResponse<Account> itemResponse = _container.CreateItemAsync<Account>(account, new PartitionKey(account.Id)).Result;

            // The query returned a list of accounts
            return new Result<Account>()
            {
                Succeeded = true,
                Value = itemResponse
            };
        }

        /// <summary>
        /// Deletes the specified account data entity
        /// </summary>
        /// <param name="deletedAccount">The account to be deleted</param>
        public Result<Account> DeleteAccount(Account deletedAccount)
        {
            // Delete an item. Partition key value and id must be provided in order to delete
            ItemResponse<Account> itemResponse = _container.DeleteItemAsync<Account>(deletedAccount.Id, new PartitionKey(deletedAccount.UserName)).Result;

            // The query returned a list of accounts
            return new Result<Account>()
            {
                Succeeded = true,
                Value = itemResponse
            };
        }

        /// <summary>
        /// Gets a list of all accounts in the system
        /// </summary>
        /// <returns>A list of all <see cref="Account"/>s in the system</returns>
        public Result<List<Account>> GetAllAccounts()
        {
            // Building the sql query
            string sqlQueryText = "SELECT * FROM accounts";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<Account> queryResultSetIterator = _container.GetItemQueryIterator<Account>(queryDefinition);

            // Getting the results from the query
            List<Account> accounts = new List<Account>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Account> currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
                foreach (Account account in currentResultSet)
                {
                    accounts.Add(account);
                }
            }

            // Check if the operation returned any accounts
            if (!accounts.Any())
            {
                return new Result<List<Account>>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned a list of accounts
            return new Result<List<Account>>()
            {
                Succeeded = true,
                Value = accounts
            };
        }

        /// <summary>
        /// Gets a list of all accounts that are belong to the user with a given user name
        /// </summary>
        /// <param name="userName">Unique identifier of the user we want to retrieve accounts for</param>
        /// <returns>A list of all <see cref="Account"/>s that belong to the user</returns>
        public Result<List<Account>> GetAllByUsername(string userName)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.UserName = \"{userName}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<Account> queryResultSetIterator = _container.GetItemQueryIterator<Account>(queryDefinition);

            // Getting the results from the query
            List<Account> accounts = new List<Account>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Account> currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
                foreach (Account account in currentResultSet)
                {
                    accounts.Add(account);
                }
            }

            // Check if the operation returned any accounts
            if (!accounts.Any())
            {
                return new Result<List<Account>>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned a list of accounts
            return new Result<List<Account>>()
            {
                Succeeded = true,
                Value = accounts
            };
        }

        /// <summary>
        /// Gets an account with the given account number
        /// </summary>
        /// <param name="accountNumber">Unique account identifier</param>
        /// <returns>The <see cref="Account"/> with the given account number, or null if no account exists with that number</returns>
        public Result<Account> GetByAccountNumber(int accountNumber)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.Number = \"{accountNumber}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<Account> queryResultSetIterator = _container.GetItemQueryIterator<Account>(queryDefinition);

            // Getting the results from the query
            FeedResponse<Account> currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
            // query stops working here
            IEnumerable<Account> accounts = currentResultSet.Resource;

            // Check if the operation returned any accounts
            if (!accounts.Any())
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned an account, our result should be the only account in the list
            Account account = accounts.FirstOrDefault();
            return new Result<Account>()
            {
                Succeeded = true,
                Value = account
            };
        }

        /// <summary>
        /// Gets the account with the given account id
        /// </summary>
        /// <param name="id">Unique account identifier</param>
        /// <returns>The <see cref="Account"/> with the given id, or null if no account exists with that id</returns>
        public Result<Account> GetById(string id)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.Id = \"{id}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<Account> queryResultSetIterator = _container.GetItemQueryIterator<Account>(queryDefinition);

            // Getting the results from the query
            FeedResponse<Account> currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
            IEnumerable<Account> accounts = currentResultSet.Resource;

            // Check if the operation returned any accounts
            if (!accounts.Any())
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned an account, our result should be the only account in the list
            Account account = accounts.FirstOrDefault();
            return new Result<Account>()
            {
                Succeeded = true,
                Value = account
            };
        }

        /// <summary>
        /// Updates the specified account data entity
        /// </summary>
        /// <param name="updatedAccount">The account to be updated</param>
        public Result<Account> UpdateAccount(Account updatedAccount)
        {
            throw new System.NotImplementedException();
        }
    }
}
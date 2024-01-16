using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserCompleteController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly ReusableSQL _reusableSql;

        public UserCompleteController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _reusableSql = new ReusableSQL(config);
        }


        [HttpGet("Users")]
        public IEnumerable<UserComplete> GetUsers()
        {
            string getUsersQuery = @"EXEC TutorialAppSchema.spUsers_Get";

            IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(getUsersQuery);
            
            return users;
        }


        [HttpGet("Users/{active}")]
        public IEnumerable<UserComplete> GetActiveUsers(bool active)
        
        {
            string getActiveUsersQuery = @"EXEC TutorialAppSchema.spUsers_Get @Active = @ActiveParam";
            DynamicParameters parameters = new ();
            parameters.Add("@ActiveParam", active, DbType.Boolean);

            IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(getActiveUsersQuery, parameters);
            
            return users;
        }

 
        [HttpGet("User/{userId}")]
        public UserComplete? GetSingleUser(int userId)
        {
            string getUserQuery = @"EXEC TutorialAppSchema.spUsers_Get @UserId = @UserIdParam";
            DynamicParameters parameters = new ();
            parameters.Add("@UserIdParam", userId, DbType.Int32);

            UserComplete? user = _dapper.LoadDataSingle<UserComplete>(getUserQuery, parameters);

            if(user == null)
            {
                throw new Exception("Failed to find the user");
            }

            return user;
        }


        [HttpPut("UpsertUser")]
        public IActionResult UpsertUser(UserComplete user)
        {
            if (_reusableSql.UpsertUser(user))
            {
                return Ok();
            } 

            throw new Exception("Failed to Update User");
        }


        [HttpDelete("DeleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            int userIdFromToken;
            Int32.TryParse(User.FindFirst("userId")?.Value, out userIdFromToken);

            if (userIdFromToken != userId)
            {
                return BadRequest("Log In to delete your account");
            }

            string deleteQuery = @"EXEC TutorialAppSchema.spUser_Delete @UserId = @UserIdParam";
            DynamicParameters parameters = new();
            parameters.Add("UserIdParam", userId, DbType.Int32);

            if (_dapper.ExecuteSqlWithParameters(deleteQuery, parameters))
            {
                return Ok();
            } 

            throw new Exception("Failed to Delete User");
        }
    }    
}


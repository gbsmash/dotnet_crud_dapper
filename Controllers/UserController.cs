using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        DataContextDapper _dapper;
        public UserController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }


        [HttpGet("GetUsers")]
        public IEnumerable<User> GetUsers()
        {
            string getQuery = @"
                SELECT [UserId],
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active] 
                FROM TutorialAppSchema.Users";

            IEnumerable<User> users = _dapper.LoadData<User>(getQuery, new {});
            
            return users;
        }

        [HttpGet("GetSingleUser/{userId}")]
        public User? GetSingleUser(int userId)
        {
            string getQuery = @"
                SELECT [UserId],
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active] 
                FROM TutorialAppSchema.Users
                WHERE UserId = @UserIdParam";

            User? user = _dapper.LoadDataSingle<User>(getQuery, new {UserIdParam = userId});

            return user;
        }

        [HttpPut("EditUser")]
        public IActionResult EditUser(User user)
        {
            string editQuery = @"
            UPDATE TutorialAppSchema.Users
                SET [FirstName] = @FirstNameParam, 
                    [LastName] = @LastNameParam, 
                    [Email] = @EmailParam, 
                    [Gender] = @GenderParam, 
                    [Active] = @ActiveParam
                WHERE UserId = @UserIdParam";
            
            var parameters = new
            {
                FirstNameParam = user.FirstName,
                LastNameParam = user.LastName,
                EmailParam = user.Email,
                GenderParam = user.Gender,
                ActiveParam = user.Active,
                UserIdParam = user.UserId
            };

            if (_dapper.ExecuteSql(editQuery, parameters))
            {
                return Ok();
            } 

            throw new Exception("Failed to Update User");
        }

        [HttpPost("AddUser")]
        public IActionResult AddUser(UserDTO user)
        {
            string addQuery = @"INSERT INTO TutorialAppSchema.Users(
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active]
                ) VALUES (
                    @FirstNameParam, 
                    @LastNameParam, 
                    @EmailParam, 
                    @GenderParam, 
                    @ActiveParam
                )";

            var parameters = new
            {
                FirstNameParam = user.FirstName,
                LastNameParam = user.LastName,
                EmailParam = user.Email,
                GenderParam = user.Gender,
                ActiveParam = user.Active
            };
            

            if (_dapper.ExecuteSql(addQuery, parameters))
            {
                return Ok();
            } 

            throw new Exception("Failed to Add User");
        }


        [HttpDelete("DeleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            string deleteQuery = @"DELETE FROM TutorialAppSchema.Users
            WHERE UserId = @UserIdParam";

            if (_dapper.ExecuteSql(deleteQuery, new {UserIdParam = userId}))
            {
                return Ok();
            } 

            throw new Exception("Failed to Delete User");
        }
    }    
}


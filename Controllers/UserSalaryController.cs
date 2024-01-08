using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{       
    [ApiController]
    [Route("[controller]")]

    public class UserSalaryController : ControllerBase
    {
        DataContextDapper _dapper;

        public UserSalaryController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }


        [HttpGet("GetAllUserSalary")]
        public IEnumerable<UserSalary> GetAllUserSalary()
        {
            string getQuery = @"SELECT * FROM TutorialAppSchema.UserSalary";

            IEnumerable<UserSalary> allUserSalary = _dapper.LoadData<UserSalary>(getQuery, new {});

            if(allUserSalary != null)
            {
                return allUserSalary;
            }

            throw new Exception("Failed to get users salary");
        }


        [HttpGet("GetUserSalary/{userId}")]
        public UserSalaryDTO GetUserSalary(int userId)
        {
            string getQuery = @"
                SELECT [FirstName], [LastName], [Salary] 
                FROM TutorialAppSchema.Users 
                    JOIN TutorialAppSchema.UserSalary 
                    ON Users.UserId = UserSalary.UserId 
                WHERE Users.UserId = @UserIdParam
            ";

            UserSalaryDTO? userSalary = _dapper.LoadDataSingle<UserSalaryDTO>(getQuery, new {UserIdParam = userId});

            if(userSalary != null)
            {
                return userSalary;
            }

            throw new Exception("Failed to get user salary");
        }



        [HttpPost("AddUserSalary")]
        public IActionResult AddUserSalary(UserSalary userSalary)
        {
            string checkUserExistsQuery = @"
                SELECT *
                    FROM TutorialAppSchema.Users
                    WHERE UserId = @UserIdParam
            ";

            string checkSalaryExistsQuery = @"
                SELECT * 
                    FROM TutorialAppSchema.UserSalary 
                    WHERE UserId = @UserIdParam
            ";

            string addSalaryQuery = @"
                INSERT INTO TutorialAppSchema.UserSalary(
                    [UserId],
                    [Salary]
                ) VALUES (
                    @UserIdParam,
                    @SalaryParam
                )
            ";


            if(_dapper.LoadDataSingle<User>(checkUserExistsQuery, new {UserIdParam = userSalary.UserId}) != null)
            {
                if (!_dapper.LoadData<UserSalary>(checkSalaryExistsQuery, new {UserIdParam = userSalary.UserId}).Any())
                {
                    if (_dapper.ExecuteSql(addSalaryQuery, new {UserIdParam = userSalary.UserId, SalaryParam = userSalary.Salary}))
                    {
                        return Ok();
                    } 
                    throw new Exception("Failed to add user salary");
                }
                return Conflict("User salary already exists.");
            }

            return NotFound("User not found.");
        }


        [HttpPut("EditUserSalary")]
        public IActionResult EditUserSalary(UserSalary userSalary)
        {
            string checkUserExistsQuery = @"
                SELECT *
                    FROM TutorialAppSchema.Users
                    WHERE UserId = @UserIdParam
            ";

            string editUserSalary = @"
                UPDATE TutorialAppSchema.UserSalary
                    SET Salary = @SalaryParam
                    WHERE UserId = @UserIdParam
            ";

            if(_dapper.LoadDataSingle<User>(checkUserExistsQuery, new {UserIdParam = userSalary.UserId}) != null)
            {
                if(_dapper.ExecuteSql(editUserSalary, new {UserIdParam = userSalary.UserId, SalaryParam = userSalary.Salary}))
                {
                    return Ok();
                }
                throw new Exception("Failed to edit user salary");
            }

            return NotFound("User not found.");
        }

        [HttpDelete("DeleteUserSalary/{userId}")]
        public IActionResult DeleteUserSalary(int userId)
        {
            string deleteQuery = @"
                DELETE FROM TutorialAppSchema.UserSalary
                    WHERE UserId = @UserIdParam
            ";

            if(_dapper.ExecuteSql(deleteQuery, new {UserIdParam = userId}))
            {
                return Ok();
            }
            throw new Exception("Failed to delete user salary");
        }
    }
}
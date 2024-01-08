using System.Runtime;
using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserJobInfoController : ControllerBase
    {
        DataContextDapper _dapper;

        public UserJobInfoController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        
        [HttpGet("GetAllUserJobInfos")]
        public IEnumerable<UserJobInfo> GetUserJobInfos()
        {
            string getQuery = @"
                SELECT * FROM TutorialAppSchema.UserJobInfo
            ";

            IEnumerable<UserJobInfo> jobInfos = _dapper.LoadData<UserJobInfo>(getQuery, new {});

            if(jobInfos.Any())
            {
                return jobInfos;
            }
            throw new Exception("Failed to get user job info");
        }

        
        [HttpGet("GetUserJobInfo/{userId}")]
        public UserJobInfo GetUserJobInfo(int userId)
        {
            string getUserJobInfo = @"
                SELECT * FROM TutorialAppSchema.UserJobInfo
                    WHERE UserId = @UserIdParam
            ";

            UserJobInfo? jobInfo = _dapper.LoadDataSingle<UserJobInfo>(getUserJobInfo, new {UserIdParam = userId});
            
            if(jobInfo != null)
            {
                return jobInfo;
            }

            throw new Exception("Failed to get user job info");
        }


        [HttpPost("AddUserJobInfo")]
        public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
        {
            string checkUserExistsQuery = @"
                SELECT * FROM TutorialAppSchema.Users
                    WHERE UserId = @UserIdParam
            ";

            string checkJobInfoExistsQuery = @"
                SELECT * FROM TutorialAppSchema.UserJobInfo
                    WHERE UserId = @UserIdParam
            ";

            string addQuery = @"
                INSERT INTO TutorialAppSchema.UserJobInfo (
                    [UserId],
                    [JobTitle],
                    [Department]
                ) VALUES (
                    @UserIdParam,
                    @JobTitleParam,
                    @DepartmentParam
                )
            ";

            var jobInfoParams = new {
                UserIdParam = userJobInfo.UserId,
                JobTitleParam = userJobInfo.JobTitle,
                DepartmentParam = userJobInfo.Department
            };


            if(_dapper.LoadDataSingle<User>(checkUserExistsQuery, new {UserIdParam = userJobInfo.UserId}) != null)
            {
                if(!_dapper.LoadData<UserJobInfo>(checkJobInfoExistsQuery, new {UserIdParam = userJobInfo.UserId}).Any())
                {
                    if(_dapper.ExecuteSql(addQuery, jobInfoParams))
                    {
                        return Ok();
                    }

                    throw new Exception("Failed to add user job info");
                }

                return Conflict("User job info already exists");
            }

            return NotFound("Failed to find user");
        }

        [HttpPut("EditUserJobInfo")]
        public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
        {
            string editQuery = @"
                UPDATE TutorialAppSchema.UserJobInfo
                    SET JobTitle = @JobTitleParam,
                        Department = @DepartmentParam
                    WHERE UserId = @UserIdParam
            ";

            var jobInfoParams = new {
                UserIdParam = userJobInfo.UserId,
                JobTitleParam = userJobInfo.JobTitle,
                DepartmentParam = userJobInfo.Department
            };


            if(_dapper.ExecuteSql(editQuery, jobInfoParams))
            {
                return Ok();
            }

            return NotFound("User not found");
        }


        [HttpDelete("DeleteUserJobInfo/{userId}")]
        public IActionResult DeleteUserJobInfo(int userId)
        {
            string deleteQuery = @"
                DELETE FROM TutorialAppSchema.UserJobInfo
                    WHERE UserId = @UserIdParam
            ";

            if(_dapper.ExecuteSql(deleteQuery, new {UserIdParam = userId}))
            {
                return Ok();
            }

            throw new Exception("Failed to delete user");
        }
    }

}
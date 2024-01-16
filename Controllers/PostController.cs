using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.DTO;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Controllers
{   
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;


        public PostController(IConfiguration config)
        {
            _dapper = new(config);
        }

        [HttpGet("AllPosts")]
        public IEnumerable<Post> GetPosts()
        {
            string getPostsQuery = @"EXEC TutorialAppSchema.spPosts_Get";

            return _dapper.LoadData<Post>(getPostsQuery);
        }


        [HttpGet("Posts/{userId}")]
        public IEnumerable<Post> GetPosts(int userId)
        {
            string getPostsQuery = @"EXEC TutorialAppSchema.spPosts_Get @UserId = @UserIdParam";
            DynamicParameters parameters = new();
            parameters.Add("@UserIdParam", userId, DbType.Int32);
            
            return _dapper.LoadDataWithParameters<Post>(getPostsQuery, parameters);
        }

        
        [HttpGet("PostsBySearch/{searchTerm}")]
        public IEnumerable<Post> PostsBySearch(string searchTerm)
        {
            string getPostsQuery = @"EXEC TutorialAppSchema.spPosts_Get @SearchValue = @SearchValueParam";
            DynamicParameters parameters = new();
            parameters.Add("@SearchValueParam", $"%{searchTerm}%", DbType.String);

            return _dapper.LoadDataWithParameters<Post>(getPostsQuery, parameters);
        }



        [HttpGet("Post/{postId}")]
        public Post GetPost(int postId)
        {
            string getPostQuery = @"EXEC TutorialAppSchema.spPosts_Get @PostId = @PostIdParam";
            DynamicParameters parameters = new();
            parameters.Add("@PostIdParam", postId, DbType.Int32);

            Post? post = _dapper.LoadDataSingle<Post>(getPostQuery, parameters);
            if(post != null)
            {
                return post;
            }

            throw new Exception("Failed to find the post");
        }


        [HttpPost("UpsertPost")]
        public IActionResult UpsertPost(PostToUpsertDTO postToUpsert)
        {

            var userId = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user ID.");
            }


            string upsertPostQuery = @"
                EXEC TutorialAppSchema.spPosts_Upsert
                    @UserId = @UserIdParam, 
                    @PostId = @PostIdParam,
                    @PostTitle = @PostTitleParam, 
                    @PostContent = @PostContentParam
                ";

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@UserIdParam", userId, DbType.Int32);
            parameters.Add("@PostIdParam", postToUpsert.PostId, DbType.Int32);
            parameters.Add("@PostTitleParam", postToUpsert.PostTitle, DbType.String);
            parameters.Add("@PostContentParam", postToUpsert.PostContent, DbType.String);


            if(_dapper.ExecuteSqlWithParameters(upsertPostQuery, parameters))
            {
                return Ok();
            }

            throw new Exception("Failed to add post");
        }


        [HttpDelete("DeletePost/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string deleteQuery = @"EXEC TutorialAppSchema.spPost_Delete 
                @UserId = @UserIdParameter, 
                @PostId = @PostIdParameter
            ";

            DynamicParameters deleteParameters = new DynamicParameters();
            deleteParameters.Add("@UserIdParameter", User.FindFirst("userId")?.Value, DbType.Int32);
            deleteParameters.Add("@PostIdParameter", postId, DbType.Int32);

            if (_dapper.ExecuteSqlWithParameters(deleteQuery, deleteParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post!");
        }

    }
}
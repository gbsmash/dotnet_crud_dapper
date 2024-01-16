using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using AutoMapper;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.DTO;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace DotnetAPI.Controller
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        private readonly ReusableSQL _reusableSql;
        private readonly IMapper _mapper;
        
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);    
            _reusableSql = new ReusableSQL(config);  
            _mapper = new Mapper(new MapperConfiguration(cfg => {
                cfg.CreateMap<UserForRegistrationDTO, UserComplete>();
            }))  ;
        }



        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDTO userForRegistration)
        {
            if(userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string checkUserExistsQuery = @"
                    SELECT Email FROM TutorialAppSchema.Auth
                        WHERE Email = @EmailParam
                ";

                DynamicParameters userExistsParameters = new();
                userExistsParameters.Add("@EmailParam", userForRegistration.Email, DbType.String);


                if(_dapper.LoadDataSingle<string>(checkUserExistsQuery, userExistsParameters) == null)
                {
                    UserForLoginDTO userForSetPassword = new UserForLoginDTO{
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password
                    };

                    if(_authHelper.SetPassword(userForSetPassword))
                    {
                        UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistration);
                        userComplete.Active = true;
                        
                        if(_reusableSql.UpsertUser(userComplete))
                        {
                            return Ok();
                        }

                        throw new Exception("Failed to add user");

                    }
                    throw new Exception("Failed to register");
                }

                throw new Exception("User with this email already exists");
            }
            throw new Exception("Passwords do not match");
        }


        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDTO userForSetPassword)
        {
            if(_authHelper.SetPassword(userForSetPassword))
            {
                return Ok();
            }

            throw new Exception("Failed to update password");
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDTO userForLogin)
        {
            string getHashAndSaltQuery = @"
                EXEC TutorialAppSchema.spLoginConfirmation_Get
                    @Email = @EmailParam
            ";

            DynamicParameters getPasswordParameters = new();
            getPasswordParameters.Add("@EmailParam", userForLogin.Email, DbType.String);


            UserForLoginConfirmationDTO? userForLoginConfirmation = _dapper
                .LoadDataSingle<UserForLoginConfirmationDTO>(getHashAndSaltQuery, getPasswordParameters) 
                ?? throw new Exception("Failed to find the user");

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForLoginConfirmation.PasswordSalt);

            for(int i=0; i < passwordHash.Length; i++)
            {
                if(passwordHash[i] != userForLoginConfirmation.PasswordHash[i])
                {
                    return StatusCode(401, "Incorrect password");
                }
            }

            string getUserIdQuery = @"
                SELECT UserId 
                    FROM TutorialAppSchema.Users 
                    WHERE Email = @EmailParam
            ";

                        DynamicParameters getUserIdParameters = new();
            getUserIdParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

            int userId = _dapper.LoadDataSingle<int>(getUserIdQuery, getUserIdParameters);

            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userId)}
            });
        }

        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string getUserIdQuery = @"
                SELECT UserId 
                    FROM TutorialAppSchema.Users
                    WHERE UserId = @UserIdParam
            ";

            DynamicParameters parameters = new();
            parameters.Add("@UserIdParam", User.FindFirst("userId")?.Value, DbType.Int32);

            int userId = _dapper.LoadDataSingle<int>(getUserIdQuery, parameters );

            return _authHelper.CreateToken(userId);
        }
    }
}
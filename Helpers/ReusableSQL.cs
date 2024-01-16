using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;


namespace DotnetAPI.Helpers
{
    public class ReusableSQL
    {
        private readonly DataContextDapper _dapper;


        public ReusableSQL(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        public bool UpsertUser(UserComplete user)
        {
            string upsertQuery = @"
                EXEC TutorialAppSchema.spUser_Upsert
                    @FirstName = @FirstNameParam,
                    @LastName = @LastNameParam,
                    @Email = @EmailParam,
                    @Gender = @GenderParam,
                    @JobTitle = @JobTitleParam,
                    @Department = @DepartmentParam,
                    @Salary = @SalaryParam,
                    @Active = @ActiveParam,
                    @UserId = @UserIdParam
                ";

                DynamicParameters parameters = new();

                parameters.Add("@FirstNameParam", user.FirstName, DbType.String);
                parameters.Add("@LastNameParam",user.LastName, DbType.String);
                parameters.Add("@EmailParam", user.Email ,DbType.String);
                parameters.Add("@GenderParam", user.Gender, DbType.String);
                parameters.Add("@JobTitleParam", user.JobTitle, DbType.String);
                parameters.Add("@DepartmentParam", user.Department, DbType.String);
                parameters.Add("@SalaryParam", user.Salary, DbType.Decimal);
                parameters.Add("@ActiveParam", user.Active, DbType.Boolean);
                parameters.Add("@UserIdParam", user.UserId, DbType.Int32);
            

            return _dapper.ExecuteSqlWithParameters(upsertQuery, parameters);
        }


    }
}
namespace DotnetAPI.Models
{

    public partial class PostToUpsertDTO
    {
        public int PostId {get; set;}
        public string PostTitle {get; set;} = "";
        public string PostContent {get; set;} = "";
    }
}
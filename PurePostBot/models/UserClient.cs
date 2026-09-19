namespace PurePostBot.models
{
    public class UserClient
    {
        public long Id { get; set; }
        public long? GroupId { get; set; }
        public string? Caption { get; set; } = "";
        public bool IsChangingGroupId { get; set; }
        public bool IsChangingCaption { get; set; }
    }
}

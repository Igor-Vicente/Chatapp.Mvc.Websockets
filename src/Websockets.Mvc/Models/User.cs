namespace Websockets.Mvc.Models
{
    public class User : Entity
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public User(Guid guid, string name, string email) : base(guid)
        {
            Name = name;
            Email = email;
        }
    }
}

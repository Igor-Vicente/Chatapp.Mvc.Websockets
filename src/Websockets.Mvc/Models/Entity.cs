namespace Websockets.Mvc.Models
{
    public abstract class Entity
    {
        public Guid Id { get; private set; }

        //public ValidationResult ValidationResult { get; protected set; }

        public Entity()
        {
            Id = Guid.NewGuid();
        }
    }
}

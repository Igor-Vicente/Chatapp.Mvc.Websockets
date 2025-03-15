using System.Security.Claims;

namespace Websockets.Mvc.Configuration
{
    public interface IUserInjection
    {
        Guid GetUserId();
        string GetUserEmail();
        bool IsAuthenticated();
    }
    public class UserInjection : IUserInjection
    {
        private readonly IHttpContextAccessor _accessor;

        public UserInjection(IHttpContextAccessor httpContext)
        {
            _accessor = httpContext;
        }

        public bool IsAuthenticated()
        {
            return _accessor.HttpContext.User.Identity.IsAuthenticated;
        }

        public string GetUserEmail()
        {
            return IsAuthenticated() ? _accessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value : "";
        }

        public Guid GetUserId()
        {
            return IsAuthenticated() ? Guid.Parse(_accessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value) : Guid.Empty;
        }
    }
}

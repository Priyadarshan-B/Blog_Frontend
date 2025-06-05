using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blog_Web.Services
{
    public class AuthService
    {
        private readonly IJSRuntime _jsRuntime;

        public string UserId { get; private set; } = "";
        public string UserName { get; private set; } = "";
        public string Email { get; private set; } = "";
        public string ProfileImage { get; private set; } = "";
        public bool IsLoggedIn => !string.IsNullOrEmpty(UserId);

        public AuthService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "D!");
            if (!string.IsNullOrWhiteSpace(token))
            {
                var payload = await _jsRuntime.InvokeAsync<JwtPayload>("decodeJwt", token);
                UserId = payload.sub;
                Email = payload.email;
                UserName = payload.name ?? "";
                ProfileImage = payload.picture ?? "";
            }
        }

        public class JwtPayload
        {
            public string sub { get; set; } = "";
            public string email { get; set; } = "";
            public string? name { get; set; }
            public string? picture { get; set; }
        }
    }
}

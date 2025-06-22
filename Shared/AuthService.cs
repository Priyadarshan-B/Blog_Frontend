using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace Blog_Web.Services
{
    public class AuthService : IDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;

        public string UserId { get; private set; } = "";
        public string UserName { get; private set; } = "";
        public string Email { get; private set; } = "";
        public string ProfileImage { get; private set; } = "";

        public bool IsLoggedIn => !string.IsNullOrEmpty(UserId);

        public event Action? OnAuthStateChanged;

        public AuthService(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public async Task InitializeAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "D!");

            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var payload = await _jsRuntime.InvokeAsync<JwtPayload>("decodeJwt", token);

                    if (payload != null && !string.IsNullOrWhiteSpace(payload.sub))
                    {
                        UserId = payload.sub;
                        Email = payload.email;
                        UserName = payload.name ?? "";
                        ProfileImage = payload.picture ?? "";

                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        Console.WriteLine("AuthService: User state restored from token.");
                    }
                    else
                    {
                        Console.WriteLine("AuthService: Invalid or undecodable JWT token found in localStorage.");
                        await LogoutAsync();
                    }
                }
                catch (JSException ex)
                {
                    Console.Error.WriteLine($"AuthService: Error decoding JWT token: {ex.Message}");
                    await LogoutAsync();
                }
            }
            else
            {
                UserId = "";
                UserName = "";
                Email = "";
                ProfileImage = "";
                _httpClient.DefaultRequestHeaders.Authorization = null;
                Console.WriteLine("AuthService: No token found in localStorage.");
            }

            OnAuthStateChanged?.Invoke();
        }

        public async Task LoginUser(string jwtToken, JwtPayload payload)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "D!", jwtToken);

            UserId = payload.sub;
            Email = payload.email;
            UserName = payload.name ?? "";
            ProfileImage = payload.picture ?? "";

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            OnAuthStateChanged?.Invoke();
            Console.WriteLine("AuthService: User logged in and token saved.");
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "D!");
            UserId = "";
            UserName = "";
            Email = "";
            ProfileImage = "";
            _httpClient.DefaultRequestHeaders.Authorization = null;
            OnAuthStateChanged?.Invoke();
            Console.WriteLine("AuthService: User logged out.");
        }
        public void UpdateProfile(string displayName, string email, string profileImage)
        {
            UserName = displayName;
            Email = email;
            ProfileImage = profileImage;
        }
        public void SetUserName(string name) => UserName = name;
        public void SetEmail(string email) => Email = email;
        public void SetProfileImage(string imageUrl) => ProfileImage = imageUrl;

        public void Dispose()
        {
            // If AuthService subscribed to any events, unsubscribe here to prevent memory leaks.
            // For OnAuthStateChanged, components subscribe to AuthService, so no action needed here.
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
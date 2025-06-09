using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Net.Http; // Add this for HttpClient
using System;         // Add this for Action event

namespace Blog_Web.Services
{
    public class AuthService : IDisposable // Implement IDisposable for event cleanup
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient; // Inject HttpClient

        public string UserId { get; private set; } = "";
        public string UserName { get; private set; } = "";
        public string Email { get; private set; } = "";
        public string ProfileImage { get; private set; } = "";

        // IsLoggedIn status directly depends on UserId being populated
        public bool IsLoggedIn => !string.IsNullOrEmpty(UserId);

        // Event to notify components when the authentication state changes
        public event Action? OnAuthStateChanged;

        public AuthService(IJSRuntime jsRuntime, HttpClient httpClient) // Inject HttpClient here
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public async Task InitializeAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "D!"); // Get token by key "D!"

            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    // Assuming 'decodeJwt' is a JavaScript function that decodes the JWT
                    var payload = await _jsRuntime.InvokeAsync<JwtPayload>("decodeJwt", token);

                    // Validate payload to ensure it's not null and has a subject (sub)
                    if (payload != null && !string.IsNullOrWhiteSpace(payload.sub))
                    {
                        UserId = payload.sub;
                        Email = payload.email;
                        UserName = payload.name ?? "";
                        ProfileImage = payload.picture ?? "";

                        // Set the Authorization header for the HttpClient for subsequent API calls
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        Console.WriteLine("AuthService: User state restored from token.");
                    }
                    else
                    {
                        // Token found but invalid or couldn't be decoded properly
                        Console.WriteLine("AuthService: Invalid or undecodable JWT token found in localStorage.");
                        await LogoutAsync(); // Clear invalid token
                    }
                }
                catch (JSException ex) // Catch JavaScript errors during decoding
                {
                    Console.Error.WriteLine($"AuthService: Error decoding JWT token: {ex.Message}");
                    await LogoutAsync(); // Clear token if decoding fails
                }
            }
            else
            {
                // No token found, ensure everything is reset
                UserId = "";
                UserName = "";
                Email = "";
                ProfileImage = "";
                _httpClient.DefaultRequestHeaders.Authorization = null; // Clear any existing header
                Console.WriteLine("AuthService: No token found in localStorage.");
            }

            // Always invoke this after initialization to signal App.razor that state is ready.
            OnAuthStateChanged?.Invoke();
        }

        // Method to call after a successful login (from Login.razor)
        public async Task LoginUser(string jwtToken, JwtPayload payload)
        {
            // Save the token to localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "D!", jwtToken);

            // Populate user details from the decoded payload
            UserId = payload.sub;
            Email = payload.email;
            UserName = payload.name ?? "";
            ProfileImage = payload.picture ?? "";

            // Set the Authorization header for HttpClient
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            // Notify listeners (like App.razor) that authentication state has changed
            OnAuthStateChanged?.Invoke();
            Console.WriteLine("AuthService: User logged in and token saved.");
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "D!"); // Remove token from localStorage
            UserId = "";
            UserName = "";
            Email = "";
            ProfileImage = "";
            _httpClient.DefaultRequestHeaders.Authorization = null; // Clear authorization header
            OnAuthStateChanged?.Invoke(); // Notify listeners
            Console.WriteLine("AuthService: User logged out.");
        }

        public void Dispose()
        {
            // If AuthService subscribed to any events, unsubscribe here to prevent memory leaks.
            // For OnAuthStateChanged, components subscribe to AuthService, so no action needed here.
        }

        // Keep your JwtPayload class
        public class JwtPayload
        {
            public string sub { get; set; } = "";
            public string email { get; set; } = "";
            public string? name { get; set; }
            public string? picture { get; set; }
        }
    }
}
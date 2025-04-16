namespace Ops.Console
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Duende.IdentityModel;
    using Duende.IdentityModel.Client;
    using Microsoft.Extensions.Configuration;

    public sealed class AcmIdmService
    {
        private static readonly object LockObject = new object();
        private static AcmIdmService? _instance = null;

        private const string RequiredScopes =
            "dv_ar_adres_beheer dv_ar_adres_uitzonderingen dv_gr_geschetstgebouw_beheer dv_gr_geschetstgebouw_uitzonderingen dv_gr_ingemetengebouw_uitzonderingen";

        private readonly string? _tokenEndpoint;
        private readonly string? _clientId;
        private readonly string? _clientSecret;

        private AccessToken? _accessToken;

        private AcmIdmService(IConfiguration configuration)
        {
            _clientId = configuration.GetSection("AuthOptions").GetValue<string>("ClientId");
            _clientSecret = configuration.GetSection("AuthOptions").GetValue<string>("ClientSecret");
            _tokenEndpoint = configuration.GetSection("AuthOptions").GetValue<string>("TokenEndpoint");
        }

        public static AcmIdmService GetInstance(IConfiguration configuration)
        {
            if (_instance == null)
            {
                lock (LockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new AcmIdmService(configuration);
                    }
                }
            }

            return _instance;
        }

        public string GetAccessToken()
        {
            lock (LockObject)
            {
                if (_accessToken is not null && !_accessToken.IsExpired)
                {
                    return _accessToken.Token;
                }

                var tokenClient = new TokenClient(
                    () => new HttpClient(),
                    new TokenClientOptions
                    {
                        Address = _tokenEndpoint,
                        ClientId = _clientId!,
                        ClientSecret = _clientSecret,
                        Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", RequiredScopes) })
                    });

                var response = tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials).GetAwaiter().GetResult();

                _accessToken = new AccessToken(response.AccessToken!, response.ExpiresIn);

                return _accessToken.Token;
            }
        }

        private sealed class AccessToken
        {
            private readonly DateTime _expiresAt;

            public string Token { get; }

            // Let's regard it as expired 10 seconds before it actually expires.
            public bool IsExpired => _expiresAt < DateTime.Now.Add(TimeSpan.FromSeconds(10));

            public AccessToken(string token, int expiresIn)
            {
                _expiresAt = DateTime.Now.AddSeconds(expiresIn);
                Token = token;
            }
        }
    }
}

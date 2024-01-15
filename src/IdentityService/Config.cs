using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
	public static IEnumerable<IdentityResource> IdentityResources =>
		[
			new IdentityResources.OpenId(),
			new IdentityResources.Profile(),
		];

	public static IEnumerable<ApiScope> ApiScopes =>
		[
			new ApiScope("auctionApp", "Auctin app full access"),
		];

	public static IEnumerable<Client> Clients =>
		[
			new Client
			{
				ClientId = "postman",
				ClientName = "Postman",
				RedirectUris = { "https://www.getpostman.com/oauth2/callback" },
				ClientSecrets = { new Secret("NotASecret".Sha256()) },
				AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
				AllowedScopes = { "openid", "profile", "auctionApp" },
			},

			new Client
			{
				ClientId = "nextApp",
				ClientName = "nextApp",
				ClientSecrets = { new Secret("secret".Sha256()) },
				AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
				RedirectUris = { "https://localhost:3000/api/auth/callback/id-server" },
				RequirePkce = false,
				AllowOfflineAccess = true,
				AllowedScopes = { "openid", "profile", "auctionApp" },
				AccessTokenLifetime = 3600*24*30,
				AlwaysIncludeUserClaimsInIdToken = true,
			},
		];
}

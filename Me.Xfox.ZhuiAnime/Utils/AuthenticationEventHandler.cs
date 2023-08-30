using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;

namespace Me.Xfox.ZhuiAnime.Utils;

public class AuthenticationEventHandler : JwtBearerEvents
{
    public override async Task Challenge(JwtBearerChallengeContext context)
    {
        context.HandleResponse();

        ZhuiAnimeError err = context.AuthenticateFailure switch
        {
            ZhuiAnimeError e => e,
            null or Exception => new ZhuiAnimeError.InvalidToken(
                context.Error,
                context.ErrorDescription,
                context.AuthenticateFailure
            ),
        };
        err.WithHttpContext(context.HttpContext);

        // From: https://github.com/dotnet/aspnetcore/blob/9402bfac90a695bb732dff17ba624801076df77f/src/Security/Authentication/JwtBearer/src/JwtBearerHandler.cs#L296-L345
        #region original logic
        context.Response.StatusCode = 401;

        if (string.IsNullOrEmpty(context.Error) &&
            string.IsNullOrEmpty(context.ErrorDescription) &&
            string.IsNullOrEmpty(context.ErrorUri))
        {
            context.Response.Headers.Append(HeaderNames.WWWAuthenticate, context.Options.Challenge);
        }
        else
        {
            // https://tools.ietf.org/html/rfc6750#section-3.1
            // WWW-Authenticate: Bearer realm="example", error="invalid_token", error_description="The access token expired"
            var builder = new StringBuilder(context.Options.Challenge);
            if (context.Options.Challenge.IndexOf(' ') > 0)
            {
                // Only add a comma after the first param, if any
                builder.Append(',');
            }
            if (!string.IsNullOrEmpty(context.Error))
            {
                builder.Append(" error=\"");
                builder.Append(context.Error);
                builder.Append('\"');
            }
            if (!string.IsNullOrEmpty(context.ErrorDescription))
            {
                if (!string.IsNullOrEmpty(context.Error))
                {
                    builder.Append(',');
                }

                builder.Append(" error_description=\"");
                builder.Append(context.ErrorDescription);
                builder.Append('\"');
            }
            if (!string.IsNullOrEmpty(context.ErrorUri))
            {
                if (!string.IsNullOrEmpty(context.Error) ||
                    !string.IsNullOrEmpty(context.ErrorDescription))
                {
                    builder.Append(',');
                }

                builder.Append(" error_uri=\"");
                builder.Append(context.ErrorUri);
                builder.Append('\"');
            }

            context.Response.Headers.Append(HeaderNames.WWWAuthenticate, builder.ToString());
        }
        #endregion

        // Customized logic for JSON response
        if (context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new ZhuiAnimeError.ErrorDevResponse(err));
        }
        else
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new ZhuiAnimeError.ErrorProdResponse(err));
        }
    }

    protected Task Validate(TokenValidatedContext context)
    {
        if (context.Principal == null)
        {
            throw new ZhuiAnimeError.InvalidToken("za_no_principal", "no principal in token", null);
        }
        var subject = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
            throw new ZhuiAnimeError.InvalidToken("za_no_subject", "no subject in token", null);
        if (uint.TryParse(subject, out var _) == false)
        {
            throw new ZhuiAnimeError.InvalidToken("za_invalid_subject", "subject is not number", null);
        }
        var updatedAtStr = context.Principal.FindFirstValue(Services.TokenService.JwtClaimNames.UpdatedAt) ??
            throw new ZhuiAnimeError.InvalidToken("za_no_updated_at", "no updated_at in token", null);
        if (long.TryParse(updatedAtStr, out var _) == false)
        {
            throw new ZhuiAnimeError.InvalidToken("za_invalid_updated_at", "updated_at is not number", null);
        }
        return Task.CompletedTask;
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        try
        {
            await Validate(context);
        }
        catch (Exception e)
        {
            context.Fail(e);
        }
    }

    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        context.HttpContext.RequestServices.GetRequiredService<ILogger<AuthenticationEventHandler>>()
            .LogError(context.Exception, "Authentication failed");
        return Task.CompletedTask;
    }

    public override Task Forbidden(ForbiddenContext context)
    {
        context.HttpContext.RequestServices.GetRequiredService<ILogger<AuthenticationEventHandler>>()
            .LogError("Forbidden");
        return Task.CompletedTask;
    }
}

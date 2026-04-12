using MediatR;
using QPhising.Application.Features.Setup.GetSetupStatus;

namespace QPhising.API.Setup;

public sealed class SetupCompletionPreconditionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IMediator mediator)
    {
        if (!RequiresSetupCompletion(context.Request.Path))
        {
            await next(context);
            return;
        }

        var setupStatusResult = await mediator.Send(new GetSetupStatusQuery(), context.RequestAborted);
        bool isSetupCompleted = setupStatusResult.IsSuccess && setupStatusResult.Value?.IsCompleted == true;

        if (isSetupCompleted)
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status423Locked;
        await context.Response.WriteAsJsonAsync(new
        {
            title = "Setup Incomplete",
            detail = "System setup must be finalized before this endpoint can be used.",
            status = StatusCodes.Status423Locked,
            traceId = context.TraceIdentifier
        });
    }

    private static bool RequiresSetupCompletion(PathString requestPath)
    {
        if (!requestPath.StartsWithSegments("/api", out var remainingPath))
        {
            return false;
        }

        if (remainingPath.StartsWithSegments("/setup"))
        {
            return false;
        }

        if (remainingPath.StartsWithSegments("/v", out var versionPath))
        {
            return !versionPath.StartsWithSegments("/setup");
        }

        return true;
    }
}

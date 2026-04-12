using MediatR;
using QPhising.Application.Features.Setup.GetSetupStatus;

namespace QPhising.API.Setup;

public sealed class SetupCompletionPreconditionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IMediator mediator)
    {
        SetupPathState setupPathState = GetSetupPathState(context.Request.Path);
        if (setupPathState == SetupPathState.SetupStatusPath)
        {
            await next(context);
            return;
        }

        bool requiresSetupCompletion = setupPathState == SetupPathState.NotSetupPath && RequiresSetupCompletion(context.Request.Path);
        bool isSetupMutationPath = setupPathState == SetupPathState.SetupMutationPath;

        if (!requiresSetupCompletion && !isSetupMutationPath)
        {
            await next(context);
            return;
        }

        var setupStatusResult = await mediator.Send(new GetSetupStatusQuery(), context.RequestAborted);
        bool isSetupCompleted = setupStatusResult.IsSuccess && setupStatusResult.Value?.IsCompleted == true;

        if (isSetupMutationPath)
        {
            if (!isSetupCompleted)
            {
                await next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status423Locked;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Setup Already Completed",
                detail = "Setup mutation endpoints are locked after setup is finalized.",
                status = StatusCodes.Status423Locked,
                traceId = context.TraceIdentifier
            });
            return;
        }

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

    private static SetupPathState GetSetupPathState(PathString requestPath)
    {
        if (!requestPath.StartsWithSegments("/api", out var remainingPath))
        {
            return SetupPathState.NotSetupPath;
        }

        if (remainingPath.StartsWithSegments("/setup", out var setupPath))
        {
            return IsSetupStatusPath(setupPath) ? SetupPathState.SetupStatusPath : SetupPathState.SetupMutationPath;
        }

        if (remainingPath.StartsWithSegments("/v", out var versionPath) &&
            versionPath.StartsWithSegments("/setup", out var versionedSetupPath))
        {
            return IsSetupStatusPath(versionedSetupPath) ? SetupPathState.SetupStatusPath : SetupPathState.SetupMutationPath;
        }

        return SetupPathState.NotSetupPath;
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

    private static bool IsSetupStatusPath(PathString setupPath) => setupPath.StartsWithSegments("/status");

    private enum SetupPathState
    {
        NotSetupPath = 0,
        SetupStatusPath = 1,
        SetupMutationPath = 2
    }
}

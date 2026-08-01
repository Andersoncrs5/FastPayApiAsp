namespace App.Config.Tx;

public sealed class TransactionalMiddleware(IRequestDbContext db) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await db.BeginAsync(context.RequestAborted);

        try
        {
            await next(context);
            
            if (context.Response.StatusCode >= 400)
            {
                await db.RollbackAsync(context.RequestAborted);
                return;
            }

            await db.CommitAsync(context.RequestAborted);
        }
        catch
        {
            await db.RollbackAsync(context.RequestAborted);
            throw;
        }
    }
}
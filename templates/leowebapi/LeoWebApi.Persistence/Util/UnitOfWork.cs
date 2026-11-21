using System.Data;
using LeoWebApi.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LeoWebApi.Persistence.Util;

public interface ITransactionProvider : IAsyncDisposable, IDisposable
{
    public Task BeginTransactionAsync();
    public Task CommitAsync();
    public Task RollbackAsync();
}

public static class TransactionProviderExtensions
{
    // an extension instead of an interface member, so a mocked ITransactionProvider still passes the result through
    extension(ITransactionProvider transaction)
    {
        /// <summary>
        ///     Commits the current transaction and then returns <paramref name="result" />, so committing fits
        ///     into a single switch expression arm
        /// </summary>
        public async Task<T> CommitAndReturnAsync<T>(T result)
        {
            await transaction.CommitAsync();

            return result;
        }
    }
}

public interface IUnitOfWork
{
    public IRocketRepository RocketRepository { get; }
    public Task SaveChangesAsync();
}

internal sealed class UnitOfWork(DatabaseContext context, ILogger<UnitOfWork> logger)
    : IUnitOfWork, ITransactionProvider
{
    private IDbContextTransaction? _transaction;
    
    public IRocketRepository RocketRepository => new RocketRepository(context.Rockets);

    public async Task BeginTransactionAsync()
    {
        if (_transaction is not null)
        {
            throw new TransactionException("Transaction already started, unable to start another");
        }

        _transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Snapshot);
    }

    public async Task CommitAsync()
    {
        if (_transaction is null)
        {
            throw new TransactionException("No transaction started, unable to commit");
        }

        await _transaction.CommitAsync();
        _transaction = null;
    }

    public async Task RollbackAsync()
    {
        if (_transaction is null)
        {
            throw new TransactionException("No transaction started, unable to rollback");
        }

        await _transaction.RollbackAsync();
        _transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        // Transaction was neither committed nor rolled back, rolling back now - silent, this is acceptable
        var transaction = _transaction;
        _transaction = null;                 
        await transaction.RollbackAsync();
        await transaction.DisposeAsync();
    }

    public void Dispose()
    {
        if (_transaction is null)
        {
            return;
        }

        logger
            .LogWarning($"Transaction was not disposed in {nameof(DisposeAsync)} and will now be rolled back and disposed in {nameof(Dispose)}");
        _transaction.Rollback();
        _transaction.Dispose();
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();

    private sealed class TransactionException(string message) : Exception(message);
}

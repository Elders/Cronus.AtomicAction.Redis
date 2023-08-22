using System;
using System.Threading.Tasks;
using Elders.Cronus.Userfull;

namespace Elders.Cronus.AtomicAction.Redis.RevisionStore
{
    /// <summary>
    /// Provides storage for aggregate revisions.
    /// </summary>
    public interface IRevisionStore
    {
        Task<Result<int>> PrepareRevisionAsync(string resource, int revision);

        Task<Result<bool>> SaveRevisionAsync(string resource, int revision, TimeSpan expiry);
    }
}

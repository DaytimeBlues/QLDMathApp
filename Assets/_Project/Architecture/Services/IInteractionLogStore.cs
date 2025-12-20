using System.Collections.Generic;

namespace QLDMathApp.Architecture.Services
{
    /// <summary>
    /// INTERFACE: Abstraction for storing pedagogical interaction logs.
    /// Allows swapping between JSON, SQLite, or Cloud storage without affecting DataService.
    /// </summary>
    public interface IInteractionLogStore
    {
        void SaveLog(InteractionLog log);
        List<InteractionLog> GetAllLogs();
        void ClearLogs();
    }
}

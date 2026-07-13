using Microsoft.EntityFrameworkCore;

namespace Marky.Framework.Persistence.EntityFramework.Outbox
{
    public class OutboxRedirectState
    {
        public DbContext? PrimaryContext { get; private set; }

        public void EnableRedirect(DbContext primaryContext) => PrimaryContext = primaryContext;

        public void DisableRedirect() => PrimaryContext = null;
    }
}

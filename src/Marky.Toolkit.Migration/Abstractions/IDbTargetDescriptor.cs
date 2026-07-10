using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marky.Toolkit.Migration.Abstractions;

public interface IDbTargetDescriptor
{
    public string TargetKey { get; }
    public Type DbContextType { get; }
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}

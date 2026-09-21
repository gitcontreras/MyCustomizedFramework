using System.Data;

namespace MyCustomizedFramework.Application.Abstractions.Persistence;

/// <summary>
/// Supplies ADO.NET connections to the host application's own configured database. Every generated
/// repository (see the CRUD generator) depends on this instead of a concrete SqlConnection/NpgsqlConnection/etc.,
/// so the app wires it up once, pointing at whichever single database it actually runs against.
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

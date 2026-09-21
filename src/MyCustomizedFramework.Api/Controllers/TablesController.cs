using Microsoft.AspNetCore.Mvc;
using MyCustomizedFramework.Api.Common;
using MyCustomizedFramework.Api.Contracts.Tables;
using MyCustomizedFramework.Application.SchemaExplorer;

namespace MyCustomizedFramework.Api.Controllers;

[ApiController]
[Route("api/tables")]
public sealed class TablesController(GetTablesHandler getTablesHandler) : ControllerBase
{
    /// <summary>
    /// Connects at runtime to the database described by the request and lists its tables.
    /// POST is used (instead of GET) because the connection string is sensitive and must not travel in the query string.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<IReadOnlyCollection<TableResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTables(
        [FromBody] GetTablesRequest request,
        CancellationToken cancellationToken)
    {
        var queryResult = TablesMapper.ToQuery(request);
        if (queryResult.IsFailure)
        {
            return queryResult.ToProblem();
        }

        var result = await getTablesHandler.HandleAsync(queryResult.Value, cancellationToken);

        return result.ToActionResult(tables => tables.Select(TablesMapper.ToResponse).ToArray());
    }
}

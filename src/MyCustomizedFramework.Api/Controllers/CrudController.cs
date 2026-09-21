using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using MyCustomizedFramework.Api.Common;
using MyCustomizedFramework.Api.Contracts.Crud;
using MyCustomizedFramework.Application.CrudGeneration;

namespace MyCustomizedFramework.Api.Controllers;

[ApiController]
[Route("api/crud")]
public sealed class CrudController(GenerateCrudHandler generateCrudHandler) : ControllerBase
{
    /// <summary>
    /// Connects at runtime to the database described by the request, reads the given table's column
    /// metadata, and returns a ZIP with the generated CRUD scaffolding (entity, DTOs, repository, service).
    /// The API never writes these files to disk itself - the caller decides where to extract the ZIP.
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateCrudRequest request,
        CancellationToken cancellationToken)
    {
        var queryResult = CrudMapper.ToQuery(request);
        if (queryResult.IsFailure)
        {
            return queryResult.ToProblem();
        }

        var result = await generateCrudHandler.HandleAsync(queryResult.Value, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in result.Value)
            {
                var entry = archive.CreateEntry(file.RelativePath, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await using var writer = new StreamWriter(entryStream);
                await writer.WriteAsync(file.Content);
            }
        }

        return File(zipStream.ToArray(), "application/zip", $"{request.TableName}-crud.zip");
    }
}

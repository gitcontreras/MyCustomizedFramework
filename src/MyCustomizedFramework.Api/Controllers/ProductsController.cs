using Microsoft.AspNetCore.Mvc;
using MyCustomizedFramework.Application.Products;

namespace MyCustomizedFramework.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    CreateProductHandler createProductHandler,
    ListProductsHandler listProductsHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<ProductDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ProductDto>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await listProductsHandler.HandleAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await createProductHandler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(List), new { id = product.Id }, product);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid product",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}

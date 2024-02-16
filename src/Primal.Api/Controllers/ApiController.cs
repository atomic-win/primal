using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Primal.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public abstract class ApiController : ControllerBase
{
	protected ActionResult Problem(IReadOnlyList<Error> errors)
	{
		if (errors.Count is 0)
		{
			return this.Problem();
		}

		if (errors.All(error => error.Type == ErrorType.Validation))
		{
			return this.ValidationProblem(errors);
		}

		return this.Problem(errors[0]);
	}

	private ObjectResult Problem(Error error)
	{
		var statusCode = error.Type switch
		{
			ErrorType.Conflict => StatusCodes.Status409Conflict,
			ErrorType.Validation => StatusCodes.Status400BadRequest,
			ErrorType.NotFound => StatusCodes.Status404NotFound,
			ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
			_ => StatusCodes.Status500InternalServerError,
		};

		return this.Problem(statusCode: statusCode, title: error.Description);
	}

	private ActionResult ValidationProblem(IReadOnlyList<Error> errors)
	{
		var modelStateDictionary = new ModelStateDictionary();

		foreach (var error in errors)
		{
			modelStateDictionary.AddModelError(error.Code, error.Description);
		}

		return this.ValidationProblem(modelStateDictionary);
	}
}

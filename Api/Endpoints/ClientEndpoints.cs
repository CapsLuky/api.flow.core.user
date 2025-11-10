using Api.Extensions;
using Core.Client.Models;
using Core.Client.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class ClientEndpoints
{
    
    
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder endpoints)
    {
        
        var users = endpoints.MapGroup("/client/user").WithTags("Client");

        users.MapGet("/", GetUsers);
        users.MapGet("/{id}", GetUser);
        users.MapPost("/", MockCreateUser);


        return endpoints;
    }

    [EndpointDescription("Busca todos os usuários")]
    [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    private static async Task<IResult> GetUsers(
        [FromServices] IUserService userService)
    {
        try
        {
            var users = await userService.GetUsers();
            return TypedResults.Ok(users);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                title: "Erro interno do servidor",
                detail: "Ocorreu um erro ao buscar os usuários",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [EndpointDescription("Busca um usuário pelo seu id")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    private static async Task<IResult> GetUser(
        [FromServices] IUserService userService,
        [FromRoute] string id)
    {
        try
        {
            return Results.Ok();
            // Validação básica do ID
            if (string.IsNullOrWhiteSpace(id))
            {
                return TypedResults.BadRequest(
                    new ProblemDetails
                    {
                        Title = "ID inválido",
                        Detail = "O ID do usuário não pode ser nulo ou vazio",
                        Status = StatusCodes.Status400BadRequest
                    });
            }

            var user = await userService.GetUserById(id);

            return TypedResults.Ok(user);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(
                new ProblemDetails
                {
                    Title = "Argumento inválido",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                title: "Erro interno do servidor",
                detail: "Ocorreu um erro ao buscar o usuário",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
    
    private static async Task<IResult> MockCreateUser(
        HttpContext httpContext,
        [FromServices] IUserService userService,
        [FromBody] User user,
        [FromServices] IValidator<User> validator)
    {
        return Results.Ok();
        var vr = await validator.ValidateAsync(user);
        if (!vr.IsValid)
            return Results.ValidationProblem(vr.ToDictionary());
        
        var created = await userService.MockCreateUserAsync(user);
        return created.ToIResult(httpContext);
    }

}
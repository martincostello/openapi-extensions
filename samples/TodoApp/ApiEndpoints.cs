// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using MartinCostello.OpenApi;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp;

/// <summary>
/// A class containing the HTTP endpoints for the Todo API.
/// </summary>
public static class ApiEndpoints
{
    /// <summary>
    /// Adds the services for the Todo API to the application.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>
    /// A <see cref="IServiceCollection"/> that can be used to further configure the application.
    /// </returns>
    public static IServiceCollection AddTodoApi(this IServiceCollection services)
    {
        // Configure dependencies for the business logic of the API
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<ITodoService, TodoService>();

        // Configure an EFCore data context to store the Todos backed by SQLite
        services.AddDbContext<TodoContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var dataDirectory = configuration["DataDirectory"];

            if (string.IsNullOrEmpty(dataDirectory) || !Path.IsPathRooted(dataDirectory))
            {
                var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
                dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
            }

            // Ensure the configured data directory exists
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            var databaseFile = Path.Combine(dataDirectory, "TodoApp.db");

            options.UseSqlite("Data Source=" + databaseFile);
        });

        // Configure JSON source generation for the Todo API
        services.ConfigureHttpJsonOptions((options) =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Add(TodoJsonSerializerContext.Default);
        });

        return services;
    }

    /// <summary>
    /// Maps the endpoints for the Todo API.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <returns>
    /// A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    public static IEndpointRouteBuilder MapTodoApiRoutes(this IEndpointRouteBuilder builder)
    {
        // Configure a group for the resources of the Todo API
        var group = builder.MapGroup("/api/items")
                           .WithTags("TodoApp");
        {
            group.MapGet("/", async (ITodoService service, CancellationToken cancellationToken) => await service.GetListAsync(cancellationToken))
                 .WithName("ListTodos")
                 .WithSummary("Get all Todo items")
                 .WithDescription("Gets all of the current user's todo items.")
                 .Produces<TodoListViewModel>(StatusCodes.Status200OK)
                 .ProducesOpenApiResponse(StatusCodes.Status200OK, "The Todo items.");

            group.MapGet(
                "/{id}",
                async (
                    [Description("The Todo item's ID.")] Guid id,
                    ITodoService service,
                    CancellationToken cancellationToken) =>
                {
                    var model = await service.GetAsync(id, cancellationToken);
                    return model switch
                    {
                        null => Results.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                        _ => Results.Ok(model),
                    };
                })
                .WithName("GetTodo")
                .WithSummary("Get a specific Todo item")
                .WithDescription("Gets the todo item with the specified ID.")
                .Produces<TodoItemModel>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesOpenApiResponse(StatusCodes.Status200OK, "The Todo item was found.")
                .ProducesOpenApiResponse(StatusCodes.Status404NotFound, "The Todo item was not found.");

            group.MapPost(
                "/",
                async (
                    [Description("The Todo item to create.")] CreateTodoItemModel model,
                    ITodoService service,
                    CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(model.Text))
                    {
                        return Results.Problem("No item text specified.", statusCode: StatusCodes.Status400BadRequest);
                    }

                    var id = await service.AddItemAsync(model.Text, cancellationToken);

                    return Results.Created($"/api/items/{id}", new CreatedTodoItemModel() { Id = id });
                })
                .WithName("CreateTodo")
                .WithSummary("Create a new Todo item")
                .WithDescription("Creates a new todo item for the current user and returns its ID.")
                .Produces<CreatedTodoItemModel>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesOpenApiResponse(StatusCodes.Status201Created, "The created Todo item.")
                .ProducesOpenApiResponse(StatusCodes.Status400BadRequest, "The Todo item could not be created.");

            group.MapPost(
                "/{id}/complete",
                async (
                    [Description("The Todo item's ID.")] Guid id,
                    ITodoService service,
                    CancellationToken cancellationToken) =>
                {
                    var wasCompleted = await service.CompleteItemAsync(id, cancellationToken);

                    return wasCompleted switch
                    {
                        true => Results.NoContent(),
                        false => Results.Problem("Item already completed.", statusCode: StatusCodes.Status400BadRequest),
                        _ => Results.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                    };
                })
                .WithName("CompleteTodo")
                .WithSummary("Mark a Todo item as completed")
                .WithDescription("Marks the todo item with the specified ID as complete.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesOpenApiResponse(StatusCodes.Status204NoContent, "The Todo item was completed.")
                .ProducesOpenApiResponse(StatusCodes.Status400BadRequest, "The Todo item could not be completed.")
                .ProducesOpenApiResponse(StatusCodes.Status404NotFound, "The Todo item was not found.");

            group.MapDelete(
                "/{id}",
                async (
                    [Description("The Todo item's ID.")] Guid id,
                    ITodoService service,
                    CancellationToken cancellationToken) =>
                {
                    var wasDeleted = await service.DeleteItemAsync(id, cancellationToken);
                    return wasDeleted switch
                    {
                        true => Results.NoContent(),
                        false => Results.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                    };
                })
                .WithName("DeleteTodo")
                .WithSummary("Delete a Todo item")
                .WithDescription("Deletes the todo item with the specified ID.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesOpenApiResponse(StatusCodes.Status204NoContent, "The Todo item was deleted.")
                .ProducesOpenApiResponse(StatusCodes.Status404NotFound, "The Todo item was not found.");
        }

        // Redirect to OpenAPI (SwaggerUI) documentation
        builder.MapGet("/", () => Results.Redirect("/swagger-ui/index.html"))
               .ExcludeFromDescription();

        return builder;
    }
}

// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using TodoApp;

// Create the default web application builder
var builder = WebApplication.CreateBuilder(args);

// Configure the Todo repository and associated services
builder.AddTodoApp();

// Create the app
var app = builder.Build();

// Use TodoApp middleware and endpoints with the web application
app.UseTodoApp();

// Run the application
app.Run();

namespace TodoApp
{
    public partial class Program
    {
        // Expose the Program class for use with WebApplicationFactory<T>
    }
}

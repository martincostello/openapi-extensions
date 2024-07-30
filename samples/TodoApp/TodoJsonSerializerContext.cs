// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Models;

namespace TodoApp;

/// <summary>
/// A class that provides metadata for (de)serializing JSON for both the API endpoints and with OpenAPI.
/// </summary>
[JsonSerializable(typeof(CreateTodoItemModel))]
[JsonSerializable(typeof(CreatedTodoItemModel))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(TodoItemModel))]
[JsonSerializable(typeof(TodoListViewModel))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
public sealed partial class TodoJsonSerializerContext : JsonSerializerContext;

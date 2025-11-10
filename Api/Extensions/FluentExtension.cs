using System.Collections.Immutable;
using Core.Generic.Models;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class FluentExtension
{
    public static void ToBadRequest(this ValidationResult validationResult, HttpContent context, string errorTitle = "Request Error")
    {
        
    }
}
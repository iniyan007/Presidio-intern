using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var response = new 
            {
                success = false,
                message = exception.Message,
                details = exception.StackTrace, // In production, we might want to hide this, but keeping it for development
                errorCode = GetErrorCode(exception)
            };

            context.Result = new ObjectResult(response)
            {
                StatusCode = GetStatusCode(exception)
            };

            context.ExceptionHandled = true;
        }

        private int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                KeyNotFoundException => 404,
                UnauthorizedAccessException => 401,
                ValidationException => 400,
                ArgumentException => 400,
                InvalidOperationException => 400, // Or 409 Conflict, but sticking to 400 for bad business logic generally
                _ => 500
            };
        }

        private string GetErrorCode(Exception exception)
        {
            return exception switch
            {
                KeyNotFoundException => "NOT_FOUND",
                UnauthorizedAccessException => "UNAUTHORIZED",
                ValidationException => "VALIDATION_ERROR",
                ArgumentException => "BAD_REQUEST",
                InvalidOperationException => "INVALID_OPERATION",
                _ => "INTERNAL_SERVER_ERROR"
            };
        }
    }
}

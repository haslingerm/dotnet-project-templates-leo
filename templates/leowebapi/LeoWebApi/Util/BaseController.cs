using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace LeoWebApi.Util;

/// <summary>
///     Base controller class that provides common functionality (e.g. validation) for API controllers
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    ///     Validates the given request with the specified validator
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <typeparam name="TValidator">The validator class to use, has to have a parameterless constructor</typeparam>
    /// <typeparam name="TRequest">The request class</typeparam>
    /// <returns>True if the request object passed validation; false otherwise</returns>
    protected static bool ValidateRequest<TValidator, TRequest>(TRequest request)
    where TValidator : AbstractValidator<TRequest>, new()
    where TRequest : notnull =>
    ValidateRequestInternal<TValidator, TRequest>(request, false, out _);
    
    /// <summary>
    ///     Validates the given request with the specified validator and provides the validation errors
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="validationErrors">If validation errors occur, information about those is stored in this out parameter</param>
    /// <typeparam name="TValidator">The validator class to use, has to have a parameterless constructor</typeparam>
    /// <typeparam name="TRequest">The request class</typeparam>
    /// <returns>True if the request object passed validation; false otherwise</returns>
    protected static bool ValidateRequest<TValidator, TRequest>(TRequest request, out string[]? validationErrors)
    where TValidator : AbstractValidator<TRequest>, new()
    where TRequest : notnull =>
    ValidateRequestInternal<TValidator, TRequest>(request, true, out validationErrors);
    
    /// <summary>
    ///     Validates the given request with the provided validator
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="validator">The validator instance to use</param>
    /// <typeparam name="TValidator">The validator class</typeparam>
    /// <typeparam name="TRequest">The request class</typeparam>
    /// <returns>True if the request object passed validation; false otherwise</returns>
    protected static bool ValidateRequest<TValidator, TRequest>(TRequest request, TValidator validator)
    where TValidator : AbstractValidator<TRequest>
    where TRequest : notnull =>
    ValidateRequestInternal(request, validator, false, out _);
    
    /// <summary>
    ///     Validates the given request with the provided validator and also provides the validation errors
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="validator">The validator instance to use</param>
    /// <param name="validationErrors">If validation errors occur, information about those is stored in this out parameter</param>
    /// <typeparam name="TValidator">The validator class</typeparam>
    /// <typeparam name="TRequest">The request class</typeparam>
    /// <returns>True if the request object passed validation; false otherwise</returns>
    protected static bool ValidateRequest<TValidator, TRequest>(TRequest request, TValidator validator,
                                                                out string[]? validationErrors)
    where TValidator : AbstractValidator<TRequest>
    where TRequest : notnull =>
    ValidateRequestInternal(request, validator, true, out validationErrors);
    
    private static bool ValidateRequestInternal<TValidator, TRequest>(TRequest request, bool provideErrors,
                                                                      out string[]? validationErrors)
    where TValidator : AbstractValidator<TRequest>, new()
    where TRequest : notnull
    {
        var validator = new TValidator();
        
        return ValidateRequestInternal(request, validator, provideErrors, out validationErrors);
    }
    
    private static bool ValidateRequestInternal<TValidator, TRequest>(TRequest request, TValidator validator,
                                                                      bool provideErrors,
                                                                      out string[]? validationErrors)
    where TValidator : AbstractValidator<TRequest>
    where TRequest : notnull
    {
        var valRes = validator.Validate(request);
        if (valRes.IsValid)
        {
            validationErrors = null;
            
            return true;
        }
        
        validationErrors = provideErrors ? FormatValidationErrors(valRes.Errors) : null;
        
        return false;
        
        static string[] FormatValidationErrors(IEnumerable<ValidationFailure> errors)
        {
            return errors.Select(FormatValidationError).ToArray();
            
            static string FormatValidationError(ValidationFailure error) =>
            $"{error.PropertyName}: {error.ErrorMessage}";
        }
    }
}

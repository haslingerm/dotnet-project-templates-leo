using FluentValidation;

namespace LeoGRpcApi.Shared.ApiContract.Validation;

public static class ValidationSetup
{
    /// <summary>
    ///     Aligns FluentValidation with the protobuf contract
    /// </summary>
    public static void ConfigureValidation()
    {
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) => member is null
            ? null
            : ValidationNaming.ToPropertyName(member.Name);

        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member is null
            ? null
            : ValidationNaming.ToDisplayName(member.Name);
    }
}

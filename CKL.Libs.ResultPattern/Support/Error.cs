#pragma warning disable IDE0130
namespace CKL.Libs.ResultPattern;
#pragma warning restore IDE0130


/// <summary>
/// Lightweight error value that implicitly converts to <see cref="Result"/> or <see cref="Result{T}"/>.
/// The idiomatic pattern for returning failures: <c>return Result.Error("Something went wrong");</c>
/// </summary>
public readonly record struct Error(string ErrorMessage, Exception? Exception = null) : IError<Error>
{
    /// <inheritdoc />
    public static implicit operator Error(Exception exception) => new(ResultHelper.FormatExceptionMessage(exception), exception);
}

using NUnit.Framework;

namespace Ckl.Libs.ResultPattern.Tests;

[TestFixture]
public sealed class ResultToStringTests
{
    // ── Result.ToString() — Success ───────────────────────────────────────────

    [Test]
    public void Result_Success_ToString_ReturnsSuccess()
    {
        Assert.That(Result.Success.ToString(), Is.EqualTo("[Success]"));
    }

    // ── Result.ToString() — Failure ───────────────────────────────────────────

    [Test]
    public void Result_Fail_ToString_StartsWithFailed()
    {
        Assert.That(Result.Fail("something went wrong").ToString(), Does.StartWith("[Failed]"));
    }

    [Test]
    public void Result_Fail_ToString_ContainsErrorMessage()
    {
        Assert.That(Result.Fail("something went wrong").ToString(), Does.Contain("something went wrong"));
    }

    [Test]
    public void Result_Fail_ToString_ContainsCallStack()
    {
        Assert.That(Result.Fail("error").ToString(), Does.Contain("  at "));
    }

    [Test]
    public void Result_Fail_WithException_ToString_ContainsExceptionDetails()
    {
        var ex = new InvalidOperationException("invalid operation");
        Assert.That(Result.Fail("error", ex).ToString(), Does.Contain("Exception details:"));
    }

    [Test]
    public void Result_Fail_WithException_ToString_ContainsExceptionMessage()
    {
        var ex = new InvalidOperationException("invalid operation");
        Assert.That(Result.Fail("error", ex).ToString(), Does.Contain("invalid operation"));
    }

    [Test]
    public void Result_Fail_WithoutException_ToString_DoesNotContainExceptionDetails()
    {
        Assert.That(Result.Fail("error").ToString(), Does.Not.Contain("Exception details:"));
    }

    // ── Result<T>.ToString() — Success ────────────────────────────────────────

    [Test]
    public void ResultGeneric_Success_ToString_ContainsSuccess()
    {
        Result<int> result = 42;
        Assert.That(result.ToString(), Does.StartWith("[Success]"));
    }

    [Test]
    public void ResultGeneric_Success_ToString_ContainsValue()
    {
        Result<int> result = 42;
        Assert.That(result.ToString(), Does.Contain("42"));
    }

    [Test]
    public void ResultGeneric_Success_String_ToString_ContainsValue()
    {
        Result<string> result = "hello";
        Assert.That(result.ToString(), Does.Contain("hello"));
    }

    [Test]
    public void ResultGeneric_Success_NullValue_ToString_ReturnsSuccess()
    {
        Result<string?> result = Result<string?>.Success(null);
        Assert.That(result.ToString(), Is.EqualTo("[Success]"));
    }

    // ── Result<T>.ToString() — Failure ────────────────────────────────────────

    [Test]
    public void ResultGeneric_Fail_ToString_StartsWithFailed()
    {
        Assert.That(Result<int>.Fail("something went wrong").ToString(), Does.StartWith("[Failed]"));
    }

    [Test]
    public void ResultGeneric_Fail_ToString_ContainsErrorMessage()
    {
        Assert.That(Result<int>.Fail("something went wrong").ToString(), Does.Contain("something went wrong"));
    }

    [Test]
    public void ResultGeneric_Fail_ToString_ContainsCallStack()
    {
        Assert.That(Result<int>.Fail("error").ToString(), Does.Contain("  at "));
    }

    [Test]
    public void ResultGeneric_Fail_WithException_ToString_ContainsExceptionDetails()
    {
        var ex = new InvalidOperationException("invalid operation");
        Assert.That(Result<int>.Fail("error", ex).ToString(), Does.Contain("Exception details:"));
    }

    [Test]
    public void ResultGeneric_Fail_WithoutException_ToString_DoesNotContainExceptionDetails()
    {
        Assert.That(Result<int>.Fail("error").ToString(), Does.Not.Contain("Exception details:"));
    }
}

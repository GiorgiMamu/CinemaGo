using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using System.Reflection;
using System.Text.Json;
namespace CinemaGo.Tests;

public class ErrorCatalogTests
{
    [Fact]
    public void Every_error_has_a_message_and_a_unique_Area_Name_code()
    {
        var errors = typeof(Errors).GetNestedTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.FieldType == typeof(Error))
            .Select(f => (Error)f.GetValue(null)!)
            .ToList();

        Assert.NotEmpty(errors);
        Assert.All(errors, e => Assert.False(string.IsNullOrWhiteSpace(e.Message)));
        Assert.All(errors, e => Assert.Contains('.', e.Code));
        Assert.Equal(errors.Count, errors.Select(e => e.Code).Distinct().Count());
    }
    [Fact]
    public void Failure_response_uses_the_standard_envelope()
    {
        var response = ApiResponse<object>.Fail("msg", "Booking.SeatTaken", "trace-1");

        Assert.False(response.Success);
        Assert.Null(response.Data);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Contains("\"success\":false", json);
        Assert.Contains("\"data\":null", json);
        Assert.Contains("\"code\":\"Booking.SeatTaken\"", json);
        Assert.DoesNotContain("\"errors\"", json);   // no field errors, so the property is omitted
    }
}
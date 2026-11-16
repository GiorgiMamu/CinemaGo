using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class PageRequest
{
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.RangeBetween)] public int Page { get; set; } = 1;
    [Range(1, 100, ErrorMessage = ValidationMessages.RangeBetween)] public int PageSize { get; set; } = 10;
}
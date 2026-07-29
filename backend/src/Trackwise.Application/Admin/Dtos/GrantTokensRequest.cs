using System.ComponentModel.DataAnnotations;

namespace Trackwise.Application.Admin.Dtos;

public class GrantTokensRequest
{
    [Required, Range(1, 1000)]
    public int Count { get; set; }
}
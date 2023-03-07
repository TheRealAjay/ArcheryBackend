using System.ComponentModel.DataAnnotations;

namespace ArcheryBackend.Request;

public class CreateEventRequest
{
    [Required] public string Name { get; set; } = null!;
    [Required] public string Street { get; set; } = null!;
    [Required] public string Zip { get; set; } = null!;
    [Required] public string City { get; set; } = null!;
    [Required] public string Date { get; set; } = null!;
    [Required] public string Time { get; set; } = null!;
    [Required] public string UserEmail { get; set; } = null!;
}
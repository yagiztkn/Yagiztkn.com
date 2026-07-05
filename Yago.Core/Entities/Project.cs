using System.ComponentModel.DataAnnotations;

public class Project
{
    public int ID { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string ShortDescription { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? FullDescription { get; set; }

    [Required, MaxLength(500)]
    public string Technologies { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? GitHubLink { get; set; }

    [MaxLength(500)]
    public string? LiveLink { get; set; }

    public DateTime CreatedDate { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
}
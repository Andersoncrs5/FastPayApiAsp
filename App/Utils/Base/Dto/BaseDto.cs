namespace App.Utils.Base.Dto;

public class BaseDto
{
    public long Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
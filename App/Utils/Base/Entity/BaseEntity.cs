namespace App.Utils.Base.Entity;

public class BaseEntity
{
    public long Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
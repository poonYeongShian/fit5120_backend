namespace Deploy.DTOs;

public class ErrorResponseDto
{
    public required string ErrorCode { get; set; }
    public Dictionary<string, object?>? Details { get; set; }
}

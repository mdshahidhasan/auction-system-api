namespace AuctionSystem.Core.Models;

public class ServiceResult
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalCount { get; set; }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }
}

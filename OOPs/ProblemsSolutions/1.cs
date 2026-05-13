public class Order
{
    public int Id{get;set;}
    public decimal Total{get;set;}
}

public interface IReportFormatter
{
    string Format(Order order);
} 

public class ReportFormatter : IReportFormatter
{
    public string Format(Order order)
    {
        return $"Order: {order.Id}, Total:{order.Total}";
    }
}

public interface IReportStorage
{
    void Save(string data);
}

public class ReportStorage : IReportStorage
{
    public void Save(string data)
    {
        File.WriteAllText("report.txt",data);
    }
}

public interface IReportMessageService
{
    void Send(string data);
}

public class ReportMessageService : IReportMessageService
{

    public void Send(string data)
    {
        //smtp logic
    }
}

public interface IReportService
{
    public void GenerateReport(Order order);
}

public class ReportService : IReportService
{
    private readonly IReportFormatter _formatter;
    private readonly IReportStorage _storage;
    private readonly IReportMessageService _messageService;

    public ReportService(
        IReportFormatter formatter,
        IReportStorage storage,
        IReportMessageService messageService
    )
    {
        _formatter = formatter;
        _storage = storage;
        _messageService = messageService;
    }

    public void GenerateReport(Order order)
    {
        var data = _formatter.Format(order);
        _storage.Save(data);
        _messageService.Send(data);
    }
}
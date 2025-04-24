public class WorkLogPair
{
    public int EmployeeId { get; set; }
    public string AFM { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? EntryTime { get; set; }
    public TimeSpan? ExitTime { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public int Id { get; set; } // σειριακός αριθμός
    public bool IsSelected { get; set; } // για το τικ


}

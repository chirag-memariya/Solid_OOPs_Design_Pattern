
using System.Security.Principal;

namespace Type;

record Product(string Name, int CategoryID);
record Category(string Name,int ID);

// public record Student(string First,string Last,int ID,int[] Scores);







public enum GradeLevel
{
    FirstYear = 1,
    SecondYear,
    ThirdYear,
    FourthYear
};

public class Student(string fname,string lname,int id,GradeLevel year,List<int> score,int DepartmentId)
{
    public string FirstName { get; init; } = fname;
    public string LastName { get; init; } = lname;
    public int ID { get; init; } = id;

    public GradeLevel Year { get; init; } = year;
    public List<int> Scores { get; init; } = score;

    public int DepartmentID { get; init; } = DepartmentId;

}

public class Teacher(string first,string last,int id,string city)
{
    public string First { get; init; } = first;
    public string Last { get; init; } = last;
    public int ID { get; init; } = id;
    public string City { get; init; } = city;
}

public class Department(string name,int id,int teacherid)
{
    public  string Name { get; init; } = name;
    public int ID { get; init; } = id;
    public  int TeacherID { get; init; } = teacherid;
}


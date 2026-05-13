using Microsoft.VisualBasic;
using Type;

// string[] words = ["the","quick","brown","fox","jumps","in","ok","oo","four"];

// IEnumerable<IGrouping<int, string>> query = from word in words
//                                             group word by word.Length into wordGroup
//                                             orderby wordGroup.Key
//                                             select wordGroup;
// foreach(var group in query)
// {
//     Console.WriteLine(group.Key);
//     foreach(var word in group)
//     {
//         Console.WriteLine("    "+word);
//     }
// }







Student[] students =
{
    new Student("Chirag","Memriya",1,GradeLevel.FirstYear,new List<int>{90,80,85},101),
    new Student("Chirag4","Memriya4",5,GradeLevel.FirstYear,new List<int>{94,85,89},102),
    new Student("Chirag6","Memriya6",7,GradeLevel.FirstYear,new List<int>{96,87,81},103),
    new Student("Chirag61","Memriya61",71,GradeLevel.FirstYear,new List<int>{96,87,81},103),
    new Student("Chirag3","Memriya3",4,GradeLevel.FirstYear,new List<int>{93,84,88},104),
    new Student("Chirag5","Memriya5",6,GradeLevel.FirstYear,new List<int>{95,86,80},105),
    new Student("Teacher1","Last1",61,GradeLevel.FirstYear,new List<int>{95,86,80},105),
    new Student("Chirag1","Memriya1",2,GradeLevel.FirstYear,new List<int>{91,82,85},106),
    new Student("Chirag2","Memriya2",3,GradeLevel.FirstYear,new List<int>{92,83,87},106),

};

Teacher[] teachers =
{
    new Teacher("Teacher1","Last1",1,"City1"),
    new Teacher("Teacher2","Last2",2,"City2"),
    new Teacher("Teacher3","Last3",3,"City3"),
    new Teacher("Teacher4","Last4",4,"City4"),
    new Teacher("Teacher5","Last5",5,"City5"),
    new Teacher("Teacher6","Last6",6,"City6"),
};

Department[] departments =
{
    new Department("CSE1",101,1),
    new Department("CSE2",102,2),
    new Department("CSE3",102,3),
    new Department("CSE4",103,4),
    new Department("CSE5",104,5),
    new Department("CSE6",105,6),
    new Department("CSE7",106,7),
};




// var query = from department in departments
//             join student in students on department.ID equals student.DepartmentID
//             orderby student.ID
//             select new
//             {
//                 Name = $" {student.FirstName} {student.LastName}",
//                 DepartmentName = department.Name
//             };

// foreach(var data in query)
// {
//     Console.WriteLine($" {data.Name.PadRight(20,' ')}  ----> {data.DepartmentName}");
// }

// var query = students.Join(departments,
//         student => student.DepartmentID, department => department.ID,

//         (student, department) =>

//             new
//             {
//                 Name = student.LastName,
//                 DepartmentName = department.Name
//             }

// );

// foreach(var data in query)
// {
//     Console.WriteLine($"{data.Name} {data.DepartmentName}");
// }


// IEnumerable<IEnumerable<Student>> studentGroups = from department in departments
//                     join student in students on department.ID equals student.DepartmentID into studentGroup
//                     select studentGroup;

// foreach (IEnumerable<Student> studentGroup in studentGroups)
// {
//     Console.WriteLine("Group");
//     foreach (Student student in studentGroup)
//     {
//         Console.WriteLine($"  - {student.FirstName}, {student.LastName}");
//     }
// }

// var query =
//     from department in departments
//     join teacher in teachers on department.TeacherID equals teacher.ID
//     select new
//     {
//         Name = teacher.First,
//         DepartmentName = department.ID
//     };

// foreach(var data in query)
// {
//     Console.WriteLine(data.Name + " " + data.DepartmentName);
// }


// var query =
//     from teacher in teachers
//     join student in students
//     on new
//     {
//         teacher.First,
//         teacher.Last
//     } equals new
//     {
//         First= student.FirstName,
//         Last = student.LastName
//     }
//     select teacher.First + " " + student.LastName;

// foreach(var data in query)
// {
//     Console.WriteLine(data);
// }



// The first join matches Department.ID and Student.DepartmentID from the list of students and
// departments, based on a common ID. The second join matches teachers who lead departments
// with the students studying in that department.

var query = from department in departments
            join student in students on department.ID equals student.DepartmentID
            join teacher in teachers on department.TeacherID equals teacher.ID
            select new
            {
                StudentName = $"{student.FirstName} {student.LastName}",

            };
public interface IStudentService
{
    Task<Student> CreateAsync(StudentDto dto);
    Task<List<Student>> GetStudentsAsync();
}
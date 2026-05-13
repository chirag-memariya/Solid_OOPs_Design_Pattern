public interface IStudentRepository
{
    Task<List<Student>> GetAllStudent();
    Task<Student> AddAsync(Student student);
}
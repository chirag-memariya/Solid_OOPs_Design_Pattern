using AutoMapper;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;
    private readonly IMapper _mapper;
    public StudentService(IStudentRepository repo,IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    } 
    public async Task<Student> CreateAsync(StudentDto dto)
    {
        var student = _mapper.Map<Student>(dto);
        return await _repo.AddAsync(student);
    }
    public async Task<List<Student>> GetStudentsAsync()
    {
        return await _repo.GetAllStudent();
    }
}
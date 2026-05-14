namespace Lambda;

class Person
{
    public string Name { get; set; }
    public int Age { get; set; }


    public static void Main(string[] args)
    {
        List<Person> people = new List<Person>
        {
            new Person{Name = "John",Age=25},
            new Person{Name = "Mary",Age=32},
            new Person{Name = "Bob",Age=19}
        };

        var sorted = people.OrderBy(p => p.Age);//this p can be any later or work for example x or y or z or any thing like abc etc.
            //for first iteration p is john's person object
            //for second iteration p is Mary's person object
            //for third iteration p is Bob's person object
    }
}
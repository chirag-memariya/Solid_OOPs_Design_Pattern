namespace Project;
public class Customer
{
    public int Revenue{get;set;}
    public Customer(int rev)
    {
        Revenue = rev;
    }
}

public class ORM
{
    public List<Customer> Customers = new List<Customer>();

    public ORM()
    {
        Customers.Add(new Customer(2256));
        Customers.Add(new Customer(22562));
        Customers.Add(new Customer(2243));
        Customers.Add(new Customer(2234));
        Customers.Add(new Customer(2254));
        Customers.Add(new Customer(225));
        Customers.Add(new Customer(26));
        Customers.Add(new Customer(22));
        Customers.Add(new Customer(202));
        Customers.Add(new Customer(102));
        Customers.Add(new Customer(221));
        Customers.Add(new Customer(90));
        Customers.Add(new Customer(100));
        Customers.Add(new Customer(1001));
        Customers.Add(new Customer(140));
        Customers.Add(new Customer(8943));
    }

    public IQueryable<Customer> GetCustomersAsQueryable()
    {
        return Customers.AsQueryable();
    }

    public IEnumerable<Customer> GetCustomersAsEnumerable()
    {
        return Customers.AsEnumerable();
    }


}
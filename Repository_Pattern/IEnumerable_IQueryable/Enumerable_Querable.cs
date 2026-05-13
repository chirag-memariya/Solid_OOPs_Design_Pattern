using Microsoft.VisualBasic;
using Project;

internal class Solve
{
    public static void Main(string[] args)
    {
          var db = new ORM();
          IEnumerable<Customer> e = db.GetCustomersAsEnumerable();
          //e => 50000 suppose db row size is 50000

          var highPayingCustomers = e.Where(c=>c.Revenue > 2500);
          //highPayingCustomers => 25000  


          IQueryable<Customer> q = db.GetCustomersAsQueryable(); //this is provided by default by ef core
          // q => 0
          var highPayingCustomers1= q.Where(c=>c.Revenue > 2500);
          // highPayingCustomers1 => 0

          var finalData = highPayingCustomers1.ToList();

          //select all from customers where revenue > 2500 //=> finally query execute here 
    }
}
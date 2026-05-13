namespace ExtensionMethodsTutorials1;
internal class Program
{
    static void MainTest()
    {
        List<int> numbers = new List<int>{1,2,3,4,5};
        var avg = numbers.Average();
    }
}

    public static class IEnumerableExtensions
    {
        public static double Average(this IEnumerable<int> source)
        {
            if(source == null)
            {
                throw new ArgumentException(nameof(source));
            }
            return (double)source.Sum() / source.Count();
        }
    }

    //An extension method allows you to "add" new methods to existing types without modifying their source code
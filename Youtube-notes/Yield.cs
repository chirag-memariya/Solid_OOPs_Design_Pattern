namespace Yield;
// public class Sol
// {
//     public static void Main(string[] args)
//     {
//         var numbers = GetEvenNumvers(1000000000);// 1 Billion
//         foreach(int number in numbers)
//         {
//             Console.WriteLine(number);
//         }
//     }

//     static IEnumerable<int> GetEvenNumvers(int max)
//     {
//         List<int> numbers = new List<int>();
//         for(int i = 0; i < max; i++)
//         {
//             if (i % 2 == 0)
//             {
//                 numbers.Add(i);
//             }
//         }
//         return numbers;
//     }
// }


/// <summary>
/// after using Yield
/// </summary>

public class Sol
{
    public static void Main(string[] args)
    {
        var numbers = GetEvenNumvers(1000000000);// 1 Billion
        // after reaching this numbers it has notthing but only iterable only , 
        // which yet not started
        foreach(int number in numbers)
        {
            //it will start when we itrate over this only
            Console.WriteLine(number);
        }
    }

    static IEnumerable<int> GetEvenNumvers(int max)
    {
        for(int i = 0; i < max; i++)
        {
            if (i % 2 == 0)
            {
                yield return i;
            }
        }
    }
}

//so for this it first go to foreach and after it goes to yield return , which return 0 as first
//element so after that yield return it goes to foerach and print that number and iterate store that number and this will goes
//after each element returned



// with yiel we can return values from a sequeance wihtout the need of entire sequeance get created upfront
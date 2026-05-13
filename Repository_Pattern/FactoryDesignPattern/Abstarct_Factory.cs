/*
namespace AbstractFactory.Repo;

class Factory
{
    void Main()
    {
        new NavigationBar(new Apple());
        new DropdownMenu(new Apple());
    }
}


public class NavigationBar
{
    public NavigationBar(Apple factory) => factory.CreateButton();
}

public class DropdownMenu
{
    public DropdownMenu(Apple factory)=> factory.CreateButton();
}

public class Apple 
{
    public Button CreateButton()
    {
        return new Button { Type = "iOS Button"};
    }
}

*/



// using above we are providing conceate type to parameter in Navigation and dropdown constructor
// so we can not replace it with other implementaion of that or if some change accured 
//then we need to update it on multiple places


//after using interface we can inject other class as well that implement this 


//this is called DI, we define what we need or abstract it ,
//the things that do the actual creation is the thing that is supplied.

//after apply abstract factory 

namespace AbstractFactory.Repo;
class AbstractFactory
{
    void Main()
    {
        new NavigationBar(new Android());
        new DropdownMenu(new Android());
        
        new NavigationBar(new Apple());
        new DropdownMenu(new Apple());
    }
}

public class NavigationBar
{
    public NavigationBar(IUIFactory factory) => factory.CreateButton();//used interface here
}

public class DropdownMenu
{
    public DropdownMenu(IUIFactory factory)=> factory.CreateButton();//used interface here
}

public interface IUIFactory
{
    public Button CreateButton();
}

public class Apple : IUIFactory
{
    public Button CreateButton()
    {
        return new Button { Type = "iOS Button"};
    }
}

public class Android : IUIFactory
{
    public Button CreateButton()
    {
        return new Button { Type = "Android Button"};
    }
}
//when to use : when we creating something in two places identically, then bring them in one place
class Factory
{
    void Main()
    {
        new NavigationBar();
        new DropdownMenu();
    }
}


/*
    //without factory desing pattern
    public class NavigationBar
    {
        public NavigationBar() => new Button{ Type = "Default Button"};
    }


    public class DropdownMenu
    {
        public DropdownMenu() => new Button{ Type = "Default Button"};
    }

    public class Button
    {
        public string Type{get;set;}
    }
*/


public class NavigationBar
{
    public NavigationBar() => ButtonFactory.CreateButton();
}


public class DropdownMenu
{
    public DropdownMenu() => ButtonFactory.CreateButton();
}

public class ButtonFactory
{
    public static Button CreateButton()
    {
        return new Button{ Type = "Default Button"};
    }
}

public class Button
{
    public string Type{get;set;}
}

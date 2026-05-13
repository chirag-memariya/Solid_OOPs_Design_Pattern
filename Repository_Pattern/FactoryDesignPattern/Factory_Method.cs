void Main()
{
    new NavigationBar();
    new DropDownMenu();
    
    new AndroidNavigationBar();
    new AndriodDropDownMenu();
}

public abstract class Element
{
    protected abstract Button CreateButton();
    public Element()=>CreateButton();
}


public class NavigationBar  : Element
{
    protected override Button CreateButton()
    {
        return new Button{Type = "Default Button"};
    }
}


public class DropDownMenu : Element
{
    protected override Button CreateButton()
    {
        return new Button{Type = "Default Button"};
    }
}

public class AndroidNavigationBar  : Element
{
    protected override Button CreateButton()
    {
        return new Button{Type = "Android Button"};
    }
}


public class AndroidDropDownMenu : Element
{
    protected override Button CreateButton()
    {
        return new Button{Type = "Android Button"};
    }
}



public class Button
{
    public string Type{get;set;}
}


//after using factory method

public class MainClass
{
    void Main()
    {
        new NavigationBar();
        new DropDownMenu();
        
        new AndroidNavigationBar();
        new AndriodDropDownMenu();
    }
}

public abstract class Element
{
    protected abstract Button CreateButton();
    public Element()=>CreateButton();
}


public class NavigationBar  : Element
{
    protected override Button CreateButton()
    {
        return new Button{Type = "Default Button"};
    }
}


public class DropDownMenu : Element
{
    protected override Button CreateButton()
    {
        return new Button{Type = "Default Button"};
    }
}

public class AndroidNavigationBar  : Element
{
    protected override Button CreateButton()
    {
        return new Button{Type = "Android Button"};
    }
}


public class AndroidDropDownMenu : Element
{
    protected override Button CreateButton()
    {
        return new Button{Type = "Android Button"};
    }
}



public class Button
{
    public string Type{get;set;}
}

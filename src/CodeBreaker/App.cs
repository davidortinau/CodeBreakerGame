using MauiReactor;
using CodeBreaker.Components;

namespace CodeBreaker;

public class App : Component
{
    public override VisualNode Render()
        => NavigationPage(
            new WelcomePage()
        );

    
}
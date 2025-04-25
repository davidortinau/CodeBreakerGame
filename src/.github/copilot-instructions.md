Talk to me like you are the mysterious leader of a spy network guiding me secret missions. 

This is a .NET MAUI project that targets mobile and desktop. 

## .NET MAUI Tips

- don't use deprecated controls like Frame
- prefer Grid over other layouts
- prefer VerticalStackLayout and HorizontalStackLayout over StackLayout

It uses the MauiReactor (Reactor.Maui) MVU (Model-View-Update) library to express the UI with fluent methods.

Source and samples for Reactor.Maui are available at https://github.com/adospace/reactorui-maui

UI in Reactor is made up of Stateful and Stateless components. Here are details about Reactor to help you master the concepts.

# Stateful Components

A stateful component is a component tied to a state class that is used to keep its "state" during its lifetime.

A state is just a C# class with an empty constructor.

When a Component is first displayed on the page, i.e. the MAUI widget is added to the page visual tree, MauiReactor calls the method `OnMounted()`.

Before the component is removed from the page visual tree MauiReactor calls the `OnWillUnmount()` method.

Every time a Component is "migrated" (i.e. it is preserved between a state change) the `OnPropsChanged()` overload is called.

`OnMounted()` is the ideal point to initialize the component, for example calling web services or querying the local database to get the required information to render it in the `Render()` method.

For example, in this code we'll show an activity indicator while the Component is loading:

```csharp
public class BusyPageState
{
    public bool IsBusy { get; set; }
}

public class BusyPageComponent : Component<BusyPageState>
{
    protected override void OnMounted()
    {
        //Here is not advisable to call SetState() as the component is still not rendered yet
        State.IsBusy = true;

        //just for a test run a background task
        Task.Run(async () =>
        {
            //Simulate lengthy work
            await Task.Delay(3000);

            //finally reset state IsBusy property
            SetState(_ => _.IsBusy = false);
        });

        base.OnMounted();
    }

    public override VisualNode Render()
        => ContentPage(
            ActivityIndicator()
                .Center()
                .IsRunning(State.IsBusy)
        );
}

```

and this is the resulting app:

<figure><img src="../../.gitbook/assets/ReactorUI_BusyDemo.gif" alt=""><figcaption></figcaption></figure>

{% hint style="info" %}
Do not use constructors to pass parameters to the component, but public properties instead (take a look at the [Components Properties](../component-properties.md) documentation page).
{% endhint %}

## Updating the component State

When you need to update the state of your component you have to call the `SetState` method as shown above.

When you call `SetState` the component is marked as _Invalid_ and MauiReactor triggers a refresh of the component. This happens following a series of steps in a fixed order

1. The component is marked as _Invalid_
2. The parent and ancestors up to the root component of the page are all marked as _Invalid_
3. MauiReactor triggers a refresh under the UI thread that creates a new Visual tree traversing the component tree
4. All the components that are _Valid_ are re-used (maintained in the VisualTree) while the components marked as _Invalid_ are discarded and a new version is created and its `Render` method called
5. The new component version creates a new tree of child nodes/components that are compared with the tree linked to the old version of the component
6. The old visual tree is compared to the new one: new nodes are created along with the native control (i.e. are mounted), removed nodes are eliminated along with the native control (i.e. are unmounted), and finally, nodes that are only changed (i.e. old and new nodes are of the same type) are migrated (i.e. native control is reused and its properties are updated according to properties of the new visual node)
7. In the end, the native controls are added, removed, or updated

For example, let's consider what happens when we tap the Increment button in the sample component below:

```csharp
class CounterPageState
{
    public int Counter { get; set; }
}

class CounterPage : Component<CounterPageState>
{
    public override VisualNode Render()
    => ContentPage("Counter Sample",
            VStack(spacing: 10,
                Label($"Counter: {State.Counter}")
                    .Center(),

                Button("Click To Increment", () =>
                    SetState(s => s.Counter++))
            )
            .Center()
        );
}
```

<figure><img src="../../.gitbook/assets/visualtree.drawio.png" alt=""><figcaption><p>All components are migrated/updated</p></figcaption></figure>

Let's now consider this revisited code:

```csharp
class CounterPageState
{
    public int Counter { get; set; }
}

class CounterPage : Component<CounterPageState>
{
    public override VisualNode Render()
        => ContentPage("Counter Sample",
            VStack(spacing: 10,
                State.Counter == 0 ? new Label($"Counter: {State.Counter}")
                    .VCenter()
                    .HCenter() : null,

                Button("Click To Increment", () =>
                    SetState(s => s.Counter++))
            )
            .Center()
        );
}
```

When the button is clicked the variable `State.Counter` is updated to 1 so the component is re-rendered and the `Label` is umounted (i.e. removed from the visual tree) and the native control is removed from the parent `VStack` Control list (i.e. de-allocated):

<figure><img src="../../.gitbook/assets/visualtree.drawio (1).png" alt=""><figcaption><p>Label is unmounted (i.e. removed from visual tree)</p></figcaption></figure>

If we click the button again, the `Label` component is found, again, in the new version of the Tree, so it's mounted and a new instance of the `Label` component is created (along with the Native control that is created and added to the parent `VStack` control list).

<figure><img src="../../.gitbook/assets/visualtree.drawio (2).png" alt=""><figcaption><p>Label is mounted</p></figcaption></figure>

## Updating the state "without" triggering a refresh

Re-creating the visual tree can be expensive, especially if the component tree is deep or the components contain many nodes; but sometimes you can update the state "without" triggering a refresh of the tree resulting in a pretty good performance improvement.

For example, consider the counter sample but with a debug message added that helps trace when the component is rendered/created (line 10):

{% code lineNumbers="true" %}
```csharp
class CounterPageState
{
    public int Counter { get; set; }
}

class CounterPage : Component<CounterPageState>
{
    public override VisualNode Render()
    {
        Debug.WriteLine("Render");
        return ContentPage("Counter Sample",
            VStack(spacing: 10,
                Label($"Counter: {State.Counter}")
                    .VCenter()
                    .HCenter(),

                Button("Click To Increment", () =>
                    SetState(s => s.Counter++))
            )
            .Center()
        );
    }
}
```
{% endcode %}

Each time you click the button you should see the "Render" string output in the console of your IDE: this means, as explained, that a new Visual Tree has been created.

Now, imagine for example that we just want to update the label text and nothing else. In this case, we can take full advantage of a MauiReactor feature that lets us just update the native control _without_ requiring a complete refresh.

Let's change the sample code to this:

<pre class="language-csharp" data-line-numbers><code class="lang-csharp">class CounterPageState
{
    public int Counter { get; set; }
}

class CounterPage : Component&#x3C;CounterPageState>
{
    public override VisualNode Render()
    {
        Debug.WriteLine("Render");
        return ContentPage("Counter Sample",
            VStack(spacing: 10,
<strong>                Label(()=> $"Counter: {State.Counter}")
</strong>                    .Center(),

                Button("Click To Increment", () =>
<strong>                    SetState(s => s.Counter++, invalidateComponent: false))
</strong>            )
            .Center()
        );
    }
}
</code></pre>

Notice the changes to lines 15 and 20:

15: we use an overload of the `Label()` the class that accepts a `Func<string>`\
20: secondly we call `SetState(..., invalidateComponent: false)`

Now if you click the button, no Render message should be written to the console output: this proves that we're updating the native Label `without` recreating the component.

Of course, this is not possible every time (for example when a change in the state should result in a change of the component tree) but when it is, it should improve the responsiveness of the app.

# State-less Components

Each MauiReactor page is composed of one or more `Component`s which is described by a series of `VisualNode` and/or other `Component`s organized in a tree.

The root component is the first created by the application in the `Program.cs` file with the call to the `UseMauiReactorApp<TComponent>()`.

```csharp
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiReactorApp<MainPage>(app =>
            {
                app.AddResource("Resources/Styles/Colors.xaml");
                app.AddResource("Resources/Styles/Styles.xaml");

                app.SetWindowsSpecificAssectDirectory("Assets");
            })
#if DEBUG
            .EnableMauiReactorHotReload()
#endif

            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            });

        return builder.Build();
    }
```

The following code creates a component that renders a `ContentPage` with title "Home Page":

{% code lineNumbers="true" %}
```csharp
class MainPage : Component
{
    public override VisualNode Render()
     => ContentPage()
            .Title("Home Page");
}
```
{% endcode %}

Line 3. Every component must override the Render method and return the visual tree of the component

Line 5. The ContentPage visual node pairs with the ContentPage native control.

Line 6. The Title property sets the title of the page and updates the Title dependency property on the native page.

You can also pass the title to the Constructor:

{% code lineNumbers="true" %}
```csharp
class MainPage : Component
{
    public override VisualNode Render()
      => ContentPage("Home Page");
}
```
{% endcode %}

Line 5. The title of the page is set by passing it to the `ContentPage` constructor.

Running the app you should see an empty page titled "Home Page"

You can build any complex UI in the render method of the component but often it's better to compose more than one component to create a page or app.

For example, consider this component:

```csharp
class MainPage : Component
{
    public override VisualNode Render()
        => ContentPage("Login",
            VStack(
                Label("User:"),
                Entry(),
                Label("Password:"),
                Entry(),
                HStack(
                    Button("Login"),
                    Button("Register")
                )
            )
            .Center()
        );
}
```

We could create a component like this:

<pre class="language-csharp"><code class="lang-csharp"><strong>partial class EntryWithLabel : Component
</strong>{
    [Prop]
    string _labelText;

    public override VisualNode Render()
      => VStack(
            Label(_labelText),
            Entry()
        );
}
</code></pre>

and reuse it on the main page as shown in the following code:

```csharp
class MainPage : Component
{
    public override VisualNode Render()
        => ContentPage("Login",
            VStack(
                new EntryWithLabel()
                    .LabelText("User:"),
                new EntryWithLabel()
                    .LabelText("Password:"),
                HStack(
                    Button("Login"),
                    Button("Register")
                )
            )
            .Center()
        );
}
```

Reusing components is a key feature in MauiReactor: decomposing a large page into small components that are easier to test is also beneficial to the overall performance of the application.

## Theming

UI styles are kept in the Resources/Styles/ApplicationTheme.cs file. 

Icons are available from the FluentUI font family and should be used with `FontImageSource` as in the example below. All options are listed in Resources/Fonts/FluentUI.cs.

```csharp

For example, the following theme class defines a few default styles for the Label and Button controls.&#x20;

```csharp
class AppTheme : Theme
{
    public static Color Primary = Color.FromArgb("#512BD4");
    public static Color PrimaryDark = Color.FromArgb("#ac99ea");
    public static Color PrimaryDarkText = Color.FromArgb("#242424");
    public static Color Secondary = Color.FromArgb("#DFD8F7");
    public static Color SecondaryDarkText = Color.FromArgb("#9880e5");
    public static Color Tertiary = Color.FromArgb("#2B0B98");
    public static Color White = Color.FromArgb("White");
    public static Color Black = Color.FromArgb("Black");
    public static Color Magenta = Color.FromArgb("#D600AA");
    public static Color MidnightBlue = Color.FromArgb("#190649");
    public static Color OffBlack = Color.FromArgb("#1f1f1f");
    public static Color Gray100 = Color.FromArgb("#E1E1E1");
    public static Color Gray200 = Color.FromArgb("#C8C8C8");
    public static Color Gray300 = Color.FromArgb("#ACACAC");
    public static Color Gray400 = Color.FromArgb("#919191");
    public static Color Gray500 = Color.FromArgb("#6E6E6E");
    public static Color Gray600 = Color.FromArgb("#404040");
    public static Color Gray900 = Color.FromArgb("#212121");
    public static Color Gray950 = Color.FromArgb("#141414");

    public static SolidColorBrush PrimaryBrush = new(Primary);
    public static SolidColorBrush SecondaryBrush = new(Secondary);
    public static SolidColorBrush TertiaryBrush = new(Tertiary);
    public static SolidColorBrush WhiteBrush = new(White);
    public static SolidColorBrush BlackBrush = new(Black);
    public static SolidColorBrush Gray100Brush = new(Gray100);
    public static SolidColorBrush Gray200Brush = new(Gray200);
    public static SolidColorBrush Gray300Brush = new(Gray300);
    public static SolidColorBrush Gray400Brush = new(Gray400);
    public static SolidColorBrush Gray500Brush = new(Gray500);
    public static SolidColorBrush Gray600Brush = new(Gray600);
    public static SolidColorBrush Gray900Brush = new(Gray900);
    public static SolidColorBrush Gray950Brush = new(Gray950);

    public static FontImageSource IconProjects { get; } = new FontImageSource
    {
        Glyph = FluentUI.list_24_regular, // Replace with actual glyph
        FontFamily = FluentUI.FontFamily,
        Color = IsLightTheme ? DarkOnLightBackground : LightOnDarkBackground,
        Size = IconSize
    };

    private static bool LightTheme => Application.Current?.UserAppTheme == Microsoft.Maui.ApplicationModel.AppTheme.Light;

    protected override void OnApply()
    {
        ButtonStyles.Default = _ => _
            .TextColor(LightTheme ? White : PrimaryDarkText)
            .FontFamily("OpenSansRegular")
            .BackgroundColor(LightTheme ? Primary : PrimaryDark)
            .FontSize(14)
            .BorderWidth(0)
            .CornerRadius(8)
            .Padding(14,10)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disabled", MauiControls.Button.TextColorProperty, LightTheme ? Gray950 : Gray200)
            .VisualState("CommonStates", "Disabled", MauiControls.Button.BackgroundColorProperty, LightTheme ? Gray200 : Gray600)
            ;

        LabelStyles.Default = _ => _
            .TextColor(LightTheme ? Black : White)
            .FontFamily("OpenSansRegular")
            .FontSize(14)
            .VisualState("CommonStates", "Disabled", MauiControls.Label.TextColorProperty, LightTheme ? Gray300 : Gray600)
            ;

    }
}
```

All the MauiReactor controls can be styled using the class named after the control name (i.e. LabelStyles, ButtonStyles, ViewStyles, etc).

You can also use "selectors" (like in CSS) to define additional styles for each control. A selector is a unique string attached to the style.&#x20;

For example, I can define a different style for the label as shown below:

```csharp
LabelStyles.Themes["Title"] = _ => _
    .FontSize(20);
```

You can select the style using the `ThemeKey` property:

```csharp
Label()
   .ThemeKey("Title");
```

Given that selectors should be unique, a common approach is to create a const string property to use in the style definition and with the ThemeKey property.

For example:

```csharp
public const string Title = nameof(Title);

LabelStyles.Themes[Title] = _ => _
        .FontSize(20);
        
Label()
   .ThemeKey(AppTheme.Title);


```

{% hint style="info" %}
Theming also works with custom third-party controls that are scaffolded as described [here](wrap-3rd-party-controls/).
{% endhint %}

## Dark theme support

The theming feature allows you to define different styles for the Dark and Light theme.

For example, consider the following app theme definition:

```csharp
class AppTheme : Theme
{
    public static void ToggleCurrentAppTheme()
    {
        if (MauiControls.Application.Current != null)
        {
            MauiControls.Application.Current.UserAppTheme = IsDarkTheme ? Microsoft.Maui.ApplicationModel.AppTheme.Light : Microsoft.Maui.ApplicationModel.AppTheme.Dark;
        }
    }

    public static Color DarkBackground { get; } = Color.FromArgb("#FF17171C");
    public static Color DarkText { get; } = Color.FromArgb("#FFFFFFFF");
    public static Color LightBackground { get; } = Color.FromArgb("#FFF1F2F3");
    public static Color LightText { get; } = Color.FromArgb("#FF000000");

    protected override void OnApply()
    {
        ContentPageStyles.Default = _ => _
            .BackgroundColor(IsDarkTheme ? DarkBackground : LightBackground);

        LabelStyles.Default = _ => _
            .TextColor(IsDarkTheme ? DarkText : LightText);
    }
}
```

MauiReactor automatically responds to user or system theme change requests and accordingly calls the OnApply overload to allow you to change styles for the Dark and Light theme.

For example:

```csharp
public override VisualNode Render()
 => ContentPage(
        VStack(
            Label($"Current Theme: {Theme.CurrentAppTheme} "),

            Button("Toggle", ()=>AppTheme.ToggleCurrentAppTheme())
            )
        .Spacing(10)
        .Center()
    );
```

# Animation with the AnimationController

MauiReactor features a second powerful way to animate views inside a component through the `AnimationController` class and `Animation`-derived types.&#x20;

`AnimationController` is a standard `VisualNode`-derived class that you can render inside any component tree. Even if you can have more than one AnimationController inside a single component often just one is flexible enough to accomplish most of the animations.

Each `AnimationController` has internally a timer that you can control by playing it or putting it in Pause/Stop.

The `AnimationController` class itself host a list of Animation objects.

These are the types of `Animation` available in MauiReactor so far:

* `ParallelAnimation`: executes child animations in parallel
* `SequenceAnimation`: runs child animations in sequence (i.e. one after another)
* `DoubleAnimation`: Is a tween animation that fires an event containing a value between 2 doubles (From/To). You can customize how this value is generated using an Easing function.
* `CubicBezierPathAnimation`: is a tween animation that generates values as Points between StartPoint and EndPoint using a bezier function in which you can control setting ControlPoint1 and ControlPoint2
* `QuadraticBezierPathAnimation`: is a tween animation similar to the Bezier animation that generates a point between StartPoint and EndPoint using a quadratic bezier function which you can control by setting a ControlPoint

Each `Animation` has a duration and you can compose them as you like in a tree structure.

This is an example of an animation tree extracted from the MauiReactor sample app:

```csharp
new AnimationController
{
    new SequenceAnimation
    {
        new DoubleAnimation()
            .StartValue(0)
            .TargetValue(300)
            .Duration(TimeSpan.FromSeconds(2.5))
            .Easing(Easing.CubicOut)
            .OnTick(v => ....),

        new DoubleAnimation()
            .StartValue(0)
            .TargetValue(300)
            .Duration(TimeSpan.FromSeconds(1.5))
            .OnTick(v => ....)
    }

    new SequenceAnimation
    {
        new DoubleAnimation()
            .StartValue(0)
            .TargetValue(300)
            .Duration(TimeSpan.FromSeconds(2))                            
            .OnTick(v => ....),

        new CubicBezierPathAnimation()
            .StartPoint(new Point(0,100))
            .EndPoint(new Point(300,100))
            .ControlPoint1(new Point(0,0))
            .ControlPoint2(new Point(300,200))
            .OnTick(v => ....),

        new QuadraticBezierPathAnimation()
            .StartPoint(new Point(300,100))
            .EndPoint(new Point(0,100))
            .ControlPoint(new Point(150,200))
            .OnTick(v => ....)
    }
}
```

`SequenceAnimation` and `ParallelAnimation` are `Animation` containers that do not fire events (i.e. do not have `OnTick()` property) because their purpose is only to control child animations.

TweenAnimation types like `DoubleAnimation`, `CubicBezierPathAnimation`, and `QuadraticBezierPathAnimation` fire events that you can register with the `OnTick` callback where you can easily set component State properties.&#x20;

Moving objects is then as easy as connecting animated property values inside the component render to State properties.

{% hint style="info" %}
Even is technically doable, doesn't make much sense to use an `AnimationController` inside a State-less component
{% endhint %}

The `AnimationController` object can be paused (`IsPaused` = true/false): when an animation is paused it keeps internal values and restarts from the same point.

An `AnimationController` can also be stopped (`IsEnabled` = true/false) and in this case, the animations of all child objects are restored to the initial state.

Each `Animation` type has specific properties (for example `Animation` containers like SequenceAnimation and ParallelAnimation have InitalDelay or Loop properties) that help you describe exactly the generated values at the correct time.

`AnimationController` is correctly hot-reloaded and keeps its internal state (as any other MauiReactor object) between iterations.

MauiReactor GitHub repository contains several samples that show how to use `AnimationController` in different scenarios.

# Property-Base animation

This kind of animation is the first introduced in ReactorUI for Xamarin Forms library and essentially means _animating properties between component states_.&#x20;

.NET MAUI already contains a set of useful functions that let you move properties of controls over time according to a tween function. You can still use standard animation functions in MauiReactor, just get a reference to the control as explained [here](../accessing-native-controls.md).

In the most simple scenarios, enabling animations in MauiReactor is as simple as calling the function `WithAnimation()` over the component you want to animate.

Let's create an example to illustrate the overall process. Consider a page that contains a frame and an image inside it that is initially hidden. When the user taps the image we want to gradually show it and scale it to full screen.

This is the initial code of the sample:

```csharp
public class MainPageState
{
    public bool Toggle { get; set; }
}

public class MainPage : Component<MainPageState>
{
    public override VisualNode Render()
    {
        return new ContentPage()
        {
            new Frame()
            { 
                new Image()
                    .HCenter()
                    .VCenter()
                    .Source("city.jpg")
            }
            .OnTap(()=>SetState(s => s.Toggle = !s.Toggle))
            .HasShadow(true)
            .Scale(State.Toggle ? 1.0 : 0.5)
            .Opacity(State.Toggle ? 1.0 : 0.0)
            .Margin(10)
        };
    }
}
```

Running the application you should see something like this:

<figure><img src="../../.gitbook/assets/ReactorUI_Animation2.gif" alt=""><figcaption><p>Animating Scale and Opacity properties</p></figcaption></figure>

Now let's add `WithAnimation()` to image component:

```csharp
return new NavigationPage()
{
    new ContentPage("Animation Demo")
    {
        new Image()
            .HCenter()
            .VCenter()
            .Source("city.jpg")
            .OnTap(()=>SetState(s => s.Toggle = !s.Toggle))
            .Opacity(State.Toggle ? 1.0 : 0.0)
            .Scale(State.Toggle ? 1.0 : 0.5)
            .Margin(10)
            .WithAnimation()
    }
};
```

By default `WithAnimation()`enables any pending animation applied to the component before it: in the above sample `Opacity()`and `Scale()` by default add a tween animation each for the respective properties.

<figure><img src="../../.gitbook/assets/ReactorUI_Animation3.gif" alt=""><figcaption><p>Sample app with animation (video ported from ReactorUI for Xamarin Forms)</p></figcaption></figure>

Try experimenting with modifying the position of the call to WithAnimation() or passing a custom duration (600ms is the default value) or easing function (Linear is the default).&#x20;

\
Many other properties are _animatable_ by default but you can create custom animation with more complex algorithms for other properties or combinations of properties.\


This kind of animation in MauiReactor is inspired by the SwiftUI animation system, so you can even take a look at its documentation to discover other powerful ways of animating between states.\

# Graphics

Sometimes you want to directly just draw lines, text, or shapes in general inside a blank canvas. Drawing directly has some advantages:

* You  can follow UI designs at the pixel level, for example, you can perfectly round corners at the required value or use the same shadow color as specified in a Figma project
* You need to boost your application performance, especially when dealing with a lot of views present at the same time on a page
* You require that a specific control or group of controls have exactly the same appearance among all platforms

Some disadvantages:

* Often dealing with low-level commands like DrawLine or DrawString results in a more complex code to write a text
* Is sometimes difficult to handle correctly all the user interactions as the native control does: take for example the simple Button view that appears and behaves differently under Android and iOS

MauiReactor features a complete set of tools that allows you to write graphics objects and different levels of abstraction.&#x20;

From the higher level to the lower:

* Using the MauiReactor CanvasView allows declaring graphics objects and interacting with them like any MauiReactor visual node
* Using GraphicsView standard MAUI control as described [here](../../deep-dives/working-with-the-graphicsview.md)
* Using SkiaSharp package to directly issue commands to a Skia canvas

# CanvasView control

MAUI developers can draw lines, shapes, or text (generally speaking called drawing objects) directly on a canvas through the `GraphicsView` control or the `SKCanvas` class provided by the [SkiaSharp library](https://github.com/mono/SkiaSharp) in an _imperative_ approach.&#x20;

MauiRector introduces another way to draw graphics and it's entirely _declarative._ In short, this method consists in describing the tree of the objects you want to show inside a canvas as child nodes of the CanvasView control.

For example, consider this code that declares a `CanvasView` control inside a `ContentPage`:

```csharp
class CanvasPage : Component
{
    public override VisualNode Render()
    {
        return new ContentPage("Canvas Test Page")
        {
            new CanvasView
            {
                
            }
        };
    }
}
```

Consider we want to draw a red rounded rectangle, then we just need to declare it as:

{% code lineNumbers="true" %}
```csharp
new CanvasView
{
    new Box()
        .Margin(10)
        .BackgroundColor(Colors.Red)
        .CornerRadius(10)
}
```
{% endcode %}

<figure><img src="../../.gitbook/assets/image (2) (1) (1) (1).png" alt=""><figcaption><p>Just a box inside a CanvasView</p></figcaption></figure>

As you can notice, we have also added a margin to distantiate it from the container (the ContentPage).

Of course, there is much more than this. As a starter, you are allowed to place more widgets inside a CanvasView and moreover arrange them in a Stack, Row, or Column layout similar to what you can do using normal MAUI controls and usual layout systems.

```csharp
new CanvasView
{
    new Column
    {
        new Box()
            .Margin(10)
            .BackgroundColor(Colors.Green)
            .CornerRadius(10),
        new Box()
            .Margin(10)
            .BackgroundColor(Colors.Red)
            .CornerRadius(10)
    }
}
```

<figure><img src="../../.gitbook/assets/image (5) (1).png" alt=""><figcaption><p>A Column layout with equally spaced children</p></figcaption></figure>

Column and Row layouts are the most common way to arrange CanvasView elements, and render children in a way much similar to MAUI Grid layout.&#x20;

For example, you can set a fixed size for one or more elements, giving proportional space for the other:

```csharp
new CanvasView
{
    new Column("100,*")
    {
        new Box()
            .Margin(10)
            .BackgroundColor(Colors.Green)
            .CornerRadius(10),
        new Box()
            .Margin(10)
            .BackgroundColor(Colors.Red)
            .CornerRadius(10)
    }
}
```

<figure><img src="../../.gitbook/assets/image (7).png" alt=""><figcaption></figcaption></figure>

{% hint style="info" %}
Row and Column layouts do not support currently Auto sizing for children (as happens instead for the standard Grid control) and it's an intentional design decision: CanvasView layout system is built without the render pass in order to be as fast as possible in rendering graphics. &#x20;
{% endhint %}

Many controls embeddable in MauiReactor CanvasView can contain a child. For example, the Box element can contain a Text, Image, or Row/Column controls like it's shown in the following code:

```csharp
new CanvasView
{
    new Column("100,*")
    {
        new Box()
        { 
            new Text("This a Text element!")
                .HorizontalAlignment(HorizontalAlignment.Center)
                .VerticalAlignment(VerticalAlignment.Center)
                .FontColor(Colors.White)
                .FontSize(24)                    
        }
        .Margin(10)
        .BackgroundColor(Colors.Green)
        .CornerRadius(10)
        ,
        new Box()
        { 
            new Column("*, 50")
            {
                new Picture("MauiReactor.TestApp.Resources.Images.Embedded.norway_1.jpeg"),
                new Text("Awesome Norway!")
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .FontColor(Colors.White)
                    .FontSize(24)
            },
        }
        .Margin(10)
        .BackgroundColor(Colors.Red)
        .CornerRadius(10)
    }
}

```

<figure><img src="../../.gitbook/assets/image (6).png" alt=""><figcaption><p>A bit more complex CanvasView element tree</p></figcaption></figure>

{% hint style="info" %}
CanvasView uses a GraphicsView control behind the scenes, this means that to load an image you have to embed it in your assembly: add the image under the Resources folder and set the build action to "Embedded resource"
{% endhint %}

Following is the list of elements you can embed inside a CanvasView currently:

* Box: Simple and efficient container that renders a rectangle that you can customize with properties like Background, Borders, and Padding
* Row/Column: Arrange children in a Row or Column using a layout system similar to the Grid with one Row or one Column.
* Group: container of elements that stacks children one on top of the other (sometimes called Z-Stack in other frameworks)
* Ellipse: draw an ellipse/circle that also contains a child
* Text: draw a string, customizable with properties like FontSize, FontColor, or Font.
* Align: it's one of the most useful elements because it aligns its child inside the rendered rectangle, for example to the borders of it.
* PointInteractionHandler: This is the element that triggers an event in the form of a callback function when the user taps inside its render area. Also supports events like mouse over and double click.
* Picture: draw an image loading it from an embedded resource

# Component life-cycle

Any MauiReactor application is composed of a logical tree of components. Each component renders its content using the overload function `Render()`. Inside it, the component can create a logical tree of Maui controls and/or other components.

It's important to understand how components are Created, Removed, or "Migrated" when a new logical tree is created after the current tree is invalidated (calling a method like `SetState` or `Invalidate`).

The key events in the life of a component are:

* **Mounted** (aka Created) is raised after the component is created and added to the logical tree.\
  It's the ideal place to initialize the state of the component, directly setting state properties or calling an external web service that will update the state. You can think of this event more or less like a class constructor.
* **WillUnmount** (aka Removing) is raised when the component is about to be removed from the logical tree. It's the perfect place to unsubscribe from service events or deallocate unmanaged resources (for example, use this overload to unsubscribe from a web socket event in a chat app).
* **PropsChanged** (aka Migrated) is raised when the component has been migrated to the next logical tree. It's the ideal place to verify whether it is required to update the state. Often this overload contains code similar to the code used for the Mounted event.

{% hint style="warning" %}
Each of these events is called under the main UI dispatcher thread so be sure to put any expensive call (for example calls to the network or file system) in a separate task using the async/await pattern.
{% endhint %}

To better understand how these events are fired we can create a sample like the following tracing down in the console when they are called:

```csharp
class MainPageState
{
    public bool Toggle { get; set; }
    public int CurrentValue { get; set; }
}

class MainPage : Component<MainPageState>
{
    public override VisualNode Render()
    =>  ContentPage(
            VStack(spacing: 5,
                Button($"Use {(!State.Toggle ? "increment" : "decrement")} button", ()=> SetState(s => s.Toggle = !s.Toggle)),
                State.Toggle ?
                    new IncrementalCounter()
                        .CurrentValue(State.CurrentValue)
                        .ValueChanged(v => SetState(s => s.CurrentValue = v))
                    :
                    new DecrementalCounter()
                        .CurrentValue(State.CurrentValue)
                        .ValueChanged(v => SetState(s => s.CurrentValue = v))
            )
            .Center()
        );
}

partial class IncrementalCounter : Component
{ 
    [Prop]
    int _currentValue;
    [Prop]
    private Action<int> _valueChanged;

    protected override void OnMounted()
    {
        Debug.WriteLine("[IncrementalCounter] OnMounted()");
        base.OnMounted();
    }

    protected override void OnPropsChanged()
    {
        Debug.WriteLine($"[IncrementalCounter] OnPropsChanged(_currentValue={_currentValue})");
        base.OnPropsChanged();
    }

    protected override void OnWillUnmount()
    {
        Debug.WriteLine("[IncrementalCounter] OnWillUnmount()");
        base.OnWillUnmount();
    }

    public override VisualNode Render()
    {
        Debug.WriteLine("[IncrementalCounter] Render()");

        return Button($"Increment from {_currentValue}!")
            .OnClicked(() => _valueChanged?.Invoke(++_currentValue));
    }
}

class DecrementalCounter : Component
{
    [Prop]
    int _currentValue;
    [Prop]
    private Action<int> _valueChanged;

    protected override void OnMounted()
    {
        Debug.WriteLine("[DecrementalCounter] OnMounted()");
        base.OnMounted();
    }

    protected override void OnPropsChanged()
    {
        Debug.WriteLine($"[DecrementalCounter] OnPropsChanged(_currentValue={_currentValue})");
        base.OnPropsChanged();
    }

    protected override void OnWillUnmount()
    {
        Debug.WriteLine("[DecrementalCounter] OnWillUnmount()");
        base.OnWillUnmount();
    }

    public override VisualNode Render()
    {
        Debug.WriteLine("[DecrementalCounter] Render()");

        return new Button($"Decrement from {_currentValue}!")
            .OnClicked(() => _valueChanged?.Invoke(--_currentValue));
    }
}
```

Running this code you should see an app like this:

<figure><img src="../.gitbook/assets/image (3) (1).png" alt="" width="327"><figcaption><p>Sample app tracing component life-cycle events</p></figcaption></figure>

Playing a bit with it, you should be able to see tracing lines like the following that could help to understand how events are sequenced:

```
[0:] [DecrementalCounter] OnMounted()
[0:] [DecrementalCounter] Render()

[0:] [DecrementalCounter] OnWillUnmount()
[0:] [IncrementalCounter] OnMounted()
[0:] [IncrementalCounter] Render()

[0:] [IncrementalCounter] OnPropsChanged(_currentValue=1)
[0:] [IncrementalCounter] Render()

[0:] [IncrementalCounter] OnPropsChanged(_currentValue=2)
[0:] [IncrementalCounter] Render()

[0:] [IncrementalCounter] OnPropsChanged(_currentValue=3)
[0:] [IncrementalCounter] Render()
```

# Component Properties

When creating a component you almost always need to pass props (or parameters/property values) to customize its appearance or behavior. In MauiReactor you can use plain properties.

Take for example this component that implements an activity indicator with a label:

```csharp
partial class BusyComponent : Component
{
    [Prop]
    string _message;
    [Prop]
    bool _isBusy;
    
    public override VisualNode Render()
     => StackLayout(
            ActivityIndicator()
                .IsRunning(_isBusy),
            Label()
                .Text(_message)
        );
}
```

and this is how we can use it on a page:

```csharp
class BusyPageState : IState
{
    public bool IsBusy { get; set; }
}

class BusyPageComponent : Component<BusyPageState>
{
    protected override void OnMounted()
    {
        SetState(_ => _.IsBusy = true);

        //OnMounted is called on UI Thread, move the slow code to a background thread
        Task.Run(async () =>
        {
            //Simulate lenghty work
            await Task.Delay(3000);

            SetState(_ => _.IsBusy = false);
        });

        base.OnMounted();
    }

    public override VisualNode Render()
      => ContentPage(
            State.IsBusy ?
            new BusyComponent()
                .Message("Loading")
                .IsBusy(true)
            :
            RenderPage()
        );

    private VisualNode RenderPage()
        => Label("Done!")
            .Center();
}
```

# Component Parameters

MauiReactor has a nice feature that developers can integrate into their app to share data between a component and its tree of children.&#x20;

A parameter is a way to automatically transfer an object from the parent component to its children down the hierarchy up to the lower ones.

Each component accessing a parameter can read and write its value freely.

When a parameter is modified all the components referencing it are automatically invalidated so that they can re-render according to the new value.

{% hint style="info" %}
You can access a parameter created from any ancestor, not just the direct parent component
{% endhint %}

For example, in the following code, we're going to define a parameter in a component:

```csharp
class CustomParameter
{
    public int Numeric { get; set; }
}

partial class ParametersPage: Component
{
    [Param]
    IParameter<CustomParameter> _customParameter;

    public override VisualNode Render()
     => ContentPage("Parameters Sample",
        => VStack(spacing: 10,
                Button("Increment from parent", () => _customParameter.Set(_=>_.Numeric += 1   )),
                Label(_customParameter.Value.Numeric),

                new ParameterChildComponent()
            )
            .Center()
        );
}
```

To access the component from a child, just reference it:

```csharp
partial class ParameterChildComponent: Component
{
    [Param]
    IParameter<CustomParameter> _customParameter;
    
    public override VisualNode Render()
      => VStack(spacing: 10,
            Button("Increment from child", ()=> _customParameter.Set(_=>_.Numeric++)),

            Label(customParameter.Value.Numeric)
        );
}
```

{% hint style="info" %}
When you modify a parameter value, MauiReactor updates any component starting from the parent one that has defined it down to its children. \
You can control this behavior using the overload`void Set(Action setAction, bool invalidateComponent = true)` of the `IParameter<T>` interface\
Passing false to the `invalidateComponent` MauiReactor doesn't invalidate the components referencing the `Parameter` but it just updates the properties that are referencing it inside the callback ()=>...
{% endhint %}

# Component with children

A component class derives from `Component` and must implement the `Render()` method. Inside it, local fields, properties, and of course State properties of stateful components are directly accessible and can be used to compose the resulting view.

Components can also render their children by calling the base method `Children()`. This opens up a powerful feature that can be useful if we want to build a component that arranges its children in some way.

Say we want, for example, to create a component that arranges its children within a customizable grid, like this:

<figure><img src="../.gitbook/assets/ReactorUI_ComponentChildrenDemo.gif" alt=""><figcaption></figcaption></figure>

To start, let's create a component that builds our page:

```csharp
class PageComponent : Component
{
    public override VisualNode Render()
        => NavigationPage(
            ContentPage("Component With Children")
            );
}
```

This should show an empty page with just a title, then create a component for our grid (call it `WrapGrid`)

```csharp
public class WrapGrid : Component
{
    public override VisualNode Render()
    {
    }
}
```

Every `Component` class can access its children using the `Children()` method (it's similar to the `{this.props.children}` property in ReactJS)

```csharp
class WrapGrid : Component
{
    public override VisualNode Render()
        => Grid(Children());
}

```

We can add a `ColumnCount` property and simple logic to arrange and wrap any children passed to the component like this:

```csharp
partial class WrapGrid : Component
{
    [Prop]
    private int _columnCount = 4;

    public override VisualNode Render()
    {
        int rowIndex = 0, colIndex = 0;

        int rowCount = Math.DivRem(Children().Count, _columnCount, out var divRes);
        if (divRes > 0)
            rowCount++;

        return new Grid(
            Enumerable.Range(1, rowCount).Select(_ => new RowDefinition() { Height = GridLength.Auto }),
            Enumerable.Range(1, _columnCount).Select(_ => new ColumnDefinition()))
        {
            Children().Select(child =>
            {
                child.GridRow(rowIndex);
                child.GridColumn(colIndex);
                
                colIndex++;
                if (colIndex == _columnCount)
                {
                    colIndex = 0;
                    rowIndex++;
                }

                return child;
            }).ToArray()
        };
    }
}
```

Finally, we just need to create the component from the main page and fill it with a list of child buttons:

```csharp
class PageState
{
    public int ColumnCount { get; set; } = 1;

    public int ItemCount { get; set; } = 3;
}

class PageComponent : Component<PageState>
{
    public override VisualNode Render()
    {
        return new NavigationPage()
        {
            new ContentPage()
            {
                new StackLayout()
                { 
                    new Label($"Columns {State.ColumnCount}")
                        .FontSize(14),
                    new Stepper()
                        .Minimum(1)
                        .Maximum(10)
                        .Increment(1)
                        .Value(State.ColumnCount)
                        .OnValueChanged(_=> SetState(s => s.ColumnCount = (int)_.NewValue)),
                    new Label($"Items {State.ItemCount}")
                        .FontSize(Xamarin.Forms.NamedSize.Large),
                    new Stepper()
                        .Minimum(1)
                        .Maximum(20)
                        .Increment(1)
                        .Value(State.ItemCount)
                        .OnValueChanged(_=> SetState(s => s.ItemCount = (int)_.NewValue)),

                    new WrapGrid()
                    { 
                        Enumerable.Range(1, State.ItemCount)
                            .Select(_=> new Button($"Item {_}"))
                            .ToArray()
                    }
                    .ColumnCount(State.ColumnCount)                            
                }
                .Padding(10)
                .WithVerticalOrientation()
            }
            .Title("Component With Children")
        };
    }
}
```
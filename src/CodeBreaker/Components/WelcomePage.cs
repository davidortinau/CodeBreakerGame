using MauiReactor;
using CodeBreaker.Resources.Styles;
using Microsoft.Maui.Graphics;

namespace CodeBreaker.Components;

public partial class WelcomePage : Component
{
    [Prop]
    System.Action<int>? _onStartGame;

    public override VisualNode Render()
        => ContentPage(
            Grid(
                rows: "*,auto,auto,auto,*",
                columns: "*",
                // Logo
                Image("logo.png")
                    .HCenter()
                    .GridRow(1),

                // Title
                Label("CODE BREAKER")
                    .FontSize(36)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(ApplicationTheme.Gray900)
                    .FontFamily("monospace")
                    .HCenter()
                    .GridRow(2),

                // Buttons
                HStack(spacing: 12,
                    Button("Easy")
                        .FontSize(18)
                        .FontAttributes(FontAttributes.Bold)
                        .BackgroundColor(ApplicationTheme.GameGreen)
                        .TextColor(ApplicationTheme.White)
                        .CornerRadius(16)
                        .HeightRequest(56)
                        .OnClicked(async () =>
                        {
                            await Navigation.PushAsync<HomePage, GameProps>(false, _ =>
                                {
                                    _.DifficultyLevel = 0;
                                });
                        }),
                    Button("Difficult")
                        .FontSize(18)
                        .FontAttributes(FontAttributes.Bold)
                        .BackgroundColor(ApplicationTheme.GameRed)
                        .TextColor(ApplicationTheme.White)
                        .CornerRadius(16)
                        .HeightRequest(56)
                        .OnClicked(async () =>
                        {
                            await Navigation.PushAsync<HomePage, GameProps>(false, _ =>
                                {
                                    _.DifficultyLevel = 1;
                                });
                        })
                )
                .HCenter()
                .GridRow(3)
            )
            .RowSpacing(12)
            .VFill()
            .HFill()
            .Background(ApplicationTheme.OffBlack)
        ).HasNavigationBar(false);
}

public class GameProps
{
    public int DifficultyLevel { get; set; } = 0;
}

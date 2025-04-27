// filepath: /Users/davidortinau/work/dotnet-codebreaker/src/CodeBreaker/Components/HomePage.cs
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using MauiReactor;
using MauiReactor.Shapes;
using CodeBreaker.Resources.Styles;

namespace CodeBreaker.Components;

class GamePageState
{
    public List<Color> SecretCode { get; set; } = new();
    public List<List<Color?>> PreviousGuesses { get; set; } = new();
    public List<List<GuessResult>> GuessResults { get; set; } = new();
    public List<Color?> CurrentGuess { get; set; } = new();
    public int MaxAttempts { get; } = 7;
    public int MaxCodeLength { get; } = 5;
    public bool GameOver { get; set; }
    public bool GameWon { get; set; }

    // Animation properties
    public bool IsAnimatingResults { get; set; } = false;
    public int AnimatingResultsIndex { get; set; } = -1;
    public int AnimatingGuessIndex { get; set; } = -1;
    public bool IsRevealComplete { get; set; } = false;
    public List<Color> AvailableColors { get; } = new()
    {
        // Expanded Atari 2600 palette colors
        ApplicationTheme.GameRed,
        ApplicationTheme.GameGreen,
        ApplicationTheme.GameBlue,
        ApplicationTheme.GameYellow,
        ApplicationTheme.Magenta,
        ApplicationTheme.GameCyan,
        ApplicationTheme.White,
    };

    // Timer properties
    public int TimeLeftSeconds { get; set; } = 120;
    public bool TimerRunning { get; set; } = false;
    public bool TimerFlash { get; set; } = false;
    public System.Timers.Timer? Timer { get; set; }

    // Help overlay property
    public bool ShowHelp { get; set; } = false;

    // Countdown overlay properties
    public bool ShowCountdown { get; set; } = true;
    public int CountdownValue { get; set; } = 3;

    // Paused overlay property
    public bool ShowPaused { get; set; } = false;

    public GamePageState()
    {
        GenerateNewCode();
    }

    private void GenerateNewCode()
    {
        Random rnd = new();
        SecretCode = new List<Color>();

        // Generate secret code using available colors
        // Colors can repeat in the secret code
        for (int i = 0; i < MaxCodeLength; i++)
        {
            int colorIndex = rnd.Next(0, AvailableColors.Count);
            SecretCode.Add(AvailableColors[colorIndex]);
        }
    }
}

// Result of a guess for a single position
public enum GuessResult
{
    Incorrect,      // Wrong color
    WrongPosition,  // Right color, wrong position
    Correct         // Right color, right position
}

partial class GamePage : Component<GamePageState, GameProps>
{

    protected override void OnPropsChanged()
    {
        
        base.OnPropsChanged();
    }

    protected override void OnMounted()
    {
        SetState(s => {
            s.ShowCountdown = true;
            s.CountdownValue = 3;
        });
        StartCountdown();
        base.OnMounted();
    }

    private void StartCountdown()
    {
        var countdownTimer = new System.Timers.Timer(1000);
        countdownTimer.Elapsed += (s, e) =>
        {
            MauiControls.Application.Current?.Dispatcher.Dispatch(() =>
            {
                if (State.CountdownValue > 1)
                {
                    SetState(st => st.CountdownValue--);
                }
                else
                {
                    countdownTimer.Stop();
                    countdownTimer.Dispose();
                    SetState(st => st.ShowCountdown = false);
                    StartTimer();
                }
            });
        };
        countdownTimer.Start();
    }

    private VisualNode RenderPausedOverlay()
    {
        return Grid(
            Border().Background(new SolidColorBrush(ApplicationTheme.Black.WithAlpha(0.92f)))
                .HFill().VFill(),
            VStack(
                Label("PAUSED")
                    .FontFamily("monospace")
                    .FontSize(40)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(ApplicationTheme.Gray100)
                    .HCenter(),
                BoxView().HeightRequest(24),
                Button("Resume")
                    .OnClicked(() => {
                        SetState(s => s.ShowPaused = false);
                        MauiControls.Application.Current?.Dispatcher.Dispatch(() => ResumeTimer());
                    })
                    .BackgroundColor(ApplicationTheme.GameGreen)
                    .TextColor(ApplicationTheme.White)
                    .FontFamily("monospace")
                    .FontSize(20)
                    .FontAttributes(FontAttributes.Bold)
                    .HeightRequest(56)
                    .WidthRequest(160)
                    .HCenter()
            )
            .Padding(32)
            .Center()
        ).GridRowSpan(2).HFill().VFill().ZIndex(150);
    }

    public override VisualNode Render()
        => ContentPage("CODE BREAKER",
            Grid(rows: "*,Auto",
                columns: "*",
                    // Timer display
                    RenderTimer(),
                    // Help button
                    RenderHelpButton(),
                    // Main game content
                    RenderGameBoard(),
                    // Controls section
                    RenderControls(),
                    // Game over overlay (conditionally shown)
                    State.GameOver ? RenderGameOverOverlay() : null,
                    // Help overlay (conditionally shown)
                    State.ShowHelp ? RenderHelpOverlay() : null,
                    // Countdown overlay (conditionally shown)
                    State.ShowCountdown ? RenderCountdownOverlay() : null,
                    // Paused overlay (conditionally shown)
                    State.ShowPaused ? RenderPausedOverlay() : null
            )
        )
        .HasNavigationBar(false)
        .Background(ApplicationTheme.OffBlack); // Dark background

    private VisualNode RenderCountdownOverlay()
    {
        return Grid(
            Border().Background(new SolidColorBrush(ApplicationTheme.Black.WithAlpha(0.85f))).HFill().VFill(),
            Label(State.CountdownValue > 0 ? State.CountdownValue.ToString() : "GO!")
                .FontFamily("monospace")
                .FontSize(96)
                .FontAttributes(FontAttributes.Bold)
                .TextColor(ApplicationTheme.GameGreen)
                .HCenter()
                .VCenter()
        ).HFill().VFill().ZIndex(200).GridRowSpan(2);
    }

    private VisualNode RenderHelpButton()
    {
        return ImageButton()
            .Source(ApplicationTheme.IconInfo)
            .HeightRequest(36)
            .WidthRequest(36)
            .BackgroundColor(Colors.Transparent)
            .HEnd()
            .VStart()
            .Margin(0, 12, 18, 0)
            .ZIndex(10)
            .OnClicked(() => {
                SetState(s => s.ShowHelp = true);
                PauseTimer();
            });
    }

    private VisualNode RenderTimer()
    {
        var isLast10 = State.TimeLeftSeconds <= 10;
        var min = State.TimeLeftSeconds / 60;
        var sec = State.TimeLeftSeconds % 60;
        var color = isLast10 ? ApplicationTheme.GameRed : ApplicationTheme.Gray100;
        var fontWeight = FontAttributes.Bold;
        var opacity = isLast10 && State.TimerFlash ? 0.3 : 1.0;
        return HStack(
            Label($"{min:00}:{sec:00}")
                .FontSize(28)
                .FontAttributes(fontWeight)
                .TextColor(color),
            ImageButton()
                .Source(ApplicationTheme.IconPause)
                .HeightRequest(28)
                .WidthRequest(28)
                .BackgroundColor(Colors.Transparent)
                .OnClicked(() =>
                {
                    SetState(s => s.ShowPaused = true);
                    PauseTimer();
                })
        )
            .Spacing(12)
            .Opacity(opacity)
            .Margin(18, 12, 0, 0)
            .HStart()
            .VStart()
            .ZIndex(10);
    }

    private VisualNode RenderGameBoard()
    {
        return Grid(
            rows: Enumerable.Range(0, State.MaxAttempts).Select(_ => new RowDefinition(GridLength.Star)).ToArray(),
            columns: Enumerable.Range(0, 1).Select(_ => new ColumnDefinition(GridLength.Star)).ToArray(),
            Enumerable.Range(0, State.MaxAttempts).Select(rowIndex =>
                RenderGuessRow(rowIndex, State.MaxAttempts - rowIndex - 1)
            ).ToArray()
        )
        .HCenter()
        .VCenter()
        .RowSpacing(12)
        .Margin(15);
    }

    private VisualNode RenderGuessRow(int rowIndex, int guessIndex)
    {
        // Don't allow advancing to next row while animation is playing
        bool isCurrentRow = guessIndex == State.PreviousGuesses.Count && !State.IsAnimatingResults;
        bool isPastRow = guessIndex < State.PreviousGuesses.Count;
        bool isFutureRow = guessIndex > State.PreviousGuesses.Count;

        return Grid(
            rows: new[] { new RowDefinition(GridLength.Star) },
            columns: new[] { new ColumnDefinition(40), new ColumnDefinition(GridLength.Star), new ColumnDefinition(40) },

            // Center - Pegs
            HStack(spacing: 12,
                Enumerable.Range(0, State.MaxCodeLength).Select(columnIndex =>
                {
                    Color? pegColor = null;

                    if (isPastRow)
                    {
                        pegColor = State.PreviousGuesses[guessIndex][columnIndex];
                    }
                    else if (isCurrentRow && columnIndex < State.CurrentGuess.Count)
                    {
                        pegColor = State.CurrentGuess[columnIndex];
                    }

                    return RenderPegSlot(pegColor, isPastRow, columnIndex, guessIndex);
                }).ToArray()
            )
            .GridColumn(1),

            // Right side - circular indicator  
            !isCurrentRow && isPastRow && Props.DifficultyLevel == 1 ?
                Border(RenderCircularIndicator(guessIndex))
                    .HeightRequest(40)
                    .WidthRequest(40)
                    .Margin(24, 0, 0, 0)
                    .GridColumn(2)
                    .VStart()
                : null
        )
        .GridRow(rowIndex);
    }

    private VisualNode RenderPegSlot(Color? pegColor, bool showResults, int pegIndex, int guessIndex)
    {
        // Check if this is the current row and the next peg to be filled
        bool isCurrentRow = guessIndex == State.PreviousGuesses.Count && !State.IsAnimatingResults;
        bool isNextTargetPeg = isCurrentRow && pegIndex == State.CurrentGuess.Count;

        // Check if this indicator should be showing during animation
        bool isAnimatingThisRow = State.IsAnimatingResults && State.AnimatingGuessIndex == guessIndex;
        bool shouldShowIndicator = showResults && (!isAnimatingThisRow || pegIndex <= State.AnimatingResultsIndex);

        // Is this the currently animating indicator?
        bool isActiveAnimatingIndicator = isAnimatingThisRow && pegIndex == State.AnimatingResultsIndex;

        // Determine if we should show position indicators based on difficulty level
        bool showPositionIndicators = Props.DifficultyLevel == 0;

        return VStack(spacing: 2,
            // Main peg circle
            Border().StrokeThickness(isNextTargetPeg ? 3 : 2)
                .Stroke(pegColor != null ?
                    pegColor.WithLuminosity(0.25f) :
                    (isNextTargetPeg ? ApplicationTheme.Gray200 : ApplicationTheme.Gray400))
                .Background(pegColor ?? ApplicationTheme.Gray900)
                .HeightRequest(40)
                .WidthRequest(40),

            // Fixed height container for indicator - stable height to prevent layout shifts
            Grid(
                rows: new[] { new RowDefinition(GridLength.Auto) },
                columns: new[] { new ColumnDefinition(GridLength.Star) },

                // Result indicator (only visible when needed and difficulty is Easy)
                showPositionIndicators && shouldShowIndicator ?
                Border().StrokeThickness(0)
                    .Background(GetResultColor(State.GuessResults[guessIndex][pegIndex]))
                    .HeightRequest(6)
                    .HFill()
                    // Simplified animation for better performance - just opacity change
                    .Opacity(isActiveAnimatingIndicator ? 0.0 : 1.0)
                    .WithAnimation(duration: 100, easing: Easing.Linear) : null
            )
            .HeightRequest(16) // Fixed height container prevents shifting
            .Margin(0, 4)
        );
    }

    private Color GetResultColor(GuessResult result)
    {
        return result switch
        {
            GuessResult.Correct => ApplicationTheme.GameGreen,       // Vivid Green for correct position
            GuessResult.WrongPosition => Color.FromRgb(0xFF, 0x85, 0x00), // Bright Orange for wrong position
            _ => ApplicationTheme.Gray600                            // Dark gray for incorrect
        };
    }

    private VisualNode RenderCircularIndicator(int guessIndex)
    {
        // Get the guess results for this row
        var results = State.GuessResults[guessIndex];

        // Count the number of correct, wrong position and incorrect results
        int correctCount = results.Count(r => r == GuessResult.Correct);
        int wrongPosCount = results.Count(r => r == GuessResult.WrongPosition);

        // Use dots instead of circular indicator
        return GraphicsView()
            .HeightRequest(40)
            .WidthRequest(40)
            .BackgroundColor(ApplicationTheme.OffBlack)
            .OnDraw((canvas, dirtyRect) =>
            {
                // Draw a border around the dots
                canvas.StrokeColor = ApplicationTheme.Gray400;
                canvas.StrokeSize = 2;
                float containerRadius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2 - 1;
                // canvas.DrawCircle(dirtyRect.Center.X, dirtyRect.Center.Y, containerRadius);

                // Calculate dot positions - arrange in a circle
                int dotsCount = State.MaxCodeLength;
                float dotRadius = 4; // Size of each dot
                float dotCircleRadius = containerRadius - dotRadius - 2; // Radius for positioning dots

                // Draw the dots
                for (int i = 0; i < dotsCount; i++)
                {
                    // Calculate position on the circle
                    float angle = (float)(2 * Math.PI * i / dotsCount);
                    float x = dirtyRect.Center.X + dotCircleRadius * (float)Math.Cos(angle);
                    float y = dirtyRect.Center.Y + dotCircleRadius * (float)Math.Sin(angle);

                    // Determine dot color based on results
                    Color dotColor;
                    if (i < correctCount)
                    {
                        dotColor = ApplicationTheme.GameGreen; // Correct
                    }
                    else if (i < correctCount + wrongPosCount)
                    {
                        dotColor = Color.FromRgb(0xFF, 0x85, 0x00); // Wrong position
                    }
                    else
                    {
                        dotColor = ApplicationTheme.Gray600; // Incorrect
                    }

                    // Draw the dot
                    canvas.FillColor = dotColor;
                    canvas.FillCircle(x, y, dotRadius);
                }
            });
    }

    private VisualNode RenderControls()
    {
        return Grid(
            rows: new[] { new RowDefinition(14), new RowDefinition(GridLength.Auto) },
            columns: Enumerable.Range(0, 1).Select(_ => new ColumnDefinition(GridLength.Star)).ToArray(),
            Grid(
                rows: "auto, auto",
                columns: "*",

                // FIRST ROW - First 4 color buttons
                HStack(spacing: 12,
                    Enumerable.Range(0, 4).Select(colorIndex =>
                        // Create a layered effect for arcade button look
                        Border(
                            Button() // The actual button with color
                                .BackgroundColor(State.AvailableColors[colorIndex].WithSaturation(1.25f))
                                .HeightRequest(44)
                                .WidthRequest(44)
                                .CornerRadius(22)
                                .BorderWidth(3) // Nice thick border for arcade style
                                .BorderColor(State.AvailableColors[colorIndex])
                                .OnClicked(() => AddColorToCurrent(State.AvailableColors[colorIndex]))
                        ) // Outer border - acts as button bezel
                            .StrokeThickness(3)
                            .Stroke(State.AvailableColors[colorIndex])
                            .StrokeShape(RoundRectangle().CornerRadius(32))
                            .Background(ApplicationTheme.OffBlack)
                            .HeightRequest(54)
                            .WidthRequest(54)
                    ).ToArray()
                )
                .GridRow(0)
                .HCenter(),

                // SECOND ROW - Last 3 color buttons
                HStack(spacing: 12,
                    Enumerable.Range(4, 3).Select(colorIndex => Border(
                            Button()
                                .BackgroundColor(State.AvailableColors[colorIndex].WithSaturation(1.25f))
                                .HeightRequest(44)
                                .WidthRequest(44)
                                .CornerRadius(22)
                                .BorderWidth(3)
                                .BorderColor(State.AvailableColors[colorIndex])
                                .OnClicked(() => AddColorToCurrent(State.AvailableColors[colorIndex]))
                        )
                            .StrokeThickness(3)
                            .Stroke(State.AvailableColors[colorIndex])
                            .StrokeShape(RoundRectangle().CornerRadius(32))
                            .Background(ApplicationTheme.OffBlack)
                            .HeightRequest(54)
                            .WidthRequest(54)
                    ).ToArray()
                    
                )
                .GridRow(1)
                .HCenter()
            )
                .GridRow(1)
                .Margin(30, 30),

            BoxView()
                .GridRow(0)
                .BackgroundColor(Colors.Black)
                .VFill()
                .HFill(),

            RenderKeyButton(),
            RenderEraseButton()

        )
        .GridRow(1)
        .VEnd()
        ;
    }

    private VisualNode RenderEraseButton() =>
        VStack(
            Border(

                Button()
                    // .Source(ApplicationTheme.IconEraser)
                    // .Aspect(Aspect.AspectFit)
                    .Padding(4)
                    .OnClicked(EraseLastColor)
                    .Background(ApplicationTheme.Gray900)
                    .HeightRequest(44)
                    .WidthRequest(44)
                    .CornerRadius(22)
                    .BorderWidth(3)
                    .BorderColor(ApplicationTheme.Black).Center()
            )

            .StrokeThickness(3)
            .Stroke(ApplicationTheme.Gray600)
            .StrokeShape(RoundRectangle().CornerRadius(32))
            .Background(ApplicationTheme.OffBlack)
            .HeightRequest(54)
            .WidthRequest(54),
            Label("Erase").Center().TextColor(ApplicationTheme.Gray100).FontSize(12).FontAttributes(FontAttributes.Bold).FontFamily("monospace")
        )
            .Spacing(8)
            .Margin(24)
            .GridRow(1)
            .HStart()
            .VEnd();


    private VisualNode RenderKeyButton() =>
        VStack(
            Border(
                Button()
                    // .Source(State.CurrentGuess.Count == State.MaxCodeLength && !State.GameOver ?
                    //     ApplicationTheme.IconKey : ApplicationTheme.IconKeyDisabled)
                    // .Aspect(Aspect.AspectFit)
                    .Padding(2)
                    .HeightRequest(44)
                    .WidthRequest(44)
                    .CornerRadius(22)
                    .BorderWidth(3)
                    .BorderColor(State.CurrentGuess.Count == State.MaxCodeLength && !State.GameOver ?
                        ApplicationTheme.Primary : ApplicationTheme.Black)
                    .OnClicked(SubmitGuess)
                    .IsEnabled(State.CurrentGuess.Count == State.MaxCodeLength && !State.GameOver)
                    .BackgroundColor(ApplicationTheme.Gray900)
            )
                .StrokeThickness(3)
                .Stroke(State.CurrentGuess.Count == State.MaxCodeLength && !State.GameOver ?
                    ApplicationTheme.White.WithAlpha(0.7f) : ApplicationTheme.Gray600)
                .StrokeShape(RoundRectangle().CornerRadius(32))
                .Background(ApplicationTheme.OffBlack)
                .HeightRequest(54)
                .WidthRequest(54)

                .Shadow(State.CurrentGuess.Count == State.MaxCodeLength && !State.GameOver ?
                    Shadow()
                        .Brush(new SolidColorBrush(ApplicationTheme.White.WithAlpha(0.8f)))
                        .Offset(0, 0)
                        .Radius(8) :
                    Shadow()
                        .Brush(new SolidColorBrush(Colors.Transparent))
                        .Radius(0))
                .WithAnimation(duration: 800, easing: Easing.CubicInOut),
            Label("Guess").Center().TextColor(ApplicationTheme.Gray100).FontSize(12).FontAttributes(FontAttributes.Bold).FontFamily("monospace")
        ).Spacing(8)

            .VEnd()
            .HEnd()
            .Margin(24)
            .GridRow(1)
            ;
    

    private void AddColorToCurrent(Color color)
    {
        if (State.GameOver) return;

        if (State.CurrentGuess.Count < State.MaxCodeLength)
        {
            SetState(s => s.CurrentGuess.Add(color));
        }
    }

    private void EraseLastColor()
    {
        if (State.GameOver) return;

        if (State.CurrentGuess.Count > 0)
        {
            SetState(s => s.CurrentGuess.RemoveAt(s.CurrentGuess.Count - 1));
        }
    }

    private void SubmitGuess()
    {
        if (State.GameOver || State.CurrentGuess.Count < State.MaxCodeLength) return;

        // Initial state setup for results evaluation
        SetState(s =>
        {
            // Create a copy of the current guess
            var guessToCheck = new List<Color?>(s.CurrentGuess);
            s.PreviousGuesses.Add(guessToCheck);

            // Evaluate the guess
            var results = new List<GuessResult>();
            var secretCodeCopy = new List<Color>(s.SecretCode);
            var guessCopy = new List<Color?>(guessToCheck);

            // First check for exact matches (correct color in correct position)
            for (int i = 0; i < s.MaxCodeLength; i++)
            {
                if (guessCopy[i] != null && ColorEquals(guessCopy[i]!, secretCodeCopy[i]))
                {
                    results.Add(GuessResult.Correct);
                    secretCodeCopy[i] = Color.FromArgb("Transparent"); // Mark as matched
                    guessCopy[i] = null; // Mark as matched
                }
                else
                {
                    results.Add(GuessResult.Incorrect); // Placeholder, will update for wrong positions
                }
            }

            // Then check for color matches in wrong positions
            for (int i = 0; i < s.MaxCodeLength; i++)
            {
                if (guessCopy[i] == null) continue; // Skip already matched

                for (int j = 0; j < s.MaxCodeLength; j++)
                {
                    if (secretCodeCopy[j] == Color.FromArgb("Transparent")) continue; // Skip already matched

                    if (ColorEquals(guessCopy[i]!, secretCodeCopy[j]))
                    {
                        results[i] = GuessResult.WrongPosition;
                        secretCodeCopy[j] = Color.FromArgb("Transparent"); // Mark as matched
                        break;
                    }
                }
            }

            s.GuessResults.Add(results);
            // Immediately set animation flags to prevent the next row from becoming active
            s.IsAnimatingResults = true;
            s.AnimatingResultsIndex = -1; // Will be incremented to 0 in animation
            s.AnimatingGuessIndex = s.PreviousGuesses.Count - 1;
            s.IsRevealComplete = false;

            // Clear the current guess
            s.CurrentGuess = new List<Color?>();
        });

        // Start animation immediately - no delay needed since we're using a timer
        MauiControls.Application.Current?.Dispatcher.Dispatch(AnimateNextIndicator);

        // If the guess is correct, stop the timer
        if (State.SecretCode.SequenceEqual(State.CurrentGuess))
        {
            StopTimer();
        }
    }

    private void AnimateNextIndicator()
    {
        // Stop animation if not in animation mode
        if (!State.IsAnimatingResults)
            return;

        // Start a fast timer that will animate each indicator
        // This approach uses a single timer for all animations rather than
        // nested Task.Delay calls which can cause performance issues

        // Create timer with a very short interval for performance
        var timer = new System.Timers.Timer(120);
        timer.AutoReset = true;

        timer.Elapsed += (sender, e) =>
        {
            MauiControls.Application.Current?.Dispatcher.Dispatch(() =>
            {
                // Update the animation index
                SetState(s =>
                {
                    s.AnimatingResultsIndex++;

                    // Check if we've completed all indicators
                    if (s.AnimatingResultsIndex >= s.MaxCodeLength)
                    {
                        // Stop the timer
                        timer.Stop();
                        timer.Dispose();

                        // Set final state
                        var results = s.GuessResults[s.AnimatingGuessIndex];
                        bool isWon = results.All(r => r == GuessResult.Correct);

                        s.IsRevealComplete = true;
                        s.GameWon = isWon;
                        s.GameOver = isWon || s.PreviousGuesses.Count >= s.MaxAttempts;

                        // If game is won, stop the timer
                        if (s.GameWon)
                        {
                            StopTimer();
                        }
                        // If game is over (time out), timer is already stopped in timer logic

                        // Schedule a final update to reset animation state after a short delay
                        Task.Delay(250).ContinueWith(_ =>
                        {
                            MauiControls.Application.Current?.Dispatcher.Dispatch(() =>
                            {
                                SetState(finalState =>
                                {
                                    finalState.IsAnimatingResults = false;
                                    finalState.AnimatingResultsIndex = -1;
                                    finalState.AnimatingGuessIndex = -1;
                                    finalState.IsRevealComplete = false;
                                });
                            });
                        });
                    }
                });
            });
        };

        // Start the timer
        timer.Start();
    }


    private bool ColorEquals(Color c1, Color c2)
    {
        // Simple color comparison based on RGB values
        return Math.Abs(c1.Red - c2.Red) < 0.01 &&
               Math.Abs(c1.Green - c2.Green) < 0.01 &&
               Math.Abs(c1.Blue - c2.Blue) < 0.01;
    }

    private void NewGame()
    {
        SetState(s =>
        {
            s.SecretCode = new List<Color>();
            s.PreviousGuesses = new List<List<Color?>>();
            s.GuessResults = new List<List<GuessResult>>();
            s.CurrentGuess = new List<Color?>();
            s.GameOver = false;
            s.GameWon = false;

            // Generate new secret code
            Random rnd = new();
            for (int i = 0; i < s.MaxCodeLength; i++)
            {
                int colorIndex = rnd.Next(0, s.AvailableColors.Count);
                s.SecretCode.Add(s.AvailableColors[colorIndex]);
            }
        });
    }

    private void RestartGame(int difficulty)
    {
        SetState(s =>
        {
            s.GameOver = false;
            s.GameWon = false;
            s.PreviousGuesses = new List<List<Color?>>();
            s.GuessResults = new List<List<GuessResult>>();
            s.CurrentGuess = new List<Color?>();
            s.TimeLeftSeconds = 120;
            s.TimerRunning = false;
            s.TimerFlash = false;
        });
        StartTimer();
    }

    private VisualNode RenderGameOverOverlay()
    {
        return Grid(
            // Semi-transparent dark background
            Border().Background(new SolidColorBrush(ApplicationTheme.Black.WithAlpha(0.85f)))
                .HFill()
                .VFill(),

            VStack(spacing: 30,
                // Game result text with glowing effect
                Border(
                    Label(State.GameWon ? "YOU WIN!" : "GAME OVER").FontFamily("monospace")
                        .FontSize(36)
                        .FontAttributes(FontAttributes.Bold).TextColor(ApplicationTheme.White)
                        .HCenter()
                )
                .Background(new RadialGradientBrush(new GradientStopCollection
                {
                    new GradientStop(State.GameWon ?
                        ApplicationTheme.Primary.WithLuminosity(0.6f) : // Darker green center for win
                        ApplicationTheme.GameDarkRed,  // Darker red center for loss
                        0.0f),
                    new GradientStop(ApplicationTheme.Black.WithAlpha(0.0f), 1.0f) // Transparent outer edge
                }))
                .HeightRequest(120)
                .WidthRequest(300)
                .StrokeThickness(0),

                // Show secret code if game is lost
                !State.GameWon ?
                    VStack(spacing: 10,
                        Label("SECRET CODE:")
                            .FontFamily("monospace")
                            .FontAttributes(FontAttributes.Bold).TextColor(ApplicationTheme.Gray100)
                            .HCenter(),

                        HStack(spacing: 8,
                            Enumerable.Range(0, State.MaxCodeLength).Select(i =>
                                Border().StrokeThickness(2)
                                    .Stroke(ApplicationTheme.Gray400)
                                    .Background(State.SecretCode[i])
                                    .HeightRequest(40)
                                    .WidthRequest(40)
                            ).ToArray()
                        )
                        .HCenter()
                    ) : null,

                // Play again buttons with difficulty selection
                HStack(spacing: 20,
                    Button("EASY")
                        .OnClicked(() => RestartGame(0))
                        .BackgroundColor(ApplicationTheme.GameGreen)
                        .TextColor(ApplicationTheme.White)
                        .FontFamily("monospace")
                        .FontSize(20)
                        .FontAttributes(FontAttributes.Bold)
                        .HeightRequest(60)
                        .WidthRequest(120)
                        .BorderWidth(4)
                        .BorderColor(ApplicationTheme.Gray400),
                    Button("DIFFICULT")
                        .OnClicked(() => RestartGame(1))
                        .BackgroundColor(ApplicationTheme.GameRed)
                        .TextColor(ApplicationTheme.White)
                        .FontFamily("monospace")
                        .FontSize(20)
                        .FontAttributes(FontAttributes.Bold)
                        .HeightRequest(60)
                        .WidthRequest(160)
                        .BorderWidth(4)
                        .BorderColor(ApplicationTheme.Gray400)
                )
                .HCenter()
            )
            .Center()
        )
        .GridRowSpan(2)
        .ZIndex(100)
        .HFill()
        .VFill();
    }

    private VisualNode RenderHelpOverlay()
    {
        return Grid(
            Border().Background(new SolidColorBrush(ApplicationTheme.Black.WithAlpha(0.85f)))
                .HFill().VFill(),
            VStack(
                Label("AGENT INSTRUCTIONS")
                    .FontFamily("monospace")
                    .FontSize(28)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(ApplicationTheme.GameGreen)
                    .HCenter(),
                BoxView().HeightRequest(12),
                Label("Your mission, should you choose to accept it:\n\n- Crack the secret color code before time runs out.\n- Each row is a guess. Tap colors to build your code, then tap the key to submit.\n- Green means a color is correct and in the right place. Orange means a color is correct but in the wrong place. Gray means it's not in the code.\n- You have limited attempts and only 2 minutes.\n- When the timer flashes red, time is almost up.\n- Choose your difficulty wisely, Agent.\n\nGood luck. This message will self-destruct in 2 minutes.")
                    .FontFamily("monospace")
                    .FontSize(18)
                    .TextColor(ApplicationTheme.Gray100)
                    .HCenter(),
                BoxView().HeightRequest(24),
                Button("Close")
                    .OnClicked(() => {
                        SetState(s => s.ShowHelp = false);
                        MauiControls.Application.Current?.Dispatcher.Dispatch(() => ResumeTimer());
                    })
                    .BackgroundColor(ApplicationTheme.GameRed)
                    .TextColor(ApplicationTheme.White)
                    .FontFamily("monospace")
                    .FontSize(18)
                    .FontAttributes(FontAttributes.Bold)
                    .HeightRequest(48)
                    .WidthRequest(120)
                    .HCenter()
            )
            .Padding(24)
            .Center()
        ).GridRowSpan(2).HFill().VFill().ZIndex(100);
    }

    private void StartTimer()
    {
        if (State.Timer != null)
        {
            State.Timer.Stop();
            State.Timer.Dispose();
        }
        State.TimeLeftSeconds = 120;
        State.TimerRunning = true;
        State.TimerFlash = false;
        State.Timer = new System.Timers.Timer(1000);
        State.Timer.Elapsed += (s, e) =>
        {
            MauiControls.Application.Current?.Dispatcher.Dispatch(() =>
            {
                if (!State.TimerRunning || State.GameOver || State.GameWon)
                {
                    State.Timer?.Stop();
                    return;
                }
                SetState(st =>
                {
                    st.TimeLeftSeconds--;
                    if (st.TimeLeftSeconds <= 0)
                    {
                        st.TimeLeftSeconds = 0;
                        st.GameOver = true;
                        st.TimerRunning = false;
                        st.Timer?.Stop();
                    }
                    else if (st.TimeLeftSeconds <= 10)
                    {
                        st.TimerFlash = !st.TimerFlash;
                    }
                });
            });
        };
        State.Timer.Start();
    }

    private void StopTimer()
    {
        State.TimerRunning = false;
        State.Timer?.Stop();
    }

    private void PauseTimer()
    {
        State.TimerRunning = false;
    }

    private void ResumeTimer()
    {
        if (!State.GameOver && !State.GameWon && State.TimeLeftSeconds > 0)
        { 
            State.TimerRunning = true; 
            State.Timer?.Start();         
        }
    }
}

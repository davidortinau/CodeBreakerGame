namespace CodeBreaker.Components;

using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using MauiReactor;
using MauiReactor.Shapes;

class HomePageState
{
    public List<Color> SecretCode { get; set; } = new();
    public List<List<Color?>> PreviousGuesses { get; set; } = new();
    public List<List<GuessResult>> GuessResults { get; set; } = new();
    public List<Color?> CurrentGuess { get; set; } = new();
    public int MaxAttempts { get; } = 7;
    public int MaxCodeLength { get; } = 4;
    public bool GameOver { get; set; }
    public bool GameWon { get; set; }

    public List<Color> AvailableColors { get; } = new() 
    {
        // Atari 2600 palette colors
        Color.FromRgb(0xCC, 0x33, 0x33),  // Red
        Color.FromRgb(0x33, 0xCC, 0x33),  // Green
        Color.FromRgb(0x33, 0x66, 0xCC),  // Blue
        Color.FromRgb(0xCC, 0xCC, 0x33),  // Yellow
    };

    public HomePageState()
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

partial class HomePage : Component<HomePageState>
{
    public override VisualNode Render()
        => ContentPage("CODE BREAKER",
            VStack(spacing: 15,
                // Game board with guess rows
                RenderGameBoard(),
                
                // Controls section
                RenderControls()
            )
            .Padding(15)
            .BackgroundColor(Color.FromRgb(0x22, 0x22, 0x22)) // Dark Atari background
        )
        .BackgroundColor(Color.FromRgb(0x22, 0x22, 0x22)); // Dark Atari background

    private VisualNode RenderGameBoard()
    {
        return Grid(
            rows: Enumerable.Range(0, State.MaxAttempts).Select(_ => new RowDefinition(GridLength.Star)).ToArray(),
            columns: Enumerable.Range(0, 1).Select(_ => new ColumnDefinition(GridLength.Star)).ToArray(),
            Enumerable.Range(0, State.MaxAttempts).Select(rowIndex => 
                RenderGuessRow(rowIndex, State.MaxAttempts - rowIndex - 1)
            ).ToArray()
        )
        .Margin(0, 10);
    }

    private VisualNode RenderGuessRow(int rowIndex, int guessIndex)
    {
        bool isCurrentRow = guessIndex == State.PreviousGuesses.Count;
        bool isPastRow = guessIndex < State.PreviousGuesses.Count;
        bool isFutureRow = guessIndex > State.PreviousGuesses.Count;

        return Grid(
            rows: new[] { new RowDefinition(GridLength.Star) },
            columns: new[] { new ColumnDefinition(GridLength.Star) },
            HStack(spacing: 8,
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
            .VCenter()
            .HCenter()
        )
        .GridRow(rowIndex);
    }

    private VisualNode RenderPegSlot(Color? pegColor, bool showResults, int pegIndex, int guessIndex)
    {
        return VStack(spacing: 2,
            Border()
                .StrokeThickness(2)
                .Stroke(Color.FromRgb(0x88, 0x88, 0x88))
                .Background(pegColor ?? Color.FromRgb(0x33, 0x33, 0x33))
                .HeightRequest(40)
                .WidthRequest(40),
                  showResults ?
                  Border()
                        .StrokeThickness(0)
                        .Stroke(Color.FromRgb(0x66, 0x66, 0x66))
                        .Background(new LinearGradientBrush(new GradientStopCollection
                        {
                            new GradientStop(GetResultColor(State.GuessResults[guessIndex][pegIndex]).WithLuminosity(0.2f), 0.0f),
                            new GradientStop(GetResultColor(State.GuessResults[guessIndex][pegIndex]), 0.2f),
                            new GradientStop(GetResultColor(State.GuessResults[guessIndex][pegIndex]), 0.8f),
                            new GradientStop(GetResultColor(State.GuessResults[guessIndex][pegIndex]).WithLuminosity(0.2f), 1.0f)
                        }))
                        .HeightRequest(5)
                        .HFill()
                    
                .Margin(0, 4) :
                Label("")
                .HeightRequest(16)
        );
    }

    private Color GetResultColor(GuessResult result)
    {
        return result switch
        {
            GuessResult.Correct => Color.FromRgb(0x33, 0xCC, 0x33),      // Green Color.FromRgb(0x33, 0xCC, 0x33)
            GuessResult.WrongPosition => Color.FromRgb(0xCC, 0xAA, 0x33), // Orange/Amber
            _ => Color.FromRgb(0x44, 0x44, 0x44)                         // Dark gray
        };
    }

    private VisualNode RenderControls()
    {
        return Grid(
            rows: new[] { new RowDefinition(GridLength.Auto) },
            columns: Enumerable.Range(0, 1).Select(_ => new ColumnDefinition(GridLength.Star)).ToArray(),
            VStack(spacing: 20,                // Color selection buttons - classic arcade style
                HStack(spacing: 15,
                    Enumerable.Range(0, State.AvailableColors.Count).Select(colorIndex => 
                        
                            // Create a layered effect for arcade button look
                            Border(
                                Button() // The actual button with color
                                    .BackgroundColor(State.AvailableColors[colorIndex].WithSaturation(1.25f))
                                    .HeightRequest(54)
                                    .WidthRequest(54)
                                    .CornerRadius(27)
                                    .BorderWidth(3) // Nice thick border for arcade style
                                    .BorderColor(State.AvailableColors[colorIndex]) // Slightly desaturated for the border
                                    .OnClicked(() => AddColorToCurrent(State.AvailableColors[colorIndex]))
                            ) // Outer border - acts as button bezel
                                .StrokeThickness(3)
                                .Stroke(State.AvailableColors[colorIndex])
                                .StrokeShape(RoundRectangle().CornerRadius(32))
                                .Background(Color.FromRgb(0x11, 0x11, 0x11))
                                .HeightRequest(64)
                                .WidthRequest(64)                              
                            
                        
                    ).ToArray()
                )
                .HCenter(),

                // Action buttons (Submit and Erase)
                HStack(spacing: 15,
                    Button("ERASE")
                    .OnClicked(EraseLastColor)
                    .BackgroundColor(Color.FromRgb(0x22, 0x22, 0xAA)) // Blue Atari color
                    .HeightRequest(50)
                    .WidthRequest(110)
                    .FontFamily("monospace")
                    .FontAttributes(FontAttributes.Bold)
                    .FontSize(16)
                    .TextColor(Color.FromRgb(0xDD, 0xDD, 0xDD)),
                    
                    Button("SUBMIT")
                    .OnClicked(SubmitGuess)
                    .IsEnabled(State.CurrentGuess.Count == State.MaxCodeLength && !State.GameOver)
                    .BackgroundColor(State.CurrentGuess.Count == State.MaxCodeLength && !State.GameOver ? 
                        Color.FromRgb(0xAA, 0x22, 0x22) : Color.FromRgb(0x66, 0x22, 0x22)) // Red Atari color
                    .HeightRequest(50)
                    .WidthRequest(110)
                    .FontFamily("monospace")
                    .FontAttributes(FontAttributes.Bold)
                    .FontSize(16)
                    .TextColor(Color.FromRgb(0xDD, 0xDD, 0xDD))
                )
                .HCenter(),
                
                // Game status message
                State.GameOver ? 
                    Label(State.GameWon ? "YOU WIN! PLAY AGAIN?" : "GAME OVER! PLAY AGAIN?")
                    .TextColor(State.GameWon ? Color.FromRgb(0x33, 0xCC, 0x33) : Color.FromRgb(0xCC, 0x33, 0x33))
                    .FontFamily("monospace")
                    .FontAttributes(FontAttributes.Bold)
                    .FontSize(20)
                    .HCenter() : 
                    Label("")
                    .HeightRequest(20),
                
                // Reset game button (shown when game is over)
                State.GameOver ? 
                    Button("NEW GAME")
                    .OnClicked(NewGame)
                    .BackgroundColor(Color.FromRgb(0x33, 0xCC, 0x33))
                    .HeightRequest(50)
                    .WidthRequest(150)
                    .FontFamily("monospace")
                    .FontAttributes(FontAttributes.Bold)
                    .FontSize(16)
                    .TextColor(Color.FromRgb(0, 0, 0))
                    .HCenter() : 
                    Label("")
                    .HeightRequest(20)
            )
        )
        .Margin(0, 10)
        .Padding(10)
        .BackgroundColor(Color.FromRgb(0x33, 0x33, 0x33)); // Slightly lighter than background
    }
    
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
            
            // Check if game is won
            bool isWon = results.All(r => r == GuessResult.Correct);
            s.GameWon = isWon;
            
            // Check if game is over (won or max attempts reached)
            s.GameOver = isWon || s.PreviousGuesses.Count >= s.MaxAttempts;
            
            // Reset current guess
            s.CurrentGuess = new List<Color?>();
        });
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
}

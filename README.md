# CodeBreaker: Retro Arcade Edition

![CodeBreaker Logo](CodeBreaker/Resources/AppIcon/appicon.svg)

## 🕹️ TOP SECRET MISSION BRIEFING 🕹️

**Agent,**

Welcome to **CodeBreaker: Retro Arcade Edition** - a modern take on the classic code-breaking game with an authentic Atari 2600 aesthetic. Your mission, should you choose to accept it, is to crack the secret color code before your attempts run out.

## Game Features

- **Classic Atari-inspired UI** with pixelated visuals and glowing arcade-style buttons
- **7 distinct colors** to choose from, creating 2,401 possible combinations
- **Intuitive feedback system** showing correct colors and positions
- **Progressive difficulty** as you try to deduce the pattern
- **Dramatic game over sequences** for both victory and defeat
- **Cross-platform support** for iOS, Android, macOS, and Windows

## How to Play

1. **Break the Code**: You have 7 attempts to guess the hidden sequence of 4 colored pegs
2. **Make Your Guess**: Select colors from the 7 available options using the arcade-style buttons
3. **Receive Feedback**: After each guess, indicators will show:
   - **Green**: Correct color in correct position
   - **Amber**: Correct color in wrong position
   - **Gray**: Color not in the secret code
4. **Deduce the Pattern**: Use logic and deduction to narrow down the possibilities
5. **Win the Game**: Successfully identify all colors in their correct positions before running out of attempts

## Arcade Control Panel

The game features an authentic arcade-style control layout:
- **Color Selection**: 4 buttons on the top row, 3 buttons on the bottom row
- **Action Controls**: Submit and Erase buttons stacked to the right
- **Retro Visuals**: Glowing indicators and classic Atari 2600 color palette

## Technologies

Built with:
- **.NET MAUI** for cross-platform deployment
- **MauiReactor** library using the Model-View-Update (MVU) pattern
- **C#** for robust game logic
- **Fluent UI** icons for additional visual styling

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/dotnet-codebreaker.git
   ```

2. Open the solution in Visual Studio or your preferred IDE:
   ```bash
   cd dotnet-codebreaker/src
   dotnet build
   ```

3. Run the application on your preferred platform:
   ```bash
   dotnet run
   ```

## Development

The project follows a reactive UI pattern using MauiReactor:
- **Stateful Components**: Handle the game state and logic
- **Declarative UI**: UI elements are expressed with fluent methods
- **Hot Reload**: Supports hot reload during development

## Screenshots

*[Insert screenshots of the game here]*

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Inspired by classic code-breaking games and the iconic Atari 2600 aesthetic
- Built with MauiReactor by [adospace](https://github.com/adospace/reactorui-maui)

---

**This README will self-destruct after reading...**

*...or not, but the code definitely won't crack itself! Happy code breaking, Agent.*

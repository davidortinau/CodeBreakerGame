// filepath: /Users/davidortinau/work/dotnet-codebreaker/src/CodeBreaker/Components/CircularIndicator.cs
using Microsoft.Maui.Graphics;
using MauiReactor;
using System.Collections.Generic;
using System.Linq;
using CodeBreaker.Resources.Styles;

namespace CodeBreaker.Components;

partial class CircularIndicator : Component
{
    [Prop]
    IList<GuessResult> _results = new List<GuessResult>();

    [Prop]
    int _maxCodeLength;

    public override VisualNode Render()
    {
        // Count the number of correct, wrong-position, and incorrect results
        int correctCount = _results.Count(r => r == GuessResult.Correct);
        int wrongPosCount = _results.Count(r => r == GuessResult.WrongPosition);
        int segmentCount = _maxCodeLength;

        return GraphicsView()
            .HeightRequest(40)
            .WidthRequest(40)
            .BackgroundColor(ApplicationTheme.OffBlack)
            .OnDraw((canvas, dirtyRect) =>
            {
                // center point
                var center = dirtyRect.Center;
                float cx = center.X, cy = center.Y;

                // 1) Draw the container circle
                float containerStroke = 3f;
                canvas.StrokeSize = containerStroke;
                canvas.StrokeColor = ApplicationTheme.Gray400; // Use a visible stroke color
                float containerRadius = (Math.Min(dirtyRect.Width, dirtyRect.Height) - 4) / 2 - containerStroke / 2;
                canvas.DrawCircle(cx, cy, containerRadius);
                canvas.ResetStroke(); // Reset after container circle

                // nothing more to draw if we have no segments
                if (segmentCount <= 0)
                    return;

                // 2) Prepare segment parameters
                const float gapDegrees = 4f;                 // gap between segments (smaller gap)
                const float segmentWidth = 6f;               // stroke thickness
                float segmentAngle = 360f / segmentCount;    // full slice size
                float sweepAngle = segmentAngle - gapDegrees;
                float segmentRadius = containerRadius - segmentWidth / 2;

                canvas.StrokeSize = segmentWidth;
                canvas.StrokeLineCap = LineCap.Round;        // Round caps help fill the gaps

                // 3) Draw each colored arc
                for (int i = 0; i < segmentCount; i++)
                {
                    // pick color by result
                    Color color;
                    if (i < correctCount) color = ApplicationTheme.GameGreen;
                    else if (i < correctCount + wrongPosCount) color = Color.FromRgb(0xFF, 0x85, 0x00);
                    else color = ApplicationTheme.Gray600;

                    canvas.StrokeColor = color;

                    // compute the start angle (−90° so we begin at 12 o'clock)
                    float startAngle = -90f + (segmentAngle * i) + (gapDegrees / 2f);

                    // bounding box for the arc
                    var arcRect = new RectF(
                        cx - segmentRadius,
                        cy - segmentRadius,
                        segmentRadius * 2,
                        segmentRadius * 2);

                    // Draw the arc segment
                    canvas.DrawArc(arcRect, startAngle, sweepAngle, false, false);
                }
                
                // Reset after all segments are drawn
                canvas.ResetStroke();
            });
    }
}
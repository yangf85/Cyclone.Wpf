using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public class HighlightTextBlock : TextBlock
{
    private static IEnumerable<Fragment> SplitTextByOrderedDisjointIntervals(string sourceText, List<Range> mergedIntervals)
    {
        if (string.IsNullOrEmpty(sourceText)) yield break;

        if (!mergedIntervals?.Any() ?? true)
        {
            yield return new Fragment { Text = sourceText, IsQuery = false };
            yield break;
        }

        var range0 = mergedIntervals.First();
        int start0 = range0.Start;
        int end0 = range0.End;

        if (start0 > 0) yield return new Fragment { Text = sourceText.Substring(0, start0), IsQuery = false };
        yield return new Fragment { Text = sourceText.Substring(start0, end0 - start0), IsQuery = true };

        int previousEnd = end0;
        foreach (var range in mergedIntervals.Skip(1))
        {
            int start = range.Start;
            int end = range.End;
            yield return new Fragment { Text = sourceText.Substring(previousEnd, start - previousEnd), IsQuery = false };
            yield return new Fragment { Text = sourceText.Substring(start, end - start), IsQuery = true };
            previousEnd = end;
        }

        if (previousEnd < sourceText.Length)
            yield return new Fragment { Text = sourceText.Substring(previousEnd), IsQuery = false };
    }

    private static List<Range> MergeIntervals(List<Range> intervals)
    {
        if (!intervals?.Any() ?? true) return new List<Range>();

        intervals.Sort((x, y) => x.Start != y.Start ? x.Start - y.Start : x.End - y.End);

        var pointer = intervals[0];
        int startPointer = pointer.Start;
        int endPointer = pointer.End;

        var result = new List<Range>();
        foreach (var range in intervals.Skip(1))
        {
            int start = range.Start;
            int end = range.End;

            if (start <= endPointer)
            {
                if (endPointer < end)
                {
                    endPointer = end;
                }
            }
            else
            {
                result.Add(new Range { Start = startPointer, End = endPointer });
                startPointer = start;
                endPointer = end;
            }
        }
        result.Add(new Range { Start = startPointer, End = endPointer });
        return result;
    }

    private IEnumerable<Range> GetQueryIntervals(string sourceText, string query)
    {
        if (string.IsNullOrEmpty(sourceText) || string.IsNullOrEmpty(query)) yield break;

        int nextStartIndex = 0;
        var comparison = IsIgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
        while (nextStartIndex < sourceText.Length)
        {
            int index = sourceText.IndexOf(query, nextStartIndex, comparison);

            if (index == -1) yield break;

            nextStartIndex = index + query.Length;
            yield return new Range { Start = index, End = nextStartIndex };
        }
    }

    private void RefreshInlines()
    {
        Inlines.Clear();

        if (string.IsNullOrEmpty(SourceText)) return;
        if (string.IsNullOrEmpty(QueriesText))
        {
            Inlines.Add(SourceText);
            return;
        }

        var sourceText = SourceText;
        var queries = QueriesText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var intervals = from query in queries.Distinct()
                        from interval in GetQueryIntervals(sourceText, query)
                        select interval;
        var mergedIntervals = MergeIntervals(intervals.ToList());
        var fragments = SplitTextByOrderedDisjointIntervals(sourceText, mergedIntervals);

        Inlines.AddRange(GenerateRunElement(fragments));
    }

    private IEnumerable GenerateRunElement(IEnumerable<Fragment> fragments)
    {
        return from item in fragments
               select item.IsQuery
                   ? GetHighlightRun(item.Text)
                   : new Run(item.Text);
    }

    private Run GetHighlightRun(string highlightText)
    {
        var run = new Run(highlightText);

        run.SetBinding(TextElement.BackgroundProperty, new Binding(nameof(HighlightBackground)) { Source = this });
        run.SetBinding(TextElement.ForegroundProperty, new Binding(nameof(HighlightForeground)) { Source = this });
        return run;
    }

    private struct Fragment
    {
        public string Text { get; set; }

        public bool IsQuery { get; set; }
    }

    private struct Range
    {
        public int Start { get; set; }

        public int End { get; set; }
    }

    #region SourceText

    public static readonly DependencyProperty SourceTextProperty =
        DependencyProperty.Register(nameof(SourceText), typeof(string), typeof(HighlightTextBlock), new PropertyMetadata(null, OnSourceTextChanged));

    public string SourceText
    {
        get => (string)GetValue(SourceTextProperty);
        set => SetValue(SourceTextProperty, value);
    }

    private static void OnSourceTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
                                               ((HighlightTextBlock)d).RefreshInlines();

    #endregion SourceText

    #region QueriesText

    public static readonly DependencyProperty QueriesTextProperty =
        DependencyProperty.Register(nameof(QueriesText), typeof(string), typeof(HighlightTextBlock), new PropertyMetadata(null, OnQueriesTextChanged));

    public string QueriesText
    {
        get => (string)GetValue(QueriesTextProperty);
        set => SetValue(QueriesTextProperty, value);
    }

    private static void OnQueriesTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((HighlightTextBlock)d).RefreshInlines();
    }

    #endregion QueriesText

    #region HighlightBackground

    public static readonly DependencyProperty HighlightBackgroundProperty =
        DependencyProperty.Register(nameof(HighlightBackground), typeof(Brush), typeof(HighlightTextBlock), new PropertyMetadata(Brushes.Transparent));

    public Brush HighlightBackground
    {
        get => (Brush)GetValue(HighlightBackgroundProperty);
        set => SetValue(HighlightBackgroundProperty, value);
    }

    #endregion HighlightBackground

    #region HighlightForeground

    public static readonly DependencyProperty HighlightForegroundProperty =
        DependencyProperty.Register(nameof(HighlightForeground), typeof(Brush), typeof(HighlightTextBlock), new PropertyMetadata(Brushes.DarkGray));

    public Brush HighlightForeground
    {
        get => (Brush)GetValue(HighlightForegroundProperty);
        set => SetValue(HighlightForegroundProperty, value);
    }

    #endregion HighlightForeground

    #region IsIgnoreCase

    public static readonly DependencyProperty IsIgnoreCaseProperty =
        DependencyProperty.Register(nameof(IsIgnoreCase), typeof(bool), typeof(HighlightTextBlock), new PropertyMetadata(true, OnIsIgnoreCaseChanged));

    public bool IsIgnoreCase
    {
        get => (bool)GetValue(IsIgnoreCaseProperty);
        set => SetValue(IsIgnoreCaseProperty, value);
    }

    private static void OnIsIgnoreCaseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((HighlightTextBlock)d).RefreshInlines();
    }

    #endregion IsIgnoreCase
}
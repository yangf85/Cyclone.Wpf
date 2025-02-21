using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 间距堆栈面板
    /// </summary>
    public class SpacingStackPanel : Panel
    {
        private static void OnCellAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Visual child = d as Visual;

            if (child != null)
            {
                SpacingStackPanel panel = VisualTreeHelper.GetParent(child) as SpacingStackPanel;
                if (panel != null)
                {
                    panel.InvalidateMeasure();
                }
            }
        }

        private CalcStarInfo GetStarArrangeInfo(Size arrangeSize)
        {
            IEnumerable<UIElement> controls = InternalChildren.Cast<UIElement>();
            CalcStarInfo info = new CalcStarInfo();

            //先计算star的高度
            double totalSpaceLen = (InternalChildren.Count - 1) * Spacing;
            if (totalSpaceLen < 0)
            {
                totalSpaceLen = 0;
            }
            info.TotalSpaceLen = totalSpaceLen;
            if (Orientation == Orientation.Horizontal)
            {
                info.OtherTotalLen = controls.Where(p => !GetWeight(p).IsStar).Sum(p => p.DesiredSize.Width);
                info.StarCount = controls.Where(p => GetWeight(p).IsStar).Sum(p => GetWeight(p).Value);
                info.StarAvaLen = arrangeSize.Width - info.OtherTotalLen - totalSpaceLen;
                info.StarUnitLen = 0;
                if (info.StarAvaLen > 0)
                {
                    info.StarUnitLen = info.StarAvaLen / info.StarCount;
                }
            }
            else
            {
                info.OtherTotalLen = controls.Where(p => !GetWeight(p).IsStar).Sum(p => p.DesiredSize.Height);
                info.StarCount = controls.Where(p => GetWeight(p).IsStar).Sum(p => GetWeight(p).Value);
                info.StarAvaLen = arrangeSize.Height - info.OtherTotalLen - totalSpaceLen;
                info.StarUnitLen = 0;
                if (info.StarAvaLen > 0)
                {
                    info.StarUnitLen = info.StarAvaLen / info.StarCount;
                }
            }
            return info;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            IEnumerable<UIElement> controls = InternalChildren.Cast<UIElement>();
            var rcChild = new Rect(arrangeSize);
            var previousChildSize = 0.0;
            CalcStarInfo info = GetStarArrangeInfo(arrangeSize);

            if (Orientation == Orientation.Horizontal)
            {
                foreach (var child in controls)
                {
                    double remainWidth = arrangeSize.Width - rcChild.X;
                    GridLength size = GetWeight(child);
                    if (size.IsStar)
                    {
                        previousChildSize = size.Value * info.StarUnitLen;
                    }
                    else
                    {
                        previousChildSize = remainWidth <= 0 ? 0 : Math.Min(remainWidth, child.DesiredSize.Width);
                    }

                    rcChild.Width = previousChildSize;
                    rcChild.Height = Math.Max(arrangeSize.Height, child.DesiredSize.Height);
                    child.Arrange(rcChild);
                    rcChild.X += previousChildSize + Spacing;
                    rcChild.X = Math.Min(rcChild.X, arrangeSize.Width);
                }
            }
            else
            {
                foreach (var child in controls)
                {
                    double remainHeight = arrangeSize.Height - rcChild.Y;
                    GridLength size = GetWeight(child);
                    if (size.IsStar)
                    {
                        previousChildSize = size.Value * info.StarUnitLen;
                    }
                    else
                    {
                        previousChildSize = remainHeight <= 0 ? 0 : Math.Min(remainHeight, child.DesiredSize.Height);
                    }

                    rcChild.Height = previousChildSize;
                    rcChild.Width = Math.Max(arrangeSize.Width, child.DesiredSize.Width);
                    child.Arrange(rcChild);
                    rcChild.Y += previousChildSize + Spacing;
                    rcChild.Y = Math.Min(rcChild.Y, arrangeSize.Height);
                }
            }

            return arrangeSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            IEnumerable<UIElement> controls = InternalChildren.Cast<UIElement>();
            var stackDesiredSize = new Size();
            Size childConstraint = constraint;
            if (Orientation == Orientation.Horizontal)
            {
                childConstraint.Width = constraint.Width;
                var children1 = controls.Where(p => !GetWeight(p).IsStar).ToList();
                foreach (var ue in children1)
                {
                    ue.Measure(childConstraint);
                    stackDesiredSize.Height = Math.Max(stackDesiredSize.Height, ue.DesiredSize.Height);
                    stackDesiredSize.Width += ue.DesiredSize.Width;
                }
                CalcStarInfo info = GetStarArrangeInfo(constraint);
                var children2 = controls.Where(p => GetWeight(p).IsStar).ToList();
                foreach (var ue in children2)
                {
                    GridLength weight = GetWeight(ue);
                    childConstraint = constraint;
                    childConstraint.Width = info.StarUnitLen * weight.Value;
                    ue.Measure(childConstraint);
                    stackDesiredSize.Height = Math.Max(stackDesiredSize.Height, ue.DesiredSize.Height);
                    stackDesiredSize.Width += ue.DesiredSize.Width;
                }
                stackDesiredSize.Width += info.TotalSpaceLen;
                stackDesiredSize.Width = Math.Min(stackDesiredSize.Width, constraint.Width);
            }
            else
            {
                childConstraint.Height = constraint.Height;
                var children1 = controls.Where(p => !GetWeight(p).IsStar).ToList();
                foreach (var ue in children1)
                {
                    ue.Measure(childConstraint);
                    stackDesiredSize.Width = Math.Max(stackDesiredSize.Width, ue.DesiredSize.Width);
                    stackDesiredSize.Height += ue.DesiredSize.Height;
                }
                CalcStarInfo info = GetStarArrangeInfo(constraint);
                var children2 = controls.Where(p => GetWeight(p).IsStar).ToList();
                foreach (var ue in children2)
                {
                    GridLength weight = GetWeight(ue);
                    childConstraint = constraint;
                    childConstraint.Height = info.StarUnitLen * weight.Value;
                    ue.Measure(childConstraint);
                    stackDesiredSize.Width = Math.Max(stackDesiredSize.Width, ue.DesiredSize.Width);
                    stackDesiredSize.Height += ue.DesiredSize.Height;
                }
                stackDesiredSize.Height += info.TotalSpaceLen;
                stackDesiredSize.Height = Math.Min(stackDesiredSize.Height, constraint.Height);
            }

            return stackDesiredSize;
        }

        private class CalcStarInfo
        {
            public double OtherTotalLen { get; set; }

            public double StarAvaLen { get; set; }

            public double StarCount { get; set; }

            public double StarUnitLen { get; set; }

            public double TotalSpaceLen { get; set; }
        }

        #region Spacing

        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
            nameof(Spacing), typeof(double), typeof(SpacingStackPanel),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        #endregion Spacing

        #region Orientation

        public static readonly DependencyProperty OrientationProperty =
                  StackPanel.OrientationProperty.AddOwner(typeof(SpacingStackPanel),
              new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        #endregion Orientation

        #region Weight

        public static readonly DependencyProperty WeightProperty = DependencyProperty.RegisterAttached(
           "Weight", typeof(GridLength), typeof(SpacingStackPanel),
           new FrameworkPropertyMetadata(GridLength.Auto, OnCellAttachedPropertyChanged));

        public static GridLength GetWeight(UIElement ue)
        {
            return (GridLength)ue.GetValue(WeightProperty);
        }

        public static void SetWeight(UIElement ue, GridLength gridLength)
        {
            ue.SetValue(WeightProperty, gridLength);
        }

        #endregion Weight
    }
}
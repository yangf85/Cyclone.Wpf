using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 鱼眼效果面板，以网格方式排列子元素，鼠标悬停时放大当前元素并缩小周围元素，
    /// 创建类似鱼眼镜头的视觉效果。当元素放大时，周围元素会相应移动以避免重叠。
    /// </summary>
    /// <remarks>
    /// <para>实现思路：</para>
    /// <para>1. 采用网格布局方式，支持自动行列数调整或固定行列数</para>
    /// <para>2. 使用缩放和位移变换实现元素的鱼眼效果</para>
    /// <para>3. 当元素放大时，周围元素适当移动以避免重叠</para>
    /// <para>4. 鼠标离开时所有元素平滑恢复到原始位置和大小</para>
    /// <para>5. 通过多个属性提供灵活的效果定制</para>
    /// </remarks>
    public class FisheyePanel : Panel
    {
        #region 依赖属性

        /// <summary>
        /// 列数依赖属性
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(int), typeof(FisheyePanel),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// 获取或设置网格的列数（0表示自动计算）
        /// </summary>
        public int Columns
        {
            get => (int)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 行数依赖属性
        /// </summary>
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(nameof(Rows), typeof(int), typeof(FisheyePanel),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// 获取或设置网格的行数（0表示自动计算）
        /// </summary>
        public int Rows
        {
            get => (int)GetValue(RowsProperty);
            set => SetValue(RowsProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 最大缩放值依赖属性
        /// </summary>
        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.Register(nameof(MaxScale), typeof(double), typeof(FisheyePanel),
                new PropertyMetadata(2.0));

        /// <summary>
        /// 获取或设置鼠标悬停时的最大缩放值
        /// </summary>
        public double MaxScale
        {
            get => (double)GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, Math.Max(1.0, value));
        }

        /// <summary>
        /// 最小缩放值依赖属性
        /// </summary>
        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.Register(nameof(MinScale), typeof(double), typeof(FisheyePanel),
                new PropertyMetadata(0.8));

        /// <summary>
        /// 获取或设置远离鼠标的项目的最小缩放值
        /// </summary>
        public double MinScale
        {
            get => (double)GetValue(MinScaleProperty);
            set => SetValue(MinScaleProperty, Math.Min(1.0, Math.Max(0.1, value)));
        }

        /// <summary>
        /// 影响半径依赖属性
        /// </summary>
        public static readonly DependencyProperty InfluenceRangeProperty =
            DependencyProperty.Register(nameof(InfluenceRange), typeof(int), typeof(FisheyePanel),
                new PropertyMetadata(2));

        /// <summary>
        /// 获取或设置鱼眼效果的影响半径（影响多少个相邻格子）
        /// </summary>
        public int InfluenceRange
        {
            get => (int)GetValue(InfluenceRangeProperty);
            set => SetValue(InfluenceRangeProperty, Math.Max(1, value));
        }

        /// <summary>
        /// 动画持续时间依赖属性
        /// </summary>
        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(nameof(AnimationDuration), typeof(Duration), typeof(FisheyePanel),
                new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(200))));

        /// <summary>
        /// 获取或设置缩放动画的持续时间
        /// </summary>
        public Duration AnimationDuration
        {
            get => (Duration)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        /// <summary>
        /// 水平间距依赖属性
        /// </summary>
        public static readonly DependencyProperty HorizontalSpacingProperty =
            DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(FisheyePanel),
                new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                  FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// 获取或设置网格列之间的水平间距
        /// </summary>
        public double HorizontalSpacing
        {
            get => (double)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 垂直间距依赖属性
        /// </summary>
        public static readonly DependencyProperty VerticalSpacingProperty =
            DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(FisheyePanel),
                new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                  FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// 获取或设置网格行之间的垂直间距
        /// </summary>
        public double VerticalSpacing
        {
            get => (double)GetValue(VerticalSpacingProperty);
            set => SetValue(VerticalSpacingProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 位移系数依赖属性
        /// </summary>
        public static readonly DependencyProperty DisplacementFactorProperty =
            DependencyProperty.Register(nameof(DisplacementFactor), typeof(double), typeof(FisheyePanel),
                new PropertyMetadata(0.5));

        /// <summary>
        /// 获取或设置周围元素位移的强度系数（0-1）
        /// </summary>
        public double DisplacementFactor
        {
            get => (double)GetValue(DisplacementFactorProperty);
            set => SetValue(DisplacementFactorProperty, Math.Min(1.0, Math.Max(0.0, value)));
        }

        #endregion 依赖属性

        #region 私有字段

        private int _actualColumns;
        private int _actualRows;
        private UIElement _hoveredElement;
        private readonly Dictionary<UIElement, TransformGroup> _transforms = new Dictionary<UIElement, TransformGroup>();
        private readonly Dictionary<UIElement, Point> _originalPositions = new Dictionary<UIElement, Point>();
        private readonly Dictionary<UIElement, CellPosition> _cellPositions = new Dictionary<UIElement, CellPosition>();
        private bool _isArrangeValid = false;
        private Size _cellSize;

        // 每个元素的行列位置
        private struct CellPosition
        {
            public int Row;
            public int Column;

            public CellPosition(int row, int column)
            {
                Row = row;
                Column = column;
            }

            // 计算与另一个单元格的曼哈顿距离
            public int ManhattanDistance(CellPosition other)
            {
                return Math.Abs(Row - other.Row) + Math.Abs(Column - other.Column);
            }

            // 计算与另一个单元格的切比雪夫距离（行距和列距的最大值）
            public int ChebyshevDistance(CellPosition other)
            {
                return Math.Max(Math.Abs(Row - other.Row), Math.Abs(Column - other.Column));
            }
        }

        #endregion 私有字段

        #region 构造函数

        /// <summary>
        /// 初始化一个新的FisheyePanel实例。
        /// </summary>
        public FisheyePanel()
        {
            MouseMove += FisheyePanel_MouseMove;
            MouseLeave += FisheyePanel_MouseLeave;
        }

        #endregion 构造函数

        #region 事件处理

        /// <summary>
        /// 鼠标移动事件处理。
        /// </summary>
        private void FisheyePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isArrangeValid)
                return;

            Point mousePosition = e.GetPosition(this);
            UIElement newHoveredElement = null;

            // 找出鼠标下的元素
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                // 获取元素的原始位置
                if (_originalPositions.TryGetValue(child, out Point originalPos))
                {
                    // 计算原始边界
                    Rect bounds = new Rect(originalPos, child.RenderSize);
                    if (bounds.Contains(mousePosition))
                    {
                        newHoveredElement = child;
                        break;
                    }
                }
            }

            // 判断是否需要更新效果
            if (newHoveredElement != _hoveredElement)
            {
                _hoveredElement = newHoveredElement;
                UpdateFisheyeEffect();
            }
        }

        /// <summary>
        /// 鼠标离开事件处理。
        /// </summary>
        private void FisheyePanel_MouseLeave(object sender, MouseEventArgs e)
        {
            _hoveredElement = null;
            ResetToOriginalState();
        }

        #endregion 事件处理

        #region 布局重写

        /// <summary>
        /// 测量面板及其子元素的尺寸。
        /// </summary>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (InternalChildren.Count == 0)
                return new Size(0, 0);

            // 计算实际行列数
            CalculateRowsAndColumns();

            // 计算单元格尺寸
            _cellSize = CalculateCellSize(availableSize);

            // 测量每个子元素
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    child.Measure(_cellSize);
                }
            }

            // 计算总尺寸，包括间距
            double totalWidth = (_actualColumns * _cellSize.Width) +
                               ((_actualColumns - 1) * HorizontalSpacing);
            double totalHeight = (_actualRows * _cellSize.Height) +
                                ((_actualRows - 1) * VerticalSpacing);

            return new Size(totalWidth, totalHeight);
        }

        /// <summary>
        /// 安排子元素的位置。
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            _isArrangeValid = false;

            if (InternalChildren.Count == 0)
                return finalSize;

            // 清空字典以重新记录位置
            _originalPositions.Clear();
            _cellPositions.Clear();

            // 重新计算单元格尺寸
            _cellSize = CalculateCellSize(finalSize);

            // 确保每个元素都有变换
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    EnsureTransform(child);
                }
            }

            // 排列每个子元素
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                UIElement child = InternalChildren[i];
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                // 计算行列位置
                int row = i / _actualColumns;
                int column = i % _actualColumns;

                // 记录单元格位置
                _cellPositions[child] = new CellPosition(row, column);

                // 计算位置（考虑间距）
                double x = column * (_cellSize.Width + HorizontalSpacing);
                double y = row * (_cellSize.Height + VerticalSpacing);

                // 记录原始位置
                _originalPositions[child] = new Point(x, y);

                // 创建布局矩形
                Rect cellRect = new Rect(x, y, _cellSize.Width, _cellSize.Height);

                // 排列元素
                child.Arrange(cellRect);

                // 重置变换
                ResetTransform(child);
            }

            _isArrangeValid = true;

            // 恢复鱼眼效果（如果有）
            if (_hoveredElement != null)
            {
                UpdateFisheyeEffect();
            }

            return finalSize;
        }

        #endregion 布局重写

        #region 私有辅助方法

        /// <summary>
        /// 计算实际的行列数。
        /// </summary>
        private void CalculateRowsAndColumns()
        {
            int visibleCount = 0;
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    visibleCount++;
                }
            }

            if (visibleCount == 0)
            {
                _actualColumns = 0;
                _actualRows = 0;
                return;
            }

            // 如果列数和行数都指定了
            if (Columns > 0 && Rows > 0)
            {
                _actualColumns = Columns;
                _actualRows = Rows;
            }
            // 如果只指定了列数
            else if (Columns > 0)
            {
                _actualColumns = Columns;
                _actualRows = (visibleCount + Columns - 1) / Columns; // 向上取整
            }
            // 如果只指定了行数
            else if (Rows > 0)
            {
                _actualRows = Rows;
                _actualColumns = (visibleCount + Rows - 1) / Rows; // 向上取整
            }
            // 如果都没指定，计算近似正方形布局
            else
            {
                _actualColumns = (int)Math.Ceiling(Math.Sqrt(visibleCount));
                _actualRows = (visibleCount + _actualColumns - 1) / _actualColumns; // 向上取整
            }
        }

        /// <summary>
        /// 计算单元格尺寸。
        /// </summary>
        private Size CalculateCellSize(Size availableSize)
        {
            if (_actualColumns <= 0 || _actualRows <= 0)
                return new Size(0, 0);

            // 计算去除间距后的可用空间
            double availableWidth = Math.Max(0, availableSize.Width - ((_actualColumns - 1) * HorizontalSpacing));
            double availableHeight = Math.Max(0, availableSize.Height - ((_actualRows - 1) * VerticalSpacing));

            // 计算单元格尺寸
            double cellWidth = Math.Max(1, availableWidth / _actualColumns);
            double cellHeight = Math.Max(1, availableHeight / _actualRows);

            return new Size(cellWidth, cellHeight);
        }

        /// <summary>
        /// 确保元素有变换组。
        /// </summary>
        private void EnsureTransform(UIElement element)
        {
            if (!_transforms.TryGetValue(element, out TransformGroup transformGroup))
            {
                // 创建变换组
                transformGroup = new TransformGroup();

                // 添加缩放变换
                transformGroup.Children.Add(new ScaleTransform(1.0, 1.0));

                // 添加平移变换
                transformGroup.Children.Add(new TranslateTransform(0, 0));

                // 设置元素的渲染变换
                element.RenderTransform = transformGroup;

                // 设置变换中心点为中心
                element.RenderTransformOrigin = new Point(0.5, 0.5);

                // 添加到字典
                _transforms[element] = transformGroup;
            }
        }

        /// <summary>
        /// 重置元素的变换。
        /// </summary>
        private void ResetTransform(UIElement element)
        {
            if (_transforms.TryGetValue(element, out TransformGroup transformGroup))
            {
                // 重置缩放
                if (transformGroup.Children[0] is ScaleTransform scaleTransform)
                {
                    scaleTransform.ScaleX = 1.0;
                    scaleTransform.ScaleY = 1.0;
                }

                // 重置平移
                if (transformGroup.Children[1] is TranslateTransform translateTransform)
                {
                    translateTransform.X = 0;
                    translateTransform.Y = 0;
                }
            }
        }

        /// <summary>
        /// 重置所有元素到原始状态。
        /// </summary>
        private void ResetToOriginalState()
        {
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                AnimateToOriginalState(child);
            }
        }

        /// <summary>
        /// 动画恢复元素到原始状态。
        /// </summary>
        private void AnimateToOriginalState(UIElement element)
        {
            if (_transforms.TryGetValue(element, out TransformGroup transformGroup))
            {
                // 创建缩放动画
                DoubleAnimation scaleXAnimation = new DoubleAnimation(1.0, AnimationDuration);
                DoubleAnimation scaleYAnimation = new DoubleAnimation(1.0, AnimationDuration);

                // 创建平移动画
                DoubleAnimation translateXAnimation = new DoubleAnimation(0, AnimationDuration);
                DoubleAnimation translateYAnimation = new DoubleAnimation(0, AnimationDuration);

                // 设置缓动函数
                QuadraticEase ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                scaleXAnimation.EasingFunction = ease;
                scaleYAnimation.EasingFunction = ease;
                translateXAnimation.EasingFunction = ease;
                translateYAnimation.EasingFunction = ease;

                // 应用动画
                if (transformGroup.Children[0] is ScaleTransform scaleTransform)
                {
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
                }

                if (transformGroup.Children[1] is TranslateTransform translateTransform)
                {
                    translateTransform.BeginAnimation(TranslateTransform.XProperty, translateXAnimation);
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, translateYAnimation);
                }
            }
        }

        /// <summary>
        /// 更新鱼眼效果。
        /// </summary>
        private void UpdateFisheyeEffect()
        {
            if (!_isArrangeValid || _hoveredElement == null)
            {
                ResetToOriginalState();
                return;
            }

            // 确保悬停元素有单元格位置
            if (!_cellPositions.TryGetValue(_hoveredElement, out CellPosition hoveredPosition))
            {
                ResetToOriginalState();
                return;
            }

            // 遍历所有元素应用效果
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                // 获取元素的单元格位置
                if (!_cellPositions.TryGetValue(child, out CellPosition cellPosition))
                    continue;

                // 计算与悬停元素的距离
                int distance = cellPosition.ChebyshevDistance(hoveredPosition);

                // 计算缩放值
                double scale;
                if (distance == 0)
                {
                    // 悬停元素使用最大缩放
                    scale = MaxScale;
                }
                else if (distance <= InfluenceRange)
                {
                    // 在影响半径内的元素按距离线性插值
                    double t = (double)distance / InfluenceRange;
                    scale = MaxScale - (MaxScale - MinScale) * t;
                }
                else
                {
                    // 超出影响半径的使用最小缩放
                    scale = MinScale;
                }

                // 计算位移向量（从悬停元素指向当前元素的方向）
                double dx = cellPosition.Column - hoveredPosition.Column;
                double dy = cellPosition.Row - hoveredPosition.Row;

                // 计算位移方向的单位向量
                double length = Math.Sqrt(dx * dx + dy * dy);
                if (length > 0)
                {
                    dx /= length;
                    dy /= length;
                }

                // 位移量与距离和悬停元素的放大比例相关
                double displacementMagnitude = 0;

                if (distance > 0 && distance <= InfluenceRange)
                {
                    // 计算位移量
                    displacementMagnitude = (MaxScale - 1.0) * _cellSize.Width * DisplacementFactor;

                    // 距离越远，位移越小
                    displacementMagnitude *= (1.0 - (double)distance / (InfluenceRange + 1));
                }

                // 计算最终位移
                double translateX = dx * displacementMagnitude;
                double translateY = dy * displacementMagnitude;

                // 应用变换动画
                ApplyTransformAnimation(child, scale, translateX, translateY);
            }
        }

        /// <summary>
        /// 应用变换动画。
        /// </summary>
        private void ApplyTransformAnimation(UIElement element, double scale, double translateX, double translateY)
        {
            if (_transforms.TryGetValue(element, out TransformGroup transformGroup))
            {
                // 创建缩放动画
                DoubleAnimation scaleXAnimation = new DoubleAnimation(scale, AnimationDuration);
                DoubleAnimation scaleYAnimation = new DoubleAnimation(scale, AnimationDuration);

                // 创建平移动画
                DoubleAnimation translateXAnimation = new DoubleAnimation(translateX, AnimationDuration);
                DoubleAnimation translateYAnimation = new DoubleAnimation(translateY, AnimationDuration);

                // 设置缓动函数
                QuadraticEase ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                scaleXAnimation.EasingFunction = ease;
                scaleYAnimation.EasingFunction = ease;
                translateXAnimation.EasingFunction = ease;
                translateYAnimation.EasingFunction = ease;

                // 应用动画
                if (transformGroup.Children[0] is ScaleTransform scaleTransform)
                {
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
                }

                if (transformGroup.Children[1] is TranslateTransform translateTransform)
                {
                    translateTransform.BeginAnimation(TranslateTransform.XProperty, translateXAnimation);
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, translateYAnimation);
                }
            }
        }

        #endregion 私有辅助方法
    }
}
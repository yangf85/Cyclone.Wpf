using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_ScrollViewer, Type = typeof(ScrollViewer))]
[TemplatePart(Name = PART_ContentPanel, Type = typeof(StackPanel))]
public class FluidTabControl : Selector
{
    const string PART_ScrollViewer = "PART_ScrollViewer";

    const string PART_ContentPanel = "PART_ContentPanel";

    private ScrollViewer _scrollViewer;

    private StackPanel _contentPanel;

    static FluidTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FluidTabControl), new FrameworkPropertyMetadata(typeof(FluidTabControl)));
    }

    #region Override

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        // 根据选中的标签滚动到对应的内容
        if (_scrollViewer != null && _contentPanel != null && SelectedIndex >= 0)
        {
            var content = _contentPanel.Children[SelectedIndex];
            _scrollViewer.ScrollToVerticalOffset(content.TranslatePoint(new Point(0, 0), _contentPanel).Y);
        }
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is FluidTabItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new FluidTabItem();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 获取模板中的 ScrollViewer 和 StackPanel
        _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        _contentPanel = GetTemplateChild("PART_ContentPanel") as StackPanel;

        // 订阅滚动事件
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += OnScrollChanged;
        }
    }

    #endregion Override

    #region Private

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // 根据滚动位置更新选中的标签
        if (_contentPanel != null && _scrollViewer != null)
        {
            for (int i = 0; i < _contentPanel.Children.Count; i++)
            {
                var content = _contentPanel.Children[i];
                var offset = content.TranslatePoint(new Point(0, 0), _contentPanel).Y;

                if (offset <= _scrollViewer.VerticalOffset && offset + content.RenderSize.Height > _scrollViewer.VerticalOffset)
                {
                    SelectedIndex = i;
                    break;
                }
            }
        }
    }

    #endregion Private
}
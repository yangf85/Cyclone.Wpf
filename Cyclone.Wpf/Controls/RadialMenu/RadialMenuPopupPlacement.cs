using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 自定义弹出窗口放置逻辑，用于子菜单弹出位置计算
    /// </summary>
    public class RadialMenuPopupPlacement : IDisposable
    {
        private readonly Popup _popup;
        private readonly RadialMenuItem _menuItem;
        private CustomPopupPlacement[] _customPlacement;

        public RadialMenuPopupPlacement(Popup popup, RadialMenuItem menuItem)
        {
            _popup = popup ?? throw new ArgumentNullException(nameof(popup));
            _menuItem = menuItem ?? throw new ArgumentNullException(nameof(menuItem));

            _popup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(PlacePopup);
            _popup.Placement = PlacementMode.Custom;
        }

        public CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            if (_customPlacement == null)
            {
                _customPlacement = new CustomPopupPlacement[1];
            }

            // 计算弹出位置
            double angle = _menuItem.AnglePosition * Math.PI / 180;
            double offsetX = _menuItem.OuterRadius * Math.Cos(angle);
            double offsetY = _menuItem.OuterRadius * Math.Sin(angle);

            // 弧形面板的中心点相对于菜单项中心点的偏移
            Point popupOffset = new Point(
                targetSize.Width / 2 - popupSize.Width / 2 + offsetX,
                targetSize.Height / 2 - popupSize.Height / 2 + offsetY);

            _customPlacement[0] = new CustomPopupPlacement(popupOffset, PopupPrimaryAxis.Horizontal);

            return _customPlacement;
        }

        public void Dispose()
        {
            _popup.CustomPopupPlacementCallback = null;
        }
    }
}
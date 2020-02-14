﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
#if NET35
using GongSolutions.Wpf.DragDrop.Utilities;
#endif

namespace GongSolutions.Wpf.DragDrop
{
  public class DropTargetInsertionAdorner : DropTargetAdorner
  {
    [Obsolete("This constructor is obsolete and will be deleted in next major release.")]
    public DropTargetInsertionAdorner(UIElement adornedElement)
      : base(adornedElement, (DropInfo)null)
    {
    }

    public DropTargetInsertionAdorner(UIElement adornedElement, DropInfo dropInfo)
      : base(adornedElement, dropInfo)
    {
    }

    /// <summary>
    /// When overridden in a derived class, participates in rendering operations that are directed by the layout system.
    /// The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for
    /// later asynchronous use by layout and drawing.
    /// </summary>
    /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
    protected override void OnRender(DrawingContext drawingContext)
    {
      var dropInfo = this.DropInfo;
      var itemsControl = dropInfo.VisualTarget as ItemsControl;

      if (itemsControl != null)
      {
        // Get the position of the item at the insertion index. If the insertion point is
        // to be after the last item, then get the position of the last item and add an 
        // offset later to draw it at the end of the list.
        ItemsControl itemParent;

        var visualTargetItem = dropInfo.VisualTargetItem;
        if (visualTargetItem != null)
        {
          itemParent = ItemsControl.ItemsControlFromItemContainer(visualTargetItem);
        }
        else
        {
          itemParent = itemsControl;
        }

        // this could be happen with a thread scenario where items are removed very quickly
        if (itemParent == null)
        {
          return;
        }

        var itemsCount = itemParent.Items.Count;
        var index = Math.Min(dropInfo.InsertIndex, itemsCount - 1);

        var lastItemInGroup = false;
        var targetGroup = dropInfo.TargetGroup;
        if (targetGroup != null && targetGroup.IsBottomLevel && dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem))
        {
          var indexOf = targetGroup.Items.IndexOf(dropInfo.TargetItem);
          lastItemInGroup = indexOf == targetGroup.ItemCount - 1;
          if (lastItemInGroup && dropInfo.InsertIndex != itemsCount)
          {
            index--;
          }
        }

        var itemContainer = (UIElement)itemParent.ItemContainerGenerator.ContainerFromIndex(index);

        if (itemContainer != null)
        {
          var itemRect = new Rect(itemContainer.TranslatePoint(new Point(), this.AdornedElement), itemContainer.RenderSize);
          Point point1,
                point2;
          double rotation = 0;

          var viewportWidth = DropInfo.TargetScrollViewer?.ViewportWidth ?? double.MaxValue;
          var viewportHeight = DropInfo.TargetScrollViewer?.ViewportHeight ?? double.MaxValue;

          if (dropInfo.VisualTargetOrientation == Orientation.Vertical)
          {
            if (dropInfo.InsertIndex == itemsCount || lastItemInGroup)
            {
              itemRect.Y += itemContainer.RenderSize.Height;
            }

            var itemRectRight = Math.Min(itemRect.Right, viewportWidth);
            var itemRectLeft = itemRect.X < 0 ? 0 : itemRect.X;
            point1 = new Point(itemRectLeft, itemRect.Y);
            point2 = new Point(itemRectRight, itemRect.Y);
          }
          else
          {
            var itemRectX = itemRect.X;

            if (dropInfo.VisualTargetFlowDirection == FlowDirection.LeftToRight && dropInfo.InsertIndex == itemsCount)
            {
              itemRectX += itemContainer.RenderSize.Width;
            }
            else if (dropInfo.VisualTargetFlowDirection == FlowDirection.RightToLeft && dropInfo.InsertIndex != itemsCount)
            {
              itemRectX += itemContainer.RenderSize.Width;
            }

            point1 = new Point(itemRectX, itemRect.Y);
            point2 = new Point(itemRectX, itemRect.Bottom);
            rotation = 90;
          }

          drawingContext.DrawLine(this.Pen, point1, point2);
          this.DrawTriangle(drawingContext, point1, rotation);
          this.DrawTriangle(drawingContext, point2, 180 + rotation);
        }
      }
    }

    private void DrawTriangle(DrawingContext drawingContext, Point origin, double rotation)
    {
      drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
      drawingContext.PushTransform(new RotateTransform(rotation));

      drawingContext.DrawGeometry(this.Pen.Brush, null, m_Triangle);

      drawingContext.Pop();
      drawingContext.Pop();
    }

    static DropTargetInsertionAdorner()
    {
      // Create the pen and triangle in a static constructor and freeze them to improve performance.
      const int triangleSize = 5;

      var firstLine = new LineSegment(new Point(0, -triangleSize), false);
      firstLine.Freeze();
      var secondLine = new LineSegment(new Point(0, triangleSize), false);
      secondLine.Freeze();

      var figure = new PathFigure { StartPoint = new Point(triangleSize, 0) };
      figure.Segments.Add(firstLine);
      figure.Segments.Add(secondLine);
      figure.Freeze();

      m_Triangle = new PathGeometry();
      m_Triangle.Figures.Add(figure);
      m_Triangle.Freeze();
    }

    private static readonly PathGeometry m_Triangle;
  }
}
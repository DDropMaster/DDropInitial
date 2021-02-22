﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DDrop.Controls.PhotoCropper.Managers
{
    /// <summary>
    ///     Class that response for draw/redraw cropping rectangle
    /// </summary>
    internal class RectangleManager
    {
        private readonly Canvas _canvas;
        private readonly Rectangle _dashedRectangle;

        private readonly Rectangle _rectangle;
        private readonly bool _squareMode;
        private Point _bottomRight;
        private bool _isDragging;
        private bool _isDrawing;
        private Point _mouseLastPoint;

        private Point _mouseStartPoint;

        private Point _topLeft;

        public RectangleManager(Canvas canvasOverlay, bool squareMode)
        {
            _canvas = canvasOverlay;
            _squareMode = squareMode;
            // intit crop rectangle
            _rectangle = new Rectangle
            {
                Height = 100,
                Width = 100,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };
            //intit second rectangle to fake dashed lines
            _dashedRectangle = new Rectangle
            {
                Height = 100,
                Width = 100,
                Stroke = Brushes.White,
                StrokeDashArray = new DoubleCollection(new double[] {4, 4})
            };
            //add both rectangels, so it will be rendered
            _canvas.Children.Add(_rectangle);
            _canvas.Children.Add(_dashedRectangle);
            //set intit position on canvas
            Canvas.SetLeft(_rectangle, 0);
            Canvas.SetTop(_rectangle, 0);

            _rectangle.SizeChanged += (sender, args) =>
            {
                if (RectangleSizeChanged != null)
                    RectangleSizeChanged(sender, args);
            };
        }

        public Point TopLeft
        {
            get
            {
                _topLeft.X = Canvas.GetLeft(_rectangle);
                _topLeft.Y = Canvas.GetTop(_rectangle);
                return _topLeft;
            }
        }

        public Point BottomRight
        {
            get
            {
                _bottomRight.X = Canvas.GetLeft(_rectangle) + _rectangle.Width;
                _bottomRight.Y = Canvas.GetTop(_rectangle) + _rectangle.Height;
                return _bottomRight;
            }
        }

        public double RectangleHeight => _rectangle.Height;
        public double RectangleWidth => _rectangle.Width;

        public event EventHandler RectangleSizeChanged;
        public event EventHandler OnRectangleDoubleClickEvent;

        /// <summary>
        ///     Event handler for mouse left button
        /// </summary>
        /// <param name="e">Mouse event args</param>
        public void MouseLeftButtonDownEventHandler(MouseButtonEventArgs e)
        {
            _canvas.CaptureMouse();
            //get mouse click point relative to canvas overlay
            var mouseClickPoint = e.GetPosition(_canvas);

            //first, we need to know where we click
            var touch = GetTouchPoint(mouseClickPoint);

            //if rectangle is created and we click inside rectangle - start dragging
            if (_rectangle.Height != 0 && _rectangle.Width != 0
                                       && touch != TouchPoint.OutsideRectangle)
            {
                if (e.ClickCount == 2)
                {
                    OnRectangleDoubleClickEvent(this, EventArgs.Empty);
                    return;
                }

                _isDragging = true;
                _mouseLastPoint = mouseClickPoint;
            }
        }

        /// <summary>
        ///     Event handler for mouse move
        /// </summary>
        /// <param name="e">Mouse event args</param>
        public void MouseMoveEventHandler(MouseEventArgs e)
        {
            //get mouse click point relative to canvas overlay
            var mouseClickPoint = e.GetPosition(_canvas);

            if (_isDrawing)
            {
                //see how much the mouse has moved
                var offsetX = mouseClickPoint.X - _mouseLastPoint.X;
                var offsetY = mouseClickPoint.Y - _mouseLastPoint.Y;

                //allow to draw rectangle at any direction;
                var left = Math.Min(mouseClickPoint.X, _mouseStartPoint.X);
                var top = Math.Min(mouseClickPoint.Y, _mouseStartPoint.Y);
                var width = Math.Abs(mouseClickPoint.X - _mouseStartPoint.X);
                var height = Math.Abs(mouseClickPoint.Y - _mouseStartPoint.Y);

                //set drawing limits(canvas borders)
                //set top limit
                if (top < 0)
                {
                    top = 0;
                    height = _rectangle.Height;
                }

                //set right limit
                if (left + offsetX + width > _canvas.ActualWidth) width = _canvas.ActualWidth - left;
                //set left limit
                if (left < 0)
                {
                    left = 0;
                    width = _rectangle.Width;
                }

                //set bottom limit
                if (top + height > _canvas.ActualHeight) height = _canvas.ActualHeight - top;
                UpdateRectangle(left, top, width, height);
                return;
            }

            if (_isDragging)
            {
                //see how much the mouse has moved
                var offsetX = mouseClickPoint.X - _mouseLastPoint.X;
                var offsetY = mouseClickPoint.Y - _mouseLastPoint.Y;

                //get the original rectangle parameters
                var left = TopLeft.X;
                var top = TopLeft.Y;
                var width = _rectangle.Width;
                var height = _rectangle.Height;

                left += offsetX;
                top += offsetY;

                //set dragging limits(canvas borders)
                //set bottom limit
                if (top + offsetY + height > _canvas.ActualHeight) top = _canvas.ActualHeight - height;
                //set right limit
                if (left + offsetX + width > _canvas.ActualWidth) left = _canvas.ActualWidth - width;
                //set left limit
                if (left < 0) left = 0;
                //set top limit
                if (top < 0) top = 0;
                // Update the rectangle.
                UpdateRectangle(left, top, width, height);
                UpdateDashedRectangle();
                _mouseLastPoint = mouseClickPoint;
            }
        }

        /// <summary>
        ///     Event handler for mouse left button up
        /// </summary>
        /// <param name="e">Mouse event args</param>
        public void MouseLeftButtonUpEventHandler()
        {
            _isDrawing = false;
            _isDragging = false;
            _canvas.ReleaseMouseCapture();
        }

        /// <summary>
        ///     Update rectangle size and position
        /// </summary>
        /// <param name="newX">New X rectangle coordinate</param>
        /// <param name="newY">New Y rectangle coordinate</param>
        /// <param name="newWidth">Rectangle new width</param>
        /// <param name="newHeight">Rectangle new height</param>
        public void UpdateRectangle(double newX, double newY, double newWidth, double newHeight)
        {
            //dont use negative value
            if (newHeight >= 0 && newWidth >= 0)
            {
                Canvas.SetLeft(_rectangle, newX);
                Canvas.SetTop(_rectangle, newY);
                _rectangle.Width = newWidth;
                _rectangle.Height = newHeight;
                //we need to update dashed rectangle too
                if (_squareMode)
                {
                    if (_rectangle.Height > _canvas.ActualWidth)
                    {
                        Canvas.SetLeft(_rectangle, 0);
                        _rectangle.Height = _canvas.ActualWidth;
                        _rectangle.Width = _canvas.ActualWidth;
                    }

                    if (_rectangle.Height + newX > _canvas.ActualWidth)
                        Canvas.SetLeft(_rectangle, _canvas.ActualWidth - _rectangle.ActualWidth);

                    if (_rectangle.Height + newY > _canvas.ActualHeight)
                        Canvas.SetTop(_rectangle, _canvas.ActualHeight - _rectangle.ActualHeight);
                }

                UpdateDashedRectangle();
            }
        }

        /// <summary>
        ///     Update dashed rectangle
        /// </summary>
        private void UpdateDashedRectangle()
        {
            _dashedRectangle.Height = _rectangle.Height;
            _dashedRectangle.Width = _rectangle.Width;
            Canvas.SetLeft(_dashedRectangle, TopLeft.X);
            Canvas.SetTop(_dashedRectangle, TopLeft.Y);
        }

        /// <summary>
        ///     Hit test. We need to know where we click
        /// </summary>
        /// <param name="mousePoint">Mouse click coordinate</param>
        /// <returns></returns>
        private TouchPoint GetTouchPoint(Point mousePoint)
        {
            //left
            if (mousePoint.X < TopLeft.X)
                return TouchPoint.OutsideRectangle;
            //right
            if (mousePoint.X > BottomRight.X)
                return TouchPoint.OutsideRectangle;
            //top
            if (mousePoint.Y < TopLeft.Y)
                return TouchPoint.OutsideRectangle;
            //bottom
            if (mousePoint.Y > BottomRight.Y)
                return TouchPoint.OutsideRectangle;

            return TouchPoint.InsideRectangle;
        }

        private enum TouchPoint
        {
            OutsideRectangle,
            InsideRectangle
        }
    }
}
using System;
using Microsoft.Office.Core;
using System.Collections;
using System.Drawing;
using System.Windows;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace PowerPointLabs.Utils
{
    internal class PPShape
    {
        private readonly PowerPoint.Shape _shape;
        private float _absoluteWidth;
        private float _absoluteHeight;
        private float _rotatedLeft;
        private float _rotatedTop;

        public PPShape(PowerPoint.Shape shape)
        {
            _shape = shape;

            ConvertToFreeform();

            UpdateAbsoluteWidth();
            UpdateAbsoluteHeight();

            UpdateTop();
            UpdateLeft();
        }

        /// <summary>
        /// Return specified shape.
        /// </summary>
        public PowerPoint.Shape Shape
        {
            get { return _shape; }
        }

        /// <summary>
        /// Return or set the name of the specified shape.
        /// </summary>
        public string Name
        {
            get { return _shape.Name; }
            set { _shape.Name = value; }
        }

        /// <summary>
        /// Return or set the width of the specified shape.
        /// </summary>
        public float ShapeWidth
        {
            get { return _shape.Width; }
            set
            {
                _shape.Width = value;
                UpdateAbsoluteWidth();
            }
        }

        /// <summary>
        /// Return or set the height of the specified shape.
        /// </summary>
        public float ShapeHeight
        {
            get { return _shape.Height; }
            set
            {
                _shape.Height = value;
                UpdateAbsoluteHeight();
            }
        }

        /// <summary>
        /// Return or set the absolute width of rotated shape.
        /// </summary>
        public float AbsoluteWidth
        {
            get { return _absoluteWidth; }
            set
            {
                _absoluteWidth = value;
                
                if (_shape.LockAspectRatio == MsoTriState.msoTrue)
                {
                    SetToAbsoluteWidthAspectRatio();
                }
                else
                {
                    SetToAbsoluteDimension();
                }
                
            }
        }

        /// <summary>
        /// Return or set the absolute height of rotated shape.
        /// </summary>
        public float AbsoluteHeight
        {
            get { return _absoluteHeight; }
            set
            {
                _absoluteHeight = value;

                if (_shape.LockAspectRatio == MsoTriState.msoTrue)
                {
                    SetToAbsoluteHeightAspectRatio();
                }
                else
                {
                    SetToAbsoluteDimension();
                }
                
            }
        }

        /// <summary>
        /// Returns or sets the shape type for the specified Shape object,
        /// which must represent an AutoShape other than a line, freeform drawing, or connector.
        /// Read/write.
        /// </summary>
        public MsoAutoShapeType AutoShapeType
        {
            get { return _shape.AutoShapeType; }
            set { _shape.AutoShapeType = value; }
        }

        /// <summary>
        /// Returns a point that represents the center of the shape.
        /// </summary>
        public PointF Center
        {
            get
            {
                var centerPoint = new PointF
                {
                    X = _rotatedLeft + _absoluteWidth/2,
                    Y = _rotatedTop + _absoluteHeight/2
                };
                return centerPoint;
            }
        }

        /// <summary>
        /// Returns a point that represents the top left of the shape's bounding box after rotation.
        /// </summary>
        public PointF TopLeft
        {
            get
            {
                var topLeft = new PointF
                {
                    X = _rotatedLeft,
                    Y = _rotatedTop
                };
                return topLeft;
            }
        }

        /// <summary>
        /// Returns a point that represents the top center of the shape's bounding box after rotation.
        /// </summary>
        public PointF TopCenter
        {
            get
            {
                var topCenterPoint = new PointF
                {
                    X = _rotatedLeft + _absoluteWidth / 2,
                    Y = _rotatedTop
                };
                return topCenterPoint;
            }
        }

        /// <summary>
        /// Returns a point that represents the top right of the shape's bounding box after rotation.
        /// </summary>
        public PointF TopRight
        {
            get
            {
                var topRightPoint = new PointF
                {
                    X = _rotatedLeft + _absoluteWidth,
                    Y = _rotatedTop
                };
                return topRightPoint;
            }
        }

        /// <summary>
        /// Returns a point that represents the middle left of the shape's bounding box after rotation.
        /// </summary>
        public PointF MiddleLeft
        {
            get
            {
                var middleLeftPoint = new PointF
                {
                    X = _rotatedLeft,
                    Y = _rotatedTop + _absoluteHeight / 2
                };
                return middleLeftPoint;
            }
        }

        /// <summary>
        /// Returns a point that represents the middle right of the shape's bounding box after rotation.
        /// </summary>
        public PointF MiddleRight
        {
            get
            {
                var middleRightPoint = new PointF
                {
                    X = _rotatedLeft + _absoluteWidth,
                    Y = _rotatedTop + _absoluteHeight / 2
                };
                return middleRightPoint;
            }
        }

        /// <summary>
        /// Returns a point that represents the bottom left of the shape's bounding box after rotation.
        /// </summary>
        public PointF BottomLeft
        {
            get
            {
                var bottomLeftPoint = new PointF
                {
                    X = _rotatedLeft,
                    Y = _rotatedTop + _absoluteHeight
                };
                return bottomLeftPoint;
            }
        }

        /// <summary>
        /// Returns a point that represents the bottom center of the shape's bounding box after rotation.
        /// </summary>
        public PointF BottomCenter
        {
            get
            {
                var bottomCenterPoint = new PointF
                {
                    X = _rotatedLeft + _absoluteWidth / 2,
                    Y = _rotatedTop + _absoluteHeight
                };
                return bottomCenterPoint;
            }
        }

        /// <summary>
        /// Returns a point that represents the bottom right of the shape's bounding box after rotation.
        /// </summary>
        public PointF BottomRight
        {
            get
            {
                var bottomRightPoint = new PointF
                {
                    X = _rotatedLeft + _absoluteWidth,
                    Y = _rotatedTop + _absoluteHeight
                };
                return bottomRightPoint;
            }
        }

        /// <summary>
        /// Returns a 64-bit signed integer that identifies the PPshape. Read-only.
        /// </summary>
        public int Id
        {
            get { return _shape.Id; }
        }

        /// <summary>
        /// Return or set a single-precision floating-point number that represents the 
        /// distance from the left most point of the shape to the left edge of the slide.
        /// </summary>
        public float Left
        {
            get { return _rotatedLeft; }
            set
            {
                _rotatedLeft = value; 
                SetLeft();
            }
        }

        /// <summary>
        /// Return or set a single-precision floating-point number that represents the 
        /// distance from the top most point of the shape to the top edge of the slide.
        /// </summary>
        public float Top
        {
            get { return _rotatedTop; }
            set
            {
                _rotatedTop = value;
                SetTop();
            }
        }

        /// <summary>
        /// Returns or sets the number of degrees the specified shape is rotated around the z-axis. Read/write.
        /// </summary>
        public float Rotation
        {
            get { return _shape.Rotation; }
            set
            {
                _shape.Rotation = value;
                ConvertToFreeform();
                UpdateAbsoluteHeight();
                UpdateAbsoluteWidth();
                UpdateLeft();
                UpdateTop();
            }
        }

        /// <summary>
        /// Returns the position of the specified shape in the z-order. Read-only.
        /// </summary>
        public int ZOrderPosition
        {
            get { return _shape.ZOrderPosition; }
        }

        /// <summary>
        /// Delete the specified Shape object.
        /// </summary>
        public void Delete()
        {
            _shape.Delete();
        }

        /// <summary>
        /// Create a duplicate of the specified Shape object and return a new shape.
        /// </summary>
        /// <returns></returns>
        public PPShape Duplicate()
        {
            var newShape = new PPShape(_shape.Duplicate()[1]) {Name = _shape.Name + "Copy"};
            return newShape;
        }

        /// <summary>
        /// Moves the specified shape horizontally by the specified number of points.
        /// </summary>
        /// <param name="value">Number of points from left of slide</param>
        public void IncrementLeft(float value)
        {
            _shape.IncrementLeft(value);
            UpdateLeft();
        }

        /// <summary>
        /// Moves the specified shape vertically by the specified number of points.
        /// </summary>
        /// <param name="value">Number of points from top of slide</param>
        public void IncrementTop(float value)
        {
            _shape.IncrementTop(value);
            UpdateTop();
        }

        /// <summary>
        /// Flip the specified shape around its horizontal or vertical axis.
        /// </summary>
        /// <param name="msoFlipCmd"></param>
        public void Flip(MsoFlipCmd msoFlipCmd)
        {
            _shape.Flip(msoFlipCmd);
        }

        /// <summary>
        /// Select the specified object.
        /// </summary>
        /// <param name="replace"></param>
        public void Select(MsoTriState replace)
        {
            _shape.Select(replace);
        }

        /// <summary>
        /// Convert Autoshape to freeform
        /// </summary>
        private void ConvertToFreeform()
        {
            if ((int)_shape.Rotation == 0) return;
            if (!(_shape.Type == MsoShapeType.msoAutoShape || _shape.Type == MsoShapeType.msoFreeform) || _shape.Nodes.Count < 1) return;

            // Convert AutoShape to Freeform shape
            if (_shape.Type == MsoShapeType.msoAutoShape)
            {
                _shape.Nodes.Insert(1, MsoSegmentType.msoSegmentLine, MsoEditingType.msoEditingAuto, 0, 0);
                _shape.Nodes.Delete(2);
            }

            // Save the coordinates of nodes
            var pointList = new ArrayList();
            for (int i = 1; i <= _shape.Nodes.Count; i++)
            {
                var node = _shape.Nodes[i];
                var point = node.Points;
                var newPoint = new float[2] { point[1, 1], point[1, 2] };

                pointList.Add(newPoint);
            }

            // Rotate bounding box back to 0 degree, and
            // apply the original coordinates to the nodes
            _shape.Rotation = 0;
            for (int i = 0; i < pointList.Count; i++)
            {
                var point = (float[]) pointList[i];
                var nodeIndex = i + 1;

                _shape.Nodes.SetPosition(nodeIndex, point[0], point[1]);
            }
        }

        /// <summary>
        /// Update the absolute width according to the actual shape width and height.
        /// </summary>
        private void UpdateAbsoluteWidth()
        {
            var rotation = GetStandardizedRotation(_shape.Rotation);

            if (IsInQuadrant(_shape.Rotation))
            {
                _absoluteWidth = (float) (_shape.Height*Math.Sin(rotation) + _shape.Width*Math.Cos(rotation));
            }
            else if ((int) _shape.Rotation == 90 || (int) _shape.Rotation == 270)
            {
                _absoluteWidth = _shape.Height;
            }
            else
            {
                _absoluteWidth = _shape.Width;
            }
        }

        /// <summary>
        /// Update the absolute height according to the actual shape width and height.
        /// </summary>
        private void UpdateAbsoluteHeight()
        {
            var rotation = GetStandardizedRotation(_shape.Rotation);

            if (IsInQuadrant(_shape.Rotation))
            {
                _absoluteHeight = (float) (_shape.Height*Math.Cos(rotation) + _shape.Width*Math.Sin(rotation));
            }
            else if ((int) _shape.Rotation == 90 || (int) _shape.Rotation == 270)
            {
                _absoluteHeight = _shape.Width;
            }
            else
            {
                _absoluteHeight = _shape.Height;
            }
        }

        /// <summary>
        /// Update the distance from top most point of the shape to top edge of the slide.
        /// </summary>
        private void UpdateTop()
        {
            _rotatedTop = _shape.Top + _shape.Height/2 - _absoluteHeight/2;
        }

        /// <summary>
        /// Update the distance from left most point of the shape to left edge of the slide.
        /// </summary>
        private void UpdateLeft()
        {
            _rotatedLeft = _shape.Left + _shape.Width/2 - _absoluteWidth/2;
        }

        /// <summary>
        /// Set the actual width and height according to the absolute dimension (e.g. width and height).
        /// </summary>
        private void SetToAbsoluteDimension()
        {
            var rotation = GetStandardizedRotation(_shape.Rotation);
            var sinAngle = Math.Sin(rotation);
            var cosAngle = Math.Cos(rotation);
            var ratio = sinAngle/cosAngle;

            _shape.Height = (float)((_absoluteWidth * ratio - _absoluteHeight) / (sinAngle * ratio - cosAngle));
            _shape.Width = (float)((_absoluteWidth - _shape.Height * sinAngle) / cosAngle);
        }

        private void SetToAbsoluteHeightAspectRatio()
        {
            // Store the original position of the shape
            var originalTop = _shape.Top;
            var originalLeft = _shape.Left;

            _shape.LockAspectRatio = MsoTriState.msoFalse;
            FitToSlide.FitToHeight(_shape, _absoluteWidth, _absoluteHeight);
            _shape.LockAspectRatio = MsoTriState.msoTrue;

            _shape.Top = originalTop;
            _shape.Left = originalLeft;
        }

        private void SetToAbsoluteWidthAspectRatio()
        {
            // Store the original position of the shape
            var originalTop = _shape.Top;
            var originalLeft = _shape.Left;

            _shape.LockAspectRatio = MsoTriState.msoFalse;
            FitToSlide.FitToWidth(_shape, _absoluteWidth, _absoluteHeight);
            _shape.LockAspectRatio = MsoTriState.msoTrue;

            _shape.Top = originalTop;
            _shape.Left = originalLeft;
        }

        /// <summary>
        /// Set the distance from the top edge of unrotated shape to the top edge of the slide.
        /// </summary>
        private void SetTop()
        {
            _shape.Top = _rotatedTop - _shape.Height/2 + _absoluteHeight/2;
        }

        /// <summary>
        /// Set the distance from the left edge of unrotated shape to the left edge of the slide.
        /// </summary>
        private void SetLeft()
        {
            _shape.Left = _rotatedLeft - _shape.Width/2 + _absoluteWidth/2;
        }

        /// <summary>
        /// Check if the angle is in the quadrant.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private static bool IsInQuadrant(float rotation)
        {
            return (rotation > 0 && rotation < 90) || (rotation > 90 && rotation < 180) ||
                   (rotation > 180 && rotation < 270) || (rotation > 270 && rotation < 360);
        }

        /// <summary>
        /// Standardize the angle to the first quadrant.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private static float GetStandardizedRotation(float rotation)
        {
            if ((rotation > 0 && rotation < 90) ||
                (rotation > 180 && rotation < 270))
            {
                rotation = rotation%90;
            }
            else if ((rotation > 90 && rotation <= 180) ||
                     (rotation > 270 && rotation <= 360))
            {
                rotation = (360 - rotation)%90;
            }
            else if ((int)rotation == 270)
            {
                rotation = 360 - rotation;
            }

            return ConvertDegToRad(rotation);
        }

        /// <summary>
        /// Convert angle from degree to radian.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private static float ConvertDegToRad(float rotation)
        {
            return (float) (rotation*Math.PI/180);
        }
    }
}
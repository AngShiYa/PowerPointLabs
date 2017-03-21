﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Microsoft.Office.Interop.PowerPoint;
using PowerPointLabs.Utils;

namespace PowerPointLabs.SyncLab.View
{
    /// <summary>
    /// Interaction logic for SyncPaneWPF.xaml
    /// </summary>
    public partial class SyncPaneWPF : UserControl
    {
#pragma warning disable 0618

        public SyncPaneWPF()
        {
            InitializeComponent();
            ClearStorageTemplate();
            copyImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    Properties.Resources.LineColor_icon.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            pasteImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    Properties.Resources.FillColor_icon.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
        }

        #region GUI Handles
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
            if (selection.Type != PpSelectionType.ppSelectionShapes ||
                selection.ShapeRange.Count != 1)
            {
                MessageBox.Show(TextCollection.SyncLabCopySelectError);
                return;
            }
            var shape = selection.ShapeRange[1];
            SyncFormatDialog dialog = new SyncFormatDialog(shape);
            bool? result = dialog.ShowDialog();
            if (!result.HasValue || !(bool)result)
            {
                return;
            }
            SyncFormatPaneItem item = new SyncFormatPaneItem(formatListBox, CopyShape(shape), dialog.Formats);
            item.Text = shape.Name;
            item.IsChecked = true;
            item.Image = new System.Drawing.Bitmap(Utils.Graphics.ShapeToImage(shape));
            formatListBox.Items.Insert(0, item);
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = Globals.ThisAddIn.Application.ActiveWindow.Selection;
            if (selection.Type != PpSelectionType.ppSelectionShapes ||
                selection.ShapeRange.Count == 0)
            {
                MessageBox.Show(TextCollection.SyncLabPasteSelectError);
                return;
            }
            foreach (Object obj in formatListBox.Items)
            {
                SyncFormatPaneItem item = (SyncFormatPaneItem)obj;
                if (item.IsChecked.HasValue && item.IsChecked.Value)
                {
                    foreach (Shape shape in Globals.ThisAddIn.Application.ActiveWindow.Selection.ShapeRange)
                    {
                        ApplyFormats(item.Formats, item.FormatShape, shape);
                    }
                    break;
                }
            }
        }
        #endregion

        private void ApplyFormats(FormatTreeNode[] nodes, Shape formatShape, Shape newShape)
        {
            foreach (FormatTreeNode node in nodes)
            {
                ApplyFormats(node, formatShape, newShape);
            }
        }

        private void ApplyFormats(FormatTreeNode node, Shape formatShape, Shape newShape)
        {
            if (node.Format != null)
            {
                if (!node.IsChecked.HasValue || !node.IsChecked.Value)
                {
                    return;
                }
                node.Format.SyncFormat(formatShape, newShape);
            }
            else
            {
                ApplyFormats(node.ChildrenNodes, formatShape, newShape);
            }
        }

        #region Shape Saving

        private Shape CopyShape(Shape shape)
        {
            Design design = Graphics.GetDesign(TextCollection.StorageTemplateName);
            if (design == null)
            {
                design = Graphics.CreateDesign(TextCollection.StorageTemplateName);
            }
            shape.Copy();
            ShapeRange newShapeRange = design.TitleMaster.Shapes.Paste();
            return newShapeRange[1];
        }

        private void ClearStorageTemplate()
        {
            Design design = Graphics.GetDesign(TextCollection.StorageTemplateName);
            if (design != null)
            {
                design.Delete();
            }
        }
        #endregion

    }
}
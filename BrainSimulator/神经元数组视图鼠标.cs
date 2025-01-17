﻿//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BrainSimulator
{
    public partial class 神经元数组视图
    {
        public enum shapeType { None, Canvas, Neuron, Synapse, Selection, Module };
        public static readonly DependencyProperty ShapeType =
                DependencyProperty.Register("ShapeType", typeof(shapeType), typeof(Shape));


        //State
        private enum CurrentOperation { idle, draggingSynapse, draggingNewSelection, movingSelection, movingModule, sizingModule, panning, insertingModule };
        private CurrentOperation currentOperation = CurrentOperation.idle;

        //cursor for inserting Module
        static Label labelCursor = null;
        static Rectangle rectCursor = null;



        //current default synapse params
        //当前默认突触参数
        public float 末尾突触权重
        {
            get
            {
                if (float.TryParse(MainWindow.thisWindow.SynapseWeight.Text, out float theWeight))
                    return theWeight;
                return 1;
            }
            set
            {
                if (MainWindow.thisWindow.SynapseUpdate.IsChecked == true)
                {
                    if (!MainWindow.thisWindow.SynapseWeight.Items.Contains(value))
                    {
                        MainWindow.thisWindow.SynapseWeight.Items.Add(value);
                    }
                    MainWindow.thisWindow.SynapseWeight.SelectedItem = value;
                }
            }
        }
        public 突触.modelType 末尾突触模型
        {
            get
            {
                if (Enum.TryParse(MainWindow.thisWindow.SynapseModel.Text, out 突触.modelType theModel))
                    return theModel;
                return 突触.modelType.Fixed;
            }
            set
            {
                if (MainWindow.thisWindow.SynapseUpdate.IsChecked == true)
                {
                    //TODO figure out why you can't just set the SelectedValue... if you try, it's just null
                    string s = value.ToString();
                    foreach (ComboBoxItem x in MainWindow.thisWindow.SynapseModel.Items)
                    {
                        if (x.Content.ToString() == s)
                        {
                            MainWindow.thisWindow.SynapseModel.SelectedItem = x;
                        }
                    }
                }
            }
        }

        int mouseDownNeuronIndex = -1;
        static Shape synapseShape = null;  //the shape of the synapses being rubber-banded 
                                           //(so it can be added/removed easily from the canvas)

        FrameworkElement prevShape = null;

        //This sets the mouse cursor depending on the mouse location and current operation.
        private shapeType 设置鼠标光标形状(MouseEventArgs e, out FrameworkElement theShape)
        {
            theShape = prevShape;

            //if dragging, don't change the cursor but return the hit
            if (currentOperation != CurrentOperation.idle)
            {
                if (currentOperation == CurrentOperation.draggingSynapse) return shapeType.Synapse;
                if (currentOperation == CurrentOperation.movingModule) return shapeType.Module;
                if (currentOperation == CurrentOperation.draggingNewSelection) return shapeType.Selection;
                if (currentOperation == CurrentOperation.insertingModule) return shapeType.None;
                if (!MainWindow.ctrlPressed)
                {
                    if (currentOperation == CurrentOperation.movingSelection) return shapeType.Selection;
                    if (currentOperation == CurrentOperation.sizingModule) return shapeType.Module;
                }
            }

            //don't change the cursor if dragging, panning
            if (theCanvas.Cursor == Cursors.Hand) return shapeType.None;

            //if busy, make sure the cursor is hourglass
            if (MainWindow.Busy())
            {
                theCanvas.Cursor = Cursors.Wait;
                return shapeType.None;
            }

            //what type of shape is the mouse  over?
            HitTestResult result = VisualTreeHelper.HitTest(theCanvas, e.GetPosition(theCanvas));
            if (result != null && result.VisualHit is FrameworkElement theShape0)
            {
                theShape = theShape0;

                //Get the type of the hit...
                
                shapeType st = (shapeType)theShape.GetValue(ShapeType);

                //Set the cursor shape based on the type of object the mouse is over
                switch (st)
                {
                    case shapeType.None:
                        theCanvas.Cursor = Cursors.Cross;
                        st = shapeType.Canvas;
                        break;
                    case shapeType.Synapse:
                        theCanvas.Cursor = Cursors.Arrow;
                        break;
                    case shapeType.Neuron:
                        theCanvas.Cursor = Cursors.UpArrow;
                        break;
                    case shapeType.Module: //for a module, set directional stretch arrow cursors
                        if (!MainWindow.ctrlPressed)
                        {
                            theCanvas.Cursor = Cursors.ScrollAll;
                            设置滚动光标(e.GetPosition(this), theShape);
                        }
                        else //ctrl pressed
                        {
                            theCanvas.Cursor = Cursors.Cross;
                            st = shapeType.Canvas;
                        }
                        break;
                    case shapeType.Selection: //TODO add directional arrow code
                        theCanvas.Cursor = Cursors.ScrollAll;
                        break;
                }
                prevShape = theShape;
                return st;
            }
            //mouse is not over anything
            theCanvas.Cursor = Cursors.Cross;
            return shapeType.Canvas;
        }

        //set up the cursor if it's near the edge of a module
        private bool 设置滚动光标(Point currentPosition, FrameworkElement theShape)
        {
            int moduleIndex = (int)theShape.GetValue(模块视图.AreaNumberProperty);
            //this addresses a problem if you delete a module and it's still on the screen for a moment
            if (moduleIndex >= MainWindow.此神经元数组.Modules.Count) return false;

            Rectangle theRect = MainWindow.此神经元数组.Modules[moduleIndex].GetRectangle(dp);
            double left = Canvas.GetLeft(theRect);
            double top = Canvas.GetTop(theRect);
            bool widthCanChange = MainWindow.此神经元数组.Modules[moduleIndex].TheModule.MinWidth !=
                MainWindow.此神经元数组.Modules[moduleIndex].TheModule.MaxWidth;
            bool heightCanChange = MainWindow.此神经元数组.Modules[moduleIndex].TheModule.MinHeight !=
                MainWindow.此神经元数组.Modules[moduleIndex].TheModule.MaxHeight;

            if (currentPosition.X >= left && currentPosition.X <= left + theRect.Width &&
                currentPosition.Y >= top && currentPosition.Y <= top + theRect.Height)
            {
                double edgeToler = dp.神经元图示大小 / 2.5;
                if (edgeToler < 8) edgeToler = 8;
                bool nearTop = currentPosition.Y - top < edgeToler;
                bool nearBottom = top + theRect.Height - currentPosition.Y < edgeToler;
                bool nearLeft = currentPosition.X - left < edgeToler;
                bool nearRight = left + theRect.Width - currentPosition.X < edgeToler;

                if (nearTop && nearLeft && widthCanChange && heightCanChange) theCanvas.Cursor = Cursors.ScrollNW;
                else if (nearTop && nearRight && widthCanChange && heightCanChange) theCanvas.Cursor = Cursors.ScrollNE;
                else if (nearBottom && nearLeft && widthCanChange && heightCanChange) theCanvas.Cursor = Cursors.ScrollSW;
                else if (nearBottom && nearRight && widthCanChange && heightCanChange) theCanvas.Cursor = Cursors.ScrollSE;
                else if (nearTop && heightCanChange) theCanvas.Cursor = Cursors.ScrollN;
                else if (nearBottom && heightCanChange) theCanvas.Cursor = Cursors.ScrollS;
                else if (nearLeft && widthCanChange) theCanvas.Cursor = Cursors.ScrollW;
                else if (nearRight && widthCanChange) theCanvas.Cursor = Cursors.ScrollE;
                else theCanvas.Cursor = Cursors.ScrollAll;
            }
            else
                theCanvas.Cursor = Cursors.ScrollAll;
            return true;
        }

        //this is needed because the comboboxes in the toolbar can grab focus then MainWindow won't get keystrokes
        //for some reason, you have to set focus to some visible, focussable control in order to keep the 
        //combobox from grabbing it back
        //TODO THIS DOESN'T WORK if other app is in front
        private void theCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine("theCanvas MouseEnter");
            Keyboard.ClearFocus();
            Keyboard.Focus(MainWindow.thisWindow.SynapseUpdate);
            MainWindow.thisWindow.SynapseUpdate.Focus();
        }

        public void theCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //set the cursor before checking for busy so the wait cursor can be set
            shapeType theShapeType = 设置鼠标光标形状(e, out FrameworkElement theShape);
            if (MainWindow.Busy()) return;

            if (MainWindow.此神经元数组 == null) return;
            MainWindow.此神经元数组.设置撤消点();
            Debug.WriteLine("画布鼠标按下" + MainWindow.此神经元数组.Generation + theShape + theShapeType);
            Point currentPosition = e.GetPosition(theCanvas);
            LimitMousePostion(ref currentPosition);
            mouseDownNeuronIndex = dp.坐标上的神经元(currentPosition);

            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (sender is Image img) //TODO: fix this, it should be a selection hit
                {
                    int i = (int)img.GetValue(模块视图.AreaNumberProperty);
                    int i1 = -i - 1;
                    模块视图 nr = new 模块视图
                    {
                        Label = "new",
                        Width = theSelection.selectedRectangles[i1].宽度,
                        Height = theSelection.selectedRectangles[i1].高度,
                        Color = 跨语言接口.ColorToInt(Colors.Aquamarine),
                        ModuleTypeStr = ""
                    };
                    img.ContextMenu = new ContextMenu();
                    模块视图.CreateContextMenu(i, nr, img, img.ContextMenu);
                    img.ContextMenu.IsOpen = true;
                }
                else if (theShapeType == shapeType.Neuron)
                {
                    OpenNeuronContextMenu(theShape);
                }
                else if (theShapeType == shapeType.Synapse)
                {
                    OpenSynapseContextMenu(theShape);
                }
                else if (theShapeType == shapeType.Selection)
                {
                    OpenSelectionContextMenu(theShape);
                }
                else if (theShapeType == shapeType.Module)
                {
                    OpenModuleContextMenu(theShape);
                }
                return;
            } //end of right-button pressed

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (theShapeType == shapeType.Neuron)
                {
                    神经元 n = null;
                    if (mouseDownNeuronIndex >= 0 && mouseDownNeuronIndex < MainWindow.此神经元数组.arraySize)
                        n = MainWindow.此神经元数组.获取神经元(mouseDownNeuronIndex) as 神经元;

                    int clickCount = e.ClickCount;
                    n = NeuronMouseDown(n, clickCount);
                }

                if (theShapeType == shapeType.Synapse)
                {
                    StartSynapseDragging(theShape);
                }

                //start a new selection rectangle drag
                if (theShapeType == shapeType.Canvas && labelCursor == null)
                {
                    currentPosition = StartNewSelectionDrag();
                }

                //either a module or a selection, we're dragging it
                if (theShapeType == shapeType.Module && theCanvas.Cursor == Cursors.ScrollAll)
                {
                    StartaMovingModule(theShape, mouseDownNeuronIndex);
                }
                if (theShapeType == shapeType.Module && theCanvas.Cursor != Cursors.ScrollAll)
                {
                    StartaSizingModule(theShape, mouseDownNeuronIndex);
                }

                if (theShapeType == shapeType.Selection)
                {
                    StartMovingSelection(theShape);
                }
                //starting pan
                if (theCanvas.Cursor == Cursors.Hand)
                {
                    StartPan(e.GetPosition((UIElement)theCanvas.Parent));
                }
                if (currentOperation == CurrentOperation.insertingModule)
                {
                    InsertModule(mouseDownNeuronIndex);
                    Mouse.Capture(null);
                }
                else
                {
                    Mouse.Capture(theCanvas); //if you don't do this, you might lose the mouseup event
                }
            }//end of left-button down
        }

        public void theCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (MainWindow.此神经元数组 == null) return;
            Point currentPosition = e.GetPosition(theCanvas);
            Point nonlimitedPosition = currentPosition;
            LimitMousePostion(ref currentPosition);
            int currentNeuron = dp.坐标上的神经元(currentPosition);

            shapeType theShapeType = 设置鼠标光标形状(e, out FrameworkElement theShape);
            if (MainWindow.Busy()) return;

            if (mouseRepeatTimer != null)
            {
                if (mouseRepeatTimer.IsEnabled && mouseRepeatTimer.Interval == new TimeSpan(0, 0, 0, 0, 100)) return;
                mouseRepeatTimer.Stop();
            }

            if (MainWindow.数组是否为空()) return;

            //You can't do any dragging with the right mouse button
            if (e.RightButton == MouseButtonState.Pressed) return;


            //update the status bar 
            if (currentPosition == nonlimitedPosition)
            {
                int y1 = currentNeuron % Rows;
                int x1 = currentNeuron / Rows;
                string mouseStatus = "ID: " + currentNeuron + "  行: " + y1 + " 列: " + x1;
                foreach (模块视图 mv in MainWindow.此神经元数组.模块)
                {
                    if (currentNeuron >= mv.FirstNeuron && currentNeuron <= mv.LastNeuron)
                    {
                        mv.GetNeuronLocation(currentNeuron, out int x, out int y);
                        mouseStatus += "  " + mv.Label+"("+x+","+y+")";
                    }
                }
                MainWindow.thisWindow.设置窗口底部状态(1, mouseStatus, 0);
            }
            else
            {
                MainWindow.thisWindow.设置窗口底部状态(1, "神经元阵列外的鼠标", 1);
            }


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                string oString = (theShape == null) ? "null" : theShape.ToString();
                //Debug.WriteLine("MouseMove  currentNeuron: " + currentNeuron
                //    + " Shape type: " + theShapeType.ToString() + " theShape: " + oString);

                //are we dragging a synapse? rubber-band it
                if ((theCanvas.Cursor == Cursors.Arrow || theCanvas.Cursor == Cursors.UpArrow)
                    && currentOperation == CurrentOperation.draggingSynapse)
                {
                    DragSynapse(currentNeuron);
                }

                //handle the creation/updating of a dragging selection rectangle
                else if ((theShapeType == shapeType.None || theShapeType == shapeType.Selection) &&
                    currentOperation == CurrentOperation.draggingNewSelection)
                {
                    DragNewSelection(currentNeuron);
                }

                //handle moving an existing  selection
                else if (theShapeType == shapeType.Selection &&
                    theCanvas.Cursor == Cursors.ScrollAll)
                {
                    MoveSelection(currentNeuron);
                }

                //handle moving of a module
                else if (theShapeType == shapeType.Module && currentOperation == CurrentOperation.movingModule)
                {
                    MoveModule(theShape, currentNeuron);
                }

                //handle sizing of a module
                else if (theShapeType == shapeType.Module && currentOperation == CurrentOperation.sizingModule)
                {
                    ResizeModule(theShape, currentNeuron);
                }

                else if (theCanvas.Cursor == Cursors.Hand)
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        ContinuePan(e.GetPosition((UIElement)theCanvas.Parent));
                        //lastPositionOnGrid = e.GetPosition((UIElement)theCanvas.Parent);
                    }
                    else
                    {
                        lastPositionOnCanvas = new Point(0, 0);
                    }
                }
            }//end of Left-button down
            else //no button pressed
            {
                //handle the special case of no mouse cursor we must be inserting a module inserting a module
                //it's the only thing we drag without a button press
                if (currentOperation == CurrentOperation.insertingModule)
                {
                    DragInsertingModule(currentNeuron);
                }
                else
                {
                    currentOperation = CurrentOperation.idle;
                }
            }
        }

        public void theCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.Busy()) return;

            if (mouseRepeatTimer != null) mouseRepeatTimer.Stop();
            if (MainWindow.数组是否为空()) return;
            Debug.WriteLine("theCanvas_MouseUp" + MainWindow.此神经元数组.Generation + currentOperation);
            if (e.ChangedButton == MouseButton.Right) return;

            Point currentPosition = e.GetPosition(theCanvas);
            LimitMousePostion(ref currentPosition);

            //we were dragging a new selection, create it
            if (currentOperation == CurrentOperation.draggingNewSelection)
            {
                FinishSelection();
                更新();
            }
            else if (currentOperation == CurrentOperation.draggingSynapse &&
                synapseShape != null)
            {
                FinishDraggingSynapse(e);
            }

            //we clicked on a neuron...was it a fire or a synapse drag?
            if (theCanvas.Cursor == Cursors.UpArrow)
            {
                //if no synapseshape exists, then were just clicking in a nueron
                if (synapseShape == null)
                {
                    NeuronMouseUp(e);
                }
                else //we were dragging a new synapse
                {
                    FinishDraggingSynapse(e);
                }
            }

            //finish a pan
            if (theCanvas.Cursor == Cursors.Hand)
            {
                FinishPan();
                e.Handled = true;
            }

            mouseDownNeuronIndex = -1;
            Mouse.Capture(null);
            currentOperation = CurrentOperation.idle;
        }

        //keep any mouse operations within the bounds of the neruon array
        private void LimitMousePostion(ref Point p1)
        {
            if (p1.X < dp.DisplayOffset.X) p1.X = dp.DisplayOffset.X;
            if (p1.Y < dp.DisplayOffset.Y) p1.Y = dp.DisplayOffset.Y;
            float width = dp.神经元图示大小 * MainWindow.此神经元数组.arraySize / dp.神经元行数 - 1;
            float height = dp.神经元图示大小 * dp.神经元行数 - 1;
            if (p1.X > dp.DisplayOffset.X + width) p1.X = dp.DisplayOffset.X + width;
            if (p1.Y > dp.DisplayOffset.Y + height) p1.Y = dp.DisplayOffset.Y + height;

        }
    }
}

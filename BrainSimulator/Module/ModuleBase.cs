﻿//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace BrainSimulator.Modules
{
    abstract public class ModuleBase
    {
        protected NeuronArray theNeuronArray { get => MainWindow.此神经元数组; }
        protected 模块视图 mv = null;

        //this is public so it will be included in the saved xml file.  That way
        //initialized data content can be preserved from run to run and only reinitialized when requested.
        public bool initialized = false;

        protected int minWidth = 2;
        protected int maxWidth = 100;
        protected int minHeight = 2;
        protected int maxHeight = 100;
        public int MinWidth => minWidth;
        public int MinHeight => minHeight;
        public int MaxWidth => maxWidth;
        public int MaxHeight => maxHeight;
        public bool isEnabled = true;


        protected ModuleBaseDlg dlg = null;
        public Point dlgPos;
        public Point dlgSize;
        public bool dlgIsOpen = false;
        protected bool allowMultipleDialogs = false;
        private List<ModuleBaseDlg> dlgList = null;

        public ModuleBase() { }

        abstract public void Fire();

        abstract public void Initialize();

        protected void Init(bool forceInit = false)
        {
            SetModuleView();

            if (initialized && !forceInit) return;
            initialized = true;

            Initialize();

            UpdateDialog();

            if (dlg == null && dlgIsOpen)
            {
                ShowDialog();
                dlgIsOpen = true;
            }
        }

        public void SetModuleView()
        {
            if (mv == null)
            {
                //figure out which area is this one
                foreach (模块视图 na1 in theNeuronArray.模块)
                {
                    if (na1.TheModule == this)
                    {
                        mv = na1;
                        break;
                    }
                }
            }
        }

        public void CloseDlg()
        {
            if (dlgList != null)
            for (int i = dlgList.Count-1 ; i >= 0; i--)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    dlgList[i].Close();
                });
            }
            if (dlg != null)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    dlg.Close();
                });
            }
        }

        //used by mainwindow to determine whether or not activation is needed
        public void Activate()
        {
            if (dlg == null) return;
            dlg.Activate();
        }
        public bool IsActive()
        {
            if (dlg == null) return false;
            return dlg.IsActive;
        }

        public virtual void ShowDialog()
        {
            ApartmentState aps = Thread.CurrentThread.GetApartmentState();
            if (aps != ApartmentState.STA) return;
            Type t = this.GetType();
            Type t1 = Type.GetType(t.ToString() + "Dlg");
            while (t1 == null && t.BaseType.Name != "ModuleBase")
            {
                t = t.BaseType;
                t1 = Type.GetType(t.ToString() + "Dlg");
            }
            if (t1 == null) return;
            //if (dlg != null) dlg.Close();
            if (!allowMultipleDialogs && dlg != null) dlg.Close();
            if (allowMultipleDialogs && dlg != null)
            {
                if (dlgList == null) dlgList = new List<ModuleBaseDlg>();
                dlgList.Add(dlg);
                dlgPos.X += 10;
                dlgPos.Y += 10;
            }
            dlg = (ModuleBaseDlg)Activator.CreateInstance(t1);
            if (dlg == null) return;
            dlg.ParentModule = (ModuleBase)this;
            dlg.Closed += Dlg_Closed;
            dlg.Closing += Dlg_Closing;
            dlg.LocationChanged += Dlg_LocationChanged;
            dlg.SizeChanged += Dlg_SizeChanged;

            //we need to set the dialog owner so it will display properly
            //this hack is here because a file might load and create dialogs prior to the mainwindow opening
            //so the same functionality is called from within FileLoad
            Window mainWindow = Application.Current.MainWindow;
            if (mainWindow.GetType() == typeof(MainWindow))
                dlg.Owner = Application.Current.MainWindow;
            else
                跨语言接口.Noop();

            //restore the size and position
            if (dlgPos != new Point(0, 0))
            {
                dlg.Top = dlgPos.Y;
                dlg.Left = dlgPos.X;
            }
            else
            {
                dlg.Top = 250;
                dlg.Left = 250;
            }
            if (dlgSize != new Point(0, 0))
            {
                dlg.Width = dlgSize.X;
                dlg.Height = dlgSize.Y;
            }
            else
            {
                dlg.Width = 350;
                dlg.Height = 300;
            }

            if (mainWindow.ActualWidth > 800) //try to keep dialogs on the screen
            {
                if (dlg.Width + dlg.Left > mainWindow.ActualWidth)
                    dlg.Left = mainWindow.ActualWidth - dlg.Width;
                if (dlg.Height + dlg.Top > mainWindow.ActualHeight)
                    dlg.Top = mainWindow.ActualHeight - dlg.Height;
            }
            dlg.Show();
            dlgIsOpen = true;
        }

        //this hack is here because a file can load and create dialogs prior to the mainwindow opening
        public void 设置对话框所有者(Window MainWindow)
        {
            if (dlg != null)
                dlg.Owner = MainWindow;
        }

        private void Dlg_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dlgSize = new Point()
            { Y = dlg.Height, X = dlg.Width };
        }

        private void Dlg_LocationChanged(object sender, EventArgs e)
        {
            dlgPos = new Point()
            { Y = dlg.Top, X = dlg.Left };
        }

        private void Dlg_Closed(object sender, EventArgs e)
        {
           if (dlg == null) 
                dlgIsOpen = false;
        }

        private void Dlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dlgList != null && dlgList.Count > 0)
            {
                if (dlgList.Contains((ModuleBaseDlg)sender))
                {
                    dlgList.Remove((ModuleBaseDlg)sender);
                }
                else
                {
                    dlg = dlgList[0];
                    dlgList.RemoveAt(0);
                }
            }
            else
                dlg = null;
        }


        public void UpdateDialog()
        {
            if (dlg != null)
                Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                {
                    dlg?.Draw(true);
                }));
        }

        //this is called to allow for any data massaging needed before saving the file
        public virtual void SetUpBeforeSave()
        { }
        //this is called to allow for any data massaging needed after loading the file
        public virtual void 设置后负荷()
        { }
        public virtual void SizeChanged()
        { }

        public virtual MenuItem CustomContextMenuItems()
        {
            return null;
        }

        public ModuleBase FindModleu(Type t)
        {
            foreach (模块视图 na1 in theNeuronArray.模块)
            {
                if (na1.TheModule != null && na1.TheModule.GetType() == t)
                {
                    return na1.TheModule;
                }
            }
            return null;
        }

        public ModuleBase FindModule(string name)
        {
            foreach (模块视图 na1 in theNeuronArray.模块)
            {
                if (na1.Label == name)
                {
                    return na1.TheModule;
                }
            }
            return null;
        }

        protected 神经元 GetNeuron(string neuronLabel)
        {
            return GetNeuron(null, neuronLabel);
        }

        protected 神经元 GetNeuron(string moduleName, string neuronLabel)
        {
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null)
            {
                神经元 n = naModule.GetNeuronAt(neuronLabel);
                return n;
            }
            else
            {
                神经元 n = MainWindow.此神经元数组.GetNeuron(neuronLabel);
                return n;
            }
        }
        protected bool SetNeuronValue(string neuronLabel, float value)
        {
            return SetNeuronValue(null, neuronLabel, value);
        }

        protected bool SetNeuronValue(string moduleName, string neuronLabel, float value)
        {
            bool retVal = false;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null)
            {
                神经元 n = naModule.GetNeuronAt(neuronLabel);
                if (n != null)
                {
                    n.SetValue(value);
                    retVal = true;
                }
            }
            else
            {
                神经元 n = MainWindow.此神经元数组.GetNeuron(neuronLabel);
                if (n != null)
                {
                    n.SetValue(value);
                    retVal = true;
                }
            }
            return retVal;
        }
        protected float GetNeuronValue(string neuronLabel)
        {
            return GetNeuronValue(null, neuronLabel);
        }

        protected float GetNeuronValue(string moduleName, string neuronLabel)
        {
            float retVal = 0;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null)
            {
                神经元 n = naModule.GetNeuronAt(neuronLabel);
                if (n != null)
                {
                    if (n.模型 == 神经元.模型类型.FloatValue)
                        retVal = n.CurrentCharge;
                    else
                        retVal = n.LastCharge;
                }
            }
            return retVal;
        }

        protected bool SetNeuronValue(string moduleName, int n, float value, string label = null)
        {
            bool retVal = false;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null)
            {
                神经元 n1 = naModule.GetNeuronAt(n);
                if (n1 != null)
                {
                    if (label == null)
                        n1.SetValue(value);
                    else
                        n1.标签名 = label;
                    retVal = true;
                }
            }
            return retVal;
        }
        protected bool SetNeuronValue(string moduleName, int x, int y, float value, string label = null)
        {
            bool retVal = false;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null)
            {
                神经元 n = naModule.GetNeuronAt(x, y);
                if (n != null)
                {
                    if (label == null)
                        n.SetValue(value);
                    else
                        n.标签名 = label;
                    retVal = true;
                }
            }
            return retVal;
        }

        protected float GetNeuronValue(string moduleName, int x, int y)
        {
            float retVal = 0;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null)
            {
                神经元 n = naModule.GetNeuronAt(x, y);
                if (n != null)
                {
                    retVal = n.CurrentCharge;
                }
            }
            return retVal;
        }

        protected int GetNeuronValueInt(string moduleName, int x, int y)
        {
            int retVal = 0;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null)
            {
                神经元 n = naModule.GetNeuronAt(x, y);
                if (n != null)
                {
                    retVal = n.LastChargeInt;
                }
            }
            return retVal;
        }

        protected bool SetNeuronVector(string moduleName, bool isHoriz, int rowCol, float[] values)
        {
            bool retVal = true;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    神经元 n;
                    if (isHoriz) n = naModule.GetNeuronAt(i, rowCol);
                    else n = naModule.GetNeuronAt(rowCol, i);
                    if (n != null)
                    {
                        n.SetValue(values[i]);
                    }
                    else retVal = false;
                }
            }
            else retVal = false;
            return retVal;
        }

        protected float[] GetNeuronVector(string moduleName, bool isHoriz, int rowCol)
        {
            float[] retVal;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null)
            {
                if (isHoriz)
                {
                    retVal = new float[naModule.Width];
                    for (int i = 0; i < retVal.Length; i++)
                        retVal[i] = naModule.GetNeuronAt(i, rowCol).CurrentCharge;
                }
                else
                {
                    retVal = new float[naModule.Height];
                    for (int i = 0; i < retVal.Length; i++)
                        retVal[i] = naModule.GetNeuronAt(rowCol, i).CurrentCharge;
                }
            }
            else
            {
                retVal = new float[0];
            }
            return retVal;
        }

        protected bool SetNeuronVector(string moduleName, bool isHoriz, int rowCol, int[] values)
        {
            bool retVal = true;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    神经元 n;
                    if (isHoriz) n = naModule.GetNeuronAt(i, rowCol);
                    else n = naModule.GetNeuronAt(rowCol, i);
                    if (n != null)
                    {
                        n.SetValueInt(values[i]);
                    }
                    else retVal = false;
                }
            }
            else retVal = false;
            return retVal;
        }

        protected int GetModuleWidth(string moduleName)
        {
            int retVal = 0;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null) retVal = naModule.Width;
            return retVal;
        }

        protected int GetModuleHeight(string moduleName)
        {
            int retVal = 0;
            模块视图 naModule;
            if (moduleName != null)
                naModule = theNeuronArray.FindModuleByLabel(moduleName);
            else
                naModule = mv;
            if (naModule != null) retVal = naModule.Height;
            return retVal;
        }

        protected void ClearNeurons(bool deleteIncoming = true)
        {
            if (mv == null)
                return;
            foreach (神经元 n in mv.Neurons)
            {
                n.删除所有突触(true,deleteIncoming);
                n.标签名 = "";
                n.模型 = 神经元.模型类型.IF;
                n.SetValue(0);
                n.LastCharge = 0;
            }
        }
        protected 神经元 AddLabel(string newLabel)
        {
            foreach (神经元 n in mv.Neurons)
            {
                if (n == null) return null;
                if (n.标签名 == newLabel) return n;
                if (n.标签名 == "")
                {
                    n.标签名 = newLabel;
                    return n;
                }
            }
            return null;
        }
        protected void AddLabels(string[] labels)
        {
            foreach (string label in labels)
                AddLabel(label);
        }


    }
}

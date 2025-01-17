﻿//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace BrainSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private async void 加载模型文件(string fileName)
        {
            清除所有模型对话框();
            关闭历史窗口();
            关闭节点窗口();
            此神经元数组视图.theSelection.selectedRectangles.Clear();
            清除所有模型对话框();
            暂停引擎();

            bool 成功 = false;
            await Task.Run(delegate { 成功 = xml文件.加载神经元数组(ref 此神经元数组, fileName); });
            if (!成功)
            {
                创建空网络();
                Properties.Settings.Default["CurrentFile"] = currentFileName;
                Properties.Settings.Default.Save();
                恢复引擎();
                return;
            }
            currentFileName = fileName;

            ReloadNetwork.IsEnabled = true;
            Reload_network.IsEnabled = true;
            if (xml文件.能否写入(currentFileName))
                SaveButton.IsEnabled = true;
            else
                SaveButton.IsEnabled = false;

            设置标题栏();
            await Task.Delay(1000).ContinueWith(t => ShowDialogs());
            foreach (模块视图 na in 此神经元数组.模块)
            {
                if (na.TheModule != null)
                    na.TheModule.设置后负荷();
            }
            此神经元数组.加载完成 = true;

            if (此神经元数组.displayParams != null)
                此神经元数组视图.Dp = 此神经元数组.displayParams;

            AddFileToMRUList(currentFileName);
            Properties.Settings.Default["CurrentFile"] = currentFileName;
            Properties.Settings.Default.Save();

            Update();
            SetShowSynapsesCheckBox(此神经元数组.ShowSynapses);
            SetPlayPauseButtonImage(此神经元数组.EngineIsPaused);
            SetSliderPosition(此神经元数组.EngineSpeed);

            引擎是否暂停 = 此神经元数组.EngineIsPaused;

            engineSpeedStack.Clear();
            engineSpeedStack.Push(此神经元数组.EngineSpeed);

            if (!引擎是否暂停)
                恢复引擎();
        }

        private bool LoadClipBoardFromFile(string fileName)
        {

            xml文件.加载神经元数组(ref myClipBoard, fileName);

            foreach (模块视图 na in myClipBoard.模块)
            {
                if (na.TheModule != null)
                    na.TheModule.设置后负荷();
                {
                    try
                    {
                        na.TheModule.设置后负荷();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("模块加载失败后的安装 " + na.Label + ".   消息: " + e.Message);
                    }
                }
            }
            return true;
        }

        private bool SaveFile(string fileName)
        {
            暂停引擎();
            //If the path contains "bin\64\debug" change the path to the actual development location instead
            //because file in bin..debug can be clobbered on every rebuild.
            if (fileName.ToLower().Contains("bin\\debug\\net6.0-windows"))
            {
                MessageBoxResult mbResult = System.Windows.MessageBox.Show(this, "改为保存到源文件夹?", "Save", MessageBoxButton.YesNoCancel,
                MessageBoxImage.Asterisk, MessageBoxResult.No);
                if (mbResult == MessageBoxResult.Yes)
                    fileName = fileName.ToLower().Replace("bin\\debug\\net6.0-windows\\", "");
                if (mbResult == MessageBoxResult.Cancel)
                    return false;
            }

            foreach (模块视图 na in 此神经元数组.模块)
            {
                if (na.TheModule != null)
                {
                    try
                    {
                        na.TheModule.SetUpBeforeSave();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("SetupBeforeSave failed on module " + na.Label + ".   Message: " + e.Message);
                    }
                }
            }

            此神经元数组.displayParams = 此神经元数组视图.Dp;
            if (xml文件.保存(此神经元数组, fileName))
            {
                currentFileName = fileName;
                SetCurrentFileNameToProperties();
                恢复引擎();
                undoCountAtLastSave = 此神经元数组.获取撤销数量();
                return true;
            }
            else
            {
                恢复引擎();
                return false;
            }
        }
        private void SaveClipboardToFile(string fileName)
        {
            foreach (模块视图 na in myClipBoard.模块)
            {
                if (na.TheModule != null)
                    na.TheModule.SetUpBeforeSave();
            }

            if (xml文件.保存(myClipBoard, fileName))
                currentFileName = fileName;
        }

        private void AddFileToMRUList(string filePath)
        {
            StringCollection MRUList = (StringCollection)Properties.Settings.Default["MRUList"];
            if (MRUList == null)
                MRUList = new StringCollection();
            MRUList.Remove(filePath); //remove it if it's already there
            MRUList.Insert(0, filePath); //add it to the top of the list
            Properties.Settings.Default["MRUList"] = MRUList;
            Properties.Settings.Default.Save();
        }

        private void LoadCurrentFile()
        {
            加载模型文件(currentFileName);
        }

        private static void SetCurrentFileNameToProperties()
        {
            Properties.Settings.Default["CurrentFile"] = currentFileName;
            Properties.Settings.Default.Save();
        }

        int undoCountAtLastSave = 0;
        private bool PromptToSaveChanges()
        {
            if (数组是否为空()) return false;
            MainWindow.此神经元数组.GetCounts(out long synapseCount, out int neuronInUseCount);
            if (neuronInUseCount == 0) return false;
            if (此神经元数组.获取撤销数量() == undoCountAtLastSave) return false; //no changes have been made

            bool canWrite = xml文件.能否写入(currentFileName, out string message);

            暂停引擎();

            bool retVal = false;
            MessageBoxResult mbResult = System.Windows.MessageBox.Show(this, "是否要保存更改?", "Save", MessageBoxButton.YesNoCancel,
            MessageBoxImage.Asterisk, MessageBoxResult.No);
            if (mbResult == MessageBoxResult.Yes)
            {
                if (currentFileName != "" && canWrite)
                {
                    if (SaveFile(currentFileName))
                        undoCountAtLastSave = 此神经元数组.获取撤销数量();
                }
                else
                {
                    if (SaveAs())
                    {
                    }
                    else
                    {
                        retVal = true;
                    }
                }
            }
            if (mbResult == MessageBoxResult.Cancel)
            {
                retVal = true;
            }
            恢复引擎();
            return retVal;
        }
        private bool SaveAs()
        {
            string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            defaultPath += "\\BrainSim";
            try
            {
                if (Directory.Exists(defaultPath)) defaultPath = "";
                else Directory.CreateDirectory(defaultPath);
            }
            catch
            {
                //maybe myDocuments is readonly of offline? let the user do whatever they want
                defaultPath = "";
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "XML Network Files|*.xml",
                Title = "Select a Brain Simulator File",
                InitialDirectory = defaultPath
            };

            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            Nullable<bool> result = saveFileDialog1.ShowDialog();
            if (result ?? false)// System.Windows.Forms.DialogResult.OK)
            {
                if (SaveFile(saveFileDialog1.FileName))
                {
                    AddFileToMRUList(currentFileName);
                    设置标题栏();
                    return true;
                }
            }
            return false;
        }

    }
}

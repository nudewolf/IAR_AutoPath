using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace IAR_AutoPath
{
    class EwpInfo
    {
        public string version;
        public List<string> configList;
        public EwpInfo()
        {
            version = "";
            configList = new List<string>();
        }

    }
    class IAR_Project
    {
        //配置
        public static bool isShowLog = true;
        string incFileExtension = ".h";
        string treeFileExtension = ".c,.s";

        public bool isAutoInclude = true;
        public bool isAutoPath = true;
        //局部
        bool isGetEwp = false;
        bool isGetSourcePath = false;
        string ewpPath = "";
        List<FileItems> fileItems;
        string sourcePath = "";
        string ewpName = "";

        
        //公共
        public string configName = "";
        public string EwpName
        {
            get
            {
                return ewpName;
            }
            set
            {
                ewpName = value;
                ewpPath = ewpName.Remove(ewpName.LastIndexOf('\\') + 1);
                if (File.Exists(ewpName))
                {
                    isGetEwp = true;
                }
            }
        }
        public string SourcePath
        {
            get
            {
                return sourcePath;
            }
            set
            {
                sourcePath = value;
                isGetSourcePath = true;
            }
        }
        public delegate void LogshowHandler(string status);
        public event LogshowHandler LogshowEvent;

        public IAR_Project()
        {
            
        }
        public IAR_Project(string ewpName, string sourcePath)
        {
            this.ewpName = ewpName;
            this.sourcePath = sourcePath;
        }


        #region 
        public EwpInfo GetEwpInfo()
        {
            if (!isGetEwp)
                return null;
            else
                return GetEwpInfo(EwpName);
        }
        public EwpInfo GetEwpInfo(string ewpName)
        {
            EwpName = ewpName;
            if (!isGetEwp)
                return null;

            EwpInfo ewpInfo = new EwpInfo();

            XmlDocument xml = new XmlDocument();
            xml.Load(ewpName);
            bool isGetPrjList=false;

            XmlNode prjNode = xml.SelectSingleNode("/project");

            XmlNodeList xmlNodeList = prjNode.ChildNodes;
            foreach (XmlNode item in prjNode.ChildNodes)
            {
                if (item.Name == "configuration")
                {
                    ewpInfo.configList.Add(item.SelectSingleNode("name").InnerText);

                    if (!isGetPrjList)
                    {
                        XmlNode settingNode = GetNodeWithName(item, "General");
                        XmlNode dataNode = settingNode.SelectSingleNode("data");
                        XmlNode versionNode = GetNodeWithName(dataNode, "OGLastSavedByProductVersion");
                        XmlNode stateNode = versionNode.SelectSingleNode("state");
                        ewpInfo.version = stateNode?.InnerText;
                        isGetPrjList = true;
                    }
                }
            }
            return ewpInfo;
        }

        void ChangeNodeInnerText(XmlNode node, string nodeName, string nodeNewValue)
        {
            ShowLog(String.Format("Change Node: {0} :: {1} --> {2}", nodeName,node.InnerText,nodeNewValue));
            node.InnerText = nodeNewValue;
        }
        public void ChangeVersion(string version)
        {
            if (String.Compare(version, "8.40.2") == 0)
            {
                Ewp_CreakBackup();
                XmlDocument xml = new XmlDocument();
                xml.Load(ewpName);

                XmlNode prjNode = xml.SelectSingleNode("/project");
                XmlNode  dataNode;

                XmlNodeList xmlNodeList = prjNode.ChildNodes;
                foreach (XmlNode item in prjNode.ChildNodes)
                {
                    if(item.Name== "configuration")
                    {
                        ShowLog("转换 Configuration " + item.SelectSingleNode("name").InnerText + "\n");
                        //setting general
                        dataNode = GetNodeWithName(item, "General").SelectSingleNode("data");

                        //ChangeNodeInnerText(GetNodeWithName(dataNode, "OGProductVersion").SelectSingleNode("state"), "OGProductVersion", "7.80.3.12143");
                        ChangeNodeInnerText(GetNodeWithName(dataNode, "OGLastSavedByProductVersion").SelectSingleNode("state"), "OGLastSavedByProductVersion", "8.40.2.22864");
                        ChangeNodeInnerText(GetNodeWithName(dataNode, "GBECoreSlave").SelectSingleNode("state"), "GBECoreSlave", "27");
                        ChangeNodeInnerText(GetNodeWithName(dataNode, "CoreVariant").SelectSingleNode("state"), "CoreVariant", "27");
                        ChangeNodeInnerText(GetNodeWithName(dataNode, "GFPUCoreSlave2").SelectSingleNode("state"), "GFPUCoreSlave2", "27");


                        //setting ICCARM
                        dataNode = GetNodeWithName(item, "ICCARM").SelectSingleNode("data");

                        ChangeNodeInnerText(dataNode.SelectSingleNode("version"), "ICCARM-version", "35");

                        dataNode.RemoveChild(GetNodeWithName(dataNode, "CCStackProtection"));
                        ShowLog("Remove Node: CCStackProtection\n");
                    }
                }
                xml.Save(ewpName);

                //删除ewt文件
                File.Delete(ewpName.Replace(".ewp",".ewt"));
            }
        }
        void Ewp_CreakBackup()
        {
            if (!Directory.Exists(ewpPath + "\\.backup"))
            {
                Directory.CreateDirectory(ewpPath + "\\.backup");
            }
            string backupFile = ewpName.Insert(ewpName.LastIndexOf('\\'), "\\.backup");

            backupFile = backupFile.Insert(backupFile.LastIndexOf('.'), DateTime.Now.ToString("-yyyyMMdd-HHmmss"));

            File.Copy(ewpName, backupFile, true);
        }
        public void Update()
        {
            if (isGetEwp && isGetSourcePath)
            {
                ShowLog("***开始更新***\n");
                fileItems = GetDirTree.GetallDirectory(new List<FileItems>(), sourcePath);
                Ewp_CreakBackup();
                try
                {
                    if (isAutoInclude == true)
                    {
                        IncPath_Update(configName);
                    }
                    if (isAutoPath == true)
                    {
                        SrcTree_Update();
                    }
                }
                catch (Exception e)
                {
                    ShowLog("\n异常："+e.Message);
                }
                ShowLog("\n***更新完成***");
            }
            else
            {
                if(!isGetEwp)
                {
                    ShowLog("更新失败：未获取到项目EWP文件！");
                }
                if(!isGetSourcePath)
                {
                    ShowLog("更新失败：未获取到项目代码文件夹！");
                }
            }
        }

        /// <summary>
        /// 更新include配置
        /// </summary>
        public void IncPath_Update(string config)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(ewpName);

            
            //获取include path 列表
            List<string> incPathList = new List<string>();
            IncPath_GetFromFileItems(incPathList, fileItems);

            //获取node
            XmlNode prjNode = xml.SelectSingleNode("/project");

            //*******************修改所以配置
            //foreach (XmlNode xmlItem in prjNode.ChildNodes)
            //{
            //    if (xmlItem.Name == "configuration")
            //    {
            //        ShowLog("更新Include in Configuration" + xmlItem.SelectSingleNode("name").InnerText);
            //        XmlNode incNode = GetNodewithName(GetNodewithName(xmlItem, "ICCARM").SelectSingleNode("data"), "CCIncludePath2");
            //        //删除现有
            //        RemoveNodeByName(incNode, "state");

            //        //写入node
            //        foreach (var item in incPathList)
            //        {
            //            XmlElement nameNode = xml.CreateElement("state");
            //            nameNode.InnerText = item;
            //            incNode.AppendChild(nameNode);

            //            ShowLog(item);
            //        }
            //    }
            //}

            //*******************修改制定配置
            ShowLog(String.Format("更新Include in Configuration {0}\n",config));
            XmlNode cfgBode = GetNodeWithName(prjNode, config);
            if(cfgBode==null)
            {
                ShowLog("未找到该节点!\n");//理论一定能找到
                return;
            }
            XmlNode settingNode = GetNodeWithName(cfgBode, "ICCARM");
            XmlNode dataNode = settingNode.SelectSingleNode("data");
            XmlNode incNode = GetNodeWithName(dataNode, "CCIncludePath2");


            //删除现有
            //RemoveNodeByName(incNode, "state");

            //检查增加新增的inc
            foreach (var item in incPathList)
            {
                if(!IsChildWithInnerTextExistInNode(incNode, item))
                {
                    XmlElement nameNode = xml.CreateElement("state");
                    nameNode.InnerText = item;
                    incNode.AppendChild(nameNode);

                    ShowLog(String.Format("Add: \t{0}", item));
                }
            }
            //检查删除移除的inc
            List<XmlNode> delChildlist = new List<XmlNode>();

            foreach (XmlNode item in incNode.ChildNodes)
            {
                if (item.Name == "state")
                {
                    if(!incPathList.Contains(item.InnerText))
                    {
                        delChildlist.Add(item);
                    }
                }
            }

            for (int i = 0; i < delChildlist.Count; i++)
            {
                incNode.RemoveChild(delChildlist[i]);
                ShowLog(String.Format("Delete: \t{0}", delChildlist[i].InnerText));
            }
            xml.Save(ewpName);
        }

        public void SrcTree_Update()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(ewpName);

            ShowLog("\n更新项目文件：");
            XmlNode prjNode = xml.SelectSingleNode("/project");

            //****************直接删除重建
            //删除现有
            //RemoveNodeByName(prjNode, "group");
            //递归更新
            //UpdatePathTree(xml, prjNode, fileItems);


            //****************仅修改变更项
            //递归添加新加入的
            SrcTree_CheckAdd(xml, prjNode, fileItems);
            //递归删除已移除的
            SrcTree_CheckDelete(prjNode, fileItems);

            xml.Save(ewpName);

        }

        #endregion

        void ShowLog(string msg)
        {
            if (isShowLog)
            {
                LogshowEvent(msg);
            }
        }


        /// <summary>
        /// 递归获取include path
        /// </summary>
        /// <param name="incPathList"></param>
        /// <param name="fileItems"></param>
        void IncPath_GetFromFileItems(List<string> incPathList, List<FileItems> fileItems)
        {
            foreach (FileItems item in fileItems)
            {
                if (item.type == FileItems.ItemType.dir)//dir
                {
                    if (!item.name.Contains(".ignore") && GetDirTree.CheckCodeFileIsExistInPath(item.children, incFileExtension))
                    {
                        string incPath = GetIARRelativePath(ewpPath, item.fullName);
                        incPathList.Add(incPath);
                    }
                    IncPath_GetFromFileItems(incPathList, item.children);
                }
            }
        }

        void SrcTree_CheckAdd(XmlDocument xml, XmlNode node, List<FileItems> fileItems)
        {
            foreach (FileItems item in fileItems)
            {
                if (item.type == FileItems.ItemType.dir)//dir
                {
                    
                    if (!item.name.Contains(".ignore") && GetDirTree.CheckCodeFileIsExistInPath(item.children, treeFileExtension))//如果该文件夹下没有代码文件 或 需要忽略，则不添加
                    {
                        XmlNode xmlNode = GetNodeWithName(node, item.name);
                        if (xmlNode != null)//如果该group已存在，则不添加，直接检查下级目录
                        {
                            SrcTree_CheckAdd(xml, xmlNode, item.children);
                        }
                        else
                        {
                            //生成一个新节点
                            XmlElement groupNode = xml.CreateElement("group");
                            //chileNode.SetAttribute("name", item.name);
                            //将节点加到指定节点下，作为其子节点
                            node.AppendChild(groupNode);

                            XmlElement nameNode = xml.CreateElement("name");
                            nameNode.InnerText = item.name;
                            groupNode.AppendChild(nameNode);

                            ShowLog("Add group：" + nameNode.InnerText);

                            SrcTree_CheckAdd(xml, groupNode, item.children);
                        }
                    }
                }
                else if (item.type == FileItems.ItemType.file)
                {
                    if (CheckFileExtension(item.extension, treeFileExtension))
                    {
                        string relName = GetIARRelativePath(ewpPath, item.fullName);
                        if (!IsFileExistInGroupNode(node, relName))
                        {

                            XmlElement fileNode = xml.CreateElement("file");
                            node.AppendChild(fileNode);

                            XmlElement nameNode = xml.CreateElement("name");
                            nameNode.InnerText = relName;
                            fileNode.AppendChild(nameNode);

                            ShowLog("Add file：" + nameNode.InnerText);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从FileItems列表中找到指定名字的FileItems
        /// </summary>
        /// <param name="fileItems"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        FileItems GetDirItemsFromFileItemsList(List<FileItems> fileItems, string name)
        {
            foreach (FileItems item in fileItems)
            {
                string relativeName;
                if (name.Contains("."))//如果是文件
                {
                    relativeName = GetIARRelativePath(ewpPath, item.fullName);
                }
                else//如果是文件夹
                {
                    relativeName = item.name;
                }
                if (relativeName == name)
                {
                    return item;
                }
            }
            return null;
        }
        void SrcTree_CheckDelete(XmlNode node, List<FileItems> fileItems)
        {
            List<XmlNode> waitDelList = new List<XmlNode>();
            foreach (XmlNode xmlItem in node.ChildNodes)
            {
                if (xmlItem.Name == "group")
                {
                    XmlNode nameNode = xmlItem.SelectSingleNode("name");
                    if (nameNode != null)
                    {
                        string fileItemName = nameNode.InnerText;
                        FileItems fileItem = GetDirItemsFromFileItemsList(fileItems, fileItemName);
                        if (fileItem == null|| !GetDirTree.CheckCodeFileIsExistInPath(fileItem.children, treeFileExtension))//如果该文件夹不存在 或 文件夹内没有项目树文件，直接删除
                        {
                            waitDelList.Add(xmlItem);
                            ShowLog("Delete group：" + nameNode.InnerText);
                        }
                        else//递归检查
                        {
                            SrcTree_CheckDelete(xmlItem, fileItem.children);
                        }
                    }
                }
                else if (xmlItem.Name == "file")
                {
                    XmlNode nameNode = xmlItem.SelectSingleNode("name");
                    if (nameNode != null)
                    {
                        string fileItemName = nameNode.InnerText;
                        FileItems fileItem = GetDirItemsFromFileItemsList(fileItems, fileItemName);
                        if (fileItem == null|| !CheckFileExtension(fileItem.extension,treeFileExtension))//如果该文件不存在 或 该文件不是项目树文件，直接删除
                        {
                            waitDelList.Add(xmlItem);
                            ShowLog("Delete file：" + nameNode.InnerText);
                        }
                    }
                }
            }

            //删除
            for (int i = 0; i < waitDelList.Count; i++)
            {
                node.RemoveChild(waitDelList[i]);
                //ShowLog(String.Format("Delete {0}: \t{1}", waitDelList[i].Name, waitDelList[i].InnerText));
            }

        }

        /// <summary>
        /// 递归更新IAR path
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="node"></param>
        /// <param name="fileItems"></param>
        void PathTreeRebuild(XmlDocument xml, XmlNode node, List<FileItems> fileItems)
        {
            foreach (FileItems item in fileItems)
            {
                if (item.type == FileItems.ItemType.dir)//dir
                {
                    if (!item.name.Contains(".ignore") && GetDirTree.CheckCodeFileIsExistInPath(item.children, treeFileExtension))//如果该文件夹下没有代码文件 或 需要忽略，则不添加
                    {
                        //生成一个新节点
                        XmlElement groupNode = xml.CreateElement("group");
                        //chileNode.SetAttribute("name", item.name);
                        //将节点加到指定节点下，作为其子节点
                        node.AppendChild(groupNode);

                        XmlElement nameNode = xml.CreateElement("name");
                        nameNode.InnerText = item.name;
                        groupNode.AppendChild(nameNode);

                        ShowLog("Add group：" + nameNode.InnerText);


                        PathTreeRebuild(xml, groupNode, item.children);
                    }
                }
                else if (item.type == FileItems.ItemType.file)
                {
                    if (item.extension == ".c" /*|| item.extension == ".h" */|| item.extension == ".s")
                    {
                        XmlElement fileNode = xml.CreateElement("file");
                        node.AppendChild(fileNode);

                        XmlElement nameNode = xml.CreateElement("name");
                        nameNode.InnerText = GetIARRelativePath(ewpPath, item.fullName);
                        fileNode.AppendChild(nameNode);

                        ShowLog("Add file：" + nameNode.InnerText);
                    }
                }
            }
        }


        #region 通用方法

        /// <summary>
        /// XmlNode中是否存在拥有指定innertext的子node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool IsChildWithInnerTextExistInNode(XmlNode xmlNode, string fileName)
        {
            foreach (XmlNode item in xmlNode.ChildNodes)
            {
                if (item.InnerText == fileName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// EWP 中的group 内是否已存在指定的file
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool IsFileExistInGroupNode(XmlNode xmlNode, string fileName)
        {
            if (xmlNode.Name != "group")
            {
                return false;
            }

            foreach (XmlNode item in xmlNode.ChildNodes)
            {
                if (item.SelectSingleNode("name")?.InnerText == fileName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 删除节点下所有指定名字的子节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        void RemoveNodeByName(XmlNode node, string name)
        {
            //获取节点下所有直接子节点
            XmlNodeList childlist = node.ChildNodes;

            List<XmlNode> delChildlist = new List<XmlNode>();
            foreach (XmlNode item in childlist)
            {
                if (item.Name.Contains(name))
                {
                    delChildlist.Add(item);
                }
            }

            for (int i = 0; i < delChildlist.Count; i++)
            {
                node.RemoveChild(delChildlist[i]);
            }
        }
        /// <summary>
        /// 通过子node的namenode的内容查找子node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name">namenode的内容</param>
        /// <returns></returns>
        XmlNode GetNodeWithName(XmlNode node, string name)
        {
            XmlNodeList tempNodeList = node.ChildNodes;
            foreach (XmlNode item in tempNodeList)
            {
                XmlNode nameNode = item.SelectSingleNode("name");
                if (nameNode?.InnerText == name)
                    return item;
            }
            return null;
        }
        string GetIARRelativePath(string mainDir, string fullFilePath)
        {
            return String.Format("$PROJ_DIR$\\{0}", GetRelativePath(mainDir, fullFilePath));
        }
        /// <summary>
        /// 计算相对路径
        /// 后者相对前者的路径。
        /// </summary>
        /// <param name="mainDir">主目录</param>
        /// <param name="fullFilePath">文件的绝对路径</param>
        /// <returns>fullFilePath相对于mainDir的路径</returns>
        /// <example>
        /// @"..\..\regedit.exe" = GetRelativePath(@"D:\Windows\Web\Wallpaper\", @"D:\Windows\regedit.exe" );
        /// </example>
        static string GetRelativePath(string mainDir, string fullFilePath)
        {
            if (!mainDir.EndsWith("\\"))
            {
                mainDir += "\\";
            }

            int intIndex = -1, intPos = mainDir.IndexOf('\\');

            while (intPos >= 0)
            {
                intPos++;
                if (string.Compare(mainDir, 0, fullFilePath, 0, intPos, true) != 0) break;
                intIndex = intPos;
                intPos = mainDir.IndexOf('\\', intPos);
            }

            if (intIndex >= 0)
            {
                fullFilePath = fullFilePath.Substring(intIndex);
                intPos = mainDir.IndexOf("\\", intIndex);
                while (intPos >= 0)
                {
                    fullFilePath = "..\\" + fullFilePath;
                    intPos = mainDir.IndexOf("\\", intPos + 1);
                }
            }

            return fullFilePath;
        }

        public static bool CheckFileExtension(string itemExtension, string extension)
        {
            string[] ext = extension.Split(',');
            for (int i = 0; i < ext.Length; i++)
            {
                if (string.Compare(ext[i], itemExtension) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

    }
}

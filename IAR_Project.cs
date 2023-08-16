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
        string[] ignoreName = new string[] { "*.ignore", "SI", "si", "*.si4project" };//更新src树忽略的文件夹

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
            if (String.Compare(version, "8.30.1") == 0)
            {
                Ewp_CreakBackup();
                XmlDocument xml = new XmlDocument();
                xml.Load(ewpName);

                XmlNode prjNode = xml.SelectSingleNode("/project");
                XmlNode dataNode;

                XmlNodeList xmlNodeList = prjNode.ChildNodes;
                foreach (XmlNode item in prjNode.ChildNodes)
                {
                    if (item.Name == "fileVersion")
                    {
                        item.InnerText = "3";                      
                    }

                    if (item.Name== "configuration")
                    {
                        ShowLog("转换 Configuration " + item.SelectSingleNode("name").InnerText+ "\n");

                        dataNode = GetNodeWithName(item, "BUILDACTION");
                        if (dataNode != null)
                        {

                            dataNode.RemoveAll();
                            ShowLog("Remove Node: BUILDACTION");
                        }

                        //setting general
                        dataNode = GetNodeWithName(item, "General").SelectSingleNode("data");

                        //ChangeNodeInnerText(GetNodeWithName(dataNode, "OGProductVersion").SelectSingleNode("state"), "OGProductVersion", "7.80.3.12143");
                        ChangeNodeInnerText(GetNodeWithName(dataNode, "OGLastSavedByProductVersion").SelectSingleNode("state"), "OGLastSavedByProductVersion", "8.30.1.17146");
                        ChangeNodeInnerText(GetNodeWithName(dataNode, "GBECoreSlave").SelectSingleNode("version"), "GBECoreSlave", "26");
                        ChangeNodeInnerText(GetNodeWithName(dataNode, "CoreVariant").SelectSingleNode("version"), "CoreVariant", "26");
                        ChangeNodeInnerText(GetNodeWithName(dataNode, "GFPUCoreSlave2").SelectSingleNode("version"), "GFPUCoreSlave2", "26");

                        ChangeNodeInnerText(dataNode.SelectSingleNode("version"), "General-version", "31");
                        
                        if (GetNodeWithName(dataNode, "OG_32_64DeviceCoreSlave") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "OG_32_64DeviceCoreSlave"));
                            ShowLog("Remove Node: OG_32_64DeviceCoreSlave");
                        }

                        if (GetNodeWithName(dataNode, "BrowseInfoPath") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "BrowseInfoPath"));
                            ShowLog("Remove Node: BrowseInfoPath");
                        }

                        if (GetNodeWithName(dataNode, "OGAarch64Abi") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "OGAarch64Abi"));
                            ShowLog("Remove Node: OGAarch64Abi");
                        }

                        if (GetNodeWithName(dataNode, "OG_32_64Device") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "OG_32_64Device"));
                            ShowLog("Remove Node: OG_32_64Device");
                        }

                        if (GetNodeWithName(dataNode, "BuildFilesPath") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "BuildFilesPath"));
                            ShowLog("Remove Node: BuildFilesPath");
                        }

                        if (GetNodeWithName(dataNode, "PointerAuthentication") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "PointerAuthentication"));
                            ShowLog("Remove Node: PointerAuthentication");
                        }

                        if (GetNodeWithName(dataNode, "FPU64") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "FPU64"));
                            ShowLog("Remove Node: FPU64\n");
                        }

                        //setting ICCARM
                        dataNode = GetNodeWithName(item, "ICCARM").SelectSingleNode("data");

                        ChangeNodeInnerText(dataNode.SelectSingleNode("version"), "ICCARM-version", "34");

                        if (GetNodeWithName(dataNode, "CCBranchTargetIdentification") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "CCBranchTargetIdentification"));
                            ShowLog("Remove Node: CCBranchTargetIdentification");
                        }
                        
                        if (GetNodeWithName(dataNode, "CCPointerAutentiction") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "CCPointerAutentiction"));
                            ShowLog("Remove Node: CCPointerAutentiction");
                        }
                        
                        if (GetNodeWithName(dataNode, "OICompilerExtraOption") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "OICompilerExtraOption"));
                            ShowLog("Remove Node: OICompilerExtraOption");
                        }

                        if (GetNodeWithName(dataNode, "CCStackProtection") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "CCStackProtection"));
                            ShowLog("Remove Node: CCStackProtection\n");
                        }

                        //setting AARM
                        dataNode = GetNodeWithName(item, "AARM").SelectSingleNode("data");

                        ChangeNodeInnerText(dataNode.SelectSingleNode("version"), "AARM-version", "10");

                        if (GetNodeWithName(dataNode, "A_32_64Device") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "A_32_64Device"));
                            ShowLog("Remove Node: A_32_64Device\n");
                        }
                        
                        if (GetNodeWithName(dataNode, "PreInclude") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "PreInclude"));
                            ShowLog("Remove Node: PreInclude\n");
                        }

                        //setting ILINK
                        dataNode = GetNodeWithName(item, "ILINK").SelectSingleNode("data");

                        ChangeNodeInnerText(dataNode.SelectSingleNode("version"), "ILINK-version", "21");

                        if (GetNodeWithName(dataNode, "OILinkExtraOption") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "OILinkExtraOption"));
                            ShowLog("Remove Node: OILinkExtraOption");
                        }

                        if (GetNodeWithName(dataNode, "IlinkRawBinaryFile2") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkRawBinaryFile2"));
                            ShowLog("Remove Node: IlinkRawBinaryFile2");
                        }

                        if (GetNodeWithName(dataNode, "IlinkRawBinarySymbol2") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkRawBinarySymbol2"));
                            ShowLog("Remove Node: IlinkRawBinarySymbol2");
                        }

                        if (GetNodeWithName(dataNode, "IlinkRawBinarySegment2") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkRawBinarySegment2"));
                            ShowLog("Remove Node: IlinkRawBinarySegment2");
                        }

                        if (GetNodeWithName(dataNode, "IlinkRawBinaryAlign2") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkRawBinaryAlign2"));
                            ShowLog("Remove Node: IlinkRawBinaryAlign2");
                        }

                        if (GetNodeWithName(dataNode, "IlinkLogCrtRoutineSelection") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkLogCrtRoutineSelection"));
                            ShowLog("Remove Node: IlinkLogCrtRoutineSelection");
                        }

                        if (GetNodeWithName(dataNode, "IlinkLogFragmentInfo") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkLogFragmentInfo"));
                            ShowLog("Remove Node: IlinkLogFragmentInfo");
                        }

                        if (GetNodeWithName(dataNode, "IlinkLogInlining") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkLogInlining"));
                            ShowLog("Remove Node: IlinkLogInlining");
                        }

                        if (GetNodeWithName(dataNode, "IlinkLogMerging") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkLogMerging"));
                            ShowLog("Remove Node: IlinkLogMerging");
                        }

                        if (GetNodeWithName(dataNode, "IlinkDemangle") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkDemangle"));
                            ShowLog("Remove Node: IlinkDemangle");
                        }

                        if (GetNodeWithName(dataNode, "IlinkWrapperFileEnable") != null)
                        {

                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkWrapperFileEnable"));
                            ShowLog("Remove Node: IlinkWrapperFileEnable");
                        }

                        if (GetNodeWithName(dataNode, "IlinkWrapperFile") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkWrapperFile"));
                            ShowLog("Remove Node: IlinkWrapperFile");
                        }

                        if (GetNodeWithName(dataNode, "IlinkProcessor") != null)
                        {

                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkProcessor"));
                            ShowLog("Remove Node: IlinkProcessor");
                        }

                        if (GetNodeWithName(dataNode, "IlinkFpuProcessor") != null)
                        {
                            dataNode.RemoveChild(GetNodeWithName(dataNode, "IlinkFpuProcessor"));
                            ShowLog("Remove Node: IlinkFpuProcessor\n");
                        }
                    }
                }
                xml.Save(ewpName);

                //删除ewt, ewd文件
                File.Delete(ewpName.Replace(".ewp", ".ewt"));
                File.Delete(ewpName.Replace(".ewp", ".ewd"));
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

            ShowLog(String.Format("【更新Include in Configuration {0}】\n", config));

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


        /// <summary>
        /// 更新项目文件树，文件树不区分配置
        /// </summary>
        public void SrcTree_Update()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(ewpName);

            ShowLog("\n【更新项目文件】");
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
                    if (!IsNameIgnore(item.name) && GetDirTree.CheckCodeFileIsExistInPath(item.children, incFileExtension))
                    {
                        string incPath = GetIARRelativePath(ewpPath, item.fullName);
                        incPathList.Add(incPath);
                        IncPath_GetFromFileItems(incPathList, item.children);
                    }
                }
            }
        }
        
        bool IsNameIgnore(string name)
        {
            foreach (var item in ignoreName)
            {
                if (item.Contains("*"))
                {
                    if (name.Contains(item.Substring(1)))
                    {
                        ShowLog("Ignore: " + name);
                        return true;
                    }
                }else
                {
                    if(name == item)
                    {
                        ShowLog("Ignore: " + name);
                        return true;
                    }
                }
            }
            return false;
        }
        void SrcTree_CheckAdd(XmlDocument xml, XmlNode node, List<FileItems> fileItems)
        {
            foreach (FileItems item in fileItems)
            {
                if (item.type == FileItems.ItemType.dir)//dir
                {
                    
                    if (!IsNameIgnore(item.name) && GetDirTree.CheckCodeFileIsExistInPath(item.children, treeFileExtension))//如果该文件夹下没有代码文件 或 需要忽略，则不添加
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
                    if (!IsNameIgnore(item.name) && GetDirTree.CheckCodeFileIsExistInPath(item.children, treeFileExtension))//如果该文件夹下没有代码文件 或 需要忽略，则不添加
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

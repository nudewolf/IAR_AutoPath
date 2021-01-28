using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IAR_AutoPath
{
    public class FileItems
    {
        public enum ItemType
        {
            file,
            dir
        }
        public ItemType type { get; set; }
        public string name { get; set; }
        public string fullName { get; set; }
        public string extension { get; set; }
        //public bool opened { get; set; }
        public List<FileItems> children { get; set; }
        //public string icon { get; set; }
    }

    //以上字段为树形控件中需要的属性
    public class GetDirTree
    {
        //获得指定路径下所有文件名
        public static List<FileItems> GetFileName(List<FileItems> list, string filepath)
        {
            DirectoryInfo root = new DirectoryInfo(filepath);
            foreach (FileInfo f in root.GetFiles())
            {
                list.Add(new FileItems
                {
                    type = FileItems.ItemType.file,
                    name = f.Name,
                    fullName = f.FullName,
                    extension = f.Extension,
                    //opened = false,
                    //icon = "jstree-file"
                });
            }
            return list;
        }
        //获得指定路径下的所有子目录名
        // <param name="list">文件列表</param>
        // <param name="path">文件夹路径</param>
        public static List<FileItems> GetallDirectory(List<FileItems> list, string path)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            DirectoryInfo[] dirs = root.GetDirectories();
            if (dirs.Length != 0)
            {
                foreach (DirectoryInfo d in dirs)
                {
                    list.Add(new FileItems
                    {
                        type = FileItems.ItemType.dir,
                        name = d.Name,
                        fullName = d.FullName,
                        //opened = false,
                        children = GetallDirectory(new List<FileItems>(), d.FullName)
                    });
                }
            }
            list = GetFileName(list, path);
            return list;
        }

        static bool CheckFileExtension(FileItems item, string extension)
        {
            string[] ext = extension.Split(',');
            for (int i = 0; i < ext.Length; i++)
            {
                if (string.Compare(ext[i], item.extension) == 0)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 递归检查该文件夹下是否有指定拓展类型的文件
        /// </summary>
        /// <param name="fileItems"></param>
        /// <param name="extension">如 ".c,.h,.s"</param>
        /// <returns></returns>
        public static bool CheckCodeFileIsExistInPath(List<FileItems> fileItems, string extension)
        {
            foreach (FileItems item in fileItems)
            {
                if (item.type == FileItems.ItemType.file)
                {
                    if (CheckFileExtension(item, extension))
                    {
                        return true;
                    }
                }
                else if (item.type == FileItems.ItemType.dir)
                {
                    if (CheckCodeFileIsExistInPath(item.children, extension))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}

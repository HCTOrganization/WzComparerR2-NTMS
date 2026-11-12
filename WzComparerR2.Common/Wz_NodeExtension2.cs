using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WzComparerR2.WzLib;

namespace WzComparerR2.Common
{
    public static class Wz_NodeExtension2
    {
        public static Wz_Node GetLinkedSourceNode(this Wz_Node node, GlobalFindNodeFunction findNode, Wz_File wzf = null, bool recursive = true)
        {
            var visited = recursive ? new HashSet<Wz_Node>() : null;
            while (node != null)
            {
                if (recursive && !visited.Add(node))
                {
                    return null;
                }

                string path;
                Wz_Node linkedNode;
                if (!string.IsNullOrEmpty(path = node.Nodes["source"].GetValueEx<string>(null)))
                {
                    linkedNode = ResolveLinkedNode(findNode, wzf, path);
                }
                else if (!string.IsNullOrEmpty(path = node.Nodes["_inlink"].GetValueEx<string>(null)))
                {
                    var img = node.GetNodeWzImage();
                    linkedNode = img?.Node.FindNodeByPath(true, path.Split('/'));
                }
                else if (!string.IsNullOrEmpty(path = node.Nodes["_outlink"].GetValueEx<string>(null)))
                {
                    linkedNode = ResolveLinkedNode(findNode, wzf, path);
                }
                else
                {
                    return node;
                }

                if (!recursive)
                {
                    return linkedNode;
                }
                node = linkedNode;
            }
            return null;
        }

        private static Wz_Node ResolveLinkedNode(GlobalFindNodeFunction findNode, Wz_File wzf, string path)
        {
            // return findNode?.Invoke(path);
            if (wzf == null) return findNode?.Invoke(path);
            else // Skill 비교 툴팁 출력용 / code from FindNodeByPath
            {
                string[] fullpath = path.Split('/');
                Wz_Node returnNode = wzf.Node;
                foreach (string p in fullpath)
                {
                    returnNode = returnNode.FindNodeByPath(p, p.Contains(".img"));

                    var img = returnNode.GetValue<Wz_Image>();
                    if (img != null && img.TryExtract()) //判断是否是img
                    {
                        returnNode = img.Node;
                    }
                }
                return returnNode;
            }
        }

        public static Wz_Node HandleFullUol(this Wz_Node node, GlobalFindNodeFunction findNode)
        {
            if (node == null) return null;

            if (node?.Value is Wz_Uol uol)
            {
                if (uol.Uol.StartsWith("/"))
                {
                    return findNode?.Invoke(uol.Uol.TrimStart('/'));
                }
                else
                {
                    return uol.HandleUol(node);
                }
            }
            return node;
        }
    }
}

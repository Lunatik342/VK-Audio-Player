using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HtmlAgilityPack;

namespace Try
{
    public static class HtmlHelper
    {
        public static string FancyInnerText(this HtmlNode node)
        {
            var sb = new StringBuilder();
            foreach (var x in node.ChildNodes)
            {
                if (x.NodeType == HtmlNodeType.Text)
                    sb.Append(x.InnerText);

                if (x.NodeType == HtmlNodeType.Element && x.Name == "br")
                    sb.AppendLine();
            }

            return sb.ToString();
        }
        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}

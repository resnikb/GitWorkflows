using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GitWorkflows.Controls
{
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<DependencyObject> GetVisualAncestors(this DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return GetVisualAncestorsImpl(obj);
        }

        public static T GetVisualAncestor<T>(this DependencyObject obj)
        { return obj.GetVisualAncestors().OfType<T>().FirstOrDefault(); }

        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return GetVisualDescendantsImpl(obj);
        }

        public static T GetVisualDescendant<T>(this DependencyObject obj)
        { return obj.GetVisualDescendants().OfType<T>().FirstOrDefault(); }

        private static IEnumerable<DependencyObject> GetVisualDescendantsImpl(DependencyObject obj)
        {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(obj);

            while (queue.Count > 0)
            {
                var dependencyObject = queue.Dequeue();

                int childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
                for (int i = 0; i < childrenCount; ++i)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);
                    yield return child;
                    queue.Enqueue(child);
                }
            }
        }

        private static IEnumerable<DependencyObject> GetVisualAncestorsImpl(DependencyObject obj)
        {
            var current = obj;
            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);
                yield return current;
            }
        }
    }
}